namespace FractalDataWorks.Configuration;

/// <summary>
/// Implementation of validation result
/// </summary>
public class ValidationResult : IValidationResult
{
    /// <summary>
    /// Whether the validation passed
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Validation errors if any
    /// </summary>
    public string[] Errors { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success()
    {
        return new ValidationResult { IsValid = true };
    }
    
    /// <summary>
    /// Creates a failed validation result
    /// </summary>
    public static ValidationResult Failure(params string[] errors)
    {
        return new ValidationResult 
        { 
            IsValid = false, 
            Errors = errors 
        };
    }
}