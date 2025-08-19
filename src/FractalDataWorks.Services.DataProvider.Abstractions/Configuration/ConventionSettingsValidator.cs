using FluentValidation;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Validator for ConventionSettings.
/// </summary>
internal sealed class ConventionSettingsValidator : AbstractValidator<ConventionSettings>
{
    public ConventionSettingsValidator()
    {
        RuleFor(x => x.IdentifierPatterns)
            .NotNull()
            .WithMessage("Identifier patterns cannot be null.");

        RuleFor(x => x.MeasurePatterns)
            .NotNull()
            .WithMessage("Measure patterns cannot be null.");

        RuleFor(x => x.MetadataPatterns)
            .NotNull()
            .WithMessage("Metadata patterns cannot be null.");

        RuleFor(x => x.MeasureDataTypes)
            .NotNull()
            .WithMessage("Measure data types cannot be null.");

        RuleFor(x => x.MetadataDataTypes)
            .NotNull()
            .WithMessage("Metadata data types cannot be null.");

        // Ensure patterns don't contain empty or null values
        RuleForEach(x => x.IdentifierPatterns)
            .NotEmpty()
            .WithMessage("Identifier patterns cannot contain empty values.");

        RuleForEach(x => x.MeasurePatterns)
            .NotEmpty()
            .WithMessage("Measure patterns cannot contain empty values.");

        RuleForEach(x => x.MetadataPatterns)
            .NotEmpty()
            .WithMessage("Metadata patterns cannot contain empty values.");

        RuleForEach(x => x.MeasureDataTypes)
            .NotEmpty()
            .WithMessage("Measure data types cannot contain empty values.");

        RuleForEach(x => x.MetadataDataTypes)
            .NotEmpty()
            .WithMessage("Metadata data types cannot contain empty values.");
    }
}