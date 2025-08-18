using FluentValidation;
using FractalDataWorks.Configuration.Validation;

namespace FractalDataWorks.Services.DataProvider.MsSql.Configuration;

/// <summary>
/// Validator for MsSqlConfiguration.
/// </summary>
public sealed class MsSqlConfigurationValidator : ConfigurationValidatorBase<MsSqlConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlConfigurationValidator"/> class.
    /// </summary>
    public MsSqlConfigurationValidator()
    {
        ConfigureConnectionStringValidation();
        ConfigureTimeoutValidation();
        ConfigurePoolingValidation();
        ConfigureSchemaValidation();
        ConfigureServerValidation();
        ConfigureDatabaseValidation();
        ConfigurePortValidation();
        ConfigureInstanceValidation();
        ConfigureAuthenticationValidation();
        ConfigureOptionalSettingsValidation();
        ConfigurePacketSizeValidation();
        ConfigureMappingValidation();
        ConfigureProviderValidation();
        ConfigureLogicalValidation();
    }

    /// <summary>
    /// Configures connection string validation rules.
    /// </summary>
    private void ConfigureConnectionStringValidation()
    {
        When(x => string.IsNullOrEmpty(x.ConnectionString), () =>
        {
            RuleFor(x => x.ServerName)
                .NotEmpty()
                .WithMessage("Server name is required when connection string is not provided");

            RuleFor(x => x.DatabaseName)
                .NotEmpty()
                .WithMessage("Database name is required when connection string is not provided");
        });
    }

    /// <summary>
    /// Configures timeout validation rules.
    /// </summary>
    private void ConfigureTimeoutValidation()
    {
        RuleFor(x => x.ConnectionTimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("Connection timeout must be greater than 0")
            .LessThanOrEqualTo(2147483)
            .WithMessage("Connection timeout cannot exceed 2147483 seconds");

        RuleFor(x => x.CommandTimeoutSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Command timeout must be non-negative")
            .LessThanOrEqualTo(2147483)
            .WithMessage("Command timeout cannot exceed 2147483 seconds");
    }

    /// <summary>
    /// Configures pooling validation rules.
    /// </summary>
    private void ConfigurePoolingValidation()
    {
        RuleFor(x => x.MaxPoolSize)
            .GreaterThan(0)
            .When(x => x.EnableConnectionPooling)
            .WithMessage("Maximum pool size must be greater than 0 when connection pooling is enabled")
            .LessThanOrEqualTo(32767)
            .When(x => x.EnableConnectionPooling)
            .WithMessage("Maximum pool size cannot exceed 32767");
    }

    /// <summary>
    /// Configures schema validation rules.
    /// </summary>
    private void ConfigureSchemaValidation()
    {
        RuleFor(x => x.DefaultSchema)
            .NotEmpty()
            .WithMessage("Default schema cannot be empty")
            .MaximumLength(128)
            .WithMessage("Default schema name cannot exceed 128 characters")
            .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*$")
            .WithMessage("Default schema name must be a valid SQL identifier");
    }

    /// <summary>
    /// Configures server validation rules.
    /// </summary>
    private void ConfigureServerValidation()
    {
        RuleFor(x => x.ServerName)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.ServerName))
            .WithMessage("Server name cannot exceed 255 characters");
    }

    /// <summary>
    /// Configures database validation rules.
    /// </summary>
    private void ConfigureDatabaseValidation()
    {
        RuleFor(x => x.DatabaseName)
            .MaximumLength(128)
            .When(x => !string.IsNullOrEmpty(x.DatabaseName))
            .WithMessage("Database name cannot exceed 128 characters")
            .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*$")
            .When(x => !string.IsNullOrEmpty(x.DatabaseName))
            .WithMessage("Database name must be a valid SQL identifier");
    }

    /// <summary>
    /// Configures port validation rules.
    /// </summary>
    private void ConfigurePortValidation()
    {
        RuleFor(x => x.Port)
            .GreaterThan(0)
            .When(x => x.Port.HasValue)
            .WithMessage("Port must be greater than 0")
            .LessThanOrEqualTo(65535)
            .When(x => x.Port.HasValue)
            .WithMessage("Port cannot exceed 65535");
    }

    /// <summary>
    /// Configures instance validation rules.
    /// </summary>
    private void ConfigureInstanceValidation()
    {
        RuleFor(x => x.InstanceName)
            .MaximumLength(16)
            .When(x => !string.IsNullOrEmpty(x.InstanceName))
            .WithMessage("Instance name cannot exceed 16 characters")
            .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*$")
            .When(x => !string.IsNullOrEmpty(x.InstanceName))
            .WithMessage("Instance name must be a valid SQL identifier");

        // Port and instance name are mutually exclusive
        RuleFor(x => x)
            .Must(x => !(x.Port.HasValue && !string.IsNullOrEmpty(x.InstanceName)))
            .WithMessage("Port and instance name cannot both be specified")
            .WithName(nameof(MsSqlConfiguration.Port));
    }

    /// <summary>
    /// Configures authentication validation rules.
    /// </summary>
    private void ConfigureAuthenticationValidation()
    {
        When(x => !x.UseWindowsAuthentication, () =>
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required for SQL Server authentication")
                .MaximumLength(128)
                .WithMessage("Username cannot exceed 128 characters");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required for SQL Server authentication")
                .MinimumLength(1)
                .WithMessage("Password cannot be empty")
                .MaximumLength(128)
                .WithMessage("Password cannot exceed 128 characters");
        });
    }

    /// <summary>
    /// Configures optional settings validation rules.
    /// </summary>
    private void ConfigureOptionalSettingsValidation()
    {
        RuleFor(x => x.ApplicationName)
            .MaximumLength(128)
            .When(x => !string.IsNullOrEmpty(x.ApplicationName))
            .WithMessage("Application name cannot exceed 128 characters");

        RuleFor(x => x.WorkstationId)
            .MaximumLength(128)
            .When(x => !string.IsNullOrEmpty(x.WorkstationId))
            .WithMessage("Workstation ID cannot exceed 128 characters");
    }

    /// <summary>
    /// Configures packet size validation rules.
    /// </summary>
    private void ConfigurePacketSizeValidation()
    {
        RuleFor(x => x.PacketSize)
            .GreaterThanOrEqualTo(512)
            .When(x => x.PacketSize.HasValue)
            .WithMessage("Packet size must be at least 512 bytes")
            .LessThanOrEqualTo(32767)
            .When(x => x.PacketSize.HasValue)
            .WithMessage("Packet size cannot exceed 32767 bytes")
            .Must(BeValidPacketSize)
            .When(x => x.PacketSize.HasValue)
            .WithMessage("Packet size must be a multiple of 512 bytes");
    }

    /// <summary>
    /// Configures mapping validation rules.
    /// </summary>
    private void ConfigureMappingValidation()
    {
        RuleForEach(x => x.SchemaMapping)
            .Must(BeValidSchemaMapping)
            .WithMessage("Schema mapping keys and values must be valid SQL identifiers");

        RuleForEach(x => x.AdditionalParameters)
            .Must(BeValidConnectionStringParameter)
            .WithMessage("Additional parameter keys cannot be empty and values cannot be null");
    }

    /// <summary>
    /// Configures provider validation rules.
    /// </summary>
    private void ConfigureProviderValidation()
    {
        RuleFor(x => x.ProviderType)
            .NotEmpty()
            .WithMessage("Provider type is required")
            .Equal("SqlServer")
            .WithMessage("Provider type must be 'SqlServer' for SQL Server configurations");
    }

    /// <summary>
    /// Configures logical validation rules.
    /// </summary>
    private void ConfigureLogicalValidation()
    {
        RuleFor(x => x)
            .Must(HaveValidConnectionConfiguration)
            .WithMessage("Either a complete connection string or valid server/database configuration must be provided")
            .WithName("Configuration");
    }

    /// <summary>
    /// Validates that packet size is a multiple of 512 bytes.
    /// </summary>
    /// <param name="packetSize">The packet size to validate.</param>
    /// <returns>True if the packet size is valid; otherwise, false.</returns>
    private static bool BeValidPacketSize(int? packetSize)
    {
        return !packetSize.HasValue || packetSize.Value % 512 == 0;
    }

    /// <summary>
    /// Validates that schema mapping entries have valid SQL identifiers.
    /// </summary>
    /// <param name="schemaMapping">The schema mapping entry to validate.</param>
    /// <returns>True if the schema mapping is valid; otherwise, false.</returns>
    private static bool BeValidSchemaMapping(KeyValuePair<string, string> schemaMapping)
    {
        return IsValidSqlIdentifier(schemaMapping.Key) && IsValidSqlIdentifier(schemaMapping.Value);
    }

    /// <summary>
    /// Validates that additional parameter entries are valid.
    /// </summary>
    /// <param name="parameter">The parameter entry to validate.</param>
    /// <returns>True if the parameter is valid; otherwise, false.</returns>
    private static bool BeValidConnectionStringParameter(KeyValuePair<string, string> parameter)
    {
        return !string.IsNullOrEmpty(parameter.Key) && parameter.Value != null;
    }

    /// <summary>
    /// Validates that the configuration has either a connection string or valid connection components.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <returns>True if the configuration is valid; otherwise, false.</returns>
    private static bool HaveValidConnectionConfiguration(MsSqlConfiguration config)
    {
        // If connection string is provided, it's valid
        if (!string.IsNullOrEmpty(config.ConnectionString))
        {
            return true;
        }

        // Otherwise, we need server name and database name
        return !string.IsNullOrEmpty(config.ServerName) && !string.IsNullOrEmpty(config.DatabaseName);
    }

    /// <summary>
    /// Validates that a string is a valid SQL identifier.
    /// </summary>
    /// <param name="identifier">The identifier to validate.</param>
    /// <returns>True if the identifier is valid; otherwise, false.</returns>
    private static bool IsValidSqlIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return false;
        }

        // SQL identifiers must start with a letter or underscore
        // and can contain letters, digits, underscores, @, #, or $
        return System.Text.RegularExpressions.Regex.IsMatch(
            identifier,
            @"^[a-zA-Z_@#$][a-zA-Z0-9_@#$]*$",
            System.Text.RegularExpressions.RegexOptions.None,
            TimeSpan.FromMilliseconds(100));
    }
}