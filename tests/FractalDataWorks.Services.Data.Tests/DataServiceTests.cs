using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Connections;
using FractalDataWorks.Connections.Data.Commands;
using FractalDataWorks.Data;
using FractalDataWorks.Entities;
using FractalDataWorks.Services.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Data.Tests;

public class DataServiceTests
{
    private readonly DataConfiguration _configuration;
    private readonly IDataServiceProvider _provider;
    private readonly ILogger<DataService> _logger;
    private readonly DataService _sut;
    
    public DataServiceTests()
    {
        _configuration = new DataConfiguration
        {
            DefaultConnectionId = "default-connection",
            LogCommandDetails = true,
            DefaultTimeoutSeconds = 30
        };
        
        _provider = Substitute.For<IDataServiceProvider>();
        _logger = Substitute.For<ILogger<DataService>>();
        
        _sut = new DataService(_configuration, _provider, _logger);
    }
    
    [Fact]
    public void ConstructorShouldThrowWhenProviderIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new DataService(_configuration, null!, _logger))
            .ParamName.ShouldBe("provider");
    }
    
    [Fact]
    public async Task ExecuteWithConnectionIdShouldUseSpecifiedConnection()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("TestProvider", "TestDB", "TestTable")
            .Build();
        
        var mockConnection = Substitute.For<IDataConnection>();
        mockConnection.Execute<TestEntity[]>(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(GenericResult<TestEntity[]>.Success(Array.Empty<TestEntity>())));
        
        _provider.GetConnectionById("specific-connection")
            .Returns(GenericResult<IDataConnection>.Success(mockConnection));
        
        // Act
        var result = await _sut.Execute<TestEntity[]>("specific-connection", command);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        _provider.Received(1).GetConnectionById("specific-connection");
        await mockConnection.Received(1).Execute<TestEntity[]>(command, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task ExecuteWithEmptyConnectionIdShouldUseDefaultConnection()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("TestProvider", "TestDB", "TestTable")
            .Build();
        
        var mockConnection = Substitute.For<IDataConnection>();
        mockConnection.Execute<TestEntity[]>(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(GenericResult<TestEntity[]>.Success(Array.Empty<TestEntity>())));
        
        _provider.GetConnectionById("default-connection")
            .Returns(GenericResult<IDataConnection>.Success(mockConnection));
        
        // Act
        var result = await _sut.Execute<TestEntity[]>("", command);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        _provider.Received(1).GetConnectionById("default-connection");
    }
    
    [Fact]
    public async Task ExecuteWithoutConnectionIdShouldUseDefaultConnection()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("TestProvider", "TestDB", "TestTable")
            .Build();
        
        var mockConnection = Substitute.For<IDataConnection>();
        mockConnection.Execute<TestEntity[]>(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(GenericResult<TestEntity[]>.Success(Array.Empty<TestEntity>())));
        
        _provider.GetConnectionById("default-connection")
            .Returns(GenericResult<IDataConnection>.Success(mockConnection));
        
        // Act
        var result = await _sut.Execute<TestEntity[]>(command);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        _provider.Received(1).GetConnectionById("default-connection");
    }
    
    [Fact]
    public async Task ExecuteShouldReturnFailureWhenNoConnectionIdAndNoDefault()
    {
        // Arrange
        var service = new DataService(
            new DataConfiguration { DefaultConnectionId = null }, 
            _provider, 
            _logger);
        
        var command = QueryCommand<TestEntity>.Create()
            .From("TestProvider", "TestDB", "TestTable")
            .Build();
        
        // Act
        var result = await service.Execute<TestEntity[]>("", command);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldContain("No connection ID specified and no default configured");
    }
    
    [Fact]
    public async Task ExecuteShouldReturnFailureWhenConnectionNotFound()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("TestProvider", "TestDB", "TestTable")
            .Build();
        
        _provider.GetConnectionById("test-connection")
            .Returns(GenericResult<IDataConnection>.Failure("Connection not found"));
        
        // Act
        var result = await _sut.Execute<TestEntity[]>("test-connection", command);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldContain("Connection not found");
    }
    
    [Fact]
    public async Task ExecuteShouldReturnFailureWhenCommandExecutionFails()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("TestProvider", "TestDB", "TestTable")
            .Build();
        
        var mockConnection = Substitute.For<IDataConnection>();
        mockConnection.Execute<TestEntity[]>(command, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(GenericResult<TestEntity[]>.Failure("Query failed")));
        
        _provider.GetConnectionById("test-connection")
            .Returns(GenericResult<IDataConnection>.Success(mockConnection));
        
        // Act
        var result = await _sut.Execute<TestEntity[]>("test-connection", command);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldContain("Query failed");
    }
    
    [Fact]
    public async Task ExecuteShouldHandleExceptions()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("TestProvider", "TestDB", "TestTable")
            .Build();
        
        var mockConnection = Substitute.For<IDataConnection>();
        mockConnection.Execute<TestEntity[]>(command, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));
        
        _provider.GetConnectionById("test-connection")
            .Returns(GenericResult<IDataConnection>.Success(mockConnection));
        
        // Act
        var result = await _sut.Execute<TestEntity[]>("test-connection", command);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldContain("Execution error: Unexpected error");
    }
    
    [Fact]
    public async Task ExecuteShouldPassCancellationToken()
    {
        // Arrange
        var command = QueryCommand<TestEntity>.Create()
            .From("TestProvider", "TestDB", "TestTable")
            .Build();
        
        var cts = new CancellationTokenSource();
        var mockConnection = Substitute.For<IDataConnection>();
        mockConnection.Execute<TestEntity[]>(command, cts.Token)
            .Returns(Task.FromResult(GenericResult<TestEntity[]>.Success(Array.Empty<TestEntity>())));
        
        _provider.GetConnectionById("test-connection")
            .Returns(GenericResult<IDataConnection>.Success(mockConnection));
        
        // Act
        await _sut.Execute<TestEntity[]>("test-connection", command, cts.Token);
        
        // Assert
        await mockConnection.Received(1).Execute<TestEntity[]>(command, cts.Token);
    }
    
    [Fact]
    public async Task TestConnectionShouldDelegateToProvider()
    {
        // Arrange
        _provider.TestConnection("test-connection", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(GenericResult<bool>.Success(true)));
        
        // Act
        var result = await _sut.TestConnection("test-connection");
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
        await _provider.Received(1).TestConnection("test-connection", Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task TestConnectionShouldConvertGenericResult()
    {
        // Arrange
        _provider.TestConnection("test-connection", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(GenericResult<bool>.Failure("Connection failed")));
        
        // Act
        var result = await _sut.TestConnection("test-connection");
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldContain("Connection failed");
    }
    
    [Fact]
    public void GetAvailableConnectionsShouldDelegateToProvider()
    {
        // Arrange
        var connections = new[] { "conn1", "conn2", "conn3" };
        _provider.GetAvailableConnections()
            .Returns(GenericResult<string[]>.Success(connections));
        
        // Act
        var result = _sut.GetAvailableConnections();
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(connections);
        _provider.Received(1).GetAvailableConnections();
    }
    
    [Fact]
    public void GetAvailableConnectionsShouldConvertGenericResult()
    {
        // Arrange
        _provider.GetAvailableConnections()
            .Returns(GenericResult<string[]>.Failure("Provider error"));
        
        // Act
        var result = _sut.GetAvailableConnections();
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldContain("Provider error");
    }
    
    [Fact]
    public async Task ExecuteShouldWorkWithDifferentCommandTypes()
    {
        // Arrange
        var insertCommand = InsertCommand<TestEntity>.Create()
            .Into("TestProvider", "TestDB", "TestTable")
            .WithEntity(new TestEntity { Id = 1, Name = "Test" })
            .Build();
        
        var mockConnection = Substitute.For<IDataConnection>();
        mockConnection.Execute<TestEntity>(insertCommand, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(GenericResult<TestEntity>.Success(new TestEntity { Id = 1, Name = "Test" })));
        
        _provider.GetConnectionById("test-connection")
            .Returns(GenericResult<IDataConnection>.Success(mockConnection));
        
        // Act
        var result = await _sut.Execute<TestEntity>("test-connection", insertCommand);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(1);
        result.Value.Name.ShouldBe("Test");
    }
    
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}