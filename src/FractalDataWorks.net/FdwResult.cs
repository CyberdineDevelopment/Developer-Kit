using System;
using FractalDataWorks;

namespace FractalDataWorks;

/// <summary>
/// Basic implementation of IFdwResult.
/// </summary>
public class FdwResult : IFdwResult
{
    private FdwResult(bool isSuccess, IFdwMessage? message = null)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    /// <inheritdoc/>
    public bool IsSuccess { get; }
    
    /// <inheritdoc/>
    public bool IsFailure => !IsSuccess;
    
    /// <inheritdoc/>
    public IFdwMessage? Message { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static FdwResult Success() => new(true);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed result.</returns>
    public static FdwResult Failure(IFdwMessage message) => new(false, message ?? throw new ArgumentNullException(nameof(message)));
}

/// <summary>
/// Basic implementation of IFdwResult with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class FdwResult<T> : IFdwResult<T>
{
    private readonly T _value;
    private readonly bool _hasValue;

    private FdwResult(bool isSuccess, T value, IFdwMessage? message = null)
    {
        IsSuccess = isSuccess;
        _value = value;
        _hasValue = isSuccess;
        Message = message;
    }

    /// <inheritdoc/>
    public bool IsSuccess { get; }
    
    /// <inheritdoc/>
    public bool IsFailure => !IsSuccess;
    
    /// <inheritdoc/>
    public IFdwMessage? Message { get; }

    /// <inheritdoc/>
    public T Value
    {
        get
        {
            if (!_hasValue)
                throw new InvalidOperationException("Cannot access value of a failed result.");
            return _value;
        }
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A successful result.</returns>
    public static FdwResult<T> Success(T value) => new(true, value);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed result.</returns>
    public static FdwResult<T> Failure(IFdwMessage message) => new(false, default!, message ?? throw new ArgumentNullException(nameof(message)));
}
