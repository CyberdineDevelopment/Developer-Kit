using FractalDataWorks;
using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Hosts.Messages;

/// <summary>
/// Base class for host-related messages.
/// Enhanced Enums will generate HostMessages static class.
/// </summary>
[EnhancedEnumBase("HostMessages", ReturnType = "FractalDataWorks.Hosts.Messages.IHostMessage")]
public abstract class HostMessageBase : MessageBase, IHostMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HostMessageBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this message.</param>
    /// <param name="name">The name of this message.</param>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected HostMessageBase(int id, string name, string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(id, name, code, message, severity) { }
}
