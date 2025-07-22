using FractalDataWorks;
using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Base class for service-related messages.
/// Enhanced Enums will generate ServiceMessages static class.
/// </summary>
[EnhancedEnumBase("ServiceMessages", ReturnType = "IFdwMessage", ReturnTypeNamespace = "FractalDataWorks")]
public abstract class ServiceMessageBase : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceMessageBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this message.</param>
    /// <param name="name">The name of this message.</param>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected ServiceMessageBase(int id, string name, string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(id, name, code, message, severity) { }
}
