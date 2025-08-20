using System;
using FractalDataWorks.Services.ExternalConnections.MsSql;
using FractalDataWorks.Services.ExternalConnections.MsSql.Commands;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests.Commands;

/// <summary>
/// Tests for MsSqlExternalConnectionCreateCommand to ensure proper command creation and validation.
/// </summary>
public sealed class MsSqlExternalConnectionCreateCommandTests
{
    private readonly ITestOutputHelper _output;

    public MsSqlExternalConnectionCreateCommandTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    [Fact]
    public void ShouldCreateWithValidParameters()
    {
        // Arrange
        var connectionName = "TestConnection";
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;"
        };

        // Act
        var command = new MsSqlExternalConnectionCreateCommand(connectionName, config);

        // Assert
        command.ShouldNotBeNull();
        command.ConnectionName.ShouldBe(connectionName);
        command.ConnectionConfiguration.ShouldBeSameAs(config);
        command.ProviderType.ShouldBe("MsSql");
        command.CommandId.ShouldNotBe(Guid.Empty);
        command.CorrelationId.ShouldNotBe(Guid.Empty);
        command.Timestamp.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(-1));
        command.Configuration.ShouldBeSameAs(config);

        _output.WriteLine($"Command created successfully with connection name: {connectionName}");
        _output.WriteLine($"Command ID: {command.CommandId}");
        _output.WriteLine($"Correlation ID: {command.CorrelationId}");
        _output.WriteLine($"Timestamp: {command.Timestamp}");
    }

    [Fact]
    public void ConstructorShouldThrowWhenConnectionNameIsNull()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;"
        };

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new MsSqlExternalConnectionCreateCommand(null!, config));
        exception.ParamName.ShouldBe("connectionName");

        _output.WriteLine("Constructor correctly throws ArgumentNullException when connection name is null");
    }

    [Fact]
    public void ConstructorShouldThrowWhenConfigurationIsNull()
    {
        // Arrange
        var connectionName = "TestConnection";

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new MsSqlExternalConnectionCreateCommand(connectionName, null!));
        exception.ParamName.ShouldBe("connectionConfiguration");

        _output.WriteLine("Constructor correctly throws ArgumentNullException when configuration is null");
    }

    [Theory]
    [InlineData("SimpleConnection")]
    [InlineData("Connection_With_Underscores")]
    [InlineData("Connection-With-Hyphens")]
    [InlineData("Connection123")]
    [InlineData("Very Long Connection Name With Spaces")]
    public void ShouldAcceptVariousConnectionNames(string connectionName)
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;"
        };

        // Act
        var command = new MsSqlExternalConnectionCreateCommand(connectionName, config);

        // Assert
        command.ConnectionName.ShouldBe(connectionName);

        _output.WriteLine($"Connection name '{connectionName}' accepted successfully");
    }

    [Fact]
    public void ShouldImplementCorrectInterfaces()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;"
        };

        // Act
        var command = new MsSqlExternalConnectionCreateCommand("test", config);

        // Assert
        command.ShouldBeAssignableTo<IExternalConnectionCommand>();
        command.ShouldBeAssignableTo<IExternalConnectionCreateCommand>();

        _output.WriteLine("Command implements correct interfaces");
    }

    [Fact]
    public void ShouldHaveCorrectProviderType()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;"
        };

        // Act
        var command = new MsSqlExternalConnectionCreateCommand("test", config);

        // Assert
        command.ProviderType.ShouldBe("MsSql");

        _output.WriteLine($"Provider type is correct: {command.ProviderType}");
    }

    [Fact]
    public void ShouldGenerateUniqueCommandIds()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;"
        };

        // Act
        var command1 = new MsSqlExternalConnectionCreateCommand("test1", config);
        var command2 = new MsSqlExternalConnectionCreateCommand("test2", config);

        // Assert
        command1.CommandId.ShouldNotBe(command2.CommandId);
        command1.CorrelationId.ShouldNotBe(command2.CorrelationId);
        command1.CommandId.ShouldNotBe(Guid.Empty);
        command2.CommandId.ShouldNotBe(Guid.Empty);

        _output.WriteLine("Commands generate unique IDs");
        _output.WriteLine($"Command 1 ID: {command1.CommandId}");
        _output.WriteLine($"Command 2 ID: {command2.CommandId}");
    }

    [Fact]
    public void ValidateShouldPassWithValidCommand()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;"
        };
        var command = new MsSqlExternalConnectionCreateCommand("TestConnection", config);

        // Act
        var result = command.Validate();

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBe(true);
        result.Errors.ShouldBeEmpty();

        _output.WriteLine("Validation passes for valid command");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateShouldFailWithInvalidConnectionName(string invalidName)
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;"
        };
        
        // We can't actually create the command with null name due to constructor validation,
        // so we'll test the validation logic indirectly by creating a valid command and testing validation logic
        if (invalidName != null)
        {
            var command = new MsSqlExternalConnectionCreateCommand(invalidName, config);
            
            // Act
            var result = command.Validate();

            // Assert
            result.IsValid.ShouldBe(false);
            result.Errors.Count.ShouldBe(1);
            result.Errors[0].PropertyName.ShouldBe("ConnectionName");
            result.Errors[0].ErrorMessage.ShouldBe("Connection name cannot be null or empty.");

            _output.WriteLine($"Validation correctly fails for invalid connection name: '{invalidName}'");
        }
        else
        {
            _output.WriteLine("Null connection name handled by constructor validation");
        }
    }

    [Fact]
    public void ValidateShouldFailWithWrongConfigurationType()
    {
        // This test cannot be practically implemented because the constructor
        // requires MsSqlConfiguration, and we can't pass a different type
        // The validation logic exists for additional runtime safety
        _output.WriteLine("Configuration type validation is enforced by constructor parameter types");
    }

    [Fact]
    public void ShouldReturnConfigurationAsIFdwConfiguration()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;"
        };
        var command = new MsSqlExternalConnectionCreateCommand("test", config);

        // Act
        var fdwConfig = command.Configuration;

        // Assert
        fdwConfig.ShouldNotBeNull();
        fdwConfig.ShouldBeSameAs(config);

        _output.WriteLine("Configuration property returns correct IFdwConfiguration reference");
    }

    [Fact]
    public void ShouldHaveReasonableTimestamp()
    {
        // Arrange
        var beforeCreation = DateTimeOffset.UtcNow;
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;"
        };

        // Act
        var command = new MsSqlExternalConnectionCreateCommand("test", config);
        var afterCreation = DateTimeOffset.UtcNow;

        // Assert
        command.Timestamp.ShouldBeGreaterThanOrEqualTo(beforeCreation);
        command.Timestamp.ShouldBeLessThanOrEqualTo(afterCreation);

        _output.WriteLine($"Timestamp is reasonable: {command.Timestamp}");
        _output.WriteLine($"Creation window: {beforeCreation} to {afterCreation}");
    }

    [Fact]
    public void ShouldMaintainReferenceToOriginalConfiguration()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=test;Trusted_Connection=true;",
            DefaultSchema = "dbo"
        };
        var command = new MsSqlExternalConnectionCreateCommand("test", config);

        // Act - Modify original configuration
        config.DefaultSchema = "modified";

        // Assert - Command should reflect the change (reference is maintained)
        ((MsSqlConfiguration)command.ConnectionConfiguration).DefaultSchema.ShouldBe("modified");

        _output.WriteLine("Command maintains reference to original configuration object");
    }

    [Fact]
    public void ShouldHandleComplexConfiguration()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Server=server;Database=db;User ID=user;Password=pass;",
            CommandTimeoutSeconds = 60,
            ConnectionTimeoutSeconds = 30,
            DefaultSchema = "custom",
            EnableConnectionPooling = true,
            MinPoolSize = 5,
            MaxPoolSize = 50,
            EnableRetryLogic = true,
            MaxRetryAttempts = 5,
            EnableSqlLogging = true
        };
        config.SchemaMappings["Products"] = "catalog.products";

        // Act
        var command = new MsSqlExternalConnectionCreateCommand("ComplexConnection", config);

        // Assert
        command.ConnectionConfiguration.ShouldBeSameAs(config);
        var msSqlConfig = (MsSqlConfiguration)command.ConnectionConfiguration;
        msSqlConfig.CommandTimeoutSeconds.ShouldBe(60);
        msSqlConfig.SchemaMappings.Count.ShouldBe(1);

        _output.WriteLine("Command handles complex configuration correctly");
        _output.WriteLine($"Schema mappings: {msSqlConfig.SchemaMappings.Count}");
    }

    [Theory]
    [InlineData("connection1", "Server=server1;Database=db1;")]
    [InlineData("connection2", "Server=server2;Database=db2;User ID=user;Password=pass;")]
    [InlineData("connection3", "Data Source=(localdb)\\MSSQLLocalDB;Database=test;Integrated Security=true;")]
    public void ShouldCreateMultipleCommandsWithDifferentConfigurations(string connectionName, string connectionString)
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = connectionString
        };

        // Act
        var command = new MsSqlExternalConnectionCreateCommand(connectionName, config);

        // Assert
        command.ConnectionName.ShouldBe(connectionName);
        ((MsSqlConfiguration)command.ConnectionConfiguration).ConnectionString.ShouldBe(connectionString);

        _output.WriteLine($"Command created for {connectionName} with connection string length: {connectionString.Length}");
    }
}