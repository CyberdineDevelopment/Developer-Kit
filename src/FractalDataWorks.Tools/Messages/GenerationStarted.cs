using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Tools.Messages;

/// <summary>
/// Message for generation started events.
/// </summary>
[EnumOption]
public class GenerationStarted : ToolMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationStarted"/> class.
    /// </summary>
    public GenerationStarted() 
        : base("TOOL_001", "Code generation started for {0}", MessageSeverity.Information) { }
}
