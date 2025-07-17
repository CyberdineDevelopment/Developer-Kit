namespace FractalDataWorks.Validation;

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