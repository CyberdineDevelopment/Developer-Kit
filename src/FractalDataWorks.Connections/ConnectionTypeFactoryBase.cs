using System;
using System.Threading.Tasks;

namespace FractalDataWorks.Connections;

/// <summary>
/// Base class for connection type factory definitions that create connection instances.
/// This is a generic base with basic constraints but no Enhanced Enum attributes.
/// </summary>
public abstract class ConnectionTypeFactoryBase<TConnection, TConfiguration>
    where TConnection : class, IExternalConnection
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTypeFactoryBase{TConnection, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this connection type.</param>
    /// <param name="name">The name of this connection type.</param>
    /// <param name="description">The description of this connection type.</param>
    protected ConnectionTypeFactoryBase(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
    
    /// <summary>
    /// Gets the unique identifier for this connection type.
    /// </summary>
    public int Id { get; }
    
    /// <summary>
    /// Gets the name of this connection type.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the description of this connection type.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Creates a new instance of the connection with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use for creating the connection.</param>
    /// <returns>A new connection instance.</returns>
    public abstract object Create(TConfiguration configuration);
    
    /// <summary>
    /// Gets a connection instance by configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration to use.</param>
    /// <returns>A task containing the connection instance.</returns>
    public abstract Task<TConnection> GetConnection(string configurationName);
    
    /// <summary>
    /// Gets a connection instance by configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to use.</param>
    /// <returns>A task containing the connection instance.</returns>
    public abstract Task<TConnection> GetConnection(int configurationId);
}