using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Main interface for authentication operations in the FractalDataWorks framework.
/// Provides a unified facade for authentication operations across different provider implementations.
/// </summary>
/// <remarks>
/// This interface abstracts authentication operations using a command-based pattern,
/// allowing different authentication providers (Azure Entra, Okta, Auth0, etc.)
/// to be used interchangeably through a consistent API.
/// </remarks>
public interface IAuthenticationService : IFdwService
{
    /// <summary>
    /// Executes an authentication command.
    /// </summary>
    /// <param name="command">The authentication command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the command result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <remarks>
    /// This is the primary method for executing authentication operations. The command pattern
    /// allows for consistent handling of different operation types while maintaining
    /// flexibility for provider-specific implementations.
    /// </remarks>
    Task<IFdwResult<object?>> Execute(IAuthenticationCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a typed authentication command.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="command">The authentication command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the typed command result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <remarks>
    /// This method provides compile-time type safety for authentication operations when the
    /// expected result type is known. It eliminates the need for runtime type checking.
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
    /// Batch operations allow for efficient processing of multiple authentication operations.
    /// This is useful for scenarios like bulk user validation or multi-step authentication flows.
    /// </remarks>
    Task<IFdwResult<IAuthenticationBatchResult>> ExecuteBatch(IReadOnlyList<IAuthenticationCommand> commands, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates an authentication command before execution.
    /// </summary>
    /// <param name="command">The authentication command to validate.</param>
    /// <returns>A result indicating whether the command is valid for execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <remarks>
    /// This method performs validation of the command including parameter checking,
    /// security policy verification, and provider capability assessment.
    /// It allows pre-flight validation without executing the actual operation.
    /// </remarks>
    IFdwResult ValidateCommand(IAuthenticationCommand command);
    
    /// <summary>
    /// Gets the authentication providers available to this service.
    /// </summary>
    /// <returns>A collection of available authentication providers.</returns>
    /// <remarks>
    /// This method returns the providers that have been registered and are available
    /// for handling authentication operations. Useful for capability discovery and provider selection.
    /// </remarks>
    IReadOnlyCollection<IAuthenticationProvider> GetAvailableProviders();
    
    /// <summary>
    /// Gets an authentication provider by its identifier.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <returns>The authentication provider if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="providerId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="providerId"/> is empty or whitespace.</exception>
    /// <remarks>
    /// This method allows direct access to specific providers when fine-grained
    /// control over provider selection is required.
    /// </remarks>
    IAuthenticationProvider? GetProvider(string providerId);
    
    /// <summary>
    /// Gets an authentication provider that supports a specific authentication flow.
    /// </summary>
    /// <param name="authenticationFlow">The authentication flow type.</param>
    /// <returns>The authentication provider if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="authenticationFlow"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="authenticationFlow"/> is empty or whitespace.</exception>
    /// <remarks>
    /// This method finds a provider that supports the specified authentication flow,
    /// enabling automatic provider selection based on authentication requirements.
    /// </remarks>
    IAuthenticationProvider? GetProviderForFlow(string authenticationFlow);
    
    /// <summary>
    /// Gets authentication providers that support a specific realm or domain.
    /// </summary>
    /// <param name="realm">The realm or domain identifier.</param>
    /// <returns>A collection of authentication providers that support the realm.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="realm"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="realm"/> is empty or whitespace.</exception>
    /// <remarks>
    /// This method finds providers configured for a specific realm or domain,
    /// enabling realm-based provider routing in multi-tenant scenarios.
    /// </remarks>
    IReadOnlyCollection<IAuthenticationProvider> GetProvidersForRealm(string realm);
    
    /// <summary>
    /// Performs a health check on all registered authentication providers.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    /// <remarks>
    /// This method checks the health and availability of all registered authentication providers,
    /// providing insight into the overall health of the authentication system.
    /// </remarks>
    Task<IFdwResult<IAuthenticationServiceHealth>> HealthCheckAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets session information for active authentication sessions.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing session information.</returns>
    /// <remarks>
    /// This method provides information about currently active authentication sessions
    /// across all providers, useful for session management and monitoring.
    /// </remarks>
    Task<IFdwResult<IReadOnlyCollection<IAuthenticationSession>>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets session information for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the user's sessions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is empty or whitespace.</exception>
    /// <remarks>
    /// This method retrieves all active sessions for a specific user across all providers,
    /// useful for user session management and security monitoring.
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
    /// <remarks>
    /// This method terminates a specific authentication session, invalidating
    /// any tokens or credentials associated with that session.
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
    /// <remarks>
    /// This method terminates all authentication sessions for a user across all providers,
    /// effectively logging the user out from all devices and applications.
    /// </remarks>
    Task<IFdwResult> RevokeUserSessionsAsync(string userId, CancellationToken cancellationToken = default);
}