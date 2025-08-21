using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Data.SqlClient;

namespace FractalDataWorks.Services.ExternalConnections.MsSql;

/// <summary>
/// Translates LINQ expressions to SQL WHERE clauses.
/// </summary>
internal sealed class ExpressionTranslator : ExpressionVisitor
{
    private readonly StringBuilder _sql = new();
    private readonly IList<SqlParameter> _parameters;
    private int _parameterCounter;

    public ExpressionTranslator(ref int parameterCounter, IList<SqlParameter> parameters)
    {
        _parameterCounter = parameterCounter;
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    public string Translate(Expression expression)
    {
        _sql.Clear();
        Visit(expression);
        return _sql.ToString();
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _sql.Append('(');
        Visit(node.Left);
        
        _sql.Append(' ');
        _sql.Append(GetOperator(node.NodeType));
        _sql.Append(' ');
        
        Visit(node.Right);
        _sql.Append(')');
        
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
        {
            _sql.Append($"[{node.Member.Name}]");
        }
        else
        {
            // Handle constants and complex expressions
            var value = GetMemberValue(node);
            var paramName = $"@p{_parameterCounter++}";
            _sql.Append(paramName);
            _parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
        }
        
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        var paramName = $"@p{_parameterCounter++}";
        _sql.Append(paramName);
        _parameters.Add(new SqlParameter(paramName, node.Value ?? DBNull.Value));
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (string.Equals(node.Method.Name, "Contains", StringComparison.Ordinal) && node.Method.DeclaringType == typeof(string))
        {
            Visit(node.Object);
            _sql.Append(" LIKE ");
            
            // Handle the pattern
            if (node.Arguments[0] is ConstantExpression constant)
            {
                var paramName = $"@p{_parameterCounter++}";
                _sql.Append(paramName);
                _parameters.Add(new SqlParameter(paramName, $"%{constant.Value}%"));
            }
            else
            {
                Visit(node.Arguments[0]);
            }
        }
        else if (string.Equals(node.Method.Name, "StartsWith", StringComparison.Ordinal) && node.Method.DeclaringType == typeof(string))
        {
            Visit(node.Object);
            _sql.Append(" LIKE ");
            
            if (node.Arguments[0] is ConstantExpression constant)
            {
                var paramName = $"@p{_parameterCounter++}";
                _sql.Append(paramName);
                _parameters.Add(new SqlParameter(paramName, $"{constant.Value}%"));
            }
            else
            {
                Visit(node.Arguments[0]);
            }
        }
        else if (string.Equals(node.Method.Name, "EndsWith", StringComparison.Ordinal) && node.Method.DeclaringType == typeof(string))
        {
            Visit(node.Object);
            _sql.Append(" LIKE ");
            
            if (node.Arguments[0] is ConstantExpression constant)
            {
                var paramName = $"@p{_parameterCounter++}";
                _sql.Append(paramName);
                _parameters.Add(new SqlParameter(paramName, $"%{constant.Value}"));
            }
            else
            {
                Visit(node.Arguments[0]);
            }
        }
        else
        {
            // For other method calls, try to evaluate them as constants
            var value = Expression.Lambda(node).Compile().DynamicInvoke();
            var paramName = $"@p{_parameterCounter++}";
            _sql.Append(paramName);
            _parameters.Add(new SqlParameter(paramName, value ?? DBNull.Value));
        }
        
        return node;
    }

    private static string GetOperator(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "!=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Expression type {nodeType} is not supported.")
        };
    }

    private static object? GetMemberValue(MemberExpression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
        var getter = getterLambda.Compile();
        return getter();
    }
}