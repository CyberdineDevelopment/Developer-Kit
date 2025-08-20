using System.Threading.Tasks;
using FluentValidation.Results;

namespace FractalDataWorks.Validation;

/// <summary>
/// Defines the contract for validators in the Fractal framework.
/// </summary>
/// <typeparam name="T">The type of object to validate.</typeparam>
public interface IFdwValidator<T>
{
    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A validation result containing any validation failures.</returns>
    Task<ValidationResult> Validate(T instance);

    /// <summary>
    /// Validates the specified instance and returns a FdwResult.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A successful result if valid; otherwise, a failure result with validation errors.</returns>
    Task<IFdwResult<T>> ValidateToResult(T instance);
}