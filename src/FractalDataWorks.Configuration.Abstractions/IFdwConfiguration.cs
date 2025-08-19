namespace FractalDataWorks;

/// <summary>
/// Base interface for all configuration objects.
/// </summary>
public interface IFdwConfiguration
{
    /// <summary>
    /// Gets the section name for this configuration.
    /// </summary>
    string SectionName { get; }
    
    /// <summary>
    /// Validates this configuration.
    /// </summary>
    /// <returns>An IFdwResult indicating success or failure with details.</returns>
    IFdwResult Validate();
}
