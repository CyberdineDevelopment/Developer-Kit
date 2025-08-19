using FluentValidation;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Validator for DatumCategorizationStrategy.
/// </summary>
internal sealed class DatumCategorizationStrategyValidator : AbstractValidator<DatumCategorizationStrategy>
{
    public DatumCategorizationStrategyValidator()
    {
        RuleFor(x => x.Mode)
            .IsInEnum()
            .WithMessage("Categorization mode must be a valid enum value.");

        RuleFor(x => x.FallbackCategory)
            .IsInEnum()
            .WithMessage("Fallback category must be a valid enum value.");

        RuleFor(x => x.Conventions)
            .NotNull()
            .WithMessage("Convention settings cannot be null.")
            .SetValidator(new ConventionSettingsValidator());
    }
}