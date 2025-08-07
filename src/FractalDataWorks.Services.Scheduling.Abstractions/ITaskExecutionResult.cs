namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Represents the result of a task execution.
/// </summary>
public interface ITaskExecutionResult
{
    /// <summary>
    /// Gets whether the task execution was successful.
    /// </summary>
    bool IsSuccessful { get; }

    /// <summary>
    /// Gets the execution output or result.
    /// </summary>
    object? Result { get; }

    /// <summary>
    /// Gets the error message if the execution failed.
    /// </summary>
    string? ErrorMessage { get; }
}