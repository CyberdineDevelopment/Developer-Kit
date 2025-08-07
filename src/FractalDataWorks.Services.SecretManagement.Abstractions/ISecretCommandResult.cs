using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Interface representing the result of an individual secret command within a batch.
/// Provides detailed information about the execution of a single command.
/// </summary>
/// <remarks>
/// Command results contain both success and failure information, allowing
/// batch processors to understand the outcome of each individual operation.
/// </remarks>
public interface ISecretCommandResult
{
    /// <summary>
    /// Gets the original command that was executed.
    /// </summary>
    /// <value>The executed secret command.</value>
    ISecretCommand Command { get; }
    
    /// <summary>
    /// Gets the position of this command in the original batch.
    /// </summary>
    /// <value>The zero-based position in the batch.</value>
    int BatchPosition { get; }
    
    /// <summary>
    /// Gets a value indicating whether the command executed successfully.
    /// </summary>
    /// <value><c>true</c> if the command was successful; otherwise, <c>false</c>.</value>
    bool IsSuccessful { get; }
    
    /// <summary>
    /// Gets the result data from the command execution.
    /// </summary>
    /// <value>The command result data, or null if the command failed or returned no data.</value>
    object? ResultData { get; }
    
    /// <summary>
    /// Gets the error message if the command failed.
    /// </summary>
    /// <value>The error message, or null if the command was successful.</value>
    string? ErrorMessage { get; }
    
    /// <summary>
    /// Gets additional error details if the command failed.
    /// </summary>
    /// <value>A collection of detailed error information, or empty if the command was successful.</value>
    IReadOnlyList<string> ErrorDetails { get; }
    
    /// <summary>
    /// Gets the exception that caused the command to fail.
    /// </summary>
    /// <value>The exception, or null if no exception occurred.</value>
    Exception? Exception { get; }
    
    /// <summary>
    /// Gets the execution time for this specific command.
    /// </summary>
    /// <value>The command execution duration.</value>
    TimeSpan ExecutionTime { get; }
    
    /// <summary>
    /// Gets when this command started executing.
    /// </summary>
    /// <value>The command start timestamp.</value>
    DateTimeOffset StartedAt { get; }
    
    /// <summary>
    /// Gets when this command completed executing.
    /// </summary>
    /// <value>The command completion timestamp.</value>
    DateTimeOffset CompletedAt { get; }
    
    /// <summary>
    /// Gets additional metadata about the command execution.
    /// </summary>
    /// <value>A dictionary of metadata properties.</value>
    /// <remarks>
    /// Metadata may include provider-specific information, performance metrics,
    /// or other operational details about the command execution.
    /// </remarks>
    IReadOnlyDictionary<string, object> Metadata { get; }
}