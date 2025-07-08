using System.Linq.Expressions;

namespace FractalDataWorks.Data;

/// <summary>
/// Represents a delete operation - a type of data operation
/// </summary>
/// <typeparam name="T">The entity type being deleted</typeparam>
public interface IDataDelete<T> : IDataOperation where T : class
{
    /// <summary>
    /// The predicate expression for filtering which records to delete
    /// </summary>
    Expression<Func<T, bool>> WhereClause { get; }
    
    /// <summary>
    /// The target container name (table, file, etc.)
    /// </summary>
    string TargetName { get; }
}