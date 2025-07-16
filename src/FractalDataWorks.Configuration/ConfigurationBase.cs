using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Validation;
using FluentValidation;
using FluentValidation.Results;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Base class for all configuration types in the Fractal framework.
/// </summary>
/// <typeparam name="TConfiguration">The derived configuration type.</typeparam>
public abstract class ConfigurationBase<TConfiguration> : IFractalConfiguration
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
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
                var validationTask = Validate();
                validationTask.Wait();
                _lastValidationResult = validationTask.Result;
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this configuration was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets the last validation result.
    /// </summary>
    public IValidationResult? LastValidationResult => _lastValidationResult;

    /// <summary>
    /// Validates this configuration.
    /// </summary>
    /// <returns>A task containing the validation result.</returns>
    public virtual async Task<IValidationResult> Validate()
    {
        var validator = GetValidator();
        if (validator == null)
        {
            return new ConfigurationValidationResult(true, Array.Empty<IValidationError>());
        }

        var fluentResult = await validator.ValidateAsync((TConfiguration)this);
        
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
            Severity.Info => ValidationSeverity.Info,
            _ => ValidationSeverity.Error
        };
    }
}

/// <summary>
/// Implementation of IValidationResult for configuration validation.
/// </summary>
internal class ConfigurationValidationResult : IValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationResult"/> class.
    /// </summary>
    /// <param name="isValid">Whether the validation succeeded.</param>
    /// <param name="errors">The validation errors.</param>
    public ConfigurationValidationResult(bool isValid, IEnumerable<IValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets a value indicating whether the validation succeeded.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the collection of validation errors.
    /// </summary>
    public IReadOnlyList<IValidationError> Errors { get; }
}

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