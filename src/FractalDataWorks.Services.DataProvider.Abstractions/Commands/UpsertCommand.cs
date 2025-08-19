using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Commands;

/// <summary>
/// Represents a provider-agnostic upsert command for inserting or updating data records.
/// </summary>
/// <typeparam name="TEntity">The type of entity to upsert.</typeparam>
/// <remarks>
/// Upsert (INSERT or UPDATE) commands provide an atomic operation that either inserts
/// a new record if it doesn't exist or updates an existing record if it does exist.
/// The conflict resolution is typically based on primary key or unique constraints.
/// </remarks>
public sealed class UpsertCommand<TEntity> : DataCommandBase<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpsertCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="conflictFields">The fields used to detect conflicts (e.g., primary key, unique constraints).</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when conflictFields is empty.</exception>
    public UpsertCommand(
        string connectionName,
        TEntity entity,
        IEnumerable<string> conflictFields,
        DataPath? targetContainer = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Upsert", connectionName, targetContainer, parameters, metadata, timeout)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        
        if (conflictFields == null)
            throw new ArgumentNullException(nameof(conflictFields));
        
        ConflictFields = conflictFields.ToList().AsReadOnly();
        
        if (ConflictFields.Count == 0)
            throw new ArgumentException("Conflict fields cannot be empty.", nameof(conflictFields));
    }

    /// <summary>
    /// Gets the entity to upsert.
    /// </summary>
    public TEntity Entity { get; }

    /// <summary>
    /// Gets the fields used to detect conflicts.
    /// </summary>
    public IReadOnlyList<string> ConflictFields { get; }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <summary>
    /// Creates a new UpsertCommand that only updates specific fields on conflict.
    /// </summary>
    /// <param name="updateFields">The fields to update when a conflict is detected.</param>
    /// <returns>A new UpsertCommand instance with selective updates.</returns>
    public UpsertCommand<TEntity> OnConflictUpdate(params string[] updateFields)
    {
        if (updateFields == null || updateFields.Length == 0)
            throw new ArgumentException("Update fields cannot be null or empty.", nameof(updateFields));
        
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["OnConflictUpdate"] = updateFields.ToList()
        };
        
        return new UpsertCommand<TEntity>(
            ConnectionName, 
            Entity, 
            ConflictFields, 
            TargetContainer, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new UpsertCommand that ignores conflicts (INSERT only, skip on conflict).
    /// </summary>
    /// <returns>A new UpsertCommand instance that ignores conflicts.</returns>
    public UpsertCommand<TEntity> OnConflictIgnore()
    {
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["OnConflictIgnore"] = true
        };
        
        return new UpsertCommand<TEntity>(
            ConnectionName, 
            Entity, 
            ConflictFields, 
            TargetContainer, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new UpsertCommand that returns information about the operation performed.
    /// </summary>
    /// <returns>A new UpsertCommand instance configured to return operation details.</returns>
    public UpsertCommand<TEntity> ReturnOperation()
    {
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["ReturnOperation"] = true
        };
        
        return new UpsertCommand<TEntity>(
            ConnectionName, 
            Entity, 
            ConflictFields, 
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
        return new UpsertCommand<TEntity>(
            connectionName,
            Entity,
            ConflictFields,
            targetContainer,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the upsert command.
    /// </summary>
    /// <returns>A string describing the upsert command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var target = TargetContainer != null ? $" in {TargetContainer}" : $" in {entityName}";
        var conflictInfo = $" on [{string.Join(", ", ConflictFields)}]";
        
        return $"Upsert<{entityName}>({ConnectionName}){target}{conflictInfo}";
    }
}