using System;
using System.Linq.Expressions;

namespace FractalDataWorks.Data;

/// <summary>
/// Defines the contract for query parsers that translate LINQ expressions to backend-specific queries.
/// </summary>
/// <typeparam name="TSource">The source query type (e.g., IQueryable).</typeparam>
/// <typeparam name="TTarget">The target query type (e.g., SQL, JSON query).</typeparam>
public interface IQueryParser<TSource, TTarget>
{
    /// <summary>
    /// Parses a source query into a target query format.
    /// </summary>
    /// <param name="source">The source query to parse.</param>
    /// <returns>The parsed target query.</returns>
    TTarget Parse(TSource source);

    /// <summary>
    /// Parses an expression into a target query format.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <param name="expression">The expression to parse.</param>
    /// <returns>The parsed target query.</returns>
    TTarget Parse<T>(Expression<Func<T, bool>> expression);

    /// <summary>
    /// Determines whether the parser can handle the given source type.
    /// </summary>
    /// <param name="sourceType">The type of the source.</param>
    /// <returns>True if the parser can handle the type; otherwise, false.</returns>
    bool CanHandle(Type sourceType);
}