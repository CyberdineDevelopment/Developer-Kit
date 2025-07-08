namespace FractalDataWorks.Configuration
{
    /// <summary>
    /// Base interface for all configuration types in the FractalDataWorks ecosystem.
    /// Provides self-validating configuration with strongly-typed settings.
    /// </summary>
    public interface IConfigurationBase
    {
        /// <summary>
        /// Validates the configuration and returns a validation result
        /// </summary>
        /// <returns>The validation result</returns>
        IValidationResult Validate();
    }
    
    /// <summary>
    /// Interface for validation results
    /// </summary>
    public interface IValidationResult
    {
        /// <summary>
        /// Gets whether the validation passed
        /// </summary>
        bool IsValid { get; }
        
        /// <summary>
        /// Gets the validation errors if any
        /// </summary>
        string[] Errors { get; }
    }
}