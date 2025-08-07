using System;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Represents task execution metrics.
/// </summary>
public interface ITaskExecutionMetrics
{
    /// <summary>
    /// Gets the total execution time.
    /// </summary>
    TimeSpan ExecutionTime { get; }

    /// <summary>
    /// Gets the number of times the task has been executed.
    /// </summary>
    int ExecutionCount { get; }
}