using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Tools.Messages;

/// <summary>
/// Base class for tool-related messages.
/// Enhanced Enums will generate ToolMessages static class.
/// </summary>
[EnhancedEnumOption("ToolMessages")]
public abstract class ToolMessageBase : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolMessageBase"/> class.
    /// </summary>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected ToolMessageBase(string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(code, message, severity) { }
}
