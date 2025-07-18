using FractalDataWorks.EnhancedEnums.Abstractions;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Tools.Messages;

/// <summary>
/// Message for generation completed events.
/// </summary>
[EnumOption]
public class GenerationCompleted : ToolMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationCompleted"/> class.
    /// </summary>
    public GenerationCompleted() 
        : base("TOOL_002", "Code generation completed: {0} files generated in {1}ms", MessageSeverity.Information) { }
}
