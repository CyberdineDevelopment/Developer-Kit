using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message for successful command execution.
/// </summary>
[EnumOption]
public class CommandExecuted : ServiceMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandExecuted"/> class.
    /// </summary>
    public CommandExecuted() 
        : base("SVC_003", "Command {0} executed successfully in {1}ms", MessageSeverity.Information) { }
}
