using System.Collections.Generic;
using System.Linq;
using FractalDataWorks;
using FractalDataWorks.Configuration;

namespace FractalDataWorks.Services.EnhancedEnums;

/// <summary>
/// Implementation of IConfigurationRegistry that wraps a list of configurations.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration managed by this registry.</typeparam>
public sealed class ConfigurationRegistryImpl<TConfiguration> : IConfigurationRegistry<TConfiguration>
    where TConfiguration : IFdwConfiguration
{
    private readonly List<TConfiguration> _configurations;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationRegistryImpl{TConfiguration}"/> class.
    /// </summary>
    /// <param name="configurations">The collection of configurations to manage.</param>
    public ConfigurationRegistryImpl(IEnumerable<TConfiguration> configurations)
    {
        _configurations = configurations?.ToList() ?? new List<TConfiguration>();
    }

    /// <summary>
    /// Gets a configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <returns>The configuration if found; otherwise, null.</returns>
    public TConfiguration? Get(int id)
    {
        return _configurations.FirstOrDefault(c => ((dynamic)c).Id == id);
    }

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    /// <returns>All available configurations.</returns>
    public IEnumerable<TConfiguration> GetAll()
    {
        return _configurations.AsEnumerable();
    }

    /// <summary>
    /// Tries to get a configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <param name="configuration">The configuration if found; otherwise, null.</param>
    /// <returns>True if the configuration was found; otherwise, false.</returns>
    public bool TryGet(int id, out TConfiguration? configuration)
    {
        configuration = Get(id);
        return configuration != null;
    }
}