using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Data.Messages;

/// <summary>
/// Base class for data-related messages.
/// Enhanced Enums will generate DataMessages static class.
/// </summary>
[EnhancedEnumOption("DataMessages")]
public abstract class DataMessageBase : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMessageBase"/> class.
    /// </summary>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected DataMessageBase(string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(code, message, severity) { }
}
