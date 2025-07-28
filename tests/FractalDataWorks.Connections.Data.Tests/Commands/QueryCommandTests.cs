using System;
using System.Linq.Expressions;
using FractalDataWorks.Connections.Data.Commands;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Connections.Data.Tests.Commands;

public class QueryCommandTests
{
    [Fact]
    public void CreateShouldReturnNewBuilder()
    {
        // Act
        var builder = QueryCommand<TestEntity>.Create();
        
        // Assert
        builder.ShouldNotBeNull();
        builder.ShouldBeOfType<QueryCommandBuilder<TestEntity>>();
    }
    
    [Fact]
    public void BuildShouldCreateValidQueryCommand()
    {
        // Arrange
        var builder = QueryCommand<TestEntity>.Create();
        
        // Act
        var command = builder
            .From("SqlServer", "TestDB", "TestTable")
            .Build();
        
        // Assert
        command.ShouldNotBeNull();
        command.DataStore.ShouldBe("SqlServer");
        command.Container.ShouldBe("TestDB");
        command.Record.ShouldBe("TestTable");
        command.OperationType.ShouldBe("Query");
        command.TargetName.ShouldBe("TestTable");
    }
    
    [Fact]
    public void BuilderShouldSetAllProperties()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> whereClause = e => e.Id > 5;
        Expression<Func<TestEntity, object>> orderBy = e => e.Name;
        
        // Act
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable")
            .Select("Id", "Name", "Email")
            .Where(whereClause)
            .OrderBy(orderBy, descending: true)
            .Skip(10)
            .Take(20)
            .WithId(123)
            .WithParameter("CustomParam", "Value")
            .Build();
        
        // Assert
        command.Attributes.ShouldBe(new[] { "Id", "Name", "Email" });
        command.WhereClause.ShouldBe(whereClause);
        command.OrderBy.ShouldBe(orderBy);
        command.OrderByDescending.ShouldBeTrue();
        command.Skip.ShouldBe(10);
        command.Take.ShouldBe(20);
        command.Identifier.ShouldBe(123);
        command.Parameters["CustomParam"].ShouldBe("Value");
    }
    
    [Fact]
    public void BuilderShouldBeImmutable()
    {
        // Arrange
        var builder1 = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable");
        
        // Act
        var builder2 = builder1.Where(e => e.Id > 5);
        var command1 = builder1.Build();
        var command2 = builder2.Build();
        
        // Assert
        builder1.ShouldNotBe(builder2);
        command1.WhereClause.ShouldBeNull();
        command2.WhereClause.ShouldNotBeNull();
    }
    
    [Fact]
    public void ParametersShouldInitializeAsEmptyDictionary()
    {
        // Act
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable")
            .Build();
        
        // Assert
        command.Parameters.ShouldNotBeNull();
        command.Parameters.ShouldBeEmpty();
    }
    
    [Fact]
    public void WithParameterShouldAddMultipleParameters()
    {
        // Act
        var command = QueryCommand<TestEntity>.Create()
            .From("SqlServer", "TestDB", "TestTable")
            .WithParameter("Param1", "Value1")
            .WithParameter("Param2", 42)
            .WithParameter("Param3", true)
            .Build();
        
        // Assert
        command.Parameters.Count.ShouldBe(3);
        command.Parameters["Param1"].ShouldBe("Value1");
        command.Parameters["Param2"].ShouldBe(42);
        command.Parameters["Param3"].ShouldBe(true);
    }
    
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}