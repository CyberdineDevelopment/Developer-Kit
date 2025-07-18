using System;
using FluentValidation;

namespace FractalDataWorks.Configuration.Validation;

/// <summary>
/// Base validator for configuration types.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration to validate.</typeparam>
public abstract class ConfigurationValidatorBase<TConfiguration> : AbstractValidator<TConfiguration>
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidatorBase{TConfiguration}"/> class.
    /// </summary>
    protected ConfigurationValidatorBase()
    {
        // Common validation rules for all configurations
        RuleFor(c => c.Id)
            .GreaterThan(0)
            .When(c => c.Id != 0)
            .WithMessage("Configuration ID must be greater than 0");

        RuleFor(c => c.Name)
            .NotEmpty()
            .WithMessage("Configuration name is required")
            .MaximumLength(100)
            .WithMessage("Configuration name must not exceed 100 characters");

        RuleFor(c => c.CreatedAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Created date cannot be in the future");

        RuleFor(c => c.ModifiedAt)
            .GreaterThanOrEqualTo(c => c.CreatedAt)
            .When(c => c.ModifiedAt.HasValue)
            .WithMessage("Modified date must be after created date");

        // Add custom validation rules in derived classes
        ConfigureRules();
    }

    /// <summary>
    /// Configures additional validation rules in derived validators.
    /// </summary>
    protected abstract void ConfigureRules();
}