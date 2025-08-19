using FractalDataWorks;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

/// <summary>
/// Command interface for discovering connection schemas and metadata.
/// </summary>
public interface IExternalConnectionDiscoveryCommand : IExternalConnectionCommand
{
    /// <summary>
    /// Gets the name of the connection to discover.
    /// </summary>
    string ConnectionName { get; }
    
    /// <summary>
    /// Gets the starting path for schema discovery (optional).
    /// </summary>
    string? StartPath { get; }
    
    /// <summary>
    /// Gets the discovery options.
    /// </summary>
    ConnectionDiscoveryOptions Options { get; }
}