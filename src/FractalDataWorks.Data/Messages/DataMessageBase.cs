using FractalDataWorks;
using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Data.Messages;

/// <summary>
/// Base class for data-related messages.
/// Enhanced Enums will generate DataMessages static class.
/// </summary>
[EnhancedEnumBase("DataMessages", ReturnType = "FractalDataWorks.Data.Messages.IDataMessage")]
public abstract class DataMessageBase : MessageBase, IDataMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMessageBase"/> class.
    /// </summary>
    /// <param name="id">The id for this message.</param>
    /// <param name="name">The name of this message.</param>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected DataMessageBase(int id, string name, string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(id, name, code, message, severity) { }
}
