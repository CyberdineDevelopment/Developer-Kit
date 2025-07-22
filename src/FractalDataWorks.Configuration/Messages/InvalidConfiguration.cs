using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Messages;

/// <summary>
/// Message for invalid configuration errors.
/// </summary>
[EnumOption]
public class InvalidConfiguration : ConfigurationMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidConfiguration"/> class.
    /// </summary>
    public InvalidConfiguration() 
        : base(1, "InvalidConfiguration", "CFG_001", "Invalid configuration for {0}: {1}", MessageSeverity.Error) { }
}
