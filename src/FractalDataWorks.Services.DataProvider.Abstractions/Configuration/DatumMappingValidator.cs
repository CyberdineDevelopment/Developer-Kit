using System;
using System.Collections.Generic;
using FluentValidation;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Validator for DatumMapping.
/// </summary>
internal sealed class DatumMappingValidator : AbstractValidator<DatumMapping>
{
    public DatumMappingValidator()
    {
        RuleFor(x => x.LogicalName)
            .NotEmpty()
            .WithMessage("Logical name is required.")
            .MaximumLength(100)
            .WithMessage("Logical name cannot exceed 100 characters.");

        // Physical columns are required unless it's a calculated field with transform expression
        RuleFor(x => x.PhysicalColumns)
            .Must(HavePhysicalColumnsOrTransform)
            .WithMessage("Datum mapping must have either physical columns or a transform expression.");

        // Physical column names cannot be empty
        RuleForEach(x => x.PhysicalColumns)
            .NotEmpty()
            .WithMessage("Physical column names cannot be empty.")
            .MaximumLength(100)
            .WithMessage("Physical column names cannot exceed 100 characters.");

        // Transform expression cannot be empty if specified
        RuleFor(x => x.TransformExpression)
            .MaximumLength(1000)
            .WithMessage("Transform expression cannot exceed 1000 characters.");

        // Default value type should be compatible with DataType if both are specified
        RuleFor(x => x.DefaultValue)
            .Must(BeCompatibleWithDataType)
            .When(x => x.DefaultValue != null && x.DataType != null)
            .WithMessage("Default value type must be compatible with the specified data type.");
    }

    private static bool HavePhysicalColumnsOrTransform(DatumMapping mapping, List<string> physicalColumns)
    {
        return physicalColumns.Count > 0 || !string.IsNullOrWhiteSpace(mapping.TransformExpression);
    }

    private static bool BeCompatibleWithDataType(DatumMapping mapping, object? defaultValue)
    {
        if (defaultValue == null || mapping.DataType == null)
            return true;

        // Allow string default values as they can be converted
        if (defaultValue is string)
            return true;

        try
        {
            // Try to convert the default value to the target type
            Convert.ChangeType(defaultValue, mapping.DataType);
            return true;
        }
        catch
        {
            return false;
        }
    }
}