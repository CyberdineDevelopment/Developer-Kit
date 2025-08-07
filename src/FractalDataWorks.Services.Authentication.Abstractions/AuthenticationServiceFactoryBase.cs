using FractalDataWorks.Services;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base class for authentication service factories.
/// </summary>
/// <typeparam name="TAuthenticationService">The authentication service type.</typeparam>
/// <typeparam name="TAuthenticationConfiguration">The authentication configuration type.</typeparam>
public abstract class AuthenticationServiceFactoryBase<TAuthenticationService, TAuthenticationConfiguration>
    : ServiceFactoryBase, IServiceFactory<TAuthenticationService, TAuthenticationConfiguration>
    where TAuthenticationService : IAuthenticationService
    where TAuthenticationConfiguration : IAuthenticationConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationServiceFactoryBase{TAuthenticationService, TAuthenticationConfiguration}"/> class.
    /// </summary>
    protected AuthenticationServiceFactoryBase() 
        : base() 
    { 
    }

    /// <summary>
    /// Creates an authentication service instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public abstract IFdwResult<TAuthenticationService> Create(TAuthenticationConfiguration configuration);
    
    /// <summary>
    /// Creates an authentication service instance for the specified configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public abstract Task<TAuthenticationService> GetService(string configurationName);
    
    /// <summary>
    /// Creates an authentication service instance for the specified configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public abstract Task<TAuthenticationService> GetService(int configurationId);

    /// <inheritdoc/>
    public new IFdwResult<TAuthenticationService> Create(IFdwConfiguration configuration)
    {
        if (configuration is TAuthenticationConfiguration authConfig)
        {
            return Create(authConfig);
        }
        return FdwResult<TAuthenticationService>.Failure("Invalid configuration type.");
    }
}