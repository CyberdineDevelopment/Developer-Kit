using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Interface for task execution context provided to scheduled tasks.
/// Contains runtime information and services available during task execution.
/// </summary>
/// <remarks>
/// The execution context provides tasks with access to framework services,
/// cancellation mechanisms, and runtime information needed for execution.
/// </remarks>
public interface ITaskExecutionContext
{
    /// <summary>
    /// Gets the unique identifier for this execution instance.
    /// </summary>
    /// <value>A unique identifier for this specific task execution.</value>
    /// <remarks>
    /// Execution identifiers help track individual task runs and correlate
    /// logs, metrics, and results across the system.
    /// </remarks>
    string ExecutionId { get; }
    
    /// <summary>
    /// Gets the scheduled execution time for this task run.
    /// </summary>
    /// <value>The UTC timestamp when this execution was scheduled to run.</value>
    /// <remarks>
    /// Scheduled time may differ from actual start time due to scheduling
    /// delays, resource availability, or dependency completion timing.
    /// </remarks>
    DateTimeOffset ScheduledTime { get; }
    
    /// <summary>
    /// Gets the actual start time for this task execution.
    /// </summary>
    /// <value>The UTC timestamp when task execution actually began.</value>
    DateTimeOffset StartTime { get; }
    
    /// <summary>
    /// Gets the cancellation token for this task execution.
    /// </summary>
    /// <value>A cancellation token that signals when the task should stop executing.</value>
    /// <remarks>
    /// Tasks should regularly check the cancellation token and stop execution
    /// gracefully when cancellation is requested. This enables responsive
    /// task termination and scheduler shutdown.
    /// </remarks>
    CancellationToken CancellationToken { get; }
    
    /// <summary>
    /// Gets the service provider for accessing framework services.
    /// </summary>
    /// <value>The service provider instance for dependency resolution.</value>
    /// <remarks>
    /// The service provider enables tasks to access data providers, external connections,
    /// transformation services, and other framework components during execution.
    /// </remarks>
    IServiceProvider ServiceProvider { get; }
    
    // Logger support would be added here in the future
    // /// <summary>
    // /// Gets the logger instance for this task execution.
    // /// </summary>
    // /// <value>A logger configured for this specific task and execution.</value>
    // /// <remarks>
    // /// The logger is pre-configured with task context information for consistent
    // /// and traceable logging during task execution.
    // /// </remarks>
    // ILogger Logger { get; } // Would require Microsoft.Extensions.Logging reference
    
    /// <summary>
    /// Gets runtime execution metrics for this task run.
    /// </summary>
    /// <value>The execution metrics collector for this task run.</value>
    /// <remarks>
    /// Metrics collector enables tasks to report custom metrics and performance
    /// data during execution for monitoring and optimization purposes.
    /// </remarks>
    ITaskExecutionMetrics Metrics { get; }
    
    /// <summary>
    /// Gets additional context properties for this execution.
    /// </summary>
    /// <value>A dictionary of execution-specific context properties.</value>
    /// <remarks>
    /// Context properties may include scheduler-specific information,
    /// environment details, or custom data provided by the execution environment.
    /// </remarks>
    IReadOnlyDictionary<string, object> Properties { get; }
    
    /// <summary>
    /// Reports progress for long-running task executions.
    /// </summary>
    /// <param name="percentage">The completion percentage (0-100).</param>
    /// <param name="message">An optional progress message.</param>
    /// <remarks>
    /// Progress reporting enables monitoring of long-running tasks and provides
    /// feedback for administrative interfaces and user notifications.
    /// </remarks>
    void ReportProgress(int percentage, string? message = null);
    
    /// <summary>
    /// Sets a checkpoint in task execution for resumption purposes.
    /// </summary>
    /// <param name="checkpointData">The checkpoint data for resuming execution.</param>
    /// <returns>
    /// A task representing the asynchronous checkpoint operation.
    /// </returns>
    /// <remarks>
    /// Checkpoints enable task resumption after interruption or failure,
    /// particularly useful for long-running or multi-step operations.
    /// Not all schedulers support checkpoint functionality.
    /// </remarks>
    Task SetCheckpointAsync(object checkpointData);
}