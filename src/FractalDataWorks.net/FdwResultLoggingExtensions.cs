using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks;

/// <summary>
/// Extension methods for FdwResult that combine logging and result creation.
/// </summary>
/// <ExcludeFromTest>Extension methods for logging with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage]
[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "These are utility methods that accept dynamic messages from callers")]
public static class FdwResultLoggingExtensions
{
    /// <summary>
    /// Creates a failed result with a message and logs the failure.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The failure message.</param>
    /// <param name="logLevel">The log level (defaults to Error).</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult FailureWithLog(this ILogger logger, string message, LogLevel logLevel = LogLevel.Error)
    {
        switch (logLevel)
        {
            case LogLevel.Critical:
                logger.LogCritical("{Message}", message);
                break;
            case LogLevel.Error:
                logger.LogError("{Message}", message);
                break;
            case LogLevel.Warning:
                logger.LogWarning("{Message}", message);
                break;
            case LogLevel.Information:
                logger.LogInformation("{Message}", message);
                break;
            case LogLevel.Debug:
                logger.LogDebug("{Message}", message);
                break;
            case LogLevel.Trace:
                logger.LogTrace("{Message}", message);
                break;
        }
        return FdwResult.Failure(message);
    }

    /// <summary>
    /// Creates a failed result with a message and logs the failure with exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="message">The failure message.</param>
    /// <param name="logLevel">The log level (defaults to Error).</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult FailureWithLog(this ILogger logger, Exception exception, string message, LogLevel logLevel = LogLevel.Error)
    {
        switch (logLevel)
        {
            case LogLevel.Critical:
                logger.LogCritical(exception, "{Message}", message);
                break;
            case LogLevel.Error:
                logger.LogError(exception, "{Message}", message);
                break;
            case LogLevel.Warning:
                logger.LogWarning(exception, "{Message}", message);
                break;
            case LogLevel.Information:
                logger.LogInformation(exception, "{Message}", message);
                break;
            case LogLevel.Debug:
                logger.LogDebug(exception, "{Message}", message);
                break;
            case LogLevel.Trace:
                logger.LogTrace(exception, "{Message}", message);
                break;
        }
        return FdwResult.Failure(message);
    }

    /// <summary>
    /// Creates a failed result with a message and logs the failure.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The failure message.</param>
    /// <param name="logLevel">The log level (defaults to Error).</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult<T> FailureWithLog<T>(this ILogger logger, string message, LogLevel logLevel = LogLevel.Error)
    {
        switch (logLevel)
        {
            case LogLevel.Critical:
                logger.LogCritical("{Message}", message);
                break;
            case LogLevel.Error:
                logger.LogError("{Message}", message);
                break;
            case LogLevel.Warning:
                logger.LogWarning("{Message}", message);
                break;
            case LogLevel.Information:
                logger.LogInformation("{Message}", message);
                break;
            case LogLevel.Debug:
                logger.LogDebug("{Message}", message);
                break;
            case LogLevel.Trace:
                logger.LogTrace("{Message}", message);
                break;
        }
        return FdwResult<T>.Failure(message);
    }

    /// <summary>
    /// Creates a failed result with a message and logs the failure with exception.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="message">The failure message.</param>
    /// <param name="logLevel">The log level (defaults to Error).</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult<T> FailureWithLog<T>(this ILogger logger, Exception exception, string message, LogLevel logLevel = LogLevel.Error)
    {
        switch (logLevel)
        {
            case LogLevel.Critical:
                logger.LogCritical(exception, "{Message}", message);
                break;
            case LogLevel.Error:
                logger.LogError(exception, "{Message}", message);
                break;
            case LogLevel.Warning:
                logger.LogWarning(exception, "{Message}", message);
                break;
            case LogLevel.Information:
                logger.LogInformation(exception, "{Message}", message);
                break;
            case LogLevel.Debug:
                logger.LogDebug(exception, "{Message}", message);
                break;
            case LogLevel.Trace:
                logger.LogTrace(exception, "{Message}", message);
                break;
        }
        return FdwResult<T>.Failure(message);
    }

    /// <summary>
    /// Creates a successful result with a value and logs the success.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="logger">The logger instance.</param>
    /// <param name="value">The success value.</param>
    /// <param name="message">Optional success message.</param>
    /// <param name="logLevel">The log level (defaults to Information).</param>
    /// <returns>A successful result.</returns>
    public static IFdwResult<T> SuccessWithLog<T>(this ILogger logger, T value, string? message = null, LogLevel logLevel = LogLevel.Information)
    {
        if (message is not null)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    logger.LogCritical("{Message}", message);
                    break;
                case LogLevel.Error:
                    logger.LogError("{Message}", message);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning("{Message}", message);
                    break;
                case LogLevel.Information:
                    logger.LogInformation("{Message}", message);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug("{Message}", message);
                    break;
                case LogLevel.Trace:
                    logger.LogTrace("{Message}", message);
                    break;
            }
        }
        
        return FdwResult<T>.Success(value);
    }
}