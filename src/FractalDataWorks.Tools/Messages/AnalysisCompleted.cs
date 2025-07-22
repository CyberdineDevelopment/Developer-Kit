using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Tools.Messages;

/// <summary>
/// Message for analysis completed events.
/// </summary>
[EnumOption]
public class AnalysisCompleted : ToolMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisCompleted"/> class.
    /// </summary>
    public AnalysisCompleted() 
        : base(3, "AnalysisCompleted", "TOOL_003", "Analysis completed: {0} issues found", MessageSeverity.Information) { }
}
