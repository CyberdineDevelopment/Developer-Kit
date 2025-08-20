using FluentValidation.Results;

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
    /// <returns>A FluentValidation ValidationResult indicating success or failure with details.</returns>
    ValidationResult Validate();
}
