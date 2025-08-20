using System;
using FractalDataWorks.Services.ExternalConnections.MsSql.Commands;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests.Commands;

/// <summary>
/// Tests for MsSqlExternalConnectionDiscoveryCommand to ensure proper command creation and validation.
/// </summary>
public sealed class MsSqlExternalConnectionDiscoveryCommandTests
{
    private readonly ITestOutputHelper _output;

    public MsSqlExternalConnectionDiscoveryCommandTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    [Fact]
    public void ShouldCreateWithMinimalParameters()
    {
        // Arrange
        var connectionName = "TestConnection";

        // Act
        var command = new MsSqlExternalConnectionDiscoveryCommand(connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ConnectionName.ShouldBe(connectionName);
        command.StartPath.ShouldBeNull();
        command.Options.ShouldNotBeNull();
        command.CommandId.ShouldNotBe(Guid.Empty);
        command.CorrelationId.ShouldNotBe(Guid.Empty);
        command.Timestamp.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(-1));
        command.Configuration.ShouldBeNull();

        _output.WriteLine($"Command created with minimal parameters: {connectionName}");
        _output.WriteLine($"Command ID: {command.CommandId}");
    }

    [Fact]
    public void ShouldCreateWithAllParameters()
    {
        // Arrange
        var connectionName = "TestConnection";
        var startPath = "dbo.Products";
        var options = new ConnectionDiscoveryOptions
        {
            MaxDepth = 5,
            IncludeTables = true,
            IncludeViews = false
        };

        // Act
        var command = new MsSqlExternalConnectionDiscoveryCommand(connectionName, startPath, options);

        // Assert
        command.ConnectionName.ShouldBe(connectionName);
        command.StartPath.ShouldBe(startPath);
        command.Options.ShouldBeSameAs(options);
        command.Options.MaxDepth.ShouldBe(5);

        _output.WriteLine($"Command created with all parameters - Connection: {connectionName}, StartPath: {startPath}");
    }

    [Fact]
    public void ConstructorShouldThrowWhenConnectionNameIsNull()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new MsSqlExternalConnectionDiscoveryCommand(null!));
        exception.ParamName.ShouldBe("connectionName");

        _output.WriteLine("Constructor correctly throws ArgumentNullException when connection name is null");
    }

    [Fact]
    public void ShouldUseDefaultOptionsWhenNotProvided()
    {
        // Arrange
        var connectionName = "TestConnection";

        // Act
        var command = new MsSqlExternalConnectionDiscoveryCommand(connectionName);

        // Assert
        command.Options.ShouldNotBeNull();
        command.Options.ShouldBeOfType<ConnectionDiscoveryOptions>();

        _output.WriteLine("Default options created when not provided");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateShouldFailWithInvalidConnectionName(string invalidName)
    {
        // Note: Constructor will throw for null, so we only test empty/whitespace
        if (invalidName != null)
        {
            // Arrange
            var command = new MsSqlExternalConnectionDiscoveryCommand(invalidName);

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
    public void ValidateShouldFailWithNegativeMaxDepth()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions { MaxDepth = -1 };
        var command = new MsSqlExternalConnectionDiscoveryCommand("TestConnection", options: options);

        // Act
        var result = command.Validate();

        // Assert
        result.IsValid.ShouldBe(false);
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].PropertyName.ShouldContain("MaxDepth");
        result.Errors[0].ErrorMessage.ShouldBe("Max depth cannot be negative.");

        _output.WriteLine("Validation correctly fails for negative MaxDepth");
    }

    [Fact]
    public void ValidateShouldPassWithValidCommand()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions { MaxDepth = 3 };
        var command = new MsSqlExternalConnectionDiscoveryCommand("TestConnection", "dbo", options);

        // Act
        var result = command.Validate();

        // Assert
        result.IsValid.ShouldBe(true);
        result.Errors.ShouldBeEmpty();

        _output.WriteLine("Validation passes for valid command");
    }

    [Fact]
    public void ShouldImplementCorrectInterfaces()
    {
        // Arrange & Act
        var command = new MsSqlExternalConnectionDiscoveryCommand("test");

        // Assert
        command.ShouldBeAssignableTo<IExternalConnectionCommand>();
        command.ShouldBeAssignableTo<IExternalConnectionDiscoveryCommand>();

        _output.WriteLine("Command implements correct interfaces");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("dbo")]
    [InlineData("schema.table")]
    [InlineData("catalog.schema.table")]
    public void ShouldAcceptVariousStartPaths(string startPath)
    {
        // Act
        var command = new MsSqlExternalConnectionDiscoveryCommand("test", startPath);

        // Assert
        command.StartPath.ShouldBe(startPath);

        _output.WriteLine($"Start path '{startPath}' accepted successfully");
    }

    [Fact]
    public void ShouldGenerateUniqueIds()
    {
        // Act
        var command1 = new MsSqlExternalConnectionDiscoveryCommand("test1");
        var command2 = new MsSqlExternalConnectionDiscoveryCommand("test2");

        // Assert
        command1.CommandId.ShouldNotBe(command2.CommandId);
        command1.CorrelationId.ShouldNotBe(command2.CorrelationId);

        _output.WriteLine("Commands generate unique IDs");
    }

    [Fact]
    public void ConfigurationShouldBeNull()
    {
        // Act
        var command = new MsSqlExternalConnectionDiscoveryCommand("test");

        // Assert
        command.Configuration.ShouldBeNull();

        _output.WriteLine("Discovery command Configuration property is null as expected");
    }
}