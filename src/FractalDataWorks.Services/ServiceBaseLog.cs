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
    /// <summary>
    /// Logs that a service has started successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceName">The name of the service that started.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Service {ServiceName} started")]
    public static partial void ServiceStarted(ILogger logger, string serviceName);

    /// <summary>
    /// Logs an invalid configuration error.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The configuration error message.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Invalid configuration: {Message}")]
    public static partial void InvalidConfiguration(ILogger logger, string message);

    /// <summary>
    /// Logs an invalid configuration warning.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The configuration warning message.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "{Message}")]
    public static partial void InvalidConfigurationWarning(ILogger logger, string message);

    /// <summary>
    /// Logs that a command is being executed.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command being executed.</param>
    /// <param name="service">The service executing the command.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Executing command {CommandType} in {Service}")]
    public static partial void ExecutingCommand(ILogger logger, string commandType, string service);

    /// <summary>
    /// Logs successful command execution with timing information.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command that was executed.</param>
    /// <param name="duration">The execution duration in milliseconds.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Command {CommandType} executed successfully in {Duration}ms")]
    public static partial void CommandExecuted(ILogger logger, string commandType, double duration);

    /// <summary>
    /// Logs command execution failure.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type of command that failed.</param>
    /// <param name="error">The error message describing the failure.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Command {CommandType} failed: {Error}")]
    public static partial void CommandFailed(ILogger logger, string commandType, string error);

    /// <summary>
    /// Logs operation failure with exception details.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationType">The type of operation that failed.</param>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
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