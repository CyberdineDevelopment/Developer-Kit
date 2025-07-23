using System.Threading.Tasks;
using FractalDataWorks.Connections;

namespace FractalDataWorks.Services;

/// <summary>
/// Generic factory interface for creating Connection instances
/// </summary>
public interface IConnectionFactory
{
    /// <summary>
    /// Creates a connection of the specified type using the provided configuration.
    /// </summary>
    /// <typeparam name="T">The type of connection to create.</typeparam>
    /// <param name="configuration">The configuration to use for creating the connection.</param>
    /// <returns>A result containing the created connection or an error.</returns>
    IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IExternalConnection;
    
    /// <summary>
    /// Creates a connection using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use for creating the connection.</param>
    /// <returns>A result containing the created connection or an error.</returns>
    IFdwResult<IExternalConnection> Create(IFdwConfiguration configuration);
}

/// <summary>
/// Generic factory interface for creating Connection instances of a specific type.
/// </summary>
public interface IConnectionFactory<TConnection> : IConnectionFactory
    where TConnection : IExternalConnection
{
    /// <summary>
    /// Creates a connection of the specified type using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use for creating the connection.</param>
    /// <returns>A result containing the created connection or an error.</returns>
    new IFdwResult<TConnection> Create(IFdwConfiguration configuration);
}

/// <summary>
/// Generic factory interface for creating Connection instances with specific configuration type.
/// </summary>
public interface IConnectionFactory<TConnection, TConfiguration> : IConnectionFactory<TConnection>
    where TConnection : IExternalConnection
    where TConfiguration : IFdwConfiguration
{
    /// <summary>
    /// Creates a connection using the provided typed configuration.
    /// </summary>
    /// <param name="configuration">The typed configuration to use for creating the connection.</param>
    /// <returns>A result containing the created connection or an error.</returns>
    IFdwResult<TConnection> Create(TConfiguration configuration);
    
    /// <summary>
    /// Gets a connection by configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration.</param>
    /// <returns>A task containing the connection.</returns>
    Task<TConnection> GetConnection(string configurationName);
    
    /// <summary>
    /// Gets a connection by configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <returns>A task containing the connection.</returns>
    Task<TConnection> GetConnection(int configurationId);
}