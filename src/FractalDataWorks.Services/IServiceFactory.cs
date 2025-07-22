namespace FractalDataWorks.Services;

/// <summary>
/// Generic factory interface for creating Service instances
/// </summary>
public interface IServiceFactory
{
    /// <summary>
    /// Creates a service instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to create.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService;

    /// <summary>
    /// Creates a service instance.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    IFdwResult<IFdwService> Create(IFdwConfiguration configuration);
}

/// <summary>
/// Generic factory interface for creating Service instances of a specific type.
/// </summary>
/// <typeparam name="TService">The type of service this factory creates.</typeparam>
public interface IServiceFactory<TService> : IServiceFactory
    where TService : IFdwService
{
    /// <summary>
    /// Creates a service instance.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    new IFdwResult<TService> Create(IFdwConfiguration configuration);
}

/// <summary>
/// Generic factory interface for creating Service instances with specific configuration type.
/// </summary>
/// <typeparam name="TService">The type of service this factory creates.</typeparam>
/// <typeparam name="TConfiguration">The type of configuration the service uses.</typeparam>
public interface IServiceFactory<TService, TConfiguration> : IServiceFactory<TService>
    where TService : IFdwService
    where TConfiguration : IFdwConfiguration
{
    /// <summary>
    /// Creates a service instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    IFdwResult<TService> Create(TConfiguration configuration);
}