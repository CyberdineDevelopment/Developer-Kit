using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Connections.Messages;

/// <summary>
/// Message for authorization failures.
/// Adding this class automatically includes it in ConnectionMessages collection.
/// </summary>
[EnumOption]
public class NotAuthorized : ConnectionMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotAuthorized"/> class.
    /// </summary>
    public NotAuthorized() 
        : base(1, "NotAuthorized", "CONN_001", "Not authorized to perform {0}", MessageSeverity.Error) { }
}
