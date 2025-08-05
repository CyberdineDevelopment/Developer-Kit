using System;
using System.Collections.Generic;

namespace FractalDataWorks.Framework.Abstractions;

/// <summary>
/// Implementation of operation results for the FractalDataWorks framework.
/// Provides a consistent way to handle success/failure states and error information.
/// </summary>
/// <typeparam name="T">The type of data returned on successful operations.</typeparam>
/// <remarks>
/// This class implements the <see cref="IFdwResult{T}"/> interface and provides
/// factory methods for creating success and failure results.
/// </remarks>
public sealed class FdwResult<T> : IFdwResult<T>
{
    private readonly T? _value;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FdwResult{T}"/> class for a successful operation.
    /// </summary>
    /// <param name="value">The result value.</param>
    private FdwResult(T value)
    {
        IsSuccess = true;
        _value = value;
        ErrorMessage = null;
        ErrorDetails = Array.Empty<string>();
        Exception = null;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FdwResult{T}"/> class for a failed operation.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorDetails">Additional error details.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
    private FdwResult(string errorMessage, IReadOnlyList<string>? errorDetails = null, Exception? exception = null)
    {
        IsSuccess = false;
        _value = default;
        ErrorMessage = errorMessage;
        ErrorDetails = errorDetails ?? Array.Empty<string>();
        Exception = exception;
    }
    
    /// <inheritdoc />
    public bool IsSuccess { get; }
    
    /// <inheritdoc />
    public T? Value => _value;
    
    /// <inheritdoc />
    public string? ErrorMessage { get; }
    
    /// <inheritdoc />
    public IReadOnlyList<string> ErrorDetails { get; }
    
    /// <inheritdoc />
    public Exception? Exception { get; }
    
    /// <inheritdoc />
    public bool TryGetValue(out T? value)
    {
        value = _value;
        return IsSuccess;
    }
    
    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <returns>A successful <see cref="FdwResult{T}"/>.</returns>
    public static FdwResult<T> Success(T value)
    {
        return new FdwResult<T>(value);
    }
    
    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed <see cref="FdwResult{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errorMessage"/> is null.</exception>
    public static FdwResult<T> Failure(string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));
        return new FdwResult<T>(errorMessage);
    }
    
    /// <summary>
    /// Creates a failed result with the specified error message and exception.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failed <see cref="FdwResult{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="errorMessage"/> or <paramref name="exception"/> is null.
    /// </exception>
    public static FdwResult<T> Failure(string errorMessage, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));
        return new FdwResult<T>(errorMessage, null, exception);
    }
    
    /// <summary>
    /// Creates a failed result with the specified error message and additional error details.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorDetails">Additional error details.</param>
    /// <returns>A failed <see cref="FdwResult{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="errorMessage"/> or <paramref name="errorDetails"/> is null.
    /// </exception>
    public static FdwResult<T> Failure(string errorMessage, IReadOnlyList<string> errorDetails)
    {
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));
        ArgumentNullException.ThrowIfNull(errorDetails, nameof(errorDetails));
        return new FdwResult<T>(errorMessage, errorDetails);
    }
    
    /// <summary>
    /// Creates a failed result with the specified error message, error details, and exception.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorDetails">Additional error details.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failed <see cref="FdwResult{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public static FdwResult<T> Failure(string errorMessage, IReadOnlyList<string> errorDetails, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));
        ArgumentNullException.ThrowIfNull(errorDetails, nameof(errorDetails));
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));
        return new FdwResult<T>(errorMessage, errorDetails, exception);
    }
}

/// <summary>
/// Implementation of non-generic operation results for the FractalDataWorks framework.
/// Provides basic success/failure information without typed result data.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IFdwResult"/> interface and provides
/// factory methods for creating success and failure results without return values.
/// </remarks>
public sealed class FdwResult : IFdwResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FdwResult"/> class for a successful operation.
    /// </summary>
    private FdwResult()
    {
        IsSuccess = true;
        ErrorMessage = null;
        ErrorDetails = Array.Empty<string>();
        Exception = null;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FdwResult"/> class for a failed operation.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorDetails">Additional error details.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
    private FdwResult(string errorMessage, IReadOnlyList<string>? errorDetails = null, Exception? exception = null)
    {
        IsSuccess = false;
        ErrorMessage = errorMessage;
        ErrorDetails = errorDetails ?? Array.Empty<string>();
        Exception = exception;
    }
    
    /// <inheritdoc />
    public bool IsSuccess { get; }
    
    /// <inheritdoc />
    public string? ErrorMessage { get; }
    
    /// <inheritdoc />
    public IReadOnlyList<string> ErrorDetails { get; }
    
    /// <inheritdoc />
    public Exception? Exception { get; }
    
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="FdwResult"/>.</returns>
    public static FdwResult Success()
    {
        return new FdwResult();
    }
    
    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed <see cref="FdwResult"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errorMessage"/> is null.</exception>
    public static FdwResult Failure(string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));
        return new FdwResult(errorMessage);
    }
    
    /// <summary>
    /// Creates a failed result with the specified error message and exception.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failed <see cref="FdwResult"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="errorMessage"/> or <paramref name="exception"/> is null.
    /// </exception>
    public static FdwResult Failure(string errorMessage, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));
        return new FdwResult(errorMessage, null, exception);
    }
    
    /// <summary>
    /// Creates a failed result with the specified error message and additional error details.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorDetails">Additional error details.</param>
    /// <returns>A failed <see cref="FdwResult"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="errorMessage"/> or <paramref name="errorDetails"/> is null.
    /// </exception>
    public static FdwResult Failure(string errorMessage, IReadOnlyList<string> errorDetails)
    {
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));
        ArgumentNullException.ThrowIfNull(errorDetails, nameof(errorDetails));
        return new FdwResult(errorMessage, errorDetails);
    }
    
    /// <summary>
    /// Creates a failed result with the specified error message, error details, and exception.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorDetails">Additional error details.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failed <see cref="FdwResult"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public static FdwResult Failure(string errorMessage, IReadOnlyList<string> errorDetails, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(errorMessage, nameof(errorMessage));
        ArgumentNullException.ThrowIfNull(errorDetails, nameof(errorDetails));
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));
        return new FdwResult(errorMessage, errorDetails, exception);
    }
}