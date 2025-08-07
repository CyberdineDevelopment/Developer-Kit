using FractalDataWorks.Services;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthenticationService : IFdwService<IAuthenticationCommand>
{
}