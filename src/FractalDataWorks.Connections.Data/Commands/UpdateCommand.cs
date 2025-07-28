using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Data;

namespace FractalDataWorks.Connections.Data.Commands;

/// <summary>
/// Concrete implementation of an update command
/// </summary>
/// <typeparam name="T">The entity type being updated</typeparam>
public class UpdateCommand<T> : IUpdateCommand<T> where T : class
{
    // IDataCommand properties
    public string DataStore { get; init; } = string.Empty;
    public string Container { get; init; } = string.Empty;
    public string Record { get; init; } = string.Empty;
    public string[]? Attributes { get; init; }
    
    // IUpdateCommand properties
    public Expression<Func<T, bool>>? WhereClause { get; init; }
    public Action<T>? UpdateAction { get; init; }
    public Dictionary<string, object>? UpdateValues { get; init; }
    
    // IDataOperation properties
    public string OperationType => "Update";
    public string TargetName => Record;
    public Dictionary<string, object> Parameters { get; init; } = new();
    
    /// <summary>
    /// Creates a new update command builder
    /// </summary>
    public static UpdateCommandBuilder<T> Create() => new();
}

/// <summary>
/// Fluent builder for update commands
/// </summary>
public class UpdateCommandBuilder<T> where T : class
{
    private readonly UpdateCommand<T> _command = new();
    
    public UpdateCommandBuilder<T> In(string dataStore, string container, string record)
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
    
    public UpdateCommandBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        return this with { _command = _command with { WhereClause = predicate } };
    }
    
    public UpdateCommandBuilder<T> Set(Action<T> updateAction)
    {
        return this with { _command = _command with { UpdateAction = updateAction } };
    }
    
    public UpdateCommandBuilder<T> SetValues(Dictionary<string, object> values)
    {
        return this with { _command = _command with { UpdateValues = values } };
    }
    
    public UpdateCommandBuilder<T> SetValue(string property, object value)
    {
        var values = _command.UpdateValues == null 
            ? new Dictionary<string, object>() 
            : new Dictionary<string, object>(_command.UpdateValues);
        values[property] = value;
        
        return this with { _command = _command with { UpdateValues = values } };
    }
    
    public UpdateCommandBuilder<T> OnlyAttributes(params string[] attributes)
    {
        return this with { _command = _command with { Attributes = attributes } };
    }
    
    public UpdateCommandBuilder<T> WithParameter(string key, object value)
    {
        var parameters = new Dictionary<string, object>(_command.Parameters) { [key] = value };
        return this with { _command = _command with { Parameters = parameters } };
    }
    
    public UpdateCommand<T> Build()
    {
        if (_command.UpdateAction == null && _command.UpdateValues == null)
        {
            throw new InvalidOperationException("Update command must have either UpdateAction or UpdateValues");
        }
        
        return _command;
    }
}