using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Services;
using FractalDataWorks.Services.DataProvider.Abstractions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FractalDataWorks.Services.DataProvider.Configuration;

/// <summary>
/// Thread-safe registry for DataStoreConfiguration instances with hot-reload support.
/// </summary>
/// <remarks>
/// This registry provides centralized access to data store configurations with the following features:
/// - Named store lookup by connection name
/// - Provider-specific configuration retrieval  
/// - Hot-reload support through IOptionsMonitor
/// - Thread-safe access to configurations
/// - Comprehensive error handling and logging
/// - Efficient lookup using StringComparer.Ordinal
/// 
/// The registry automatically updates when configuration changes are detected and
/// maintains internal caches for performance while ensuring thread safety.
/// </remarks>
public sealed class DataStoreConfigurationRegistry : IConfigurationRegistry<DataStoreConfiguration>, IDisposable
{
    private readonly ILogger<DataStoreConfigurationRegistry> _logger;
    private readonly IOptionsMonitor<DataStoreConfiguration[]> _optionsMonitor;
    private readonly ConcurrentDictionary<string, DataStoreConfiguration> _configurationsByName;
    private readonly ConcurrentDictionary<string, List<DataStoreConfiguration>> _configurationsByProvider;
    private readonly object _refreshLock = new object();
    private readonly IDisposable _changeTokenRegistration;
    private DataStoreConfiguration[] _allConfigurations = Array.Empty<DataStoreConfiguration>();
    private DataStoreConfiguration? _defaultConfiguration;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataStoreConfigurationRegistry"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for this registry.</param>
    /// <param name="optionsMonitor">The options monitor for hot-reload support.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or optionsMonitor is null.</exception>
    public DataStoreConfigurationRegistry(
        ILogger<DataStoreConfigurationRegistry> logger,
        IOptionsMonitor<DataStoreConfiguration[]> optionsMonitor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        
        _configurationsByName = new ConcurrentDictionary<string, DataStoreConfiguration>(StringComparer.Ordinal);
        _configurationsByProvider = new ConcurrentDictionary<string, List<DataStoreConfiguration>>(StringComparer.Ordinal);

        // Register for configuration changes
        _changeTokenRegistration = _optionsMonitor.OnChange(OnConfigurationChanged);
        
        // Initialize with current configuration
        RefreshConfigurations();
        
        _logger.LogInformation("DataStoreConfigurationRegistry initialized with {ConfigCount} configurations", 
            _allConfigurations.Length);
    }

    /// <inheritdoc/>
    public DataStoreConfiguration? Get(int id)
    {
        ThrowIfDisposed();
        
        return _allConfigurations.FirstOrDefault(c => c.Id == id);
    }

    /// <inheritdoc/>
    public IEnumerable<DataStoreConfiguration> GetAll()
    {
        ThrowIfDisposed();
        
        return _allConfigurations.AsEnumerable();
    }

    /// <inheritdoc/>
    public bool TryGet(int id, out DataStoreConfiguration? configuration)
    {
        configuration = Get(id);
        return configuration != null;
    }

