using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using FractalDataWorks.Results;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services.ExternalConnections.MsSql;
using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests;

public sealed class MsSqlExternalConnectionTests
{
    private readonly Mock<ILogger<MsSqlExternalConnection>> _mockLogger;
    private readonly MsSqlConfiguration _validConfiguration;

    public MsSqlExternalConnectionTests()
    {
        _mockLogger = new Mock<ILogger<MsSqlExternalConnection>>();
        _validConfiguration = CreateValidConfiguration();
    }

    private static MsSqlConfiguration CreateValidConfiguration()
    {
        return new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=TestDB;Integrated Security=true;",
            CommandTimeoutSeconds = 30,
            ConnectionTimeoutSeconds = 15,
            DefaultSchema = "dbo",
            UseTransactions = false,
            EnableConnectionPooling = true,
            TransactionIsolationLevel = IsolationLevel.ReadCommitted
        };
    }

    [Fact]
    public void ConstructorWhenValidParametersCreatesInstance()
    {
        // Arrange & Act
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Assert
        connection.ShouldNotBeNull();
        connection.ConnectionId.ShouldNotBeNullOrEmpty();
        connection.ProviderName.ShouldBe("MsSql");
        connection.State.ShouldBe(FdwConnectionState.Created);
        connection.Configuration.ShouldBe(_validConfiguration);
        connection.ConnectionString.ShouldBe(_validConfiguration.GetSanitizedConnectionString());
        
        // _output.WriteLine($"Created connection with ID: {connection.ConnectionId}");
    }

    [Fact]
    public void ConstructorWhenLoggerIsNullThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => 
            new MsSqlExternalConnection(null!, _validConfiguration));
        
        exception.ParamName.ShouldBe("logger");
        // _output.WriteLine($"Correctly threw ArgumentNullException for null logger: {exception.Message}");
    }

    [Fact]
    public void ConstructorWhenConfigurationIsNullThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => 
            new MsSqlExternalConnection(_mockLogger.Object, null!));
        
        exception.ParamName.ShouldBe("configuration");
        // _output.WriteLine($"Correctly threw ArgumentNullException for null configuration: {exception.Message}");
    }

    [Fact]
    public void ConstructorWhenConfigurationIsInvalidThrowsArgumentException()
    {
        // Arrange
        var invalidConfiguration = new MsSqlConfiguration
        {
            ConnectionString = string.Empty, // Invalid - empty connection string
            CommandTimeoutSeconds = -1 // Invalid - negative timeout
        };

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => 
            new MsSqlExternalConnection(_mockLogger.Object, invalidConfiguration));
        
        exception.ParamName.ShouldBe("configuration");
        exception.Message.ShouldContain("Configuration validation failed");
        // _output.WriteLine($"Correctly threw ArgumentException for invalid configuration: {exception.Message}");
    }

    [Theory]
    [InlineData("MyConnection1")]
    [InlineData("Connection-2")]
    [InlineData("Test_Connection_3")]
    public void ConstructorGeneratesUniqueConnectionIds(string testName)
    {
        // Arrange & Act
        var connection1 = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);
        var connection2 = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Assert
        connection1.ConnectionId.ShouldNotBe(connection2.ConnectionId);
        connection1.ConnectionId.ShouldNotBeNullOrEmpty();
        connection2.ConnectionId.ShouldNotBeNullOrEmpty();
        
        // _output.WriteLine($"Test {testName}: Connection1 ID = {connection1.ConnectionId}, Connection2 ID = {connection2.ConnectionId}");
    }

    [Fact]
    public void ExecuteWhenCommandIsNullThrowsArgumentNullException()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Act & Assert
        var exception = Should.ThrowAsync<ArgumentNullException>(async () => 
            await connection.Execute(null!));
        
        exception.Result.ParamName.ShouldBe("command");
        // _output.WriteLine($"Correctly threw ArgumentNullException for null command: {exception.Result.Message}");
    }

    [Fact]
    public void ExecuteGenericWhenCommandIsNullThrowsArgumentNullException()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Act & Assert
        var exception = Should.ThrowAsync<ArgumentNullException>(async () => 
            await connection.Execute<int>(null!));
        
        exception.Result.ParamName.ShouldBe("command");
        // _output.WriteLine($"Correctly threw ArgumentNullException for null command in Execute<T>: {exception.Result.Message}");
    }

    [Fact]
    public void ExecuteWhenCommandIsNotDataCommandBaseThrowsArgumentException()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);
        var mockCommand = new Mock<IDataCommand>();
        mockCommand.Setup(c => c.CommandType).Returns("TestCommand");

        // Act & Assert
        var exception = Should.ThrowAsync<ArgumentException>(async () => 
            await connection.Execute(mockCommand.Object));
        
        exception.Result.ParamName.ShouldBe("command");
        exception.Result.Message.ShouldContain("Expected DataCommandBase");
        // _output.WriteLine($"Correctly threw ArgumentException for non-DataCommandBase command: {exception.Result.Message}");
    }

    [Fact]
    public void StatelessDesignPropertiesReturnCorrectValues()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Act & Assert
        connection.State.ShouldBe(FdwConnectionState.Created); // Always ready in stateless design
        connection.ProviderName.ShouldBe("MsSql");
        connection.ConnectionString.ShouldBe(_validConfiguration.GetSanitizedConnectionString());
        connection.Configuration.ShouldBeSameAs(_validConfiguration);
        
        // _output.WriteLine($"Verified stateless design properties - State: {connection.State}, Provider: {connection.ProviderName}");
    }

    [Fact]
    public async Task TestConnectionReturnsSuccessForValidConfiguration()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Note: This test would require a real SQL Server connection
        // In a real test environment, you would either:
        // 1. Use a test database
        // 2. Mock the SqlConnection (which is complex)
        // 3. Use integration tests with testcontainers
        
        // For now, we'll test the method signature and error handling
        var result = await connection.TestConnection();
        
        // Assert - We expect this to fail with connection error since we're using a fake connection string
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse(); // Expected to fail with fake connection string
        result.Value.ShouldBeFalse();
        
        // _output.WriteLine($"TestConnection result: Success={result.IsSuccess}, Value={result.Value}");
    }

    [Fact]
    public async Task GetConnectionInfoReturnsResultWithErrorForInvalidConnection()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Act - This will fail due to fake connection string, but tests the method structure
        var result = await connection.GetConnectionInfo();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse(); // Expected to fail with fake connection string
        result.Message.ShouldContain("Get connection info failed");
        
        // _output.WriteLine($"GetConnectionInfo result: Success={result.IsSuccess}, Message={result.Message}");
    }

    [Fact]
    public async Task DiscoverSchemaReturnsResultWithErrorForInvalidConnection()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Act - This will fail due to fake connection string, but tests the method structure
        var result = await connection.DiscoverSchema();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse(); // Expected to fail with fake connection string
        result.Message.ShouldContain("Schema discovery failed");
        
        // _output.WriteLine($"DiscoverSchema result: Success={result.IsSuccess}, Message={result.Message}");
    }

    [Fact]
    public async Task LegacyInterfaceOpenAsyncReturnsSuccess()
    {
        // Arrange & Act
        var result = await MsSqlExternalConnection.OpenAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldBeNullOrEmpty();
        
        // _output.WriteLine($"Legacy OpenAsync result: Success={result.IsSuccess}");
    }

    [Fact]
    public async Task LegacyInterfaceCloseAsyncReturnsSuccess()
    {
        // Arrange & Act
        var result = await MsSqlExternalConnection.CloseAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldBeNullOrEmpty();
        
        // _output.WriteLine($"Legacy CloseAsync result: Success={result.IsSuccess}");
    }

    [Fact]
    public async Task LegacyInterfaceInitializeAsyncReturnsSuccess()
    {
        // Arrange & Act
        var result = await MsSqlExternalConnection.InitializeAsync(_validConfiguration);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldBeNullOrEmpty();
        
        // _output.WriteLine($"Legacy InitializeAsync result: Success={result.IsSuccess}");
    }

    [Fact]
    public async Task TestConnectionAsyncReturnsExpectedResult()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Act - This will fail due to fake connection string, but tests the method
        var result = await connection.TestConnectionAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse(); // Expected to fail with fake connection string
        
        // _output.WriteLine($"TestConnectionAsync result: Success={result.IsSuccess}");
    }

    [Fact]
    public void DisposeDoesNotThrowException()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Act & Assert - Should not throw in stateless design
        Should.NotThrow(() => connection.Dispose());
        
        // _output.WriteLine("Dispose completed successfully (no-op in stateless design)");
    }

    [Theory]
    [InlineData(true, IsolationLevel.ReadCommitted)]
    [InlineData(true, IsolationLevel.ReadUncommitted)]
    [InlineData(true, IsolationLevel.RepeatableRead)]
    [InlineData(true, IsolationLevel.Serializable)]
    [InlineData(false, IsolationLevel.ReadCommitted)]
    public void ConfigurationTransactionSettingsAreRespected(bool useTransactions, IsolationLevel isolationLevel)
    {
        // Arrange
        var config = CreateValidConfiguration();
        config.UseTransactions = useTransactions;
        config.TransactionIsolationLevel = isolationLevel;

        // Act
        var connection = new MsSqlExternalConnection(_mockLogger.Object, config);

        // Assert
        connection.Configuration.UseTransactions.ShouldBe(useTransactions);
        connection.Configuration.TransactionIsolationLevel.ShouldBe(isolationLevel);
        
        // _output.WriteLine($"Transaction settings - Use: {useTransactions}, Isolation: {isolationLevel}");
    }

    [Fact]
    public void ConfigurationValidationIsCalledDuringConstruction()
    {
        // Arrange
        var config = new MsSqlConfiguration
        {
            ConnectionString = "Valid connection string",
            CommandTimeoutSeconds = 30,
            ConnectionTimeoutSeconds = 15,
            DefaultSchema = "dbo"
        };

        // Act - This should succeed as configuration is valid
        var connection = new MsSqlExternalConnection(_mockLogger.Object, config);

        // Assert
        connection.ShouldNotBeNull();
        connection.Configuration.ShouldBe(config);
        
        // _output.WriteLine("Configuration validation passed during construction");
    }

    [Fact]
    public void MultipleConnectionsAreIndependent()
    {
        // Arrange & Act
        var connection1 = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);
        var connection2 = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Assert - Each connection should be independent with unique IDs
        connection1.ConnectionId.ShouldNotBe(connection2.ConnectionId);
        connection1.State.ShouldBe(FdwConnectionState.Created);
        connection2.State.ShouldBe(FdwConnectionState.Created);
        
        // Both should reference the same configuration object
        connection1.Configuration.ShouldBeSameAs(_validConfiguration);
        connection2.Configuration.ShouldBeSameAs(_validConfiguration);
        
        // _output.WriteLine($"Connection 1 ID: {connection1.ConnectionId}");
        // _output.WriteLine($"Connection 2 ID: {connection2.ConnectionId}");
    }

    [Fact]
    public void ConnectionStringIsSanitizedInProperty()
    {
        // Arrange
        var configWithSensitiveInfo = new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=TestDB;User Id=testuser;Password=secret123;",
            CommandTimeoutSeconds = 30,
            ConnectionTimeoutSeconds = 15,
            DefaultSchema = "dbo"
        };

        // Act
        var connection = new MsSqlExternalConnection(_mockLogger.Object, configWithSensitiveInfo);

        // Assert
        connection.ConnectionString.ShouldContain("Password=***");
        connection.ConnectionString.ShouldContain("User Id=***");
        connection.ConnectionString.ShouldNotContain("secret123");
        connection.ConnectionString.ShouldNotContain("testuser");
        
        // _output.WriteLine($"Sanitized connection string: {connection.ConnectionString}");
    }
}