using System;
using System.Linq.Expressions;
using FractalDataWorks.Commands;

namespace FractalDataWorks.Data;

/// <summary>
/// Defines the contract for data commands.
/// </summary>
public interface IDataCommand : ICommand
{
    /// <summary>
    /// Gets the type of entity this command operates on.
    /// </summary>
    Type EntityType { get; }

    /// <summary>
    /// Gets the operation type.
    /// </summary>
    DataOperation Operation { get; }

    /// <summary>
    /// Gets the criteria for the operation.
    /// </summary>
    object? Criteria { get; }
}

/// <summary>
/// Defines a data command with specific entity type.
/// </summary>
/// <typeparam name="TEntity">The type of entity.</typeparam>
public interface IDataCommand<TEntity> : IDataCommand
    where TEntity : class
{
    /// <summary>
    /// Gets the entity for insert/update operations.
    /// </summary>
    TEntity? Entity { get; }

    /// <summary>
    /// Gets the predicate for query/delete operations.
    /// </summary>
    Expression<Func<TEntity, bool>>? Predicate { get; }
}