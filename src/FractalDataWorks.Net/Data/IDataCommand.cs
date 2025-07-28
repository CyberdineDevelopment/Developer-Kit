using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FractalDataWorks.Data;

/// <summary>
/// Base interface for all data commands that can be executed against any storage type
/// </summary>
public interface IDataCommand : IDataOperation
{
    /// <summary>
    /// The storage provider type (SqlServer, FileSystem, RestApi, etc.)
    /// </summary>
    string DataStore { get; }
    
    /// <summary>
    /// The container location (database connection string, folder path, base URL)
    /// </summary>
    string Container { get; }
    
    /// <summary>
    /// The record collection name (table, file, endpoint)
    /// </summary>
    string Record { get; }
    
    /// <summary>
    /// Attributes to retrieve or affect (null = all)
    /// </summary>
    string[]? Attributes { get; }
}

/// <summary>
/// Query command for retrieving data with LINQ support
/// </summary>
/// <typeparam name="T">The entity type being queried</typeparam>
public interface IQueryCommand<T> : IDataCommand, IDataQuery<T> where T : class
{
    /// <summary>
    /// Optional identifier for single record retrieval
    /// </summary>
    object? Identifier { get; }
}

/// <summary>
/// Insert command for adding new records
/// </summary>
/// <typeparam name="T">The entity type being inserted</typeparam>
public interface IInsertCommand<T> : IDataCommand, IDataInsert<T> where T : class
{
    /// <summary>
    /// The entity to insert
    /// </summary>
    new T? Entity { get; }
    
    /// <summary>
    /// Multiple entities for bulk insert
    /// </summary>
    IEnumerable<T>? Entities { get; }
}

/// <summary>
/// Update command for modifying existing records
/// </summary>
/// <typeparam name="T">The entity type being updated</typeparam>
public interface IUpdateCommand<T> : IDataCommand, IDataUpdate<T> where T : class
{
    /// <summary>
    /// The predicate expression for filtering records to update
    /// </summary>
    new Expression<Func<T, bool>>? WhereClause { get; }
    
    /// <summary>
    /// The update action to apply to matching records
    /// </summary>
    Action<T>? UpdateAction { get; }
    
    /// <summary>
    /// Property-value pairs for updates (alternative to UpdateAction)
    /// </summary>
    Dictionary<string, object>? UpdateValues { get; }
}

/// <summary>
/// Delete command for removing records
/// </summary>
/// <typeparam name="T">The entity type being deleted</typeparam>
public interface IDeleteCommand<T> : IDataCommand, IDataDelete<T> where T : class
{
    /// <summary>
    /// The predicate expression for filtering records to delete
    /// </summary>
    new Expression<Func<T, bool>>? WhereClause { get; }
    
    /// <summary>
    /// Optional identifier for single record deletion
    /// </summary>
    object? Identifier { get; }
}