using FractalDataWorks;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Configuration interface for authentication services.
/// </summary>
public interface IAuthenticationConfiguration : IFdwConfiguration
{
    /// <summary>
    /// Gets the client identifier for the authentication provider.
    /// </summary>
    string ClientId { get; }

    /// <summary>
    /// Gets the authentication authority URL.
    /// </summary>
    string Authority { get; }

    /// <summary>
    /// Gets the redirect URI for authentication responses.
    /// </summary>
    string RedirectUri { get; }

    /// <summary>
    /// Gets the scopes requested during authentication.
    /// </summary>
    string[] Scopes { get; }

    /// <summary>
    /// Gets a value indicating whether token caching is enabled.
    /// </summary>
    bool EnableTokenCaching { get; }

    /// <summary>
    /// Gets the token cache lifetime in minutes.
    /// </summary>
    int TokenCacheLifetimeMinutes { get; }
}