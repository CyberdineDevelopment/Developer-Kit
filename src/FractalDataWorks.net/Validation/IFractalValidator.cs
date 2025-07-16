using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Validation;

/// <summary>
/// Defines the contract for validators in the Fractal framework.
/// </summary>
/// <typeparam name="T">The type of object to validate.</typeparam>
public interface IFractalValidator<T>
{
    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A validation result containing any validation failures.</returns>
    Task<IValidationResult> Validate(T instance);

    /// <summary>
    /// Validates the specified instance and returns a FractalResult.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A successful result if valid; otherwise, a failure result with validation errors.</returns>
    Task<FractalResult<T>> ValidateToResult(T instance);
}

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

/// <summary>
/// Represents a single validation error.
/// </summary>
public interface IValidationError
{
    /// <summary>
    /// Gets the name of the property that failed validation.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// Gets the error code.
    /// </summary>
    string? ErrorCode { get; }

    /// <summary>
    /// Gets the severity of the error.
    /// </summary>
    ValidationSeverity Severity { get; }
}

/// <summary>
/// Defines the severity levels for validation errors.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Information level - not an error.
    /// </summary>
    Info,

    /// <summary>
    /// Warning level - should be addressed but not blocking.
    /// </summary>
    Warning,

    /// <summary>
    /// Error level - must be addressed.
    /// </summary>
    Error
}