using System;
using System.Collections.Generic;
using FractalDataWorks.Services.ExternalConnections.MsSql;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests;

/// <summary>
/// Tests for MsSqlConfiguration to ensure proper configuration management, validation, and behavior.
/// </summary>
public sealed class MsSqlConfigurationTests
{
    private readonly ITestOutputHelper _output;

    public MsSqlConfigurationTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    [Fact]
    public void ShouldInitializeWithDefaultValues()
    {
        // Act
        var config = new MsSqlConfiguration();

        // Assert
        config.ConnectionString.ShouldBe(string.Empty);
        config.CommandTimeoutSeconds.ShouldBe(30);
        config.ConnectionTimeoutSeconds.ShouldBe(15);
        config.DefaultSchema.ShouldBe("dbo");
        config.SchemaMappings.ShouldNotBeNull();
        config.SchemaMappings.Count.ShouldBe(0);
        config.EnableConnectionPooling.ShouldBe(true);
        config.MinPoolSize.ShouldBe(0);
        config.MaxPoolSize.ShouldBe(100);
        config.EnableMultipleActiveResultSets.ShouldBe(false);
        config.EnableRetryLogic.ShouldBe(true);
        config.MaxRetryAttempts.ShouldBe(3);
        config.RetryDelayMilliseconds.ShouldBe(1000);
        config.EnableSqlLogging.ShouldBe(false);
        config.MaxSqlLogLength.ShouldBe(1000);
        config.SectionName.ShouldBe("ExternalConnections:MsSql");

        _output.WriteLine($"Default values: ConnectionTimeout={config.ConnectionTimeoutSeconds}, CommandTimeout={config.CommandTimeoutSeconds}, DefaultSchema={config.DefaultSchema}");
    }

    [Theory]
    [InlineData("Server=localhost;Database=test;Trusted_Connection=true;", 30, 15)]
    [InlineData("Server=(localdb)\\MSSQLLocalDB;Database=TestDb;Integrated Security=true;", 60, 30)]
    [InlineData("Data Source=server;Initial Catalog=db;User ID=user;Password=pass;", 120, 45)]
    public void ShouldSetPropertiesCorrectly(string connectionString, int commandTimeout, int connectionTimeout)
    {
        // Act
        var config = new MsSqlConfiguration
        {
            ConnectionString = connectionString,
            CommandTimeoutSeconds = commandTimeout,
            ConnectionTimeoutSeconds = connectionTimeout
        };

        // Assert
        config.ConnectionString.ShouldBe(connectionString);
        config.CommandTimeoutSeconds.ShouldBe(commandTimeout);
        config.ConnectionTimeoutSeconds.ShouldBe(connectionTimeout);

        _output.WriteLine($"Set connection string length: {connectionString.Length}");
        _output.WriteLine($"Set timeouts: Command={commandTimeout}, Connection={connectionTimeout}");
    }

    [Theory]
    [InlineData("custom", 5, 200, false)]
    [InlineData("sales", 0, 50, true)]
    [InlineData("inventory", 10, 150, true)]
    public void ShouldSetPoolingPropertiesCorrectly(string schema, int minPool, int maxPool, bool enablePooling)
    {
        // Act
        var config = new MsSqlConfiguration
        {
            DefaultSchema = schema,
            MinPoolSize = minPool,
            MaxPoolSize = maxPool,
            EnableConnectionPooling = enablePooling
        };

        // Assert
        config.DefaultSchema.ShouldBe(schema);
        config.MinPoolSize.ShouldBe(minPool);
        config.MaxPoolSize.ShouldBe(maxPool);
        config.EnableConnectionPooling.ShouldBe(enablePooling);

        _output.WriteLine($"Schema: {schema}, Pooling: {enablePooling}, Pool Size: {minPool}-{maxPool}");
    }

    [Theory]
    [InlineData(false, 0, 500, false, 2000)]
    [InlineData(true, 5, 2000, true, 500)]
    [InlineData(true, 1, 100, false, 10000)]
    public void ShouldSetRetryAndLoggingPropertiesCorrectly(bool enableMars, int maxRetries, int retryDelay, bool enableLogging, int maxLogLength)
    {
        // Act
        var config = new MsSqlConfiguration
        {
            EnableMultipleActiveResultSets = enableMars,
            MaxRetryAttempts = maxRetries,
            RetryDelayMilliseconds = retryDelay,
            EnableSqlLogging = enableLogging,
            MaxSqlLogLength = maxLogLength
        };

        // Assert
        config.EnableMultipleActiveResultSets.ShouldBe(enableMars);
        config.MaxRetryAttempts.ShouldBe(maxRetries);
        config.RetryDelayMilliseconds.ShouldBe(retryDelay);
        config.EnableSqlLogging.ShouldBe(enableLogging);
        config.MaxSqlLogLength.ShouldBe(maxLogLength);

        _output.WriteLine($"MARS: {enableMars}, Retries: {maxRetries}, Delay: {retryDelay}ms, Logging: {enableLogging}, MaxLog: {maxLogLength}");
    }

