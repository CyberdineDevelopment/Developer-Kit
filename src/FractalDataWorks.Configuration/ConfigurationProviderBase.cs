using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using FractalDataWorks.Validation;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Base implementation of a configuration provider.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration managed by this provider.</typeparam>
public abstract class ConfigurationProviderBase<TConfiguration> : 
    IFractalConfigurationProvider<TConfiguration>
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _source = source ?? throw new ArgumentNullException(nameof(source));

        // Subscribe to source changes
        _source.Changed += OnSourceChanged;
    }

    /// <summary>
    /// Gets a configuration by its ID.
    /// </summary>
    /// <param name="id">The ID of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    public async Task<FdwResult<TConfiguration>> Get(int id)
    {
        await _cacheLock.WaitAsync();
        try
        {
            // Check cache first
            if (_cache.TryGetValue(id, out var cached))
            {
                return FdwResult<TConfiguration>.Success(cached);
            }

            // Load from source
            var loadResult = await _source.Load<TConfiguration>();
            if (loadResult.IsFailure)
            {
                return FdwResult<TConfiguration>.Failure(loadResult.Error!);
            }

            // Update cache
            foreach (var config in loadResult.Value!)
            {
                _cache[config.Id] = config;
            }

            // Return requested configuration
            return _cache.TryGetValue(id, out var configuration)
                ? FdwResult<TConfiguration>.Success(configuration)
                : FdwResult<TConfiguration>.Failure($"Configuration with ID {id} not found");
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
    public async Task<FdwResult<TConfiguration>> Get(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return FdwResult<TConfiguration>.Failure("Configuration name cannot be empty");
        }

        var allResult = await GetAll();
        if (allResult.IsFailure)
        {
            return FdwResult<TConfiguration>.Failure(allResult.Error!);
        }

        var configuration = allResult.Value!
            .FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        return configuration != null
            ? FdwResult<TConfiguration>.Success(configuration)
            : FdwResult<TConfiguration>.Failure($"Configuration with name '{name}' not found");
    }

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    /// <returns>A task containing the collection of configurations.</returns>
    public async Task<FdwResult<IEnumerable<TConfiguration>>> GetAll()
    {
        await _cacheLock.WaitAsync();
        try
        {
            // If cache is empty, load from source
            if (_cache.Count == 0)
            {
                var loadResult = await _source.Load<TConfiguration>();
                if (loadResult.IsFailure)
                {
                    return FdwResult<IEnumerable<TConfiguration>>.Failure(loadResult.Error!);
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
    public async Task<FdwResult<IEnumerable<TConfiguration>>> GetEnabled()
    {
        var allResult = await GetAll();
        if (allResult.IsFailure)
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
    public async Task<FdwResult<TConfiguration>> Save(TConfiguration configuration)
    {
        if (configuration == null)
        {
            return FdwResult<TConfiguration>.Failure("Configuration cannot be null");
        }

        // Validate configuration
        var validationResult = await Validate(configuration);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return FdwResult<TConfiguration>.Failure(errors);
        }

        // Mark as modified if updating
        if (configuration.Id > 0)
        {
            configuration.ModifiedAt = DateTime.UtcNow;
        }

        // Save to source
        var saveResult = await _source.Save(configuration);
        if (saveResult.IsFailure)
        {
            return saveResult;
        }

        // Update cache
        await _cacheLock.WaitAsync();
        try
        {
            _cache[configuration.Id] = configuration;
        }
        finally
        {
            _cacheLock.Release();
        }

        _logger.LogInformation("Saved configuration {ConfigurationType} with ID {Id}",
            typeof(TConfiguration).Name, configuration.Id);

        return FdwResult<TConfiguration>.Success(configuration);
    }

    /// <summary>
    /// Deletes a configuration by its ID.
    /// </summary>
    /// <param name="id">The ID of the configuration to delete.</param>
    /// <returns>A task containing the delete operation result.</returns>
    public async Task<FdwResult<NonResult>> Delete(int id)
    {
        if (id <= 0)
        {
            return FdwResult<NonResult>.Failure("Invalid configuration ID");
        }

        // Delete from source
        var deleteResult = await _source.Delete<TConfiguration>(id);
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        // Remove from cache
        await _cacheLock.WaitAsync();
        try
        {
            _cache.Remove(id);
        }
        finally
        {
            _cacheLock.Release();
        }

        _logger.LogInformation("Deleted configuration {ConfigurationType} with ID {Id}",
            typeof(TConfiguration).Name, id);

        return FdwResult<NonResult>.Success(NonResult.Value);
    }

    /// <summary>
    /// Validates a configuration.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A task containing the validation result.</returns>
    public virtual Task<IValidationResult> Validate(TConfiguration configuration)
    {
        return configuration.Validate();
    }

    /// <summary>
    /// Handles source change events.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private async void OnSourceChanged(object? sender, ConfigurationSourceChangedEventArgs e)
    {
        if (e.ConfigurationType != typeof(TConfiguration))
        {
            return;
        }

        await _cacheLock.WaitAsync();
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

        _logger.LogDebug("Configuration cache updated due to {ChangeType} event",
            e.ChangeType);
    }
}