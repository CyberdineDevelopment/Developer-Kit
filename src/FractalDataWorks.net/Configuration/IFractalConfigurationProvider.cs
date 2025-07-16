using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Validation;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Defines the contract for configuration providers in the Fractal framework.
/// </summary>
public interface IFractalConfigurationProvider
{
    /// <summary>
    /// Gets a configuration by its ID.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to retrieve.</typeparam>
    /// <param name="id">The ID of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    Task<FractalResult<TConfiguration>> Get<TConfiguration>(int id)
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Gets a configuration by its name.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configuration to retrieve.</typeparam>
    /// <param name="name">The name of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    Task<FractalResult<TConfiguration>> Get<TConfiguration>(string name)
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Gets all configurations of a specific type.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configurations to retrieve.</typeparam>
    /// <returns>A task containing the collection of configurations.</returns>
    Task<FractalResult<IEnumerable<TConfiguration>>> GetAll<TConfiguration>()
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Gets all enabled configurations of a specific type.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of configurations to retrieve.</typeparam>
    /// <returns>A task containing the collection of enabled configurations.</returns>
    Task<FractalResult<IEnumerable<TConfiguration>>> GetEnabled<TConfiguration>()
        where TConfiguration : IFractalConfiguration;

    /// <summary>
    /// Reloads configurations from the source.
    /// </summary>
    /// <returns>A task representing the reload operation.</returns>
    Task<FractalResult<NonResult>> Reload();

    /// <summary>
    /// Gets the configuration source associated with this provider.
    /// </summary>
    IFractalConfigurationSource Source { get; }
}

/// <summary>
/// Defines the contract for typed configuration providers in the Fractal framework.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this provider manages.</typeparam>
public interface IFractalConfigurationProvider<TConfiguration>
    where TConfiguration : IFractalConfiguration
{
    /// <summary>
    /// Gets a configuration by its ID.
    /// </summary>
    /// <param name="id">The ID of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    Task<FractalResult<TConfiguration>> Get(int id);

    /// <summary>
    /// Gets a configuration by its name.
    /// </summary>
    /// <param name="name">The name of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    Task<FractalResult<TConfiguration>> Get(string name);

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    /// <returns>A task containing the collection of configurations.</returns>
    Task<FractalResult<IEnumerable<TConfiguration>>> GetAll();

    /// <summary>
    /// Gets all enabled configurations.
    /// </summary>
    /// <returns>A task containing the collection of enabled configurations.</returns>
    Task<FractalResult<IEnumerable<TConfiguration>>> GetEnabled();

    /// <summary>
    /// Saves a configuration.
    /// </summary>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>A task containing the saved configuration result.</returns>
    Task<FractalResult<TConfiguration>> Save(TConfiguration configuration);

    /// <summary>
    /// Deletes a configuration by its ID.
    /// </summary>
    /// <param name="id">The ID of the configuration to delete.</param>
    /// <returns>A task containing the delete operation result.</returns>
    Task<FractalResult<NonResult>> Delete(int id);

    /// <summary>
    /// Validates a configuration.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A task containing the validation result.</returns>
    Task<IValidationResult> Validate(TConfiguration configuration);
}