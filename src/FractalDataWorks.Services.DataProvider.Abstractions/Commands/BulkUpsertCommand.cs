using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Commands;

/// <summary>
/// Represents a provider-agnostic bulk upsert command for efficiently upserting multiple records.
/// </summary>
/// <typeparam name="TEntity">The type of entity to upsert.</typeparam>
public sealed class BulkUpsertCommand<TEntity> : DataCommandBase<int>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BulkUpsertCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="entities">The entities to upsert.</param>
    /// <param name="conflictFields">The fields used to detect conflicts.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="batchSize">The batch size for bulk operations.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when entities or conflictFields is empty, or batchSize is invalid.</exception>
    public BulkUpsertCommand(
        string connectionName,
        IEnumerable<TEntity> entities,
        IEnumerable<string> conflictFields,
        DataPath? targetContainer = null,
        int batchSize = 1000,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("BulkUpsert", connectionName, targetContainer, parameters, metadata, timeout)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));
        
        Entities = entities.ToList().AsReadOnly();
        
        if (Entities.Count == 0)
            throw new ArgumentException("Entities collection cannot be empty.", nameof(entities));
        
        if (conflictFields == null)
            throw new ArgumentNullException(nameof(conflictFields));
        
        ConflictFields = conflictFields.ToList().AsReadOnly();
        
        if (ConflictFields.Count == 0)
            throw new ArgumentException("Conflict fields cannot be empty.", nameof(conflictFields));
        
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive.", nameof(batchSize));
        
        BatchSize = batchSize;
    }

    /// <summary>
    /// Gets the entities to upsert.
    /// </summary>
    public IReadOnlyList<TEntity> Entities { get; }

    /// <summary>
    /// Gets the fields used to detect conflicts.
    /// </summary>
    public IReadOnlyList<string> ConflictFields { get; }

    /// <summary>
    /// Gets the batch size for bulk operations.
    /// </summary>
    public int BatchSize { get; }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <summary>
    /// Creates a new BulkUpsertCommand that only updates specific fields on conflict.
    /// </summary>
    /// <param name="updateFields">The fields to update when a conflict is detected.</param>
    /// <returns>A new BulkUpsertCommand instance with selective updates.</returns>
    public BulkUpsertCommand<TEntity> OnConflictUpdate(params string[] updateFields)
    {
        if (updateFields == null || updateFields.Length == 0)
            throw new ArgumentException("Update fields cannot be null or empty.", nameof(updateFields));
        
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["OnConflictUpdate"] = updateFields.ToList()
        };
        
        return new BulkUpsertCommand<TEntity>(
            ConnectionName, 
            Entities, 
            ConflictFields, 
            TargetContainer, 
            BatchSize, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new BulkUpsertCommand that ignores conflicts.
    /// </summary>
    /// <returns>A new BulkUpsertCommand instance that ignores conflicts.</returns>
    public BulkUpsertCommand<TEntity> OnConflictIgnore()
    {
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["OnConflictIgnore"] = true
        };
        
        return new BulkUpsertCommand<TEntity>(
            ConnectionName, 
            Entities, 
            ConflictFields, 
            TargetContainer, 
            BatchSize, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new BulkUpsertCommand with a different batch size.
    /// </summary>
    /// <param name="batchSize">The new batch size.</param>
    /// <returns>A new BulkUpsertCommand instance with the specified batch size.</returns>
    public BulkUpsertCommand<TEntity> WithBatchSize(int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive.", nameof(batchSize));
        
        return new BulkUpsertCommand<TEntity>(
            ConnectionName, 
            Entities, 
            ConflictFields, 
            TargetContainer, 
            batchSize, 
            Parameters, 
            Metadata, 
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
        return new BulkUpsertCommand<TEntity>(
            connectionName,
            Entities,
            ConflictFields,
            targetContainer,
            BatchSize,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the bulk upsert command.
    /// </summary>
    /// <returns>A string describing the bulk upsert command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var target = TargetContainer != null ? $" in {TargetContainer}" : $" in {entityName}";
        var conflictInfo = $" on [{string.Join(", ", ConflictFields)}]";
        
        return $"BulkUpsert<{entityName}>({ConnectionName}){target}{conflictInfo} - {Entities.Count} entities, batch size {BatchSize}";
    }
}