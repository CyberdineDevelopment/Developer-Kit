using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Commands;

/// <summary>
/// Represents a provider-agnostic bulk insert command for adding multiple data records efficiently.
/// </summary>
/// <typeparam name="TEntity">The type of entity to insert.</typeparam>
public sealed class BulkInsertCommand<TEntity> : DataCommandBase<int>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BulkInsertCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="batchSize">The batch size for bulk operations.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentNullException">Thrown when entities is null.</exception>
    /// <exception cref="ArgumentException">Thrown when entities is empty or batchSize is invalid.</exception>
    public BulkInsertCommand(
        string connectionName,
        IEnumerable<TEntity> entities,
        DataPath? targetContainer = null,
        int batchSize = 1000,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("BulkInsert", connectionName, targetContainer, parameters, metadata, timeout)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));
        
        Entities = entities.ToList().AsReadOnly();
        
        if (Entities.Count == 0)
            throw new ArgumentException("Entities collection cannot be empty.", nameof(entities));
        
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive.", nameof(batchSize));
        
        BatchSize = batchSize;
    }

    /// <summary>
    /// Gets the entities to insert.
    /// </summary>
    public IReadOnlyList<TEntity> Entities { get; }

    /// <summary>
    /// Gets the batch size for bulk operations.
    /// </summary>
    public int BatchSize { get; }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <summary>
    /// Creates a new BulkInsertCommand that ignores duplicate key violations.
    /// </summary>
    /// <returns>A new BulkInsertCommand instance configured to ignore duplicates.</returns>
    public BulkInsertCommand<TEntity> IgnoreDuplicates()
    {
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["IgnoreDuplicates"] = true
        };
        
        return new BulkInsertCommand<TEntity>(
            ConnectionName, 
            Entities, 
            TargetContainer, 
            BatchSize, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new BulkInsertCommand with a different batch size.
    /// </summary>
    /// <param name="batchSize">The new batch size.</param>
    /// <returns>A new BulkInsertCommand instance with the specified batch size.</returns>
    public BulkInsertCommand<TEntity> WithBatchSize(int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive.", nameof(batchSize));
        
        return new BulkInsertCommand<TEntity>(
            ConnectionName, 
            Entities, 
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
        return new BulkInsertCommand<TEntity>(
            connectionName,
            Entities,
            targetContainer,
            BatchSize,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the bulk insert command.
    /// </summary>
    /// <returns>A string describing the bulk insert command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var target = TargetContainer != null ? $" into {TargetContainer}" : $" into {entityName}";
        
        return $"BulkInsert<{entityName}>({ConnectionName}){target} - {Entities.Count} entities, batch size {BatchSize}";
    }
}