using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using FractalDataWorks.Services.DataProvider.EnhancedEnums;
using FractalDataWorks.Services.DataProvider.Services;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.DataProvider.Tests.Services;

/// <summary>
/// Tests for the ExternalDataConnectionProvider implementation.
/// </summary>
public sealed class ExternalDataConnectionProviderTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<ExternalDataConnectionProvider>> _loggerMock;
    private readonly Mock<IExternalDataConnection<IExternalConnectionConfiguration>> _connectionMock;
    private readonly Mock<IExternalConnectionConfiguration> _configurationMock;

    public ExternalDataConnectionProviderTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<ExternalDataConnectionProvider>>();
        _connectionMock = new Mock<IExternalDataConnection<IExternalConnectionConfiguration>>();
        _configurationMock = new Mock<IExternalConnectionConfiguration>();
    }

    [Fact]
    public void ConstructorShouldThrowWhenServiceProviderIsNull()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new ExternalDataConnectionProvider(null!, _loggerMock.Object));

        exception.ParamName.ShouldBe("serviceProvider");

        
    }

    [Fact]
    public void ConstructorShouldThrowWhenLoggerIsNull()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new ExternalDataConnectionProvider(_serviceProviderMock.Object, null!));

        exception.ParamName.ShouldBe("logger");

        
    }

    [Fact]
    public void ConstructorShouldCreateProviderWithValidParameters()
    {
        // Act
        var provider = new ExternalDataConnectionProvider(_serviceProviderMock.Object, _loggerMock.Object);

        // Assert
        provider.ShouldNotBeNull();
        provider.ConnectionCount.ShouldBe(0);

        
    }

    [Fact]
    public async Task ExecuteCommandShouldReturnFailureWhenCommandIsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.ExecuteCommand<Customer>(null!);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Command cannot be null");

        
    }

    [Fact]
    public async Task ExecuteCommandShouldReturnFailureWhenConnectionNameIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();
        var command = DataCommands.QueryAll<Customer>();

        // Act
        var result = await provider.ExecuteCommand<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Command must specify a valid connection name");

        
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteCommandShouldReturnFailureWhenConnectionNameIsInvalid(string? connectionName)
    {
        // Arrange
        var provider = CreateProvider();
        var command = DataCommands.QueryAll<Customer>(connectionName!);

        // Act
        var result = await provider.ExecuteCommand<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Command must specify a valid connection name");

        
    }

    [Fact]
    public async Task ExecuteCommandShouldReturnFailureWhenCommandValidationFails()
    {
        // Arrange
        var provider = CreateProvider();
        var command = CreateInvalidCommand();

        // Act
        var result = await provider.ExecuteCommand<Customer>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Command validation failed");

        
    }

    [Fact]
    public async Task ExecuteCommandShouldReturnFailureWhenConnectionNotFound()
    {
        // Arrange
        var provider = CreateProvider();
        var command = DataCommands.QueryAll<Customer>("NonExistentConnection");

        // Act
        var result = await provider.ExecuteCommand<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Connection 'NonExistentConnection' not found");

        
    }

    [Fact]
    public async Task ExecuteCommandShouldReturnFailureWhenConnectionDoesNotImplementInterface()
    {
        // Arrange
        var provider = CreateProvider();
        var command = DataCommands.QueryAll<Customer>("TestConnection");
        
        // Register a non-conforming object
        provider.RegisterConnection("TestConnection", new object() as IExternalDataConnection<IExternalConnectionConfiguration>);

        // Act
        var result = await provider.ExecuteCommand<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Connection 'TestConnection' does not implement the expected interface");

        
    }

    [Fact]
    public async Task ExecuteCommandShouldReturnFailureWhenConnectionTestFails()
    {
        // Arrange
        var provider = CreateProvider();
        var command = DataCommands.QueryAll<Customer>("TestConnection");

        _connectionMock
            .Setup(c => c.TestConnection(It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Failure("Connection test failed"));

        provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Act
        var result = await provider.ExecuteCommand<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Connection 'TestConnection' is not available");

        
    }

    [Fact]
    public async Task ExecuteCommandShouldReturnFailureWhenConnectionTestReturnsFalse()
    {
        // Arrange
        var provider = CreateProvider();
        var command = DataCommands.QueryAll<Customer>("TestConnection");

        _connectionMock
            .Setup(c => c.TestConnection(It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Success(false));

        provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Act
        var result = await provider.ExecuteCommand<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Connection 'TestConnection' is not available");

        
    }

    [Fact]
    public async Task ExecuteCommandShouldReturnSuccessWhenCommandExecutesSuccessfully()
    {
        // Arrange
        var provider = CreateProvider();
        var command = DataCommands.QueryAll<Customer>("TestConnection");
        var expectedCustomers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", IsActive = true },
            new() { Id = 2, Name = "Jane Smith", IsActive = true }
        };

        SetupSuccessfulConnection(expectedCustomers);
        provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Act
        var result = await provider.ExecuteCommand<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedCustomers);
        result.Value!.Count.ShouldBe(2);

        
    }

    [Fact]
    public async Task ExecuteCommandShouldReturnFailureWhenCommandExecutionFails()
    {
        // Arrange
        var provider = CreateProvider();
        var command = DataCommands.QueryAll<Customer>("TestConnection");

        _connectionMock
            .Setup(c => c.TestConnection(It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Success(true));

        _connectionMock
            .Setup(c => c.Execute<List<Customer>>(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<List<Customer>>.Failure("Query execution failed"));

        provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Act
        var result = await provider.ExecuteCommand<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Query execution failed");

        
    }

    [Fact]
    public async Task ExecuteCommandShouldHandleExceptionsGracefully()
    {
        // Arrange
        var provider = CreateProvider();
        var command = DataCommands.QueryAll<Customer>("TestConnection");

        _connectionMock
            .Setup(c => c.TestConnection(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection error"));

        provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Act
        var result = await provider.ExecuteCommand<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Connection error");

        
    }

    [Fact]
    public async Task DiscoverConnectionSchemaShouldReturnFailureWhenConnectionNameIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.DiscoverConnectionSchema(string.Empty);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Connection name cannot be null or empty");

        
    }

    [Fact]
    public async Task DiscoverConnectionSchemaShouldReturnFailureWhenConnectionNotFound()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.DiscoverConnectionSchema("NonExistentConnection");

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Connection 'NonExistentConnection' not found");

        
    }

    [Fact]
    public async Task DiscoverConnectionSchemaShouldReturnSuccessWhenSchemaDiscoverySucceeds()
    {
        // Arrange
        var provider = CreateProvider();
        var expectedContainers = new List<DataContainer>
        {
            new(DataPath.Create(".", "sales", "customers"), "customers", ContainerType.Table),
            new(DataPath.Create(".", "sales", "orders"), "orders", ContainerType.Table)
        };

        _connectionMock
            .Setup(c => c.DiscoverSchema(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<IEnumerable<DataContainer>>.Success(expectedContainers));

        provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Act
        var result = await provider.DiscoverConnectionSchema("TestConnection");

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count().ShouldBe(2);

        
    }

    [Fact]
    public async Task IsConnectionAvailableShouldReturnFailureWhenConnectionNameIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.IsConnectionAvailable(string.Empty);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Connection name cannot be null or empty");

        
    }

    [Fact]
    public async Task IsConnectionAvailableShouldReturnFalseWhenConnectionNotFound()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.IsConnectionAvailable("NonExistentConnection");

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();

        
    }

    [Fact]
    public async Task IsConnectionAvailableShouldReturnTrueWhenConnectionIsAvailable()
    {
        // Arrange
        var provider = CreateProvider();

        _connectionMock
            .Setup(c => c.TestConnection(It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Success(true));

        provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Act
        var result = await provider.IsConnectionAvailable("TestConnection");

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();

        
    }

    [Fact]
    public async Task GetConnectionsMetadataShouldReturnSuccessWithEmptyDictionaryWhenNoConnections()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.GetConnectionsMetadata();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count.ShouldBe(0);

        
    }

    [Fact]
    public async Task GetConnectionsMetadataShouldReturnMetadataForAllConnections()
    {
        // Arrange
        var provider = CreateProvider();
        var connectionInfo = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Status"] = "Available",
            ["Type"] = "TestConnection"
        };

        _connectionMock
            .Setup(c => c.GetConnectionInfo(It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<IDictionary<string, object>>.Success(connectionInfo));

        provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Act
        var result = await provider.GetConnectionsMetadata();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count.ShouldBe(1);
        result.Value!.ShouldContainKey("TestConnection");

        
    }

    [Fact]
    public void RegisterConnectionShouldThrowWhenNameIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            provider.RegisterConnection(string.Empty, _connectionMock.Object));

        exception.ParamName.ShouldBe("name");

        
    }

    [Fact]
    public void RegisterConnectionShouldThrowWhenConnectionIsNull()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            provider.RegisterConnection<IExternalConnectionConfiguration>("TestConnection", null!));

        exception.ParamName.ShouldBe("connection");

        
    }

    [Fact]
    public void RegisterConnectionShouldReturnTrueWhenConnectionRegisteredSuccessfully()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Assert
        result.ShouldBeTrue();
        provider.ConnectionCount.ShouldBe(1);
        provider.GetConnectionNames().ShouldContain("TestConnection");

        
    }

    [Fact]
    public void RegisterConnectionShouldReturnFalseWhenConnectionNameAlreadyExists()
    {
        // Arrange
        var provider = CreateProvider();
        var secondConnection = new Mock<IExternalDataConnection<IExternalConnectionConfiguration>>();

        provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Act
        var result = provider.RegisterConnection("TestConnection", secondConnection.Object);

        // Assert
        result.ShouldBeFalse();
        provider.ConnectionCount.ShouldBe(1); // Should remain 1

        
    }

    [Fact]
    public void UnregisterConnectionShouldThrowWhenNameIsEmpty()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            provider.UnregisterConnection(string.Empty));

        exception.ParamName.ShouldBe("name");

        
    }

    [Fact]
    public void UnregisterConnectionShouldReturnTrueWhenConnectionUnregisteredSuccessfully()
    {
        // Arrange
        var provider = CreateProvider();
        provider.RegisterConnection("TestConnection", _connectionMock.Object);

        // Act
        var result = provider.UnregisterConnection("TestConnection");

        // Assert
        result.ShouldBeTrue();
        provider.ConnectionCount.ShouldBe(0);
        provider.GetConnectionNames().ShouldNotContain("TestConnection");

        
    }

    [Fact]
    public void UnregisterConnectionShouldReturnFalseWhenConnectionNotFound()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = provider.UnregisterConnection("NonExistentConnection");

        // Assert
        result.ShouldBeFalse();

        
    }

    [Fact]
    public void GetConnectionNamesShouldReturnAllRegisteredConnectionNames()
    {
        // Arrange
        var provider = CreateProvider();
        var connection1 = new Mock<IExternalDataConnection<IExternalConnectionConfiguration>>();
        var connection2 = new Mock<IExternalDataConnection<IExternalConnectionConfiguration>>();

        provider.RegisterConnection("Connection1", connection1.Object);
        provider.RegisterConnection("Connection2", connection2.Object);

        // Act
        var names = provider.GetConnectionNames();

        // Assert
        names.ShouldNotBeNull();
        names.Count().ShouldBe(2);
        names.ShouldContain("Connection1");
        names.ShouldContain("Connection2");

        
    }

    [Fact]
    public void ConnectionCountShouldReturnCorrectCount()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert - Initial count
        provider.ConnectionCount.ShouldBe(0);

        // Add connections
        provider.RegisterConnection("Connection1", _connectionMock.Object);
        provider.ConnectionCount.ShouldBe(1);

        var connection2 = new Mock<IExternalDataConnection<IExternalConnectionConfiguration>>();
        provider.RegisterConnection("Connection2", connection2.Object);
        provider.ConnectionCount.ShouldBe(2);

        // Remove connection
        provider.UnregisterConnection("Connection1");
        provider.ConnectionCount.ShouldBe(1);

        
    }

    private ExternalDataConnectionProvider CreateProvider()
    {
        return new ExternalDataConnectionProvider(_serviceProviderMock.Object, _loggerMock.Object);
    }

    private void SetupSuccessfulConnection<T>(T expectedResult)
    {
        _connectionMock
            .Setup(c => c.TestConnection(It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Success(true));

        _connectionMock
            .Setup(c => c.Execute<T>(It.IsAny<DataCommandBase>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<T>.Success(expectedResult));
    }

    private TestCommand CreateInvalidCommand()
    {
        return new TestCommand("TestConnection");
    }

    // Helper classes for testing
    private sealed class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // Test command that always fails validation
    private sealed class TestCommand : DataCommandBase
    {
        public TestCommand(string connectionName) : base("test", connectionName, null, typeof(object))
        {
        }

        public override bool IsDataModifying => false;

        protected override DataCommandBase CreateCopy(string? connectionName = null, 
            DataPath? target = null,
            IReadOnlyDictionary<string, object?>? extendedProperties = null,
            IReadOnlyDictionary<string, object>? metadata = null,
            TimeSpan? timeout = null)
        {
            return new TestCommand(connectionName ?? ConnectionName);
        }

        public override ValidationResult Validate()
        {
            return new ValidationResult(new[] { new ValidationFailure("TestProperty", "Test validation error") });
        }
    }
}
