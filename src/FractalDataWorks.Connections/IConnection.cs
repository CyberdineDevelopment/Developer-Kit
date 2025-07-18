namespace FractalDataWorks.Connections;

/// <summary>
/// Represents an active connection.
/// </summary>
public interface IConnection : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the unique identifier for this connection.
    /// </summary>
    Guid ConnectionId { get; }
    
    /// <summary>
    /// Gets a value indicating whether this connection is open.
    /// </summary>
    bool IsOpen { get; }
    
    /// <summary>
    /// Gets the timestamp when this connection was established.
    /// </summary>
    DateTimeOffset EstablishedAt { get; }
    
    /// <summary>
    /// Closes the connection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CloseAsync(CancellationToken cancellationToken = default);
}
