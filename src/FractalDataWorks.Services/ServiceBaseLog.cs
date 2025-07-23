using System;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services;

/// <summary>
/// High-performance logging methods for ServiceBase using source generators.
/// </summary>
public static partial class ServiceBaseLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Service {ServiceName} started")]
    public static partial void ServiceStarted(ILogger logger, string serviceName);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Invalid configuration: {Message}")]
    public static partial void InvalidConfiguration(ILogger logger, string message);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "{Message}")]
    public static partial void InvalidConfigurationWarning(ILogger logger, string message);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Executing command {CommandType} in {Service}")]
    public static partial void ExecutingCommand(ILogger logger, string commandType, string service);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Command {CommandType} executed successfully in {Duration}ms")]
    public static partial void CommandExecuted(ILogger logger, string commandType, double duration);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Command {CommandType} failed: {Error}")]
    public static partial void CommandFailed(ILogger logger, string commandType, string error);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "Operation {OperationType} failed: {Error}")]
    public static partial void OperationFailed(ILogger logger, string operationType, string error, Exception? exception);
}