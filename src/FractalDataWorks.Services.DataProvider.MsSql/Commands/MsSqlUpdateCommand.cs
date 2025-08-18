using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FractalDataWorks.Services.DataProvider.Abstractions;

namespace FractalDataWorks.Services.DataProvider.MsSql.Commands;

/// <summary>
/// Microsoft SQL Server update command for UPDATE operations.
/// </summary>
/// <typeparam name="TResult">The type of result expected from the update operation (typically int for affected rows or the updated entity).</typeparam>
public sealed class MsSqlUpdateCommand<TResult> : MsSqlCommandBase, IDataCommand<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlUpdateCommand{TResult}"/> class.
    /// </summary>
    /// <param name="sqlText">The UPDATE SQL statement.</param>
    /// <param name="target">The target table for the update operation.</param>
    /// <param name="parameters">Update parameters containing the values and WHERE conditions.</param>
    /// <param name="metadata">Additional metadata such as schema information and optimistic concurrency settings.</param>
    /// <param name="timeout">The command timeout.</param>
    public MsSqlUpdateCommand(
        string sqlText,
        string? target = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Update", target, typeof(TResult), sqlText, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <inheritdoc/>
    protected override IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata)
    {
        return new MsSqlUpdateCommand<TResult>(SqlText, Target, newParameters, newMetadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);
        return new MsSqlUpdateCommand<TResult>(SqlText, Target, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        ArgumentNullException.ThrowIfNull(newMetadata);
        return new MsSqlUpdateCommand<TResult>(SqlText, Target, Parameters, newMetadata, Timeout);
    }
}

/// <summary>
/// Non-generic Microsoft SQL Server update command for UPDATE operations.
/// </summary>
public sealed class MsSqlUpdateCommand : MsSqlCommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlUpdateCommand"/> class.
    /// </summary>
    /// <param name="sqlText">The UPDATE SQL statement.</param>
    /// <param name="expectedResultType">The type of result expected from the update operation.</param>
    /// <param name="target">The target table for the update operation.</param>
    /// <param name="parameters">Update parameters containing the values and WHERE conditions.</param>
    /// <param name="metadata">Additional metadata such as schema information and optimistic concurrency settings.</param>
    /// <param name="timeout">The command timeout.</param>
    public MsSqlUpdateCommand(
        string sqlText,
        Type expectedResultType,
        string? target = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Update", target, expectedResultType, sqlText, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <inheritdoc/>
    protected override IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata)
    {
        return new MsSqlUpdateCommand(SqlText, ExpectedResultType, Target, newParameters, newMetadata, Timeout);
    }

    /// <summary>
    /// Creates a new update command for a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <returns>A strongly-typed update command.</returns>
    public MsSqlUpdateCommand<TResult> AsTyped<TResult>()
    {
        return new MsSqlUpdateCommand<TResult>(SqlText, Target, Parameters, Metadata, Timeout);
    }
}

