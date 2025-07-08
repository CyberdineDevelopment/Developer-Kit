using System.Linq.Expressions;

namespace FractalDataWorks.Data;

/// <summary>
/// Represents a query operation - a type of data operation.
/// Uses universal LINQ expressions that work across all storage types.
/// </summary>
/// <typeparam name="T">The entity type being queried</typeparam>
public interface IDataQuery<T> : IDataOperation where T : class
{
    /// <summary>
    /// The predicate expression for filtering results
    /// </summary>
    Expression<Func<T, bool>>? WhereClause { get; }
    
    /// <summary>
    /// The expression for ordering results
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }
    
    /// <summary>
    /// Whether to order in descending order
    /// </summary>
    bool OrderByDescending { get; }
    
    /// <summary>
    /// Number of records to skip (for paging)
    /// </summary>
    int? Skip { get; }
    
    /// <summary>
    /// Number of records to take (for paging)
    /// </summary>
    int? Take { get; }
    
    /// <summary>
    /// The target container name (table, file, etc.)
    /// </summary>
    string TargetName { get; }
}