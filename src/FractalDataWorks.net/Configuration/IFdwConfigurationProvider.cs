using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Defines the contract for configuration providers in the Fractal framework.
/// </summary>
public interface IFdwConfigurationProvider
{
    /// <summary>
    /// Gets a configuration by its ID.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to retrieve.</typeparam>
    /// <param name="id">The ID of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    Task<FdwResult<TConfiguration>> Get<TConfiguration>(int id)
        where TConfiguration : IFdwConfiguration;

    /// <summary>
    /// Gets a configuration by its name.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to retrieve.</typeparam>
    /// <param name="name">The name of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    Task<FdwResult<TConfiguration>> Get<TConfiguration>(string name)
        where TConfiguration : IFdwConfiguration;

    /// <summary>
    /// Gets all configurations of a specific type.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configurations to retrieve.</typeparam>
    /// <returns>A task containing the collection of configurations.</returns>
    Task<FdwResult<IEnumerable<TConfiguration>>> GetAll<TConfiguration>()
        where TConfiguration : IFdwConfiguration;

    /// <summary>
    /// Gets all enabled configurations of a specific type.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configurations to retrieve.</typeparam>
    /// <returns>A task containing the collection of enabled configurations.</returns>
    Task<FdwResult<IEnumerable<TConfiguration>>> GetEnabled<TConfiguration>()
        where TConfiguration : IFdwConfiguration;

    /// <summary>
    /// Reloads configurations from the source.
    /// </summary>
    /// <returns>A task representing the reload operation.</returns>
    Task<FdwResult<NonResult>> Reload();

    /// <summary>
    /// Gets the configuration source associated with this provider.
    /// </summary>
    IFdwConfigurationSource Source { get; }
}