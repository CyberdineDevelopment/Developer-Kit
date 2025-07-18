using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Connections.Messages;

/// <summary>
/// Message for connection failures.
/// </summary>
[EnumOption]
public class ConnectionFailed : ConnectionMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionFailed"/> class.
    /// </summary>
    public ConnectionFailed() 
        : base("CONN_002", "Failed to establish connection to {0}", MessageSeverity.Error) { }
}
