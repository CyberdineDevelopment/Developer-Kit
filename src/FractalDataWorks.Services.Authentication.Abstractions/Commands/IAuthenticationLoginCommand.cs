using System.Collections.Generic;
using FractalDataWorks;

namespace FractalDataWorks.Services.Authentication.Abstractions.Commands;

/// <summary>
/// Command interface for authentication login operations.
/// </summary>
public interface IAuthenticationLoginCommand : IAuthenticationCommand
{
    /// <summary>
    /// Gets the username for authentication.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Gets the authentication flow type to use.
    /// </summary>
    /// <remarks>
    /// Common values: "Interactive", "Silent", "ClientCredentials", "DeviceCode"
    /// </remarks>
    string FlowType { get; }

    /// <summary>
    /// Gets additional scopes to request beyond the default configuration.
    /// </summary>
    string[]? AdditionalScopes { get; }

    /// <summary>
    /// Gets extra query parameters to include in the authentication request.
    /// </summary>
    IReadOnlyDictionary<string, string>? ExtraQueryParameters { get; }

    /// <summary>
    /// Gets the login hint for the authentication provider.
    /// </summary>
    string? LoginHint { get; }

    /// <summary>
    /// Gets the prompt behavior for the authentication request.
    /// </summary>
    /// <remarks>
    /// Common values: "select_account", "login", "consent", "none"
    /// </remarks>
    string? Prompt { get; }
}