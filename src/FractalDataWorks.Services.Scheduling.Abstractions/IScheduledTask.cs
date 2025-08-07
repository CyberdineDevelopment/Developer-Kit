using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Interface for scheduled tasks in the FractalDataWorks framework.
/// Defines the contract for tasks that can be executed by schedulers.
/// </summary>
/// <remarks>
/// Scheduled tasks encapsulate work that needs to be performed on a recurring
/// or one-time basis. They provide execution logic, configuration, and result handling
/// within the framework's scheduling system.
/// </remarks>
public interface IScheduledTask
{
    /// <summary>
    /// Gets the unique identifier for this task.
    /// </summary>
    /// <value>A unique identifier for the task instance.</value>
    /// <remarks>
    /// Task identifiers are used for scheduling, tracking, and management purposes.
    /// They should remain constant for the lifetime of the task definition.
    /// </remarks>
    string TaskId { get; }
    
    /// <summary>
    /// Gets the display name of this task.
    /// </summary>
    /// <value>A human-readable name for the task.</value>
    /// <remarks>
    /// Task names are used in user interfaces, logs, and monitoring displays
    /// to help identify and understand the purpose of scheduled tasks.
    /// </remarks>
    string TaskName { get; }
    
    /// <summary>
    /// Gets the category or type of this task.
    /// </summary>
    /// <value>The task category identifier.</value>
    /// <remarks>
    /// Task categories help organize and filter tasks for management purposes.
    /// Common categories include "DataProcessing", "Maintenance", "Reporting",
    /// "Integration", "Monitoring".
    /// </remarks>
    string TaskCategory { get; }
    
    /// <summary>
    /// Gets the priority of this task for execution ordering.
    /// </summary>
    /// <value>A numeric priority value where higher numbers indicate higher priority.</value>
    /// <remarks>
    /// Task priority influences execution order when multiple tasks are ready
    /// to execute simultaneously. Higher priority tasks are executed first.
    /// </remarks>
    int Priority { get; }
    
    /// <summary>
    /// Gets the expected execution time for this task.
    /// </summary>
    /// <value>The estimated task execution duration, or null if unknown.</value>
    /// <remarks>
    /// Expected execution time helps schedulers plan resource allocation
    /// and detect tasks that are taking longer than normal to complete.
    /// </remarks>
    TimeSpan? ExpectedExecutionTime { get; }
    
    /// <summary>
    /// Gets the maximum allowed execution time for this task.
    /// </summary>
    /// <value>The maximum execution time before the task is considered timed out, or null for no limit.</value>
    /// <remarks>
    /// Execution timeouts prevent runaway tasks from consuming resources
    /// indefinitely and enable automatic task termination when limits are exceeded.
    /// </remarks>
    TimeSpan? MaxExecutionTime { get; }
    
    /// <summary>
    /// Gets the task dependencies that must complete before this task can execute.
    /// </summary>
    /// <value>A collection of task identifiers that this task depends on.</value>
    /// <remarks>
    /// Task dependencies enable workflow coordination and ensure tasks execute
    /// in the correct order. Dependent tasks wait for their dependencies to
    /// complete successfully before starting execution.
    /// </remarks>
    IReadOnlyList<string> Dependencies { get; }
    
    /// <summary>
    /// Gets the configuration parameters for this task.
    /// </summary>
    /// <value>A dictionary of task-specific configuration values.</value>
    /// <remarks>
    /// Configuration parameters provide task-specific settings that influence
    /// execution behavior. The structure depends on the specific task implementation.
    /// </remarks>
    IReadOnlyDictionary<string, object> Configuration { get; }
    
    /// <summary>
    /// Gets additional metadata about this task.
    /// </summary>
    /// <value>A dictionary of task metadata properties.</value>
    /// <remarks>
    /// Metadata provides additional context about the task such as owner information,
    /// version details, or integration-specific properties that may be useful
    /// for task management and monitoring.
    /// </remarks>
    IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Gets a value indicating whether this task can be executed concurrently with itself.
    /// </summary>
    /// <value><c>true</c> if multiple instances of this task can run simultaneously; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Some tasks may not be safe for concurrent execution due to resource conflicts
    /// or data consistency requirements. Non-concurrent tasks are queued when already executing.
    /// </remarks>
    bool AllowsConcurrentExecution { get; }
    
    /// <summary>
    /// Executes the task with the provided execution context.
    /// </summary>
    /// <param name="context">The execution context containing runtime information and services.</param>
    /// <returns>
    /// A task representing the asynchronous execution operation.
    /// The result contains the execution outcome and any result data.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <remarks>
    /// This method contains the actual work logic for the task. It receives
    /// an execution context that provides access to framework services,
    /// cancellation tokens, and runtime information.
    /// </remarks>
    Task<IFdwResult<object?>> ExecuteAsync(ITaskExecutionContext context);
    
    /// <summary>
    /// Validates the task configuration and dependencies.
    /// </summary>
    /// <returns>A result indicating whether the task is properly configured and ready for execution.</returns>
    /// <remarks>
    /// This method enables early validation of task configuration before
    /// scheduling or execution. It can check for required configuration
    /// parameters, dependency availability, and other prerequisites.
    /// </remarks>
    IFdwResult ValidateTask();
    
    /// <summary>
    /// Handles cleanup operations when the task is cancelled or fails.
    /// </summary>
    /// <param name="context">The execution context used during task execution.</param>
    /// <param name="reason">The reason for cleanup (cancellation, failure, etc.).</param>
    /// <returns>
    /// A task representing the asynchronous cleanup operation.
    /// </returns>
    /// <remarks>
    /// This method is called when task execution is interrupted or completes
    /// with an error. It enables tasks to perform necessary cleanup operations
    /// such as releasing resources or rolling back partial changes.
    /// </remarks>
    Task OnCleanupAsync(ITaskExecutionContext context, string reason);
}