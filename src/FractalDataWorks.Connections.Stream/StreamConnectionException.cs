using System;

namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Represents errors that occur during stream operations.
/// </summary>
public class StreamConnectionException : Exception
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
    /// Initializes a new instance of the <see cref="StreamConnectionException"/> class.
    /// </summary>
    public StreamConnectionException()
    {
        ErrorCode = StreamErrorCode.Unknown;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnectionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public StreamConnectionException(string message)
        : base(message)
    {
        ErrorCode = StreamErrorCode.Unknown;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnectionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public StreamConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = StreamErrorCode.Unknown;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnectionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    public StreamConnectionException(string message, StreamErrorCode errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnectionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="operation">The operation that failed.</param>
    public StreamConnectionException(string message, StreamErrorCode errorCode, StreamOperation operation)
        : base(message)
    {
        ErrorCode = errorCode;
        Operation = operation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnectionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="innerException">The inner exception.</param>
    public StreamConnectionException(string message, StreamErrorCode errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}