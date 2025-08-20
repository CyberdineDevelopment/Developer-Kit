using System.Collections.Generic;

namespace FractalDataWorks.Services;

/// <summary>
/// Defines the contract for service providers in the Fractal Data Works framework.
/// </summary>
public interface IFdwServiceProvider
{
    /// <summary>
    /// Gets a service instance by configuration.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>The service instance.</returns>
    IFdwResult<TService> Get<TService>(IFdwConfiguration configuration)
        where TService : IFdwService;

    /// <summary>
    /// Gets a service instance by configuration ID.
    /// </summary>
    /// <typeparam name="TService">The type of service to retrieve.</typeparam>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <returns>The service instance.</returns>
    IFdwResult<TService> Get<TService>(int configurationId)
        where TService : IFdwService;
}

/// <summary>
/// Defines the contract for typed service providers in the Fractal framework.
/// </summary>
/// <typeparam name="TService">The type of service this provider manages.</typeparam>
public interface IFdwServiceProvider<TService>
    where TService : IFdwService
{
    /// <summary>
    /// Gets a service instance by configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>The service instance.</returns>
    IFdwResult<TService> Get(IFdwConfiguration configuration);

    /// <summary>
    /// Gets a service instance by configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration.</param>
    /// <returns>The service instance.</returns>
    IFdwResult<TService> Get(int configurationId);

}

