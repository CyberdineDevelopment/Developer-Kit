using System;
using FractalDataWorks.Framework.Abstractions;

namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Interface for batch execution results in the FractalDataWorks framework.
/// Provides information about the outcome of executing multiple data commands as a batch.
/// </summary>
/// <remarks>
/// Batch results aggregate the outcomes of multiple command executions and provide
/// both overall success/failure information and detailed results for individual commands.
/// This enables fine-grained error handling and result processing for batch operations.
/// </remarks>
public interface IBatchResult
{
    /// <summary>
    /// Gets the unique identifier for this batch execution.
    /// </summary>
    /// <value>A unique identifier for the batch execution instance.</value>
    /// <remarks>
    /// Batch identifiers are used for tracking, logging, and debugging purposes.
    /// They help correlate batch operations with individual command executions.
    /// </remarks>
    string BatchId { get; }
    
    /// <summary>
    /// Gets the total number of commands in the batch.
    /// </summary>
    /// <value>The total count of commands that were attempted in the batch.</value>
    int TotalCommands { get; }
    
    /// <summary>
    /// Gets the number of commands that executed successfully.
    /// </summary>
    /// <value>The count of commands that completed without errors.</value>
    int SuccessfulCommands { get; }
    
    /// <summary>
    /// Gets the number of commands that failed during execution.
    /// </summary>
    /// <value>The count of commands that encountered errors during execution.</value>
    int FailedCommands { get; }
    
    /// <summary>
    /// Gets the number of commands that were skipped due to earlier failures.
    /// </summary>
    /// <value>The count of commands that were not executed due to batch processing policies.</value>
    /// <remarks>
    /// Some batch execution strategies may skip remaining commands after encountering
    /// failures, depending on the configured error handling behavior.
    /// </remarks>
    int SkippedCommands { get; }
    
    /// <summary>
    /// Gets a value indicating whether the entire batch completed successfully.
    /// </summary>
    /// <value><c>true</c> if all commands in the batch succeeded; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// This property provides a quick way to determine batch success without
    /// examining individual command results. Useful for simple success/failure scenarios.
    /// </remarks>
    bool IsSuccessful { get; }
    
    /// <summary>
    /// Gets the total time taken to execute the batch.
    /// </summary>
    /// <value>The duration from batch start to completion.</value>
    /// <remarks>
    /// Execution time includes all command processing, data provider overhead,
    /// and any parallelization or batching optimizations applied by the provider.
    /// </remarks>
    TimeSpan ExecutionTime { get; }
    
    /// <summary>
    /// Gets the timestamp when batch execution started.
    /// </summary>
    /// <value>The UTC timestamp when batch execution began.</value>
    DateTimeOffset StartedAt { get; }
    
    /// <summary>
    /// Gets the timestamp when batch execution completed.
    /// </summary>
    /// <value>The UTC timestamp when batch execution finished.</value>
    DateTimeOffset CompletedAt { get; }
    
    /// <summary>
    /// Gets the results of individual commands in the batch.
    /// </summary>
    /// <value>A collection of command results in the same order as the original commands.</value>
    /// <remarks>
    /// Individual command results provide detailed information about each command's
    /// execution, including success/failure status, returned data, and error details.
    /// The collection maintains the same order as the input commands for easy correlation.
    /// </remarks>
    IReadOnlyList<ICommandResult> CommandResults { get; }
    
    /// <summary>
    /// Gets any errors that occurred at the batch level (not specific to individual commands).
    /// </summary>
    /// <value>A collection of batch-level error messages, or empty if no batch errors occurred.</value>
    /// <remarks>
    /// Batch-level errors are those that prevent the entire batch from executing properly,
    /// such as connection failures, transaction errors, or provider-level issues.
    /// These are distinct from individual command execution errors.
    /// </remarks>
    IReadOnlyList<string> BatchErrors { get; }
    
    /// <summary>
    /// Gets additional metadata about the batch execution.
    /// </summary>
    /// <value>A dictionary of metadata properties related to batch execution.</value>
    /// <remarks>
    /// Batch metadata may include information about execution strategies used,
    /// parallelization details, provider-specific optimizations, or performance metrics.
    /// Common metadata keys include "ExecutionStrategy", "ParallelCommands", "BatchSize".
    /// </remarks>
    IReadOnlyDictionary<string, object> BatchMetadata { get; }
    
