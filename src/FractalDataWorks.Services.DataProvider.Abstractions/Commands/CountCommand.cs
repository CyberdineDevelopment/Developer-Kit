using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Commands;

/// <summary>
/// Represents a provider-agnostic query command for counting records.
/// </summary>
/// <typeparam name="TEntity">The type of entity to count.</typeparam>
public sealed class CountCommand<TEntity> : DataCommandBase<int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="predicate">The filter predicate expression.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    public CountCommand(
        string connectionName,
        Expression<Func<TEntity, bool>>? predicate = null,
        DataPath? targetContainer = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Count", connectionName, targetContainer, parameters, metadata, timeout)
    {
        Predicate = predicate;
    }

    /// <summary>
    /// Gets the filter predicate expression.
    /// </summary>
    public Expression<Func<TEntity, bool>>? Predicate { get; }

    /// <inheritdoc/>
    public override bool IsDataModifying => false;

    /// <inheritdoc/>
    protected override DataCommandBase CreateCopy(
        string connectionName,
        DataPath? targetContainer,
        IReadOnlyDictionary<string, object?> parameters,
        IReadOnlyDictionary<string, object> metadata,
        TimeSpan? timeout)
    {
        return new CountCommand<TEntity>(
            connectionName,
            Predicate,
            targetContainer,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the count command.
    /// </summary>
    /// <returns>A string describing the count command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var predicateInfo = Predicate != null ? " with filter" : "";
        var target = TargetContainer != null ? $" from {TargetContainer}" : $" from {entityName}";
        
        return $"Count<{entityName}>({ConnectionName}){target}{predicateInfo}";
    }
}