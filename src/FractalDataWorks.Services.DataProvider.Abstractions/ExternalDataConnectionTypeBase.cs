using FractalDataWorks;
using FractalDataWorks.Services;
using FractalDataWorks.Services.ExternalConnections.Abstractions;

namespace FractalDataWorks.Services.DataProvider.Abstractions;

/// <summary>
/// Base class for external data connection type definitions.
/// Follows the ServiceTypeBase pattern for data connections.
/// </summary>
public abstract class ExternalDataConnectionTypeBase : ServiceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalDataConnectionTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection type.</param>
    /// <param name="name">The name of this connection type.</param>
    /// <param name="description">The description of this connection type.</param>
    protected ExternalDataConnectionTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }
    
    /// <summary>
    /// Gets whether this connection type supports database transactions.
    /// </summary>
    public abstract bool SupportsTransactions { get; }
    
    /// <summary>
    /// Gets whether this connection type supports batch operations.
    /// </summary>
    public abstract bool SupportsBatchOperations { get; }
    
    /// <summary>
    /// Gets whether this connection type supports automatic schema discovery.
    /// </summary>
    public abstract bool SupportsSchemaDiscovery { get; }
    
    /// <summary>
    /// Gets the provider type identifier (e.g., "MsSql", "MongoDB", "FileSystem").
    /// </summary>
    public abstract string ProviderType { get; }
    
    /// <summary>
    /// Creates a factory for this connection type.
    /// </summary>
    /// <returns>The connection factory.</returns>
    public abstract IExternalDataConnectionFactory CreateConnectionFactory();
}

/// <summary>
/// Generic external data connection type with typed configuration.
/// </summary>
/// <typeparam name="TConnection">The connection type that implements IExternalDataConnection.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the connection.</typeparam>
public abstract class ExternalDataConnectionTypeBase<TConnection, TConfiguration> 
    : ExternalDataConnectionTypeBase
    where TConnection : class, IExternalDataConnection<TConfiguration>
    where TConfiguration : class, IExternalConnectionConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalDataConnectionTypeBase{TConnection, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection type.</param>
    /// <param name="name">The name of this connection type.</param>
    /// <param name="description">The description of this connection type.</param>
    protected ExternalDataConnectionTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }
    
    /// <summary>
    /// Creates a typed factory for this connection type.
    /// </summary>
    /// <returns>The typed connection factory.</returns>
    public abstract IExternalDataConnectionFactory<TConnection, TConfiguration> CreateTypedConnectionFactory();
    
    /// <summary>
    /// Creates a factory for this connection type.
    /// </summary>
    /// <returns>The connection factory.</returns>
    public override IExternalDataConnectionFactory CreateConnectionFactory() => CreateTypedConnectionFactory();
}
