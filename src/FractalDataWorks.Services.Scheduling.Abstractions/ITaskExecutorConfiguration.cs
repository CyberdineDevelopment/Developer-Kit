namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Represents a task executor configuration.
/// </summary>
public interface ITaskExecutorConfiguration
{
    /// <summary>
    /// Gets the execution mode.
    /// </summary>
    string ExecutionMode { get; }
}