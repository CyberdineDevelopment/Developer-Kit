namespace FractalDataWorks.Validation;

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