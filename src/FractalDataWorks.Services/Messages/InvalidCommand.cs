using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message for invalid command errors.
/// </summary>
[EnumOption]
public class InvalidCommand : ServiceMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidCommand"/> class.
    /// </summary>
    public InvalidCommand() 
        : base(6, "InvalidCommand", "SVC_006", "Invalid command: {0}", MessageSeverity.Error) { }
}