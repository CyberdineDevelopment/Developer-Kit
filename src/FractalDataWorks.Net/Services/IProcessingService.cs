using FractalDataWorks.Configuration;

namespace FractalDataWorks.Services;

/// <summary>
/// Service interface - more specific than IGenericService.
/// Services "process" things.
/// </summary>
public interface IProcessingService : IGenericService
{
    /// <summary>
    /// Processes a request with any configuration type.
    /// More specific than Serve - indicates this service processes requests.
    /// </summary>
    /// <typeparam name="T">The expected result type</typeparam>
    /// <param name="configuration">The configuration for the operation</param>
    /// <returns>IGenericResult of the processing operation</returns>
    IGenericResult<T> Process<T>(IConfigurationBase configuration);
}

/// <summary>
/// Service interface with strongly-typed configuration
/// </summary>
/// <typeparam name="TConfiguration">The service configuration type</typeparam>
public interface IProcessingService<TConfiguration> : IProcessingService, IGenericService<TConfiguration>
    where TConfiguration : IConfigurationBase
{
    /// <summary>
    /// Processes a request with the expected configuration type
    /// </summary>
    /// <typeparam name="T">The expected result type</typeparam>
    /// <param name="configuration">The strongly-typed configuration</param>
    /// <returns>IGenericResult of the processing operation</returns>
    IGenericResult<T> Process<T>(TConfiguration configuration);
}