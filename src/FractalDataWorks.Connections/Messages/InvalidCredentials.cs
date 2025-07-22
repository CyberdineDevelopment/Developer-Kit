using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Connections.Messages;

/// <summary>
/// Message for invalid credential errors.
/// </summary>
[EnumOption]
public class InvalidCredentials : ConnectionMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidCredentials"/> class.
    /// </summary>
    public InvalidCredentials() 
        : base(4, "InvalidCredentials", "CONN_004", "Invalid credentials provided for {0}", MessageSeverity.Error) { }
}
