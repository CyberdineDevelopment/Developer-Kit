namespace FractalDataWorks.Configuration;

/// <summary>
/// Base class for all configuration types
/// </summary>
public abstract class ConfigurationBase : IConfigurationBase
{
    /// <summary>
    /// Configuration identifier
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Configuration description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this configuration is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Validates the configuration
    /// </summary>
    /// <returns>Validation result indicating if configuration is valid</returns>
    public virtual IValidationResult Validate()
    {
        return ValidationResult.Success();
    }
}