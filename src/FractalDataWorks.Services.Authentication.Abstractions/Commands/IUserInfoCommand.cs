namespace FractalDataWorks.Services.Authentication.Abstractions.Commands;

/// <summary>
/// Command interface for retrieving user information.
/// </summary>
public interface IUserInfoCommand : IAuthenticationCommand
{
    /// <summary>
    /// Gets the access token to use for retrieving user information.
    /// </summary>
    string AccessToken { get; }

    /// <summary>
    /// Gets the user identifier to retrieve information for.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the specific claims to retrieve.
    /// </summary>
    /// <remarks>
    /// If null or empty, all available claims will be retrieved.
    /// Common values: "name", "email", "given_name", "family_name", "preferred_username"
    /// </remarks>
    string[]? RequestedClaims { get; }

    /// <summary>
    /// Gets a value indicating whether to retrieve extended user profile information.
    /// </summary>
    bool IncludeExtendedProfile { get; }

    /// <summary>
    /// Gets the source to retrieve user information from.
    /// </summary>
    /// <remarks>
    /// Common values: "Token", "UserInfoEndpoint", "Graph", "Cache"
    /// </remarks>
    string UserInfoSource { get; }
}