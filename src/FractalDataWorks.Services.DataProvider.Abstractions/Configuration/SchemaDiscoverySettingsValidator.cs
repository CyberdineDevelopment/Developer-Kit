using FluentValidation;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Validator for SchemaDiscoverySettings.
/// </summary>
internal sealed class SchemaDiscoverySettingsValidator : AbstractValidator<SchemaDiscoverySettings>
{
    public SchemaDiscoverySettingsValidator()
    {
        RuleFor(x => x.CacheStrategy)
            .IsInEnum()
            .WithMessage("Cache strategy must be a valid enum value.");

        RuleFor(x => x.CacheDurationMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Cache duration must be non-negative.");

        RuleFor(x => x.AutoRefreshIntervalMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Auto-refresh interval must be non-negative.");

        RuleFor(x => x.MaxContainers)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum containers must be non-negative.");

        RuleFor(x => x.DiscoveryTimeoutSeconds)
            .GreaterThan(0)
            .When(x => x.Enabled)
            .WithMessage("Discovery timeout must be positive when discovery is enabled.");

        RuleFor(x => x.IncludePatterns)
            .NotNull()
            .WithMessage("Include patterns cannot be null.");

        RuleFor(x => x.ExcludePatterns)
            .NotNull()
            .WithMessage("Exclude patterns cannot be null.");

        RuleFor(x => x.ExtendedSettings)
            .NotNull()
            .WithMessage("Extended settings cannot be null.");

        // Ensure patterns don't contain empty or null values
        RuleForEach(x => x.IncludePatterns)
            .NotEmpty()
            .WithMessage("Include patterns cannot contain empty values.");

        RuleForEach(x => x.ExcludePatterns)
            .NotEmpty()
            .WithMessage("Exclude patterns cannot contain empty values.");

        // Cache duration should be reasonable when auto-refresh is enabled
        RuleFor(x => x.CacheDurationMinutes)
            .GreaterThan(x => x.AutoRefreshIntervalMinutes)
            .When(x => x.AutoRefreshIntervalMinutes > 0 && x.CacheDurationMinutes > 0)
            .WithMessage("Cache duration should be greater than auto-refresh interval.");
    }
}