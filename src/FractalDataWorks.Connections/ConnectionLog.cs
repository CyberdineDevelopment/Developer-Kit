using System;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections;

/// <summary>
/// High-performance logging methods for ConnectionBase using source generators.
/// </summary>
internal static partial class ConnectionLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Already connected to {ConnectionString}")]
    public static partial void AlreadyConnected(ILogger logger, string connectionString);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Connecting to {ConnectionString}")]
    public static partial void Connecting(ILogger logger, string connectionString);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Successfully connected to {ConnectionString}")]
    public static partial void Connected(ILogger logger, string connectionString);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Failed to connect: {Error}")]
    public static partial void ConnectionError(ILogger logger, string error);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "{Message}")]
    public static partial void ConnectionTimeoutError(ILogger logger, string message);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Not connected")]
    public static partial void NotConnected(ILogger logger);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Disconnecting from {ConnectionString}")]
    public static partial void Disconnecting(ILogger logger, string connectionString);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "Successfully disconnected")]
    public static partial void Disconnected(ILogger logger);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Error,
        Message = "Error during disconnect: {Error}")]
    public static partial void DisconnectError(ILogger logger, string error);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Error,
        Message = "Connection test failed")]
    public static partial void ConnectionTestFailed(ILogger logger, Exception? exception);
}