using System.Linq.Expressions;

namespace FractalDataWorks.Data;

/// <summary>
/// Represents an update operation - a type of data operation
/// </summary>
/// <typeparam name="T">The entity type being updated</typeparam>
public interface IDataUpdate<T> : IDataOperation where T : class
{
    /// <summary>
    /// The predicate expression for filtering which records to update
    /// </summary>
    Expression<Func<T, bool>> WhereClause { get; }
    
    /// <summary>
    /// The expression defining how to update the entity
    /// </summary>
    Expression<Func<T, T>> UpdateExpression { get; }
    
    /// <summary>
    /// The target container name (table, file, etc.)
    /// </summary>
    string TargetName { get; }
}