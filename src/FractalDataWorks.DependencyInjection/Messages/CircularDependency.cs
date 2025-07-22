using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.DependencyInjection.Messages;

/// <summary>
/// Message for when circular dependencies are detected.
/// </summary>
[EnumOption]
public class CircularDependency : DependencyInjectionMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircularDependency"/> class.
    /// </summary>
    public CircularDependency() 
        : base(2, "CircularDependency", "DI_002", "Circular dependency detected: {0}", MessageSeverity.Error) { }
}