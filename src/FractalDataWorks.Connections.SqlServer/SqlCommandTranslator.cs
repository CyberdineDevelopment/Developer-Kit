using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FractalDataWorks.Connections.Data;
using FractalDataWorks.Connections.Data.Commands;
using FractalDataWorks.Data;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections.SqlServer;

/// <summary>
/// Translates data commands to SQL commands
/// </summary>
public class SqlCommandTranslator : CommandTranslatorBase<SqlCommand>
{
    public SqlCommandTranslator(ILogger<SqlCommandTranslator> logger) : base(logger)
    {
    }
    
    protected override SqlCommand TranslateQuery(IQueryCommand<object> command)
    {
        var sql = new StringBuilder("SELECT ");
        
        // Select clause
        if (command.Attributes?.Any() == true)
        {
            sql.Append(string.Join(", ", command.Attributes.Select(a => $"[{a}]")));
        }
        else
        {
            sql.Append("*");
        }
        
        // From clause
        sql.Append($" FROM [{command.Record}]");
        
        // Where clause
        var sqlCommand = new SqlCommand();
        if (command.WhereClause != null)
        {
            var whereClause = TranslateExpression(command.WhereClause, sqlCommand);
            sql.Append($" WHERE {whereClause}");
        }
        else if (command.Identifier != null)
        {
            sql.Append(" WHERE [Id] = @Id");
            sqlCommand.Parameters.AddWithValue("@Id", command.Identifier);
        }
        
        // Order by clause
        if (command.OrderBy != null)
        {
            var orderByClause = ExtractMemberName(command.OrderBy);
            sql.Append($" ORDER BY [{orderByClause}]");
            if (command.OrderByDescending)
            {
                sql.Append(" DESC");
            }
        }
        
        // Pagination
        if (command.Skip.HasValue || command.Take.HasValue)
        {
            if (command.OrderBy == null)
            {
                // SQL Server requires ORDER BY for OFFSET/FETCH
                sql.Append(" ORDER BY (SELECT NULL)");
            }
            
            if (command.Skip.HasValue)
            {
                sql.Append($" OFFSET {command.Skip.Value} ROWS");
            }
            else
            {
                sql.Append(" OFFSET 0 ROWS");
            }
            
            if (command.Take.HasValue)
            {
                sql.Append($" FETCH NEXT {command.Take.Value} ROWS ONLY");
            }
        }
        
        sqlCommand.CommandText = sql.ToString();
        _logger.LogDebug("Translated query command to SQL: {Sql}", sqlCommand.CommandText);
        
        return sqlCommand;
    }
    
    protected override SqlCommand TranslateInsert(IInsertCommand<object> command)
    {
        var sqlCommand = new SqlCommand();
        
        if (command.Entity != null)
        {
            // Single entity insert
            var properties = GetEntityProperties(command.Entity, command.Attributes);
            var columns = string.Join(", ", properties.Keys.Select(k => $"[{k}]"));
            var values = string.Join(", ", properties.Keys.Select(k => $"@{k}"));
            
            sqlCommand.CommandText = $"INSERT INTO [{command.Record}] ({columns}) VALUES ({values})";
            
            foreach (var prop in properties)
            {
                sqlCommand.Parameters.AddWithValue($"@{prop.Key}", prop.Value ?? DBNull.Value);
            }
        }
        else if (command.Entities?.Any() == true)
        {
            // Multiple entities - would be handled by bulk copy in the connection
            throw new NotSupportedException("Use bulk copy for multiple entities");
        }
        
        _logger.LogDebug("Translated insert command to SQL: {Sql}", sqlCommand.CommandText);
        return sqlCommand;
    }
    
    protected override SqlCommand TranslateUpdate(IUpdateCommand<object> command)
    {
        var sqlCommand = new SqlCommand();
        var sql = new StringBuilder($"UPDATE [{command.Record}] SET ");
        
        if (command.UpdateValues != null)
        {
            // Update using dictionary values
            var setClauses = new List<string>();
            foreach (var kvp in command.UpdateValues)
            {
                setClauses.Add($"[{kvp.Key}] = @{kvp.Key}");
                sqlCommand.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
            }
            sql.Append(string.Join(", ", setClauses));
        }
        else if (command.UpdateAction != null)
        {
            // This is complex - would need to analyze the Action delegate
            throw new NotSupportedException("Use UpdateValues instead of UpdateAction for SQL translation");
        }
        else
        {
            throw new InvalidOperationException("Update command must have UpdateValues or UpdateAction");
        }
        
        // Where clause
        if (command.WhereClause != null)
        {
            var whereClause = TranslateExpression(command.WhereClause, sqlCommand);
            sql.Append($" WHERE {whereClause}");
        }
        
        sqlCommand.CommandText = sql.ToString();
        _logger.LogDebug("Translated update command to SQL: {Sql}", sqlCommand.CommandText);
        
        return sqlCommand;
    }
    