/// <summary>
/// Factory methods for creating type-safe SQL Server update commands.
/// </summary>
public static class MsSqlUpdateCommandFactory
{
    /// <summary>
    /// Creates an update command using a type-safe predicate for WHERE conditions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="updateValues">The entity with updated values.</param>
    /// <param name="whereCondition">The type-safe WHERE condition.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="excludeProperties">Properties to exclude from the update.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An update command with type-safe conditions.</returns>
    public static MsSqlUpdateCommand<int> UpdateWhere<TEntity>(
        TEntity updateValues,
        Expression<Func<TEntity, bool>> whereCondition,
        string? schema = null,
        Expression<Func<TEntity, object>>[]? excludeProperties = null,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(updateValues);
        ArgumentNullException.ThrowIfNull(whereCondition);

        var excludeNames = ExtractPropertyNames(excludeProperties);
        var (whereClause, whereParameters) = MsSqlCommandBase.ConvertPredicateToSql(whereCondition);
        
        var updateableProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .Where(p => !excludeNames.Contains(p.Name))
            .ToList();

        var setClauses = updateableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = @Set_{p.Name}");
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"UPDATE {tableRef} SET {string.Join(", ", setClauses)} WHERE {whereClause}";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        
        // Add update parameters with Set_ prefix to avoid conflicts
        foreach (var property in updateableProperties)
        {
            var value = property.GetValue(updateValues);
            parameters[$"Set_{property.Name}"] = value;
        }

        // Add WHERE parameters
        foreach (var param in whereParameters)
        {
            parameters[param.Key] = param.Value;
        }

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlUpdateCommand<int>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates an update command for a specific entity by ID using type-safe property mapping.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="idProperty">Expression pointing to the ID property.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="excludeProperties">Properties to exclude from the update.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An update command for the specified entity.</returns>
    public static MsSqlUpdateCommand<int> UpdateById<TEntity, TId>(
        TEntity entity,
        Expression<Func<TEntity, TId>> idProperty,
        string? schema = null,
        Expression<Func<TEntity, object>>[]? excludeProperties = null,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(idProperty);

        var idPropertyName = ExtractPropertyName(idProperty);
        var excludeNames = ExtractPropertyNames(excludeProperties);
        excludeNames.Add(idPropertyName); // Always exclude the ID column from SET clause
        
        var updateableProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .Where(p => !excludeNames.Contains(p.Name))
            .ToList();

        var idPropertyInfo = typeof(TEntity).GetProperty(idPropertyName) ??
            throw new ArgumentException($"Property {idPropertyName} not found on entity.");

        var setClauses = updateableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = @{p.Name}");
        var idColumnName = MsSqlCommandBase.EscapeIdentifier(idPropertyName);
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"UPDATE {tableRef} SET {string.Join(", ", setClauses)} WHERE [{idColumnName}] = @{idPropertyName}";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        
        // Add update parameters
        foreach (var property in updateableProperties)
        {
            var value = property.GetValue(entity);
            parameters[property.Name] = value;
        }

        // Add ID parameter for WHERE clause
        var idValue = idPropertyInfo.GetValue(entity);
        parameters[idPropertyName] = idValue;

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlUpdateCommand<int>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates an update command that returns the updated entity using type-safe expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="idProperty">Expression pointing to the ID property.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="excludeProperties">Properties to exclude from the update.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An update command that returns the updated entity.</returns>
    public static MsSqlUpdateCommand<TEntity> UpdateByIdReturnEntity<TEntity, TId>(
        TEntity entity,
        Expression<Func<TEntity, TId>> idProperty,
        string? schema = null,
        Expression<Func<TEntity, object>>[]? excludeProperties = null,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(idProperty);

        var idPropertyName = ExtractPropertyName(idProperty);
        var excludeNames = ExtractPropertyNames(excludeProperties);
        excludeNames.Add(idPropertyName); // Always exclude the ID column from SET clause
        
        var updateableProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .Where(p => !excludeNames.Contains(p.Name))
            .ToList();

        var idPropertyInfo = typeof(TEntity).GetProperty(idPropertyName) ??
            throw new ArgumentException($"Property {idPropertyName} not found on entity.");

        var setClauses = updateableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = @{p.Name}");
        var idColumnName = MsSqlCommandBase.EscapeIdentifier(idPropertyName);
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"UPDATE {tableRef} SET {string.Join(", ", setClauses)} OUTPUT INSERTED.* WHERE [{idColumnName}] = @{idPropertyName}";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        
        // Add update parameters
        foreach (var property in updateableProperties)
        {
            var value = property.GetValue(entity);
            parameters[property.Name] = value;
        }

        // Add ID parameter for WHERE clause
        var idValue = idPropertyInfo.GetValue(entity);
        parameters[idPropertyName] = idValue;

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["ReturnUpdated"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlUpdateCommand<TEntity>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates an update command with selective property updates using type-safe expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity containing the values to update.</param>
    /// <param name="updateProperties">Expressions selecting which properties to update.</param>
    /// <param name="whereCondition">The type-safe WHERE condition.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="returnUpdated">Whether to return the updated entities.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An update command with selective property updates.</returns>
    public static MsSqlUpdateCommand<TResult> UpdateProperties<TEntity, TResult>(
        TEntity entity,
        Expression<Func<TEntity, object>>[] updateProperties,
        Expression<Func<TEntity, bool>> whereCondition,
        string? schema = null,
        bool returnUpdated = false,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(updateProperties);
        ArgumentNullException.ThrowIfNull(whereCondition);

        if (updateProperties.Length == 0)
        {
            throw new ArgumentException("At least one property to update must be specified.", nameof(updateProperties));
        }

        var updatePropertyNames = ExtractPropertyNames(updateProperties);
        var selectedProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .Where(p => updatePropertyNames.Contains(p.Name))
            .ToList();

        var (whereClause, whereParameters) = MsSqlCommandBase.ConvertPredicateToSql(whereCondition);
        
        var setClauses = selectedProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = @Set_{p.Name}");
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"UPDATE {tableRef} SET {string.Join(", ", setClauses)}";
        
        if (returnUpdated)
        {
            sql += " OUTPUT INSERTED.*";
        }
        
        sql += $" WHERE {whereClause}";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        
        // Add SET parameters with prefix to avoid conflicts
        foreach (var property in selectedProperties)
        {
            var value = property.GetValue(entity);
            parameters[$"Set_{property.Name}"] = value;
        }

        // Add WHERE parameters
        foreach (var param in whereParameters)
        {
            parameters[param.Key] = param.Value;
        }

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["SelectiveUpdate"] = true
        };
        
        if (returnUpdated)
        {
            metadata["ReturnUpdated"] = true;
        }
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlUpdateCommand<TResult>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates an update command with optimistic concurrency using type-safe property expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="idProperty">Expression pointing to the ID property.</param>
    /// <param name="versionProperty">Expression pointing to the version/timestamp property.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="excludeProperties">Properties to exclude from the update.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An update command with optimistic concurrency control.</returns>
    public static MsSqlUpdateCommand<int> UpdateWithConcurrency<TEntity, TId, TVersion>(
        TEntity entity,
        Expression<Func<TEntity, TId>> idProperty,
        Expression<Func<TEntity, TVersion>> versionProperty,
        string? schema = null,
        Expression<Func<TEntity, object>>[]? excludeProperties = null,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(idProperty);
        ArgumentNullException.ThrowIfNull(versionProperty);

        var idPropertyName = ExtractPropertyName(idProperty);
        var versionPropertyName = ExtractPropertyName(versionProperty);
        var excludeNames = ExtractPropertyNames(excludeProperties);
        
        excludeNames.Add(idPropertyName); // Always exclude the ID column from SET clause
        excludeNames.Add(versionPropertyName); // Exclude version column from SET clause
        
        var updateableProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .Where(p => !excludeNames.Contains(p.Name))
            .ToList();

        var idPropertyInfo = typeof(TEntity).GetProperty(idPropertyName) ??
            throw new ArgumentException($"Property {idPropertyName} not found on entity.");
        var versionPropertyInfo = typeof(TEntity).GetProperty(versionPropertyName) ??
            throw new ArgumentException($"Property {versionPropertyName} not found on entity.");

        var setClauses = updateableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = @{p.Name}");
        var idColumnName = MsSqlCommandBase.EscapeIdentifier(idPropertyName);
        var versionColumnName = MsSqlCommandBase.EscapeIdentifier(versionPropertyName);
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"UPDATE {tableRef} SET {string.Join(", ", setClauses)} WHERE [{idColumnName}] = @{idPropertyName} AND [{versionColumnName}] = @{versionPropertyName}";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        
        // Add update parameters
        foreach (var property in updateableProperties)
        {
            var value = property.GetValue(entity);
            parameters[property.Name] = value;
        }

        // Add ID and version parameters for WHERE clause
        var idValue = idPropertyInfo.GetValue(entity);
        var versionValue = versionPropertyInfo.GetValue(entity);
        parameters[idPropertyName] = idValue;
        parameters[versionPropertyName] = versionValue;

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

        return new MsSqlUpdateCommand<int>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Extracts property names from property selector expressions.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertySelectors">The property selector expressions.</param>
    /// <returns>Set of property names.</returns>
    private static HashSet<string> ExtractPropertyNames<T>(Expression<Func<T, object>>[]? propertySelectors)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        if (propertySelectors != null)
        {
            foreach (var selector in propertySelectors)
            {
                names.Add(ExtractPropertyName(selector));
            }
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