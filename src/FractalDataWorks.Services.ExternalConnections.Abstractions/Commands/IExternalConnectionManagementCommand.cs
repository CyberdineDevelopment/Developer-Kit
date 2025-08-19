using FractalDataWorks;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

/// <summary>
/// Command interface for managing external connections (list, remove, etc.).
/// </summary>
public interface IExternalConnectionManagementCommand : IExternalConnectionCommand
{
    /// <summary>
    /// Gets the management operation to perform.
    /// </summary>
    ConnectionManagementOperation Operation { get; }
    
    /// <summary>
    /// Gets the connection name (optional, depending on operation).
    /// </summary>
    string? ConnectionName { get; }
}