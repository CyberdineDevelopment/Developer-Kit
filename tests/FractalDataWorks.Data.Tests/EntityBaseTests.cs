using System;
using System.Linq;
using FractalDataWorks.Data;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Data.Tests;

/// <summary>
/// Tests for EntityBase class.
/// </summary>
public class EntityBaseTests
{
    private readonly ITestOutputHelper _output;

    public EntityBaseTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstructorShouldSetDefaultValues()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.ShouldBe(0);
        entity.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
        entity.CreatedBy.ShouldBeNull();
        entity.ModifiedAt.ShouldBeNull();
        entity.ModifiedBy.ShouldBeNull();
        entity.Version.ShouldBeNull();
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAt.ShouldBeNull();
        entity.DeletedBy.ShouldBeNull();

        _output.WriteLine($"Entity created: Id={entity.Id}, CreatedAt={entity.CreatedAt}, IsDeleted={entity.IsDeleted}");
    }

    [Fact]
    public void IsTransientShouldReturnTrueForDefaultId()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var isTransient = entity.IsTransient();

        // Assert
        isTransient.ShouldBeTrue();
        _output.WriteLine($"Entity with default ID is transient: {isTransient}");
    }

    [Fact]
    public void IsTransientShouldReturnFalseForNonDefaultId()
    {
        // Arrange
        var entity = new TestEntity { Id = 123 };

        // Act
        var isTransient = entity.IsTransient();

        // Assert
        isTransient.ShouldBeFalse();
        _output.WriteLine($"Entity with ID {entity.Id} is transient: {isTransient}");
    }

    [Fact]
    public void MarkAsCreatedShouldSetCreatedProperties()
    {
        // Arrange
        var entity = new TestEntity();
        var userId = "test-user";
        var beforeMark = DateTime.UtcNow;

        // Act
        entity.MarkAsCreated(userId);

        // Assert
        entity.CreatedBy.ShouldBe(userId);
        entity.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeMark);
        entity.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);

        _output.WriteLine($"Entity marked as created by '{userId}' at {entity.CreatedAt}");
    }

    [Fact]
    public void MarkAsModifiedShouldSetModifiedProperties()
    {
        // Arrange
        var entity = new TestEntity();
        var userId = "test-user";
        var beforeMark = DateTime.UtcNow;

        // Act
        entity.MarkAsModified(userId);

        // Assert
        entity.ModifiedBy.ShouldBe(userId);
        entity.ModifiedAt.ShouldNotBeNull();
        entity.ModifiedAt.Value.ShouldBeGreaterThanOrEqualTo(beforeMark);
        entity.ModifiedAt.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);

        _output.WriteLine($"Entity marked as modified by '{userId}' at {entity.ModifiedAt}");
    }

    [Fact]
    public void MarkAsDeletedShouldSetDeletedProperties()
    {
        // Arrange
        var entity = new TestEntity();
        var userId = "test-user";
        var beforeMark = DateTime.UtcNow;

        // Act
        entity.MarkAsDeleted(userId);

        // Assert
        entity.IsDeleted.ShouldBeTrue();
        entity.DeletedBy.ShouldBe(userId);
        entity.DeletedAt.ShouldNotBeNull();
        entity.DeletedAt.Value.ShouldBeGreaterThanOrEqualTo(beforeMark);
        entity.DeletedAt.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);

        _output.WriteLine($"Entity marked as deleted by '{userId}' at {entity.DeletedAt}");
    }

    [Fact]
    public void RestoreShouldUnmarkDeletedAndMarkAsModified()
    {
        // Arrange
        var entity = new TestEntity();
        entity.MarkAsDeleted("delete-user");
        var restoreUserId = "restore-user";

        // Act
        entity.Restore(restoreUserId);

        // Assert
        entity.IsDeleted.ShouldBeFalse();
        entity.DeletedAt.ShouldBeNull();
        entity.DeletedBy.ShouldBeNull();
        entity.ModifiedBy.ShouldBe(restoreUserId);
        entity.ModifiedAt.ShouldNotBeNull();

        _output.WriteLine($"Entity restored by '{restoreUserId}', IsDeleted={entity.IsDeleted}, ModifiedBy={entity.ModifiedBy}");
    }

    [Fact]
    public void EqualsShouldReturnTrueForSameReference()
    {
        // Arrange
        var entity = new TestEntity { Id = 1 };

        // Act & Assert
        entity.Equals(entity).ShouldBeTrue();
        _output.WriteLine("Entity equals itself (same reference)");
    }

    [Fact]
    public void EqualsShouldReturnTrueForSameIdAndType()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 123 };
        var entity2 = new TestEntity { Id = 123 };

        // Act & Assert
        entity1.Equals(entity2).ShouldBeTrue();
        entity2.Equals(entity1).ShouldBeTrue();
        _output.WriteLine($"Entities with same ID ({entity1.Id}) are equal");
    }

    [Fact]
    public void EqualsShouldReturnFalseForDifferentIds()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 123 };
        var entity2 = new TestEntity { Id = 456 };

        // Act & Assert
        entity1.Equals(entity2).ShouldBeFalse();
        _output.WriteLine($"Entities with different IDs ({entity1.Id}, {entity2.Id}) are not equal");
    }

    [Fact]
    public void EqualsShouldReturnFalseForDifferentTypes()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 123 };
        var entity2 = new AnotherTestEntity { Id = 123 };

        // Act & Assert
        entity1.Equals(entity2).ShouldBeFalse();
        _output.WriteLine("Entities of different types are not equal even with same ID");
    }

    [Fact]
    public void EqualsShouldReturnFalseForTransientEntities()
    {
        // Arrange
        var entity1 = new TestEntity(); // Id = 0, transient
        var entity2 = new TestEntity(); // Id = 0, transient

        // Act & Assert
        entity1.Equals(entity2).ShouldBeFalse();
        _output.WriteLine("Transient entities are not equal even with same default ID");
    }

    [Fact]
    public void EqualsShouldReturnFalseForNullOrNonEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 123 };

        // Act & Assert
        entity.Equals(null).ShouldBeFalse();
        entity.Equals("not an entity").ShouldBeFalse();
        entity.Equals(123).ShouldBeFalse();

        _output.WriteLine("Entity correctly returns false for null and non-entity objects");
    }

    [Fact]
    public void GetHashCodeShouldBeConsistentForSameEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 123 };

        // Act
        var hash1 = entity.GetHashCode();
        var hash2 = entity.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
        _output.WriteLine($"Entity hash code is consistent: {hash1}");
    }

    [Fact]
    public void GetHashCodeShouldBeSameForEqualEntities()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 123 };
        var entity2 = new TestEntity { Id = 123 };

        // Act
        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
        _output.WriteLine($"Equal entities have same hash code: {hash1}");
    }

    [Fact]
    public void GetHashCodeShouldUseDifferentAlgorithmForTransientEntities()
    {
        // Arrange
        var entity1 = new TestEntity(); // transient
        var entity2 = new TestEntity(); // transient

        // Act
        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();

        // Assert
        hash1.ShouldNotBe(hash2); // Should be different as they use base.GetHashCode()
        _output.WriteLine($"Transient entities have different hash codes: {hash1}, {hash2}");
    }

    [Fact]
    public void ValidateShouldReturnNoErrorsForValidEntity()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = 123,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ModifiedAt = DateTime.UtcNow
        };

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.ShouldBeEmpty();
        _output.WriteLine("Valid entity has no validation errors");
    }

    [Fact]
    public void ValidateShouldReturnErrorForTransientNonDeletedEntity()
    {
        // Arrange
        var entity = new TestEntity(); // Id = 0 (transient), IsDeleted = false

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.Count.ShouldBe(1);
        errors[0].ShouldBe("Entity ID cannot be empty for non-deleted entities");
        _output.WriteLine($"Validation error for transient entity: {errors[0]}");
    }

    [Fact]
    public void ValidateShouldReturnErrorForFutureCreatedDate()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = 123,
            CreatedAt = DateTime.UtcNow.AddDays(1) // Future date
        };

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.ShouldContain("Created date cannot be in the future");
        _output.WriteLine($"Validation error for future created date: {errors.First(e => e.Contains("future"))}");
    }

    [Fact]
    public void ValidateShouldReturnErrorForModifiedDateBeforeCreated()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = 123,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow.AddDays(-1) // Before created
        };

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.ShouldContain("Modified date cannot be before created date");
        _output.WriteLine($"Validation error for modified date before created: {errors.First(e => e.Contains("Modified"))}");
    }

    [Fact]
    public void ValidateShouldReturnErrorForDeletedDateBeforeCreated()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = 123,
            CreatedAt = DateTime.UtcNow,
            DeletedAt = DateTime.UtcNow.AddDays(-1) // Before created
        };

        // Act
        var errors = entity.Validate().ToList();

        // Assert
        errors.ShouldContain("Deleted date cannot be before created date");
        _output.WriteLine($"Validation error for deleted date before created: {errors.First(e => e.Contains("Deleted"))}");
    }

    [Fact]
    public void IsValidShouldReturnTrueForValidEntity()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = 123,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var isValid = entity.IsValid();

        // Assert
        isValid.ShouldBeTrue();
        _output.WriteLine($"Entity is valid: {isValid}");
    }

    [Fact]
    public void IsValidShouldReturnFalseForInvalidEntity()
    {
        // Arrange
        var entity = new TestEntity(); // Transient and not deleted

        // Act
        var isValid = entity.IsValid();

        // Assert
        isValid.ShouldBeFalse();
        _output.WriteLine($"Entity is valid: {isValid}");
    }

    [Theory]
    [InlineData("user1")]
    [InlineData("admin@example.com")]
    [InlineData("123456")]
    public void MarkAsCreatedShouldAcceptVariousUserIdFormats(string userId)
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.MarkAsCreated(userId);

        // Assert
        entity.CreatedBy.ShouldBe(userId);
        _output.WriteLine($"Entity marked as created by user: '{userId}'");
    }

    // Test entity implementations
    public class TestEntity : EntityBase<int>
    {
    }

    public class AnotherTestEntity : EntityBase<int>
    {
    }
}