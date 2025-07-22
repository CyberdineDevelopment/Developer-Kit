using FractalDataWorks;
using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Messages;

/// <summary>
/// Base class for configuration-related messages.
/// Enhanced Enums will generate ConfigurationMessages static class.
/// </summary>
[EnhancedEnumBase("ConfigurationMessages", ReturnType = "IFdwMessage", ReturnTypeNamespace = "FractalDataWorks")]
public abstract class ConfigurationMessageBase : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationMessageBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this message.</param>
    /// <param name="name">The name of this message.</param>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected ConfigurationMessageBase(int id, string name, string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(id, name, code, message, severity) { }
}
