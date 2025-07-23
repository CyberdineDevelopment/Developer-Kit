using System;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for service type definitions using Enhanced Enums.
/// This class should have the Enhanced Enum attribute and constraints.
/// </summary>
[EnhancedEnumBase("ServiceTypes", 
    ReturnType = "IServiceFactory<IFdwService, IFdwConfiguration>",
    ReturnTypeNamespace = "FractalDataWorks.Services")]
public abstract class ServiceTypeBase<TService, TConfiguration> : ServiceTypeFactoryBase<TService, TConfiguration>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase{TService, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="description">The description of this service type.</param>
    protected ServiceTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }
}