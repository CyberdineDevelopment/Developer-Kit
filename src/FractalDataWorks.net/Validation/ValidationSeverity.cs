namespace FractalDataWorks.Validation;

/// <summary>
/// Represents the severity of a validation error.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Information-level message.
    /// </summary>
    Information,

    /// <summary>
    /// Warning-level message.
    /// </summary>
    Warning,

    /// <summary>
    /// Error-level message.
    /// </summary>
    Error
}