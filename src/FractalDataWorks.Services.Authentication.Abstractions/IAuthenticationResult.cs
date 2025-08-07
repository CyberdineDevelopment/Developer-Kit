using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface representing the result of an authentication operation.
/// Provides information about the authentication outcome, user details, and security tokens.
/// </summary>
/// <remarks>
/// Authentication results contain both success and failure information,
/// along with security artifacts like tokens and user claims when authentication succeeds.
/// </remarks>
public interface IAuthenticationResult
{
    /// <summary>
    /// Gets a value indicating whether the authentication was successful.
    /// </summary>
    /// <value><c>true</c> if authentication succeeded; otherwise, <c>false</c>.</value>
    bool IsSuccess { get; }
    
    /// <summary>
    /// Gets a value indicating whether the authentication was successful but requires additional steps.
    /// </summary>
    /// <value><c>true</c> if additional authentication steps are required; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// This is commonly used for multi-factor authentication scenarios where the first
    /// factor has been validated but additional factors are required for complete authentication.
    /// </remarks>
    bool RequiresAdditionalAuthentication { get; }
    
    /// <summary>
    /// Gets the type of additional authentication required.
    /// </summary>
    /// <value>The additional authentication type, or null if none required.</value>
    /// <remarks>
    /// Common values include "MFA", "SMS", "Email", "TOTP", "Push", or provider-specific types.
    /// This helps clients understand what additional steps are needed.
    /// </remarks>
    string? AdditionalAuthenticationType { get; }
    
    /// <summary>
    /// Gets the authenticated user principal.
    /// </summary>
    /// <value>The user principal if authentication succeeded; otherwise, null.</value>
    IUserPrincipal? User { get; }
    
    /// <summary>
    /// Gets the authentication token information.
    /// </summary>
    /// <value>The authentication token if available; otherwise, null.</value>
    IAuthenticationToken? Token { get; }
    
    /// <summary>
    /// Gets the refresh token information.
    /// </summary>
    /// <value>The refresh token if available; otherwise, null.</value>
    IAuthenticationToken? RefreshToken { get; }
    
    /// <summary>
    /// Gets the session identifier for this authentication.
    /// </summary>
    /// <value>The session identifier, or null if not applicable.</value>
    string? SessionId { get; }
    
    /// <summary>
    /// Gets when the authentication occurred.
    /// </summary>
    /// <value>The authentication timestamp.</value>
    DateTimeOffset AuthenticatedAt { get; }
    
    /// <summary>
    /// Gets when the authentication expires.
    /// </summary>
    /// <value>The expiration timestamp, or null if it doesn't expire.</value>
    DateTimeOffset? ExpiresAt { get; }
    
    /// <summary>
    /// Gets the authentication method that was used.
    /// </summary>
    /// <value>The authentication method (e.g., "Password", "OAuth2", "SAML", "Certificate").</value>
    string AuthenticationMethod { get; }
    
    /// <summary>
    /// Gets the authentication provider that processed the authentication.
    /// </summary>
    /// <value>The provider identifier.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets the realm or domain where the authentication occurred.
    /// </summary>
    /// <value>The authentication realm, or null if not applicable.</value>
    string? Realm { get; }
    
    /// <summary>
    /// Gets the granted scopes or permissions.
    /// </summary>
    /// <value>A collection of granted scope names.</value>
    /// <remarks>
    /// Scopes define what resources or operations the authenticated user
    /// is authorized to access. Common examples include API endpoints,
    /// data access levels, or functional permissions.
    /// </remarks>
    IReadOnlyCollection<string> GrantedScopes { get; }
    
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
    /// Gets the user's claims.
    /// </summary>
    /// <value>A collection of claims associated with the user.</value>
    /// <remarks>
    /// Claims provide detailed information about the user and can include
    /// both identity information (name, email) and authorization data (roles, permissions).
    /// </remarks>
    IReadOnlyCollection<Claim> Claims { get; }
    
    /// <summary>
    /// Gets any error messages from the authentication attempt.
    /// </summary>
    /// <value>A collection of error messages, or empty if successful.</value>
    IReadOnlyList<string> ErrorMessages { get; }
    
    /// <summary>
    /// Gets any warning messages from the authentication attempt.
    /// </summary>
    /// <value>A collection of warning messages, or empty if no warnings.</value>
    /// <remarks>
    /// Warnings might include messages about password expiration, account policies,
    /// or security recommendations without preventing successful authentication.
    /// </remarks>
    IReadOnlyList<string> WarningMessages { get; }
    
    /// <summary>
    /// Gets additional metadata about the authentication result.
    /// </summary>
    /// <value>A dictionary of metadata properties.</value>
    /// <remarks>
    /// Metadata can include provider-specific information, security context data,
    /// audit trail information, or other operational details about the authentication.
    /// </remarks>
    IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Gets the authentication strength or assurance level.
    /// </summary>
    /// <value>The authentication strength level.</value>
    /// <remarks>
    /// Authentication strength indicates the level of confidence in the authentication
    /// based on the methods used and security factors validated.
    /// </remarks>
    AuthenticationStrength Strength { get; }
    
    /// <summary>
    /// Gets information about multi-factor authentication if applicable.
    /// </summary>
    /// <value>MFA information, or null if MFA was not used.</value>
    IMultiFactorAuthenticationInfo? MultiFactorInfo { get; }
}