using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Messages;

/// <summary>
/// Message for configuration validation failures.
/// </summary>
[EnumOption]
public class ValidationFailed : ConfigurationMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFailed"/> class.
    /// </summary>
    public ValidationFailed() 
        : base("CFG_003", "Configuration validation failed: {0}", MessageSeverity.Error) { }
}
