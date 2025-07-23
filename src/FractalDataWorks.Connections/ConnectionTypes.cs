using System;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.EnumTypes.Services;
using FractalDataWorks.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections;

/// <summary>
/// Base class for connection type definitions using Enhanced Enums.
/// </summary>
[EnhancedEnumBase("ConnectionTypes", 
    ReturnType = "IServiceFactory<IExternalConnection, IFdwConfiguration>",
    ReturnTypeNamespace = "FractalDataWorks.Services")]
public abstract class ConnectionType<TConnection, TConfiguration> : ServiceTypeFactoryBase<TConnection, TConfiguration>
    where TConnection : class, IExternalConnection
    where TConfiguration : class, IFdwConfiguration
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionType{TConnection, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection type.</param>
    /// <param name="name">The name of this connection type.</param>
    /// <param name="description">The description of this connection type.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="logger">The logger instance.</param>
    protected ConnectionType(int id, string name, string description, IServiceProvider serviceProvider, ILogger logger)
        : base(id, name, description)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a connection instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the connection.</param>
    /// <returns>A result containing the created connection or an error message.</returns>
    public override IFdwResult<TConnection> Create(TConfiguration configuration)
    {
        try
        {
            _logger.LogDebug("Creating connection of type {ConnectionType} with configuration {ConfigurationName}", 
                typeof(TConnection).Name, configuration.Name);

            var connection = CreateConnectionInstance(configuration);
            
            _logger.LogInformation("Successfully created connection {ConnectionName} of type {ConnectionType}", 
                Name, typeof(TConnection).Name);
                
            return FdwResult<TConnection>.Success(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create connection {ConnectionName} of type {ConnectionType}", 
                Name, typeof(TConnection).Name);
                
            return FdwResult<TConnection>.Failure($"Failed to create connection: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a connection instance for the specified configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the connection instance.</returns>
    public override async Task<TConnection> GetService(string configurationName)
    {
        var configurationProvider = _serviceProvider.GetRequiredService<IConfigurationProvider<TConfiguration>>();
        var configuration = await configurationProvider.GetConfiguration(configurationName);
        
        var result = Create(configuration);
        if (result.IsSuccess)
        {
            return result.Value!;
        }
        
        throw new InvalidOperationException(result.ErrorMessage);
    }

    /// <summary>
    /// Creates a connection instance for the specified configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the connection instance.</returns>
    public override async Task<TConnection> GetService(int configurationId)
    {
        var configurationProvider = _serviceProvider.GetRequiredService<IConfigurationProvider<TConfiguration>>();
        var configuration = await configurationProvider.GetConfiguration(configurationId);
        
        var result = Create(configuration);
        if (result.IsSuccess)
        {
            return result.Value!;
        }
        
        throw new InvalidOperationException(result.ErrorMessage);
    }

    /// <summary>
    /// Creates the actual connection instance. Derived classes must implement this method.
    /// </summary>
    /// <param name="configuration">The configuration for the connection.</param>
    /// <returns>The created connection instance.</returns>
    protected abstract TConnection CreateConnectionInstance(TConfiguration configuration);
}