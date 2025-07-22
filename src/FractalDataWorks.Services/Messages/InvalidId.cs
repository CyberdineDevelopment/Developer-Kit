using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message for invalid ID errors.
/// </summary>
[EnumOption]
public class InvalidId : ServiceMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidId"/> class.
    /// </summary>
    public InvalidId() 
        : base(9, "InvalidId", "SVC_009", "Invalid ID: {0}", MessageSeverity.Error) { }
}