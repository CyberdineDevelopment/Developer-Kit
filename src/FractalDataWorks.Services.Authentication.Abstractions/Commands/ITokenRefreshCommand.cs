namespace FractalDataWorks.Services.Authentication.Abstractions.Commands;

/// <summary>
/// Command interface for token refresh operations.
/// </summary>
public interface ITokenRefreshCommand : IAuthenticationCommand
{
    /// <summary>
    /// Gets the refresh token to use for obtaining new tokens.
    /// </summary>
    string RefreshToken { get; }

    /// <summary>
    /// Gets the account identifier for which to refresh tokens.
    /// </summary>
    string AccountId { get; }

    /// <summary>
    /// Gets the scopes to request for the new access token.
    /// </summary>
    /// <remarks>
    /// If null or empty, the original scopes from the initial authentication will be used.
    /// </remarks>
    string[]? Scopes { get; }

    /// <summary>
    /// Gets a value indicating whether to force refresh even if a valid cached token exists.
    /// </summary>
    bool ForceRefresh { get; }

    /// <summary>
    /// Gets the client assertion to use for confidential client authentication.
    /// </summary>
    string? ClientAssertion { get; }

    /// <summary>
    /// Gets the client assertion type for confidential client authentication.
    /// </summary>
    /// <remarks>
    /// Common values: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
    /// </remarks>
    string? ClientAssertionType { get; }
}