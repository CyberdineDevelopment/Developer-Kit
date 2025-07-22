using FractalDataWorks;
using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Connections.Messages;

/// <summary>
/// Base class for connection-related messages.
/// Enhanced Enums will generate ConnectionMessages static class.
/// </summary>
[EnhancedEnumBase("ConnectionMessages", ReturnType = "IFdwMessage", ReturnTypeNamespace = "FractalDataWorks")]
public abstract class ConnectionMessageBase : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionMessageBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this message.</param>
    /// <param name="name">The name of this message.</param>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected ConnectionMessageBase(int id, string name, string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(id, name, code, message, severity) { }
}
