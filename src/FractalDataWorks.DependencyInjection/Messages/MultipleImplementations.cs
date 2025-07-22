using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.DependencyInjection.Messages;

/// <summary>
/// Message for when multiple implementations exist for a service.
/// </summary>
[EnumOption]
public class MultipleImplementations : DependencyInjectionMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleImplementations"/> class.
    /// </summary>
    public MultipleImplementations() 
        : base(3, "MultipleImplementations", "DI_003", "Multiple implementations found for service '{0}': {1}", MessageSeverity.Warning) { }
}