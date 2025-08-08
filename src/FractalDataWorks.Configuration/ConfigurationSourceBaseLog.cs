using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Configuration;

/// <summary>
/// High-performance logging methods for ConfigurationSourceBase using source generators.
/// </summary>
/// <ExcludeFromTest>Source-generated logging class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage(Justification = "Source-generated logging class with no business logic")]
public static partial class ConfigurationSourceBaseLog
{
    /// <summary>
    /// Logs when a configuration source raises a change event.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="sourceName">The name of the configuration source.</param>
    /// <param name="changeType">The type of change that occurred.</param>
    /// <param name="configurationType">The type name of the configuration that changed.</param>
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Configuration source '{SourceName}' raised {ChangeType} event for {ConfigurationType}")]
    public static partial void ConfigurationChanged(
        ILogger logger, 
        string sourceName, 
        ConfigurationChangeType changeType, 
        string configurationType);
}