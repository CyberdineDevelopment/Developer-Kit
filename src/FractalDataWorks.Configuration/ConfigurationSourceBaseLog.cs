using System;
using System.Diagnostics.CodeAnalysis;
using FractalDataWorks.Configuration.Abstractions;
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

    /// <summary>
    /// Logs when configuration load fails.
    /// </summary>
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to load configuration from source {SourceName}: {Error}")]
    public static partial void LoadFailed(ILogger logger, string sourceName, string error);

    /// <summary>
    /// Logs when a configuration is saved successfully.
    /// </summary>
    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Configuration saved to source {SourceName} with ID {ConfigurationId}")]
    public static partial void ConfigurationSaved(ILogger logger, string sourceName, int configurationId);

    /// <summary>
    /// Logs when a configuration is deleted successfully.
    /// </summary>
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Configuration deleted from source {SourceName} with ID {ConfigurationId}")]
    public static partial void ConfigurationDeleted(ILogger logger, string sourceName, int configurationId);
}