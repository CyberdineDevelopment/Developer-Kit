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
    string ProviderName { get; }
    
    /// <summary>
    /// Establishes a connection asynchronously.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the connection if successful.</returns>
    Task<IFdwResult<IConnection>> ConnectAsync(string connectionString, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tests if a connection can be established.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<IFdwResult> TestConnectionAsync(string connectionString, CancellationToken cancellationToken = default);
}
