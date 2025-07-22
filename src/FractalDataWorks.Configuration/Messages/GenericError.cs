using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Messages;

/// <summary>
/// Generic error message that can be formatted with custom text.
/// </summary>
[EnumOption]
public class GenericError : ConfigurationMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericError"/> class.
    /// </summary>
    public GenericError() 
        : base(4, "GenericError", "CFG_999", "{0}", MessageSeverity.Error) { }
}