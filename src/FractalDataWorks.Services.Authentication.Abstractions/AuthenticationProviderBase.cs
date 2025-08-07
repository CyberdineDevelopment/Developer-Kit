using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FractalDataWorks;
using FractalDataWorks.Services;

using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base class for authentication providers that executes authentication commands.
/// </summary>
/// <remarks>
/// This base class provides the foundation for all authentication providers in the framework.
/// It handles command execution, validation, and logging for authentication operations.
/// </remarks>
public abstract class AuthenticationProviderBase : ServiceBase<IAuthenticationCommand, AuthenticationConfiguration, AuthenticationProviderBase>, IAuthenticationProvider
{
    /// <summary>
    /// Gets the unique identifier for this authentication provider.
    /// </summary>
    /// <value>A unique identifier for the provider.</value>
    public string ProviderId { get; }
    
    /// <summary>
    /// Gets the display name of this authentication provider.
    /// </summary>
    /// <value>A human-readable name for the provider.</value>
    public string ProviderName => Name;
    
    /// <summary>
    /// Gets the provider type identifier.
    /// </summary>
    /// <value>The provider type (e.g., "AzureEntra", "Okta", "Auth0", "LDAP").</value>
    public string ProviderType { get; }
    
    /// <summary>
    /// Gets the version of this provider implementation.
    /// </summary>
    /// <value>The provider version string.</value>
    public string Version { get; }
    
    /// <summary>
    /// Gets the supported authentication flows for this provider.
    /// </summary>
    /// <value>A collection of authentication flow names supported by this provider.</value>
    public IReadOnlyCollection<string> SupportedAuthenticationFlows { get; }
    
