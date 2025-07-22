using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Defines the contract for configuration sources in the Fractal framework.
/// </summary>
public interface IFdwConfigurationSource
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
    Task<IFdwResult<IEnumerable<TConfiguration>>> Load<TConfiguration>()
        where TConfiguration : IFdwConfiguration;

    /// <summary>
    /// Saves a configuration to this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to save.</typeparam>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>A task containing the save operation result.</returns>
    Task<IFdwResult<TConfiguration>> Save<TConfiguration>(TConfiguration configuration)
        where TConfiguration : IFdwConfiguration;

    /// <summary>
    /// Deletes a configuration from this source.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to delete.</typeparam>
    /// <param name="id">The ID of the configuration to delete.</param>
    /// <returns>A task containing the delete operation result.</returns>
    Task<IFdwResult<NonResult>> Delete<TConfiguration>(int id)
        where TConfiguration : IFdwConfiguration;

    /// <summary>
    /// Occurs when the configuration source changes.
    /// </summary>
    event EventHandler<ConfigurationSourceChangedEventArgs>? Changed;
}