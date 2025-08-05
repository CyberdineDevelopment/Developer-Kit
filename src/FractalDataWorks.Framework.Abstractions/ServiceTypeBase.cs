using System;
using FractalDataWorks.EnhancedEnums;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Framework.Abstractions;

/// <summary>
/// Base class for all service types in the FractalDataWorks framework.
/// Provides common functionality for service discovery, registration, and instantiation
/// while integrating with the EnhancedEnums system for automatic collection generation.
/// </summary>
/// <typeparam name="TService">The service interface type that this service type provides.</typeparam>
/// <remarks>
/// This class does not generate a collection itself, but provides common functionality 
/// for specialized service type collections. Derived classes should use the [EnumCollection] 
/// attribute to generate typed collections.
/// </remarks>
public abstract class ServiceTypeBase<TService> : EnumOptionBase<ServiceTypeBase<TService>>, IServiceType
    where TService : class
{
    /// <summary>
    /// Gets the service interface type that this service type provides.
    /// </summary>
    /// <value>The Type of the service interface.</value>
    public Type ServiceType { get; }
    
    /// <summary>
    /// Gets the category of this service type (e.g., "Connection", "DataProvider", "Transformation", "Scheduling").
    /// Used for organizing and filtering service types in the framework.
    /// </summary>
    /// <value>A string representing the service category.</value>
    public string Category { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeBase{TService}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The display name of this service type.</param>
    /// <param name="serviceType">The service interface type that this service provides.</param>
    /// <param name="category">The category of this service type.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/>, <paramref name="serviceType">, or <paramref name="category"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> or <paramref name="category"/> is empty or whitespace.
    /// </exception>
    protected ServiceTypeBase(int id, string name, Type serviceType, string category) : base(id, name)
    {
        ArgumentNullException.ThrowIfNull(serviceType, nameof(serviceType));
        ArgumentNullException.ThrowIfNull(category, nameof(category));
        
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category cannot be empty or whitespace.", nameof(category));
        }
        
        ServiceType = serviceType;
        Category = category;
    }
    
    /// <summary>
    /// Creates a strongly-typed instance of the service using the provided service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>An instance of the service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the service cannot be created.</exception>
    public abstract TService CreateService(IServiceProvider serviceProvider);
    
    /// <summary>
    /// Registers the service and its dependencies with the service collection.
    /// This method is called during application startup to configure the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public abstract void RegisterService(IServiceCollection services);
    
    /// <summary>
    /// Creates a service instance (non-generic bridge for IServiceType interface).
    /// This method provides a non-generic interface for the framework to create service instances.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>An instance of the service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the service cannot be created.</exception>
    object IServiceType.CreateService(IServiceProvider serviceProvider) => CreateService(serviceProvider);
}