    /// <summary>
    /// Gets the results of successful commands only.
    /// </summary>
    /// <value>A collection containing only the results of commands that executed successfully.</value>
    /// <remarks>
    /// This property provides convenient access to successful results without needing
    /// to filter the complete command results collection manually.
    /// </remarks>
    IReadOnlyList<ICommandResult> SuccessfulResults { get; }
    
    /// <summary>
    /// Gets the results of failed commands only.
    /// </summary>
    /// <value>A collection containing only the results of commands that failed during execution.</value>
    /// <remarks>
    /// This property provides convenient access to failed results for error analysis
    /// and handling without needing to filter the complete command results collection.
    /// </remarks>
    IReadOnlyList<ICommandResult> FailedResults { get; }
}

/// <summary>
/// Interface for individual command results within a batch execution.
/// Provides detailed information about a single command's execution outcome.
/// </summary>
/// <remarks>
/// Command results provide comprehensive information about individual command executions
/// within a batch, enabling fine-grained result processing and error handling.
/// </remarks>
public interface ICommandResult
{
    /// <summary>
    /// Gets the identifier of the command this result belongs to.
    /// </summary>
    /// <value>The command identifier from the original IDataCommand.</value>
    string CommandId { get; }
    
    /// <summary>
    /// Gets the type of the command this result belongs to.
    /// </summary>
    /// <value>The command type from the original IDataCommand.</value>
    string CommandType { get; }
    
    /// <summary>
    /// Gets the position of this command in the original batch.
    /// </summary>
    /// <value>The zero-based index of the command in the batch.</value>
    /// <remarks>
    /// The batch position helps correlate results with the original command order,
    /// especially when batch execution may reorder commands for optimization.
    /// </remarks>
    int BatchPosition { get; }
    
    /// <summary>
    /// Gets a value indicating whether the command executed successfully.
    /// </summary>
    /// <value><c>true</c> if the command succeeded; otherwise, <c>false</c>.</value>
    bool IsSuccessful { get; }
    
    /// <summary>
    /// Gets the result data returned by the command, if any.
    /// </summary>
    /// <value>The command result data, or null if the command returned no data or failed.</value>
    /// <remarks>
    /// Result data contains the output of successful command execution. The type and
    /// structure of the data depend on the specific command and data provider used.
    /// </remarks>
    object? ResultData { get; }
    
    /// <summary>
    /// Gets the error message if the command failed.
    /// </summary>
    /// <value>The error message describing the failure, or null if the command succeeded.</value>
    string? ErrorMessage { get; }
    
    /// <summary>
    /// Gets additional error details if the command failed.
    /// </summary>
    /// <value>A collection of detailed error information, or empty if the command succeeded.</value>
    /// <remarks>
    /// Error details may include stack traces, provider-specific error codes,
    /// constraint violation details, or other diagnostic information useful for troubleshooting.
    /// </remarks>
    IReadOnlyList<string> ErrorDetails { get; }
    
    /// <summary>
    /// Gets the exception that caused the command failure, if any.
    /// </summary>
    /// <value>The exception that occurred during command execution, or null if no exception occurred.</value>
    Exception? Exception { get; }
    
    /// <summary>
    /// Gets the time taken to execute this command.
    /// </summary>
    /// <value>The duration of command execution, or null if timing information is not available.</value>
    /// <remarks>
    /// Execution time includes data provider processing time but may not include
    /// time spent waiting in queues or for batch coordination.
    /// </remarks>
    TimeSpan? ExecutionTime { get; }
    
    /// <summary>
    /// Gets the timestamp when this command started executing.
    /// </summary>
    /// <value>The UTC timestamp when command execution began, or null if not available.</value>
    DateTimeOffset? StartedAt { get; }
    
    /// <summary>
    /// Gets the timestamp when this command completed executing.
    /// </summary>
    /// <value>The UTC timestamp when command execution finished, or null if not available.</value>
    DateTimeOffset? CompletedAt { get; }
    
    /// <summary>
    /// Gets additional metadata about this command's execution.
    /// </summary>
    /// <value>A dictionary of metadata properties related to command execution.</value>
    /// <remarks>
    /// Command metadata may include provider-specific information, optimization details,
    /// performance metrics, or other data relevant to understanding the command execution.
    /// </remarks>
    IReadOnlyDictionary<string, object> CommandMetadata { get; }
}