namespace FractalDataWorks.Connections;

/// <summary>
/// Base interface for all types of connections
/// </summary>
public interface IConnection
{
    // Base connection interface - can be extended for specific connection types
}

/// <summary>
/// Generic connection interface with configuration
/// </summary>
/// <typeparam name="TConfiguration">The connection configuration type</typeparam>
public interface IConnection<TConfiguration> : IConnection
    where TConfiguration : ConnectionConfiguration<TConfiguration>
{
    /// <summary>
    /// Gets the connection configuration
    /// </summary>
    TConfiguration Configuration { get; }
}