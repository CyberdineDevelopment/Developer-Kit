using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Telemetry.Logging;

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

    // .NET 8+ LogProperties examples for advanced structured logging
    
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "Command executed with detailed context")]
    public static partial void CommandExecutedWithContext(ILogger logger, [LogProperties] ICommand command);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Service configuration validated")]
    public static partial void ConfigurationValidated(ILogger logger, [LogProperties] IFdwConfiguration configuration);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Warning,
        Message = "Performance metrics available")]
    public static partial void PerformanceMetrics(ILogger logger, 
        [LogProperties(OmitReferenceName = true)] PerformanceMetrics metrics);
}

/// <summary>
/// Performance metrics for structured logging.
/// </summary>
public record PerformanceMetrics(
    double Duration,
    int ItemsProcessed,
    string OperationType,
    [property: LogPropertyIgnore] string? SensitiveData = null);