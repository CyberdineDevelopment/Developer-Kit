using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.ExternalConnections.MsSql;

/// <summary>
/// Translates DataCommandBase instances to parameterized SQL Server SQL statements.
/// </summary>
/// <remarks>
/// This translator converts universal data commands (Query, Insert, Update, Delete, Upsert)
/// into safe, parameterized SQL Server statements. It handles the mapping between the universal
/// data model (DataRecord with Identifiers, Properties, Measures, Metadata) and SQL Server
/// table/column concepts.
/// </remarks>
internal sealed class MsSqlCommandTranslator
{
    private readonly MsSqlConfiguration _configuration;
    private readonly ILogger<MsSqlCommandTranslator> _logger;

    public MsSqlCommandTranslator(MsSqlConfiguration configuration, ILogger<MsSqlCommandTranslator> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Translates a DataCommandBase to a SQL statement with parameters.
    /// </summary>
    /// <param name="command">The command to translate.</param>
    /// <returns>A translation result containing SQL and parameters.</returns>
    public SqlTranslationResult Translate(DataCommandBase command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        try
        {
            var result = command.CommandType switch
            {
                "Query" => TranslateQuery(command),
                "Count" => TranslateCount(command),
                "Exists" => TranslateExists(command),
                "Insert" => TranslateInsert(command),
                "BulkInsert" => TranslateBulkInsert(command),
                "Update" => TranslateUpdate(command),
                "Delete" => TranslateDelete(command),
                "Upsert" => TranslateUpsert(command),
                "BulkUpsert" => TranslateBulkUpsert(command),
                _ => throw new NotSupportedException($"Command type '{command.CommandType}' is not supported.")
            };

            if (_configuration.EnableSqlLogging)
            {
                LogSql(command, result);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to translate command {CommandType} for connection {ConnectionName}", 
                command.CommandType, command.ConnectionName);
            throw;
        }
    }

    private SqlTranslationResult TranslateQuery(DataCommandBase command)
    {
        var sql = new StringBuilder();
        var parameters = new List<SqlParameter>();
        var parameterCounter = 0;

        // Get table information
        var (schema, table) = GetSchemaAndTable(command);
        var fullTableName = $"[{schema}].[{table}]";

        // Basic SELECT
        sql.Append("SELECT * FROM ").Append(fullTableName);

        // Handle WHERE clause from expression
        if (TryGetPredicate(command, out var predicate))
        {
            var whereClause = BuildWhereClause(predicate, ref parameterCounter, parameters);
            if (!string.IsNullOrEmpty(whereClause))
            {
                sql.Append(" WHERE ").Append(whereClause);
            }
        }

        // Handle ORDER BY
        if (TryGetOrderBy(command, out var orderBy))
        {
            var orderClause = BuildOrderByClause(orderBy);
            if (!string.IsNullOrEmpty(orderClause))
            {
                sql.Append(" ORDER BY ").Append(orderClause);
            }
        }

        // Handle paging
        if (command.Metadata.TryGetValue("Paged", out var isPaged) && isPaged is true)
        {
            if (command.Metadata.TryGetValue("Offset", out var offsetObj) && 
                command.Metadata.TryGetValue("Limit", out var limitObj))
            {
                var offset = Convert.ToInt32(offsetObj);
                var limit = Convert.ToInt32(limitObj);
                
                // SQL Server requires ORDER BY for OFFSET/FETCH
                if (!sql.ToString().Contains("ORDER BY"))
                {
                    sql.Append(" ORDER BY (SELECT NULL)");
                }
                
                sql.Append($" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY");
            }
        }
        else if (command.Metadata.TryGetValue("SingleResult", out var isSingle) && isSingle is true)
        {
            sql.Append(" ORDER BY (SELECT NULL) OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY");
        }

        return new SqlTranslationResult(sql.ToString(), parameters);
    }

    private SqlTranslationResult TranslateCount(DataCommandBase command)
    {
        var sql = new StringBuilder();
        var parameters = new List<SqlParameter>();
        var parameterCounter = 0;

        var (schema, table) = GetSchemaAndTable(command);
        var fullTableName = $"[{schema}].[{table}]";

        sql.Append("SELECT COUNT(*) FROM ").Append(fullTableName);

        if (TryGetPredicate(command, out var predicate))
        {
            var whereClause = BuildWhereClause(predicate, ref parameterCounter, parameters);
            if (!string.IsNullOrEmpty(whereClause))
            {
                sql.Append(" WHERE ").Append(whereClause);
            }
        }

        return new SqlTranslationResult(sql.ToString(), parameters);
    }

    private SqlTranslationResult TranslateExists(DataCommandBase command)
    {
        var sql = new StringBuilder();
        var parameters = new List<SqlParameter>();
        var parameterCounter = 0;

        var (schema, table) = GetSchemaAndTable(command);
        var fullTableName = $"[{schema}].[{table}]";

        sql.Append("SELECT CASE WHEN EXISTS (SELECT 1 FROM ").Append(fullTableName);

        if (TryGetPredicate(command, out var predicate))
        {
            var whereClause = BuildWhereClause(predicate, ref parameterCounter, parameters);
            if (!string.IsNullOrEmpty(whereClause))
            {
                sql.Append(" WHERE ").Append(whereClause);
            }
        }

        sql.Append(") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END");

        return new SqlTranslationResult(sql.ToString(), parameters);
    }

    private SqlTranslationResult TranslateInsert(DataCommandBase command)
    {
        var sql = new StringBuilder();
        var parameters = new List<SqlParameter>();
        var parameterCounter = 0;

        var (schema, table) = GetSchemaAndTable(command);
        var fullTableName = $"[{schema}].[{table}]";

        var entity = GetEntityFromCommand(command);
        var properties = GetEntityProperties(entity);

        var columns = new List<string>();
        var values = new List<string>();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(entity);
            var paramName = $"@p{parameterCounter++}";
            
            columns.Add($"[{prop.Name}]");
            values.Add(paramName);
            
            parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
        }

        sql.Append("INSERT INTO ").Append(fullTableName);
        sql.Append(" (").Append(string.Join(", ", columns)).Append(")");
        sql.Append(" VALUES (").Append(string.Join(", ", values)).Append(")");

        // Handle return identity
        if (command.Metadata.TryGetValue("ReturnIdentity", out var returnIdentity) && returnIdentity is true)
        {
            sql.Append("; SELECT SCOPE_IDENTITY()");
        }

        return new SqlTranslationResult(sql.ToString(), parameters);
    }

