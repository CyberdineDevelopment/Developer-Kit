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
    /// Gets the timestamp when this command was created.
    /// </summary>
    DateTimeOffset Timestamp { get; }
}
