using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using FractalDataWorks.Results;
using FractalDataWorks.Validation;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Base implementation of a configuration provider.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration managed by this provider.</typeparam>
public abstract class ConfigurationProviderBase<TConfiguration> : 
    IFdwConfigurationProvider<TConfiguration>, IDisposable
    where TConfiguration : ConfigurationBase<TConfiguration>, IFdwConfiguration, new()
{
    private readonly ILogger _logger;
    private readonly IFdwConfigurationSource _source;
    private readonly Dictionary<int, TConfiguration> _cache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationProviderBase{TConfiguration}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="source">The configuration source.</param>
    protected ConfigurationProviderBase(
        ILogger logger,
        IFdwConfigurationSource source)
    {
        _logger = logger;
        _source = source;

        // Subscribe to source changes
        _source.Changed += OnSourceChanged;
    }

    /// <summary>
    /// Gets a configuration by its ID.
    /// </summary>
    /// <param name="id">The ID of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    public async Task<IFdwResult<TConfiguration>> Get(int id)
    {
        await _cacheLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Check cache first
            if (_cache.TryGetValue(id, out var cached))
            {
                return FdwResult<TConfiguration>.Success(cached);
            }

            // Load from source
            var loadResult = await _source.Load<TConfiguration>().ConfigureAwait(false);
            if (loadResult.Error)
            {
                return FdwResult<TConfiguration>.Failure<TConfiguration>(loadResult.Message!);
            }

            // Update cache
            foreach (var config in loadResult.Value!)
            {
                _cache[config.Id] = config;
            }

            // Return requested configuration
            return _cache.TryGetValue(id, out var configuration)
                ? FdwResult<TConfiguration>.Success(configuration)
                : FdwResult<TConfiguration>.Failure<TConfiguration>($"Configuration with ID {id} not found");
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Gets a configuration by its name.
    /// </summary>
    /// <param name="name">The name of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    public async Task<IFdwResult<TConfiguration>> Get(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return FdwResult<TConfiguration>.Failure<TConfiguration>("Invalid configuration name");
        }

        var allResult = await GetAll().ConfigureAwait(false);
        if (allResult.Error)
        {
            return FdwResult<TConfiguration>.Failure<TConfiguration>(allResult.Message!);
        }

        var configuration = allResult.Value!
            .FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        return configuration != null
            ? FdwResult<TConfiguration>.Success(configuration)
            : FdwResult<TConfiguration>.Failure<TConfiguration>($"Configuration with name '{name}' not found");
    }

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    /// <returns>A task containing the collection of configurations.</returns>
    public async Task<IFdwResult<IEnumerable<TConfiguration>>> GetAll()
    {
        await _cacheLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // If cache is empty, load from source
            if (_cache.Count == 0)
            {
                var loadResult = await _source.Load<TConfiguration>().ConfigureAwait(false);
                if (loadResult.Error)
                {
                    return FdwResult<IEnumerable<TConfiguration>>.Failure<IEnumerable<TConfiguration>>(loadResult.Message!);
                }

                // Update cache
                foreach (var config in loadResult.Value!)
                {
                    _cache[config.Id] = config;
                }
            }

            return FdwResult<IEnumerable<TConfiguration>>.Success(_cache.Values);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Gets all enabled configurations.
    /// </summary>
    /// <returns>A task containing the collection of enabled configurations.</returns>
    public async Task<IFdwResult<IEnumerable<TConfiguration>>> GetEnabled()
    {
        var allResult = await GetAll().ConfigureAwait(false);
        if (allResult.Error)
        {
            return allResult;
        }

        var enabled = allResult.Value!.Where(c => c.IsEnabled).ToList();
        return FdwResult<IEnumerable<TConfiguration>>.Success(enabled);
    }

    /// <summary>
    /// Saves a configuration.
    /// </summary>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>A task containing the saved configuration result.</returns>
    public async Task<IFdwResult<TConfiguration>> Save(TConfiguration configuration)
    {
        if (configuration == null)
        {
            return FdwResult<TConfiguration>.Failure<TConfiguration>("Configuration cannot be null");
        }

        // Validation removed during refactoring. Assuming valid configuration.
        // TODO: Add proper validation when reimplementing

        // Mark as modified if updating
        if (configuration.Id > 0)
        {
            configuration.ModifiedAt = DateTime.UtcNow;
        }

        // Save to source
        var saveResult = await _source.Save(configuration).ConfigureAwait(false);
        if (saveResult.Error)
        {
            return saveResult;
        }

        // Update cache
        await _cacheLock.WaitAsync().ConfigureAwait(false);
        try
        {
            _cache[configuration.Id] = configuration;
        }
        finally
        {
            _cacheLock.Release();
        }

        ConfigurationProviderLog.SavedConfiguration(_logger, typeof(TConfiguration).Name, configuration.Id);

        return FdwResult<TConfiguration>.Success(configuration);
    }

    /// <summary>
    /// Deletes a configuration by its ID.
    /// </summary>
    /// <param name="id">The ID of the configuration to delete.</param>
    /// <returns>A task containing the delete operation result.</returns>
    public async Task<IFdwResult<NonResult>> Delete(int id)
    {
        if (id <= 0)
        {
            return FdwResult<NonResult>.Failure<NonResult>("Invalid configuration ID");
        }

        // Delete from source
        var deleteResult = await _source.Delete<TConfiguration>(id).ConfigureAwait(false);
        if (deleteResult.Error)
        {
            return deleteResult;
        }

        // Remove from cache
        await _cacheLock.WaitAsync().ConfigureAwait(false);
        try
        {
            _cache.Remove(id);
        }
        finally
        {
            _cacheLock.Release();
        }

        ConfigurationProviderLog.DeletedConfiguration(_logger, typeof(TConfiguration).Name, id);

        return FdwResult<NonResult>.Success(NonResult.Value);
    }

    /// <summary>
    /// Reloads configurations from the source.
    /// </summary>
    /// <returns>A task containing the reload operation result.</returns>
    public async Task<IFdwResult<NonResult>> Reload()
    {
        await _cacheLock.WaitAsync().ConfigureAwait(false);
        try
        {
            _cache.Clear();
            ConfigurationProviderLog.ReloadedConfiguration(_logger, typeof(TConfiguration).Name);
            return FdwResult<NonResult>.Success(NonResult.Value);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IValidationResult> Validate(TConfiguration configuration)
    {
        if (configuration == null)
        {
            return await Task.FromResult(new ConfigurationValidationResult(false, 
                new[] { new ConfigurationValidationError(nameof(configuration), "Configuration cannot be null", null, ValidationSeverity.Error) }));
        }

        // Basic validation can be extended in derived classes
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            return await Task.FromResult(new ConfigurationValidationResult(false, 
                new[] { new ConfigurationValidationError(nameof(configuration.Name), "Configuration name cannot be empty", null, ValidationSeverity.Error) }));
        }

        return await Task.FromResult(new ConfigurationValidationResult(true, Array.Empty<IValidationError>()));
    }

    /// <summary>
    /// Handles source change events.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnSourceChanged(object? sender, ConfigurationSourceChangedEventArgs e)
    {
        if (e.ConfigurationType != typeof(TConfiguration))
        {
            return;
        }

        // Run async operations on thread pool to avoid blocking event handler
        _ = Task.Run(async () =>
        {
            await _cacheLock.WaitAsync().ConfigureAwait(false);
            try
            {
                switch (e.ChangeType)
                {
                    case ConfigurationChangeType.Deleted:
                        if (e.ConfigurationId.HasValue)
                        {
                            _cache.Remove(e.ConfigurationId.Value);
                        }
                        break;

                    case ConfigurationChangeType.Reloaded:
                        _cache.Clear();
                        break;

                    case ConfigurationChangeType.Added:
                    case ConfigurationChangeType.Updated:
                        // Cache will be updated on next access
                        if (e.ConfigurationId.HasValue)
                        {
                            _cache.Remove(e.ConfigurationId.Value);
                        }
                        break;
                }
            }
            finally
            {
                _cacheLock.Release();
            }

            ConfigurationProviderLog.CacheUpdated(_logger, e.ChangeType);
        });
    }

    /// <summary>
    /// Disposes the configuration provider and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _source.Changed -= OnSourceChanged;
            _cacheLock?.Dispose();
        }
    }
}