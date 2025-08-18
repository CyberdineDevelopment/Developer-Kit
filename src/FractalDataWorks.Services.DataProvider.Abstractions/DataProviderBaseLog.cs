using System;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.DataProvider.Abstractions;

/// <summary>
/// High-performance logging methods for DataProviderBase using source generators.
/// </summary>
public static partial class DataProviderBaseLog
{
    /// <summary>
    /// Logs when no supported command types are provided.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="parameterName">The name of the parameter that was invalid.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "At least one supported command type must be specified for parameter {ParameterName}")]
    public static partial void NoSupportedCommandTypes(ILogger logger, string parameterName);

    /// <summary>
    /// Logs when a provider doesn't support a command type.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="providerName">The provider name.</param>
    /// <param name="commandType">The unsupported command type.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Provider '{ProviderName}' does not support command type '{CommandType}'")]
    public static partial void UnsupportedCommandType(ILogger logger, string providerName, string commandType);

    /// <summary>
    /// Logs when an empty command list is provided.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Command list cannot be empty")]
    public static partial void EmptyCommandList(ILogger logger);

    /// <summary>
    /// Logs when batch execution fails with an exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exceptionMessage">The exception message.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Batch execution failed: {ExceptionMessage}")]
    public static partial void BatchExecutionFailed(ILogger logger, string exceptionMessage);
}