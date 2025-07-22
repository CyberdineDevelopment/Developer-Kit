using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message for validation failure errors.
/// </summary>
[EnumOption]
public class ValidationFailed : ServiceMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFailed"/> class.
    /// </summary>
    public ValidationFailed() 
        : base(8, "ValidationFailed", "SVC_008", "Validation failed: {0}", MessageSeverity.Error) { }
}