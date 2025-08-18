using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Validation;

namespace FractalDataWorks.Services.DataProvider.MsSql.Commands;

/// <summary>
/// Base class for all Microsoft SQL Server data commands providing common functionality.
/// </summary>
public abstract class MsSqlCommandBase : IDataCommand
{
    private readonly Dictionary<string, object?> _parameters;
    private readonly Dictionary<string, object> _metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlCommandBase"/> class.
    /// </summary>
    /// <param name="commandType">The type of command (Query, Insert, Update, Delete, Upsert).</param>
    /// <param name="target">The target table or resource.</param>
    /// <param name="expectedResultType">The expected result type.</param>
    /// <param name="sqlText">The SQL text to execute.</param>
    /// <param name="parameters">The command parameters.</param>
    /// <param name="metadata">Additional command metadata.</param>
    /// <param name="timeout">The command timeout.</param>
    protected MsSqlCommandBase(
        string commandType,
        string? target,
        Type expectedResultType,
        string sqlText,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
    {
        CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
        Target = target;
        ExpectedResultType = expectedResultType ?? throw new ArgumentNullException(nameof(expectedResultType));
        SqlText = sqlText ?? throw new ArgumentNullException(nameof(sqlText));
        Timeout = timeout;
        
        _parameters = parameters != null 
            ? new Dictionary<string, object?>(parameters, StringComparer.Ordinal)
            : new Dictionary<string, object?>(StringComparer.Ordinal);
        
        _metadata = metadata != null 
            ? new Dictionary<string, object>(metadata, StringComparer.Ordinal)
            : new Dictionary<string, object>(StringComparer.Ordinal);

        CorrelationId = Guid.NewGuid();
        CommandId = Guid.NewGuid();
    }

    /// <inheritdoc/>
    public string CommandType { get; }

    /// <inheritdoc/>
    public string? Target { get; }

    /// <inheritdoc/>
    public Type ExpectedResultType { get; }

    /// <inheritdoc/>
    public TimeSpan? Timeout { get; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> Parameters => _parameters;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <inheritdoc/>
    public abstract bool IsDataModifying { get; }

    /// <summary>
    /// Gets the SQL text to execute.
    /// </summary>
    public string SqlText { get; }

    /// <summary>
    /// Gets the SQL command type for ADO.NET.
    /// </summary>
    public virtual CommandType SqlCommandType => System.Data.CommandType.Text;

    /// <inheritdoc/>
    public Guid CorrelationId { get; }

    /// <inheritdoc/>
    public Guid CommandId { get; }

    /// <inheritdoc/>
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    public IFdwConfiguration? Configuration => null;

    /// <summary>
    /// Applies schema mapping to the SQL text if schema information is available in metadata.
    /// </summary>
    /// <returns>The SQL text with schema mapping applied.</returns>
    protected virtual string ApplySchemaMapping()
    {
        if (!Metadata.TryGetValue("Schema", out var schemaValue) || schemaValue is not string schema)
        {
            return SqlText;
        }

        // Apply schema mapping by replacing placeholders or prepending schema name
        if (SqlText.Contains("{schema}", StringComparison.OrdinalIgnoreCase))
        {
            return SqlText.Replace("{schema}", schema, StringComparison.OrdinalIgnoreCase);
        }

        // If no placeholder found and target is specified, prepend schema to table references
        if (!string.IsNullOrEmpty(Target) && SqlText.Contains(Target, StringComparison.OrdinalIgnoreCase))
        {
            return SqlText.Replace(Target, $"{schema}.{Target}", StringComparison.OrdinalIgnoreCase);
        }

        return SqlText;
    }

    /// <inheritdoc/>
    public virtual IFdwResult Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SqlText))
        {
            errors.Add("SQL text cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(CommandType))
        {
            errors.Add("Command type cannot be null or empty.");
        }

        if (ExpectedResultType == null)
        {
            errors.Add("Expected result type cannot be null.");
        }

        if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
        {
            errors.Add("Timeout must be positive if specified.");
        }

        // Validate parameters don't contain null keys
        foreach (var parameter in Parameters)
        {
            if (string.IsNullOrWhiteSpace(parameter.Key))
            {
                errors.Add("Parameter keys cannot be null or empty.");
                break;
            }
        }

        return errors.Count > 0 
            ? FdwResult.Failure(string.Join("; ", errors))
            : FdwResult.Success();
    }

    /// <inheritdoc/>
    async Task<IValidationResult> ICommand.Validate()
    {
        await Task.CompletedTask; // Make this async to satisfy interface
        var fdwResult = Validate();
        
        // Convert IFdwResult to IValidationResult 
        // This is a simple adapter implementation
        return new ValidationResultAdapter(fdwResult);
    }

    /// <inheritdoc/>
    public virtual IDataCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        ArgumentNullException.ThrowIfNull(newParameters);
        return CreateCopy(newParameters, Metadata);
    }

