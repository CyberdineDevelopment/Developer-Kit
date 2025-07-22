using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Messages;

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
        : base(2, "ConnectionFailed", "CONN_002", "Failed to establish connection to {0}", MessageSeverity.Error) { }
}
