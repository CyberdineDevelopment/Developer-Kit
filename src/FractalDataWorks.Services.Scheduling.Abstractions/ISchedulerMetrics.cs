namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Represents scheduler metrics.
/// </summary>
public interface ISchedulerMetrics
{
    /// <summary>
    /// Gets the total number of tasks managed.
    /// </summary>
    int TotalTasks { get; }

    /// <summary>
    /// Gets the number of currently running tasks.
    /// </summary>
    int RunningTasks { get; }
}