    /// <summary>
    /// Gets a configuration by store name.
    /// </summary>
    /// <param name="name">The unique name of the data store.</param>
    /// <returns>The configuration if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public DataStoreConfiguration? GetByName(string name)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Store name cannot be null or empty", nameof(name));
        }

        _configurationsByName.TryGetValue(name, out var configuration);
        
        if (configuration != null)
        {
            _logger.LogDebug("Retrieved configuration for store {StoreName}", name);
        }
        else
        {
            _logger.LogWarning("No configuration found for store {StoreName}", name);
        }
        
        return configuration;
    }

    /// <summary>
    /// Gets all configurations for a specific provider type.
    /// </summary>
    /// <param name="providerType">The provider type (e.g., "MsSql", "PostgreSql").</param>
    /// <returns>A collection of configurations for the specified provider type.</returns>
    /// <exception cref="ArgumentException">Thrown when providerType is null or empty.</exception>
    public IEnumerable<DataStoreConfiguration> GetByProvider(string providerType)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(providerType))
        {
            throw new ArgumentException("Provider type cannot be null or empty", nameof(providerType));
        }

        if (_configurationsByProvider.TryGetValue(providerType, out var configurations))
        {
            _logger.LogDebug("Retrieved {ConfigCount} configurations for provider {ProviderType}", 
                configurations.Count, providerType);
            return configurations.AsEnumerable();
        }

        _logger.LogDebug("No configurations found for provider {ProviderType}", providerType);
        return Enumerable.Empty<DataStoreConfiguration>();
    }

    /// <summary>
    /// Gets the default data store configuration.
    /// </summary>
    /// <returns>The default configuration if found; otherwise, null.</returns>
    /// <remarks>
    /// The default configuration is determined by:
    /// 1. A configuration explicitly marked as default
    /// 2. The first enabled configuration if no explicit default exists
    /// 3. null if no configurations are available
    /// </remarks>
    public DataStoreConfiguration? GetDefault()
    {
        ThrowIfDisposed();
        
        if (_defaultConfiguration != null)
        {
            _logger.LogDebug("Retrieved default configuration for store {StoreName}", _defaultConfiguration.StoreName);
        }
        else
        {
            _logger.LogWarning("No default configuration available");
        }
        
        return _defaultConfiguration;
    }

    /// <summary>
    /// Checks if a data store is enabled by name.
    /// </summary>
    /// <param name="name">The unique name of the data store.</param>
    /// <returns>True if the store exists and is enabled; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public bool IsEnabled(string name)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Store name cannot be null or empty", nameof(name));
        }

        var configuration = GetByName(name);
        var isEnabled = configuration?.IsEnabled == true;
        
        _logger.LogDebug("Store {StoreName} enabled status: {IsEnabled}", name, isEnabled);
        
        return isEnabled;
    }

    /// <summary>
    /// Tries to get a configuration by store name.
    /// </summary>
    /// <param name="name">The unique name of the data store.</param>
    /// <param name="configuration">The configuration if found; otherwise, null.</param>
    /// <returns>True if the configuration was found; otherwise, false.</returns>
    public bool TryGetByName(string name, out DataStoreConfiguration? configuration)
    {
        try
        {
            configuration = GetByName(name);
            return configuration != null;
        }
        catch (ArgumentException)
        {
            configuration = null;
            return false;
        }
    }

    /// <summary>
    /// Gets all enabled configurations.
    /// </summary>
    /// <returns>A collection of enabled configurations.</returns>
    public IEnumerable<DataStoreConfiguration> GetAllEnabled()
    {
        ThrowIfDisposed();
        
        return _allConfigurations.Where(c => c.IsEnabled);
    }

    /// <summary>
    /// Gets the count of registered configurations.
    /// </summary>
    /// <returns>The total number of configurations.</returns>
    public int Count => _allConfigurations.Length;

    /// <summary>
    /// Gets the count of enabled configurations.
    /// </summary>
    /// <returns>The number of enabled configurations.</returns>
    public int EnabledCount => _allConfigurations.Count(c => c.IsEnabled);

    /// <summary>
    /// Called when configuration changes are detected.
    /// </summary>
    /// <param name="configurations">The updated configurations array.</param>
    private void OnConfigurationChanged(DataStoreConfiguration[] configurations)
    {
        if (_disposed)
        {
            return;
        }

        lock (_refreshLock)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _logger.LogInformation("Configuration change detected, refreshing registry");
                RefreshConfigurations();
                _logger.LogInformation("Registry refreshed with {ConfigCount} configurations", 
                    _allConfigurations.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh configurations after change detection");
            }
        }
    }

    /// <summary>
    /// Refreshes the internal configuration caches.
    /// </summary>
    private void RefreshConfigurations()
    {
        var configurations = _optionsMonitor.CurrentValue ?? Array.Empty<DataStoreConfiguration>();
        var validConfigurations = new List<DataStoreConfiguration>();

        // Clear existing caches
        _configurationsByName.Clear();
        _configurationsByProvider.Clear();

        // Process each configuration
        foreach (var config in configurations)
        {
            if (ValidateConfiguration(config))
            {
                validConfigurations.Add(config);
                ProcessConfiguration(config);
            }
        }

        // Update arrays and default
        _allConfigurations = validConfigurations.ToArray();
        _defaultConfiguration = DetermineDefaultConfiguration(validConfigurations);

        _logger.LogDebug("Processed {ValidCount} valid configurations out of {TotalCount} total", 
            validConfigurations.Count, configurations.Length);
    }

    /// <summary>
    /// Validates a configuration before adding it to the registry.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>True if the configuration is valid; otherwise, false.</returns>
    private bool ValidateConfiguration(DataStoreConfiguration configuration)
    {
        if (configuration == null)
        {
            _logger.LogWarning("Null configuration encountered, skipping");
            return false;
        }

        if (string.IsNullOrWhiteSpace(configuration.StoreName))
        {
            _logger.LogWarning("Configuration with empty StoreName encountered, skipping");
            return false;
        }

        if (string.IsNullOrWhiteSpace(configuration.ProviderType))
        {
            _logger.LogWarning("Configuration {StoreName} has empty ProviderType, skipping", configuration.StoreName);
            return false;
        }

        if (!configuration.IsValid)
        {
            _logger.LogWarning("Configuration {StoreName} failed validation, skipping: {ValidationErrors}", 
                configuration.StoreName, 
                string.Join(", ", configuration.Validate()));
            return false;
        }

        return true;
    }

    /// <summary>
    /// Processes a valid configuration and adds it to the appropriate caches.
    /// </summary>
    /// <param name="configuration">The configuration to process.</param>
    private void ProcessConfiguration(DataStoreConfiguration configuration)
    {
        // Add to name-based lookup
        if (!_configurationsByName.TryAdd(configuration.StoreName, configuration))
        {
            _logger.LogWarning("Duplicate store name {StoreName} detected, using first occurrence", 
                configuration.StoreName);
        }

        // Add to provider-based lookup
        _configurationsByProvider.AddOrUpdate(
            configuration.ProviderType,
            new List<DataStoreConfiguration> { configuration },
            (key, existing) =>
            {
                existing.Add(configuration);
                return existing;
            });

        _logger.LogDebug("Processed configuration for store {StoreName} with provider {ProviderType}", 
            configuration.StoreName, configuration.ProviderType);
    }

    /// <summary>
    /// Determines the default configuration from available configurations.
    /// </summary>
    /// <param name="configurations">The available configurations.</param>
    /// <returns>The default configuration or null if none available.</returns>
    private DataStoreConfiguration? DetermineDefaultConfiguration(List<DataStoreConfiguration> configurations)
    {
        if (configurations.Count == 0)
        {
            return null;
        }

        // Look for explicitly marked default (if such a property exists in extended properties)
        var explicitDefault = configurations.FirstOrDefault(c => 
            c.TryGetExtendedProperty<bool>("IsDefault", out var isDefault) && isDefault);

        if (explicitDefault != null)
        {
            _logger.LogInformation("Using explicitly marked default configuration: {StoreName}", 
                explicitDefault.StoreName);
            return explicitDefault;
        }

        // Fall back to first enabled configuration
        var firstEnabled = configurations.FirstOrDefault(c => c.IsEnabled);
        if (firstEnabled != null)
        {
            _logger.LogInformation("Using first enabled configuration as default: {StoreName}", 
                firstEnabled.StoreName);
        }

        return firstEnabled;
    }

    /// <summary>
    /// Throws ObjectDisposedException if this instance has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DataStoreConfigurationRegistry));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_refreshLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            
            try
            {
                _changeTokenRegistration?.Dispose();
                _configurationsByName.Clear();
                _configurationsByProvider.Clear();
                _logger.LogInformation("DataStoreConfigurationRegistry disposed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred during registry disposal");
            }
        }
    }
}