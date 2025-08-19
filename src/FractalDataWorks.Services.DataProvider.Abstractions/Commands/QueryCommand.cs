using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Commands;

/// <summary>
/// Represents a provider-agnostic query command for retrieving data records.
/// </summary>
/// <typeparam name="TEntity">The type of entity to query.</typeparam>
public sealed class QueryCommand<TEntity> : DataCommandBase<IEnumerable<TEntity>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="predicate">The filter predicate expression.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="orderBy">Optional ordering expression.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    public QueryCommand(
        string connectionName,
        Expression<Func<TEntity, bool>>? predicate = null,
        DataPath? targetContainer = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Query", connectionName, targetContainer, parameters, metadata, timeout)
    {
        Predicate = predicate;
        OrderBy = orderBy;
    }

    /// <summary>
    /// Gets the filter predicate expression.
    /// </summary>
    public Expression<Func<TEntity, bool>>? Predicate { get; }

    /// <summary>
    /// Gets the ordering expression.
    /// </summary>
    public Expression<Func<TEntity, object>>? OrderBy { get; }

    /// <inheritdoc/>
    public override bool IsDataModifying => false;

    /// <summary>
    /// Creates a new QueryCommand with a predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>A new QueryCommand instance with the specified predicate.</returns>
    public QueryCommand<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        
        return new QueryCommand<TEntity>(
            ConnectionName, 
            predicate, 
            TargetContainer, 
            OrderBy, 
            Parameters, 
            Metadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new QueryCommand with ordering.
    /// </summary>
    /// <param name="orderBy">The ordering expression.</param>
    /// <returns>A new QueryCommand instance with the specified ordering.</returns>
    public QueryCommand<TEntity> OrderByField(Expression<Func<TEntity, object>> orderBy)
    {
        if (orderBy == null)
            throw new ArgumentNullException(nameof(orderBy));
        
        return new QueryCommand<TEntity>(
            ConnectionName, 
            Predicate, 
            TargetContainer, 
            orderBy, 
            Parameters, 
            Metadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new QueryCommand with paging support.
    /// </summary>
    /// <param name="offset">Number of records to skip.</param>
    /// <param name="limit">Maximum number of records to return.</param>
    /// <returns>A new QueryCommand instance with paging.</returns>
    public QueryCommand<TEntity> Skip(int offset, int limit)
    {
        if (offset < 0)
            throw new ArgumentException("Offset cannot be negative.", nameof(offset));
        
        if (limit <= 0)
            throw new ArgumentException("Limit must be positive.", nameof(limit));
        
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["Offset"] = offset,
            ["Limit"] = limit,
            ["Paged"] = true
        };
        
        return new QueryCommand<TEntity>(
            ConnectionName, 
            Predicate, 
            TargetContainer, 
            OrderBy, 
            Parameters, 
            newMetadata, 
            Timeout);
    }

    /// <summary>
    /// Creates a new QueryCommand that returns only the first record.
    /// </summary>
    /// <returns>A new QueryCommand instance configured for single result.</returns>
    public QueryCommand<TEntity> First()
    {
        var newMetadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal)
        {
            ["SingleResult"] = true,
            ["Limit"] = 1
        };
        
        return new QueryCommand<TEntity>(
            ConnectionName, 
            Predicate, 
            TargetContainer, 
            OrderBy, 
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
        return new QueryCommand<TEntity>(
            connectionName,
            Predicate,
            targetContainer,
            OrderBy,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the query command.
    /// </summary>
    /// <returns>A string describing the query command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var predicateInfo = Predicate != null ? " with filter" : "";
        var orderInfo = OrderBy != null ? " ordered" : "";
        var target = TargetContainer != null ? $" from {TargetContainer}" : $" from {entityName}";
        
        return $"Query<{entityName}>({ConnectionName}){target}{predicateInfo}{orderInfo}";
    }
}

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

/// <summary>
/// Represents a provider-agnostic query command for checking existence of records.
/// </summary>
/// <typeparam name="TEntity">The type of entity to check.</typeparam>
public sealed class ExistsCommand<TEntity> : DataCommandBase<bool>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExistsCommand{TEntity}"/> class.
    /// </summary>
    /// <param name="connectionName">The named connection to execute against.</param>
    /// <param name="predicate">The filter predicate expression.</param>
    /// <param name="targetContainer">The target container path.</param>
    /// <param name="parameters">Additional parameters.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    public ExistsCommand(
        string connectionName,
        Expression<Func<TEntity, bool>> predicate,
        DataPath? targetContainer = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Exists", connectionName, targetContainer, parameters, metadata, timeout)
    {
        Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    /// <summary>
    /// Gets the filter predicate expression.
    /// </summary>
    public Expression<Func<TEntity, bool>> Predicate { get; }

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
        return new ExistsCommand<TEntity>(
            connectionName,
            Predicate,
            targetContainer,
            parameters,
            metadata,
            timeout);
    }

    /// <summary>
    /// Returns a string representation of the exists command.
    /// </summary>
    /// <returns>A string describing the exists command.</returns>
    public override string ToString()
    {
        var entityName = typeof(TEntity).Name;
        var target = TargetContainer != null ? $" in {TargetContainer}" : $" in {entityName}";
        
        return $"Exists<{entityName}>({ConnectionName}){target}";
    }
}