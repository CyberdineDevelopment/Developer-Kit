using FractalDataWorks.Services;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base class for authentication service type definitions.
/// </summary>
/// <typeparam name="TAuthenticationService">The authentication service type.</typeparam>
/// <typeparam name="TAuthenticationConfiguration">The authentication configuration type.</typeparam>
public abstract class AuthenticationServiceTypeBase<TAuthenticationService, TAuthenticationConfiguration>
    : ServiceTypeBase<TAuthenticationService, TAuthenticationConfiguration>
    where TAuthenticationService : IAuthenticationService
    where TAuthenticationConfiguration : IAuthenticationConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationServiceTypeBase{TAuthenticationService, TAuthenticationConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="description">The description of this service type.</param>
    protected AuthenticationServiceTypeBase(int id, string name, string description) 
        : base(id, name, description) 
    { 
    }
}