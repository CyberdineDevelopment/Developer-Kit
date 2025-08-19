using FluentValidation;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Validator for ContainerAccessConfiguration.
/// </summary>
internal sealed class ContainerAccessConfigurationValidator : AbstractValidator<ContainerAccessConfiguration>
{
    public ContainerAccessConfigurationValidator()
    {
        RuleFor(x => x.MaxRecords)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum records must be non-negative.");

        RuleFor(x => x.TimeoutSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Timeout seconds must be non-negative.");
    }
}