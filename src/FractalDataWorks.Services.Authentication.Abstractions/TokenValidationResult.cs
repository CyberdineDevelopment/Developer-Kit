using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Represents the result of token validation.
/// Provides information about the validation outcome and any errors encountered.
/// </summary>
/// <remarks>
/// Validation results help determine whether a token can be trusted
/// and provide diagnostic information for troubleshooting validation failures.
/// </remarks>
public sealed class TokenValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the token validation was successful.
    /// </summary>
    /// <value><c>true</c> if validation succeeded; otherwise, <c>false</c>.</value>
    public bool IsValid { get; }
    
    /// <summary>
    /// Gets any error messages from the validation process.
    /// </summary>
    /// <value>A collection of validation error messages.</value>
    public IReadOnlyList<string> ErrorMessages { get; }
    
    /// <summary>
    /// Gets any warning messages from the validation process.
    /// </summary>
    /// <value>A collection of validation warning messages.</value>
    /// <remarks>
    /// Warnings indicate potential issues that don't prevent validation
    /// but may require attention (e.g., token nearing expiration).
    /// </remarks>
    public IReadOnlyList<string> WarningMessages { get; }
    
    /// <summary>
    /// Gets the exception that caused validation to fail.
    /// </summary>
    /// <value>The validation exception, or null if no exception occurred.</value>
    public Exception? Exception { get; }
    
    /// <summary>
    /// Gets additional validation metadata.
    /// </summary>
    /// <value>A dictionary of validation-related metadata.</value>
    /// <remarks>
    /// Metadata may include information about which validation steps
    /// were performed, timing information, or other diagnostic details.
    /// </remarks>
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenValidationResult"/> class.
    /// </summary>
    /// <param name="isValid">Whether validation was successful.</param>
    /// <param name="errorMessages">Validation error messages.</param>
    /// <param name="warningMessages">Validation warning messages.</param>
    /// <param name="exception">Validation exception.</param>
    /// <param name="metadata">Validation metadata.</param>
    public TokenValidationResult(bool isValid, IReadOnlyList<string>? errorMessages = null, 
        IReadOnlyList<string>? warningMessages = null, Exception? exception = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        IsValid = isValid;
        ErrorMessages = errorMessages ?? Array.Empty<string>();
        WarningMessages = warningMessages ?? Array.Empty<string>();
        Exception = exception;
        Metadata = metadata ?? new Dictionary<string, object>(StringComparer.Ordinal);
    }
    
    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="warningMessages">Optional warning messages.</param>
    /// <param name="metadata">Optional validation metadata.</param>
    /// <returns>A successful validation result.</returns>
    public static TokenValidationResult Success(IReadOnlyList<string>? warningMessages = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new TokenValidationResult(true, null, warningMessages, null, metadata);
    }
    
    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errorMessages">Validation error messages.</param>
    /// <param name="exception">Optional validation exception.</param>
    /// <param name="metadata">Optional validation metadata.</param>
    /// <returns>A failed validation result.</returns>
    public static TokenValidationResult Failure(IReadOnlyList<string> errorMessages,
        Exception? exception = null, IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new TokenValidationResult(false, errorMessages, null, exception, metadata);
    }
    
    /// <summary>
    /// Creates a failed validation result with a single error message.
    /// </summary>
    /// <param name="errorMessage">The validation error message.</param>
    /// <param name="exception">Optional validation exception.</param>
    /// <param name="metadata">Optional validation metadata.</param>
    /// <returns>A failed validation result.</returns>
    public static TokenValidationResult Failure(string errorMessage,
        Exception? exception = null, IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new TokenValidationResult(false, new[] { errorMessage }, null, exception, metadata);
    }
}