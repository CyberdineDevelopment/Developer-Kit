using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FractalDataWorks.Services.DataProvider.Abstractions;

namespace FractalDataWorks.Services.DataProvider.MsSql.Commands;

/// <summary>
/// Microsoft SQL Server delete command for DELETE operations.
/// </summary>
/// <typeparam name="TResult">The type of result expected from the delete operation (typically int for affected rows).</typeparam>
public sealed class MsSqlDeleteCommand<TResult> : MsSqlCommandBase, IDataCommand<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlDeleteCommand{TResult}"/> class.
    /// </summary>
    /// <param name="sqlText">The DELETE SQL statement.</param>
    /// <param name="target">The target table for the delete operation.</param>
    /// <param name="parameters">Delete parameters containing the WHERE conditions.</param>
    /// <param name="metadata">Additional metadata such as schema information and safety settings.</param>
    /// <param name="timeout">The command timeout.</param>
    public MsSqlDeleteCommand(
        string sqlText,
        string? target = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Delete", target, typeof(TResult), sqlText, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <inheritdoc/>
    protected override IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata)
    {
        return new MsSqlDeleteCommand<TResult>(SqlText, Target, newParameters, newMetadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);
        return new MsSqlDeleteCommand<TResult>(SqlText, Target, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        ArgumentNullException.ThrowIfNull(newMetadata);
        return new MsSqlDeleteCommand<TResult>(SqlText, Target, Parameters, newMetadata, Timeout);
    }

    /// <inheritdoc/>
    public override IFdwResult Validate()
    {
        var baseResult = base.Validate();
        if (!baseResult.IsSuccess)
        {
            return baseResult;
        }

        // Additional validation for delete commands
        var errors = new List<string>();

        // Check for WHERE clause to prevent accidental full table deletes
        if (!SqlText.Contains("WHERE", StringComparison.OrdinalIgnoreCase) && 
            !Metadata.ContainsKey("AllowFullTableDelete"))
        {
            errors.Add("DELETE statement must include WHERE clause or explicit AllowFullTableDelete metadata.");
        }

        return errors.Count > 0 
            ? FdwResult.Failure(string.Join("; ", errors))
            : FdwResult.Success();
    }
}

