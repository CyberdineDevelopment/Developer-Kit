using System;
using System.Linq;
using FractalDataWorks.Data;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Data.Tests;

/// <summary>
/// Tests for EntityBase class.
/// </summary>
public class EntityBaseTests
{
    [Fact]
    public void DefaultConstructorSetsCreatedAtToUtcNow()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        var entity = new TestEntity();
        var afterCreate = DateTime.UtcNow;

        // Assert
        entity.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreate);
        entity.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreate);
    }

    [Fact]
    public void DefaultConstructorSetsDefaultValues()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.Id.ShouldBe(0);
        entity.CreatedBy.ShouldBeNull();
        entity.ModifiedAt.ShouldBeNull();
        entity.ModifiedBy.ShouldBeNull();
        entity.Version.ShouldBeNull();
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAt.ShouldBeNull();
        entity.DeletedBy.ShouldBeNull();
    }

    [Fact]
    public void IsTransientReturnsTrueForDefaultId()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        entity.IsTransient().ShouldBeTrue();
    }

    [Fact]
    public void IsTransientReturnsFalseForNonDefaultId()
    {
        // Arrange
        var entity = new TestEntity { Id = 123 };

        // Act & Assert
        entity.IsTransient().ShouldBeFalse();
    }

    [Fact]
    public void EqualsReturnsTrueForSameReference()
    {
        // Arrange
        var entity = new TestEntity { Id = 123 };

        // Act & Assert
        entity.Equals(entity).ShouldBeTrue();
    }

    [Fact]
    public void EqualsReturnsFalseForNull()
    {
        // Arrange
        var entity = new TestEntity { Id = 123 };

        // Act & Assert
        entity.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void EqualsReturnsFalseForDifferentType()
    {
        // Arrange
        var entity = new TestEntity { Id = 123 };
        var other = new object();

        // Act & Assert
        entity.Equals(other).ShouldBeFalse();
    }

    [Fact]
    public void EqualsReturnsFalseForDifferentEntityType()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 123 };
        var entity2 = new AnotherTestEntity { Id = 123 };

        // Act & Assert
        entity1.Equals(entity2).ShouldBeFalse();
    }

    [Fact]
    public void EqualsReturnsTrueForSameId()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 123 };
        var entity2 = new TestEntity { Id = 123 };

        // Act & Assert
        entity1.Equals(entity2).ShouldBeTrue();
    }

    [Fact]
    public void EqualsReturnsFalseForDifferentId()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 123 };
        var entity2 = new TestEntity { Id = 456 };

        // Act & Assert
        entity1.Equals(entity2).ShouldBeFalse();
    }

    [Fact]
    public void EqualsReturnsFalseForTransientEntities()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act & Assert
        entity1.Equals(entity2).ShouldBeFalse();
    }

    [Fact]
    public void EqualsReturnsFalseWhenOneEntityIsTransient()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 123 };
        var entity2 = new TestEntity();

        // Act & Assert
        entity1.Equals(entity2).ShouldBeFalse();
        entity2.Equals(entity1).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCodeReturnsSameValueForSameId()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 123 };
        var entity2 = new TestEntity { Id = 123 };

        // Act & Assert
        entity1.GetHashCode().ShouldBe(entity2.GetHashCode());
    }

    [Fact]
    public void GetHashCodeReturnsDifferentValueForDifferentId()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 123 };
        var entity2 = new TestEntity { Id = 456 };

        // Act & Assert
        entity1.GetHashCode().ShouldNotBe(entity2.GetHashCode());
    }

    [Fact]
    public void GetHashCodeUsesBaseImplementationForTransientEntities()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act & Assert
        entity1.GetHashCode().ShouldNotBe(entity2.GetHashCode());
    }

    [Fact]
    public void MarkAsCreatedSetsCreatedProperties()
    {
        // Arrange
        var entity = new TestEntity();
        var userId = "user123";
        var beforeMark = DateTime.UtcNow;

        // Act
        entity.MarkAsCreated(userId);
        var afterMark = DateTime.UtcNow;

        // Assert
        entity.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeMark);
        entity.CreatedAt.ShouldBeLessThanOrEqualTo(afterMark);
        entity.CreatedBy.ShouldBe(userId);
    }

    [Fact]
    public void MarkAsModifiedSetsModifiedProperties()
    {
        // Arrange
        var entity = new TestEntity();
        var userId = "user456";
        var beforeMark = DateTime.UtcNow;

        // Act
        entity.MarkAsModified(userId);
        var afterMark = DateTime.UtcNow;

        // Assert
        entity.ModifiedAt.ShouldNotBeNull();
        entity.ModifiedAt.Value.ShouldBeGreaterThanOrEqualTo(beforeMark);
        entity.ModifiedAt.Value.ShouldBeLessThanOrEqualTo(afterMark);
        entity.ModifiedBy.ShouldBe(userId);
    }

    [Fact]
    public void MarkAsDeletedSetsDeletedProperties()
    {
        // Arrange
        var entity = new TestEntity();
        var userId = "user789";
        var beforeMark = DateTime.UtcNow;

        // Act
        entity.MarkAsDeleted(userId);
        var afterMark = DateTime.UtcNow;

        // Assert
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedAt.ShouldNotBeNull();
        entity.DeletedAt.Value.ShouldBeGreaterThanOrEqualTo(beforeMark);
        entity.DeletedAt.Value.ShouldBeLessThanOrEqualTo(afterMark);
        entity.DeletedBy.ShouldBe(userId);
    }

    [Fact]
    public void RestoreResetsDeletedPropertiesAndMarksAsModified()
    {
        // Arrange
        var entity = new TestEntity();
        entity.MarkAsDeleted("deleteUser");
        var userId = "restoreUser";
        var beforeRestore = DateTime.UtcNow;

        // Act
        entity.Restore(userId);
        var afterRestore = DateTime.UtcNow;

        // Assert
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAt.ShouldBeNull();
        entity.DeletedBy.ShouldBeNull();
        entity.ModifiedAt.ShouldNotBeNull();
        entity.ModifiedAt.Value.ShouldBeGreaterThanOrEqualTo(beforeRestore);
        entity.ModifiedAt.Value.ShouldBeLessThanOrEqualTo(afterRestore);
        entity.ModifiedBy.ShouldBe(userId);
    }

    [Fact]
    public void IsValidReturnsTrueForValidEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 123 };

        // Act & Assert
        entity.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void IsValidReturnsFalseForTransientNonDeletedEntity()
    {
        // Arrange
        var entity = new TestEntity(); // Transient (Id = 0)

        // Act & Assert
        entity.IsValid().ShouldBeFalse();
    }

    [Fact]
    public void IsValidReturnsTrueForTransientDeletedEntity()
    {
        // Arrange
        var entity = new TestEntity { IsDeleted = true }; // Transient but deleted

        // Act & Assert
        entity.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void ValidateReturnsEmptyForValidEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 123 };

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsErrorForTransientNonDeletedEntity()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.Count.ShouldBe(1);
        errors.First().ShouldContain("Entity ID cannot be empty");
    }

    [Fact]
    public void ValidateReturnsErrorForFutureCreatedDate()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = 123,
            CreatedAt = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.Count.ShouldBe(1);
        errors.First().ShouldContain("Created date cannot be in the future");
    }

    [Fact]
    public void ValidateReturnsErrorForModifiedBeforeCreated()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = 123,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.Count.ShouldBe(1);
        errors.First().ShouldContain("Modified date cannot be before created date");
    }

    [Fact]
    public void ValidateReturnsErrorForDeletedBeforeCreated()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = 123,
            CreatedAt = DateTime.UtcNow,
            DeletedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.Count.ShouldBe(1);
        errors.First().ShouldContain("Deleted date cannot be before created date");
    }

    [Fact]
    public void ValidateReturnsMultipleErrors()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            // Transient (Id = 0)
            CreatedAt = DateTime.UtcNow.AddDays(1), // Future date
            ModifiedAt = DateTime.UtcNow.AddDays(-2) // Before created
        };

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.Count.ShouldBe(3);
    }

    [Fact]
    public void GenericEntityWithGuidKey()
    {
        // Arrange
        var guidEntity = new GuidEntity { Id = Guid.NewGuid() };

        // Act & Assert
        guidEntity.IsTransient().ShouldBeFalse();
        guidEntity.IsValid().ShouldBeTrue();
    }

    [Fact]
    public void GenericEntityWithStringKey()
    {
        // Arrange
        var stringEntity1 = new StringEntity { Id = "key1" };
        var stringEntity2 = new StringEntity { Id = "key1" };
        var stringEntity3 = new StringEntity { Id = "key2" };

        // Act & Assert
        stringEntity1.Equals(stringEntity2).ShouldBeTrue();
        stringEntity1.Equals(stringEntity3).ShouldBeFalse();
        stringEntity1.GetHashCode().ShouldBe(stringEntity2.GetHashCode());
    }

    [Fact]
    public void EntityBaseIntConvenienceClass()
    {
        // Arrange
        var entity = new ConcreteEntity { Id = 42 };

        // Act & Assert
        entity.ShouldBeAssignableTo<EntityBase>();
        entity.Id.ShouldBe(42);
        entity.IsTransient().ShouldBeFalse();
    }

    // Test classes
    private sealed class TestEntity : EntityBase<int>
    {
    }

    private sealed class AnotherTestEntity : EntityBase<int>
    {
    }

    private sealed class GuidEntity : EntityBase<Guid>
    {
    }

    private sealed class StringEntity : EntityBase<string>
    {
        public StringEntity()
        {
            Id = string.Empty; // Avoid null reference issues
        }
    }

    private sealed class ConcreteEntity : EntityBase
    {
    }
}