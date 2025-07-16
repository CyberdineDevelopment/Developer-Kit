using System.Collections.Generic;
using FractalDataWorks.Configuration;

namespace FractalDataWorks.Services;

/// <summary>
/// Defines the contract for service providers in the Fractal framework.
/// </summary>
public interface IFractalServiceProvider
{
    /// <summary>
    /// Gets a service instance by configuration.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>The service instance.</returns>
    TService Get<TService>(IFractalConfiguration configuration)
        where TService : IFractalService;

    /// <summary>
    /// Gets a service instance by configuration ID.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <returns>The service instance.</returns>
    TService Get<TService>(int configurationId)
        where TService : IFractalService;

    /// <summary>
    /// Tries to get a service instance by configuration.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <param name="service">The service instance if found; otherwise, null.</param>
    /// <returns>True if the service was found; otherwise, false.</returns>
    bool TryGet<TService>(IFractalConfiguration configuration, out TService? service)
        where TService : IFractalService;

    /// <summary>
    /// Tries to get a service instance by configuration ID.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <param name="service">The service instance if found; otherwise, null.</param>
    /// <returns>True if the service was found; otherwise, false.</returns>
    bool TryGet<TService>(int configurationId, out TService? service)
        where TService : IFractalService;
}

/// <summary>
/// Defines the contract for typed service providers in the Fractal framework.
/// </summary>
/// <typeparam name="TService">The type of service this provider manages.</typeparam>
public interface IFractalServiceProvider<TService>
    where TService : IFractalService
{
    /// <summary>
    /// Gets a service instance by configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>The service instance.</returns>
    TService Get(IFractalConfiguration configuration);

    /// <summary>
    /// Gets a service instance by configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <returns>The service instance.</returns>
    TService Get(int configurationId);

    /// <summary>
    /// Tries to get a service instance by configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <param name="service">The service instance if found; otherwise, null.</param>
    /// <returns>True if the service was found; otherwise, false.</returns>
    bool TryGet(IFractalConfiguration configuration, out TService? service);

    /// <summary>
    /// Tries to get a service instance by configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <param name="service">The service instance if found; otherwise, null.</param>
    /// <returns>True if the service was found; otherwise, false.</returns>
    bool TryGet(int configurationId, out TService? service);

    /// <summary>
    /// Gets all available service instances.
    /// </summary>
    /// <returns>A collection of all available service instances.</returns>
    IEnumerable<TService> GetAll();
}