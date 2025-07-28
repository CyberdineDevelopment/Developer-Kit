using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Connections.Data.Commands;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Connections.Data.Tests.Commands;

public class InsertCommandTests
{
    [Fact]
    public void CreateShouldReturnNewBuilder()
    {
        // Act
        var builder = InsertCommand<TestEntity>.Create();
        
        // Assert
        builder.ShouldNotBeNull();
        builder.ShouldBeOfType<InsertCommandBuilder<TestEntity>>();
    }
    
    [Fact]
    public void BuildWithSingleEntityShouldSucceed()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        
        // Act
        var command = InsertCommand<TestEntity>.Create()
            .Into("SqlServer", "TestDB", "TestTable")
            .WithEntity(entity)
            .Build();
        
        // Assert
        command.ShouldNotBeNull();
        command.DataStore.ShouldBe("SqlServer");
        command.Container.ShouldBe("TestDB");
        command.Record.ShouldBe("TestTable");
        command.Entity.ShouldBe(entity);
        command.Entities.ShouldBeNull();
        command.OperationType.ShouldBe("Insert");
    }
    
    [Fact]
    public void BuildWithMultipleEntitiesShouldSucceed()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Id = 1, Name = "Test1" },
            new() { Id = 2, Name = "Test2" }
        };
        
        // Act
        var command = InsertCommand<TestEntity>.Create()
            .Into("SqlServer", "TestDB", "TestTable")
            .WithEntities(entities)
            .Build();
        
        // Assert
        command.Entity.ShouldBeNull();
        command.Entities.ShouldNotBeNull();
        command.Entities.Count().ShouldBe(2);
    }
    
    [Fact]
    public void BuildWithEntitiesParamsShouldSucceed()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 1, Name = "Test1" };
        var entity2 = new TestEntity { Id = 2, Name = "Test2" };
        
        // Act
        var command = InsertCommand<TestEntity>.Create()
            .Into("SqlServer", "TestDB", "TestTable")
            .WithEntities(entity1, entity2)
            .Build();
        
        // Assert
        command.Entities.ShouldNotBeNull();
        command.Entities.Count().ShouldBe(2);
    }
    
    [Fact]
    public void BuildWithoutEntityShouldThrow()
    {
        // Arrange
        var builder = InsertCommand<TestEntity>.Create()
            .Into("SqlServer", "TestDB", "TestTable");
        
        // Act & Assert
        Should.Throw<InvalidOperationException>(() => builder.Build())
            .Message.ShouldContain("must have at least one entity");
    }
    
    [Fact]
    public void OnlyAttributesShouldSetSelectedAttributes()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test", Email = "test@example.com" };
        
        // Act
        var command = InsertCommand<TestEntity>.Create()
            .Into("SqlServer", "TestDB", "TestTable")
            .WithEntity(entity)
            .OnlyAttributes("Name", "Email")
            .Build();
        
        // Assert
        command.Attributes.ShouldBe(new[] { "Name", "Email" });
    }
    
    [Fact]
    public void WithParameterShouldAddCustomParameters()
    {
        // Arrange
        var entity = new TestEntity { Id = 1 };
        
        // Act
        var command = InsertCommand<TestEntity>.Create()
            .Into("SqlServer", "TestDB", "TestTable")
            .WithEntity(entity)
            .WithParameter("ReturnIdentity", true)
            .WithParameter("Timeout", 60)
            .Build();
        
        // Assert
        command.Parameters.Count.ShouldBe(2);
        command.Parameters["ReturnIdentity"].ShouldBe(true);
        command.Parameters["Timeout"].ShouldBe(60);
    }
    
    [Fact]
    public void BuilderShouldBeImmutable()
    {
        // Arrange
        var entity = new TestEntity { Id = 1 };
        var builder1 = InsertCommand<TestEntity>.Create()
            .Into("SqlServer", "TestDB", "TestTable")
            .WithEntity(entity);
        
        // Act
        var builder2 = builder1.OnlyAttributes("Name");
        var command1 = builder1.Build();
        var command2 = builder2.Build();
        
        // Assert
        builder1.ShouldNotBe(builder2);
        command1.Attributes.ShouldBeNull();
        command2.Attributes.ShouldBe(new[] { "Name" });
    }
    
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}