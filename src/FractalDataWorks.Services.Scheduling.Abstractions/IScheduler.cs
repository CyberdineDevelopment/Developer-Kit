using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Interface for schedulers in the FractalDataWorks framework.
/// Provides task scheduling, execution management, and monitoring capabilities.
/// </summary>
/// <remarks>
/// Schedulers manage the execution of tasks, jobs, and workflows within the framework.
/// They handle timing, dependencies, resource allocation, and execution coordination
/// to ensure reliable and efficient processing of scheduled operations.
/// </remarks>
public interface IScheduler : IFdwService
{
    /// <summary>
    /// Gets the scheduling strategies supported by this scheduler.
    /// </summary>
    /// <value>A collection of scheduling strategy identifiers.</value>
    /// <remarks>
    /// Scheduling strategies define how tasks are scheduled and executed,
    /// such as "Cron", "Interval", "Once", "Dependent", "Manual".
    /// This enables task-scheduler matching based on scheduling requirements.
    /// </remarks>
    IReadOnlyList<string> SupportedSchedulingStrategies { get; }
    
    /// <summary>
    /// Gets the task execution modes supported by this scheduler.
    /// </summary>
    /// <value>A collection of execution mode identifiers.</value>
    /// <remarks>
    /// Execution modes specify how tasks are executed, such as "Sequential",
    /// "Parallel", "Batch", "Pipeline". This enables optimization of task
    /// execution based on resource availability and performance requirements.
    /// </remarks>
    IReadOnlyList<string> SupportedExecutionModes { get; }
    
    /// <summary>
    /// Gets the maximum number of concurrent tasks this scheduler can handle.
    /// </summary>
    /// <value>The maximum concurrent task limit, or null if no limit.</value>
    /// <remarks>
    /// Concurrency limits help prevent resource exhaustion and enable
    /// capacity planning for scheduled operations. Schedulers may queue
    /// tasks when limits are reached.
    /// </remarks>
    int? MaxConcurrentTasks { get; }
    
    /// <summary>
    /// Gets the current number of active tasks being executed.
    /// </summary>
    /// <value>The count of currently executing tasks.</value>
    int ActiveTaskCount { get; }
    
    /// <summary>
    /// Gets the current number of tasks waiting in the execution queue.
    /// </summary>
    /// <value>The count of queued tasks awaiting execution.</value>
    int QueuedTaskCount { get; }
    
    /// <summary>
    /// Schedules a task for execution.
    /// </summary>
    /// <param name="task">The task to schedule.</param>
    /// <param name="schedule">The schedule configuration for the task.</param>
    /// <returns>
    /// A task representing the asynchronous scheduling operation.
    /// The result contains the scheduled task identifier if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="task"/> or <paramref name="schedule"/> is null.
    /// </exception>
    /// <remarks>
    /// This method adds a task to the scheduler with the specified schedule configuration.
    /// The task will be executed according to the schedule parameters and strategy.
    /// </remarks>
    Task<IFdwResult<string>> ScheduleTask(IScheduledTask task, ITaskSchedule schedule);
    
    /// <summary>
    /// Cancels a scheduled task.
    /// </summary>
    /// <param name="taskId">The identifier of the task to cancel.</param>
    /// <returns>
    /// A task representing the asynchronous cancellation operation.
    /// The result indicates whether the cancellation was successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="taskId"/> is empty or whitespace.</exception>
    /// <remarks>
    /// Cancelling a task removes it from the schedule and stops any future executions.
    /// If the task is currently executing, it will be requested to stop gracefully.
    /// </remarks>
    Task<IFdwResult> CancelTask(string taskId);
    
    /// <summary>
    /// Pauses a scheduled task temporarily.
    /// </summary>
    /// <param name="taskId">The identifier of the task to pause.</param>
    /// <returns>
    /// A task representing the asynchronous pause operation.
    /// The result indicates whether the pause was successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="taskId"/> is empty or whitespace.</exception>
    /// <remarks>
    /// Pausing a task prevents new executions but preserves the schedule configuration.
    /// Paused tasks can be resumed to continue scheduled execution.
    /// </remarks>
    Task<IFdwResult> PauseTask(string taskId);
    
    /// <summary>
    /// Resumes a paused scheduled task.
    /// </summary>
    /// <param name="taskId">The identifier of the task to resume.</param>
    /// <returns>
    /// A task representing the asynchronous resume operation.
    /// The result indicates whether the resume was successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="taskId"/> is empty or whitespace.</exception>
    /// <remarks>
    /// Resuming a task restores normal scheduled execution according to
    /// the original schedule configuration.
    /// </remarks>
    Task<IFdwResult> ResumeTask(string taskId);
    
    /// <summary>
    /// Executes a task immediately, bypassing the normal schedule.
    /// </summary>
    /// <param name="taskId">The identifier of the task to execute immediately.</param>
    /// <returns>
    /// A task representing the asynchronous execution operation.
    /// The result contains the task execution outcome.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="taskId"/> is empty or whitespace.</exception>
    /// <remarks>
    /// Manual execution does not affect the normal schedule - the task will
    /// continue to execute according to its configured schedule.
    /// </remarks>
    Task<IFdwResult<ITaskExecutionResult>> ExecuteTaskNow(string taskId);
    
    /// <summary>
    /// Gets the status and information for a scheduled task.
    /// </summary>
    /// <param name="taskId">The identifier of the task to query.</param>
    /// <returns>
    /// A task representing the asynchronous query operation.
    /// The result contains the task information if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="taskId"/> is empty or whitespace.</exception>
    /// <remarks>
    /// Task information includes current status, execution history,
    /// schedule configuration, and performance metrics.
    /// </remarks>
    Task<IFdwResult<ITaskInfo>> GetTaskInfo(string taskId);
    
    /// <summary>
    /// Gets information for all scheduled tasks in this scheduler.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous query operation.
    /// The result contains a collection of task information.
    /// </returns>
    /// <remarks>
    /// This method provides an overview of all tasks managed by the scheduler,
    /// useful for monitoring and administrative purposes.
    /// </remarks>
    Task<IFdwResult<IReadOnlyList<ITaskInfo>>> GetAllTasks();
    
    /// <summary>
    /// Gets scheduling and execution metrics for this scheduler.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous metrics collection operation.
    /// The result contains scheduler performance metrics if available.
    /// </returns>
    /// <remarks>
    /// Metrics help monitor scheduler performance, identify bottlenecks,
    /// and optimize task execution strategies.
    /// </remarks>
    Task<IFdwResult<ISchedulerMetrics>> GetSchedulerMetricsAsync();
    
    /// <summary>
    /// Creates a task execution context for advanced task management.
    /// </summary>
    /// <param name="configuration">The execution context configuration.</param>
    /// <returns>
    /// A task representing the asynchronous context creation operation.
    /// The result contains the execution context if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <remarks>
    /// Execution contexts provide advanced features like task dependencies,
    /// resource allocation, and execution coordination for complex workflows.
    /// </remarks>
    Task<IFdwResult<ITaskExecutor>> CreateExecutionContextAsync(ITaskExecutorConfiguration configuration);
}