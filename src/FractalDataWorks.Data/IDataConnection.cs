using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services;

namespace FractalDataWorks.Data;

/// <summary>
/// Defines the contract for data connections in the Fractal framework.
/// </summary>
public interface IDataConnection : IFdwService
{
    /// <summary>
    /// Queries data using LINQ expressions.
    /// </summary>
    /// <typeparam name="T">The type of data to query.</typeparam>
    /// <returns>A queryable data source.</returns>
    IQueryable<T> Query<T>() where T : class;

    /// <summary>
    /// Inserts a new entity.
    /// </summary>
    /// <typeparam name="T">The type of entity to insert.</typeparam>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>A task containing the result of the insert operation.</returns>
    Task<IFdwResult<T>> Insert<T>(T entity) where T : class;

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <typeparam name="T">The type of entity to update.</typeparam>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task containing the result of the update operation.</returns>
    Task<IFdwResult<T>> Update<T>(T entity) where T : class;

    /// <summary>
    /// Inserts or updates an entity.
    /// </summary>
    /// <typeparam name="T">The type of entity to upsert.</typeparam>
    /// <param name="entity">The entity to upsert.</param>
    /// <returns>A task containing the result of the upsert operation.</returns>
    Task<IFdwResult<T>> Upsert<T>(T entity) where T : class;

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <typeparam name="T">The type of entity to delete.</typeparam>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>A task containing the result of the delete operation.</returns>
    Task<IFdwResult<NonResult>> Delete<T>(T entity) where T : class;

    /// <summary>
    /// Deletes entities matching a predicate.
    /// </summary>
    /// <typeparam name="T">The type of entity to delete.</typeparam>
    /// <param name="predicate">The predicate to match entities.</param>
    /// <returns>A task containing the number of deleted entities.</returns>
    Task<IFdwResult<int>> Delete<T>(Expression<Func<T, bool>> predicate) where T : class;

    /// <summary>
    /// Executes a data command.
    /// </summary>
    /// <typeparam name="T">The type of result expected.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    Task<IFdwResult<T>> Execute<T>(IDataCommand command);

    /// <summary>
    /// Begins a transaction.
    /// </summary>
    /// <returns>A task containing the transaction.</returns>
    Task<IFdwResult<IDataTransaction>> BeginTransaction();
}