using FluentValidation;

namespace FractalDataWorks.Services.ExternalConnections.MsSql;

/// <summary>
/// Validator for MsSqlConfiguration.
/// </summary>
internal sealed class MsSqlConfigurationValidator : AbstractValidator<MsSqlConfiguration>
{
    public MsSqlConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required.");

        RuleFor(x => x.CommandTimeoutSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Command timeout must be non-negative.");

        RuleFor(x => x.ConnectionTimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("Connection timeout must be positive.");

        RuleFor(x => x.DefaultSchema)
            .NotEmpty()
            .WithMessage("Default schema cannot be empty.");

        RuleFor(x => x.MinPoolSize)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum pool size must be non-negative.");

        RuleFor(x => x.MaxPoolSize)
            .GreaterThan(0)
            .WithMessage("Maximum pool size must be positive.");

        RuleFor(x => x.MaxPoolSize)
            .GreaterThanOrEqualTo(x => x.MinPoolSize)
            .When(x => x.EnableConnectionPooling)
            .WithMessage("Maximum pool size must be greater than or equal to minimum pool size.");

        RuleFor(x => x.MaxRetryAttempts)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum retry attempts must be non-negative.");

        RuleFor(x => x.RetryDelayMilliseconds)
            .GreaterThan(0)
            .When(x => x.EnableRetryLogic)
            .WithMessage("Retry delay must be positive when retry logic is enabled.");

        RuleFor(x => x.MaxSqlLogLength)
            .GreaterThan(0)
            .WithMessage("Maximum SQL log length must be positive.");

        // Validate schema mappings don't contain null or empty values
        RuleForEach(x => x.SchemaMappings)
            .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
            .WithMessage("Schema mapping keys and values cannot be null or empty.");
    }
}