/// <summary>
/// Non-generic Microsoft SQL Server delete command for DELETE operations.
/// </summary>
public sealed class MsSqlDeleteCommand : MsSqlCommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlDeleteCommand"/> class.
    /// </summary>
    /// <param name="sqlText">The DELETE SQL statement.</param>
    /// <param name="expectedResultType">The type of result expected from the delete operation.</param>
    /// <param name="target">The target table for the delete operation.</param>
    /// <param name="parameters">Delete parameters containing the WHERE conditions.</param>
    /// <param name="metadata">Additional metadata such as schema information and safety settings.</param>
    /// <param name="timeout">The command timeout.</param>
    public MsSqlDeleteCommand(
        string sqlText,
        Type expectedResultType,
        string? target = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Delete", target, expectedResultType, sqlText, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <inheritdoc/>
    protected override IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata)
    {
        return new MsSqlDeleteCommand(SqlText, ExpectedResultType, Target, newParameters, newMetadata, Timeout);
    }

    /// <summary>
    /// Creates a new delete command for a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <returns>A strongly-typed delete command.</returns>
    public MsSqlDeleteCommand<TResult> AsTyped<TResult>()
    {
        return new MsSqlDeleteCommand<TResult>(SqlText, Target, Parameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    public override IFdwResult Validate()
    {
        var baseResult = base.Validate();
        if (!baseResult.IsSuccess)
        {
            return baseResult;
        }

        // Additional validation for delete commands
        var errors = new List<string>();

        // Check for WHERE clause to prevent accidental full table deletes
        if (!SqlText.Contains("WHERE", StringComparison.OrdinalIgnoreCase) && 
            !Metadata.ContainsKey("AllowFullTableDelete"))
        {
            errors.Add("DELETE statement must include WHERE clause or explicit AllowFullTableDelete metadata.");
        }

        return errors.Count > 0 
            ? FdwResult.Failure(string.Join("; ", errors))
            : FdwResult.Success();
    }
}

/// <summary>
/// Factory methods for creating type-safe SQL Server delete commands.
/// </summary>
public static class MsSqlDeleteCommandFactory
{
    /// <summary>
    /// Creates a delete command using a type-safe predicate for WHERE conditions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="whereCondition">The type-safe WHERE condition.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>A delete command with type-safe conditions.</returns>
    public static MsSqlDeleteCommand<int> DeleteWhere<TEntity>(
        Expression<Func<TEntity, bool>> whereCondition,
        string? schema = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(whereCondition);

        var (whereClause, whereParameters) = MsSqlCommandBase.ConvertPredicateToSql(whereCondition);
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"DELETE FROM {tableRef} WHERE {whereClause}";

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlDeleteCommand<int>(sql, typeof(TEntity).Name, whereParameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a delete command for a specific entity by ID using type-safe property mapping.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The ID type.</typeparam>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="idProperty">Expression pointing to the ID property.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>A delete command for the specified entity.</returns>
    public static MsSqlDeleteCommand<int> DeleteById<TEntity, TId>(
        TId id,
        Expression<Func<TEntity, TId>> idProperty,
        string? schema = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(idProperty);

        var idPropertyName = ExtractPropertyName(idProperty);
        var idColumnName = MsSqlCommandBase.EscapeIdentifier(idPropertyName);
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"DELETE FROM {tableRef} WHERE [{idColumnName}] = @{idPropertyName}";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [idPropertyName] = id
        };

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlDeleteCommand<int>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a delete command with optimistic concurrency using type-safe property expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The ID type.</typeparam>
    /// <typeparam name="TVersion">The version type.</typeparam>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="version">The version/timestamp value for concurrency checking.</param>
    /// <param name="idProperty">Expression pointing to the ID property.</param>
    /// <param name="versionProperty">Expression pointing to the version property.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>A delete command with optimistic concurrency control.</returns>
    public static MsSqlDeleteCommand<int> DeleteByIdWithConcurrency<TEntity, TId, TVersion>(
        TId id,
        TVersion version,
        Expression<Func<TEntity, TId>> idProperty,
        Expression<Func<TEntity, TVersion>> versionProperty,
        string? schema = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(idProperty);
        ArgumentNullException.ThrowIfNull(versionProperty);

        var idPropertyName = ExtractPropertyName(idProperty);
        var versionPropertyName = ExtractPropertyName(versionProperty);
        var idColumnName = MsSqlCommandBase.EscapeIdentifier(idPropertyName);
        var versionColumnName = MsSqlCommandBase.EscapeIdentifier(versionPropertyName);
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"DELETE FROM {tableRef} WHERE [{idColumnName}] = @{idPropertyName} AND [{versionColumnName}] = @{versionPropertyName}";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [idPropertyName] = id,
            [versionPropertyName] = version
        };

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["OptimisticConcurrency"] = true,
            ["VersionColumn"] = versionPropertyName
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlDeleteCommand<int>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a delete command that returns the deleted entities using type-safe expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="whereCondition">The type-safe WHERE condition.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>A delete command that returns the deleted entities.</returns>
    public static MsSqlDeleteCommand<IEnumerable<TEntity>> DeleteReturnDeleted<TEntity>(
        Expression<Func<TEntity, bool>> whereCondition,
        string? schema = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(whereCondition);

        var (whereClause, whereParameters) = MsSqlCommandBase.ConvertPredicateToSql(whereCondition);
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"DELETE FROM {tableRef} OUTPUT DELETED.* WHERE {whereClause}";

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["ReturnDeleted"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlDeleteCommand<IEnumerable<TEntity>>(sql, typeof(TEntity).Name, whereParameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a delete command for an entity by its instance using type-safe property matching.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity instance to delete (uses key properties for WHERE conditions).</param>
    /// <param name="keyProperties">Expressions pointing to the key properties for WHERE conditions.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>A delete command for the specified entity instance.</returns>
    public static MsSqlDeleteCommand<int> DeleteEntity<TEntity>(
        TEntity entity,
        Expression<Func<TEntity, object>>[] keyProperties,
        string? schema = null,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(keyProperties);
        
        if (keyProperties.Length == 0)
        {
            throw new ArgumentException("At least one key property must be specified.", nameof(keyProperties));
        }

        var keyPropertyNames = ExtractPropertyNames(keyProperties);
        var keyPropertyInfos = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: true)
            .Where(p => keyPropertyNames.Contains(p.Name))
            .ToList();

        var whereConditions = keyPropertyInfos.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = @{p.Name}");
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"DELETE FROM {tableRef} WHERE {string.Join(" AND ", whereConditions)}";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in keyPropertyInfos)
        {
            var value = property.GetValue(entity);
            parameters[property.Name] = value;
        }

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["EntityDelete"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlDeleteCommand<int>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a delete command for multiple entities by their IDs using type-safe property mapping.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The ID type.</typeparam>
    /// <param name="ids">The IDs of the entities to delete.</param>
    /// <param name="idProperty">Expression pointing to the ID property.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>A delete command for multiple entities.</returns>
    public static MsSqlDeleteCommand<int> DeleteByIds<TEntity, TId>(
        IEnumerable<TId> ids,
        Expression<Func<TEntity, TId>> idProperty,
        string? schema = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(idProperty);

        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            throw new ArgumentException("IDs collection cannot be empty.", nameof(ids));
        }

        var idPropertyName = ExtractPropertyName(idProperty);
        var idColumnName = MsSqlCommandBase.EscapeIdentifier(idPropertyName);
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var parameterNames = idList.Select((_, index) => $"@{idPropertyName}_{index}").ToList();
        var sql = $"DELETE FROM {tableRef} WHERE [{idColumnName}] IN ({string.Join(", ", parameterNames)})";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        for (int i = 0; i < idList.Count; i++)
        {
            parameters[$"{idPropertyName}_{i}"] = idList[i];
        }

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["BulkDelete"] = true,
            ["EntityCount"] = idList.Count
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlDeleteCommand<int>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a delete command that removes all records from a table (use with extreme caution).
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>A delete command that removes all records.</returns>
    public static MsSqlDeleteCommand<int> DeleteAll<TEntity>(
        string? schema = null,
        TimeSpan? timeout = null)
    {
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"DELETE FROM {tableRef}";

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["AllowFullTableDelete"] = true,
            ["DeleteAll"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlDeleteCommand<int>(sql, typeof(TEntity).Name, null, metadata, timeout);
    }

    /// <summary>
    /// Extracts property names from property selector expressions.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertySelectors">The property selector expressions.</param>
    /// <returns>Set of property names.</returns>
    private static HashSet<string> ExtractPropertyNames<T>(Expression<Func<T, object>>[] propertySelectors)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var selector in propertySelectors)
        {
            names.Add(ExtractPropertyName(selector));
        }
        
        return names;
    }

    /// <summary>
    /// Extracts a property name from a property selector expression.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="propertySelector">The property selector expression.</param>
    /// <returns>The property name.</returns>
    private static string ExtractPropertyName<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
    {
        if (propertySelector.Body is MemberExpression memberExpr && memberExpr.Member is PropertyInfo property)
        {
            return property.Name;
        }
        
        if (propertySelector.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression innerMember && innerMember.Member is PropertyInfo innerProperty)
        {
            return innerProperty.Name;
        }
        
        throw new ArgumentException("Expression must be a property selector.", nameof(propertySelector));
    }

    /// <summary>
    /// Extracts a property name from a property selector expression.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertySelector">The property selector expression.</param>
    /// <returns>The property name.</returns>
    private static string ExtractPropertyName<T>(Expression<Func<T, object>> propertySelector)
    {
        return ExtractPropertyName<T, object>(propertySelector);
    }
}