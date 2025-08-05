using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface representing an authenticated user principal.
/// Provides access to user identity information, roles, and claims.
/// </summary>
/// <remarks>
/// User principals encapsulate authenticated user information and provide
/// a consistent interface for accessing user details across different
/// authentication providers and systems.
/// </remarks>
public interface IUserPrincipal
{
    /// <summary>
    /// Gets the unique identifier for this user.
    /// </summary>
    /// <value>The user's unique identifier.</value>
    /// <remarks>
    /// This identifier should be unique within the authentication provider's
    /// domain and remain stable across authentication sessions.
    /// </remarks>
    string UserId { get; }
    
    /// <summary>
    /// Gets the user's username or login name.
    /// </summary>
    /// <value>The username, or null if not available.</value>
    /// <remarks>
    /// The username is typically the identifier used by the user to log in
    /// and may be different from the display name or email address.
    /// </remarks>
    string? Username { get; }
    
    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    /// <value>The email address, or null if not available.</value>
    string? Email { get; }
    
    /// <summary>
    /// Gets a value indicating whether the user's email address has been verified.
    /// </summary>
    /// <value><c>true</c> if the email is verified; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Email verification status is important for security decisions and
    /// may affect what operations the user is allowed to perform.
    /// </remarks>
    bool IsEmailVerified { get; }
    
    /// <summary>
    /// Gets the user's phone number.
    /// </summary>
    /// <value>The phone number, or null if not available.</value>
    string? PhoneNumber { get; }
    
    /// <summary>
    /// Gets a value indicating whether the user's phone number has been verified.
    /// </summary>
    /// <value><c>true</c> if the phone number is verified; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Phone number verification status is used for multi-factor authentication
    /// and account recovery scenarios.
    /// </remarks>
    bool IsPhoneNumberVerified { get; }
    
    /// <summary>
    /// Gets the user's display name or full name.
    /// </summary>
    /// <value>The display name, or null if not available.</value>
    /// <remarks>
    /// The display name is typically used in user interfaces and may be
    /// different from the username used for authentication.
    /// </remarks>
    string? DisplayName { get; }
    
    /// <summary>
    /// Gets the user's first name or given name.
    /// </summary>
    /// <value>The first name, or null if not available.</value>
    string? FirstName { get; }
    
    /// <summary>
    /// Gets the user's last name or family name.
    /// </summary>
    /// <value>The last name, or null if not available.</value>
    string? LastName { get; }
    
    /// <summary>
    /// Gets the URL of the user's profile picture or avatar.
    /// </summary>
    /// <value>The profile picture URL, or null if not available.</value>
    string? ProfilePictureUrl { get; }
    
    /// <summary>
    /// Gets the user's locale or language preference.
    /// </summary>
    /// <value>The locale identifier, or null if not available.</value>
    /// <remarks>
    /// The locale follows standard format (e.g., "en-US", "fr-FR") and can be
    /// used for localization and internationalization purposes.
    /// </remarks>
    string? Locale { get; }
    
    /// <summary>
    /// Gets the user's time zone.
    /// </summary>
    /// <value>The time zone identifier, or null if not available.</value>
    /// <remarks>
    /// Time zone information helps with displaying dates and times
    /// appropriately for the user's location.
    /// </remarks>
    string? TimeZone { get; }
    
    /// <summary>
    /// Gets when the user account was created.
    /// </summary>
    /// <value>The account creation timestamp, or null if not available.</value>
    DateTimeOffset? CreatedAt { get; }
    
    /// <summary>
    /// Gets when the user profile was last updated.
    /// </summary>
    /// <value>The last update timestamp, or null if not available.</value>
    DateTimeOffset? UpdatedAt { get; }
    
    /// <summary>
    /// Gets when the user last logged in.
    /// </summary>
    /// <value>The last login timestamp, or null if not available.</value>
    DateTimeOffset? LastLoginAt { get; }
    
    /// <summary>
    /// Gets the number of times the user has logged in.
    /// </summary>
    /// <value>The login count, or null if not available.</value>
    int? LoginCount { get; }
    
    /// <summary>
    /// Gets a value indicating whether the user account is active.
    /// </summary>
    /// <value><c>true</c> if the account is active; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Inactive accounts may be temporarily disabled, suspended, or
    /// pending activation. Authentication should fail for inactive accounts.
    /// </remarks>
    bool IsActive { get; }
    
    /// <summary>
    /// Gets a value indicating whether the user account is locked.
    /// </summary>
    /// <value><c>true</c> if the account is locked; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Locked accounts are typically the result of security policies
    /// (e.g., too many failed login attempts) and require administrative action to unlock.
    /// </remarks>
    bool IsLocked { get; }
    
    /// <summary>
    /// Gets when the user account lock expires.
    /// </summary>
    /// <value>The lock expiration timestamp, or null if not locked or permanent.</value>
    DateTimeOffset? LockExpiresAt { get; }
    
    /// <summary>
    /// Gets a value indicating whether multi-factor authentication is enabled for this user.
    /// </summary>
    /// <value><c>true</c> if MFA is enabled; otherwise, <c>false</c>.</value>
    bool IsMfaEnabled { get; }
    
