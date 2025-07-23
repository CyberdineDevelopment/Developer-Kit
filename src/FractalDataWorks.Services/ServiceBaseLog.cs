using System;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace FractalDataWorks.Services;

/// <summary>
/// High-performance logging methods for ServiceBase using source generators and Serilog structured logging.
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

    // Serilog structured logging methods for complex objects
    
    /// <summary>
    /// Logs command execution with full command context using Serilog destructuring.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="command">The command being executed.</param>
    public static void CommandExecutedWithContext(ILogger logger, ICommand command)
    {
        logger.LogInformation("Command executed with detailed context {@Command}", command);
    }

    /// <summary>
    /// Logs configuration validation with full configuration context.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration that was validated.</param>
    public static void ConfigurationValidated(ILogger logger, IFdwConfiguration? configuration)
    {
        logger.LogDebug("Service configuration validated {@Configuration}", configuration);
    }

    /// <summary>
    /// Logs performance metrics with structured data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="metrics">The performance metrics.</param>
    public static void PerformanceMetrics(ILogger logger, PerformanceMetrics metrics)
    {
        logger.LogWarning("Performance metrics available {@Metrics}", metrics);
    }

    /// <summary>
    /// Logs service operation with full context including timing and result data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationType">The type of operation.</param>
    /// <param name="duration">Duration in milliseconds.</param>
    /// <param name="result">The operation result.</param>
    /// <param name="context">Additional context data.</param>
    public static void ServiceOperationCompleted(ILogger logger, string operationType, double duration, 
        object? result, object? context = null)
    {
        logger.LogInformation("Service operation {OperationType} completed in {Duration}ms {@Result} {@Context}", 
            operationType, duration, result, context);
    }
}

/// <summary>
/// Performance metrics for structured logging.
/// Serilog will automatically destructure this record for structured logging.
/// </summary>
public record PerformanceMetrics(
    double Duration,
    int ItemsProcessed,
    string OperationType,
    string? SensitiveData = null)
{
    /// <summary>
    /// Override ToString to provide clean string representation while preserving structured data.
    /// </summary>
    public override string ToString() => $"{OperationType}: {ItemsProcessed} items in {Duration}ms";
};