using System.Collections.Generic;

namespace FractalDataWorks.Validation;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public interface IValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation succeeded.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Gets the collection of validation errors.
    /// </summary>
    IReadOnlyList<IValidationError> Errors { get; }
}