    /// <summary>
    /// Gets the available multi-factor authentication methods for this user.
    /// </summary>
    /// <value>A collection of available MFA method names.</value>
    /// <remarks>
    /// Common MFA methods include "SMS", "Email", "TOTP", "Push", "Biometric", "Hardware".
    /// </remarks>
    IReadOnlyCollection<string> AvailableMfaMethods { get; }
    
    /// <summary>
    /// Gets the user's roles.
    /// </summary>
    /// <value>A collection of role names assigned to the user.</value>
    /// <remarks>
    /// Roles provide a way to group permissions and are commonly used
    /// for authorization decisions throughout the application.
    /// </remarks>
    IReadOnlyCollection<string> Roles { get; }
    
    /// <summary>
    /// Gets the user's permissions.
    /// </summary>
    /// <value>A collection of permission names granted to the user.</value>
    /// <remarks>
    /// Permissions define specific actions or resources the user can access.
    /// These may be assigned directly or inherited through roles.
    /// </remarks>
    IReadOnlyCollection<string> Permissions { get; }
    
    /// <summary>
    /// Gets the user's claims.
    /// </summary>
    /// <value>A collection of claims associated with the user.</value>
    /// <remarks>
    /// Claims provide detailed information about the user and can include
    /// both identity information and authorization data.
    /// </remarks>
    IReadOnlyCollection<Claim> Claims { get; }
    
    /// <summary>
    /// Gets additional user profile attributes.
    /// </summary>
    /// <value>A dictionary of custom attribute key-value pairs.</value>
    /// <remarks>
    /// Custom attributes allow storing application-specific user information
    /// that doesn't fit into the standard profile properties.
    /// </remarks>
    IReadOnlyDictionary<string, object> CustomAttributes { get; }
    
    /// <summary>
    /// Gets the authentication provider that verified this user.
    /// </summary>
    /// <value>The provider identifier.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets the realm or domain where this user was authenticated.
    /// </summary>
    /// <value>The authentication realm, or null if not applicable.</value>
    string? Realm { get; }
    
    /// <summary>
    /// Gets the external identity provider used for authentication.
    /// </summary>
    /// <value>The external provider identifier, or null if not applicable.</value>
    /// <remarks>
    /// This is relevant for federated authentication scenarios where
    /// authentication is delegated to an external identity provider.
    /// </remarks>
    string? ExternalProvider { get; }
    
    /// <summary>
    /// Gets the user's identifier in the external identity provider.
    /// </summary>
    /// <value>The external user identifier, or null if not applicable.</value>
    /// <remarks>
    /// This identifier is used to link the local user account with
    /// the external identity provider account.
    /// </remarks>
    string? ExternalUserId { get; }
    
    /// <summary>
    /// Determines whether the user has a specific role.
    /// </summary>
    /// <param name="role">The role name to check.</param>
    /// <returns><c>true</c> if the user has the role; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="role"/> is empty or whitespace.</exception>
    bool HasRole(string role);
    
    /// <summary>
    /// Determines whether the user has any of the specified roles.
    /// </summary>
    /// <param name="roles">The role names to check.</param>
    /// <returns><c>true</c> if the user has any of the roles; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="roles"/> is null.</exception>
    bool HasAnyRole(IEnumerable<string> roles);
    
    /// <summary>
    /// Determines whether the user has all of the specified roles.
    /// </summary>
    /// <param name="roles">The role names to check.</param>
    /// <returns><c>true</c> if the user has all of the roles; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="roles"/> is null.</exception>
    bool HasAllRoles(IEnumerable<string> roles);
    
    /// <summary>
    /// Determines whether the user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission name to check.</param>
    /// <returns><c>true</c> if the user has the permission; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="permission"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="permission"/> is empty or whitespace.</exception>
    bool HasPermission(string permission);
    
    /// <summary>
    /// Determines whether the user has any of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permission names to check.</param>
    /// <returns><c>true</c> if the user has any of the permissions; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="permissions"/> is null.</exception>
    bool HasAnyPermission(IEnumerable<string> permissions);
    
    /// <summary>
    /// Determines whether the user has all of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permission names to check.</param>
    /// <returns><c>true</c> if the user has all of the permissions; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="permissions"/> is null.</exception>
    bool HasAllPermissions(IEnumerable<string> permissions);
    
    /// <summary>
    /// Gets the value of a specific claim.
    /// </summary>
    /// <param name="claimType">The claim type to retrieve.</param>
    /// <returns>The claim value, or null if the claim is not present.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="claimType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="claimType"/> is empty or whitespace.</exception>
    string? GetClaimValue(string claimType);
    
    /// <summary>
    /// Gets all values for a specific claim type.
    /// </summary>
    /// <param name="claimType">The claim type to retrieve values for.</param>
    /// <returns>A collection of claim values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="claimType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="claimType"/> is empty or whitespace.</exception>
    IReadOnlyCollection<string> GetClaimValues(string claimType);
}