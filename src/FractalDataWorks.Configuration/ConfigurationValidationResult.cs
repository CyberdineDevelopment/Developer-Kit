using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Validation;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Implementation of IValidationResult for configuration validation.
/// </summary>
internal class ConfigurationValidationResult : IValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationResult"/> class.
    /// </summary>
    /// <param name="isValid">Whether the validation succeeded.</param>
    /// <param name="errors">The validation errors.</param>
    public ConfigurationValidationResult(bool isValid, IEnumerable<IValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets a value indicating whether the validation succeeded.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the collection of validation errors.
    /// </summary>
    public IReadOnlyList<IValidationError> Errors { get; }
}