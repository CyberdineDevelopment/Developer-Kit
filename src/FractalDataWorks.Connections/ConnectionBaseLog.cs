using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections;

/// <summary>
/// High-performance logging methods for ConnectionBase using source generators.
/// </summary>
/// <ExcludeFromTest>Source-generated logging class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class with no business logic")]
public static partial class ConnectionBaseLog
{
    /// <summary>
    /// Logs when attempting to connect to an already connected connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionString">The connection string already connected to.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Already connected to {ConnectionString}")]
    public static partial void AlreadyConnected(ILogger logger, string connectionString);

    /// <summary>
    /// Logs when initiating a connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionString">The connection string being connected to.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Connecting to {ConnectionString}")]
    public static partial void Connecting(ILogger logger, string connectionString);

    /// <summary>
    /// Logs when a connection is successfully established.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionString">The connection string that was connected to.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Successfully connected to {ConnectionString}")]
    public static partial void Connected(ILogger logger, string connectionString);

    /// <summary>
    /// Logs when a connection attempt fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="error">The error message describing the connection failure.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Failed to connect: {Error}")]
    public static partial void ConnectionError(ILogger logger, string error);

    /// <summary>
    /// Logs when a connection times out.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The timeout error message.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "{Message}")]
    public static partial void ConnectionTimeoutError(ILogger logger, string message);

    /// <summary>
    /// Logs when an operation is attempted on a disconnected connection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Not connected")]
    public static partial void NotConnected(ILogger logger);

    /// <summary>
    /// Logs when initiating disconnection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionString">The connection string being disconnected from.</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Disconnecting from {ConnectionString}")]
    public static partial void Disconnecting(ILogger logger, string connectionString);

    /// <summary>
    /// Logs when disconnection is successful.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "Successfully disconnected")]
    public static partial void Disconnected(ILogger logger);

    /// <summary>
    /// Logs when an error occurs during disconnection.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="error">The error message describing the disconnect failure.</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Error,
        Message = "Error during disconnect: {Error}")]
    public static partial void DisconnectError(ILogger logger, string error);

    /// <summary>
    /// Logs when a connection test fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that caused the test failure.</param>
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Error,
        Message = "Connection test failed")]
    public static partial void ConnectionTestFailed(ILogger logger, Exception exception);

    /// <summary>
    /// Logs when invalid connection credentials are provided.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Error,
        Message = "Invalid connection credentials provided")]
    public static partial void InvalidCredentials(ILogger logger);

    /// <summary>
    /// Logs when a connection timeout occurs.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionString">The connection string that timed out.</param>
    /// <param name="timeoutSeconds">The timeout duration in seconds.</param>
    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Error,
        Message = "Connection timeout to {ConnectionString} after {TimeoutSeconds} seconds")]
    public static partial void ConnectionTimeout(ILogger logger, string connectionString, int timeoutSeconds);

    /// <summary>
    /// Logs when a connection attempt fails with a generic message.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The failure message.</param>
    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Error,
        Message = "Connection failed: {Message}")]
    public static partial void ConnectionFailed(ILogger logger, string message);

    /// <summary>
    /// Logs when a connection attempt fails with an exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    [LoggerMessage(
        EventId = 14,
        Level = LogLevel.Error,
        Message = "Connection attempt failed")]
    public static partial void ConnectionAttemptFailed(ILogger logger, Exception exception);
}