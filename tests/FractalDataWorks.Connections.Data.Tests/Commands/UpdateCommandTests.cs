using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Connections.Data.Commands;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Connections.Data.Tests.Commands;

public class UpdateCommandTests
{
    [Fact]
    public void CreateShouldReturnNewBuilder()
    {
        // Act
        var builder = UpdateCommand<TestEntity>.Create();
        
        // Assert
        builder.ShouldNotBeNull();
        builder.ShouldBeOfType<UpdateCommandBuilder<TestEntity>>();
    }
    
    [Fact]
    public void BuildWithUpdateActionShouldSucceed()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> whereClause = e => e.Id > 5;
        Action<TestEntity> updateAction = e => e.Name = "Updated";
        
        // Act
        var command = UpdateCommand<TestEntity>.Create()
            .In("SqlServer", "TestDB", "TestTable")
            .Where(whereClause)
            .Set(updateAction)
            .Build();
        
        // Assert
        command.DataStore.ShouldBe("SqlServer");
        command.Container.ShouldBe("TestDB");
        command.Record.ShouldBe("TestTable");
        command.WhereClause.ShouldBe(whereClause);
        command.UpdateAction.ShouldBe(updateAction);
        command.UpdateValues.ShouldBeNull();
        command.OperationType.ShouldBe("Update");
    }
    
    [Fact]
    public void BuildWithUpdateValuesShouldSucceed()
    {
        // Arrange
        var values = new Dictionary<string, object>
        {
            ["Name"] = "Updated",
            ["Email"] = "updated@example.com"
        };
        
        // Act
        var command = UpdateCommand<TestEntity>.Create()
            .In("SqlServer", "TestDB", "TestTable")
            .Where(e => e.Id == 1)
            .SetValues(values)
            .Build();
        
        // Assert
        command.UpdateValues.ShouldBe(values);
        command.UpdateAction.ShouldBeNull();
    }
    
    [Fact]
    public void SetValueShouldAddIndividualValues()
    {
        // Act
        var command = UpdateCommand<TestEntity>.Create()
            .In("SqlServer", "TestDB", "TestTable")
            .Where(e => e.Id == 1)
            .SetValue("Name", "Updated")
            .SetValue("Email", "updated@example.com")
            .SetValue("Active", true)
            .Build();
        
        // Assert
        command.UpdateValues.ShouldNotBeNull();
        command.UpdateValues.Count.ShouldBe(3);
        command.UpdateValues["Name"].ShouldBe("Updated");
        command.UpdateValues["Email"].ShouldBe("updated@example.com");
        command.UpdateValues["Active"].ShouldBe(true);
    }
    
    [Fact]
    public void BuildWithoutUpdateActionOrValuesShouldThrow()
    {
        // Arrange
        var builder = UpdateCommand<TestEntity>.Create()
            .In("SqlServer", "TestDB", "TestTable")
            .Where(e => e.Id == 1);
        
        // Act & Assert
        Should.Throw<InvalidOperationException>(() => builder.Build())
            .Message.ShouldContain("must have either UpdateAction or UpdateValues");
    }
    
    [Fact]
    public void OnlyAttributesShouldSetSelectedAttributes()
    {
        // Act
        var command = UpdateCommand<TestEntity>.Create()
            .In("SqlServer", "TestDB", "TestTable")
            .Where(e => e.Id == 1)
            .SetValue("Name", "Updated")
            .OnlyAttributes("Name", "ModifiedDate")
            .Build();
        
        // Assert
        command.Attributes.ShouldBe(new[] { "Name", "ModifiedDate" });
    }
    
    [Fact]
    public void WithParameterShouldAddCustomParameters()
    {
        // Act
        var command = UpdateCommand<TestEntity>.Create()
            .In("SqlServer", "TestDB", "TestTable")
            .Where(e => e.Id == 1)
            .SetValue("Name", "Updated")
            .WithParameter("AuditUser", "admin")
            .WithParameter("AuditTimestamp", DateTime.UtcNow)
            .Build();
        
        // Assert
        command.Parameters.Count.ShouldBe(2);
        command.Parameters["AuditUser"].ShouldBe("admin");
        command.Parameters.ShouldContainKey("AuditTimestamp");
    }
    
    [Fact]
    public void SetValueShouldOverwriteExistingValue()
    {
        // Act
        var command = UpdateCommand<TestEntity>.Create()
            .In("SqlServer", "TestDB", "TestTable")
            .Where(e => e.Id == 1)
            .SetValue("Name", "First")
            .SetValue("Name", "Second")
            .Build();
        
        // Assert
        command.UpdateValues["Name"].ShouldBe("Second");
    }
    
    [Fact]
    public void BuilderShouldBeImmutable()
    {
        // Arrange
        var builder1 = UpdateCommand<TestEntity>.Create()
            .In("SqlServer", "TestDB", "TestTable")
            .Where(e => e.Id == 1)
            .SetValue("Name", "Updated");
        
        // Act
        var builder2 = builder1.SetValue("Email", "new@example.com");
        var command1 = builder1.Build();
        var command2 = builder2.Build();
        
        // Assert
        builder1.ShouldNotBe(builder2);
        command1.UpdateValues.Count.ShouldBe(1);
        command2.UpdateValues.Count.ShouldBe(2);
    }
    
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}