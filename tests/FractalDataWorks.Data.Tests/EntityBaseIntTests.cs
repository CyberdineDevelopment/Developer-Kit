using System;
using FractalDataWorks.Data;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Data.Tests;

/// <summary>
/// Tests for EntityBase (integer primary key) class.
/// </summary>
public class EntityBaseIntTests
{
    private readonly ITestOutputHelper _output;

    public EntityBaseIntTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void EntityBaseShouldInheritFromGenericEntityBase()
    {
        // Arrange & Act
        var entity = new TestIntEntity();

        // Assert
        entity.ShouldBeAssignableTo<EntityBase<int>>();
        entity.ShouldBeAssignableTo<EntityBase>();
        _output.WriteLine("EntityBase correctly inherits from EntityBase<int>");
    }

    [Fact]
    public void EntityBaseShouldHaveIntegerPrimaryKey()
    {
        // Arrange & Act
        var entity = new TestIntEntity { Id = 42 };

        // Assert
        entity.Id.ShouldBe(42);
        entity.Id.ShouldBeOfType<int>();
        _output.WriteLine($"EntityBase has integer primary key: {entity.Id}");
    }

    [Fact]
    public void EntityBaseShouldSupportAllBaseEntityFeatures()
    {
        // Arrange
        var entity = new TestIntEntity { Id = 100 };
        var userId = "test-user";

        // Act
        entity.MarkAsCreated(userId);
        entity.MarkAsModified(userId);
        entity.MarkAsDeleted(userId);

        // Assert
        entity.CreatedBy.ShouldBe(userId);
        entity.ModifiedBy.ShouldBe(userId);
        entity.DeletedBy.ShouldBe(userId);
        entity.IsDeleted.ShouldBeTrue();

        _output.WriteLine($"EntityBase supports all base features - Created, Modified, and Deleted by: {userId}");
    }

    [Fact]
    public void EntityBaseShouldBeTransientWithDefaultId()
    {
        // Arrange & Act
        var entity = new TestIntEntity(); // Id = 0 by default

        // Assert
        entity.IsTransient().ShouldBeTrue();
        entity.Id.ShouldBe(0);
        _output.WriteLine($"EntityBase with default ID (0) is transient: {entity.IsTransient()}");
    }

    [Fact]
    public void EntityBaseShouldNotBeTransientWithNonDefaultId()
    {
        // Arrange & Act
        var entity = new TestIntEntity { Id = 1 };

        // Assert
        entity.IsTransient().ShouldBeFalse();
        _output.WriteLine($"EntityBase with ID {entity.Id} is not transient: {!entity.IsTransient()}");
    }

    [Fact]
    public void EntityBaseShouldSupportEqualityComparison()
    {
        // Arrange
        var entity1 = new TestIntEntity { Id = 123 };
        var entity2 = new TestIntEntity { Id = 123 };
        var entity3 = new TestIntEntity { Id = 456 };

        // Act & Assert
        entity1.Equals(entity2).ShouldBeTrue();
        entity1.Equals(entity3).ShouldBeFalse();
        entity1.GetHashCode().ShouldBe(entity2.GetHashCode());
        entity1.GetHashCode().ShouldNotBe(entity3.GetHashCode());

        _output.WriteLine($"Entities with same ID are equal: {entity1.Id}");
        _output.WriteLine($"Entities with different IDs are not equal: {entity1.Id} vs {entity3.Id}");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void EntityBaseShouldSupportVariousIntegerValues(int id)
    {
        // Arrange & Act
        var entity = new TestIntEntity { Id = id };

        // Assert
        entity.Id.ShouldBe(id);
        
        if (id == 0)
        {
            entity.IsTransient().ShouldBeTrue();
        }
        else
        {
            entity.IsTransient().ShouldBeFalse();
        }

        _output.WriteLine($"EntityBase supports ID value: {id}, IsTransient: {entity.IsTransient()}");
    }

    [Fact]
    public void EntityBaseShouldValidateCorrectly()
    {
        // Arrange
        var validEntity = new TestIntEntity 
        { 
            Id = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ModifiedAt = DateTime.UtcNow
        };

        var invalidEntity = new TestIntEntity(); // Transient and not deleted

        // Act & Assert
        validEntity.IsValid().ShouldBeTrue();
        invalidEntity.IsValid().ShouldBeFalse();

        _output.WriteLine($"Valid entity validation result: {validEntity.IsValid()}");
        _output.WriteLine($"Invalid entity validation result: {invalidEntity.IsValid()}");
    }

    // Test entity implementation
    public class TestIntEntity : EntityBase
    {
    }
}