    /// <inheritdoc/>
    public virtual IDataCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        ArgumentNullException.ThrowIfNull(newMetadata);
        return CreateCopy(Parameters, newMetadata);
    }

    /// <summary>
    /// Creates a copy of this command with new parameters and metadata.
    /// </summary>
    /// <param name="newParameters">The new parameters.</param>
    /// <param name="newMetadata">The new metadata.</param>
    /// <returns>A new command instance.</returns>
    protected abstract IDataCommand CreateCopy(
        IReadOnlyDictionary<string, object?> newParameters,
        IReadOnlyDictionary<string, object> newMetadata);

    /// <summary>
    /// Gets the final SQL text with schema mapping applied.
    /// </summary>
    /// <returns>The SQL text ready for execution.</returns>
    public virtual string GetExecutableSql()
    {
        return ApplySchemaMapping();
    }

    /// <summary>
    /// Converts a Func&lt;T, bool&gt; expression to a SQL WHERE clause with parameters.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="predicate">The predicate expression.</param>
    /// <returns>A tuple containing the WHERE clause and parameters.</returns>
    protected static (string WhereClause, Dictionary<string, object?> Parameters) ConvertPredicateToSql<T>(Expression<Func<T, bool>> predicate)
    {
        var visitor = new SqlExpressionVisitor();
        var whereClause = visitor.Visit(predicate.Body);
        return (whereClause, visitor.Parameters);
    }

    /// <summary>
    /// Gets the table name for a given entity type, applying schema if available.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="schema">Optional schema name.</param>
    /// <returns>The full table reference.</returns>
    protected static string GetTableName<T>(string? schema = null)
    {
        var tableName = typeof(T).Name;
        return schema != null ? $"[{EscapeIdentifier(schema)}].[{EscapeIdentifier(tableName)}]" : $"[{EscapeIdentifier(tableName)}]";
    }

    /// <summary>
    /// Gets the table name for a given table name string, applying schema if available.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="schema">Optional schema name.</param>
    /// <returns>The full table reference.</returns>
    protected static string GetTableName(string tableName, string? schema = null)
    {
        return schema != null ? $"[{EscapeIdentifier(schema)}].[{EscapeIdentifier(tableName)}]" : $"[{EscapeIdentifier(tableName)}]";
    }

    /// <summary>
    /// Escapes SQL Server identifiers to prevent injection attacks.
    /// </summary>
    /// <param name="identifier">The identifier to escape.</param>
    /// <returns>The escaped identifier.</returns>
    protected static string EscapeIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be null or empty.", nameof(identifier));

        // Remove any existing brackets and escape internal brackets
        var cleaned = identifier.Trim('[', ']').Replace("]", "]]");
        
        // Validate identifier contains only allowed characters
        if (!System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            throw new ArgumentException($"Invalid identifier: {identifier}", nameof(identifier));
            
        return cleaned;
    }

    /// <summary>
    /// Gets property information for an entity type, filtering by accessibility.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="includeReadOnly">Whether to include read-only properties.</param>
    /// <returns>Collection of property information.</returns>
    protected static IEnumerable<PropertyInfo> GetEntityProperties<T>(bool includeReadOnly = true)
    {
        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetMethod?.IsPublic == true && 
                       (includeReadOnly || (p.CanWrite && p.SetMethod?.IsPublic == true)));
    }

    /// <summary>
    /// Extracts property values from an entity into a parameter dictionary.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="excludeProperties">Properties to exclude.</param>
    /// <returns>Dictionary of property names and values.</returns>
    protected static Dictionary<string, object?> ExtractEntityParameters<T>(T entity, IEnumerable<string>? excludeProperties = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        var excludeSet = new HashSet<string>(excludeProperties ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        
        foreach (var property in GetEntityProperties<T>(includeReadOnly: false))
        {
            if (!excludeSet.Contains(property.Name))
            {
                var value = property.GetValue(entity);
                parameters[property.Name] = value;
            }
        }
        
        return parameters;
    }
}

