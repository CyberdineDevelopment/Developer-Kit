namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Represents task information and status.
/// </summary>
public interface ITaskInfo
{
    /// <summary>
    /// Gets the unique task identifier.
    /// </summary>
    string TaskId { get; }

    /// <summary>
    /// Gets the current task status.
    /// </summary>
    string Status { get; }
}