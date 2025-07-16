using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.Data;

/// <summary>
/// Base class for all entities in the Fractal framework.
/// </summary>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public abstract class EntityBase<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Gets or sets the entity's unique identifier.
    /// </summary>
    public virtual TKey Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the timestamp when the entity was created.
    /// </summary>
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user who created the entity.
    /// </summary>
    public virtual string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the entity was last modified.
    /// </summary>
    public virtual DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the entity.
    /// </summary>
    public virtual string? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the entity version for optimistic concurrency control.
    /// </summary>
    public virtual byte[]? Version { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted (soft delete).
    /// </summary>
    public virtual bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the entity was deleted.
    /// </summary>
    public virtual DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who deleted the entity.
    /// </summary>
    public virtual string? DeletedBy { get; set; }

    /// <summary>
    /// Determines whether this entity is equal to another entity.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if the entities are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not EntityBase<TKey> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        // If either entity is transient, they're not equal
        if (IsTransient() || other.IsTransient())
            return false;

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Gets the hash code for this entity.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        if (IsTransient())
        {
            return base.GetHashCode();
        }

        return Id.GetHashCode();
    }

    /// <summary>
    /// Determines whether this entity is transient (not persisted).
    /// </summary>
    /// <returns>True if the entity is transient; otherwise, false.</returns>
    public virtual bool IsTransient()
    {
        return Id.Equals(default(TKey));
    }

    /// <summary>
    /// Marks the entity as created by the specified user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    public virtual void MarkAsCreated(string userId)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = userId;
    }

    /// <summary>
    /// Marks the entity as modified by the specified user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    public virtual void MarkAsModified(string userId)
    {
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }

    /// <summary>
    /// Marks the entity as deleted by the specified user (soft delete).
    /// </summary>
    /// <param name="userId">The user ID.</param>
    public virtual void MarkAsDeleted(string userId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
    }

    /// <summary>
    /// Restores a soft-deleted entity.
    /// </summary>
    /// <param name="userId">The user ID performing the restore.</param>
    public virtual void Restore(string userId)
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        MarkAsModified(userId);
    }

    /// <summary>
    /// Validates the entity.
    /// </summary>
    /// <returns>True if the entity is valid; otherwise, false.</returns>
    public virtual bool IsValid()
    {
        return Validate().All(v => string.IsNullOrEmpty(v));
    }

    /// <summary>
    /// Validates the entity and returns validation errors.
    /// </summary>
    /// <returns>A collection of validation error messages.</returns>
    public virtual IEnumerable<string> Validate()
    {
        if (IsTransient() && !IsDeleted)
        {
            yield return "Entity ID cannot be empty for non-deleted entities";
        }

        if (CreatedAt > DateTime.UtcNow)
        {
            yield return "Created date cannot be in the future";
        }

        if (ModifiedAt.HasValue && ModifiedAt.Value < CreatedAt)
        {
            yield return "Modified date cannot be before created date";
        }

        if (DeletedAt.HasValue && DeletedAt.Value < CreatedAt)
        {
            yield return "Deleted date cannot be before created date";
        }
    }
}

/// <summary>
/// Base class for entities with integer primary keys.
/// </summary>
public abstract class EntityBase : EntityBase<int>
{
}

/// <summary>
/// Base class for entities with GUID primary keys.
/// </summary>
public abstract class GuidEntityBase : EntityBase<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuidEntityBase"/> class.
    /// </summary>
    protected GuidEntityBase()
    {
        Id = Guid.NewGuid();
    }
}