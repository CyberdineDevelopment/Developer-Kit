using System.Linq.Expressions;

namespace FractalDataWorks.Connections;

/// <summary>
/// Data connection interface - performs data-specific operations.
/// Implements universal query language that works across all storage types.
/// More specific than IConnection.
/// </summary>
public interface IDataConnection : IConnection
{
    /// <summary>
    /// Queries data using universal LINQ expressions.
    /// Works the same whether data is in SQL Server, JSON files, XML, etc.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="predicate">Universal predicate expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching entities</returns>
    Task<IGenericResult<IEnumerable<T>>> Query<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Gets a single entity using universal LINQ expressions
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="predicate">Universal predicate expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Single matching entity or error if not found/multiple found</returns>
    Task<IGenericResult<T>> Single<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Inserts an entity
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="entity">The entity to insert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The inserted entity (may have generated keys populated)</returns>
    Task<IGenericResult<T>> Insert<T>(T entity, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Updates entities using universal LINQ expressions
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="where">Universal predicate for which entities to update</param>
    /// <param name="update">Universal expression defining the update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities updated</returns>
    Task<IGenericResult<int>> Update<T>(Expression<Func<T, bool>> where, Expression<Func<T, T>> update, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Deletes entities using universal LINQ expressions
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="predicate">Universal predicate for which entities to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities deleted</returns>
    Task<IGenericResult<int>> Delete<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Gets the provider capabilities
    /// </summary>
    ProviderCapabilities Capabilities { get; }
    
    /// <summary>
    /// Checks if this connection supports the given data layout
    /// </summary>
    /// <param name="definition">The data container definition</param>
    /// <returns>True if the connection can handle this data layout</returns>
    bool SupportsDataLayout(DataContainerDefinition definition);
}

/// <summary>
/// Data connection interface with strongly-typed configuration
/// </summary>
/// <typeparam name="TConfiguration">The data connection configuration type</typeparam>
public interface IDataConnection<TConfiguration> : IDataConnection, IConnection<TConfiguration>
    where TConfiguration : DataConnectionConfiguration<TConfiguration>
{
    /// <summary>
    /// Gets the data connection configuration
    /// </summary>
    new TConfiguration Configuration { get; }
}

/// <summary>
/// Provider capabilities flags
/// </summary>
[Flags]
public enum ProviderCapabilities
{
    /// <summary>
    /// No special capabilities
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Supports basic CRUD operations
    /// </summary>
    BasicCrud = 1,
    
    /// <summary>
    /// Supports transactions
    /// </summary>
    Transactions = 2,
    
    /// <summary>
    /// Supports bulk operations
    /// </summary>
    BulkOperations = 4,
    
    /// <summary>
    /// Supports stored procedures
    /// </summary>
    StoredProcedures = 8,
    
    /// <summary>
    /// Supports full-text search
    /// </summary>
    FullTextSearch = 16,
    
    /// <summary>
    /// Supports JSON columns/operations
    /// </summary>
    JsonColumns = 32,
    
    /// <summary>
    /// Supports complex queries (joins, subqueries, etc.)
    /// </summary>
    ComplexQueries = 64,
    
    /// <summary>
    /// Supports streaming large datasets
    /// </summary>
    Streaming = 128
}
