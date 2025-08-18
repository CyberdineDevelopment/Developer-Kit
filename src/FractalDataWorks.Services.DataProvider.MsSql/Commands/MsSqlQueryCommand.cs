using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FractalDataWorks.Services.DataProvider.Abstractions;

namespace FractalDataWorks.Services.DataProvider.MsSql.Commands;

/// <summary>
/// Microsoft SQL Server query command for SELECT operations.
/// </summary>
/// <typeparam name="TResult">The type of result expected from the query.</typeparam>
public sealed class MsSqlQueryCommand<TResult> : MsSqlCommandBase, IDataCommand<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlQueryCommand{TResult}"/> class.
    /// </summary>
    /// <param name="sqlText">The SELECT SQL statement.</param>
    /// <param name="target">The target table or view being queried.</param>
    /// <param name="parameters">Query parameters for WHERE clauses and filtering.</param>
    /// <param name="metadata">Additional metadata such as schema information.</param>
    /// <param name="timeout">The query timeout.</param>
    public MsSqlQueryCommand(
        string sqlText,
        string? target = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Query", target, typeof(TResult), sqlText, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => false;

    /// <inheritdoc/>
    protected override IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata)
    {
        return new MsSqlQueryCommand<TResult>(SqlText, Target, newParameters, newMetadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);
        return new MsSqlQueryCommand<TResult>(SqlText, Target, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        ArgumentNullException.ThrowIfNull(newMetadata);
        return new MsSqlQueryCommand<TResult>(SqlText, Target, Parameters, newMetadata, Timeout);
    }
}

/// <summary>
/// Non-generic Microsoft SQL Server query command for SELECT operations.
/// </summary>
public sealed class MsSqlQueryCommand : MsSqlCommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlQueryCommand"/> class.
    /// </summary>
    /// <param name="sqlText">The SELECT SQL statement.</param>
    /// <param name="expectedResultType">The type of result expected from the query.</param>
    /// <param name="target">The target table or view being queried.</param>
    /// <param name="parameters">Query parameters for WHERE clauses and filtering.</param>
    /// <param name="metadata">Additional metadata such as schema information.</param>
    /// <param name="timeout">The query timeout.</param>
    public MsSqlQueryCommand(
        string sqlText,
        Type expectedResultType,
        string? target = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Query", target, expectedResultType, sqlText, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => false;

    /// <inheritdoc/>
    protected override IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata)
    {
        return new MsSqlQueryCommand(SqlText, ExpectedResultType, Target, newParameters, newMetadata, Timeout);
    }

    /// <summary>
    /// Creates a new query command for a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <returns>A strongly-typed query command.</returns>
    public MsSqlQueryCommand<TResult> AsTyped<TResult>()
    {
        return new MsSqlQueryCommand<TResult>(SqlText, Target, Parameters, Metadata, Timeout);
    }
}

/// <summary>
/// Factory methods for creating SQL Server query commands with type-safe expressions.
/// </summary>
public static class MsSqlQueryCommandFactory
{
    /// <summary>
    /// Creates a query command that returns entities matching the specified predicate.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="predicate">The type-safe predicate expression.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="orderBy">Optional ordering expression.</param>
    /// <param name="timeout">The query timeout.</param>
    /// <returns>A query command with type-safe filtering.</returns>
    public static MsSqlQueryCommand<IEnumerable<TEntity>> FindWhere<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        string? schema = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var (whereClause, whereParameters) = MsSqlCommandBase.ConvertPredicateToSql(predicate);
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        
        var sql = $"SELECT * FROM {tableRef} WHERE {whereClause}";
        