/// <summary>
/// Converts LINQ expressions to SQL WHERE clauses.
/// </summary>
internal sealed class SqlExpressionVisitor : ExpressionVisitor
{
    private readonly StringBuilder _sql = new();
    private readonly Dictionary<string, object?> _parameters = new(StringComparer.Ordinal);
    private int _parameterIndex = 0;

    public Dictionary<string, object?> Parameters => _parameters;

    public new string Visit(Expression expression)
    {
        _sql.Clear();
        _parameters.Clear();
        _parameterIndex = 0;
        
        base.Visit(expression);
        return _sql.ToString();
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _sql.Append('(');
        Visit(node.Left);
        
        _sql.Append(node.NodeType switch
        {
            ExpressionType.Equal => " = ",
            ExpressionType.NotEqual => " <> ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",
            ExpressionType.AndAlso => " AND ",
            ExpressionType.OrElse => " OR ",
            _ => throw new NotSupportedException($"Binary operator {node.NodeType} is not supported.")
        });
        
        Visit(node.Right);
        _sql.Append(')');
        
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Member is PropertyInfo property)
        {
            _sql.Append($"[{MsSqlCommandBase.EscapeIdentifier(property.Name)}]");
        }
        else
        {
            throw new NotSupportedException($"Member {node.Member.Name} is not supported.");
        }
        
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        var paramName = $"p{_parameterIndex++}";
        _parameters[paramName] = node.Value;
        _sql.Append($"@{paramName}");
        
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Handle string methods
        if (node.Method.DeclaringType == typeof(string))
        {
            switch (node.Method.Name)
            {
                case "Contains":
                    Visit(node.Object!);
                    _sql.Append(" LIKE '%' + ");
                    Visit(node.Arguments[0]);
                    _sql.Append(" + '%'");
                    return node;
                    
                case "StartsWith":
                    Visit(node.Object!);
                    _sql.Append(" LIKE ");
                    Visit(node.Arguments[0]);
                    _sql.Append(" + '%'");
                    return node;
                    
                case "EndsWith":
                    Visit(node.Object!);
                    _sql.Append(" LIKE '%' + ");
                    Visit(node.Arguments[0]);
                    return node;
            }
        }
        
        // Handle collection methods
        if (node.Method.Name == "Contains" && node.Arguments.Count == 2)
        {
            var collection = node.Arguments[0];
            var item = node.Arguments[1];
            
            Visit(item);
            _sql.Append(" IN ");
            
            if (collection is ConstantExpression constCollection && constCollection.Value is System.Collections.IEnumerable enumerable)
            {
                var values = new List<string>();
                foreach (var value in enumerable)
                {
                    var paramName = $"p{_parameterIndex++}";
                    _parameters[paramName] = value;
                    values.Add($"@{paramName}");
                }
                _sql.Append($"({string.Join(", ", values)})");
            }
            else
            {
                throw new NotSupportedException("Only constant collections are supported in Contains operations.");
            }
            
            return node;
        }
        
        throw new NotSupportedException($"Method {node.Method.Name} is not supported.");
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (node.NodeType == ExpressionType.Not)
        {
            _sql.Append("NOT (");
            Visit(node.Operand);
            _sql.Append(")");
            return node;
        }
        
        return base.VisitUnary(node);
    }
}

/// <summary>
/// Simple adapter to convert IFdwResult to IValidationResult for interface compatibility.
/// </summary>
internal sealed class ValidationResultAdapter : IValidationResult
{
    private readonly IFdwResult _fdwResult;

    public ValidationResultAdapter(IFdwResult fdwResult)
    {
        _fdwResult = fdwResult ?? throw new ArgumentNullException(nameof(fdwResult));
    }

    public bool IsValid => _fdwResult.IsSuccess;

    public IReadOnlyList<IValidationError> Errors => 
        _fdwResult.IsSuccess 
            ? new List<IValidationError>() 
            : new List<IValidationError> { new ValidationErrorAdapter(_fdwResult.Message ?? "Validation failed") };
}

/// <summary>
/// Simple adapter to convert error messages to IValidationError for interface compatibility.
/// </summary>
internal sealed class ValidationErrorAdapter : IValidationError
{
    public ValidationErrorAdapter(string message)
    {
        ErrorMessage = message ?? throw new ArgumentNullException(nameof(message));
        PropertyName = string.Empty;
        ErrorCode = "VALIDATION_ERROR";
        Severity = ValidationSeverity.Error;
    }

    public string ErrorMessage { get; }
    public string PropertyName { get; }
    public string? ErrorCode { get; }
    public ValidationSeverity Severity { get; }
}