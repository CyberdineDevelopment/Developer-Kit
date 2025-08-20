using System;
using System.Collections.Generic;
using FluentValidation;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Validator for DataContainerMapping.
/// </summary>
internal sealed class DataContainerMappingValidator : AbstractValidator<DataContainerMapping>
{
    public DataContainerMappingValidator()
    {
        RuleFor(x => x.LogicalName)
            .NotEmpty()
            .WithMessage("Logical name is required.")
            .MaximumLength(100)
            .WithMessage("Logical name cannot exceed 100 characters.");

        RuleFor(x => x.PhysicalPath)
            .NotEmpty()
            .WithMessage("Physical path is required.")
            .MaximumLength(500)
            .WithMessage("Physical path cannot exceed 500 characters.");

        // Validate datum mappings have unique logical names
        RuleFor(x => x.DatumMappings)
            .Must(HaveUniqueLogicalNames)
            .WithMessage("Datum mappings must have unique logical names.");

        // Validate each datum mapping
        RuleForEach(x => x.DatumMappings.Values)
            .SetValidator(new DatumMappingValidator());

        // Validate access configurations
        RuleFor(x => x.ReadAccess)
            .SetValidator(new ContainerAccessConfigurationValidator()!)
            .When(x => x.ReadAccess != null);

        RuleFor(x => x.WriteAccess)
            .SetValidator(new ContainerAccessConfigurationValidator()!)
            .When(x => x.WriteAccess != null);
    }

    private static bool HaveUniqueLogicalNames(IDictionary<string, DatumMapping> mappings)
    {
        if (mappings.Count == 0)
            return true;

        // Keys should already be unique in a dictionary, but check for case-insensitive duplicates
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in mappings.Keys)
        {
            if (!names.Add(key))
                return false;
        }
        return true;
    }
}