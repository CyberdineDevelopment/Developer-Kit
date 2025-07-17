using FractalDataWorks.Validation;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Implementation of IValidationError for configuration validation.
/// </summary>
internal class ConfigurationValidationError : IValidationError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationError"/> class.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="severity">The severity.</param>
    public ConfigurationValidationError(
        string propertyName,
        string errorMessage,
        string? errorCode,
        ValidationSeverity severity)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        Severity = severity;
    }

    /// <summary>
    /// Gets the name of the property that failed validation.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets the severity of the error.
    /// </summary>
    public ValidationSeverity Severity { get; }
}