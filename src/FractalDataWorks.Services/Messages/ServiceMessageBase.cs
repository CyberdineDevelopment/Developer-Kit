using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Base class for service-related messages.
/// Enhanced Enums will generate ServiceMessages static class.
/// Source generator can extend this across assemblies.
/// </summary>
[EnhancedEnumOption("ServiceMessages")]
public abstract class ServiceMessageBase : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceMessageBase"/> class.
    /// </summary>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected ServiceMessageBase(string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(code, message, severity) { }
}
