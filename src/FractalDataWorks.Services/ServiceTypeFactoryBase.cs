using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for service type factory definitions that create service instances.
/// This is a generic base with basic constraints but no Enhanced Enum attributes.
/// </summary>
/// <ExcludeFromTest>Abstract base class for service factories with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage(Justification = "Abstract base class for service factories with no business logic")]
public abstract class ServiceTypeFactoryBase<TService, TConfiguration>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeFactoryBase{TService, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="description">The description of this service type.</param>
    protected ServiceTypeFactoryBase(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
    
    /// <summary>
    /// Gets the unique identifier for this service type.
    /// </summary>
    public int Id { get; }
    
    /// <summary>
    /// Gets the name of this service type.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the description of this service type.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Creates a service instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public abstract object Create(TConfiguration configuration);
    
    /// <summary>
    /// Creates a service instance for the specified configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the service instance.</returns>
    public abstract Task<TService> GetService(string configurationName);
    
    /// <summary>
    /// Creates a service instance for the specified configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the service instance.</returns>
    public abstract Task<TService> GetService(int configurationId);
}