using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Messages;

/// <summary>
/// Base class for configuration-related messages.
/// Enhanced Enums will generate ConfigurationMessages static class.
/// </summary>
[EnhancedEnumOption("ConfigurationMessages")]
public abstract class ConfigurationMessageBase : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationMessageBase"/> class.
    /// </summary>
    /// <param name="code">The unique code for this message.</param>
    /// <param name="message">The message template text.</param>
    /// <param name="severity">The severity level of this message.</param>
    protected ConfigurationMessageBase(string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(code, message, severity) { }
}
