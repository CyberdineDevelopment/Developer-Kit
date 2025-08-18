using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FractalDataWorks.Services.DataProvider.Abstractions;

namespace FractalDataWorks.Services.DataProvider.MsSql.Commands;

/// <summary>
/// Microsoft SQL Server insert command for INSERT operations.
/// </summary>
/// <typeparam name="TResult">The type of result expected from the insert operation (typically the inserted entity or ID).</typeparam>
public sealed class MsSqlInsertCommand<TResult> : MsSqlCommandBase, IDataCommand<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlInsertCommand{TResult}"/> class.
    /// </summary>
    /// <param name="sqlText">The INSERT SQL statement.</param>
    /// <param name="target">The target table for the insert operation.</param>
    /// <param name="parameters">Insert parameters containing the values to be inserted.</param>
    /// <param name="metadata">Additional metadata such as schema information and return options.</param>
    /// <param name="timeout">The command timeout.</param>
    public MsSqlInsertCommand(
        string sqlText,
        string? target = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Insert", target, typeof(TResult), sqlText, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <inheritdoc/>
    protected override IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata)
    {
        return new MsSqlInsertCommand<TResult>(SqlText, Target, newParameters, newMetadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);
        return new MsSqlInsertCommand<TResult>(SqlText, Target, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    IDataCommand<TResult> IDataCommand<TResult>.WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        ArgumentNullException.ThrowIfNull(newMetadata);
        return new MsSqlInsertCommand<TResult>(SqlText, Target, Parameters, newMetadata, Timeout);
    }
}

/// <summary>
/// Non-generic Microsoft SQL Server insert command for INSERT operations.
/// </summary>
public sealed class MsSqlInsertCommand : MsSqlCommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlInsertCommand"/> class.
    /// </summary>
    /// <param name="sqlText">The INSERT SQL statement.</param>
    /// <param name="expectedResultType">The type of result expected from the insert operation.</param>
    /// <param name="target">The target table for the insert operation.</param>
    /// <param name="parameters">Insert parameters containing the values to be inserted.</param>
    /// <param name="metadata">Additional metadata such as schema information and return options.</param>
    /// <param name="timeout">The command timeout.</param>
    public MsSqlInsertCommand(
        string sqlText,
        Type expectedResultType,
        string? target = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("Insert", target, expectedResultType, sqlText, parameters, metadata, timeout)
    {
    }

    /// <inheritdoc/>
    public override bool IsDataModifying => true;

    /// <inheritdoc/>
    protected override IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata)
    {
        return new MsSqlInsertCommand(SqlText, ExpectedResultType, Target, newParameters, newMetadata, Timeout);
    }

    /// <summary>
    /// Creates a new insert command for a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <returns>A strongly-typed insert command.</returns>
    public MsSqlInsertCommand<TResult> AsTyped<TResult>()
    {
        return new MsSqlInsertCommand<TResult>(SqlText, Target, Parameters, Metadata, Timeout);
    }
}

