using FractalDataWorks;
using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Tools.Messages;

/// <summary>
/// Base class for tool-related messages.
/// Enhanced Enums will generate ToolMessages static class.
/// </summary>
[EnhancedEnumBase("ToolMessages", ReturnType = "FractalDataWorks.Tools.Messages.IToolMessage")]
public abstract class ToolMessageBase : MessageBase, IToolMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolMessageBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this message.</param>
    /// <param name="name">The name of this message.</param>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected ToolMessageBase(int id, string name, string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(id, name, code, message, severity) { }
}
