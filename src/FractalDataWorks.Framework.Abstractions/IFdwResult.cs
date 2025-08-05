using System;
using System.Collections.Generic;

namespace FractalDataWorks.Framework.Abstractions;

/// <summary>
/// Generic interface for operation results in the FractalDataWorks framework.
/// Provides a consistent way to handle success/failure states and error information.
/// </summary>
/// <typeparam name="T">The type of data returned on successful operations.</typeparam>
/// <remarks>
/// This interface enables consistent error handling and result processing across
/// all framework operations. The "Fdw" prefix avoids namespace collisions.
/// </remarks>
public interface IFdwResult<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    /// <value><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</value>
    bool IsSuccess { get; }
    
    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <value><c>true</c> if the operation failed; otherwise, <c>false</c>.</value>
    bool IsFailure => !IsSuccess;
    
    /// <summary>
    /// Gets the result data if the operation was successful.
    /// </summary>
    /// <value>The result data, or default(T) if the operation failed.</value>
    /// <remarks>
    /// This property should only be accessed when <see cref="IsSuccess"/> is <c>true</c>.
    /// Consider using <see cref="TryGetValue"/> for safer access patterns.
    /// </remarks>
    T? Value { get; }
    
    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    /// <value>The error message, or null if the operation succeeded.</value>
    string? ErrorMessage { get; }
    
    /// <summary>
    /// Gets additional error details if available.
    /// </summary>
    /// <value>A collection of additional error information, or empty if none available.</value>
    IReadOnlyList<string> ErrorDetails { get; }
    
    /// <summary>
    /// Gets the exception that caused the failure, if any.
    /// </summary>
    /// <value>The exception that caused the failure, or null if no exception occurred.</value>
    Exception? Exception { get; }
    
    /// <summary>
    /// Safely attempts to get the result value.
    /// </summary>
    /// <param name="value">When this method returns, contains the result value if successful; otherwise, the default value.</param>
    /// <returns><c>true</c> if the operation was successful and the value is available; otherwise, <c>false</c>.</returns>
    bool TryGetValue(out T? value);
}

/// <summary>
/// Non-generic interface for operation results in the FractalDataWorks framework.
/// Provides basic success/failure information without typed result data.
/// </summary>
/// <remarks>
/// Use this interface when operations don't return specific data but still need
/// to indicate success/failure status and provide error information.
/// </remarks>
public interface IFdwResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    /// <value><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</value>
    bool IsSuccess { get; }
    
    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <value><c>true</c> if the operation failed; otherwise, <c>false</c>.</value>
    bool IsFailure => !IsSuccess;
    
    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    /// <value>The error message, or null if the operation succeeded.</value>
    string? ErrorMessage { get; }
    
    /// <summary>
    /// Gets additional error details if available.
    /// </summary>
    /// <value>A collection of additional error information, or empty if none available.</value>
    IReadOnlyList<string> ErrorDetails { get; }
    
    /// <summary>
    /// Gets the exception that caused the failure, if any.
    /// </summary>
    /// <value>The exception that caused the failure, or null if no exception occurred.</value>
    Exception? Exception { get; }
}