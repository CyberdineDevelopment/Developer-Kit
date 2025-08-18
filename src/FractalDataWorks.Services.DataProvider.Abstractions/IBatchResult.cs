using System;
using System.Collections.Generic;


namespace FractalDataWorks.Services.DataProvider.Abstractions;

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