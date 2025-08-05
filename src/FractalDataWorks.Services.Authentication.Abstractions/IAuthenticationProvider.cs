using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Framework.Abstractions;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface for authentication provider implementations that handle specific authentication backends.
/// Defines the contract for providers like Azure Entra, Okta, Auth0, LDAP, etc.
/// </summary>
/// <remarks>
/// Authentication providers are responsible for implementing the actual communication with
/// authentication systems. They handle provider-specific protocols, token management,
/// error handling, and result formatting.
/// </remarks>
public interface IAuthenticationProvider : IFdwService
{
    /// <summary>
    /// Gets the unique identifier for this authentication provider.
    /// </summary>
    /// <value>A unique identifier for the provider.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets the display name of this authentication provider.
    /// </summary>
    /// <value>A human-readable name for the provider.</value>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the provider type identifier.
    /// </summary>
    /// <value>The provider type (e.g., "AzureEntra", "Okta", "Auth0", "LDAP", "OAuth2").</value>
    string ProviderType { get; }
    
    /// <summary>
    /// Gets the version of this provider implementation.
    /// </summary>
    /// <value>The provider version string.</value>
    string Version { get; }
    
    /// <summary>
    /// Gets the supported authentication flows for this provider.
    /// </summary>
    /// <value>A collection of authentication flow names supported by this provider.</value>
    /// <remarks>
    /// Common authentication flows include "OAuth2", "SAML", "OpenIDConnect", 
    /// "BasicAuth", "JWT", "MFA", "Kerberos", "Certificate".
    /// </remarks>
    IReadOnlyCollection<string> SupportedAuthenticationFlows { get; }
    
