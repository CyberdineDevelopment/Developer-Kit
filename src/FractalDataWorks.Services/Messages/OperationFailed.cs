using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message for general operation failures.
/// </summary>
[EnumOption]
public class OperationFailed : ServiceMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperationFailed"/> class.
    /// </summary>
    public OperationFailed() 
        : base("SVC_005", "Operation {0} failed: {1}", MessageSeverity.Error) { }
}
