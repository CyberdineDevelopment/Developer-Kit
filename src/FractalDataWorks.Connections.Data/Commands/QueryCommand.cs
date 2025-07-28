using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FractalDataWorks.Data;

namespace FractalDataWorks.Connections.Data.Commands;

/// <summary>
/// Concrete implementation of a query command
/// </summary>
/// <typeparam name="T">The entity type being queried</typeparam>
public class QueryCommand<T> : IQueryCommand<T> where T : class
{
    // IDataCommand properties
    public string DataStore { get; init; } = string.Empty;
    public string Container { get; init; } = string.Empty;
    public string Record { get; init; } = string.Empty;
    public string[]? Attributes { get; init; }
    
    // IQueryCommand properties
    public object? Identifier { get; init; }
    
    // IDataQuery<T> properties
    public Expression<Func<T, bool>>? WhereClause { get; init; }
    public Expression<Func<T, object>>? OrderBy { get; init; }
    public bool OrderByDescending { get; init; }
    public int? Skip { get; init; }
    public int? Take { get; init; }
    
    // IDataOperation properties
    public string OperationType => "Query";
    public string TargetName => Record;
    public Dictionary<string, object> Parameters { get; init; } = new();
    
    /// <summary>
    /// Creates a new query command builder
    /// </summary>
    public static QueryCommandBuilder<T> Create() => new();
}

/// <summary>
/// Fluent builder for query commands
/// </summary>
public class QueryCommandBuilder<T> where T : class
{
    private readonly QueryCommand<T> _command = new();
    
    public QueryCommandBuilder<T> From(string dataStore, string container, string record)
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
    
    public QueryCommandBuilder<T> Select(params string[] attributes)
    {
        return this with { _command = _command with { Attributes = attributes } };
    }
    
    public QueryCommandBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        return this with { _command = _command with { WhereClause = predicate } };
    }
    
    public QueryCommandBuilder<T> OrderBy(Expression<Func<T, object>> orderBy, bool descending = false)
    {
        return this with
        {
            _command = _command with
            {
                OrderBy = orderBy,
                OrderByDescending = descending
            }
        };
    }
    
    public QueryCommandBuilder<T> Skip(int count)
    {
        return this with { _command = _command with { Skip = count } };
    }
    
    public QueryCommandBuilder<T> Take(int count)
    {
        return this with { _command = _command with { Take = count } };
    }
    
    public QueryCommandBuilder<T> WithId(object identifier)
    {
        return this with { _command = _command with { Identifier = identifier } };
    }
    
    public QueryCommandBuilder<T> WithParameter(string key, object value)
    {
        var parameters = new Dictionary<string, object>(_command.Parameters) { [key] = value };
        return this with { _command = _command with { Parameters = parameters } };
    }
    
    public QueryCommand<T> Build() => _command;
}