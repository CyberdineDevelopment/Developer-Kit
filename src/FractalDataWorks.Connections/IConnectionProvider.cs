using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;

namespace FractalDataWorks.Connections;

/// <summary>
/// Defines a connection provider that can establish connections.
/// </summary>
public interface IConnectionProvider
{
    /// <summary>
    /// Gets the name of this connection provider.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the Connection from the Configuration
    /// </summary>
    /// <param name="configuration">The connection string.</param>
    /// <returns>A result containing the connection if successful.</returns>
    IFdwResult<IFdwConnection> GetConnection(IFdwConfiguration configuration);

    /// <summary>
    /// Gets the Connection from the Configuration
    /// </summary>
    /// <param name="connectionId">The connection string.</param>
    /// <returns>A result containing the connection if successful.</returns>
    IFdwResult<IExternalConnection> GetConnection(int connectionId);
}

/// <summary>
/// Defines a connection provider that can establish connections.
/// </summary>
/// <typeparam name="TConnection">The type of connection the provider provides</typeparam>
public interface IConnectionProvider<TConnection>
    where TConnection : IExternalConnection
{

}