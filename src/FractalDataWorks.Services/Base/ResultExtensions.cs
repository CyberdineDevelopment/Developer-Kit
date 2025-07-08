using FractalDataWorks.Models;

namespace FractalDataWorks.Services.Base;

/// <summary>
/// Extension methods for IGenericResult&lt;T&gt; pattern
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Convenience method for creating successful Unit results
    /// </summary>
    /// <param name="message">Success message</param>
    /// <returns>Successful Unit result</returns>
    public static IGenericResult<Unit> Ok(string message = "Success") 
        => IGenericResult<Unit>.Ok(Unit.Value, message);
    
    /// <summary>
    /// Maps a genericResult to a new type
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <param name="genericResult">Source genericResult</param>
    /// <param name="mapper">Mapping function</param>
    /// <returns>Mapped genericResult</returns>
    public static IGenericResult<TOut> Map<TIn, TOut>(this IGenericResult<TIn> genericResult, Func<TIn, TOut> mapper)
    {
        return genericResult.Match(
            success: (value, message) => IGenericResult<TOut>.Ok(mapper(value), message),
            failure: error => IGenericResult<TOut>.Fail(error)
        );
    }
    
    /// <summary>
    /// Async mapping operation
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <param name="resultTask">Source result task</param>
    /// <param name="mapper">Async mapping function</param>
    /// <returns>Mapped result</returns>
    public static async Task<IGenericResult<TOut>> Map<TIn, TOut>(
        this Task<IGenericResult<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper)
    {
        var result = await resultTask;
        return result switch
        {
            IGenericResult<TIn>.Success(var value, var msg) => 
                IGenericResult<TOut>.Ok(await mapper(value), msg),
            IGenericResult<TIn>.Failure(var error) => 
                IGenericResult<TOut>.Fail(error),
            _ => throw new InvalidOperationException("Invalid result state")
        };
    }
    
    /// <summary>
    /// Flat maps a genericResult (prevents IGenericResult&lt;IGenericResult&lt;T&gt;&gt;)
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <param name="genericResult">Source genericResult</param>
    /// <param name="mapper">Mapping function that returns a IGenericResult</param>
    /// <returns>Flattened genericResult</returns>
    public static IGenericResult<TOut> FlatMap<TIn, TOut>(this IGenericResult<TIn> genericResult, Func<TIn, IGenericResult<TOut>> mapper)
    {
        return genericResult.Match(
            success: (value, _) => mapper(value),
            failure: error => IGenericResult<TOut>.Fail(error)
        );
    }
    
    /// <summary>
    /// Async flat mapping operation
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <param name="resultTask">Source result task</param>
    /// <param name="mapper">Async mapping function that returns a IGenericResult</param>
    /// <returns>Flattened result</returns>
    public static async Task<IGenericResult<TOut>> FlatMap<TIn, TOut>(
        this Task<IGenericResult<TIn>> resultTask,
        Func<TIn, Task<IGenericResult<TOut>>> mapper)
    {
        var result = await resultTask;
        return result switch
        {
            IGenericResult<TIn>.Success(var value, _) => await mapper(value),
            IGenericResult<TIn>.Failure(var error) => IGenericResult<TOut>.Fail(error),
            _ => throw new InvalidOperationException("Invalid result state")
        };
    }
    
    /// <summary>
    /// Executes an action on success, returns the original genericResult
    /// </summary>
    /// <typeparam name="T">IGenericResult type</typeparam>
    /// <param name="genericResult">Source genericResult</param>
    /// <param name="action">Action to execute on success</param>
    /// <returns>Original genericResult</returns>
    public static IGenericResult<T> OnSuccess<T>(this IGenericResult<T> genericResult, Action<T> action)
    {
        if (genericResult.IsSuccess && genericResult.Value is not null)
        {
            action(genericResult.Value);
        }
        return genericResult;
    }
    
    /// <summary>
    /// Executes an action on failure, returns the original genericResult
    /// </summary>
    /// <typeparam name="T">IGenericResult type</typeparam>
    /// <param name="genericResult">Source genericResult</param>
    /// <param name="action">Action to execute on failure</param>
    /// <returns>Original genericResult</returns>
    public static IGenericResult<T> OnFailure<T>(this IGenericResult<T> genericResult, Action<ServiceMessage> action)
    {
        if (genericResult.IsFailure && genericResult.Error is not null)
        {
            action(genericResult.Error);
        }
        return genericResult;
    }
    
    /// <summary>
    /// Gets the value or a default value if the genericResult is a failure
    /// </summary>
    /// <typeparam name="T">IGenericResult type</typeparam>
    /// <param name="genericResult">Source genericResult</param>
    /// <param name="defaultValue">Default value to return on failure</param>
    /// <returns>The value or default</returns>
    public static T ValueOrDefault<T>(this IGenericResult<T> genericResult, T defaultValue)
    {
        return genericResult.IsSuccess && genericResult.Value is not null ? genericResult.Value : defaultValue;
    }
    
    /// <summary>
    /// Gets the value or computes a default value if the genericResult is a failure
    /// </summary>
    /// <typeparam name="T">IGenericResult type</typeparam>
    /// <param name="genericResult">Source genericResult</param>
    /// <param name="defaultValueFactory">Factory for computing default value</param>
    /// <returns>The value or computed default</returns>
    public static T ValueOrDefault<T>(this IGenericResult<T> genericResult, Func<T> defaultValueFactory)
    {
        return genericResult.IsSuccess && genericResult.Value is not null ? genericResult.Value : defaultValueFactory();
    }
}