using System;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using FractalDataWorks.Services.DataProvider.EnhancedEnums;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.DataProvider.Tests.EnhancedEnums;

/// <summary>
/// Tests for the DataCommandExtensions fluent syntax methods.
/// </summary>
public sealed class DataCommandExtensionsTests
{
    public DataCommandExtensionsTests()
    {
    }

    [Fact]
    public void WithConnectionShouldSetConnectionNameOnCommand()
    {
        // Arrange
        var originalCommand = DataCommands.QueryAll<Customer>();
        const string connectionName = "ProductionDB";

        // Act
        var updatedCommand = originalCommand.WithConnection(connectionName);

        // Assert
        updatedCommand.ShouldNotBeNull();
        updatedCommand.ConnectionName.ShouldBe(connectionName);
        updatedCommand.ShouldBeSameAs(originalCommand); // Should return the same instance

        
    }

    [Fact]
    public void WithConnectionShouldWorkWithDifferentCommandTypes()
    {
        // Arrange
        var queryCommand = DataCommands.Query<Customer>(c => c.IsActive);
        var insertCommand = DataCommands.Insert(new Customer { Id = 1, Name = "Test" });
        var deleteCommand = DataCommands.Delete<Customer>(c => !c.IsActive);
        const string connectionName = "TestDB";

        // Act
        var updatedQuery = queryCommand.WithConnection(connectionName);
        var updatedInsert = insertCommand.WithConnection(connectionName);
        var updatedDelete = deleteCommand.WithConnection(connectionName);

        // Assert
        updatedQuery.ConnectionName.ShouldBe(connectionName);
        updatedInsert.ConnectionName.ShouldBe(connectionName);
        updatedDelete.ConnectionName.ShouldBe(connectionName);

        
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void WithConnectionShouldAcceptEmptyOrNullConnectionNames(string? connectionName)
    {
        // Arrange
        var command = DataCommands.QueryAll<Customer>();

        // Act
        var updatedCommand = command.WithConnection(connectionName ?? string.Empty);

        // Assert
        updatedCommand.ConnectionName.ShouldBe(connectionName ?? string.Empty);

        
    }

    [Fact]
    public void WithTargetShouldSetTargetContainerOnCommand()
    {
        // Arrange
        var command = DataCommands.QueryAll<Customer>();
        var targetPath = DataPath.Create(".", "sales", "customers");

        // Act
        var updatedCommand = command.WithTarget(targetPath);

        // Assert
        updatedCommand.ShouldNotBeNull();
        updatedCommand.Target.ShouldBe(targetPath);
        updatedCommand.ShouldBeSameAs(command); // Should return the same instance

        
    }

    [Fact]
    public void WithTargetShouldWorkWithDifferentCommandTypes()
    {
        // Arrange
        var queryCommand = DataCommands.Query<Customer>(c => c.IsActive);
        var countCommand = DataCommands.Count<Customer>(c => c.IsActive);
        var updateCommand = DataCommands.Update(new Customer(), c => c.Id == 1);
        var targetPath = DataPath.Create(".", "database", "tables", "customers");

        // Act
        var updatedQuery = queryCommand.WithTarget(targetPath);
        var updatedCount = countCommand.WithTarget(targetPath);
        var updatedUpdate = updateCommand.WithTarget(targetPath);

        // Assert
        updatedQuery.Target.ShouldBe(targetPath);
        updatedCount.Target.ShouldBe(targetPath);
        updatedUpdate.Target.ShouldBe(targetPath);

        
    }

    [Fact]
    public void WithTargetShouldAcceptNullTarget()
    {
        // Arrange
        var command = DataCommands.QueryAll<Customer>();

        // Act
        var updatedCommand = command.WithTarget(null!);

        // Assert
        updatedCommand.Target.ShouldBeNull();

        
    }

    [Fact]
    public void WithTimeoutShouldSetTimeoutOnCommand()
    {
        // Arrange
        var command = DataCommands.QueryAll<Customer>();
        var timeout = TimeSpan.FromMinutes(5);

        // Act
        var updatedCommand = command.WithTimeout(timeout);

        // Assert
        updatedCommand.ShouldNotBeNull();
        updatedCommand.Timeout.ShouldBe(timeout);
        updatedCommand.ShouldBeSameAs(command); // Should return the same instance

        
    }

    [Fact]
    public void WithTimeoutShouldWorkWithDifferentCommandTypes()
    {
        // Arrange
        var existsCommand = DataCommands.Exists<Customer>(c => c.IsActive);
        var insertCommand = DataCommands.Insert(new Customer());
        var truncateCommand = DataCommands.Truncate<Customer>();
        var timeout = TimeSpan.FromSeconds(30);

        // Act
        var updatedExists = existsCommand.WithTimeout(timeout);
        var updatedInsert = insertCommand.WithTimeout(timeout);
        var updatedTruncate = truncateCommand.WithTimeout(timeout);

        // Assert
        updatedExists.Timeout.ShouldBe(timeout);
        updatedInsert.Timeout.ShouldBe(timeout);
        updatedTruncate.Timeout.ShouldBe(timeout);

        
    }

    [Theory]
    [InlineData(0)] // Zero timeout
    [InlineData(-1)] // Negative timeout (should be accepted - might represent infinite timeout)
    [InlineData(1)] // 1 millisecond
    [InlineData(60000)] // 1 minute in milliseconds
    public void WithTimeoutShouldAcceptVariousTimeoutValues(int milliseconds)
    {
        // Arrange
        var command = DataCommands.QueryAll<Customer>();
        var timeout = TimeSpan.FromMilliseconds(milliseconds);

        // Act
        var updatedCommand = command.WithTimeout(timeout);

        // Assert
        updatedCommand.Timeout.ShouldBe(timeout);

        
    }

    [Fact]
    public void FluentSyntaxShouldAllowMethodChaining()
    {
        // Arrange
        var targetPath = DataPath.Create(".", "sales", "customers");
        var timeout = TimeSpan.FromMinutes(2);
        const string connectionName = "SalesDB";

        // Act
        var command = DataCommands.Query<Customer>(c => c.IsActive)
            .WithConnection(connectionName)
            .WithTarget(targetPath)
            .WithTimeout(timeout);

        // Assert
        command.ShouldNotBeNull();
        command.ConnectionName.ShouldBe(connectionName);
        command.Target.ShouldBe(targetPath);
        command.Timeout.ShouldBe(timeout);
        command.Predicate.ShouldNotBeNull();

        _output.WriteLine($"Fluent chaining successful: Connection={command.ConnectionName}, " +
                         $"Target={command.Target}, Timeout={command.Timeout}");
    }

    [Fact]
    public void FluentSyntaxShouldWorkInDifferentOrders()
    {
        // Arrange
        var targetPath = DataPath.Create(".", "inventory", "products");
        var timeout = TimeSpan.FromSeconds(45);
        const string connectionName = "InventoryDB";

        // Act - Different order than previous test
        var command = DataCommands.Insert(new Customer { Id = 100, Name = "Test Customer" })
            .WithTimeout(timeout)
            .WithTarget(targetPath)
            .WithConnection(connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ConnectionName.ShouldBe(connectionName);
        command.Target.ShouldBe(targetPath);
        command.Timeout.ShouldBe(timeout);

        
    }

    [Fact]
    public void WithConnectionShouldReturnCorrectCommandType()
    {
        // Arrange
        var bulkInsertCommand = DataCommands.BulkInsert(new[] { new Customer() });

        // Act
        var result = bulkInsertCommand.WithConnection("TestDB");

        // Assert
        result.ShouldBeOfType<BulkInsertCommand<Customer>>();
        result.ShouldBeSameAs(bulkInsertCommand);

        
    }

    [Fact]
    public void WithTargetShouldReturnCorrectCommandType()
    {
        // Arrange
        var upsertCommand = DataCommands.Upsert(new Customer(), new[] { "Id" });
        var targetPath = DataPath.Create(".", "customers");

        // Act
        var result = upsertCommand.WithTarget(targetPath);

        // Assert
        result.ShouldBeOfType<UpsertCommand<Customer>>();
        result.ShouldBeSameAs(upsertCommand);

        
    }

    [Fact]
    public void WithTimeoutShouldReturnCorrectCommandType()
    {
        // Arrange
        var partialUpdateCommand = DataCommands.PartialUpdate<Customer>(
            new Dictionary<string, object?>(StringComparer.Ordinal) { ["Name"] = "Updated" },
            c => c.Id == 1);

        // Act
        var result = partialUpdateCommand.WithTimeout(TimeSpan.FromMinutes(1));

        // Assert
        result.ShouldBeOfType<PartialUpdateCommand<Customer>>();
        result.ShouldBeSameAs(partialUpdateCommand);

        
    }

    // Helper class for testing
    private sealed class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
