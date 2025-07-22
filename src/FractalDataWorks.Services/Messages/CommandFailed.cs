using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message for command execution failures.
/// </summary>
[EnumOption]
public class CommandFailed : ServiceMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandFailed"/> class.
    /// </summary>
    public CommandFailed() 
        : base(4, "CommandFailed", "SVC_004", "Command {0} failed: {1}", MessageSeverity.Error) { }
}
