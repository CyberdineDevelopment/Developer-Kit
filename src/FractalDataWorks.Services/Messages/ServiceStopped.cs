using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message for service stopped events.
/// </summary>
[EnumOption]
public class ServiceStopped : ServiceMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceStopped"/> class.
    /// </summary>
    public ServiceStopped() 
        : base(2, "ServiceStopped", "SVC_002", "Service {0} stopped", MessageSeverity.Information) { }
}
