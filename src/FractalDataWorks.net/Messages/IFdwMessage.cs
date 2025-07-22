namespace FractalDataWorks.Messages;

/// <summary>
/// Defines the contract for all messages in the Fractal framework.
/// </summary>
public interface IFdwMessage
{
    /// <summary>
    /// Gets the unique code for this message.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the message text.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the severity level of this message.
    /// </summary>
    MessageSeverity Severity { get; }

    /// <summary>
    /// Formats this message with the specified arguments.
    /// </summary>
    /// <param name="args">The format arguments.</param>
    /// <returns>A formatted message instance.</returns>
    IFdwMessage Format(params object[] args);

    /// <summary>
    /// Creates a new message with the specified severity.
    /// </summary>
    /// <param name="severity">The new severity level.</param>
    /// <returns>A new message instance with the updated severity.</returns>
    IFdwMessage WithSeverity(MessageSeverity severity);
}