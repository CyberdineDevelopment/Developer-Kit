using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.ExternalConnections.Abstractions;

namespace FractalDataWorks.Services.DataProvider.Abstractions;

/// <summary>
/// Non-generic factory interface for creating external data connections.
/// </summary>
public interface IExternalDataConnectionFactory
{
    /// <summary>
    /// Creates a data connection instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the connection.</param>
    /// <returns>A result containing the created connection or an error message.</returns>
    IFdwResult<IExternalDataConnection<IExternalConnectionConfiguration>> Create(IFdwConfiguration configuration);
}

/// <summary>
/// Generic factory interface for creating external data connections.
/// Specialized for data provider connections.
/// </summary>
/// <typeparam name="TConnection">The connection type that implements IExternalDataConnection.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the connection.</typeparam>
public interface IExternalDataConnectionFactory<TConnection, TConfiguration> : IExternalDataConnectionFactory
    where TConnection : class, IExternalDataConnection<TConfiguration>
    where TConfiguration : class, IExternalConnectionConfiguration
{
    /// <summary>
    /// Creates a data connection instance with the specified strongly-typed configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the connection.</param>
    /// <returns>A result containing the created connection or an error message.</returns>
    IFdwResult<TConnection> Create(TConfiguration configuration);
}
