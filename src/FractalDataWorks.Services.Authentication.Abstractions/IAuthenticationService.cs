using FractalDataWorks.Services;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Non-generic marker interface for authentication services.
/// </summary>
public interface IAuthenticationService : IFdwService 
{
}

/// <summary>
/// Service interface for authentication operations.
/// </summary>
/// <typeparam name="TAuthCommand">The authentication command type.</typeparam>
public interface IAuthenticationService<TAuthCommand> : IAuthenticationService, IFdwService<TAuthCommand>
    where TAuthCommand : IAuthenticationCommand
{
}