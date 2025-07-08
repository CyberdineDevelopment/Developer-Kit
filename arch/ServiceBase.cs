using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using FractalDataWorks;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for all services with automatic validation and logging
/// </summary>
/// <typeparam name="TConfiguration">The configuration type</typeparam>
public abstract class ServiceBase<TConfiguration> : IGenericService<TConfiguration>
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
{
    protected readonly ILogger _logger;
    protected readonly TConfiguration _configuration;
    
    /// <summary>
    /// Initializes a new instance of the service base
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="configuration">The configuration</param>
    protected ServiceBase(ILogger logger, TConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Automatic validation in constructor
        if (configuration == null)
        {
            _logger.LogError("Configuration is null, using invalid configuration");
            _configuration = InvalidConfiguration.Instance as TConfiguration 
                ?? throw new InvalidOperationException($"No invalid configuration defined for {typeof(TConfiguration).Name}");
        }
        else if (!configuration.IsValid)
        {
            _logger.LogWarning("Configuration is invalid for {Service}, using invalid configuration", ServiceName);
            _configuration = GetInvalidConfiguration();
        }
        else
        {
            _configuration = configuration;
        }
        
        _logger.LogInformation("{Service} initialized with {ConfigurationType}", 
            ServiceName, _configuration.GetType().Name);
    }
    
    /// <summary>
    /// Gets the service name
    /// </summary>
    public virtual string ServiceName => GetType().Name;
    
    /// <summary>
    /// Gets whether the service is in a healthy state
    /// </summary>
    public virtual bool IsHealthy => _configuration.IsValid;
    
    /// <summary>
    /// Gets the service configuration
    /// </summary>
    public TConfiguration Configuration => _configuration;
    
    /// <summary>
    /// Gets the invalid configuration instance for this service type
    /// </summary>
    /// <returns>The invalid configuration instance</returns>
    protected virtual TConfiguration GetInvalidConfiguration()
    {
        // Try to create a new instance - derived classes should override this
        return new TConfiguration();
    }
    
    /// <summary>
    /// Executes an operation with automatic logging and error handling
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <param name="operation">The operation to execute</param>
    /// <param name="operationName">The operation name for logging</param>
    /// <returns>Result of the operation</returns>
    protected Result<T> Serve<T>(
        Func<Result<T>> operation, 
        [CallerMemberName] string operationName = "")
    {
        try
        {
            _logger.LogDebug("Executing {Operation} in {Service}", operationName, ServiceName);
            var startTime = DateTime.UtcNow;
            
            var result = operation();
            
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            if (result.IsFailure)
            {
                _logger.LogWarning("Operation {Operation} failed after {Duration}ms: {Error}", 
                    operationName, duration, result.Error);
            }
            else
            {
                _logger.LogDebug("Operation {Operation} succeeded after {Duration}ms", 
                    operationName, duration);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in {Operation}", operationName);
            return Result<T>.Fail($"Unexpected error in {operationName}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Executes an async operation with automatic logging and error handling
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="operationName">The operation name for logging</param>
    /// <returns>Result of the operation</returns>
    protected async Task<Result<T>> Execute<T>(
        Func<Task<Result<T>>> operation, 
        [CallerMemberName] string operationName = "")
    {
        try
        {
            _logger.LogDebug("Executing async {Operation} in {Service}", operationName, ServiceName);
            var startTime = DateTime.UtcNow;
            
            var result = await operation();
            
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            if (result.IsFailure)
            {
                _logger.LogWarning("Async operation {Operation} failed after {Duration}ms: {Error}", 
                    operationName, duration, result.Error);
            }
            else
            {
                _logger.LogDebug("Async operation {Operation} succeeded after {Duration}ms", 
                    operationName, duration);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in async {Operation}", operationName);
            return Result<T>.Fail($"Unexpected error in {operationName}: {ex.Message}");
        }
    }
}
