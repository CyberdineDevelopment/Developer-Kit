using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Defines the contract for configuration sources in the Fractal framework.
/// </summary>
public interface IFractalConfigurationSource
{
    /// <summary>
    /// Gets the name of this configuration source.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this source supports write operations.
    /// </summary>
    bool IsWritable { get; }

    /// <summary>
    /// Gets a value indicating whether this source supports automatic reload.
    /// </summary>
    bool SupportsReload { get; }

    /// <summary>
    /// Loads configurations from this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to load.</typeparam>
    /// <returns>A task containing the loaded configurations.</returns>
    Task<FractalResult<IEnumerable<TConfiguration>>> Load<TConfiguration>()
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Saves a configuration to this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to save.</typeparam>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>A task containing the save operation result.</returns>
    Task<FractalResult<TConfiguration>> Save<TConfiguration>(TConfiguration configuration)
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Deletes a configuration from this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to delete.</typeparam>
    /// <param name="id">The ID of the configuration to delete.</param>
    /// <returns>A task containing the delete operation result.</returns>
    Task<FractalResult<NonResult>> Delete<TConfiguration>(int id)
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Occurs when the configuration source changes.
    /// </summary>
    event EventHandler<ConfigurationSourceChangedEventArgs>? Changed;
}

/// <summary>
/// Provides data for the ConfigurationSourceChanged event.
/// </summary>
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

/// <summary>
/// Defines the types of configuration changes.
/// </summary>
public enum ConfigurationChangeType
{
    /// <summary>
    /// A configuration was added.
    /// </summary>
    Added,

    /// <summary>
    /// A configuration was updated.
    /// </summary>
    Updated,

    /// <summary>
    /// A configuration was deleted.
    /// </summary>
    Deleted,

    /// <summary>
    /// The configuration source was reloaded.
    /// </summary>
    Reloaded
}