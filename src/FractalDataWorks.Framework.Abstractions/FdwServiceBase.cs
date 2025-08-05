using System;
using System.Threading.Tasks;

namespace FractalDataWorks.Framework.Abstractions;

/// <summary>
/// Base implementation for FractalDataWorks framework services.
/// Provides common functionality and default implementations for service lifecycle management.
/// </summary>
/// <remarks>
/// This class provides a foundation for implementing framework services with consistent
/// behavior across the ecosystem. Services should inherit from this class and override
/// the virtual methods to provide service-specific functionality.
/// </remarks>
public abstract class FdwServiceBase : IFdwService
{
    private bool _isDisposed;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FdwServiceBase"/> class.
    /// </summary>
    /// <param name="serviceId">The unique identifier for this service instance.</param>
    /// <param name="serviceName">The display name of the service.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="serviceId"/> or <paramref name="serviceName"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="serviceId"/> or <paramref name="serviceName"/> is empty or whitespace.
    /// </exception>
    protected FdwServiceBase(string serviceId, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(serviceId, nameof(serviceId));
        ArgumentNullException.ThrowIfNull(serviceName, nameof(serviceName));
        
        if (string.IsNullOrWhiteSpace(serviceId))
        {
            throw new ArgumentException("Service ID cannot be empty or whitespace.", nameof(serviceId));
        }
        
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name cannot be empty or whitespace.", nameof(serviceName));
        }
        
        ServiceId = serviceId;
        ServiceName = serviceName;
    }
    
    /// <inheritdoc />
    public string ServiceId { get; }
    
    /// <inheritdoc />
    public string ServiceName { get; }
    
    /// <inheritdoc />
    public virtual bool IsAvailable => !_isDisposed && IsServiceHealthy();
    
    /// <summary>
    /// Gets a value indicating whether this service instance has been disposed.
    /// </summary>
    /// <value><c>true</c> if the service has been disposed; otherwise, <c>false</c>.</value>
    protected bool IsDisposed => _isDisposed;
    
    /// <inheritdoc />
    public virtual async Task<IFdwResult> HealthCheckAsync()
    {
        if (_isDisposed)
        {
            return FdwResult.Failure("Service has been disposed.");
        }
        
        try
        {
            return await PerformHealthCheckAsync();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure($"Health check failed: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Performs the actual health check for this service.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous health check operation.
    /// The result indicates whether the service is healthy.
    /// </returns>
    /// <remarks>
    /// Override this method in derived classes to implement service-specific health checks.
    /// The default implementation returns success, indicating the service is healthy.
    /// </remarks>
    protected virtual Task<IFdwResult> PerformHealthCheckAsync()
    {
        return Task.FromResult<IFdwResult>(FdwResult.Success());
    }
    
    /// <summary>
    /// Determines whether the service is in a healthy state.
    /// </summary>
    /// <returns><c>true</c> if the service is healthy; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Override this method in derived classes to implement service-specific health checks
    /// that can be performed synchronously. This method is called by the <see cref="IsAvailable"/> property.
    /// </remarks>
    protected virtual bool IsServiceHealthy()
    {
        return true;
    }
    
    /// <summary>
    /// Releases the unmanaged resources used by the service and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources; 
    /// <c>false</c> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // Dispose managed resources
            }
            
            _isDisposed = true;
        }
    }
    
    /// <summary>
    /// Releases all resources used by the service.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Base implementation for FractalDataWorks framework services with typed configuration.
/// Extends the base service class with configuration management capabilities.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this service requires.</typeparam>
/// <remarks>
/// Use this base class for services that require specific configuration objects.
/// The configuration is validated and stored during initialization.
/// </remarks>
public abstract class FdwServiceBase<TConfiguration> : FdwServiceBase, IFdwService<TConfiguration>
    where TConfiguration : FdwConfigurationBase
{
    private TConfiguration? _configuration;
    private bool _isInitialized;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FdwServiceBase{TConfiguration}"/> class.
    /// </summary>
    /// <param name="serviceId">The unique identifier for this service instance.</param>
    /// <param name="serviceName">The display name of the service.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="serviceId"/> or <paramref name="serviceName"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="serviceId"/> or <paramref name="serviceName"/> is empty or whitespace.
    /// </exception>
    protected FdwServiceBase(string serviceId, string serviceName) : base(serviceId, serviceName)
    {
    }
    
    /// <summary>
    /// Gets the configuration object for this service.
    /// </summary>
    /// <value>The configuration object, or null if the service has not been initialized.</value>
    /// <remarks>
    /// This property is only available after <see cref="InitializeAsync"/> has been called successfully.
    /// </remarks>
    protected TConfiguration? Configuration => _configuration;
    
    /// <summary>
    /// Gets a value indicating whether the service has been successfully initialized.
    /// </summary>
    /// <value><c>true</c> if the service has been initialized; otherwise, <c>false</c>.</value>
    protected bool IsInitialized => _isInitialized;
    
    /// <inheritdoc />
    public override bool IsAvailable => base.IsAvailable && _isInitialized;
    
    /// <inheritdoc />
    public virtual async Task<IFdwResult> InitializeAsync(TConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        
        if (_isInitialized)
        {
            return FdwResult.Failure("Service has already been initialized.");
        }
        
        // Validate configuration
        var validationErrors = configuration.Validate();
        if (validationErrors.Count > 0)
        {
            return FdwResult.Failure("Configuration validation failed.", validationErrors);
        }
        
        try
        {
            var result = await PerformInitializationAsync(configuration);
            if (result.IsSuccess)
            {
                _configuration = configuration;
                _isInitialized = true;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            return FdwResult.Failure($"Service initialization failed: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Performs the actual initialization logic for this service.
    /// </summary>
    /// <param name="configuration">The validated configuration object.</param>
    /// <returns>
    /// A task representing the asynchronous initialization operation.
    /// The result indicates whether initialization was successful.
    /// </returns>
    /// <remarks>
    /// Override this method in derived classes to implement service-specific initialization logic.
    /// The configuration has already been validated when this method is called.
    /// </remarks>
    protected virtual Task<IFdwResult> PerformInitializationAsync(TConfiguration configuration)
    {
        return Task.FromResult<IFdwResult>(FdwResult.Success());
    }
}