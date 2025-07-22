using System;
using System.Threading.Tasks;
using FractalDataWorks.Validation;

namespace FractalDataWorks;

/// <summary>
/// Represents a command that can be executed.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the unique identifier for this command instance.
    /// </summary>
    Guid CommandId { get; }
    
    /// <summary>
    /// Gets the correlation identifier for tracking related operations.
    /// </summary>
    Guid CorrelationId { get; }
    
    /// <summary>
    /// Gets the timestamp when this command was created.
    /// </summary>
    DateTime Timestamp { get; }
    
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
