namespace FractalDataWorks;

/// <summary>
/// Base interface for all configuration objects
/// </summary>
public interface IEnhancedConfiguration
{
    /// <summary>
    /// Gets whether the configuration is valid
    /// </summary>
    bool IsValid { get; }
}

/// <summary>
/// Enhanced configuration with self-validation
/// </summary>
/// <typeparam name="T">The configuration type</typeparam>
public interface IEnhancedConfiguration<T> : IEnhancedConfiguration
    where T : class
{
    /// <summary>
    /// Validates the configuration and returns a result
    /// </summary>
    /// <returns>Success with the configuration or failure with error message</returns>
    Result<T> Validate();
}
