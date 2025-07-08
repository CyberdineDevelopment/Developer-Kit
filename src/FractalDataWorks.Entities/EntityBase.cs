namespace FractalDataWorks.Data;

/// <summary>
/// Base class for all entities in the FractalDataWorks ecosystem.
/// Provides common functionality for data objects that have identity.
/// </summary>
/// <typeparam name="TKey">The type of the entity's primary key</typeparam>
public abstract class EntityBase<TKey> : IEntity<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The unique identifier for this entity
    /// </summary>
    public virtual TKey Id { get; set; } = default!;
    
    /// <summary>
    /// When this entity was created
    /// </summary>
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this entity was last modified
    /// </summary>
    public virtual DateTime? ModifiedAt { get; set; }
    
    /// <summary>
    /// Version/timestamp for optimistic concurrency
    /// </summary>
    public virtual byte[]? Version { get; set; }
    
    /// <summary>
    /// Whether this entity has been deleted (soft delete)
    /// </summary>
    public virtual bool IsDeleted { get; set; }
    
    /// <summary>
    /// When this entity was deleted (for soft delete tracking)
    /// </summary>
    public virtual DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Additional metadata for this entity
    /// </summary>
    public virtual Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Checks if this entity has a valid identifier
    /// </summary>
    public virtual bool HasValidId => Id != null && !Id.Equals(default(TKey));
    
    /// <summary>
    /// Marks this entity as modified
    /// </summary>
    public virtual void MarkAsModified()
    {
        ModifiedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marks this entity as deleted (soft delete)
    /// </summary>
    public virtual void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        MarkAsModified();
    }
    
    /// <summary>
    /// Restores a soft-deleted entity
    /// </summary>
    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        MarkAsModified();
    }
    
    /// <summary>
    /// Equality comparison based on Id
    /// </summary>
    public virtual bool Equals(IEntity<TKey>? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (!HasValidId || !other.HasValidId) return false;
        return Id.Equals(other.Id) && GetType() == other.GetType();
    }
    
    /// <summary>
    /// Equality comparison
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is IEntity<TKey> entity && Equals(entity);
    }
    
    /// <summary>
    /// Hash code based on Id and type
    /// </summary>
    public override int GetHashCode()
    {
        if (!HasValidId) return base.GetHashCode();
        return HashCode.Combine(Id, GetType());
    }
    
    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return $"{GetType().Name}(Id: {Id})";
    }
    
    /// <summary>
    /// Equality operators
    /// </summary>
    public static bool operator ==(EntityBase<TKey>? left, EntityBase<TKey>? right)
    {
        return EqualityComparer<EntityBase<TKey>>.Default.Equals(left, right);
    }
    
    /// <summary>
    /// Inequality operators
    /// </summary>
    public static bool operator !=(EntityBase<TKey>? left, EntityBase<TKey>? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Base class for entities with string identifiers
/// </summary>
public abstract class EntityBase : EntityBase<string>
{
    /// <summary>
    /// Generates a new GUID-based identifier
    /// </summary>
    public virtual void GenerateId()
    {
        if (string.IsNullOrEmpty(Id))
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}

/// <summary>
/// Interface for entities with identity
/// </summary>
/// <typeparam name="TKey">The type of the entity's primary key</typeparam>
public interface IEntity<TKey> : IEquatable<IEntity<TKey>> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The unique identifier for this entity
    /// </summary>
    TKey Id { get; set; }
    
    /// <summary>
    /// When this entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When this entity was last modified
    /// </summary>
    DateTime? ModifiedAt { get; set; }
    
    /// <summary>
    /// Whether this entity has been deleted (soft delete)
    /// </summary>
    bool IsDeleted { get; set; }
    
    /// <summary>
    /// Checks if this entity has a valid identifier
    /// </summary>
    bool HasValidId { get; }
}

/// <summary>
/// Interface for entities with string identifiers
/// </summary>
public interface IEntity : IEntity<string>
{
}