/// <summary>
/// Factory methods for creating type-safe SQL Server insert commands.
/// </summary>
public static class MsSqlInsertCommandFactory
{
    /// <summary>
    /// Creates an insert command that returns the inserted entity with its generated ID using type-safe property mapping.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to insert (properties will be mapped automatically).</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="excludeProperties">Properties to exclude from the insert (e.g., auto-generated columns).</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An insert command that returns the inserted entity.</returns>
    public static MsSqlInsertCommand<TEntity> Insert<TEntity>(
        TEntity entity,
        string? schema = null,
        Expression<Func<TEntity, object>>[]? excludeProperties = null,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var excludeNames = ExtractPropertyNames(excludeProperties);
        
        // Always exclude ID properties that are auto-generated
        var idProperty = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
        
        if (idProperty != null)
        {
            excludeNames.Add(idProperty.Name);
        }

        var parameters = MsSqlCommandBase.ExtractEntityParameters(entity, excludeNames);
        var insertableProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .Where(p => !excludeNames.Contains(p.Name))
            .ToList();

        var columns = string.Join(", ", insertableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var parameterNames = string.Join(", ", insertableProperties.Select(p => $"@{p.Name}"));
        
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"INSERT INTO {tableRef} ({columns}) OUTPUT INSERTED.* VALUES ({parameterNames})";

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["ReturnInserted"] = true
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlInsertCommand<TEntity>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates an insert command that returns only the generated ID using type-safe property mapping.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The ID type.</typeparam>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="idProperty">Expression pointing to the ID property.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="excludeProperties">Properties to exclude from the insert.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An insert command that returns the generated ID.</returns>
    public static MsSqlInsertCommand<TId> InsertReturnId<TEntity, TId>(
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
        excludeNames.Add(idPropertyName); // Always exclude the ID column for auto-generated IDs

        var parameters = MsSqlCommandBase.ExtractEntityParameters(entity, excludeNames);
        var insertableProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .Where(p => !excludeNames.Contains(p.Name))
            .ToList();

        var columns = string.Join(", ", insertableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var parameterNames = string.Join(", ", insertableProperties.Select(p => $"@{p.Name}"));
        var idColumnName = MsSqlCommandBase.EscapeIdentifier(idPropertyName);
        
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"INSERT INTO {tableRef} ({columns}) OUTPUT INSERTED.[{idColumnName}] VALUES ({parameterNames})";

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["ReturnId"] = true,
            ["IdColumn"] = idPropertyName
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlInsertCommand<TId>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates an insert command from property selectors with type-safe column mapping.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="propertySelectors">Expressions selecting which properties to include.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="returnInserted">Whether to return the inserted entity.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>An insert command with selective property inclusion.</returns>
    public static MsSqlInsertCommand<TResult> InsertProperties<TEntity, TResult>(
        TEntity entity,
        Expression<Func<TEntity, object>>[] propertySelectors,
        string? schema = null,
        bool returnInserted = false,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(propertySelectors);

        if (propertySelectors.Length == 0)
        {
            throw new ArgumentException("At least one property selector must be provided.", nameof(propertySelectors));
        }

        var selectedPropertyNames = ExtractPropertyNames(propertySelectors);
        var selectedProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .Where(p => selectedPropertyNames.Contains(p.Name))
            .ToList();

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in selectedProperties)
        {
            var value = property.GetValue(entity);
            parameters[property.Name] = value;
        }

        var columns = string.Join(", ", selectedProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var parameterNames = string.Join(", ", selectedProperties.Select(p => $"@{p.Name}"));
        
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);
        var sql = $"INSERT INTO {tableRef} ({columns})";
        
        if (returnInserted)
        {
            sql += " OUTPUT INSERTED.*";
        }
        
        sql += $" VALUES ({parameterNames})";

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["SelectiveInsert"] = true
        };
        
        if (returnInserted)
        {
            metadata["ReturnInserted"] = true;
        }
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlInsertCommand<TResult>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
    }

    /// <summary>
    /// Creates a bulk insert command for multiple entities using type-safe property mapping.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <param name="excludeProperties">Properties to exclude from the insert.</param>
    /// <param name="timeout">The command timeout.</param>
    /// <returns>A bulk insert command.</returns>
    public static MsSqlInsertCommand<int> BulkInsert<TEntity>(
        IEnumerable<TEntity> entities,
        string? schema = null,
        Expression<Func<TEntity, object>>[]? excludeProperties = null,
        TimeSpan? timeout = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities.ToList();
        if (entityList.Count == 0)
        {
            throw new ArgumentException("Entities collection cannot be empty.", nameof(entities));
        }

        var excludeNames = ExtractPropertyNames(excludeProperties);
        
        // Always exclude ID properties that are auto-generated
        var idProperty = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));
        
        if (idProperty != null)
        {
            excludeNames.Add(idProperty.Name);
        }

        var insertableProperties = MsSqlCommandBase.GetEntityProperties<TEntity>(includeReadOnly: false)
            .Where(p => !excludeNames.Contains(p.Name))
            .ToList();

        var columns = string.Join(", ", insertableProperties.Select(p => $"[{MsSqlCommandBase.EscapeIdentifier(p.Name)}]"));
        var tableRef = MsSqlCommandBase.GetTableName<TEntity>(schema);

        // Create parameter placeholders for each entity
        var valuesClauses = new List<string>();
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);

        for (int i = 0; i < entityList.Count; i++)
        {
            var entity = entityList[i];
            var parameterNames = insertableProperties.Select(p => $"@{p.Name}_{i}").ToList();
            valuesClauses.Add($"({string.Join(", ", parameterNames)})");

            foreach (var property in insertableProperties)
            {
                var value = property.GetValue(entity);
                parameters[$"{property.Name}_{i}"] = value;
            }
        }

        var sql = $"INSERT INTO {tableRef} ({columns}) VALUES {string.Join(", ", valuesClauses)}";

        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ExpressionBased"] = true,
            ["BulkInsert"] = true,
            ["EntityCount"] = entityList.Count
        };
        
        if (schema != null)
        {
            metadata["Schema"] = schema;
        }

        return new MsSqlInsertCommand<int>(sql, typeof(TEntity).Name, parameters, metadata, timeout);
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