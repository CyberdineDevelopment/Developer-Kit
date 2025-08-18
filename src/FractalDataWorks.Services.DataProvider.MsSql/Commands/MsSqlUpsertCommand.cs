using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FractalDataWorks.Services.DataProvider.Abstractions;

namespace FractalDataWorks.Services.DataProvider.MsSql.Commands;

/// <summary>
/// Microsoft SQL Server upsert command for MERGE/UPSERT operations.
/// </summary>
/// <typeparam name="TResult">The type of result expected from the upsert operation (typically the upserted entity or operation result).</typeparam>
public sealed class MsSqlUpsertCommand<TResult> : MsSqlCommandBase, IDataCommand<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlUpsertCommand{TResult}"/> class.
    /// </summary>
    /// <param name="sqlText">The MERGE SQL statement.</param>
    /// <param name="target">The target table for the upsert operation.</param>
    /// <param name="parameters">Upsert parameters containing the values and key matching conditions.</param>
    /// <param name="metadata">Additional metadata such as schema information and merge behavior settings.</param>
    /// <param name="timeout">The command timeout.</param>
    public MsSqlUpsertCommand(
        string sqlText,
        string? target = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Upsert", target, typeof(TResult), sqlText, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <inheritdoc/>
    protected override IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata)
    {
        return new MsSqlUpsertCommand<TResult>(SqlText, Target, newParameters, newMetadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);
        return new MsSqlUpsertCommand<TResult>(SqlText, Target, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        ArgumentNullException.ThrowIfNull(newMetadata);
        return new MsSqlUpsertCommand<TResult>(SqlText, Target, Parameters, newMetadata, Timeout);
    }
}

/// <summary>
/// Non-generic Microsoft SQL Server upsert command for MERGE/UPSERT operations.
/// </summary>
public sealed class MsSqlUpsertCommand : MsSqlCommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlUpsertCommand"/> class.
    /// </summary>
    /// <param name="sqlText">The MERGE SQL statement.</param>
    /// <param name="expectedResultType">The type of result expected from the upsert operation.</param>
    /// <param name="target">The target table for the upsert operation.</param>
    /// <param name="parameters">Upsert parameters containing the values and key matching conditions.</param>
    /// <param name="metadata">Additional metadata such as schema information and merge behavior settings.</param>
    /// <param name="timeout">The command timeout.</param>
    public MsSqlUpsertCommand(
        string sqlText,
        Type expectedResultType,
        string? target = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Upsert", target, expectedResultType, sqlText, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <inheritdoc/>
    protected override IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata)
    {
        return new MsSqlUpsertCommand(SqlText, ExpectedResultType, Target, newParameters, newMetadata, Timeout);
    }

    /// <summary>
    /// Creates a new upsert command for a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <returns>A strongly-typed upsert command.</returns>
    public MsSqlUpsertCommand<TResult> AsTyped<TResult>()
    {
        return new MsSqlUpsertCommand<TResult>(SqlText, Target, Parameters, Metadata, Timeout);
    }
}

/// <summary>
/// Factory methods for creating type-safe SQL Server upsert commands.
/// </summary>
public static class MsSqlUpsertCommandFactory
{
    /// <summary>
    /// Creates an upsert command for a specific entity using MERGE statement with type-safe property expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="keyProperties">Expressions pointing to the key properties used for matching.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="excludeProperties">Properties to exclude from insert/update operations.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An upsert command that returns the merged entity.</returns>
    public static MsSqlUpsertCommand<TEntity> Upsert<TEntity>(
        TEntity entity,
        Expression<Func<TEntity, object>>[] keyProperties,
        string? schema = null,
        Expression<Func<TEntity, object>>[]? excludeProperties = null,
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
        var excludePropertyNames = ExtractPropertyNames(excludeProperties);
        
        var allEntityProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false).ToList();
        var keyEntityProperties = allEntityProperties.Where(p => keyPropertyNames.Contains(p.Name)).ToList();
        var valueProperties = allEntityProperties.Where(p => !keyPropertyNames.Contains(p.Name) && !excludePropertyNames.Contains(p.Name)).ToList();
        var allProperties = keyEntityProperties.Concat(valueProperties).ToList();

