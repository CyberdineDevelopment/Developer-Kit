using System;

namespace FractalDataWorks
{
    /// <summary>
    /// Base interface for result types used in async operations.
    /// Provides a discriminated union-like pattern for success/failure handling.
    /// </summary>
    /// <typeparam name="T">The type of the success value</typeparam>
    public interface IGenericResult<T>
    {
        /// <summary>
        /// Gets whether the result represents a success
        /// </summary>
        bool IsSuccess { get; }
        
        /// <summary>
        /// Gets whether the result represents a failure
        /// </summary>
        bool IsFailure { get; }
        
        /// <summary>
        /// Pattern matching helper for processing results
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="success">Function to call on success</param>
        /// <param name="failure">Function to call on failure</param>
        /// <returns>The processed result</returns>
        TResult Match<TResult>(
            Func<T, string, TResult> success,
            Func<IServiceMessage, TResult> failure);
    }
    
    /// <summary>
    /// Interface for service messages (errors, warnings, info)
    /// </summary>
    public interface IServiceMessage
    {
        /// <summary>
        /// Gets the message severity
        /// </summary>
        MessageSeverity Severity { get; }
        
        /// <summary>
        /// Gets the message text
        /// </summary>
        string Message { get; }
        
        /// <summary>
        /// Gets the message code (for localization/categorization)
        /// </summary>
        string Code { get; }
        
        /// <summary>
        /// Gets when the message was created
        /// </summary>
        DateTime Timestamp { get; }
    }
    
    /// <summary>
    /// Message severity levels
    /// </summary>
    public enum MessageSeverity
    {
        /// <summary>
        /// Informational message
        /// </summary>
        Info,
        
        /// <summary>
        /// Warning message
        /// </summary>
        Warning,
        
        /// <summary>
        /// Error message
        /// </summary>
        Error,
        
        /// <summary>
        /// Fatal error message
        /// </summary>
        Fatal
    }
}
