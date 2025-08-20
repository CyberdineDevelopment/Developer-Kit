namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

/// <summary>
/// Enumeration of connection management operations.
/// </summary>
public enum ConnectionManagementOperation
{
    /// <summary>
    /// List all available connections.
    /// </summary>
    ListConnections,
    
    /// <summary>
    /// Remove a specific connection.
    /// </summary>
    RemoveConnection,
    
    /// <summary>
    /// Get connection metadata.
    /// </summary>
    GetConnectionMetadata,
    
    /// <summary>
    /// Refresh connection status.
    /// </summary>
    RefreshConnectionStatus,
    
    /// <summary>
    /// Test a specific connection.
    /// </summary>
    TestConnection
}