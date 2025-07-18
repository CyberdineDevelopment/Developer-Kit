using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Message for service started events.
/// </summary>
[EnumOption]
public class ServiceStarted : ServiceMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceStarted"/> class.
    /// </summary>
    public ServiceStarted() 
        : base("SVC_001", "Service {0} started successfully", MessageSeverity.Information) { }
}