    [Fact]
    public void SchemaMappingsShouldInitializeWithCorrectComparer()
    {
        // Act
        var config = new MsSqlConfiguration();

        // Assert
        config.SchemaMappings.ShouldNotBeNull();
        config.SchemaMappings.Comparer.ShouldBe(StringComparer.Ordinal);

        _output.WriteLine("Schema mappings uses StringComparer.Ordinal");
    }

    [Fact]
    public void ShouldAddSchemaMappingsCorrectly()
    {
        // Arrange
        var config = new MsSqlConfiguration();
        var mappings = new Dictionary<string, string>
        {
            ["Products"] = "catalog.products",
            ["Orders"] = "sales.orders",
            ["Customers"] = "customers",
            ["Inventory"] = "warehouse.stock"
        };

        // Act
        foreach (var mapping in mappings)
        {
            config.SchemaMappings[mapping.Key] = mapping.Value;
        }

        // Assert
        config.SchemaMappings.Count.ShouldBe(4);
        config.SchemaMappings["Products"].ShouldBe("catalog.products");
        config.SchemaMappings["Orders"].ShouldBe("sales.orders");
        config.SchemaMappings["Customers"].ShouldBe("customers");
        config.SchemaMappings["Inventory"].ShouldBe("warehouse.stock");

        _output.WriteLine($"Added {config.SchemaMappings.Count} schema mappings");
        foreach (var mapping in config.SchemaMappings)
        {
            _output.WriteLine($"  {mapping.Key} -> {mapping.Value}");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetSanitizedConnectionStringShouldHandleEmptyStrings(string connectionString)
    {
        // Arrange
        var config = new MsSqlConfiguration { ConnectionString = connectionString };

        // Act
        var sanitized = config.GetSanitizedConnectionString();

        // Assert
        sanitized.ShouldBe("(empty)");

        _output.WriteLine($"Empty connection string returns: {sanitized}");
    }

    [Theory]
    [InlineData("Server=localhost;Database=test;User ID=testuser;Password=secret123;", "Server=localhost;Database=test;User ID=***;Password=***;")]
    [InlineData("Data Source=server;Initial Catalog=db;uid=user;pwd=pass;", "Data Source=server;Initial Catalog=db;uid=***;pwd=***;")]
    [InlineData("Server=localhost;Database=test;Trusted_Connection=true;", "Server=localhost;Database=test;Trusted_Connection=true;")]
    public void GetSanitizedConnectionStringShouldRemoveSensitiveInformation(string original, string expected)
    {
        // Arrange
        var config = new MsSqlConfiguration { ConnectionString = original };

        // Act
        var sanitized = config.GetSanitizedConnectionString();

        // Assert
        sanitized.ShouldBe(expected);

        _output.WriteLine($"Original: {original}");
        _output.WriteLine($"Sanitized: {sanitized}");
    }

    [Theory]
    [InlineData("Server=localhost;User ID=admin;Password=verylongpasswordwithspecialchars!@#$;Database=test;", "Server=localhost;User ID=***;Password=***;Database=test;")]
    [InlineData("Server=server;Initial Catalog=db;User Id=user@domain.com;Password=complex!Pass123;", "Server=server;Initial Catalog=db;User Id=***;Password=***;")]
    public void GetSanitizedConnectionStringShouldHandleComplexPasswords(string original, string expected)
    {
        // Arrange
        var config = new MsSqlConfiguration { ConnectionString = original };

        // Act
        var sanitized = config.GetSanitizedConnectionString();

        // Assert
        sanitized.ShouldBe(expected);

        _output.WriteLine($"Complex password sanitized correctly");
        _output.WriteLine($"  Original length: {original.Length}");
        _output.WriteLine($"  Sanitized length: {sanitized.Length}");
    }

    [Theory]
    [InlineData("Products")]
    [InlineData("Orders")]
    [InlineData("Customers")]
    public void ResolveSchemaAndTableShouldReturnDefaultSchemaForUnmappedContainers(string containerName)
    {
        // Arrange
        var config = new MsSqlConfiguration();

        // Act
        var (schema, table) = config.ResolveSchemaAndTable(containerName);

        // Assert
        schema.ShouldBe("dbo");
        table.ShouldBe(containerName);

        _output.WriteLine($"Container '{containerName}' resolved to schema='{schema}', table='{table}'");
    }

    [Theory]
    [InlineData("Products", "catalog.products", "catalog", "products")]
    [InlineData("Orders", "sales.orders", "sales", "orders")]
    [InlineData("Logs", "audit.application_logs", "audit", "application_logs")]
    public void ResolveSchemaAndTableShouldUseMappingWhenAvailable(string containerName, string mapping, string expectedSchema, string expectedTable)
    {
        // Arrange
        var config = new MsSqlConfiguration();
        config.SchemaMappings[containerName] = mapping;

        // Act
        var (schema, table) = config.ResolveSchemaAndTable(containerName);

        // Assert
        schema.ShouldBe(expectedSchema);
        table.ShouldBe(expectedTable);

        _output.WriteLine($"Container '{containerName}' with mapping '{mapping}' resolved to schema='{schema}', table='{table}'");
    }

    [Theory]
    [InlineData("Customers", "customers", "dbo", "customers")]
    [InlineData("Products", "product_catalog", "dbo", "product_catalog")]
    public void ResolveSchemaAndTableShouldHandleSingleNameMapping(string containerName, string mapping, string expectedSchema, string expectedTable)
    {
        // Arrange
        var config = new MsSqlConfiguration();
        config.SchemaMappings[containerName] = mapping;

        // Act
        var (schema, table) = config.ResolveSchemaAndTable(containerName);

        // Assert
        schema.ShouldBe(expectedSchema);
        table.ShouldBe(expectedTable);

        _output.WriteLine($"Single name mapping: '{containerName}' -> '{mapping}' resolved to schema='{schema}', table='{table}'");
    }

    [Theory]
    [InlineData("Test", "schema.table.extra.parts", "dbo", "Test")]
    [InlineData("Invalid", "", "dbo", "Invalid")]
    [InlineData("Null", "   ", "dbo", "Null")]
    public void ResolveSchemaAndTableShouldHandleInvalidMappings(string containerName, string mapping, string expectedSchema, string expectedTable)
    {
        // Arrange
        var config = new MsSqlConfiguration();
        config.SchemaMappings[containerName] = mapping;

        // Act
        var (schema, table) = config.ResolveSchemaAndTable(containerName);

        // Assert
        schema.ShouldBe(expectedSchema);
        table.ShouldBe(expectedTable);

        _output.WriteLine($"Invalid mapping: '{containerName}' -> '{mapping}' resolved to schema='{schema}', table='{table}'");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ResolveSchemaAndTableShouldThrowForInvalidContainerNames(string containerName)
    {
        // Arrange
        var config = new MsSqlConfiguration();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => config.ResolveSchemaAndTable(containerName));
        exception.ParamName.ShouldBe("containerName");
        exception.Message.ShouldContain("Container name cannot be null or empty");

        _output.WriteLine($"Invalid container name '{containerName}' throws ArgumentException");
    }

    [Fact]
    public void ResolveSchemaAndTableShouldHandleCaseSensitiveMappings()
    {
        // Arrange
        var config = new MsSqlConfiguration();
        config.SchemaMappings["Products"] = "catalog.products";
        config.SchemaMappings["products"] = "inventory.items";

        // Act
        var (schema1, table1) = config.ResolveSchemaAndTable("Products");
        var (schema2, table2) = config.ResolveSchemaAndTable("products");

        // Assert
        schema1.ShouldBe("catalog");
        table1.ShouldBe("products");
        schema2.ShouldBe("inventory");
        table2.ShouldBe("items");

        _output.WriteLine($"Case sensitive: 'Products' -> {schema1}.{table1}, 'products' -> {schema2}.{table2}");
    }

    [Fact]
    public void ShouldReturnCorrectSectionName()
    {
        // Arrange
        var config = new MsSqlConfiguration();

        // Act & Assert
        config.SectionName.ShouldBe("ExternalConnections:MsSql");

        _output.WriteLine($"Section name: {config.SectionName}");
    }

    [Fact]
    public void CopyToShouldCopyAllProperties()
    {
        // Arrange
        var source = new MsSqlConfiguration
        {
            ConnectionString = "Server=test;Database=db;",
            CommandTimeoutSeconds = 60,
            ConnectionTimeoutSeconds = 30,
            DefaultSchema = "custom",
            EnableConnectionPooling = false,
            MinPoolSize = 5,
            MaxPoolSize = 50,
            EnableMultipleActiveResultSets = true,
            EnableRetryLogic = false,
            MaxRetryAttempts = 5,
            RetryDelayMilliseconds = 2000,
            EnableSqlLogging = true,
            MaxSqlLogLength = 500
        };
        source.SchemaMappings["Test"] = "schema.table";

        var target = new MsSqlConfiguration();

        // Act
        source.CopyTo(target);

        // Assert
        target.ConnectionString.ShouldBe(source.ConnectionString);
        target.CommandTimeoutSeconds.ShouldBe(source.CommandTimeoutSeconds);
        target.ConnectionTimeoutSeconds.ShouldBe(source.ConnectionTimeoutSeconds);
        target.DefaultSchema.ShouldBe(source.DefaultSchema);
        target.EnableConnectionPooling.ShouldBe(source.EnableConnectionPooling);
        target.MinPoolSize.ShouldBe(source.MinPoolSize);
        target.MaxPoolSize.ShouldBe(source.MaxPoolSize);
        target.EnableMultipleActiveResultSets.ShouldBe(source.EnableMultipleActiveResultSets);
        target.EnableRetryLogic.ShouldBe(source.EnableRetryLogic);
        target.MaxRetryAttempts.ShouldBe(source.MaxRetryAttempts);
        target.RetryDelayMilliseconds.ShouldBe(source.RetryDelayMilliseconds);
        target.EnableSqlLogging.ShouldBe(source.EnableSqlLogging);
        target.MaxSqlLogLength.ShouldBe(source.MaxSqlLogLength);
        
        target.SchemaMappings.ShouldNotBeSameAs(source.SchemaMappings);
        target.SchemaMappings.Count.ShouldBe(source.SchemaMappings.Count);
        target.SchemaMappings["Test"].ShouldBe("schema.table");

        _output.WriteLine("All properties copied successfully, including schema mappings");
        _output.WriteLine($"Schema mappings copied: {target.SchemaMappings.Count} items");
    }

    [Fact]
    public void CopyToShouldNotShareSchemaMappingsReference()
    {
        // Arrange
        var source = new MsSqlConfiguration();
        source.SchemaMappings["Original"] = "original.table";
        var target = new MsSqlConfiguration();

        // Act
        source.CopyTo(target);
        source.SchemaMappings["NewItem"] = "new.table";
        target.SchemaMappings["TargetItem"] = "target.table";

        // Assert
        source.SchemaMappings.Count.ShouldBe(2);
        target.SchemaMappings.Count.ShouldBe(2);
        source.SchemaMappings.ShouldContainKey("NewItem");
        source.SchemaMappings.ShouldNotContainKey("TargetItem");
        target.SchemaMappings.ShouldContainKey("TargetItem");
        target.SchemaMappings.ShouldNotContainKey("NewItem");

        _output.WriteLine("Schema mappings are properly isolated after copy");
        _output.WriteLine($"Source mappings: {string.Join(", ", source.SchemaMappings.Keys)}");
        _output.WriteLine($"Target mappings: {string.Join(", ", target.SchemaMappings.Keys)}");
    }

    [Fact]
    public void ShouldHaveValidatorAttached()
    {
        // Arrange
        var config = new MsSqlConfiguration();

        // Act
        var validationResult = config.Validate();

        // Assert
        validationResult.ShouldNotBeNull();
        // With empty connection string, it should fail validation
        validationResult.IsValid.ShouldBe(false);
        validationResult.Errors.Count.ShouldBeGreaterThan(0);

        _output.WriteLine($"Validation performed successfully, found {validationResult.Errors.Count} errors");
        foreach (var error in validationResult.Errors)
        {
            _output.WriteLine($"  - {error.PropertyName}: {error.ErrorMessage}");
        }
    }

    [Fact]
    public void ShouldValidateSuccessfullyWithValidConfiguration()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;",
            CommandTimeoutSeconds = 30,
            ConnectionTimeoutSeconds = 15,
            DefaultSchema = "dbo"
        };

        // Act
        var validationResult = config.Validate();

        // Assert
        validationResult.ShouldNotBeNull();
        validationResult.IsValid.ShouldBe(true);
        validationResult.Errors.Count.ShouldBe(0);

        _output.WriteLine("Valid configuration passes validation");
    }
}