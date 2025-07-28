using System;
using System.Linq.Expressions;
using FractalDataWorks.Connections.Data.Commands;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Connections.Data.Tests.Commands;

public class DeleteCommandTests
{
    [Fact]
    public void CreateShouldReturnNewBuilder()
    {
        // Act
        var builder = DeleteCommand<TestEntity>.Create();
        
        // Assert
        builder.ShouldNotBeNull();
        builder.ShouldBeOfType<DeleteCommandBuilder<TestEntity>>();
    }
    
    [Fact]
    public void BuildWithWhereClauseShouldSucceed()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> whereClause = e => e.Active == false;
        
        // Act
        var command = DeleteCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable")
            .Where(whereClause)
            .Build();
        
        // Assert
        command.DataStore.ShouldBe("SqlServer");
        command.Container.ShouldBe("TestDB");
        command.Record.ShouldBe("TestTable");
        command.WhereClause.ShouldBe(whereClause);
        command.Identifier.ShouldBeNull();
        command.OperationType.ShouldBe("Delete");
        command.TargetName.ShouldBe("TestTable");
    }
    
    [Fact]
    public void BuildWithIdentifierShouldSucceed()
    {
        // Act
        var command = DeleteCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable")
            .WithId(42)
            .Build();
        
        // Assert
        command.Identifier.ShouldBe(42);
        command.WhereClause.ShouldBeNull();
    }
    
    [Fact]
    public void BuildWithBothWhereAndIdentifierShouldSucceed()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> whereClause = e => e.Active == false;
        
        // Act
        var command = DeleteCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable")
            .Where(whereClause)
            .WithId(42)
            .Build();
        
        // Assert
        command.WhereClause.ShouldBe(whereClause);
        command.Identifier.ShouldBe(42);
    }
    
    [Fact]
    public void BuildWithoutWhereOrIdentifierShouldThrow()
    {
        // Arrange
        var builder = DeleteCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable");
        
        // Act & Assert
        Should.Throw<InvalidOperationException>(() => builder.Build())
            .Message.ShouldContain("must have either WhereClause or Identifier");
    }
    
    [Fact]
    public void WithParameterShouldAddCustomParameters()
    {
        // Act
        var command = DeleteCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable")
            .WithId(42)
            .WithParameter("SoftDelete", true)
            .WithParameter("DeletedBy", "admin")
            .Build();
        
        // Assert
        command.Parameters.Count.ShouldBe(2);
        command.Parameters["SoftDelete"].ShouldBe(true);
        command.Parameters["DeletedBy"].ShouldBe("admin");
    }
    
    [Fact]
    public void IdentifierCanBeStringType()
    {
        // Act
        var command = DeleteCommand<TestEntity>.Create()
            .From("FileSystem", "/data", "customers.json")
            .WithId("customer-123")
            .Build();
        
        // Assert
        command.Identifier.ShouldBe("customer-123");
    }
    
    [Fact]
    public void IdentifierCanBeGuidType()
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Act
        var command = DeleteCommand<TestEntity>.Create()
            .From("RestApi", "https://api.example.com", "customers")
            .WithId(guid)
            .Build();
        
        // Assert
        command.Identifier.ShouldBe(guid);
    }
    
    [Fact]
    public void BuilderShouldBeImmutable()
    {
        // Arrange
        var builder1 = DeleteCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable")
            .WithId(1);
        
        // Act
        var builder2 = builder1.WithParameter("SoftDelete", true);
        var command1 = builder1.Build();
        var command2 = builder2.Build();
        
        // Assert
        builder1.ShouldNotBe(builder2);
        command1.Parameters.ShouldBeEmpty();
        command2.Parameters.Count.ShouldBe(1);
    }
    
    [Fact]
    public void ComplexWhereClauseShouldBePreserved()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> complexWhere = e => 
            e.Active == false && 
            (e.Name.StartsWith("Test") || e.Email.Contains("@deleted"));
        
        // Act
        var command = DeleteCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable")
            .Where(complexWhere)
            .Build();
        
        // Assert
        command.WhereClause.ShouldBe(complexWhere);
    }
    
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}