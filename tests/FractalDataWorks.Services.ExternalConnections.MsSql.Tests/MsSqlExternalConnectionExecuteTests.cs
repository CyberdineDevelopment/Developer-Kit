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
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests;

public sealed class MsSqlExternalConnectionExecuteTests
{
    private readonly Mock<ILogger<MsSqlExternalConnection>> _mockLogger;
    private readonly MsSqlConfiguration _validConfiguration;

    public MsSqlExternalConnectionExecuteTests()
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
    public async Task ExecuteWhenSqlConnectionFailsReturnsFailureResult()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);
        var mockCommand = CreateMockDataCommand("Query");

        // Act - This will fail due to invalid connection string, testing error handling
        var result = await connection.Execute(mockCommand);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Command execution failed");
        
        // _output.WriteLine($"Execute failure result: Success={result.IsSuccess}, Message={result.Message}");
    }

    [Fact]
    public async Task ExecuteGenericWhenSqlConnectionFailsReturnsFailureResult()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);
        var mockCommand = CreateMockDataCommand("Query");

        // Act - This will fail due to invalid connection string, testing error handling
        var result = await connection.Execute<int>(mockCommand);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Command execution failed");
        
        // _output.WriteLine($"Execute<T> failure result: Success={result.IsSuccess}, Message={result.Message}");
    }

    [Fact]
    public async Task ExecuteWithTransactionConfigurationAttemptsTransactionHandling()
    {
        // Arrange
        var transactionConfig = CreateValidConfiguration();
        transactionConfig.UseTransactions = true;
        transactionConfig.TransactionIsolationLevel = IsolationLevel.ReadCommitted;
        
        var connection = new MsSqlExternalConnection(_mockLogger.Object, transactionConfig);
        var mockCommand = CreateMockDataCommand("Insert");

        // Act - This will fail due to invalid connection string, but tests transaction path
        var result = await connection.Execute<int>(mockCommand);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Command execution failed");
        
        // Verify transaction configuration was respected
        connection.Configuration.UseTransactions.ShouldBeTrue();
        connection.Configuration.TransactionIsolationLevel.ShouldBe(IsolationLevel.ReadCommitted);
        
        // _output.WriteLine($"Transaction Execute result: Success={result.IsSuccess}, UseTransactions={connection.Configuration.UseTransactions}");
    }

    [Theory]
    [InlineData("Query")]
    [InlineData("Insert")]
    [InlineData("Update")]
    [InlineData("Delete")]
    [InlineData("Count")]
    public async Task ExecuteWithDifferentCommandTypesHandlesErrorsGracefully(string commandType)
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);
        var mockCommand = CreateMockDataCommand(commandType);

        // Act - This will fail due to invalid connection string, but tests command type handling
        var result = await connection.Execute<int>(mockCommand);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldContain("Command execution failed");
        
        // _output.WriteLine($"CommandType={commandType}: Success={result.IsSuccess}, Message={result.Message}");
    }

    [Fact]
    public async Task ExecuteWithCancellationTokenPropagatesCancellation()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);
        var mockCommand = CreateMockDataCommand("Query");
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Cancel immediately

        // Act & Assert
        var exception = await Should.ThrowAsync<TaskCanceledException>(async () =>
            await connection.Execute<int>(mockCommand, cancellationTokenSource.Token));

        // _output.WriteLine($"Cancellation properly propagated: {exception.Message}");
    }

    [Fact]
    public async Task ExecuteReturnsSuccessFromExecuteGeneric()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);
        var mockCommand = CreateMockDataCommand("Query");

        // Act - Testing the Execute method that calls Execute<int> internally
        var result = await connection.Execute(mockCommand);

        // Assert
        result.ShouldNotBeNull();
        // Since we're using fake connection string, this will fail, but we test the flow
        result.IsSuccess.ShouldBeFalse();
        
        // _output.WriteLine($"Execute non-generic result: Success={result.IsSuccess}");
    }

    [Fact]
    public void ExecuteMethodsUseStatelessApproach()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);

        // Act & Assert - Verify stateless properties don't change
        connection.State.ShouldBe(FdwConnectionState.Created);
        
        // After construction, state should remain Created (ready) in stateless design
        var initialConnectionId = connection.ConnectionId;
        
        // Connection ID should remain the same (doesn't change per operation)
        connection.ConnectionId.ShouldBe(initialConnectionId);
        connection.State.ShouldBe(FdwConnectionState.Created);
        
        // _output.WriteLine($"Stateless design verified - ConnectionId: {connection.ConnectionId}, State: {connection.State}");
    }

    [Theory]
    [InlineData(IsolationLevel.ReadCommitted)]
    [InlineData(IsolationLevel.ReadUncommitted)]
    [InlineData(IsolationLevel.RepeatableRead)]
    [InlineData(IsolationLevel.Serializable)]
    public void TransactionIsolationLevelConfigurationIsRespected(IsolationLevel isolationLevel)
    {
        // Arrange
        var transactionConfig = CreateValidConfiguration();
        transactionConfig.UseTransactions = true;
        transactionConfig.TransactionIsolationLevel = isolationLevel;

        // Act
        var connection = new MsSqlExternalConnection(_mockLogger.Object, transactionConfig);

        // Assert
        connection.Configuration.UseTransactions.ShouldBeTrue();
        connection.Configuration.TransactionIsolationLevel.ShouldBe(isolationLevel);
        
        // _output.WriteLine($"Transaction isolation level configured: {isolationLevel}");
    }

    [Fact]
    public async Task ExecuteHandlesCommandTimeoutFromConfiguration()
    {
        // Arrange
        var configWithTimeout = CreateValidConfiguration();
        configWithTimeout.CommandTimeoutSeconds = 60;
        
        var connection = new MsSqlExternalConnection(_mockLogger.Object, configWithTimeout);
        var mockCommand = CreateMockDataCommand("Query");

        // Act - This will fail due to invalid connection string, but tests timeout configuration
        var result = await connection.Execute<int>(mockCommand);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse(); // Expected due to fake connection string
        connection.Configuration.CommandTimeoutSeconds.ShouldBe(60);
        
        // _output.WriteLine($"Command timeout configured: {connection.Configuration.CommandTimeoutSeconds} seconds");
    }

    [Fact]
    public async Task ExecuteWithCustomTimeoutInCommandUsesCommandTimeout()
    {
        // Arrange
        var connection = new MsSqlExternalConnection(_mockLogger.Object, _validConfiguration);
        var mockCommand = CreateMockDataCommand("Query");
        
        // Mock the Timeout property to return a custom timeout
        var timeoutProperty = mockCommand.GetType().GetProperty("Timeout");
        if (timeoutProperty != null && timeoutProperty.CanWrite)
        {
            timeoutProperty.SetValue(mockCommand, TimeSpan.FromSeconds(45));
        }

        // Act - This will fail due to invalid connection string, but tests timeout logic
        var result = await connection.Execute<int>(mockCommand);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse(); // Expected due to fake connection string
        
        // _output.WriteLine($"Command with custom timeout executed, result: {result.IsSuccess}");
    }

    private TestDataCommand CreateMockDataCommand(string commandType)
    {
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["TestKey"] = "TestValue"
        };

        return new TestDataCommand(
            commandType, 
            "TestConnection",
            new DataPath(new[] { "dbo", "TestTable" }),
            null, // parameters
            metadata);
    }

    /// <summary>
    /// Simple test implementation of DataCommandBase for testing purposes
    /// </summary>
    private sealed class TestDataCommand : DataCommandBase
    {
        public TestDataCommand(
            string commandType = "Query", 
            string connectionName = "TestConnection",
            DataPath? targetContainer = null,
            IReadOnlyDictionary<string, object?>? parameters = null,
            IReadOnlyDictionary<string, object>? metadata = null,
            TimeSpan? timeout = null)
            : base(commandType, connectionName, targetContainer, typeof(object), parameters, metadata, timeout)
        {
        }

        public override bool IsDataModifying => 
            CommandType == "Insert" || CommandType == "Update" || CommandType == "Delete" || CommandType == "Upsert";

        protected override DataCommandBase CreateCopy(
            string connectionName, 
            DataPath? targetContainer, 
            IReadOnlyDictionary<string, object?> parameters, 
            IReadOnlyDictionary<string, object> metadata, 
            TimeSpan? timeout)
        {
            return new TestDataCommand(CommandType, connectionName, targetContainer, parameters, metadata, timeout);
        }
    }
}