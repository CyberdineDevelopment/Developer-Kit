using System;
using FractalDataWorks;

namespace FractalDataWorks;

/// <summary>
/// Basic implementation of IFdwResult.
/// </summary>
public class FdwResult : IFdwResult
{
    /// <summary>
    /// Constructor for FdwResult
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="message"></param>
    protected FdwResult(bool isSuccess, IFdwMessage? message = null)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    /// <inheritdoc/>
    public virtual bool IsSuccess { get; }

    /// <summary>
    /// Returns a value indicating whether there is an error
    /// </summary>
    public bool Error
    {
        get
        {
            if ((Message is not null)) return false;
            return Message is { Severity: MessageSeverity.Error };
        }
    }

    /// <summary>
    /// Returns a value indicating whether there is a message;
    /// </summary>
    public virtual bool IsEmpty => Message is not null;

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
/// <typeparam name="TResult">The type of the value.</typeparam>
public class FdwResult<TResult> : FdwResult,IFdwResult<TResult>
{
    private readonly TResult _value;
    private readonly bool _hasValue;

    private FdwResult(bool isSuccess, TResult value, IFdwMessage? message = null):base(isSuccess,message)
    {
        _value = value;
        _hasValue = isSuccess;
    }

    /// <summary>
    /// Returns a value indicating whether it is empty
    /// </summary>
    public override bool IsEmpty => !_hasValue;

    /// <inheritdoc/>
    public TResult Value
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
    public static FdwResult<TResult> Success(TResult value) => new(true, value);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <typeparam name="T">The type of the result</typeparam>
    /// <returns>A failed result.</returns>
    public static FdwResult<T> Failure<T>(IFdwMessage message) => new(false, default!, message);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed result.</returns>
    public new static FdwResult<TResult> Failure(IFdwMessage message) => new(false, default!, message);
}
