using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FractalDataWorks;

namespace FractalDataWorks.Services;

/// <summary>
/// Base factory for creating services with validation
/// </summary>
/// <typeparam name="TService">The service interface type</typeparam>
/// <typeparam name="TConfiguration">The configuration type</typeparam>
public abstract class ServiceFactoryBase<TService, TConfiguration>
    where TService : IGenericService
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ILogger _logger;
    
    protected ServiceFactoryBase(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Creates a service instance with the given configuration
    /// </summary>
    /// <param name="configuration">The configuration to use</param>
    /// <returns>Result containing the service or an error</returns>
    public Result<TService> CreateService(TConfiguration configuration)
    {
        try
        {
            // Validate configuration
            var validationResult = configuration?.Validate() ?? Result<TConfiguration>.Fail("Configuration is null");
            
            if (validationResult.IsFailure)
            {
                _logger.LogWarning("Failed to create service due to invalid configuration: {Error}", 
                    validationResult.Error);
                return Result<TService>.Fail($"Invalid configuration: {validationResult.Error}");
            }
            
            // Create the service
            var service = CreateServiceCore(validationResult.Value);
            
            if (service == null)
            {
                return Result<TService>.Fail("Service creation returned null");
            }
            
            _logger.LogInformation("Successfully created {ServiceType}", typeof(TService).Name);
            return Result<TService>.Ok(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create {ServiceType}", typeof(TService).Name);
            return Result<TService>.Fail($"Failed to create service: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Core method to create the service instance
    /// </summary>
    /// <param name="configuration">The validated configuration</param>
    /// <returns>The service instance</returns>
    protected abstract TService CreateServiceCore(TConfiguration configuration);
}

/// <summary>
/// Generic service provider for dependency injection
/// </summary>
public class ServiceProvider<TService> where TService : IGenericService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceProvider<TService>> _logger;
    
    public ServiceProvider(IServiceProvider serviceProvider, ILogger<ServiceProvider<TService>> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Gets a service instance
    /// </summary>
    /// <returns>Result containing the service or an error</returns>
    public Result<TService> GetService()
    {
        try
        {
            var service = _serviceProvider.GetService<TService>();
            
            if (service == null)
            {
                var error = $"Service {typeof(TService).Name} not registered";
                _logger.LogError(error);
                return Result<TService>.Fail(error);
            }
            
            if (!service.IsHealthy)
            {
                _logger.LogWarning("Service {ServiceName} is not healthy", service.ServiceName);
            }
            
            return Result<TService>.Ok(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service {ServiceType}", typeof(TService).Name);
            return Result<TService>.Fail($"Failed to get service: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets all registered services of this type
    /// </summary>
    /// <returns>Result containing the services or an error</returns>
    public Result<IEnumerable<TService>> GetServices()
    {
        try
        {
            var services = _serviceProvider.GetServices<TService>().ToList();
            
            if (!services.Any())
            {
                var error = $"No services of type {typeof(TService).Name} registered";
                _logger.LogWarning(error);
                return Result<IEnumerable<TService>>.Fail(error);
            }
            
            var unhealthyCount = services.Count(s => !s.IsHealthy);
            if (unhealthyCount > 0)
            {
                _logger.LogWarning("{Count} of {Total} {ServiceType} services are not healthy", 
                    unhealthyCount, services.Count, typeof(TService).Name);
            }
            
            return Result<IEnumerable<TService>>.Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get services {ServiceType}", typeof(TService).Name);
            return Result<IEnumerable<TService>>.Fail($"Failed to get services: {ex.Message}");
        }
    }
}