    /// <summary>
    /// Gets the supported authentication command types for this provider.
    /// </summary>
    /// <value>A collection of command type names supported by this provider.</value>
    /// <remarks>
    /// Common command types include "Login", "Logout", "ValidateToken", "RefreshToken",
    /// "GetUserInfo", "ChangePassword", "ResetPassword", "EnableMFA", "DisableMFA".
    /// </remarks>
    IReadOnlyCollection<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the supported realms or domains for this provider.
    /// </summary>
    /// <value>A collection of realm identifiers supported by this provider.</value>
    /// <remarks>
    /// Realms represent different authentication domains or tenants
    /// that this provider instance can handle.
    /// </remarks>
    IReadOnlyCollection<string> SupportedRealms { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports multi-factor authentication.
    /// </summary>
    /// <value><c>true</c> if MFA is supported; otherwise, <c>false</c>.</value>
    bool SupportsMfa { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports token refresh.
    /// </summary>
    /// <value><c>true</c> if token refresh is supported; otherwise, <c>false</c>.</value>
    bool SupportsTokenRefresh { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports user information retrieval.
    /// </summary>
    /// <value><c>true</c> if user info retrieval is supported; otherwise, <c>false</c>.</value>
    bool SupportsUserInfo { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports password operations.
    /// </summary>
    /// <value><c>true</c> if password operations are supported; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Password operations include password changes, resets, and validation.
    /// External providers (like social logins) typically don't support password operations.
    /// </remarks>
    bool SupportsPasswordOperations { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports session management.
    /// </summary>
    /// <value><c>true</c> if session management is supported; otherwise, <c>false</c>.</value>
    bool SupportsSessionManagement { get; }
    
    /// <summary>
    /// Gets the configuration for this provider.
    /// </summary>
    /// <value>The provider configuration object.</value>
    AuthenticationConfiguration Configuration { get; }
    
    /// <summary>
    /// Executes an authentication command against this provider.
    /// </summary>
    /// <param name="command">The authentication command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the command result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when the command type is not supported by this provider.</exception>
    /// <remarks>
    /// This is the primary method for executing authentication operations against the provider.
    /// The provider is responsible for translating the command into provider-specific operations.
    /// </remarks>
    Task<IFdwResult<object?>> Execute(IAuthenticationCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a typed authentication command against this provider.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="command">The authentication command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the typed command result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when the command type is not supported by this provider.</exception>
    /// <remarks>
    /// This method provides compile-time type safety for authentication operations when the
    /// expected result type is known.
    /// </remarks>
    Task<IFdwResult<TResult>> Execute<TResult>(IAuthenticationCommand<TResult> command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes multiple authentication commands as a batch operation.
    /// </summary>
    /// <param name="commands">The collection of authentication commands to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous batch operation, containing the batch results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commands"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="commands"/> is empty.</exception>
    /// <remarks>
    /// Batch operations may provide performance benefits for certain providers.
    /// If batch operations are not supported natively, the provider should
    /// fall back to sequential execution.
    /// </remarks>
    Task<IFdwResult<IAuthenticationBatchResult>> ExecuteBatch(IReadOnlyList<IAuthenticationCommand> commands, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates an authentication command for this provider.
    /// </summary>
    /// <param name="command">The authentication command to validate.</param>
    /// <returns>A result indicating whether the command is valid for this provider.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <remarks>
    /// This method performs provider-specific validation including command type support,
    /// parameter validation, and capability checking. It allows pre-flight validation
    /// without executing the actual operation.
    /// </remarks>
    IFdwResult ValidateCommand(IAuthenticationCommand command);
    
    /// <summary>
    /// Tests the connection and availability of this provider.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    /// <remarks>
    /// This method performs a lightweight test of the provider's ability to connect
    /// to and communicate with the underlying authentication system.
    /// </remarks>
    Task<IFdwResult<IAuthenticationProviderHealth>> HealthCheckAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the realms or domains configured for this provider.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the available realms.</returns>
    /// <remarks>
    /// This method returns the realms, domains, or tenants that are accessible
    /// through this provider instance. Useful for discovery and configuration validation.
    /// </remarks>
    Task<IFdwResult<IReadOnlyCollection<IAuthenticationRealm>>> GetRealmsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets provider-specific metrics and statistics.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing provider metrics.</returns>
    /// <remarks>
    /// This method returns performance metrics, usage statistics, and operational
    /// information specific to this provider instance.
    /// </remarks>
    Task<IFdwResult<IAuthenticationProviderMetrics>> GetMetricsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets active authentication sessions managed by this provider.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing active sessions.</returns>
    /// <remarks>
    /// This method returns information about currently active authentication sessions
    /// that are managed by this provider instance.
    /// </remarks>
    Task<IFdwResult<IReadOnlyCollection<IAuthenticationSession>>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets active authentication sessions for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the user's sessions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is empty or whitespace.</exception>
    /// <remarks>
    /// This method retrieves all active sessions for a specific user that are
    /// managed by this provider instance.
    /// </remarks>
    Task<IFdwResult<IReadOnlyCollection<IAuthenticationSession>>> GetUserSessionsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revokes a specific authentication session.
    /// </summary>
    /// <param name="sessionId">The session identifier to revoke.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sessionId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sessionId"/> is empty or whitespace.</exception>
    /// <exception cref="NotSupportedException">Thrown when session management is not supported by this provider.</exception>
    /// <remarks>
    /// This method terminates a specific authentication session managed by this provider,
    /// invalidating any tokens or credentials associated with that session.
    /// </remarks>
    Task<IFdwResult> RevokeSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revokes all authentication sessions for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier whose sessions should be revoked.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous revocation operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is empty or whitespace.</exception>
    /// <exception cref="NotSupportedException">Thrown when session management is not supported by this provider.</exception>
    /// <remarks>
    /// This method terminates all authentication sessions for a user that are managed
    /// by this provider, effectively logging the user out from all devices and applications.
    /// </remarks>
    Task<IFdwResult> RevokeUserSessionsAsync(string userId, CancellationToken cancellationToken = default);
}