    protected override SqlCommand TranslateDelete(IDeleteCommand<object> command)
    {
        var sqlCommand = new SqlCommand();
        var sql = new StringBuilder($"DELETE FROM [{command.Record}]");
        
        // Where clause
        if (command.WhereClause != null)
        {
            var whereClause = TranslateExpression(command.WhereClause, sqlCommand);
            sql.Append($" WHERE {whereClause}");
        }
        else if (command.Identifier != null)
        {
            sql.Append(" WHERE [Id] = @Id");
            sqlCommand.Parameters.AddWithValue("@Id", command.Identifier);
        }
        else
        {
            throw new InvalidOperationException("Delete command must have WhereClause or Identifier");
        }
        
        sqlCommand.CommandText = sql.ToString();
        _logger.LogDebug("Translated delete command to SQL: {Sql}", sqlCommand.CommandText);
        
        return sqlCommand;
    }
    
    public override IDataCommand Parse(SqlCommand sqlCommand)
    {
        // Reverse engineer SQL to command
        var sql = sqlCommand.CommandText.ToUpperInvariant();
        
        if (sql.StartsWith("SELECT"))
        {
            return ParseSelectStatement(sqlCommand);
        }
        else if (sql.StartsWith("INSERT"))
        {
            return ParseInsertStatement(sqlCommand);
        }
        else if (sql.StartsWith("UPDATE"))
        {
            return ParseUpdateStatement(sqlCommand);
        }
        else if (sql.StartsWith("DELETE"))
        {
            return ParseDeleteStatement(sqlCommand);
        }
        
        throw new NotSupportedException($"Cannot parse SQL command: {sqlCommand.CommandText}");
    }
    
    private IDataCommand ParseSelectStatement(SqlCommand sqlCommand)
    {
        // Simplified SQL parsing - real implementation would use a SQL parser
        var match = System.Text.RegularExpressions.Regex.Match(
            sqlCommand.CommandText, 
            @"SELECT\s+(.*?)\s+FROM\s+\[?(\w+)\]?", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        if (!match.Success)
            throw new InvalidOperationException("Cannot parse SELECT statement");
        
        var columns = match.Groups[1].Value;
        var table = match.Groups[2].Value;
        
        var command = new QueryCommand<object>
        {
            DataStore = "SqlServer",
            Container = "default", // Would need to extract from connection
            Record = table,
            Attributes = columns == "*" ? null : columns.Split(',').Select(c => c.Trim(' ', '[', ']')).ToArray()
        };
        
        return command;
    }
    
    private IDataCommand ParseInsertStatement(SqlCommand sqlCommand)
    {
        throw new NotImplementedException("INSERT parsing not implemented");
    }
    
    private IDataCommand ParseUpdateStatement(SqlCommand sqlCommand)
    {
        throw new NotImplementedException("UPDATE parsing not implemented");
    }
    
    private IDataCommand ParseDeleteStatement(SqlCommand sqlCommand)
    {
        throw new NotImplementedException("DELETE parsing not implemented");
    }
    
    private string TranslateExpression(Expression expression, SqlCommand command)
    {
        // Simplified expression translation - real implementation would use ExpressionVisitor
        if (expression is LambdaExpression lambda)
        {
            return TranslateExpression(lambda.Body, command);
        }
        
        if (expression is BinaryExpression binary)
        {
            var left = TranslateExpression(binary.Left, command);
            var right = TranslateExpression(binary.Right, command);
            var op = GetSqlOperator(binary.NodeType);
            return $"{left} {op} {right}";
        }
        
        if (expression is MemberExpression member)
        {
            return $"[{member.Member.Name}]";
        }
        
        if (expression is ConstantExpression constant)
        {
            var paramName = $"@p{command.Parameters.Count}";
            command.Parameters.AddWithValue(paramName, constant.Value ?? DBNull.Value);
            return paramName;
        }
        
        throw new NotSupportedException($"Expression type {expression.NodeType} not supported");
    }
    
    private string GetSqlOperator(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Operator {nodeType} not supported")
        };
    }
    
    private string ExtractMemberName(Expression expression)
    {
        if (expression is LambdaExpression lambda)
        {
            return ExtractMemberName(lambda.Body);
        }
        
        if (expression is MemberExpression member)
        {
            return member.Member.Name;
        }
        
        if (expression is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
        {
            return unaryMember.Member.Name;
        }
        
        throw new InvalidOperationException("Cannot extract member name from expression");
    }
    
    private Dictionary<string, object> GetEntityProperties(object entity, string[]? attributes)
    {
        var properties = new Dictionary<string, object>();
        var type = entity.GetType();
        
        var propertiesToInclude = attributes?.Any() == true
            ? type.GetProperties().Where(p => attributes.Contains(p.Name))
            : type.GetProperties().Where(p => p.CanRead && p.CanWrite);
        
        foreach (var prop in propertiesToInclude)
        {
            properties[prop.Name] = prop.GetValue(entity);
        }
        
        return properties;
    }
}