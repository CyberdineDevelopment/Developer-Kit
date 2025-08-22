using System;
using System.Collections.Generic;
using FluentValidation;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Validator for DataStoreConfiguration.
/// </summary>
internal sealed class DataStoreConfigurationValidator : AbstractValidator<DataStoreConfiguration>
{
    public DataStoreConfigurationValidator()
    {
        ConfigureBasicValidation();
        ConfigureConnectionPoolingValidation();
        ConfigureHealthCheckValidation();
        ConfigureContainerMappingValidation();
    }

    private void ConfigureBasicValidation()
    {
        RuleFor(x => x.StoreName)
            .NotEmpty()
            .WithMessage("Store name is required.")
            .MaximumLength(100)
            .WithMessage("Store name cannot exceed 100 characters.");

        RuleFor(x => x.ProviderType)
            .NotEmpty()
            .WithMessage("Provider type is required.")
            .MaximumLength(50)
            .WithMessage("Provider type cannot exceed 50 characters.");

        RuleFor(x => x.MaxConcurrentOperations)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum concurrent operations must be non-negative.");

        RuleFor(x => x.DefaultTimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("Default timeout must be positive.");
    }

    private void ConfigureConnectionPoolingValidation()
    {
        RuleFor(x => x.ConnectionPooling.MinPoolSize)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum pool size must be non-negative.");

        RuleFor(x => x.ConnectionPooling.MaxPoolSize)
            .GreaterThan(0)
            .WithMessage("Maximum pool size must be positive.");

        RuleFor(x => x.ConnectionPooling.MaxPoolSize)
            .GreaterThanOrEqualTo(x => x.ConnectionPooling.MinPoolSize)
            .When(x => x.ConnectionPooling.Enabled)
            .WithMessage("Maximum pool size must be greater than or equal to minimum pool size.");

        RuleFor(x => x.ConnectionPooling.IdleTimeoutSeconds)
            .GreaterThan(0)
            .When(x => x.ConnectionPooling.Enabled)
            .WithMessage("Idle timeout must be positive when connection pooling is enabled.");

        RuleFor(x => x.ConnectionPooling.MaxLifetimeSeconds)
            .GreaterThan(0)
            .When(x => x.ConnectionPooling.Enabled)
            .WithMessage("Max lifetime must be positive when connection pooling is enabled.");
    }

    private void ConfigureHealthCheckValidation()
    {
        RuleFor(x => x.HealthCheck.IntervalSeconds)
            .GreaterThan(0)
            .When(x => x.HealthCheck.Enabled)
            .WithMessage("Health check interval must be positive when health checks are enabled.");

        RuleFor(x => x.HealthCheck.TimeoutSeconds)
            .GreaterThan(0)
            .When(x => x.HealthCheck.Enabled)
            .WithMessage("Health check timeout must be positive when health checks are enabled.");

        RuleFor(x => x.HealthCheck.MaxFailures)
            .GreaterThan(0)
            .When(x => x.HealthCheck.Enabled)
            .WithMessage("Max failures must be positive when health checks are enabled.");
    }

    private void ConfigureContainerMappingValidation()
    {
        RuleFor(x => x.ContainerMappings)
            .Must(HaveUniqueLogicalNames)
            .WithMessage("Container mappings must have unique logical names.");

        RuleForEach(x => x.ContainerMappings)
            .SetValidator(new DataContainerMappingValidator());
    }

    private static bool HaveUniqueLogicalNames(IList<DataContainerMapping> mappings)
    {
        if (mappings.Count == 0)
            return true;

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in mappings)
        {
            if (!names.Add(mapping.LogicalName))
                return false;
        }
        return true;
    }
}