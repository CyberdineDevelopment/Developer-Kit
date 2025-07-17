using System.Collections.Generic;
using System;
namespace FractalDataWorks.Results;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class FdwResult<T>
{
    private readonly T? _value;
    private readonly string? _error;

    private FdwResult(T? value, string? error, bool isSuccess)
    {
        _value = value;
        _error = error;
        IsSuccess = isSuccess;
    }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    /// <returns>The value if successful; otherwise, default(T).</returns>
    public T? Value => IsSuccess ? _value : default;

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    /// <returns>The error message if failed; otherwise, null.</returns>
    public string? Error => IsFailure ? _error : null;

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <returns>A successful result containing the value.</returns>
    public static FdwResult<T> Success(T value) => new(value, null, true);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result containing the error message.</returns>
    public static FdwResult<T> Failure(string error) => new(default, error ?? "Unknown error", false);

    /// <summary>
    /// Creates a failed result from validation errors.
    /// </summary>
    /// <param name="errors">The collection of validation errors.</param>
    /// <returns>A failed result containing validation errors.</returns>
    public static FdwResult<T> Failure(IEnumerable<string> errors)
    {
        var errorMessage = string.Join("; ", errors);
        return new(default, errorMessage, false);
    }

    /// <summary>
    /// Maps the value of a successful result to a new type.
    /// </summary>
    /// <typeparam name="TNew">The new type to map to.</typeparam>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A new result with the mapped value or the original error.</returns>
    public FdwResult<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsFailure)
            return FdwResult<TNew>.Failure(_error!);

        try
        {
            return FdwResult<TNew>.Success(mapper(_value!));
        }
        catch (Exception ex)
        {
            return FdwResult<TNew>.Failure($"Mapping failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes one of two functions depending on the result state.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="success">Function to execute if successful.</param>
    /// <param name="failure">Function to execute if failed.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(
        Func<T, TResult> success,
        Func<string, TResult> failure)
    {
        return IsSuccess ? success(_value!) : failure(_error!);
    }

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator FdwResult<T>(T value) => Success(value);
}