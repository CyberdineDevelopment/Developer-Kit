using System.Collections.Generic;
using FractalDataWorks.Configuration;

namespace FractalDataWorks.Services;

/// <summary>
/// Defines the contract for configuration registries.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration managed by this registry.</typeparam>
public interface IConfigurationRegistry<TConfiguration>
    where TConfiguration : IFdwConfiguration
{
    /// <summary>
    /// Gets a configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <returns>The configuration if found; otherwise, null.</returns>
    TConfiguration? Get(int id);

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    /// <returns>All available configurations.</returns>
    IEnumerable<TConfiguration> GetAll();

    /// <summary>
    /// Tries to get a configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <param name="configuration">The configuration if found; otherwise, null.</param>
    /// <returns>True if the configuration was found; otherwise, false.</returns>
    bool TryGet(int id, out TConfiguration? configuration);
}