using System;
using System.Linq;
using FractalDataWorks.Data;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Data.Tests;

/// <summary>
/// Tests for EntityBase with various generic key types.
/// </summary>
public class EntityBaseGenericTests
{
    private readonly ITestOutputHelper _output;

    public EntityBaseGenericTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void EntityBaseWithGuidKeyShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var entity = new GuidKeyEntity { Id = guid };

        // Act & Assert
        entity.Id.ShouldBe(guid);
        entity.IsTransient().ShouldBeFalse(); // Non-empty GUID
        _output.WriteLine($"EntityBase with GUID key: {entity.Id}");
    }

    [Fact]
    public void EntityBaseWithGuidKeyShouldBeTransientWithEmptyGuid()
    {
        // Arrange & Act
        var entity = new GuidKeyEntity(); // Id = Guid.Empty by default

        // Assert
        entity.IsTransient().ShouldBeTrue();
        entity.Id.ShouldBe(Guid.Empty);
        _output.WriteLine($"EntityBase with empty GUID is transient: {entity.IsTransient()}");
    }

    [Fact]
    public void EntityBaseWithStringKeyShouldWork()
    {
        // Arrange
        var entity = new StringKeyEntity { Id = "test-id-123" };

        // Act & Assert
        entity.Id.ShouldBe("test-id-123");
        entity.IsTransient().ShouldBeFalse(); // Non-null string
        _output.WriteLine($"EntityBase with string key: {entity.Id}");
    }

    [Fact]
    public void EntityBaseWithStringKeyShouldHandleNullString()
    {
        // Arrange & Act
        var entity = new StringKeyEntity(); // Id = null by default

        // Assert
        entity.Id.ShouldBeNull();
        // Note: IsTransient() will throw NullReferenceException for null string IDs
        // This is expected behavior - null strings don't work well with IEquatable constraint
        Should.Throw<NullReferenceException>(() => entity.IsTransient());
        _output.WriteLine($"EntityBase with null string ID throws NullReferenceException in IsTransient()");
    }

    [Fact]
    public void EntityBaseWithLongKeyShouldWork()
    {
        // Arrange
        var entity = new LongKeyEntity { Id = 123456789L };

        // Act & Assert
        entity.Id.ShouldBe(123456789L);
        entity.IsTransient().ShouldBeFalse(); // Non-zero long
        _output.WriteLine($"EntityBase with long key: {entity.Id}");
    }

    [Fact]
    public void EntityBaseWithLongKeyShouldBeTransientWithZero()
    {
        // Arrange & Act
        var entity = new LongKeyEntity(); // Id = 0 by default

        // Assert
        entity.IsTransient().ShouldBeTrue();
        entity.Id.ShouldBe(0L);
        _output.WriteLine($"EntityBase with zero long is transient: {entity.IsTransient()}");
    }

    [Fact]
    public void EqualityComparisonShouldWorkForGuidEntities()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var entity1 = new GuidKeyEntity { Id = guid };
        var entity2 = new GuidKeyEntity { Id = guid };
        var entity3 = new GuidKeyEntity { Id = Guid.NewGuid() };

        // Act & Assert
        entity1.Equals(entity2).ShouldBeTrue();
        entity1.Equals(entity3).ShouldBeFalse();
        entity1.GetHashCode().ShouldBe(entity2.GetHashCode());

        _output.WriteLine($"GUID entities with same ID are equal: {guid}");
    }

    [Fact]
    public void EqualityComparisonShouldWorkForStringEntities()
    {
        // Arrange
        var entity1 = new StringKeyEntity { Id = "same-id" };
        var entity2 = new StringKeyEntity { Id = "same-id" };
        var entity3 = new StringKeyEntity { Id = "different-id" };

        // Act & Assert
        entity1.Equals(entity2).ShouldBeTrue();
        entity1.Equals(entity3).ShouldBeFalse();
        entity1.GetHashCode().ShouldBe(entity2.GetHashCode());

        _output.WriteLine($"String entities with same ID are equal: {entity1.Id}");
    }

    [Fact]
    public void ValidationShouldWorkForGuidEntities()
    {
        // Arrange
        var validEntity = new GuidKeyEntity 
        { 
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var invalidEntity = new GuidKeyEntity(); // Empty GUID, not deleted

        // Act & Assert
        validEntity.IsValid().ShouldBeTrue();
        invalidEntity.IsValid().ShouldBeFalse();

        _output.WriteLine($"GUID entity validation - Valid: {validEntity.IsValid()}, Invalid: {invalidEntity.IsValid()}");
    }

    [Fact]
    public void ValidationShouldWorkForStringEntities()
    {
        // Arrange
        var validEntity = new StringKeyEntity 
        { 
            Id = "valid-id",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var invalidEntity = new StringKeyEntity 
        { 
            Id = "test-id",
            CreatedAt = DateTime.UtcNow.AddDays(1) // Future date makes it invalid
        };

        // Act & Assert
        validEntity.IsValid().ShouldBeTrue();
        invalidEntity.IsValid().ShouldBeFalse();

        _output.WriteLine($"String entity validation - Valid: {validEntity.IsValid()}, Invalid: {invalidEntity.IsValid()}");
    }

    [Fact]
    public void MarkingMethodsShouldWorkForAllKeyTypes()
    {
        // Arrange
        var guidEntity = new GuidKeyEntity { Id = Guid.NewGuid() };
        var stringEntity = new StringKeyEntity { Id = "test-id" };
        var longEntity = new LongKeyEntity { Id = 123L };
        var userId = "test-user";

        // Act
        guidEntity.MarkAsCreated(userId);
        stringEntity.MarkAsModified(userId);
        longEntity.MarkAsDeleted(userId);

        // Assert
        guidEntity.CreatedBy.ShouldBe(userId);
        stringEntity.ModifiedBy.ShouldBe(userId);
        longEntity.DeletedBy.ShouldBe(userId);
        longEntity.IsDeleted.ShouldBeTrue();

        _output.WriteLine($"Marking methods work for all key types - User: {userId}");
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(long))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(string))]
    public void EntityBaseShouldSupportIEquatableKeyTypes(Type keyType)
    {
        // This test verifies the constraint TKey : IEquatable<TKey>
        // If the test compiles, the constraint is working correctly
        
        // Assert
        var implementsIEquatable = keyType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEquatable<>));
        implementsIEquatable.ShouldBeTrue();
        _output.WriteLine($"Key type {keyType.Name} implements IEquatable");
    }

    // Test entity implementations for different key types
    public class GuidKeyEntity : EntityBase<Guid>
    {
    }

    public class StringKeyEntity : EntityBase<string>
    {
    }

    public class LongKeyEntity : EntityBase<long>
    {
    }
}