    /// <summary>
    /// Gets the supported authentication command types for this provider.
    /// </summary>
    /// <value>A collection of command type names supported by this provider.</value>
    public IReadOnlyCollection<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the supported realms or domains for this provider.
    /// </summary>
    /// <value>A collection of realm identifiers supported by this provider.</value>
    public IReadOnlyCollection<string> SupportedRealms { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports multi-factor authentication.
    /// </summary>
    /// <value><c>true</c> if MFA is supported; otherwise, <c>false</c>.</value>
    public bool SupportsMfa { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports token refresh.
    /// </summary>
    /// <value><c>true</c> if token refresh is supported; otherwise, <c>false</c>.</value>
    public bool SupportsTokenRefresh { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports user information retrieval.
    /// </summary>
    /// <value><c>true</c> if user info retrieval is supported; otherwise, <c>false</c>.</value>
    public bool SupportsUserInfo { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports password operations.
    /// </summary>
    /// <value><c>true</c> if password operations are supported; otherwise, <c>false</c>.</value>
    public bool SupportsPasswordOperations { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports session management.
    /// </summary>
    /// <value><c>true</c> if session management is supported; otherwise, <c>false</c>.</value>
    public bool SupportsSessionManagement { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationProviderBase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for this authentication provider.</param>
    /// <param name="configuration">The provider configuration.</param>
    /// <param name="providerType">The provider type identifier.</param>
    /// <param name="version">The provider version.</param>
    /// <param name="supportedAuthenticationFlows">The authentication flows this provider supports.</param>
    /// <param name="supportedCommandTypes">The command types this provider can execute.</param>
    /// <param name="supportedRealms">The realms this provider can work with.</param>
    /// <param name="supportsMfa">Whether MFA is supported.</param>
    /// <param name="supportsTokenRefresh">Whether token refresh is supported.</param>
    /// <param name="supportsUserInfo">Whether user info retrieval is supported.</param>
    /// <param name="supportsPasswordOperations">Whether password operations are supported.</param>
    /// <param name="supportsSessionManagement">Whether session management is supported.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any string parameter is empty or whitespace, or when collections are empty.
    /// </exception>
    protected AuthenticationProviderBase(
        ILogger<AuthenticationProviderBase> logger,
        AuthenticationConfiguration configuration,
        string providerType,
        string version,
        IReadOnlyCollection<string> supportedAuthenticationFlows,
        IReadOnlyCollection<string> supportedCommandTypes,
        IReadOnlyCollection<string> supportedRealms,
        bool supportsMfa = false,
        bool supportsTokenRefresh = true,
        bool supportsUserInfo = true,
        bool supportsPasswordOperations = false,
        bool supportsSessionManagement = true) 
        : base(logger, configuration)
    {
        if (string.IsNullOrWhiteSpace(providerType))
        {
            throw new ArgumentException("Provider type cannot be empty or whitespace.", nameof(providerType));
        }
        
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Version cannot be empty or whitespace.", nameof(version));
        }
        
        if (supportedAuthenticationFlows.Count == 0)
        {
            throw new ArgumentException("At least one supported authentication flow must be specified.", nameof(supportedAuthenticationFlows));
        }
        
        if (supportedCommandTypes.Count == 0)
        {
            throw new ArgumentException("At least one supported command type must be specified.", nameof(supportedCommandTypes));
        }
        
        if (supportedRealms.Count == 0)
        {
            throw new ArgumentException("At least one supported realm must be specified.", nameof(supportedRealms));
        }
        
        // Validate that all flows are not null or empty
        foreach (var flow in supportedAuthenticationFlows)
        {
            if (string.IsNullOrWhiteSpace(flow))
            {
                throw new ArgumentException("Authentication flows cannot be null, empty, or whitespace.", nameof(supportedAuthenticationFlows));
            }
        }
        
        // Validate that all command types are not null or empty
        foreach (var commandType in supportedCommandTypes)
        {
            if (string.IsNullOrWhiteSpace(commandType))
            {
                throw new ArgumentException("Command types cannot be null, empty, or whitespace.", nameof(supportedCommandTypes));
            }
        }
        
        // Validate that all realms are not null or empty
        foreach (var realm in supportedRealms)
        {
            if (string.IsNullOrWhiteSpace(realm))
            {
                throw new ArgumentException("Realms cannot be null, empty, or whitespace.", nameof(supportedRealms));
            }
        }
        
        ProviderId = configuration.ProviderId;
        ProviderType = providerType;
        Version = version;
        SupportedAuthenticationFlows = supportedAuthenticationFlows;
        SupportedCommandTypes = supportedCommandTypes;
        SupportedRealms = supportedRealms;
        SupportsMfa = supportsMfa;
        SupportsTokenRefresh = supportsTokenRefresh;
        SupportsUserInfo = supportsUserInfo;
        SupportsPasswordOperations = supportsPasswordOperations;
        SupportsSessionManagement = supportsSessionManagement;
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<TOut>> Execute<TOut>(IAuthenticationCommand command, CancellationToken cancellationToken);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult> Execute(IAuthenticationCommand command, CancellationToken cancellationToken);
    
    /// <inheritdoc />
    protected abstract Task<IFdwResult<T>> ExecuteCore<T>(IAuthenticationCommand command);
    
    
    /// <inheritdoc />
    public virtual IFdwResult ValidateCommand(IAuthenticationCommand command)
    {
        
        // Check if this provider supports the command type
        bool supportsCommandType = false;
        foreach (var supportedType in SupportedCommandTypes)
        {
            if (string.Equals(supportedType, command.CommandType, StringComparison.OrdinalIgnoreCase))
            {
                supportsCommandType = true;
                break;
            }
        }
        
        if (!supportsCommandType)
        {
            AuthenticationProviderBaseLog.UnsupportedCommandType(Logger, Name, command.CommandType);
            return FdwResult.Failure($"Provider '{Name}' does not support command type '{command.CommandType}'.");
        }
        
        // Check if this provider supports the authentication flow
        bool supportsFlow = false;
        foreach (var supportedFlow in SupportedAuthenticationFlows)
        {
            if (string.Equals(supportedFlow, command.AuthenticationFlow, StringComparison.OrdinalIgnoreCase))
            {
                supportsFlow = true;
                break;
            }
        }
        
        if (!supportsFlow)
        {
            AuthenticationProviderBaseLog.UnsupportedAuthenticationFlow(Logger, Name, command.AuthenticationFlow);
            return FdwResult.Failure($"Provider '{Name}' does not support authentication flow '{command.AuthenticationFlow}'.");
        }
        
        // Validate the command itself
        var commandValidation = command.Validate();
        if (!commandValidation.IsSuccess)
        {
            return commandValidation;
        }
        
        // Perform provider-specific validation
        return ValidateProviderSpecificCommand(command);
    }
    
    /// <summary>
    /// Performs provider-specific command validation.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>A result indicating whether the command is valid for this provider.</returns>
    /// <remarks>
    /// Override this method in derived classes to implement provider-specific
    /// validation logic beyond basic command type and flow checking.
    /// </remarks>
    protected virtual IFdwResult ValidateProviderSpecificCommand(IAuthenticationCommand command)
    {
        return FdwResult.Success();
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<IAuthenticationProviderHealth>> HealthCheckAsync(CancellationToken cancellationToken = default);
    
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<IReadOnlyCollection<IAuthenticationRealm>>> GetRealmsAsync(CancellationToken cancellationToken = default);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<IAuthenticationProviderMetrics>> GetMetricsAsync(CancellationToken cancellationToken = default);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<IReadOnlyCollection<IAuthenticationSession>>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<IReadOnlyCollection<IAuthenticationSession>>> GetUserSessionsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult> RevokeSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult> RevokeUserSessionsAsync(string userId, CancellationToken cancellationToken = default);
    
}