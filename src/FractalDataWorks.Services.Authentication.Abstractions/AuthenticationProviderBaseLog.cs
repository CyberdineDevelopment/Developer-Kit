using System;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// High-performance logging methods for AuthenticationProviderBase using source generators.
/// </summary>
public static partial class AuthenticationProviderBaseLog
{
    /// <summary>
    /// Logs when an empty command list is provided.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Command list cannot be empty")]
    public static partial void EmptyCommandList(ILogger logger);

    /// <summary>
    /// Logs when batch execution fails with an exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exceptionMessage">The exception message.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Batch execution failed: {ExceptionMessage}")]
    public static partial void BatchExecutionFailed(ILogger logger, string exceptionMessage);

    /// <summary>
    /// Logs when a provider doesn't support a command type.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="providerName">The provider name.</param>
    /// <param name="commandType">The unsupported command type.</param>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Provider '{ProviderName}' does not support command type '{CommandType}'")]
    public static partial void UnsupportedCommandType(ILogger logger, string providerName, string commandType);

    /// <summary>
    /// Logs when a provider doesn't support an authentication flow.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="providerName">The provider name.</param>
    /// <param name="authenticationFlow">The unsupported authentication flow.</param>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Provider '{ProviderName}' does not support authentication flow '{AuthenticationFlow}'")]
    public static partial void UnsupportedAuthenticationFlow(ILogger logger, string providerName, string authenticationFlow);

    /// <summary>
    /// Logs when a health check fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="errorMessage">The health check error message.</param>
    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Health check failed: {ErrorMessage}")]
    public static partial void HealthCheckFailed(ILogger logger, string errorMessage);
}