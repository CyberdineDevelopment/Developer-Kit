using System.Threading.Tasks;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Represents a task executor.
/// </summary>
public interface ITaskExecutor
{
    /// <summary>
    /// Executes a task.
    /// </summary>
    /// <returns>The task execution result.</returns>
    Task<ITaskExecutionResult> ExecuteAsync();
}