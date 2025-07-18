using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Connections.Messages;

/// <summary>
/// Base class for connection-related messages.
/// Enhanced Enums will generate ConnectionMessages static class.
/// </summary>
[EnhancedEnumOption("ConnectionMessages")]
public abstract class ConnectionMessageBase : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionMessageBase"/> class.
    /// </summary>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected ConnectionMessageBase(string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(code, message, severity) { }
}
