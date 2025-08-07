namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Represents a task schedule configuration.
/// </summary>
public interface ITaskSchedule
{
    /// <summary>
    /// Gets the scheduling strategy.
    /// </summary>
    string ScheduleStrategy { get; }

    /// <summary>
    /// Gets the interval or cron expression.
    /// </summary>
    string ScheduleExpression { get; }
}