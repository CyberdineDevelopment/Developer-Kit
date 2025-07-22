using FractalDataWorks.Messages;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.DependencyInjection.Messages;

/// <summary>
/// Message for when a requested service is not registered.
/// </summary>
[EnumOption]
public class ServiceNotRegistered : DependencyInjectionMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceNotRegistered"/> class.
    /// </summary>
    public ServiceNotRegistered() 
        : base(1, "ServiceNotRegistered", "DI_001", "Service of type '{0}' is not registered", MessageSeverity.Error) { }
}