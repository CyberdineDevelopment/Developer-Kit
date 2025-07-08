using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Configuration;
using FractalDataWorks.Core;
using FractalDataWorks.Models;

namespace FractalDataWorks.Services.Base;

/// <summary>
/// Base class for all services with automatic validation and logging.
/// Implements the namespace hierarchy principle where any parent can execute any descendant.
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
        
        // Automatic validation in constructor with graceful fallback
        if (configuration == null)
        {
            _logger.LogError("Configuration is null for {Service}, using invalid configuration", ServiceName);
            _configuration = CreateInvalidConfiguration();
        }
        else if (!configuration.IsValid)
        {
            _logger.LogWarning("Configuration is invalid for {Service}: {Errors}", 
                ServiceName, string.Join("; ", configuration.ValidationErrors));
            _configuration = CreateInvalidConfiguration();
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
    /// Serves a request with any configuration type (Liskov substitution principle).
    /// Validates configuration type and returns appropriate errors for invalid types.
    /// </summary>
    /// <typeparam name="T">The expected result type</typeparam>
    /// <param name="configuration">The configuration for the operation</param>
    /// <returns>IGenericResult of the operation</returns>
    public virtual IGenericResult<T> Serve<T>(ConfigurationBase configuration)
    {
        // Type safety check - supports substitutability
        if (configuration is not TConfiguration typedConfig)
        {
            var errorMessage = $"Invalid configuration type for {ServiceName}. Expected {typeof(TConfiguration).Name}, got {configuration?.GetType().Name ?? "null"}";
            _logger.LogError(errorMessage);
            return IGenericResult<T>.Fail(errorMessage);
        }
        
        // Validation check
        if (!typedConfig.IsValid)
        {
            var errorMessage = $"Configuration is invalid for {ServiceName}: {string.Join("; ", typedConfig.ValidationErrors)}";
            _logger.LogWarning(errorMessage);
            return IGenericResult<T>.Fail(errorMessage);
        }
        
        return Serve<T>(typedConfig);
    }
    
    /// <summary>
    /// Serves a request with the expected configuration type
    /// </summary>
    /// <typeparam name="T">The expected result type</typeparam>
    /// <param name="configuration">The strongly-typed configuration</param>
    /// <returns>IGenericResult of the operation</returns>
    public abstract IGenericResult<T> Serve<T>(TConfiguration configuration);
    
    /// <summary>
    /// Creates an invalid configuration instance for error handling
    /// </summary>
    /// <returns>An invalid configuration instance</returns>
    protected virtual TConfiguration CreateInvalidConfiguration()
    {
        try
        {
            var invalid = new TConfiguration();
            return invalid.CreateInvalid();
        }
        catch
        {
            // If we can't create an invalid config, create a new one
            return new TConfiguration();
        }
    }
    
    /// <summary>
    /// Executes an operation with automatic logging and error handling
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <param name="operation">The operation to execute</param>
    /// <param name="operationName">The operation name for logging</param>
    /// <returns>IGenericResult of the operation</returns>
    protected IGenericResult<T> ExecuteWithLogging<T>(
        Func<IGenericResult<T>> operation, 
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
            return IGenericResult<T>.Fail($"Unexpected error in {operationName}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Executes an async operation with automatic logging and error handling
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <param name="operation">The async operation to execute</param>
    /// <param name="operationName">The operation name for logging</param>
    /// <returns>IGenericResult of the operation</returns>
    protected async Task<IGenericResult<T>> ExecuteWithLogging<T>(
        Func<Task<IGenericResult<T>>> operation, 
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
            return IGenericResult<T>.Fail($"Unexpected error in {operationName}: {ex.Message}");
        }
    }
}