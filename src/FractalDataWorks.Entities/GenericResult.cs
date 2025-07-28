using System;

namespace FractalDataWorks.Entities;

/// <summary>
/// Generic result implementation for operations
/// </summary>
public class GenericResult<T> : IGenericResult<T>
{
    private readonly bool _isSuccess;
    private readonly T? _value;
    private readonly ServiceMessage? _error;
    
    private GenericResult(bool isSuccess, T? value, ServiceMessage? error)
    {
        _isSuccess = isSuccess;
        _value = value;
        _error = error;
    }
    
    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;
    
    public T Value => _isSuccess ? _value! : throw new InvalidOperationException("Cannot access Value on failure result");
    public string Message => _isSuccess ? "Success" : _error?.Message ?? "Unknown error";
    
    public TResult Match<TResult>(
        Func<T, string, TResult> success,
        Func<IServiceMessage, TResult> failure)
    {
        return _isSuccess 
            ? success(_value!, Message)
            : failure(_error ?? ServiceMessage.Error("Unknown error"));
    }
    
    public static GenericResult<T> Success(T value, string message = "Success")
    {
        return new GenericResult<T>(true, value, null);
    }
    
    public static GenericResult<T> Failure(string error)
    {
        return new GenericResult<T>(false, default, ServiceMessage.Error(error));
    }
    
    public static GenericResult<T> Failure(ServiceMessage error)
    {
        return new GenericResult<T>(false, default, error);
    }
}