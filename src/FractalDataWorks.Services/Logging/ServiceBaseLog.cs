using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;


namespace FractalDataWorks.Services;

/// <summary>
/// High-performance logging methods for ServiceBase using source generators and Serilog structured logging.
/// </summary>
/// <ExcludeFromTest>Source-generated logging class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class with no business logic")]
public static partial class ServiceBaseLog
{

    /// <summary>
    /// Logs when a service is started.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceName">The name of the service that started.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Service {ServiceName} started")]
    public static partial void ServiceStarted(ILogger logger, string serviceName);

    /// <summary>
    /// Logs when a configuration is invalid with error severity.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The error message describing the invalid configuration.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Invalid configuration: {Message}")]
    public static partial void InvalidConfiguration(ILogger logger, string message);

    /// <summary>
    /// Logs when a configuration is invalid with warning severity.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The warning message describing the configuration issue.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "{Message}")]
    public static partial void InvalidConfigurationWarning(ILogger logger, string message);

    /// <summary>
    /// Logs when a command execution begins.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type name of the command being executed.</param>
    /// <param name="service">The name of the service executing the command.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Executing command {CommandType} in {Service}")]
    public static partial void ExecutingCommand(ILogger logger, string commandType, string service);

    /// <summary>
    /// Logs when a command has been executed successfully.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type name of the command that was executed.</param>
    /// <param name="duration">The execution duration in milliseconds.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Command {CommandType} executed successfully in {Duration}ms")]
    public static partial void CommandExecuted(ILogger logger, string commandType, double duration);

    /// <summary>
    /// Logs when a command execution fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="commandType">The type name of the command that failed.</param>
    /// <param name="error">The error message describing the failure.</param>
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Command {CommandType} failed: {Error}")]
    public static partial void CommandFailed(ILogger logger, string commandType, string error);

    /// <summary>
    /// Logs when an operation fails with an exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationType">The type of operation that failed.</param>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Error,
        Message = "Operation {OperationType} failed: {Error}")]
    public static partial void OperationFailed(ILogger logger, string operationType, string error, Exception? exception);

    /// <summary>
    /// Logs command execution with full command context using structured logging.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="command">The command being executed with full context.</param>
    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "Command executed with detailed context {@Command}")]
    public static partial void CommandExecutedWithContext(ILogger logger, ICommand command);

    /// <summary>
    /// Logs configuration validation with full configuration context.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration that was validated.</param>
    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Service configuration validated {@Configuration}")]
    public static partial void ConfigurationValidated(ILogger logger, IFdwConfiguration? configuration);

    /// <summary>
    /// Logs performance metrics with structured data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="metrics">The performance metrics to log.</param>
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Warning,
        Message = "Performance metrics available {@Metrics}")]
    public static partial void PerformanceMetrics(ILogger logger, PerformanceMetrics metrics);

    /// <summary>
    /// Logs service operation completion with full context including timing and result data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationType">The type of operation that completed.</param>
    /// <param name="duration">Duration in milliseconds.</param>
    /// <param name="result">The operation result.</param>
    /// <param name="context">Additional context data.</param>
    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Information,
        Message = "Service operation {OperationType} completed in {Duration}ms {@Result} {@Context}")]
    public static partial void ServiceOperationCompleted(
        ILogger logger,
        string operationType,
        double duration,
        object? result,
        object? context = null);

    /// <summary>
    /// Logs when an invalid command type is provided to the service.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Error,
        Message = "Invalid command type")]
    public static partial void InvalidCommandType(ILogger logger);

    /// <summary>
    /// Logs when command validation fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="validationMessage">The validation failure message.</param>
    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Error,
        Message = "Command validation failed: {ValidationMessage}")]
    public static partial void CommandValidationFailed(ILogger logger, string validationMessage);
}