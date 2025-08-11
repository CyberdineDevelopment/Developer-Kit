using System;
using FractalDataWorks.EnhancedEnums;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.EnhancedEnums;

/// <summary>
/// Base class for all service types with factory and configuration support. When your Enhanced Enum 
/// inherits from this class, the Enhanced Enums generator automatically detects this and triggers 
/// special code generation that creates sealed collection classes with dependency injection integration 
/// and service lifecycle management.
/// </summary>
/// <typeparam name="TService">The service interface type that this service type provides.</typeparam>
/// <typeparam name="TFactory">The factory type that creates instances of TService.</typeparam>
/// <typeparam name="TConfiguration">The configuration type used by this service type.</typeparam>
/// <remarks>
/// Enhanced Enums auto-detection: Inheriting from ServiceTypeBase triggers different 
/// code generation that creates sealed collection classes with DI integration support,
/// service factory management, and strongly-typed configuration binding.
/// </remarks>
public abstract class ServiceTypeBase<TService, TFactory, TConfiguration> : EnumOptionBase<ServiceTypeBase<TService, TFactory, TConfiguration>>, IServiceType
    where TService : class
    where TFactory : class, new()
    where TConfiguration : class
{

    /// <summary>
    /// Gets the description of this service type.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the service interface type that this service type provides.
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Gets the configuration type used by this service type.
    /// </summary>
    public Type ConfigurationType { get; }

    /// <summary>
    /// Gets the category of this service type (e.g., "Connection", "Notification", "Tool").
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase{TService, TFactory, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="description">The description of this service type.</param>
    /// <param name="serviceType">The service interface type.</param>
    /// <param name="configurationType">The configuration type.</param>
    /// <param name="category">The category of this service type.</param>
    protected ServiceTypeBase(int id, string name, string description, Type serviceType, Type configurationType, string category) : base(id, name)
    {
        Description = description;
        ServiceType = serviceType;
        ConfigurationType = configurationType;
        Category = category;
    }

    /// <summary>
    /// Creates a strongly-typed instance of the service using the provided service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>An instance of the service.</returns>
    public abstract TService CreateService(IServiceProvider serviceProvider);

    /// <summary>
    /// Registers the service and its dependencies with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    public abstract void RegisterService(IServiceCollection services);

    /// <summary>
    /// Gets the factory instance for creating services of this type.
    /// </summary>
    /// <returns>An instance of the factory.</returns>
    public abstract TFactory Factory();

    /// <summary>
    /// Creates a service instance (non-generic bridge for IServiceType interface).
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>An instance of the service.</returns>
    object IServiceType.CreateService(IServiceProvider serviceProvider) => CreateService(serviceProvider);
}