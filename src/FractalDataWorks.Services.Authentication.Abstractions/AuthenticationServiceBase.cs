using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base class for authentication services.
/// </summary>
/// <typeparam name="TAuthenticationCommand">The authentication command type.</typeparam>
/// <typeparam name="TAuthenticationConfiguration">The authentication configuration type.</typeparam>
/// <typeparam name="TAuthenticationService">The concrete authentication service type for logging category.</typeparam>
public abstract class AuthenticationServiceBase<TAuthenticationCommand, TAuthenticationConfiguration, TAuthenticationService> 
    : ServiceBase<TAuthenticationCommand, TAuthenticationConfiguration, TAuthenticationService>
    where TAuthenticationCommand : IAuthenticationCommand
    where TAuthenticationConfiguration : IAuthenticationConfiguration
    where TAuthenticationService : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationServiceBase{TAuthenticationCommand, TAuthenticationConfiguration, TAuthenticationService}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for the concrete service type.</param>
    /// <param name="configuration">The authentication configuration.</param>
    protected AuthenticationServiceBase(ILogger<TAuthenticationService> logger, TAuthenticationConfiguration configuration) 
        : base(logger, configuration) 
    { 
    }
}