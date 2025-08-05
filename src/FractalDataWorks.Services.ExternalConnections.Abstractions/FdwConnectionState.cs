namespace FractalDataWorks.Services.ExternalConnections.Abstractions;

/// <summary>
/// Defines the possible states of an external connection in the FractalDataWorks framework.
/// </summary>
/// <remarks>
/// Connection states help track the lifecycle of external connections and enable
/// proper connection management, pooling, and error handling throughout the framework.
/// </remarks>
public enum FdwConnectionState
{
    /// <summary>
    /// The connection is in an unknown or uninitialized state.
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// The connection has been created but not yet initialized or opened.
    /// </summary>
    Created = 1,
    
    /// <summary>
    /// The connection is currently being opened.
    /// </summary>
    Opening = 2,
    
    /// <summary>
    /// The connection is open and ready for use.
    /// </summary>
    Open = 3,
    
    /// <summary>
    /// The connection is currently executing an operation.
    /// </summary>
    Executing = 4,
    
    /// <summary>
    /// The connection is currently being closed.
    /// </summary>
    Closing = 5,
    
    /// <summary>
    /// The connection is closed.
    /// </summary>
    Closed = 6,
    
    /// <summary>
    /// The connection is in a broken or faulted state and cannot be used.
    /// </summary>
    Broken = 7,
    
    /// <summary>
    /// The connection has been disposed and cannot be reused.
    /// </summary>
    Disposed = 8
}