using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Hosts.Messages;

/// <summary>
/// Message for host starting events.
/// </summary>
[EnumOption]
public class HostStarting : HostMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HostStarting"/> class.
    /// </summary>
    public HostStarting() 
        : base(1, "HostStarting", "HOST_001", "Host {0} is starting...", MessageSeverity.Information) { }
}
