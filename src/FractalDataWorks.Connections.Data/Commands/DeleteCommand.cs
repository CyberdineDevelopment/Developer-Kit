using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Data;

namespace FractalDataWorks.Connections.Data.Commands;

/// <summary>
/// Concrete implementation of a delete command
/// </summary>
/// <typeparam name="T">The entity type being deleted</typeparam>
public class DeleteCommand<T> : IDeleteCommand<T> where T : class
{
    // IDataCommand properties
    public string DataStore { get; init; } = string.Empty;
    public string Container { get; init; } = string.Empty;
    public string Record { get; init; } = string.Empty;
    public string[]? Attributes { get; init; }
    
    // IDeleteCommand properties
    public Expression<Func<T, bool>>? WhereClause { get; init; }
    public object? Identifier { get; init; }
    
    // IDataOperation properties
    public string OperationType => "Delete";
    public string TargetName => Record;
    public Dictionary<string, object> Parameters { get; init; } = new();
    
    /// <summary>
    /// Creates a new delete command builder
    /// </summary>
    public static DeleteCommandBuilder<T> Create() => new();
}

/// <summary>
/// Fluent builder for delete commands
/// </summary>
public class DeleteCommandBuilder<T> where T : class
{
    private readonly DeleteCommand<T> _command = new();
    
    public DeleteCommandBuilder<T> From(string dataStore, string container, string record)
    {
        return this with
        {
            _command = _command with
            {
                DataStore = dataStore,
                Container = container,
                Record = record
            }
        };
    }
    
    public DeleteCommandBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        return this with { _command = _command with { WhereClause = predicate } };
    }
    
    public DeleteCommandBuilder<T> WithId(object identifier)
    {
        return this with { _command = _command with { Identifier = identifier } };
    }
    
    public DeleteCommandBuilder<T> WithParameter(string key, object value)
    {
        var parameters = new Dictionary<string, object>(_command.Parameters) { [key] = value };
        return this with { _command = _command with { Parameters = parameters } };
    }
    
    public DeleteCommand<T> Build()
    {
        if (_command.WhereClause == null && _command.Identifier == null)
        {
            throw new InvalidOperationException("Delete command must have either WhereClause or Identifier");
        }
        
        return _command;
    }
}