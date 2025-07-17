using FractalDataWorks.Configuration;

namespace FractalDataWorks.Services;

/// <summary>
/// Defines the contract for service providers in the Fractal framework.
/// </summary>
public interface IFdwServiceProvider
{
    /// <summary>
    /// Gets a service instance by configuration.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>The service instance.</returns>
    TService Get<TService>(IFdwConfiguration configuration)
        where TService : IFdwService;

    /// <summary>
    /// Gets a service instance by configuration ID.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <returns>The service instance.</returns>
    TService Get<TService>(int configurationId)
        where TService : IFdwService;

    /// <summary>
    /// Tries to get a service instance by configuration.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <param name="service">The service instance if found; otherwise, null.</param>
    /// <returns>True if the service was found; otherwise, false.</returns>
    bool TryGet<TService>(IFdwConfiguration configuration, out TService? service)
        where TService : IFdwService;

    /// <summary>
    /// Tries to get a service instance by configuration ID.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <param name="service">The service instance if found; otherwise, null.</param>
    /// <returns>True if the service was found; otherwise, false.</returns>
    bool TryGet<TService>(int configurationId, out TService? service)
        where TService : IFdwService;
}