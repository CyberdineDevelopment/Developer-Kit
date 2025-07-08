using FractalDataWorks.Configuration;

namespace FractalDataWorks;

/// <summary>
/// Base interface for all services in the FractalDataWorks ecosystem.
/// Supports the namespace hierarchy principle where any parent can execute any descendant.
/// </summary>
public interface IGenericService
{
    /// <summary>
    /// Gets the service name
    /// </summary>
    string ServiceName { get; }
    
    /// <summary>
    /// Gets whether the service is in a healthy state
    /// </summary>
    bool IsHealthy { get; }
    
    /// <summary>
    /// Serves a request with any configuration type.
    /// This is the most generic verb - covers any operation.
    /// Implementations should validate the configuration type and return appropriate errors for invalid types.
    /// </summary>
    /// <typeparam name="T">The expected result type</typeparam>
    /// <param name="configuration">The configuration for the operation</param>
    /// <returns>IGenericResult of the operation</returns>
    IGenericResult<T> Serve<T>(IConfigurationBase configuration);
}

/// <summary>
/// Generic service interface with strongly-typed configuration
/// </summary>
/// <typeparam name="TConfiguration">The configuration type this service expects</typeparam>
public interface IGenericService<TConfiguration> : IGenericService
    where TConfiguration : IConfigurationBase
{
    /// <summary>
    /// Gets the service configuration
    /// </summary>
    TConfiguration Configuration { get; }
    
    /// <summary>
    /// Serves a request with the expected configuration type
    /// </summary>
    /// <typeparam name="T">The expected result type</typeparam>
    /// <param name="configuration">The strongly-typed configuration</param>
    /// <returns>IGenericResult of the operation</returns>
    IGenericResult<T> Serve<T>(TConfiguration configuration);
}