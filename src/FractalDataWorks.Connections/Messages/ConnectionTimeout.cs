using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Connections.Messages;

/// <summary>
/// Message for connection timeout errors.
/// </summary>
[EnumOption]
public class ConnectionTimeout : ConnectionMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTimeout"/> class.
    /// </summary>
    public ConnectionTimeout() 
        : base("CONN_003", "Connection to {0} timed out after {1} seconds", MessageSeverity.Error) { }
}
