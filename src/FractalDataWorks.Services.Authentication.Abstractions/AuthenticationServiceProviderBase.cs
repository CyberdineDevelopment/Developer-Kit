using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base class for authentication service providers.
/// </summary>
/// <typeparam name="TAuthenticationServiceType">The authentication service type.</typeparam>
/// <typeparam name="TAuthenticationService">The authentication service interface.</typeparam>
/// <typeparam name="TAuthenticationConfiguration">The authentication configuration type.</typeparam>
public abstract class AuthenticationServiceProviderBase<TAuthenticationServiceType, TAuthenticationService, TAuthenticationConfiguration>
    : ServiceTypeProviderBase<TAuthenticationService, TAuthenticationServiceType, TAuthenticationConfiguration>
    where TAuthenticationServiceType : AuthenticationServiceTypeBase<TAuthenticationService, TAuthenticationConfiguration>
    where TAuthenticationService : class, IAuthenticationService
    where TAuthenticationConfiguration : class, IAuthenticationConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationServiceProviderBase{TAuthenticationServiceType, TAuthenticationService, TAuthenticationConfiguration}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurationRegistry">The configuration registry.</param>
    /// <param name="serviceTypes">The collection of available service types.</param>
    protected AuthenticationServiceProviderBase(
        ILogger<AuthenticationServiceProviderBase<TAuthenticationServiceType, TAuthenticationService, TAuthenticationConfiguration>> logger,
        IConfigurationRegistry<TAuthenticationConfiguration> configurationRegistry,
        IEnumerable<TAuthenticationServiceType> serviceTypes)
        : base(logger, configurationRegistry, serviceTypes) 
    { 
    }
}