using System;
using System.Collections.Generic;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Commands;

/// <summary>
/// Represents a provider-agnostic insert command for adding new data records.
/// </summary>
/// <typeparam name="TEntity">The type of entity to insert.</typeparam>
public sealed class InsertCommand<TEntity> : DataCommandBase<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InsertCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    public InsertCommand(
        string connectionName,
        TEntity entity,
        DataPath? targetContainer = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Insert", connectionName, targetContainer, parameters, metadata, timeout)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
    }

    /// <summary>
    /// Gets the entity to insert.
    /// </summary>
    public TEntity Entity { get; }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <summary>
    /// Creates a new InsertCommand that returns the inserted identity/key value.
    /// </summary>
    /// <returns>A new InsertCommand instance configured to return the identity.</returns>
    public InsertCommand<TEntity> ReturnIdentity()
    {
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["ReturnIdentity"] = true
        };
        
        return new InsertCommand<TEntity>(
            ConnectionName, 
            Entity, 
            TargetContainer, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new InsertCommand that ignores duplicate key violations.
    /// </summary>
    /// <returns>A new InsertCommand instance configured to ignore duplicates.</returns>
    public InsertCommand<TEntity> IgnoreDuplicates()
    {
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["IgnoreDuplicates"] = true
        };
        
        return new InsertCommand<TEntity>(
            ConnectionName, 
            Entity, 
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
        return new InsertCommand<TEntity>(
            connectionName,
            Entity,
            targetContainer,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the insert command.
    /// </summary>
    /// <returns>A string describing the insert command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var target = TargetContainer != null ? $" into {TargetContainer}" : $" into {entityName}";
        
        return $"Insert<{entityName}>({ConnectionName}){target}";
    }
}