        if (orderBy != null)
        {
            var orderByClause = ExtractOrderByClause(orderBy);
            sql += $" ORDER BY {orderByClause}";
        }

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlQueryCommand<IEnumerable<TEntity>>(sql, typeof(TEntity).Name, whereParameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a query command that returns a single entity by ID using type-safe expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="id">The entity ID.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The query timeout.</param>
    /// <returns>A query command for finding an entity by ID.</returns>
    public static MsSqlQueryCommand<TEntity?> FindById<TEntity>(
        object id,
        string? schema = null,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(id);

        // Find the Id property on the entity
        var idProperty = typeof(TEntity).GetProperty("Id") ?? 
                        throw new ArgumentException($"Entity {typeof(TEntity).Name} does not have an 'Id' property.");

        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var idColumnName = MsSqlCommandBase.EscapeIdentifier(idProperty.Name);
        var sql = $"SELECT * FROM {tableRef} WHERE [{idColumnName}] = @Id";
        
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Id"] = id
        };

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["SingleResult"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlQueryCommand<TEntity?>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a query command that returns all entities from a table using type-safe expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="orderBy">Optional ordering expression.</param>
    /// <param name="timeout">The query timeout.</param>
    /// <returns>A query command for finding all entities.</returns>
    public static MsSqlQueryCommand<IEnumerable<TEntity>> FindAll<TEntity>(
        string? schema = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        TimeSpan? timeout = null)
    {
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"SELECT * FROM {tableRef}";
        
        if (orderBy != null)
        {
            var orderByClause = ExtractOrderByClause(orderBy);
            sql += $" ORDER BY {orderByClause}";
        }

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlQueryCommand<IEnumerable<TEntity>>(sql, typeof(TEntity).Name, null, metadata, timeout);
    }

    /// <summary>
    /// Creates a query command with paging support using type-safe expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="predicate">Optional predicate expression for filtering.</param>
    /// <param name="orderBy">Required ordering expression for paging.</param>
    /// <param name="offset">Number of rows to skip.</param>
    /// <param name="pageSize">Number of rows to take.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The query timeout.</param>
    /// <returns>A query command with paging support.</returns>
    public static MsSqlQueryCommand<IEnumerable<TEntity>> FindWithPaging<TEntity>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object>> orderBy,
        int offset,
        int pageSize,
        string? schema = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(orderBy);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"SELECT * FROM {tableRef}";
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);

        if (predicate != null)
        {
            var (whereClause, whereParameters) = MsSqlCommandBase.ConvertPredicateToSql(predicate);
            sql += $" WHERE {whereClause}";
            foreach (var param in whereParameters)
            {
                parameters[param.Key] = param.Value;
            }
        }

        var orderByClause = ExtractOrderByClause(orderBy);
        sql += $" ORDER BY {orderByClause} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        
        parameters["Offset"] = offset;
        parameters["PageSize"] = pageSize;

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["Paged"] = true,
            ["Offset"] = offset,
            ["PageSize"] = pageSize
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlQueryCommand<IEnumerable<TEntity>>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a query command that returns the count of entities matching the predicate.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="predicate">Optional predicate expression for filtering.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The query timeout.</param>
    /// <returns>A query command that returns the count.</returns>
    public static MsSqlQueryCommand<int> Count<TEntity>(
        Expression<Func<TEntity, bool>>? predicate = null,
        string? schema = null,
        TimeSpan? timeout = null)
    {
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"SELECT COUNT(*) FROM {tableRef}";
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);

        if (predicate != null)
        {
            var (whereClause, whereParameters) = MsSqlCommandBase.ConvertPredicateToSql(predicate);
            sql += $" WHERE {whereClause}";
            foreach (var param in whereParameters)
            {
                parameters[param.Key] = param.Value;
            }
        }

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["CountQuery"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlQueryCommand<int>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a query command that checks if any entities match the predicate.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="predicate">The predicate expression for filtering.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The query timeout.</param>
    /// <returns>A query command that returns whether any entities exist.</returns>
    public static MsSqlQueryCommand<bool> Any<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        string? schema = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var (whereClause, whereParameters) = MsSqlCommandBase.ConvertPredicateToSql(predicate);
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"SELECT CASE WHEN EXISTS (SELECT 1 FROM {tableRef} WHERE {whereClause}) THEN 1 ELSE 0 END";

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["ExistsQuery"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlQueryCommand<bool>(sql, typeof(TEntity).Name, whereParameters, metadata, timeout);
    }

    /// <summary>
    /// Extracts the ORDER BY clause from an ordering expression.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="orderBy">The ordering expression.</param>
    /// <returns>The ORDER BY clause.</returns>
    private static string ExtractOrderByClause<TEntity>(Expression<Func<TEntity, object>> orderBy)
    {
        if (orderBy.Body is MemberExpression memberExpr && memberExpr.Member is PropertyInfo property)
        {
            return $"[{MsSqlCommandBase.EscapeIdentifier(property.Name)}]";
        }
        
        if (orderBy.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression innerMember && innerMember.Member is PropertyInfo innerProperty)
        {
            return $"[{MsSqlCommandBase.EscapeIdentifier(innerProperty.Name)}]";
        }
        
        throw new NotSupportedException("Only simple property expressions are supported for ordering.");
    }
}