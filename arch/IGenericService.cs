/// <summary>
/// Base interface for all services in the framework
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

    Result<T> Serve<T>(ConfigurationBase configuration);
}

/// <summary>
/// Generic service interface with typed configuration
/// </summary>
/// <typeparam name="TConfiguration">The configuration type</typeparam>
public interface IGenericService<TConfiguration> : IGenericService
    where TConfiguration : ConfigurationBase
{
    /// <summary>
    /// Gets the service configuration
    /// </summary>
    TConfiguration Configuration { get; }

    Result<T> Serve<T>(TConfiguration configuration);
}

public interface IGenericService<TConfiguration,TResult> :IGenericService<TConfiguration>
    where TConfiguration : ConfigurationBase
    where TResult : class
{
    /// <summary>
    /// Executes the service operation with the provided configuration
    /// </summary>
    /// <param name="configuration">The configuration to use</param>
    /// <returns>Result of the operation</returns>
    Result<T> Serve<T>(TConfiguration configuration)where T:TResult;
}