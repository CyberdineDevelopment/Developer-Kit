using System;

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
    protected FdwResult(bool isSuccess, string? message = null)
    {
        IsSuccess = isSuccess;
        Message = message ?? string.Empty;
    }

    /// <inheritdoc/>
    public virtual bool IsSuccess { get; }

    /// <inheritdoc/>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Returns a value indicating whether there is an error
    /// </summary>
    public bool Error => !IsSuccess;

    /// <summary>
    /// Returns a value indicating whether there is a message;
    /// </summary>
    public virtual bool IsEmpty => string.IsNullOrEmpty(Message);

    /// <inheritdoc/>
    public string Message { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static IFdwResult Success() => new FdwResult(true);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure(string message) => new FdwResult(false, message);

    /// <summary>
    /// Creates a failed result with an IFdwMessage.
    /// </summary>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public static IFdwResult Failure(IFdwMessage message) => new FdwResult(false, message?.Message);

}

/// <summary>
/// Basic implementation of IFdwResult with a value.
/// </summary>
/// <typeparam name="TResult">The type of the value.</typeparam>
public class FdwResult<TResult> : FdwResult, IFdwResult<TResult>
{
    private readonly TResult _value;
    private readonly bool _hasValue;

    private FdwResult(bool isSuccess, TResult value, string? message = null) : base(isSuccess, message)
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
    public static IFdwResult<TResult> Success(TResult value) => new FdwResult<TResult>(true, value);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <typeparam name="T">The type of the result</typeparam>
    /// <returns>A failed result.</returns>
    public static IFdwResult<T> Failure<T>(string message) => new FdwResult<T>(false, default!, message);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed result.</returns>
    public new static IFdwResult<TResult> Failure(string message) => new FdwResult<TResult>(false, default!, message);

    /// <summary>
    /// Creates a failed result with an IFdwMessage.
    /// </summary>
    /// <param name="message">The failure message object.</param>
    /// <returns>A failed result.</returns>
    public new static IFdwResult<TResult> Failure(IFdwMessage message) => new FdwResult<TResult>(false, default!, message?.Message);


    /// <inheritdoc/>
    public IGenericResult<TNew> Map<TNew>(Func<TResult, TNew> mapper)
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        return IsSuccess
            ? (IGenericResult<TNew>)FdwResult<TNew>.Success(mapper(Value))
            : FdwResult<TNew>.Failure(Message);
    }

    /// <inheritdoc/>
    public T Match<T>(Func<TResult, T> success, Func<string, T> failure)
    {
        if (success == null)
            throw new ArgumentNullException(nameof(success));
        if (failure == null)
            throw new ArgumentNullException(nameof(failure));

        return IsSuccess ? success(Value) : failure(Message);
    }
}