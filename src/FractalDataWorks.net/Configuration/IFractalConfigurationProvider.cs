using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Validation;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Defines the contract for typed configuration providers in the Fractal framework.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this provider manages.</typeparam>
public interface IFractalConfigurationProvider<TConfiguration>
    where TConfiguration : IFdwConfiguration
{
    /// <summary>
    /// Gets a configuration by its ID.
    /// </summary>
    /// <param name="id">The ID of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    Task<FdwResult<TConfiguration>> Get(int id);

    /// <summary>
    /// Gets a configuration by its name.
    /// </summary>
    /// <param name="name">The name of the configuration.</param>
    /// <returns>A task containing the configuration result.</returns>
    Task<FdwResult<TConfiguration>> Get(string name);

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    /// <returns>A task containing the collection of configurations.</returns>
    Task<FdwResult<IEnumerable<TConfiguration>>> GetAll();

    /// <summary>
    /// Gets all enabled configurations.
    /// </summary>
    /// <returns>A task containing the collection of enabled configurations.</returns>
    Task<FdwResult<IEnumerable<TConfiguration>>> GetEnabled();

    /// <summary>
    /// Saves a configuration.
    /// </summary>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>A task containing the saved configuration result.</returns>
    Task<FdwResult<TConfiguration>> Save(TConfiguration configuration);

    /// <summary>
    /// Deletes a configuration by its ID.
    /// </summary>
    /// <param name="id">The ID of the configuration to delete.</param>
    /// <returns>A task containing the delete operation result.</returns>
    Task<FdwResult<NonResult>> Delete(int id);

    /// <summary>
    /// Validates a configuration.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A task containing the validation result.</returns>
    Task<IValidationResult> Validate(TConfiguration configuration);
}