using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.TestHelper;
using FractalDataWorks.Services.ExternalConnections.MsSql;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests;

/// <summary>
/// Tests for MsSqlConfigurationValidator to ensure all validation rules work correctly.
/// </summary>
public sealed class MsSqlConfigurationValidatorTests
{
    private readonly ITestOutputHelper _output;
    private readonly MsSqlConfigurationValidator _validator;

    public MsSqlConfigurationValidatorTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _validator = new MsSqlConfigurationValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ShouldFailValidationWhenConnectionStringIsEmptyOrNull(string connectionString)
    {
        // Arrange
        var config = new MsSqlConfiguration { ConnectionString = connectionString };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConnectionString)
              .WithErrorMessage("Connection string is required.");

        _output.WriteLine($"Connection string '{connectionString}' correctly fails validation");
    }

    [Theory]
    [InlineData("Server=localhost;Database=test;Trusted_Connection=true;")]
    [InlineData("Data Source=server;Initial Catalog=db;User ID=user;Password=pass;")]
    [InlineData("Server=(localdb)\\MSSQLLocalDB;Database=TestDb;Integrated Security=true;")]
    public void ShouldPassValidationWhenConnectionStringIsValid(string connectionString)
    {
        // Arrange
        var config = new MsSqlConfiguration { ConnectionString = connectionString };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConnectionString);

        _output.WriteLine($"Connection string '{connectionString}' passes validation");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-100)]
    public void ShouldFailValidationWhenCommandTimeoutIsNegative(int timeout)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            CommandTimeoutSeconds = timeout 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CommandTimeoutSeconds)
              .WithErrorMessage("Command timeout must be non-negative.");

        _output.WriteLine($"Command timeout {timeout} correctly fails validation");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(120)]
    [InlineData(3600)]
    public void ShouldPassValidationWhenCommandTimeoutIsNonNegative(int timeout)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            CommandTimeoutSeconds = timeout 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CommandTimeoutSeconds);

        _output.WriteLine($"Command timeout {timeout} passes validation");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void ShouldFailValidationWhenConnectionTimeoutIsZeroOrNegative(int timeout)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            ConnectionTimeoutSeconds = timeout 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConnectionTimeoutSeconds)
              .WithErrorMessage("Connection timeout must be positive.");

        _output.WriteLine($"Connection timeout {timeout} correctly fails validation");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(120)]
    public void ShouldPassValidationWhenConnectionTimeoutIsPositive(int timeout)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            ConnectionTimeoutSeconds = timeout 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConnectionTimeoutSeconds);

        _output.WriteLine($"Connection timeout {timeout} passes validation");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ShouldFailValidationWhenDefaultSchemaIsEmptyOrNull(string schema)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            DefaultSchema = schema 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DefaultSchema)
              .WithErrorMessage("Default schema cannot be empty.");

        _output.WriteLine($"Default schema '{schema}' correctly fails validation");
    }

    [Theory]
    [InlineData("dbo")]
    [InlineData("custom")]
    [InlineData("sales")]
    [InlineData("inventory")]
    public void ShouldPassValidationWhenDefaultSchemaIsValid(string schema)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            DefaultSchema = schema 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DefaultSchema);

        _output.WriteLine($"Default schema '{schema}' passes validation");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-10)]
    public void ShouldFailValidationWhenMinPoolSizeIsNegative(int minPoolSize)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            MinPoolSize = minPoolSize 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MinPoolSize)
              .WithErrorMessage("Minimum pool size must be non-negative.");

        _output.WriteLine($"Min pool size {minPoolSize} correctly fails validation");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    public void ShouldPassValidationWhenMinPoolSizeIsNonNegative(int minPoolSize)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            MinPoolSize = minPoolSize 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MinPoolSize);

        _output.WriteLine($"Min pool size {minPoolSize} passes validation");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void ShouldFailValidationWhenMaxPoolSizeIsZeroOrNegative(int maxPoolSize)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            MaxPoolSize = maxPoolSize 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxPoolSize)
              .WithErrorMessage("Maximum pool size must be positive.");

        _output.WriteLine($"Max pool size {maxPoolSize} correctly fails validation");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void ShouldPassValidationWhenMaxPoolSizeIsPositive(int maxPoolSize)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            MaxPoolSize = maxPoolSize 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxPoolSize);

        _output.WriteLine($"Max pool size {maxPoolSize} passes validation");
    }

    [Theory]
    [InlineData(10, 5)] // Max < Min
    [InlineData(20, 10)] // Max < Min
    [InlineData(50, 25)] // Max < Min
    public void ShouldFailValidationWhenMaxPoolSizeIsLessThanMinPoolSizeWithPoolingEnabled(int minPoolSize, int maxPoolSize)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            EnableConnectionPooling = true,
            MinPoolSize = minPoolSize,
            MaxPoolSize = maxPoolSize 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxPoolSize)
              .WithErrorMessage("Maximum pool size must be greater than or equal to minimum pool size.");

        _output.WriteLine($"Max pool size {maxPoolSize} < Min pool size {minPoolSize} correctly fails validation when pooling enabled");
    }

    [Theory]
    [InlineData(5, 5)] // Max == Min
    [InlineData(10, 20)] // Max > Min
    [InlineData(0, 100)] // Max > Min
    public void ShouldPassValidationWhenMaxPoolSizeIsGreaterThanOrEqualToMinPoolSizeWithPoolingEnabled(int minPoolSize, int maxPoolSize)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            EnableConnectionPooling = true,
            MinPoolSize = minPoolSize,
            MaxPoolSize = maxPoolSize 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxPoolSize);

        _output.WriteLine($"Max pool size {maxPoolSize} >= Min pool size {minPoolSize} passes validation when pooling enabled");
    }

    [Fact]
    public void ShouldNotValidatePoolSizesWhenConnectionPoolingIsDisabled()
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            EnableConnectionPooling = false,
            MinPoolSize = 20,
            MaxPoolSize = 10 // Max < Min, but pooling is disabled
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxPoolSize);

        _output.WriteLine("Pool size validation is skipped when connection pooling is disabled");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-10)]
    public void ShouldFailValidationWhenMaxRetryAttemptsIsNegative(int maxRetryAttempts)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            MaxRetryAttempts = maxRetryAttempts 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxRetryAttempts)
              .WithErrorMessage("Maximum retry attempts must be non-negative.");

        _output.WriteLine($"Max retry attempts {maxRetryAttempts} correctly fails validation");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void ShouldPassValidationWhenMaxRetryAttemptsIsNonNegative(int maxRetryAttempts)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            MaxRetryAttempts = maxRetryAttempts 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxRetryAttempts);

        _output.WriteLine($"Max retry attempts {maxRetryAttempts} passes validation");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ShouldFailValidationWhenRetryDelayIsZeroOrNegativeWithRetryLogicEnabled(int retryDelay)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            EnableRetryLogic = true,
            RetryDelayMilliseconds = retryDelay 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RetryDelayMilliseconds)
              .WithErrorMessage("Retry delay must be positive when retry logic is enabled.");

        _output.WriteLine($"Retry delay {retryDelay} correctly fails validation when retry logic enabled");
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(5000)]
    public void ShouldPassValidationWhenRetryDelayIsPositiveWithRetryLogicEnabled(int retryDelay)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            EnableRetryLogic = true,
            RetryDelayMilliseconds = retryDelay 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RetryDelayMilliseconds);

        _output.WriteLine($"Retry delay {retryDelay} passes validation when retry logic enabled");
    }

    [Fact]
    public void ShouldNotValidateRetryDelayWhenRetryLogicIsDisabled()
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            EnableRetryLogic = false,
            RetryDelayMilliseconds = 0 // Would fail validation if retry logic was enabled
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RetryDelayMilliseconds);

        _output.WriteLine("Retry delay validation is skipped when retry logic is disabled");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ShouldFailValidationWhenMaxSqlLogLengthIsZeroOrNegative(int maxSqlLogLength)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            MaxSqlLogLength = maxSqlLogLength 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxSqlLogLength)
              .WithErrorMessage("Maximum SQL log length must be positive.");

        _output.WriteLine($"Max SQL log length {maxSqlLogLength} correctly fails validation");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(5000)]
    public void ShouldPassValidationWhenMaxSqlLogLengthIsPositive(int maxSqlLogLength)
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            MaxSqlLogLength = maxSqlLogLength 
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxSqlLogLength);

        _output.WriteLine($"Max SQL log length {maxSqlLogLength} passes validation");
    }

    [Fact]
    public void ShouldFailValidationWhenSchemaMappingHasNullOrEmptyKey()
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            SchemaMappings = new Dictionary<string, string>
            {
                ["ValidKey"] = "valid.table",
                [""] = "invalid.table"
            }
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SchemaMappings)
              .WithErrorMessage("Schema mapping keys and values cannot be null or empty.");

        _output.WriteLine("Schema mapping with empty key correctly fails validation");
    }

    [Fact]
    public void ShouldFailValidationWhenSchemaMappingHasNullOrEmptyValue()
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            SchemaMappings = new Dictionary<string, string>
            {
                ["ValidKey"] = "valid.table",
                ["InvalidKey"] = ""
            }
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SchemaMappings)
              .WithErrorMessage("Schema mapping keys and values cannot be null or empty.");

        _output.WriteLine("Schema mapping with empty value correctly fails validation");
    }

    [Fact]
    public void ShouldPassValidationWhenSchemaMappingsAreValid()
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            SchemaMappings = new Dictionary<string, string>
            {
                ["Products"] = "catalog.products",
                ["Orders"] = "sales.orders",
                ["Customers"] = "customers"
            }
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SchemaMappings);

        _output.WriteLine($"Schema mappings with {config.SchemaMappings.Count} valid entries pass validation");
    }

    [Fact]
    public void ShouldPassValidationWhenSchemaMappingsAreEmpty()
    {
        // Arrange
        var config = new MsSqlConfiguration 
        { 
            ConnectionString = "Server=test;Database=db;",
            SchemaMappings = new Dictionary<string, string>()
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SchemaMappings);

        _output.WriteLine("Empty schema mappings pass validation");
    }

    [Fact]
    public void ShouldPassValidationWithCompleteValidConfiguration()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;",
            CommandTimeoutSeconds = 30,
            ConnectionTimeoutSeconds = 15,
            DefaultSchema = "dbo",
            EnableConnectionPooling = true,
            MinPoolSize = 0,
            MaxPoolSize = 100,
            EnableMultipleActiveResultSets = false,
            EnableRetryLogic = true,
            MaxRetryAttempts = 3,
            RetryDelayMilliseconds = 1000,
            EnableSqlLogging = false,
            MaxSqlLogLength = 1000,
            SchemaMappings = new Dictionary<string, string>
            {
                ["Products"] = "catalog.products",
                ["Orders"] = "sales.orders"
            }
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();

        _output.WriteLine("Complete valid configuration passes all validation rules");
    }

    [Fact]
    public void ShouldFailValidationWithMultipleErrors()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "", // Invalid
            CommandTimeoutSeconds = -1, // Invalid
            ConnectionTimeoutSeconds = 0, // Invalid
            DefaultSchema = "", // Invalid
            MinPoolSize = -5, // Invalid
            MaxPoolSize = 0, // Invalid
            MaxRetryAttempts = -1, // Invalid
            EnableRetryLogic = true,
            RetryDelayMilliseconds = 0, // Invalid when retry logic enabled
            MaxSqlLogLength = 0 // Invalid
        };

        // Act
        var result = _validator.TestValidate(config);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConnectionString);
        result.ShouldHaveValidationErrorFor(x => x.CommandTimeoutSeconds);
        result.ShouldHaveValidationErrorFor(x => x.ConnectionTimeoutSeconds);
        result.ShouldHaveValidationErrorFor(x => x.DefaultSchema);
        result.ShouldHaveValidationErrorFor(x => x.MinPoolSize);
        result.ShouldHaveValidationErrorFor(x => x.MaxPoolSize);
        result.ShouldHaveValidationErrorFor(x => x.MaxRetryAttempts);
        result.ShouldHaveValidationErrorFor(x => x.RetryDelayMilliseconds);
        result.ShouldHaveValidationErrorFor(x => x.MaxSqlLogLength);

        var errorCount = result.Errors.Count;
        _output.WriteLine($"Invalid configuration generates {errorCount} validation errors");
        foreach (var error in result.Errors)
        {
            _output.WriteLine($"  - {error.PropertyName}: {error.ErrorMessage}");
        }
    }
}