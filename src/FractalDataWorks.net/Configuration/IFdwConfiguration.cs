using System;
using System.Threading.Tasks;
using FractalDataWorks.Validation;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Defines the base contract for all configuration types in the Fractal framework.
/// </summary>
public interface IFdwConfiguration
{
    /// <summary>
    /// Gets the unique identifier for this configuration instance.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets the name of this configuration.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this configuration is valid.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Gets a value indicating whether this configuration is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the timestamp when this configuration was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the timestamp when this configuration was last modified.
    /// </summary>
    DateTime? ModifiedAt { get; }

    /// <summary>
    /// Validates this configuration.
    /// </summary>
    /// <returns>A task containing the validation result.</returns>
    Task<IValidationResult> Validate();
}
