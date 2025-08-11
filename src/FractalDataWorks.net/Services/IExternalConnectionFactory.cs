namespace FractalDataWorks.Services;

/// <summary>
/// Interface for external connection factory.
/// </summary>
public interface IExternalConnectionFactory
{
    /// <summary>
    /// Creates a connection instance.
    /// </summary>
    /// <returns>A connection instance.</returns>
    Connections.IExternalConnection CreateConnection();
}