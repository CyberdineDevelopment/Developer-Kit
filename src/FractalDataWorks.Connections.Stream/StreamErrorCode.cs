namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Error codes for stream operations.
/// </summary>
public enum StreamErrorCode
{
    /// <summary>
    /// The stream is not connected.
    /// </summary>
    NotConnected,

    /// <summary>
    /// The operation is not supported by the stream.
    /// </summary>
    OperationNotSupported,

    /// <summary>
    /// Access to the stream was denied.
    /// </summary>
    AccessDenied,

    /// <summary>
    /// The stream has reached the end.
    /// </summary>
    EndOfStream,

    /// <summary>
    /// An I/O error occurred.
    /// </summary>
    IOError,

    /// <summary>
    /// The stream is already in use.
    /// </summary>
    StreamInUse,

    /// <summary>
    /// An unknown error occurred.
    /// </summary>
    Unknown
}