using System;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Validation;
using FluentValidation;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Base class for all configuration types in the Fractal framework.
/// </summary>
/// <typeparam name="TConfiguration">The derived configuration type.</typeparam>
public abstract class ConfigurationBase<TConfiguration> : IFdwConfiguration
    where TConfiguration : ConfigurationBase<TConfiguration>, IFdwConfiguration, new()
{
    private bool? _isValid;
    private IValidationResult? _lastValidationResult;

    /// <summary>
    /// Gets or sets the unique identifier for this configuration instance.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this configuration.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this configuration is valid.
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (!_isValid.HasValue)
            {
                // Use GetAwaiter().GetResult() to avoid AggregateException wrapping
                // This is a property getter that must be synchronous
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                _lastValidationResult = Validate().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                _isValid = _lastValidationResult.IsValid;
            }
            return _isValid.Value;
        }
    }

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
    public abstract string SectionName { get; }

    /// <summary>
    /// Gets the last validation result.
    /// </summary>
    public IValidationResult? LastValidationResult => _lastValidationResult;

    /// <summary>
    /// Validates this configuration.
    /// </summary>
    /// <returns>True if the configuration is valid; otherwise, false.</returns>
    bool IFdwConfiguration.Validate()
    {
        return IsValid;
    }

    /// <summary>
    /// Validates this configuration asynchronously.
    /// </summary>
    /// <returns>A task containing the validation result.</returns>
    public virtual async Task<IValidationResult> Validate()
    {
        var validator = GetValidator();
        if (validator == null)
        {
            return new ConfigurationValidationResult(true, Array.Empty<IValidationError>());
        }

        var fluentResult = await validator.ValidateAsync((TConfiguration)this).ConfigureAwait(false);
        
        var errors = fluentResult.Errors
            .Select(e => new ConfigurationValidationError(
                e.PropertyName,
                e.ErrorMessage,
                e.ErrorCode,
                MapSeverity(e.Severity)))
            .ToList();

        _lastValidationResult = new ConfigurationValidationResult(fluentResult.IsValid, errors);
        _isValid = fluentResult.IsValid;

        return _lastValidationResult;
    }

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
        _isValid = null;
        _lastValidationResult = null;
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

    /// <summary>
    /// Maps FluentValidation severity to our validation severity.
    /// </summary>
    /// <param name="severity">The FluentValidation severity.</param>
    /// <returns>The mapped validation severity.</returns>
    private static ValidationSeverity MapSeverity(Severity severity)
    {
        return severity switch
        {
            Severity.Error => ValidationSeverity.Error,
            Severity.Warning => ValidationSeverity.Warning,
            Severity.Info => ValidationSeverity.Information,
            _ => ValidationSeverity.Error
        };
    }
}