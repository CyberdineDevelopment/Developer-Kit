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