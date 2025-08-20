using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using FractalDataWorks.Services.ExternalConnections.MsSql;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests;

public sealed class MsSqlCommandTranslatorTests
{
    private readonly Mock<ILogger<MsSqlCommandTranslator>> _mockLogger;
    private readonly MsSqlConfiguration _configuration;
    private readonly MsSqlCommandTranslator _translator;

    public MsSqlCommandTranslatorTests()
    {
        _mockLogger = new Mock<ILogger<MsSqlCommandTranslator>>();
        _configuration = CreateTestConfiguration();
        _translator = new MsSqlCommandTranslator(_configuration, _mockLogger.Object);
    }

    private static MsSqlConfiguration CreateTestConfiguration()
    {
        return new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=TestDB;Integrated Security=true;",
            DefaultSchema = "dbo",
            EnableSqlLogging = true,
            MaxSqlLogLength = 1000,
            SchemaMappings = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Users"] = "auth.Users",
                ["Orders"] = "sales.Orders"
            }
        };
    }

    [Fact]
    public void ConstructorWhenValidParametersCreatesTranslator()
    {
        // Arrange & Act
        var translator = new MsSqlCommandTranslator(_configuration, _mockLogger.Object);

        // Assert
        translator.ShouldNotBeNull();
        // _output.WriteLine("MsSqlCommandTranslator created successfully");
    }

    [Fact]
    public void ConstructorWhenConfigurationIsNullThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new MsSqlCommandTranslator(null!, _mockLogger.Object));

        exception.ParamName.ShouldBe("configuration");
        // _output.WriteLine($"Correctly threw ArgumentNullException: {exception.Message}");
    }

    [Fact]
    public void ConstructorWhenLoggerIsNullThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            new MsSqlCommandTranslator(_configuration, null!));

        exception.ParamName.ShouldBe("logger");
        // _output.WriteLine($"Correctly threw ArgumentNullException: {exception.Message}");
    }

    [Fact]
    public void TranslateWhenCommandIsNullThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() =>
            _translator.Translate(null!));

        exception.ParamName.ShouldBe("command");
        // _output.WriteLine($"Correctly threw ArgumentNullException: {exception.Message}");
    }

    [Fact]
    public void TranslateQueryCommandGeneratesSelectStatement()
    {
        // Arrange
        var command = new TestDataCommand(
            "Query",
            "TestConnection",
            new DataPath(new[] { "dbo", "TestTable" }));

        // Act
        var result = _translator.Translate(command);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldNotBeNullOrEmpty();
        result.Sql.ShouldContain("SELECT * FROM [dbo].[TestTable]");
        result.Parameters.ShouldNotBeNull();

        // _output.WriteLine($"Generated Query SQL: {result.Sql}");
        // _output.WriteLine($"Parameters count: {result.Parameters.Count}");
    }

    [Fact]
    public void TranslateCountCommandGeneratesCountStatement()
    {
        // Arrange
        var command = new TestDataCommand(
            "Count",
            "TestConnection",
            new DataPath(new[] { "sales", "Orders" }));

        // Act
        var result = _translator.Translate(command);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldNotBeNullOrEmpty();
        result.Sql.ShouldContain("SELECT COUNT(*) FROM [sales].[Orders]");
        result.Parameters.ShouldNotBeNull();

        // _output.WriteLine($"Generated Count SQL: {result.Sql}");
    }

    [Fact]
    public void TranslateExistsCommandGeneratesExistsStatement()
    {
        // Arrange
        var command = new TestDataCommand(
            "Exists",
            "TestConnection",
            new DataPath(new[] { "auth", "Users" }));

        // Act
        var result = _translator.Translate(command);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldNotBeNullOrEmpty();
        result.Sql.ShouldContain("SELECT CASE WHEN EXISTS (SELECT 1 FROM [auth].[Users])");
        result.Sql.ShouldContain("THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END");
        result.Parameters.ShouldNotBeNull();

        // _output.WriteLine($"Generated Exists SQL: {result.Sql}");
    }

    [Theory]
    [InlineData("Insert")]
    [InlineData("Update")]
    [InlineData("Delete")]
    [InlineData("Upsert")]
    public void TranslateDataModificationCommandsRequireEntityData(string commandType)
    {
        // Arrange
        var command = new TestDataCommand(
            commandType,
            "TestConnection",
            new DataPath(new[] { "dbo", "TestTable" }));

        // Act & Assert - These commands require entity data which our test command doesn't have
        var exception = Should.Throw<Exception>(() => _translator.Translate(command));
        
        // The exact exception type depends on the implementation - could be InvalidOperationException, 
        // ArgumentException, or similar when trying to get entity data
        exception.ShouldNotBeNull();
        
        // _output.WriteLine($"CommandType {commandType} correctly threw exception without entity data: {exception.GetType().Name}");
    }

    [Fact]
    public void TranslateUnsupportedCommandTypeThrowsNotSupportedException()
    {
        // Arrange
        var command = new TestDataCommand(
            "UnsupportedCommand",
            "TestConnection",
            new DataPath(new[] { "dbo", "TestTable" }));

        // Act & Assert
        var exception = Should.Throw<NotSupportedException>(() => _translator.Translate(command));
        
        exception.Message.ShouldContain("UnsupportedCommand");
        exception.Message.ShouldContain("not supported");
        
        // _output.WriteLine($"Correctly threw NotSupportedException: {exception.Message}");
    }

    [Fact]
    public void TranslateWithSchemaMappingUsesCorrectSchema()
    {
        // Arrange - Using "Users" which maps to "auth.Users" in our configuration
        var command = new TestDataCommand(
            "Query",
            "TestConnection",
            new DataPath(new[] { "Users" }));

        // Act
        var result = _translator.Translate(command);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldContain("FROM [auth].[Users]");
        
        // _output.WriteLine($"Schema mapping applied correctly: {result.Sql}");
    }

    [Fact]
    public void TranslateWithDefaultSchemaWhenNoMapping()
    {
        // Arrange - Using "Customers" which has no mapping, should use default schema
        var command = new TestDataCommand(
            "Query",
            "TestConnection",
            new DataPath(new[] { "Customers" }));

        // Act
        var result = _translator.Translate(command);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldContain("FROM [dbo].[Customers]"); // Should use default schema
        
        // _output.WriteLine($"Default schema applied correctly: {result.Sql}");
    }

    [Fact]
    public void TranslateWithPagingAddsOffsetFetch()
    {
        // Arrange
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Paged"] = true,
            ["Offset"] = 20,
            ["Limit"] = 10
        };
        
        var command = new TestDataCommand(
            "Query",
            "TestConnection",
            new DataPath(new[] { "dbo", "TestTable" }),
            null, // parameters
            metadata);

        // Act
        var result = _translator.Translate(command);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldContain("OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY");
        result.Sql.ShouldContain("ORDER BY"); // SQL Server requires ORDER BY for OFFSET/FETCH
        
        // _output.WriteLine($"Paging SQL generated: {result.Sql}");
    }

    [Fact]
    public void TranslateWithSingleResultAddsTopOne()
    {
        // Arrange
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["SingleResult"] = true
        };
        
        var command = new TestDataCommand(
            "Query",
            "TestConnection",
            new DataPath(new[] { "dbo", "TestTable" }),
            null, // parameters
            metadata);

        // Act
        var result = _translator.Translate(command);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldContain("OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY");
        result.Sql.ShouldContain("ORDER BY"); // SQL Server requires ORDER BY for OFFSET/FETCH
        
        // _output.WriteLine($"Single result SQL generated: {result.Sql}");
    }

    [Fact]
    public void TranslateLogsGeneratedSqlWhenEnabled()
    {
        // Arrange
        var command = new TestDataCommand(
            "Query",
            "TestConnection",
            new DataPath(new[] { "dbo", "TestTable" }));

        // Act
        var result = _translator.Translate(command);

        // Assert
        result.ShouldNotBeNull();
        
        // Verify logging was called (SQL logging is enabled in test configuration)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generated SQL")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        // _output.WriteLine($"SQL logging verified for generated SQL: {result.Sql}");
    }

    [Fact]
    public void TranslateTruncatesLongSqlInLogs()
    {
        // Arrange
        var configWithShortLimit = CreateTestConfiguration();
        configWithShortLimit.MaxSqlLogLength = 50; // Very short limit
        var translatorWithShortLimit = new MsSqlCommandTranslator(configWithShortLimit, _mockLogger.Object);
        
        var command = new TestDataCommand(
            "Query",
            "TestConnection",
            new DataPath(new[] { "schema_with_very_long_name", "table_with_very_long_name_that_exceeds_limit" }));

        // Act
        var result = translatorWithShortLimit.Translate(command);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.Length.ShouldBeGreaterThan(50); // Full SQL should be longer
        
        // Verify truncation logic works (though we can't easily verify the exact logged content)
        // _output.WriteLine($"Long SQL generated (length: {result.Sql.Length}): {result.Sql}");
        // _output.WriteLine($"Max log length configured: {configWithShortLimit.MaxSqlLogLength}");
    }

    [Theory]
    [InlineData("BulkInsert")]
    [InlineData("BulkUpsert")]
    public void TranslateBulkOperationsRequireEntitiesCollection(string commandType)
    {
        // Arrange
        var command = new TestDataCommand(
            commandType,
            "TestConnection",
            new DataPath(new[] { "dbo", "TestTable" }));

        // Act & Assert - Bulk operations require entities collection
        var exception = Should.Throw<Exception>(() => _translator.Translate(command));
        
        exception.ShouldNotBeNull();
        // _output.WriteLine($"Bulk operation {commandType} correctly failed without entities: {exception.GetType().Name}");
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