using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Commands;

/// <summary>
/// Represents a provider-agnostic update command for modifying existing data records.
/// </summary>
/// <typeparam name="TEntity">The type of entity to update.</typeparam>
public sealed class UpdateCommand<TEntity> : DataCommandBase<int>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="predicate">The predicate to identify records to update.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public UpdateCommand(
        string connectionName,
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate,
        DataPath? targetContainer = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Update", connectionName, targetContainer, parameters, metadata, timeout)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    /// <summary>
    /// Gets the entity with updated values.
    /// </summary>
    public TEntity Entity { get; }

    /// <summary>
    /// Gets the predicate to identify records to update.
    /// </summary>
    public Expression<Func<TEntity, bool>> Predicate { get; }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <summary>
    /// Creates a new UpdateCommand that limits the number of records updated.
    /// </summary>
    /// <param name="limit">The maximum number of records to update.</param>
    /// <returns>A new UpdateCommand instance with the specified limit.</returns>
    public UpdateCommand<TEntity> Limit(int limit)
    {
        if (limit <= 0)
            throw new ArgumentException("Limit must be positive.", nameof(limit));
        
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["Limit"] = limit
        };
        
        return new UpdateCommand<TEntity>(
            ConnectionName, 
            Entity, 
            Predicate, 
            TargetContainer, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <inheritdoc/>
    protected override DataCommandBase CreateCopy(
        string connectionName,
        DataPath? targetContainer,
        IReadOnlyDictionary<string, object?> parameters,
        IReadOnlyDictionary<string, object> metadata,
        TimeSpan? timeout)
    {
        return new UpdateCommand<TEntity>(
            connectionName,
            Entity,
            Predicate,
            targetContainer,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the update command.
    /// </summary>
    /// <returns>A string describing the update command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var target = TargetContainer != null ? $" in {TargetContainer}" : $" in {entityName}";
        
        return $"Update<{entityName}>({ConnectionName}){target} with predicate";
    }
}

/// <summary>
/// Represents a provider-agnostic partial update command for modifying specific fields.
/// </summary>
/// <typeparam name="TEntity">The type of entity to update.</typeparam>
public sealed class PartialUpdateCommand<TEntity> : DataCommandBase<int>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartialUpdateCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="updates">The field updates to apply.</param>
    /// <param name="predicate">The predicate to identify records to update.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when updates collection is empty.</exception>
    public PartialUpdateCommand(
        string connectionName,
        IDictionary<string, object?> updates,
        Expression<Func<TEntity, bool>> predicate,
        DataPath? targetContainer = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("PartialUpdate", connectionName, targetContainer, parameters, metadata, timeout)
    {
        if (updates == null)
            throw new ArgumentNullException(nameof(updates));
        
        if (updates.Count == 0)
            throw new ArgumentException("Updates collection cannot be empty.", nameof(updates));
        
        Updates = new Dictionary<string, object?>(updates, StringComparer.OrdinalIgnoreCase);
        Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    /// <summary>
    /// Gets the field updates to apply.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Updates { get; }

    /// <summary>
    /// Gets the predicate to identify records to update.
    /// </summary>
    public Expression<Func<TEntity, bool>> Predicate { get; }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <summary>
    /// Creates a new PartialUpdateCommand with an additional field update.
    /// </summary>
    /// <param name="fieldName">The field name to update.</param>
    /// <param name="value">The new value for the field.</param>
    /// <returns>A new PartialUpdateCommand instance with the additional update.</returns>
    public PartialUpdateCommand<TEntity> Set(string fieldName, object? value)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("Field name cannot be null or empty.", nameof(fieldName));
        
        var newUpdates = new Dictionary<string, object?>(Updates, StringComparer.OrdinalIgnoreCase)
        {
            [fieldName] = value
        };
        
        return new PartialUpdateCommand<TEntity>(
            ConnectionName, 
            newUpdates, 
            Predicate, 
            TargetContainer, 
            Parameters, 
            Metadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new PartialUpdateCommand that limits the number of records updated.
    /// </summary>
    /// <param name="limit">The maximum number of records to update.</param>
    /// <returns>A new PartialUpdateCommand instance with the specified limit.</returns>
    public PartialUpdateCommand<TEntity> Limit(int limit)
    {
        if (limit <= 0)
            throw new ArgumentException("Limit must be positive.", nameof(limit));
        
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["Limit"] = limit
        };
        
        return new PartialUpdateCommand<TEntity>(
            ConnectionName, 
            new Dictionary<string, object?>(Updates, StringComparer.OrdinalIgnoreCase), 
            Predicate, 
            TargetContainer, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <inheritdoc/>
    protected override DataCommandBase CreateCopy(
        string connectionName,
        DataPath? targetContainer,
        IReadOnlyDictionary<string, object?> parameters,
        IReadOnlyDictionary<string, object> metadata,
        TimeSpan? timeout)
    {
        return new PartialUpdateCommand<TEntity>(
            connectionName,
            new Dictionary<string, object?>(Updates, StringComparer.OrdinalIgnoreCase),
            Predicate,
            targetContainer,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the partial update command.
    /// </summary>
    /// <returns>A string describing the partial update command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var target = TargetContainer != null ? $" in {TargetContainer}" : $" in {entityName}";
        var fieldCount = Updates.Count;
        
        return $"PartialUpdate<{entityName}>({ConnectionName}){target} - {fieldCount} fields with predicate";
    }
}