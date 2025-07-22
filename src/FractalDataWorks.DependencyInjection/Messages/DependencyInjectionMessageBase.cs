using FractalDataWorks;
using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.DependencyInjection.Messages;

/// <summary>
/// Base class for dependency injection-related messages.
/// Enhanced Enums will generate DependencyInjectionMessages static class.
/// </summary>
[EnhancedEnumBase("DependencyInjectionMessages", ReturnType = "FractalDataWorks.DependencyInjection.Messages.IDependencyInjectionMessage")]
public abstract class DependencyInjectionMessageBase : MessageBase, IDependencyInjectionMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyInjectionMessageBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this message.</param>
    /// <param name="name">The name for enhanced enum lookups.</param>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected DependencyInjectionMessageBase(int id, string name, string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(id, name, code, message, severity) { }
}