    private SqlTranslationResult TranslateBulkInsert(DataCommandBase command)
    {
        // For simplicity, generate multiple INSERT statements
        // In production, consider using SqlBulkCopy for better performance
        var sql = new StringBuilder();
        var parameters = new List<SqlParameter>();
        var parameterCounter = 0;

        var (schema, table) = GetSchemaAndTable(command);
        var fullTableName = $"[{schema}].[{table}]";

        var entities = GetEntitiesFromCommand(command);
        var firstEntity = entities.First();
        var properties = GetEntityProperties(firstEntity);

        var columns = properties.Select(p => $"[{p.Name}]").ToList();
        sql.Append("INSERT INTO ").Append(fullTableName);
        sql.Append(" (").Append(string.Join(", ", columns)).Append(") VALUES ");

        var valuesList = new List<string>();
        foreach (var entity in entities)
        {
            var values = new List<string>();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                var paramName = $"@p{parameterCounter++}";
                values.Add(paramName);
                parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
            }
            valuesList.Add($"({string.Join(", ", values)})");
        }

        sql.Append(string.Join(", ", valuesList));

        return new SqlTranslationResult(sql.ToString(), parameters);
    }

    private SqlTranslationResult TranslateUpdate(DataCommandBase command)
    {
        var sql = new StringBuilder();
        var parameters = new List<SqlParameter>();
        var parameterCounter = 0;

        var (schema, table) = GetSchemaAndTable(command);
        var fullTableName = $"[{schema}].[{table}]";

        var entity = GetEntityFromCommand(command);
        var properties = GetEntityProperties(entity);

        sql.Append("UPDATE ").Append(fullTableName).Append(" SET ");

        var setClauses = new List<string>();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(entity);
            var paramName = $"@p{parameterCounter++}";
            setClauses.Add($"[{prop.Name}] = {paramName}");
            parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
        }

        sql.Append(string.Join(", ", setClauses));

        // Handle WHERE clause
        if (TryGetPredicate(command, out var predicate))
        {
            var whereClause = BuildWhereClause(predicate, ref parameterCounter, parameters);
            if (!string.IsNullOrEmpty(whereClause))
            {
                sql.Append(" WHERE ").Append(whereClause);
            }
        }

        return new SqlTranslationResult(sql.ToString(), parameters);
    }

    private SqlTranslationResult TranslateDelete(DataCommandBase command)
    {
        var sql = new StringBuilder();
        var parameters = new List<SqlParameter>();
        var parameterCounter = 0;

        var (schema, table) = GetSchemaAndTable(command);
        var fullTableName = $"[{schema}].[{table}]";

        sql.Append("DELETE FROM ").Append(fullTableName);

        if (TryGetPredicate(command, out var predicate))
        {
            var whereClause = BuildWhereClause(predicate, ref parameterCounter, parameters);
            if (!string.IsNullOrEmpty(whereClause))
            {
                sql.Append(" WHERE ").Append(whereClause);
            }
        }

        return new SqlTranslationResult(sql.ToString(), parameters);
    }

    private SqlTranslationResult TranslateUpsert(DataCommandBase command)
    {
        var sql = new StringBuilder();
        var parameters = new List<SqlParameter>();
        var parameterCounter = 0;

        var (schema, table) = GetSchemaAndTable(command);
        var fullTableName = $"[{schema}].[{table}]";

        var entity = GetEntityFromCommand(command);
        var properties = GetEntityProperties(entity);
        var conflictFields = GetConflictFieldsFromCommand(command);

        // Use MERGE statement for upsert
        sql.Append("MERGE ").Append(fullTableName).Append(" AS target USING (VALUES (");

        var sourceValues = new List<string>();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(entity);
            var paramName = $"@p{parameterCounter++}";
            sourceValues.Add(paramName);
            parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
        }

        sql.Append(string.Join(", ", sourceValues));
        sql.Append(")) AS source (");
        sql.Append(string.Join(", ", properties.Select(p => $"[{p.Name}]")));
        sql.Append(") ON ");

        // Build ON clause using conflict fields
        var onConditions = conflictFields.Select(field => $"target.[{field}] = source.[{field}]");
        sql.Append(string.Join(" AND ", onConditions));

        // Handle conflict behavior
        if (command.Metadata.TryGetValue("OnConflictIgnore", out var ignoreConflict) && ignoreConflict is true)
        {
            // Just insert when not matched
            sql.Append(" WHEN NOT MATCHED THEN INSERT (");
            sql.Append(string.Join(", ", properties.Select(p => $"[{p.Name}]")));
            sql.Append(") VALUES (");
            sql.Append(string.Join(", ", properties.Select(p => $"source.[{p.Name}]")));
            sql.Append(")");
        }
        else
        {
            // Update when matched
            sql.Append(" WHEN MATCHED THEN UPDATE SET ");
            
            var updateFields = properties.Where(p => !conflictFields.Contains(p.Name));
            if (command.Metadata.TryGetValue("OnConflictUpdate", out var specificUpdateFields) && 
                specificUpdateFields is List<string> updateFieldList)
            {
                updateFields = properties.Where(p => updateFieldList.Contains(p.Name));
            }

            var updateClauses = updateFields.Select(p => $"[{p.Name}] = source.[{p.Name}]");
            sql.Append(string.Join(", ", updateClauses));

            // Insert when not matched
            sql.Append(" WHEN NOT MATCHED THEN INSERT (");
            sql.Append(string.Join(", ", properties.Select(p => $"[{p.Name}]")));
            sql.Append(") VALUES (");
            sql.Append(string.Join(", ", properties.Select(p => $"source.[{p.Name}]")));
            sql.Append(")");
        }

        sql.Append(";");

        return new SqlTranslationResult(sql.ToString(), parameters);
    }

    private SqlTranslationResult TranslateBulkUpsert(DataCommandBase command)
    {
        // For simplicity, generate individual MERGE statements
        // In production, consider more sophisticated bulk operations
        var allSql = new StringBuilder();
        var allParameters = new List<SqlParameter>();

        var entities = GetEntitiesFromCommand(command);
        foreach (var entity in entities)
        {
            // Create a temporary single-entity command
            var tempCommand = CreateSingleEntityUpsertCommand(command, entity);
            var result = TranslateUpsert(tempCommand);
            
            allSql.AppendLine(result.Sql);
            allParameters.AddRange(result.Parameters);
        }

        return new SqlTranslationResult(allSql.ToString(), allParameters);
    }

    private (string Schema, string Table) GetSchemaAndTable(DataCommandBase command)
    {
        var containerName = command.TargetContainer?.ToString() ?? GetDefaultContainerName(command);
        return _configuration.ResolveSchemaAndTable(containerName);
    }

    private static string GetDefaultContainerName(DataCommandBase command)
    {
        // Try to infer from command type
        var commandType = command.GetType();
        if (commandType.IsGenericType)
        {
            var entityType = commandType.GetGenericArguments().FirstOrDefault();
            return entityType?.Name ?? "Unknown";
        }
        return "Unknown";
    }

    private static bool TryGetPredicate(DataCommandBase command, out Expression predicate)
    {
        predicate = Expression.Constant(0); // Temporary assignment
        
        // Try to get predicate from different command types using reflection
        var predicateProperty = command.GetType().GetProperty("Predicate");
        if (predicateProperty != null)
        {
            predicate = (predicateProperty.GetValue(command) as Expression) ?? Expression.Constant(0);
            return predicate != null;
        }

        return false;
    }

    private static bool TryGetOrderBy(DataCommandBase command, out Expression orderBy)
    {
        orderBy = Expression.Constant(0); // Temporary assignment
        
        var orderByProperty = command.GetType().GetProperty("OrderBy");
        if (orderByProperty != null)
        {
            orderBy = (orderByProperty.GetValue(command) as Expression) ?? Expression.Constant(0);
            return orderBy != null;
        }

        return false;
    }

    private static object GetEntityFromCommand(DataCommandBase command)
    {
        var entityProperty = command.GetType().GetProperty("Entity");
        if (entityProperty == null)
            throw new InvalidOperationException($"Command type {command.GetType().Name} does not have an Entity property.");
        
        return entityProperty.GetValue(command) ?? throw new InvalidOperationException("Entity is null.");
    }

    private static IEnumerable<object> GetEntitiesFromCommand(DataCommandBase command)
    {
        var entitiesProperty = command.GetType().GetProperty("Entities");
        if (entitiesProperty == null)
            throw new InvalidOperationException($"Command type {command.GetType().Name} does not have an Entities property.");
        
        var entities = entitiesProperty.GetValue(command) as System.Collections.IEnumerable;
        return entities?.Cast<object>() ?? throw new InvalidOperationException("Entities is null.");
    }

    private static List<string> GetConflictFieldsFromCommand(DataCommandBase command)
    {
        var conflictFieldsProperty = command.GetType().GetProperty("ConflictFields");
        if (conflictFieldsProperty == null)
            throw new InvalidOperationException($"Command type {command.GetType().Name} does not have a ConflictFields property.");
        
        var conflictFields = conflictFieldsProperty.GetValue(command) as IEnumerable<string>;
        return conflictFields?.ToList() ?? throw new InvalidOperationException("ConflictFields is null.");
    }

    private static PropertyInfo[] GetEntityProperties(object entity)
    {
        return entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                     .ToArray();
    }

    /// <summary>
    /// Creates a single entity upsert command from a bulk upsert command.
    /// </summary>
    /// <param name="originalCommand">The original bulk command.</param>
    /// <param name="entity">The single entity.</param>
    /// <returns>A single entity upsert command.</returns>
    [ExcludeFromCodeCoverage(Justification = "Placeholder method for bulk operations that throws NotImplementedException. Will be implemented when bulk operations are fully supported.")]
    private static DataCommandBase CreateSingleEntityUpsertCommand(DataCommandBase originalCommand, object entity)
    {
        // This is a simplified approach - in production you'd want more sophisticated command cloning
        throw new NotImplementedException("Single entity upsert command creation not implemented for bulk operations.");
    }

    private string BuildWhereClause(Expression predicate, ref int parameterCounter, List<SqlParameter> parameters)
    {
        return new ExpressionTranslator(ref parameterCounter, parameters).Translate(predicate);
    }

    private static string BuildOrderByClause(Expression orderBy)
    {
        // Simplified order by translation
        if (orderBy is LambdaExpression lambda && lambda.Body is MemberExpression member)
        {
            return $"[{member.Member.Name}]";
        }
        
        return "(SELECT NULL)"; // Fallback
    }

    private void LogSql(DataCommandBase command, SqlTranslationResult result)
    {
        var logSql = result.Sql;
        if (logSql.Length > _configuration.MaxSqlLogLength)
        {
            logSql = logSql.Substring(0, _configuration.MaxSqlLogLength) + "...";
        }

        _logger.LogDebug("Generated SQL for {CommandType}: {Sql} with {ParameterCount} parameters",
            command.CommandType, logSql, result.Parameters.Count);
    }
}