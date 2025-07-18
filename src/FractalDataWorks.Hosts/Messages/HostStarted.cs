using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Hosts.Messages;

/// <summary>
/// Message for host started events.
/// </summary>
[EnumOption]
public class HostStarted : HostMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HostStarted"/> class.
    /// </summary>
    public HostStarted() 
        : base("HOST_002", "Host {0} started successfully on {1}", MessageSeverity.Information) { }
}
