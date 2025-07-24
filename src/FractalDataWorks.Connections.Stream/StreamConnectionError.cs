using System;

namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Represents errors that occur during stream operations.
/// </summary>
public class StreamConnectionError : Exception
{
    /// <summary>
    /// Gets the operation that failed.
    /// </summary>
    public StreamOperation? Operation { get; }

    /// <summary>
    /// Gets the error code.
    /// </summary>
    public StreamErrorCode ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnectionError"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    public StreamConnectionError(string message, StreamErrorCode errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnectionError"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="operation">The operation that failed.</param>
    public StreamConnectionError(string message, StreamErrorCode errorCode, StreamOperation operation)
        : base(message)
    {
        ErrorCode = errorCode;
        Operation = operation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnectionError"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="innerException">The inner exception.</param>
    public StreamConnectionError(string message, StreamErrorCode errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

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