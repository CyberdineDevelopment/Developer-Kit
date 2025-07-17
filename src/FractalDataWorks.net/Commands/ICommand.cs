using System;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using FractalDataWorks.Validation;

namespace FractalDataWorks.Commands;

/// <summary>
/// Defines the base contract for all commands in the Fractal framework.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    Guid CommandId { get; }

    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the correlation identifier for tracking related operations.
    /// </summary>
    Guid CorrelationId { get; }

    /// <summary>
    /// Gets the configuration associated with this command.
    /// </summary>
    IFdwConfiguration? Configuration { get; }

    /// <summary>
    /// Validates this command.
    /// </summary>
    /// <returns>A task containing the validation result.</returns>
    Task<IValidationResult> Validate();
}

/// <summary>
/// Defines a command with a specific result type.
/// </summary>
/// <typeparam name="TResult">The type of result this command produces.</typeparam>
public interface ICommand<TResult> : ICommand
{
    /// <summary>
    /// Executes the command logic.
    /// </summary>
    /// <returns>A task containing the command result.</returns>
    Task<FdwResult<TResult>> Execute();
}