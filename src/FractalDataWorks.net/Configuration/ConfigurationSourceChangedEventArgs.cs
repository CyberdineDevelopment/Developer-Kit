using System;
using System.Diagnostics.CodeAnalysis;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Provides data for the ConfigurationSourceChanged event.
/// </summary>
/// <ExcludeFromTest>Simple event args DTO with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage]
public class ConfigurationSourceChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationSourceChangedEventArgs"/> class.
    /// </summary>
    /// <param name="changeType">The type of change that occurred.</param>
    /// <param name="configurationType">The type of configuration that changed.</param>
    /// <param name="configurationId">The ID of the configuration that changed.</param>
    public ConfigurationSourceChangedEventArgs(
        ConfigurationChangeType changeType,
        Type configurationType,
        int? configurationId = null)
    {
        ChangeType = changeType;
        ConfigurationType = configurationType;
        ConfigurationId = configurationId;
        ChangedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the type of change that occurred.
    /// </summary>
    public ConfigurationChangeType ChangeType { get; }

    /// <summary>
    /// Gets the type of configuration that changed.
    /// </summary>
    public Type ConfigurationType { get; }

    /// <summary>
    /// Gets the ID of the configuration that changed.
    /// </summary>
    public int? ConfigurationId { get; }

    /// <summary>
    /// Gets the timestamp when the change occurred.
    /// </summary>
    public DateTime ChangedAt { get; }
}