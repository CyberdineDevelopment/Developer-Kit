using System;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services;

/// <summary>
/// Base interface for all service types in the FractalDataWorks ecosystem.
/// Provides common functionality for service discovery, registration, and instantiation.
/// </summary>
public interface IServiceType
{
    /// <summary>
    /// Gets the service interface type that this service type provides.
    /// </summary>
    /// <value>The Type of the service interface.</value>
    Type ServiceType { get; }
    
    /// <summary>
    /// Gets the category of this service type (e.g., "Connection", "DataProvider", "Transformation", "Scheduling").
    /// Used for organizing and filtering service types in the framework.
    /// </summary>
    /// <value>A string representing the service category.</value>
    string Category { get; }
    
    /// <summary>
    /// Creates an instance of the service using the provided service provider.
    /// This method enables the framework to instantiate services dynamically based on configuration.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>An instance of the service.</returns>
    object CreateService(IServiceProvider serviceProvider);
    
    /// <summary>
    /// Registers the service and its dependencies with the service collection.
    /// This method is called during application startup to configure the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    void RegisterService(IServiceCollection services);
}