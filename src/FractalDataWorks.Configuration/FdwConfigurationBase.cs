using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentValidation.Results;

namespace FractalDataWorks;

/// <summary>
/// Base class for all configuration objects in the FractalDataWorks framework.
/// Provides common functionality for configuration validation and serialization.
/// </summary>
/// <remarks>
/// Configuration classes should inherit from this base class to ensure consistent
/// behavior across the framework. This class provides virtual methods that can be
/// overridden to implement custom validation and initialization logic.
/// The "Fdw" prefix avoids namespace collisions with common configuration types.
/// </remarks>
/// <ExcludeFromTest>Abstract base configuration class with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage]
public abstract class FdwConfigurationBase : IFdwConfiguration
{
    /// <summary>
    /// Gets the section name for this configuration.
    /// </summary>
    public abstract string SectionName { get; }

    /// <summary>
    /// Validates the configuration settings.
    /// </summary>
    /// <returns>A FluentValidation ValidationResult indicating success or failure with details.</returns>
    /// <remarks>
    /// Override this method in derived classes to implement custom validation logic.
    /// The framework will call this method before using the configuration to ensure
    /// all required settings are properly configured.
    /// </remarks>
    public virtual ValidationResult Validate()
    {
        // Default implementation returns success - derived classes should override
        return new ValidationResult();
    }


    /// <summary>
    /// Initializes the configuration with default values.
    /// </summary>
    /// <remarks>
    /// Override this method in derived classes to set default values for configuration
    /// properties. This method is called during configuration object construction.
    /// </remarks>
    protected virtual void InitializeDefaults()
    {
        // Default implementation does nothing
    }

    /// <summary>
    /// Called after the configuration has been loaded and validated.
    /// </summary>
    /// <remarks>
    /// Override this method in derived classes to perform any post-initialization
    /// setup that depends on the configuration values being set and validated.
    /// </remarks>
    protected virtual void OnConfigurationLoaded()
    {
        // Default implementation does nothing
    }
}