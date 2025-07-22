using System;

namespace FractalDataWorks;

/// <summary>
/// Represents the result of a command execution.
/// </summary>
public interface ICommandResult
{
    /// <summary>
    /// Gets a value indicating whether the command execution was successful.
    /// </summary>
    bool IsSuccess { get; }
    
    /// <summary>
    /// Gets the command that was executed.
    /// </summary>
    ICommand Command { get; }
    
    /// <summary>
    /// Gets the duration of the command execution.
    /// </summary>
    TimeSpan Duration { get; }
}
