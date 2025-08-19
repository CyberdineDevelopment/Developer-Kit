using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Commands;

/// <summary>
/// Represents a provider-agnostic delete command for removing data records.
/// </summary>
/// <typeparam name="TEntity">The type of entity to delete.</typeparam>
public sealed class DeleteCommand<TEntity> : DataCommandBase<int>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="predicate">The predicate to identify records to delete.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentNullException">Thrown when predicate is null.</exception>
    public DeleteCommand(
        string connectionName,
        Expression<Func<TEntity, bool>> predicate,
        DataPath? targetContainer = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Delete", connectionName, targetContainer, parameters, metadata, timeout)
    {
        Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    /// <summary>
    /// Gets the predicate to identify records to delete.
    /// </summary>
    public Expression<Func<TEntity, bool>> Predicate { get; }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <summary>
    /// Creates a new DeleteCommand that limits the number of records deleted.
    /// </summary>
    /// <param name="limit">The maximum number of records to delete.</param>
    /// <returns>A new DeleteCommand instance with the specified limit.</returns>
    public DeleteCommand<TEntity> Limit(int limit)
    {
        if (limit <= 0)
            throw new ArgumentException("Limit must be positive.", nameof(limit));
        
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["Limit"] = limit
        };
        
        return new DeleteCommand<TEntity>(
            ConnectionName, 
            Predicate, 
            TargetContainer, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new DeleteCommand configured for soft deletion.
    /// </summary>
    /// <param name="deletedField">The field name to mark as deleted (e.g., "IsDeleted").</param>
    /// <param name="deletedValue">The value to set for the deleted field (default: true).</param>
    /// <returns>A new DeleteCommand instance configured for soft deletion.</returns>
    public DeleteCommand<TEntity> SoftDelete(string deletedField = "IsDeleted", object? deletedValue = null)
    {
        if (string.IsNullOrWhiteSpace(deletedField))
            throw new ArgumentException("Deleted field name cannot be null or empty.", nameof(deletedField));
        
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["SoftDelete"] = true,
            ["DeletedField"] = deletedField,
            ["DeletedValue"] = deletedValue ?? true
        };
        
        return new DeleteCommand<TEntity>(
            ConnectionName, 
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
        return new DeleteCommand<TEntity>(
            connectionName,
            Predicate,
            targetContainer,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the delete command.
    /// </summary>
    /// <returns>A string describing the delete command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var target = TargetContainer != null ? $" from {TargetContainer}" : $" from {entityName}";
        var softDelete = TryGetMetadata<bool>("SoftDelete", out var isSoft) && isSoft ? " (soft)" : "";
        
        return $"Delete<{entityName}>({ConnectionName}){target} with predicate{softDelete}";
    }
}