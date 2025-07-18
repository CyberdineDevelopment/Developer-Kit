using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Messages;

/// <summary>
/// Message for configuration not found errors.
/// </summary>
[EnumOption]
public class ConfigurationNotFound : ConfigurationMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationNotFound"/> class.
    /// </summary>
    public ConfigurationNotFound() 
        : base("CFG_002", "Configuration section '{0}' not found", MessageSeverity.Error) { }
}
