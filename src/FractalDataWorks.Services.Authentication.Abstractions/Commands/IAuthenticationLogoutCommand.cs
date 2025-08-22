namespace FractalDataWorks.Services.Authentication.Abstractions.Commands;

/// <summary>
/// Command interface for authentication logout operations.
/// </summary>
public interface IAuthenticationLogoutCommand : IAuthenticationCommand
{
    /// <summary>
    /// Gets the account identifier to logout.
    /// </summary>
    string AccountId { get; }

    /// <summary>
    /// Gets the logout type to perform.
    /// </summary>
    /// <remarks>
    /// Common values: "SignOut", "ClearCache", "GlobalSignOut", "FrontChannelSignOut"
    /// </remarks>
    string LogoutType { get; }

    /// <summary>
    /// Gets the post-logout redirect URI.
    /// </summary>
    string? PostLogoutRedirectUri { get; }

    /// <summary>
    /// Gets a value indicating whether to clear the token cache for this account.
    /// </summary>
    bool ClearTokenCache { get; }

    /// <summary>
    /// Gets a value indicating whether to perform a global logout across all sessions.
    /// </summary>
    bool GlobalLogout { get; }
}