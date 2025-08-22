using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using FractalDataWorks.Services.DataProvider.EnhancedEnums;
using FractalDataWorks.Services.DataProvider.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.DataProvider.Tests.Services;

/// <summary>
/// Tests for the DataProviderService implementation.
/// </summary>
public sealed class DataProviderServiceTests
{
    private readonly Mock<ILogger<DataProviderService>> _loggerMock;
    private readonly Mock<IDataProvidersConfiguration> _configurationMock;
    private readonly Mock<IExternalDataConnectionProvider> _connectionProviderMock;

    public DataProviderServiceTests()
    {
        _loggerMock = new Mock<ILogger<DataProviderService>>();
        _configurationMock = new Mock<IDataProvidersConfiguration>();
        _connectionProviderMock = new Mock<IExternalDataConnectionProvider>();
    }

    [Fact]
    public void ConstructorShouldThrowWhenConnectionProviderIsNull()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new DataProviderService(_loggerMock.Object, _configurationMock.Object, null!));

        exception.ParamName.ShouldBe("connectionProvider");

        
    }

    [Fact]
    public void ConstructorShouldCreateServiceWithValidParameters()
    {
        // Act
        var service = new DataProviderService(_loggerMock.Object, _configurationMock.Object, _connectionProviderMock.Object);

        // Assert
        service.ShouldNotBeNull();

        
    }

    [Fact]
    public async Task ExecuteShouldReturnFailureWhenCommandIsNull()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.Execute<Customer>(null!);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Command cannot be null");

        
    }

    [Fact]
    public async Task ExecuteShouldReturnFailureWhenConnectionNameIsEmpty()
    {
        // Arrange
        var service = CreateService();
        var command = DataCommands.QueryAll<Customer>();

        // Act
        var result = await service.Execute<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Command must specify a valid connection name");

        
    }

    [Fact]
    public async Task ExecuteShouldReturnFailureWhenConnectionIsNotAvailable()
    {
        // Arrange
        var service = CreateService();
        var command = DataCommands.QueryAll<Customer>("TestConnection");

        _connectionProviderMock
            .Setup(p => p.IsConnectionAvailable("TestConnection", It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Success(false));

        // Act
        var result = await service.Execute<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Connection TestConnection is not available");

        
    }

    [Fact]
    public async Task ExecuteShouldReturnFailureWhenConnectionAvailabilityCheckFails()
    {
        // Arrange
        var service = CreateService();
        var command = DataCommands.QueryAll<Customer>("TestConnection");

        _connectionProviderMock
            .Setup(p => p.IsConnectionAvailable("TestConnection", It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Failure("Connection check failed"));

        // Act
        var result = await service.Execute<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Connection TestConnection is not available");

        
    }

    [Fact]
    public async Task ExecuteShouldReturnSuccessWhenCommandExecutesSuccessfully()
    {
        // Arrange
        var service = CreateService();
        var command = DataCommands.QueryAll<Customer>("TestConnection");
        var expectedCustomers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", IsActive = true },
            new() { Id = 2, Name = "Jane Smith", IsActive = true }
        };

        _connectionProviderMock
            .Setup(p => p.IsConnectionAvailable("TestConnection", It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Success(true));

        _connectionProviderMock
            .Setup(p => p.ExecuteCommand<List<Customer>>(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<List<Customer>>.Success(expectedCustomers));

        // Act
        var result = await service.Execute<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedCustomers);
        result.Value!.Count.ShouldBe(2);

        
    }

    [Fact]
    public async Task ExecuteShouldReturnFailureWhenCommandExecutionFails()
    {
        // Arrange
        var service = CreateService();
        var command = DataCommands.QueryAll<Customer>("TestConnection");

        _connectionProviderMock
            .Setup(p => p.IsConnectionAvailable("TestConnection", It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Success(true));

        _connectionProviderMock
            .Setup(p => p.ExecuteCommand<List<Customer>>(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<List<Customer>>.Failure("Query execution failed"));

        // Act
        var result = await service.Execute<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Query execution failed");

        
    }

    [Fact]
    public async Task ExecuteShouldHandleExceptionsGracefully()
    {
        // Arrange
        var service = CreateService();
        var command = DataCommands.QueryAll<Customer>("TestConnection");

        _connectionProviderMock
            .Setup(p => p.IsConnectionAvailable("TestConnection", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection provider error"));

        // Act
        var result = await service.Execute<List<Customer>>(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Connection provider error");

        
    }

    [Fact]
    public async Task ExecuteNonGenericShouldReturnFailureWhenCommandIsNull()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.Execute(null!);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Command cannot be null");

        
    }

    [Fact]
    public async Task ExecuteNonGenericShouldReturnSuccessWhenCommandExecutesSuccessfully()
    {
        // Arrange
        var service = CreateService();
        var command = DataCommands.Insert(new Customer { Id = 1, Name = "Test" }, "TestConnection");

        _connectionProviderMock
            .Setup(p => p.IsConnectionAvailable("TestConnection", It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Success(true));

        _connectionProviderMock
            .Setup(p => p.ExecuteCommand<object>(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<object>.Success(new object()));

        // Act
        var result = await service.Execute(command);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        
    }

    [Fact]
    public async Task DiscoverSchemaShouldReturnFailureWhenConnectionNameIsEmpty()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.DiscoverSchema(string.Empty);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Connection name cannot be null or empty");

        
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DiscoverSchemaShouldReturnFailureWhenConnectionNameIsInvalid(string? connectionName)
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.DiscoverSchema(connectionName!);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Connection name cannot be null or empty");

        
    }

    [Fact]
    public async Task DiscoverSchemaShouldReturnSuccessWhenSchemaDiscoverySucceeds()
    {
        // Arrange
        var service = CreateService();
        var expectedContainers = new List<DataContainer>
        {
            new(DataPath.Create(".", "sales", "customers"), "customers", ContainerType.Table),
            new(DataPath.Create(".", "sales", "orders"), "orders", ContainerType.Table)
        };

        _connectionProviderMock
            .Setup(p => p.DiscoverConnectionSchema("TestConnection", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<IEnumerable<DataContainer>>.Success(expectedContainers));

        // Act
        var result = await service.DiscoverSchema("TestConnection");

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count().ShouldBe(2);

        
    }

    [Fact]
    public async Task DiscoverSchemaShouldReturnFailureWhenSchemaDiscoveryFails()
    {
        // Arrange
        var service = CreateService();

        _connectionProviderMock
            .Setup(p => p.DiscoverConnectionSchema("TestConnection", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<IEnumerable<DataContainer>>.Failure("Schema discovery failed"));

        // Act
        var result = await service.DiscoverSchema("TestConnection");

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Schema discovery failed");

        
    }

    [Fact]
    public async Task DiscoverSchemaShouldHandleExceptionsGracefully()
    {
        // Arrange
        var service = CreateService();

        _connectionProviderMock
            .Setup(p => p.DiscoverConnectionSchema("TestConnection", null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Schema discovery error"));

        // Act
        var result = await service.DiscoverSchema("TestConnection");

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Schema discovery error");

        
    }

    [Fact]
    public async Task DiscoverSchemaShouldPassStartPathCorrectly()
    {
        // Arrange
        var service = CreateService();
        var startPath = DataPath.Create(".", "sales");
        var expectedContainers = new List<DataContainer>();

        _connectionProviderMock
            .Setup(p => p.DiscoverConnectionSchema("TestConnection", startPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<IEnumerable<DataContainer>>.Success(expectedContainers));

        // Act
        var result = await service.DiscoverSchema("TestConnection", startPath);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        _connectionProviderMock.Verify(
            p => p.DiscoverConnectionSchema("TestConnection", startPath, It.IsAny<CancellationToken>()),
            Times.Once);

        
    }

    [Fact]
    public async Task GetConnectionsInfoShouldReturnSuccessWhenMetadataRetrievalSucceeds()
    {
        // Arrange
        var service = CreateService();
        var expectedMetadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["TestConnection1"] = new { Status = "Available", Type = "SqlServer" },
            ["TestConnection2"] = new { Status = "Available", Type = "PostgreSql" }
        };

        _connectionProviderMock
            .Setup(p => p.GetConnectionsMetadata(It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<IDictionary<string, object>>.Success(expectedMetadata));

        // Act
        var result = await service.GetConnectionsInfo();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count.ShouldBe(2);

        
    }

    [Fact]
    public async Task GetConnectionsInfoShouldReturnFailureWhenMetadataRetrievalFails()
    {
        // Arrange
        var service = CreateService();

        _connectionProviderMock
            .Setup(p => p.GetConnectionsMetadata(It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<IDictionary<string, object>>.Failure("Metadata retrieval failed"));

        // Act
        var result = await service.GetConnectionsInfo();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Metadata retrieval failed");

        
    }

    [Fact]
    public async Task GetConnectionsInfoShouldHandleExceptionsGracefully()
    {
        // Arrange
        var service = CreateService();

        _connectionProviderMock
            .Setup(p => p.GetConnectionsMetadata(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Metadata retrieval error"));

        // Act
        var result = await service.GetConnectionsInfo();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Metadata retrieval error");

        
    }

    [Fact]
    public async Task IDataProviderExecuteShouldDelegateToGenericExecute()
    {
        // Arrange
        var service = CreateService();
        var command = DataCommands.QueryAll<Customer>("TestConnection");
        var expectedCustomers = new List<Customer>
        {
            new() { Id = 1, Name = "Test", IsActive = true }
        };

        _connectionProviderMock
            .Setup(p => p.IsConnectionAvailable("TestConnection", It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Success(true));

        _connectionProviderMock
            .Setup(p => p.ExecuteCommand<List<Customer>>(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<List<Customer>>.Success(expectedCustomers));

        // Act
        var result = await ((IDataProvider)service).Execute<List<Customer>>(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedCustomers);

        
    }

    [Fact]
    public async Task ExecuteShouldWorkWithDifferentCommandTypes()
    {
        // Arrange
        var service = CreateService();
        var insertCommand = DataCommands.Insert(new Customer { Id = 1, Name = "Test" }, "TestConnection");
        var updateCommand = DataCommands.Update(new Customer { Id = 1, Name = "Updated" }, c => c.Id == 1, "TestConnection");
        var deleteCommand = DataCommands.Delete<Customer>(c => c.Id == 1, "TestConnection");

        SetupSuccessfulConnection();

        _connectionProviderMock
            .Setup(p => p.ExecuteCommand<int>(It.IsAny<DataCommandBase>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<int>.Success(1));

        // Act
        var insertResult = await service.Execute<int>(insertCommand);
        var updateResult = await service.Execute<int>(updateCommand);
        var deleteResult = await service.Execute<int>(deleteCommand);

        // Assert
        insertResult.IsSuccess.ShouldBeTrue();
        updateResult.IsSuccess.ShouldBeTrue();
        deleteResult.IsSuccess.ShouldBeTrue();

        
    }

    private DataProviderService CreateService()
    {
        return new DataProviderService(_loggerMock.Object, _configurationMock.Object, _connectionProviderMock.Object);
    }

    private void SetupSuccessfulConnection(string connectionName = "TestConnection")
    {
        _connectionProviderMock
            .Setup(p => p.IsConnectionAvailable(connectionName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FdwResult<bool>.Success(true));
    }

    // Helper class for testing
    private sealed class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
