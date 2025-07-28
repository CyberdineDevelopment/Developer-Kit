using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Data;

namespace FractalDataWorks.Connections.Data.Commands;

/// <summary>
/// Concrete implementation of an insert command
/// </summary>
/// <typeparam name="T">The entity type being inserted</typeparam>
public class InsertCommand<T> : IInsertCommand<T> where T : class
{
    // IDataCommand properties
    public string DataStore { get; init; } = string.Empty;
    public string Container { get; init; } = string.Empty;
    public string Record { get; init; } = string.Empty;
    public string[]? Attributes { get; init; }
    
    // IInsertCommand properties
    public T? Entity { get; init; }
    public IEnumerable<T>? Entities { get; init; }
    
    // IDataOperation properties
    public string OperationType => "Insert";
    public string TargetName => Record;
    public Dictionary<string, object> Parameters { get; init; } = new();
    
    /// <summary>
    /// Creates a new insert command builder
    /// </summary>
    public static InsertCommandBuilder<T> Create() => new();
}

/// <summary>
/// Fluent builder for insert commands
/// </summary>
public class InsertCommandBuilder<T> where T : class
{
    private readonly InsertCommand<T> _command = new();
    
    public InsertCommandBuilder<T> Into(string dataStore, string container, string record)
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
    
    public InsertCommandBuilder<T> WithEntity(T entity)
    {
        return this with { _command = _command with { Entity = entity } };
    }
    
    public InsertCommandBuilder<T> WithEntities(IEnumerable<T> entities)
    {
        return this with { _command = _command with { Entities = entities.ToList() } };
    }
    
    public InsertCommandBuilder<T> WithEntities(params T[] entities)
    {
        return WithEntities(entities.AsEnumerable());
    }
    
    public InsertCommandBuilder<T> OnlyAttributes(params string[] attributes)
    {
        return this with { _command = _command with { Attributes = attributes } };
    }
    
    public InsertCommandBuilder<T> WithParameter(string key, object value)
    {
        var parameters = new Dictionary<string, object>(_command.Parameters) { [key] = value };
        return this with { _command = _command with { Parameters = parameters } };
    }
    
    public InsertCommand<T> Build()
    {
        if (_command.Entity == null && (_command.Entities == null || !_command.Entities.Any()))
        {
            throw new InvalidOperationException("Insert command must have at least one entity");
        }
        
        return _command;
    }
}