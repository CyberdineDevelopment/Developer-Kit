using System;
using FractalDataWorks;

namespace FractalDataWorks.Entities
{
    /// <summary>
    /// Discriminated union-like result type for async operations.
    /// Implementation of IGenericResult using modern C# features.
    /// </summary>
    /// <typeparam name="T">The type of the success value</typeparam>
    public abstract record Result<T> : IGenericResult<T>
    {
        private Result() { }
        
        /// <summary>
        /// Represents a successful operation with a value
        /// </summary>
        public sealed record Success(T Value, string Message = "Success") : Result<T>;
        
        /// <summary>
        /// Represents a failed operation with an error
        /// </summary>
        public sealed record Failure(ServiceMessage Error) : Result<T>;
        
        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static Result<T> Ok(T value, string message = "Success") 
            => new Success(value, message);
            
        /// <summary>
        /// Creates a failed result with an error message
        /// </summary>
        public static Result<T> Fail(string error) 
            => new Failure(ServiceMessage.Error(error));
            
        /// <summary>
        /// Creates a failed result with a service message
        /// </summary>
        public static Result<T> Fail(ServiceMessage error) 
            => new Failure(error);
        
        /// <summary>
        /// Pattern matching helper for processing results
        /// </summary>
        public TResult Match<TResult>(
            Func<T, string, TResult> success,
            Func<IServiceMessage, TResult> failure) =>
            this switch
            {
                Success(var value, var message) => success(value, message),
                Failure(var error) => failure(error),
                _ => throw new InvalidOperationException("Invalid result state")
            };
        
        /// <summary>
        /// Gets whether the result represents a success
        /// </summary>
        public bool IsSuccess => this is Success;
        
        /// <summary>
        /// Gets whether the result represents a failure
        /// </summary>
        public bool IsFailure => this is Failure;
        
        /// <summary>
        /// Gets the error if this is a failure, otherwise null
        /// </summary>
        public ServiceMessage? Error => this is Failure failure ? failure.Error : null;
        
        /// <summary>
        /// Gets the value if this is a success, otherwise default
        /// </summary>
        public T? Value => this is Success success ? success.Value : default;
    }
}