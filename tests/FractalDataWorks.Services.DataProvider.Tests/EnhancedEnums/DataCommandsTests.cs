using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.EnhancedEnums;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.DataProvider.Tests.EnhancedEnums;

/// <summary>
/// Tests for the DataCommands enhanced enum factory.
/// </summary>
public sealed class DataCommandsTests
{
    public DataCommandsTests()
    {
    }

    [Fact]
    public void QueryShouldCreateQueryCommandWithPredicate()
    {
        // Arrange
        Expression<Func<Customer, bool>> predicate = c => c.IsActive;
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.Query(predicate, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<QueryCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Predicate.ShouldBe(predicate);

        
    }

    [Fact]
    public void QueryShouldCreateQueryCommandWithNullPredicate()
    {
        // Arrange
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.Query<Customer>(null, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<QueryCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Predicate.ShouldBeNull();
    }

    [Fact]
    public void QueryShouldCreateQueryCommandWithEmptyConnectionName()
    {
        // Arrange
        Expression<Func<Customer, bool>> predicate = c => c.IsActive;

        // Act
        var command = DataCommands.Query(predicate);

        // Assert
        command.ShouldNotBeNull();
        command.ConnectionName.ShouldBe(string.Empty);
        command.Predicate.ShouldBe(predicate);
    }

    [Fact]
    public void QueryAllShouldCreateQueryCommandWithoutPredicate()
    {
        // Arrange
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.QueryAll<Customer>(connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<QueryCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Predicate.ShouldBeNull();

        
    }

    [Fact]
    public void QueryAllShouldCreateQueryCommandWithEmptyConnectionName()
    {
        // Act
        var command = DataCommands.QueryAll<Customer>();

        // Assert
        command.ShouldNotBeNull();
        command.ConnectionName.ShouldBe(string.Empty);
        command.Predicate.ShouldBeNull();
    }

    [Fact]
    public void QueryByIdShouldCreateQueryCommandWithIdPredicate()
    {
        // Arrange
        const int customerId = 123;
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.QueryById<Customer, int>(customerId, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<QueryCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Predicate.ShouldNotBeNull();

        // Verify the predicate compiles and references the correct property
        var compiledPredicate = command.Predicate!.Compile();
        var customer = new Customer { Id = customerId, IsActive = true };
        compiledPredicate(customer).ShouldBeTrue();

        var wrongCustomer = new Customer { Id = 456, IsActive = true };
        compiledPredicate(wrongCustomer).ShouldBeFalse();

        
    }

    [Fact]
    public void QueryByIdShouldCreateQueryCommandWithCustomIdField()
    {
        // Arrange
        const string customId = "CUST123";
        const string connectionName = "TestConnection";
        const string idFieldName = "CustomerId";

        // Act
        var command = DataCommands.QueryById<CustomerWithCustomId, string>(customId, connectionName, idFieldName);

        // Assert
        command.ShouldNotBeNull();
        command.ConnectionName.ShouldBe(connectionName);
        command.Predicate.ShouldNotBeNull();

        
    }

    [Theory]
    [InlineData(123)]
    [InlineData(456)]
    [InlineData(0)]
    public void QueryByIdShouldWorkWithDifferentIdValues(int id)
    {
        // Act
        var command = DataCommands.QueryById<Customer, int>(id, "TestConnection");

        // Assert
        command.ShouldNotBeNull();
        command.Predicate.ShouldNotBeNull();

        var compiledPredicate = command.Predicate!.Compile();
        var customer = new Customer { Id = id, IsActive = true };
        compiledPredicate(customer).ShouldBeTrue();

        
    }

    [Fact]
    public void CountShouldCreateCountCommandWithPredicate()
    {
        // Arrange
        Expression<Func<Customer, bool>> predicate = c => c.IsActive;
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.Count(predicate, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<CountCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Predicate.ShouldBe(predicate);

        
    }

    [Fact]
    public void CountShouldCreateCountCommandWithNullPredicate()
    {
        // Arrange
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.Count<Customer>(null, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ConnectionName.ShouldBe(connectionName);
        command.Predicate.ShouldBeNull();
    }

    [Fact]
    public void ExistsShouldCreateExistsCommandWithPredicate()
    {
        // Arrange
        Expression<Func<Customer, bool>> predicate = c => c.IsActive;
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.Exists(predicate, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<ExistsCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Predicate.ShouldBe(predicate);

        
    }

    [Fact]
    public void InsertShouldCreateInsertCommandWithEntity()
    {
        // Arrange
        var customer = new Customer { Id = 123, Name = "John Doe", IsActive = true };
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.Insert(customer, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<InsertCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Entity.ShouldBe(customer);

        
    }

    [Fact]
    public void BulkInsertShouldCreateBulkInsertCommandWithEntities()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", IsActive = true },
            new() { Id = 2, Name = "Jane Smith", IsActive = true },
            new() { Id = 3, Name = "Bob Johnson", IsActive = false }
        };
        const string connectionName = "TestConnection";
        const int batchSize = 500;

        // Act
        var command = DataCommands.BulkInsert(customers, connectionName, batchSize);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<BulkInsertCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Entities.ShouldBe(customers);
        command.BatchSize.ShouldBe(batchSize);

        
    }

    [Fact]
    public void BulkInsertShouldCreateBulkInsertCommandWithDefaultBatchSize()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", IsActive = true }
        };
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.BulkInsert(customers, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.BatchSize.ShouldBe(1000); // Default batch size
    }

    [Fact]
    public void UpdateShouldCreateUpdateCommandWithEntityAndPredicate()
    {
        // Arrange
        var customer = new Customer { Id = 123, Name = "Updated Name", IsActive = true };
        Expression<Func<Customer, bool>> predicate = c => c.Id == 123;
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.Update(customer, predicate, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<UpdateCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Entity.ShouldBe(customer);
        command.Predicate.ShouldBe(predicate);

        
    }

    [Fact]
    public void UpdateByIdShouldCreateUpdateCommandWithIdPredicate()
    {
        // Arrange
        var customer = new Customer { Id = 123, Name = "Updated Name", IsActive = true };
        const int customerId = 123;
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.UpdateById<Customer, int>(customer, customerId, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<UpdateCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Entity.ShouldBe(customer);
        command.Predicate.ShouldNotBeNull();

        // Verify the predicate works correctly
        var compiledPredicate = command.Predicate!.Compile();
        compiledPredicate(customer).ShouldBeTrue();

        var wrongCustomer = new Customer { Id = 456, IsActive = true };
        compiledPredicate(wrongCustomer).ShouldBeFalse();

        
    }

    [Fact]
    public void PartialUpdateShouldCreatePartialUpdateCommand()
    {
        // Arrange
        var updates = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Name"] = "Updated Name",
            ["IsActive"] = false
        };
        Expression<Func<Customer, bool>> predicate = c => c.Id == 123;
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.PartialUpdate<Customer>(updates, predicate, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<PartialUpdateCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Updates.ShouldBe(updates);
        command.Predicate.ShouldBe(predicate);

        
    }

    [Fact]
    public void DeleteShouldCreateDeleteCommandWithPredicate()
    {
        // Arrange
        Expression<Func<Customer, bool>> predicate = c => !c.IsActive;
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.Delete(predicate, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<DeleteCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Predicate.ShouldBe(predicate);

        
    }

    [Fact]
    public void DeleteByIdShouldCreateDeleteCommandWithIdPredicate()
    {
        // Arrange
        const int customerId = 123;
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.DeleteById<Customer, int>(customerId, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<DeleteCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Predicate.ShouldNotBeNull();

        // Verify the predicate works correctly
        var compiledPredicate = command.Predicate!.Compile();
        var customer = new Customer { Id = customerId, IsActive = true };
        compiledPredicate(customer).ShouldBeTrue();

        var wrongCustomer = new Customer { Id = 456, IsActive = true };
        compiledPredicate(wrongCustomer).ShouldBeFalse();

        
    }

    [Fact]
    public void TruncateShouldCreateTruncateCommand()
    {
        // Arrange
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.Truncate<Customer>(connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<TruncateCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);

        
    }

    [Fact]
    public void UpsertShouldCreateUpsertCommandWithConflictFields()
    {
        // Arrange
        var customer = new Customer { Id = 123, Name = "John Doe", IsActive = true };
        var conflictFields = new[] { "Id" };
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.Upsert(customer, conflictFields, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<UpsertCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Entity.ShouldBe(customer);
        command.ConflictFields.ShouldBe(conflictFields);

        
    }

    [Fact]
    public void UpsertByPrimaryKeyShouldCreateUpsertCommandWithDefaultPrimaryKey()
    {
        // Arrange
        var customer = new Customer { Id = 123, Name = "John Doe", IsActive = true };
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.UpsertByPrimaryKey(customer, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<UpsertCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Entity.ShouldBe(customer);
        command.ConflictFields.ShouldContain("Id");
        command.ConflictFields.Count().ShouldBe(1);

        
    }

    [Fact]
    public void UpsertByPrimaryKeyShouldCreateUpsertCommandWithCustomPrimaryKey()
    {
        // Arrange
        var customer = new Customer { Id = 123, Name = "John Doe", IsActive = true };
        const string connectionName = "TestConnection";
        const string primaryKeyField = "CustomerId";

        // Act
        var command = DataCommands.UpsertByPrimaryKey(customer, connectionName, primaryKeyField);

        // Assert
        command.ShouldNotBeNull();
        command.ConflictFields.ShouldContain(primaryKeyField);
        command.ConflictFields.Count().ShouldBe(1);

        
    }

    [Fact]
    public void BulkUpsertShouldCreateBulkUpsertCommand()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", IsActive = true },
            new() { Id = 2, Name = "Jane Smith", IsActive = true }
        };
        var conflictFields = new[] { "Id" };
        const string connectionName = "TestConnection";
        const int batchSize = 500;

        // Act
        var command = DataCommands.BulkUpsert(customers, conflictFields, connectionName, batchSize);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<BulkUpsertCommand<Customer>>();
        command.ConnectionName.ShouldBe(connectionName);
        command.Entities.ShouldBe(customers);
        command.ConflictFields.ShouldBe(conflictFields);
        command.BatchSize.ShouldBe(batchSize);

        
    }

    [Fact]
    public void BulkUpsertShouldCreateBulkUpsertCommandWithDefaultBatchSize()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", IsActive = true }
        };
        var conflictFields = new[] { "Id" };
        const string connectionName = "TestConnection";

        // Act
        var command = DataCommands.BulkUpsert(customers, conflictFields, connectionName);

        // Assert
        command.ShouldNotBeNull();
        command.BatchSize.ShouldBe(1000); // Default batch size
    }

    // Helper classes for testing
    private sealed class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    private sealed class CustomerWithCustomId
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
