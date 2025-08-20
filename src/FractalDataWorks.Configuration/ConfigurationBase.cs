using System;
using System.Collections.Generic;
using System.Linq;

using FractalDataWorks.Results;
using FluentValidation;
using FluentValidation.Results;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Base class for all configuration types in the Fractal framework.
/// </summary>
/// <typeparam name="TConfiguration">The derived configuration type.</typeparam>
public abstract class ConfigurationBase<TConfiguration> : FdwConfigurationBase
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
{

    /// <summary>
    /// Gets or sets the unique identifier for this configuration instance.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this configuration.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this configuration is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets the timestamp when this configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this configuration was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets the section name for this configuration.
    /// </summary>
    public override abstract string SectionName { get; }



    /// <summary>
    /// Gets the validator for this configuration type.
    /// </summary>
    /// <returns>The validator instance or null if no validation is required.</returns>
    protected virtual IValidator<TConfiguration>? GetValidator()
    {
        return null;
    }

    /// <summary>
    /// Marks this configuration as modified.
    /// </summary>
    protected void MarkAsModified()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a clone of this configuration.
    /// </summary>
    /// <returns>A cloned instance of the configuration.</returns>
    public virtual TConfiguration Clone()
    {
        var clone = new TConfiguration();
        CopyTo(clone);
        return clone;
    }

    /// <summary>
    /// Copies the properties of this configuration to another instance.
    /// </summary>
    /// <param name="target">The target configuration.</param>
    protected virtual void CopyTo(TConfiguration target)
    {
        target.Id = Id;
        target.Name = Name;
        target.IsEnabled = IsEnabled;
        target.CreatedAt = CreatedAt;
        target.ModifiedAt = ModifiedAt;
    }
}