using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Hosts.Messages;

/// <summary>
/// Message for host stopping events.
/// </summary>
[EnumOption]
public class HostStopping : HostMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HostStopping"/> class.
    /// </summary>
    public HostStopping() 
        : base(3, "HostStopping", "HOST_003", "Host {0} is shutting down...", MessageSeverity.Information) { }
}
