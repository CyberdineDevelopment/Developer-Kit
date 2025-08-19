using FractalDataWorks;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

/// <summary>
/// Command interface for creating external connections.
/// </summary>
public interface IExternalConnectionCreateCommand : IExternalConnectionCommand
{
    /// <summary>
    /// Gets the name for the new connection.
    /// </summary>
    string ConnectionName { get; }
    
    /// <summary>
    /// Gets the provider type for the connection (e.g., "MsSql", "PostgreSQL").
    /// </summary>
    string ProviderType { get; }
    
    /// <summary>
    /// Gets the configuration for the connection.
    /// </summary>
    IExternalConnectionConfiguration ConnectionConfiguration { get; }
}