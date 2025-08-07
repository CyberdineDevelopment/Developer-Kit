using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Interface representing the result of a batch secret operation.
/// Provides detailed information about the execution of multiple secret commands.
/// </summary>
/// <remarks>
/// Batch results contain information about both successful and failed operations,
/// allowing callers to understand the outcome of each individual command within the batch.
/// </remarks>
public interface ISecretBatchResult
{
    /// <summary>
    /// Gets the unique identifier for this batch operation.
    /// </summary>
    /// <value>The batch identifier.</value>
    string BatchId { get; }
    
    /// <summary>
    /// Gets the total number of commands in the batch.
    /// </summary>
    /// <value>The total command count.</value>
    int TotalCommands { get; }
    
    /// <summary>
    /// Gets the number of successfully executed commands.
    /// </summary>
    /// <value>The successful command count.</value>
    int SuccessfulCommands { get; }
    
    /// <summary>
    /// Gets the number of failed commands.
    /// </summary>
    /// <value>The failed command count.</value>
    int FailedCommands { get; }
    
    /// <summary>
    /// Gets the number of skipped commands.
    /// </summary>
    /// <value>The skipped command count.</value>
    /// <remarks>
    /// Commands may be skipped due to earlier failures in transactional batches
    /// or when batch processing is halted due to critical errors.
    /// </remarks>
    int SkippedCommands { get; }
    
    /// <summary>
    /// Gets the total execution time for the batch operation.
    /// </summary>
    /// <value>The total execution duration.</value>
    TimeSpan ExecutionTime { get; }
    
    /// <summary>
    /// Gets when the batch operation started.
    /// </summary>
    /// <value>The batch start timestamp.</value>
    DateTimeOffset StartedAt { get; }
    
    /// <summary>
    /// Gets when the batch operation completed.
    /// </summary>
    /// <value>The batch completion timestamp.</value>
    DateTimeOffset CompletedAt { get; }
    
    /// <summary>
    /// Gets the results of individual commands in the batch.
    /// </summary>
    /// <value>A collection of individual command results.</value>
    /// <remarks>
    /// The results are ordered to match the original command sequence in the batch.
    /// Each result contains information about the execution of a specific command.
    /// </remarks>
    IReadOnlyList<ISecretCommandResult> CommandResults { get; }
    
    /// <summary>
    /// Gets any batch-level errors that occurred during processing.
    /// </summary>
    /// <value>A collection of batch-level error messages.</value>
    /// <remarks>
    /// Batch-level errors are issues that affect the entire batch operation,
    /// such as connection failures or provider unavailability, as opposed to
    /// individual command failures.
    /// </remarks>
    IReadOnlyList<string> BatchErrors { get; }
    
    /// <summary>
    /// Gets additional metadata about the batch operation.
    /// </summary>
    /// <value>A dictionary of metadata properties.</value>
    /// <remarks>
    /// Metadata may include provider-specific information, performance metrics,
    /// or other operational details about the batch execution.
    /// </remarks>
    IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Gets a value indicating whether the entire batch operation was successful.
    /// </summary>
    /// <value><c>true</c> if all commands executed successfully; otherwise, <c>false</c>.</value>
    bool IsCompletelySuccessful { get; }
    
    /// <summary>
    /// Gets a value indicating whether any commands in the batch were successful.
    /// </summary>
    /// <value><c>true</c> if at least one command executed successfully; otherwise, <c>false</c>.</value>
    bool IsPartiallySuccessful { get; }
}