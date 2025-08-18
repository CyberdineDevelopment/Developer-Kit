using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Services;
using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Services.DataProvider.MsSql.Configuration;
using FractalDataWorks.Services.DataProvider.MsSql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FractalDataWorks.Services.DataProvider.MsSql.EnhancedEnums;

/// <summary>
/// Factory for creating Microsoft SQL Server data provider service instances.
/// </summary>
public sealed class MsSqlDataProviderFactory : DataProvidersServiceFactoryBase<MsSqlDataProvider, MsSqlConfiguration>
{
    private readonly IConfigurationRegistry<MsSqlConfiguration>? _configurationRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlDataProviderFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurationRegistry">The configuration registry for accessing configurations by name/ID.</param>
    public MsSqlDataProviderFactory(
        ILogger<MsSqlDataProviderFactory>? logger = null, 
        IConfigurationRegistry<MsSqlConfiguration>? configurationRegistry = null) 
        : base(logger)
    {
        _configurationRegistry = configurationRegistry;
    }

    /// <summary>
    /// Creates a SQL Server data provider service instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The SQL Server configuration.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    protected override IFdwResult<MsSqlDataProvider> CreateCore(MsSqlConfiguration configuration)
    {
        if (configuration == null)
        {
            Logger.LogError("Configuration cannot be null");
            return FdwResult<MsSqlDataProvider>.Failure("Configuration cannot be null");
        }

        if (!configuration.Validate())
        {
            Logger.LogError("Configuration validation failed for SQL Server data provider");
            return FdwResult<MsSqlDataProvider>.Failure("Configuration validation failed");
        }

        try
        {
            Logger.LogDebug("Creating MsSql data provider for server: {ServerName}, database: {DatabaseName}", 
                configuration.ServerName, configuration.DatabaseName);

            // Create a typed logger for the data provider
            var dataProviderLogger = Logger as ILogger<MsSqlDataProvider> ?? 
                NullLogger<MsSqlDataProvider>.Instance;

            var dataProvider = new MsSqlDataProvider(dataProviderLogger, configuration);
            
            Logger.LogInformation("Successfully created MsSql data provider for server: {ServerName}, database: {DatabaseName}",
                configuration.ServerName, configuration.DatabaseName);

            return FdwResult<MsSqlDataProvider>.Success(dataProvider);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create MsSql data provider for server: {ServerName}, database: {DatabaseName}",
                configuration.ServerName, configuration.DatabaseName);
            return FdwResult<MsSqlDataProvider>.Failure($"Failed to create MsSql data provider: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a SQL Server data provider service instance for the specified configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the service instance.</returns>
    public override async Task<MsSqlDataProvider> GetService(string configurationName)
    {
        if (string.IsNullOrEmpty(configurationName))
        {
            throw new ArgumentException("Configuration name cannot be null or empty", nameof(configurationName));
        }

        if (_configurationRegistry == null)
        {
            throw new InvalidOperationException("Configuration registry is not available");
        }

        var configuration = _configurationRegistry.GetByName(configurationName);
        if (configuration == null)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture,
                    "No configuration found with name '{0}'", configurationName));
        }

        var result = CreateCore(configuration);
        if (result.Error)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture,
                    "Failed to create service with configuration '{0}': {1}", configurationName, result.Message));
        }

        return await Task.FromResult(result.Value!);
    }

    /// <summary>
    /// Creates a SQL Server data provider service instance for the specified configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the service instance.</returns>
    public override async Task<MsSqlDataProvider> GetService(int configurationId)
    {
        if (_configurationRegistry == null)
        {
            throw new InvalidOperationException("Configuration registry is not available");
        }

        var configuration = _configurationRegistry.Get(configurationId);
        if (configuration == null)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture,
                    "No configuration found with ID {0}", configurationId));
        }

        var result = CreateCore(configuration);
        if (result.Error)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture,
                    "Failed to create service with configuration ID {0}: {1}", configurationId, result.Message));
        }

        return await Task.FromResult(result.Value!);
    }
}