        if (keyEntityProperties.Count == 0)
        {
            throw new ArgumentException("At least one key property must exist on the entity.", nameof(keyProperties));
        }

        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sourceColumns = string.Join(", ", allProperties.Select(p => $"@{p.Name} AS [{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var keyMatchConditions = string.Join(" AND ", keyEntityProperties.Select(p => $"target.[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = source.[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var insertColumns = string.Join(", ", allProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var insertValues = string.Join(", ", allProperties.Select(p => $"source.[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var updateSet = string.Join(", ", valueProperties.Select(p => $"target.[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = source.[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));

        var sql = $@"
MERGE {tableRef} AS target
USING (SELECT {sourceColumns}) AS source
ON ({keyMatchConditions})
WHEN MATCHED THEN
    UPDATE SET {updateSet}
WHEN NOT MATCHED THEN
    INSERT ({insertColumns})
    VALUES ({insertValues})
OUTPUT $action AS MergeAction, INSERTED.*;";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in allProperties)
        {
            var value = property.GetValue(entity);
            parameters[property.Name] = value;
        }

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["UpsertOperation"] = true,
            ["KeyColumns"] = string.Join(",", keyPropertyNames)
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlUpsertCommand<TEntity>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates an upsert command using IF EXISTS pattern with type-safe property expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="keyProperties">Expressions pointing to the key properties for conflict detection.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="excludeProperties">Properties to exclude from operations.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An upsert command using alternative SQL Server syntax.</returns>
    public static MsSqlUpsertCommand<TEntity> UpsertSimple<TEntity>(
        TEntity entity,
        Expression<Func<TEntity, object>>[] keyProperties,
        string? schema = null,
        Expression<Func<TEntity, object>>[]? excludeProperties = null,
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
        var excludePropertyNames = ExtractPropertyNames(excludeProperties);
        
        var allEntityProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false).ToList();
        var keyEntityProperties = allEntityProperties.Where(p => keyPropertyNames.Contains(p.Name)).ToList();
        var insertableProperties = allEntityProperties.Where(p => !excludePropertyNames.Contains(p.Name)).ToList();
        var updateableProperties = allEntityProperties.Where(p => !keyPropertyNames.Contains(p.Name) && !excludePropertyNames.Contains(p.Name)).ToList();

        if (keyEntityProperties.Count == 0)
        {
            throw new ArgumentException("At least one key property must exist on the entity.", nameof(keyProperties));
        }

        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var insertColumns = string.Join(", ", insertableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var insertValues = string.Join(", ", insertableProperties.Select(p => $"@{p.Name}"));
        var keyConditions = string.Join(" AND ", keyEntityProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = @{p.Name}"));
        var updateSet = string.Join(", ", updateableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = @{p.Name}"));

        // Use IF EXISTS pattern for SQL Server
        var sql = $@"
IF EXISTS (SELECT 1 FROM {tableRef} WHERE {keyConditions})
BEGIN
    UPDATE {tableRef} 
    SET {updateSet}
    OUTPUT 'UPDATE' AS MergeAction, INSERTED.*
    WHERE {keyConditions}
END
ELSE
BEGIN
    INSERT INTO {tableRef} ({insertColumns})
    OUTPUT 'INSERT' AS MergeAction, INSERTED.*
    VALUES ({insertValues})
END";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in insertableProperties)
        {
            var value = property.GetValue(entity);
            parameters[property.Name] = value;
        }

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["UpsertOperation"] = true,
            ["UpsertMethod"] = "IfExists",
            ["KeyColumns"] = string.Join(",", keyPropertyNames)
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlUpsertCommand<TEntity>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a bulk upsert command for multiple entities using MERGE with type-safe property expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entities">The entities to upsert.</param>
    /// <param name="keyProperties">Expressions pointing to the key properties used for matching.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="excludeProperties">Properties to exclude from operations.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>A bulk upsert command.</returns>
    public static MsSqlUpsertCommand<IEnumerable<TEntity>> BulkUpsert<TEntity>(
        IEnumerable<TEntity> entities,
        Expression<Func<TEntity, object>>[] keyProperties,
        string? schema = null,
        Expression<Func<TEntity, object>>[]? excludeProperties = null,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(keyProperties);

        var entityList = entities.ToList();
        if (entityList.Count == 0)
        {
            throw new ArgumentException("Entities collection cannot be empty.", nameof(entities));
        }

        if (keyProperties.Length == 0)
        {
            throw new ArgumentException("At least one key property must be specified.", nameof(keyProperties));
        }

        var keyPropertyNames = ExtractPropertyNames(keyProperties);
        var excludePropertyNames = ExtractPropertyNames(excludeProperties);
        
        var allEntityProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false).ToList();
        var keyEntityProperties = allEntityProperties.Where(p => keyPropertyNames.Contains(p.Name)).ToList();
        var valueProperties = allEntityProperties.Where(p => !keyPropertyNames.Contains(p.Name) && !excludePropertyNames.Contains(p.Name)).ToList();
        var allProperties = keyEntityProperties.Concat(valueProperties).ToList();

        if (keyEntityProperties.Count == 0)
        {
            throw new ArgumentException("At least one key property must exist on the entity.", nameof(keyProperties));
        }

        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);

        // Create source table for bulk operation
        var sourceRows = new List<string>();
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);

        for (int i = 0; i < entityList.Count; i++)
        {
            var entity = entityList[i];
            var rowValues = allProperties.Select(p => $"@{p.Name}_{i}").ToList();
            sourceRows.Add($"({string.Join(", ", rowValues)})");

            foreach (var property in allProperties)
            {
                var value = property.GetValue(entity);
                parameters[$"{property.Name}_{i}"] = value;
            }
        }

        var sourceColumns = string.Join(", ", allProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var sourceValues = string.Join(", ", sourceRows);
        var keyMatchConditions = string.Join(" AND ", keyEntityProperties.Select(p => $"target.[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = source.[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var insertColumns = string.Join(", ", allProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var insertValues = string.Join(", ", allProperties.Select(p => $"source.[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var updateSet = string.Join(", ", valueProperties.Select(p => $"target.[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = source.[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));

        var sql = $@"
WITH SourceData ({sourceColumns}) AS (
    VALUES {sourceValues}
)
MERGE {tableRef} AS target
USING SourceData AS source
ON ({keyMatchConditions})
WHEN MATCHED THEN
    UPDATE SET {updateSet}
WHEN NOT MATCHED THEN
    INSERT ({insertColumns})
    VALUES ({insertValues})
OUTPUT $action AS MergeAction, INSERTED.*;";

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["BulkUpsert"] = true,
            ["EntityCount"] = entityList.Count,
            ["KeyColumns"] = string.Join(",", keyPropertyNames)
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlUpsertCommand<IEnumerable<TEntity>>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates an upsert command with conditional logic using type-safe expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to upsert.</param>
    /// <param name="existsCondition">Expression to check if the entity exists.</param>
    /// <param name="updateProperties">Properties to update when matched (if null, updates all non-key properties).</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An upsert command with conditional logic.</returns>
    public static MsSqlUpsertCommand<TEntity> UpsertConditional<TEntity>(
        TEntity entity,
        Expression<Func<TEntity, bool>> existsCondition,
        Expression<Func<TEntity, object>>[]? updateProperties = null,
        string? schema = null,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(existsCondition);

        var (whereClause, whereParameters) = MsSqlCommandBase.ConvertPredicateToSql(existsCondition);
        var allEntityProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false).ToList();
        
        var updatePropertyNames = updateProperties != null ? ExtractPropertyNames(updateProperties) : new HashSet<string>();
        var updateableProperties = updateProperties != null 
            ? allEntityProperties.Where(p => updatePropertyNames.Contains(p.Name)).ToList()
            : allEntityProperties;
        
        var insertableProperties = allEntityProperties;

        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var insertColumns = string.Join(", ", insertableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var insertValues = string.Join(", ", insertableProperties.Select(p => $"@Insert_{p.Name}"));
        var updateSet = string.Join(", ", updateableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}] = @Update_{p.Name}"));

        // Use IF EXISTS pattern with expression-based condition
        var sql = $@"
IF EXISTS (SELECT 1 FROM {tableRef} WHERE {whereClause})
BEGIN
    UPDATE {tableRef} 
    SET {updateSet}
    OUTPUT 'UPDATE' AS MergeAction, INSERTED.*
    WHERE {whereClause}
END
ELSE
BEGIN
    INSERT INTO {tableRef} ({insertColumns})
    OUTPUT 'INSERT' AS MergeAction, INSERTED.*
    VALUES ({insertValues})
END";

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        
        // Add WHERE parameters
        foreach (var param in whereParameters)
        {
            parameters[param.Key] = param.Value;
        }
        
        // Add update parameters
        foreach (var property in updateableProperties)
        {
            var value = property.GetValue(entity);
            parameters[$"Update_{property.Name}"] = value;
        }
        
        // Add insert parameters
        foreach (var property in insertableProperties)
        {
            var value = property.GetValue(entity);
            parameters[$"Insert_{property.Name}"] = value;
        }

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["UpsertOperation"] = true,
            ["UpsertMethod"] = "Conditional",
            ["ConditionalUpsert"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlUpsertCommand<TEntity>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
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
    /// <param name="propertySelector">The property selector expression.</param>
    /// <returns>The property name.</returns>
    private static string ExtractPropertyName<T>(Expression<Func<T, object>> propertySelector)
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
}