using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks;

using FractalDataWorks.EnhancedEnums.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions;
public interface IExternalConnectionConfiguration : IFdwConfiguration
{
    // Define properties and methods that are common to all external connection configurations
}
/// <summary>
/// Base class for external connection providers that generates the ExternalConnectionProviders collection.
/// Integrates with the EnhancedEnums system for automatic service discovery and registration.
/// </summary>
/// <remarks>
/// This base class provides the foundation for all external connection providers in the framework.
/// It handles service registration, provider validation, and integration with the EnhancedEnums
/// system for automatic collection generation and lookup capabilities.
/// </remarks>
[EnumCollection(
    CollectionName = "ExternalConnectionProviders", 
    IncludeReferencedAssemblies = true,
    ReturnType = typeof(IExternalConnectionProvider))]
public abstract class ExternalConnectionProviderBase<TCommand,TConfiguration,TConnection> : ServiceBase<TCommand,TConfiguration,TConnection>, IExternalConnectionProvider, IServiceType
where TCommand : ICommand
      where TConfiguration : class, IExternalConnectionConfiguration
      where TConnection : class, IExternalConnection
{
    /// <summary>
    /// Gets the service interface type that this service type provides.
    /// </summary>
    public virtual Type ServiceType => typeof(IExternalConnectionProvider);

    /// <summary>
    /// Gets the category of this service type.
    /// </summary>
    public virtual string Category => "ExternalConnection";

    /// <summary>
    /// Creates an instance of the service using the provided service provider.
    /// </summary>
    public virtual object CreateService(IServiceProvider serviceProvider) => this;

    /// <summary>
    /// Registers the service and its dependencies with the service collection.
    /// </summary>
    public virtual void RegisterService(IServiceCollection services)
    {
        services.AddSingleton<IExternalConnectionProvider>(this);
    }
    /// <summary>
    /// Gets the data store names that this connection provider supports.
    /// </summary>
    /// <value>An array of data store names (e.g., "SqlServer", "PostgreSQL", "MongoDB").</value>
    [EnumLookup("GetByDataStore", allowMultiple: true)]
    public string[] SupportedDataStores { get; }
    
    /// <summary>
    /// Gets the provider name for this connection type (e.g., "Microsoft.Data.SqlClient", "Npgsql").
    /// </summary>
    /// <value>The underlying provider or driver name.</value>
    [EnumLookup("GetByProvider")]
    public string ProviderName { get; }
    
    /// <summary>
    /// Gets the concrete connection type (e.g., SqlConnection, NpgsqlConnection).
    /// </summary>
    /// <value>The Type of the underlying connection implementation.</value>
    public Type ConnectionType { get; }
    
    /// <summary>
    /// Gets the configuration type required for this connection provider.
    /// </summary>
    /// <value>The Type of configuration object required for connection creation.</value>
    public Type ConfigurationType { get; }
    
    /// <summary>
    /// Gets the supported connection modes for this provider.
    /// </summary>
    /// <value>A collection of connection mode names supported by this provider.</value>
    [EnumLookup("GetByConnectionMode", allowMultiple: true)]
    public IReadOnlyList<string> SupportedConnectionModes { get; }
    
    /// <summary>
    /// Gets the priority of this provider when multiple providers support the same data store.
    /// </summary>
    /// <value>A numeric priority value where higher numbers indicate higher priority.</value>
    [EnumLookup("GetByPriority")]
    public int Priority { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalConnectionProviderBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection provider.</param>
    /// <param name="name">The display name of this connection provider.</param>
    /// <param name="supportedDataStores">The data store names that this provider supports.</param>
    /// <param name="providerName">The provider name for this connection.</param>
    /// <param name="connectionType">The concrete connection type.</param>
    /// <param name="configurationType">The configuration type for this connection.</param>
    /// <param name="supportedConnectionModes">The connection modes supported by this provider.</param>
    /// <param name="priority">The priority of this provider for data store selection.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any string parameter is empty or whitespace, or when arrays are empty.
    /// </exception>
    protected ExternalConnectionProviderBase(
        int id, 
        string name, 
        string[] supportedDataStores, 
        string providerName,
        Type connectionType,
        Type configurationType,
        IReadOnlyList<string> supportedConnectionModes,
        int priority = 0) 
        : base(id, name)
    {
        
        if (supportedDataStores.Length == 0)
        {
            throw new ArgumentException("At least one supported data store must be specified.", nameof(supportedDataStores));
        }
        
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentException("Provider name cannot be empty or whitespace.", nameof(providerName));
        }
        
        if (supportedConnectionModes.Count == 0)
        {
            throw new ArgumentException("At least one supported connection mode must be specified.", nameof(supportedConnectionModes));
        }
        
        // Validate that all data store names are not null or empty
        for (int i = 0; i < supportedDataStores.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(supportedDataStores[i]))
            {
                throw new ArgumentException($"Data store name at index {i} cannot be null, empty, or whitespace.", nameof(supportedDataStores));
            }
        }
        
        // Validate that all connection modes are not null or empty
        for (int i = 0; i < supportedConnectionModes.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(supportedConnectionModes[i]))
            {
                throw new ArgumentException($"Connection mode at index {i} cannot be null, empty, or whitespace.", nameof(supportedConnectionModes));
            }
        }
        
        SupportedDataStores = supportedDataStores;
        ProviderName = providerName;
        ConnectionType = connectionType;
        ConfigurationType = configurationType;
        SupportedConnectionModes = supportedConnectionModes;
        Priority = priority;
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<IExternalConnectionFactory>> CreateConnectionFactoryAsync(IServiceProvider serviceProvider);
    
    
    /// <inheritdoc />
    public virtual IFdwResult ValidateCapability(string dataStore, string? connectionMode = null)
    {
        
        if (string.IsNullOrWhiteSpace(dataStore))
        {
            ExternalConnectionProviderBaseLog.EmptyDataStoreName(Logger);
            return FdwResult.Failure("Data store name cannot be empty or whitespace.");
        }
        
        // Check if this provider supports the specified data store
        bool supportsDataStore = false;
        for (int i = 0; i < SupportedDataStores.Length; i++)
        {
            if (string.Equals(SupportedDataStores[i], dataStore, StringComparison.OrdinalIgnoreCase))
            {
                supportsDataStore = true;
                break;
            }
        }
        
        if (!supportsDataStore)
        {
            ExternalConnectionProviderBaseLog.UnsupportedDataStore(Logger, ProviderName, dataStore);
            return FdwResult.Failure($"Provider '{ProviderName}' does not support data store '{dataStore}'.");
        }
        
        // Check connection mode if specified
        if (!string.IsNullOrWhiteSpace(connectionMode))
        {
            bool supportsConnectionMode = false;
            for (int i = 0; i < SupportedConnectionModes.Count; i++)
            {
                if (string.Equals(SupportedConnectionModes[i], connectionMode, StringComparison.OrdinalIgnoreCase))
                {
                    supportsConnectionMode = true;
                    break;
                }
            }
            
            if (!supportsConnectionMode)
            {
                ExternalConnectionProviderBaseLog.UnsupportedConnectionMode(Logger, ProviderName, connectionMode);
                return FdwResult.Failure($"Provider '{ProviderName}' does not support connection mode '{connectionMode}'.");
            }
        }
        
        return FdwResult.Success();
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<IProviderMetadata>> GetProviderMetadataAsync();
    
    /// <summary>
    /// Creates a connection factory instance (bridge to generic base).
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>A connection factory instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    object IServiceType.CreateService(IServiceProvider serviceProvider) => CreateConnectionFactoryAsync(serviceProvider).GetAwaiter().GetResult();
}