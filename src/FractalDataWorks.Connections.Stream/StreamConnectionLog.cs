using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// High-performance logging methods for StreamConnection using source generators.
/// </summary>
/// <ExcludeFromTest>Source-generated logging class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class with no business logic")]
public static partial class StreamConnectionLog
{
    /// <summary>
    /// Logs when a stream operation fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Stream operation failed")]
    public static partial void StreamOperationFailed(ILogger logger);

    /// <summary>
    /// Logs when command validation fails with a specific message.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="validationMessage">The validation failure message.</param>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Stream command validation failed: {ValidationMessage}")]
    public static partial void StreamCommandValidationFailed(ILogger logger, string validationMessage);
}