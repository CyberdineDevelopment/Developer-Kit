using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Framework.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base class for authentication providers that generates the AuthenticationProviders collection.
/// Integrates with the EnhancedEnums system for automatic service discovery and registration.
/// </summary>
/// <remarks>
/// This base class provides the foundation for all authentication providers in the framework.
/// It handles provider registration, command validation, and integration with the EnhancedEnums
/// system for automatic collection generation and lookup capabilities.
/// </remarks>
[EnumCollection(
    CollectionName = "AuthenticationProviders", 
    IncludeReferencedAssemblies = true,
    ReturnType = typeof(IAuthenticationProvider))]
public abstract class AuthenticationProviderBase : ServiceTypeBase<IAuthenticationProvider>, IAuthenticationProvider
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
    [EnumLookup("GetByProviderType")]
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
    [EnumLookup("GetByAuthenticationFlow", allowMultiple: true)]
    public IReadOnlyCollection<string> SupportedAuthenticationFlows { get; }
    
    /// <summary>
    /// Gets the supported authentication command types for this provider.
    /// </summary>
    /// <value>A collection of command type names supported by this provider.</value>
    [EnumLookup("GetByCommandType", allowMultiple: true)]
    public IReadOnlyCollection<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the supported realms or domains for this provider.
    /// </summary>
    /// <value>A collection of realm identifiers supported by this provider.</value>
    [EnumLookup("GetByRealm", allowMultiple: true)]
    public IReadOnlyCollection<string> SupportedRealms { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports multi-factor authentication.
    /// </summary>
    /// <value><c>true</c> if MFA is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetMfaSupported")]
    public bool SupportsMfa { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports token refresh.
    /// </summary>
    /// <value><c>true</c> if token refresh is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetTokenRefreshSupported")]
    public bool SupportsTokenRefresh { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports user information retrieval.
    /// </summary>
    /// <value><c>true</c> if user info retrieval is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetUserInfoSupported")]
    public bool SupportsUserInfo { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports password operations.
    /// </summary>
    /// <value><c>true</c> if password operations are supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetPasswordOperationsSupported")]
    public bool SupportsPasswordOperations { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports session management.
    /// </summary>
    /// <value><c>true</c> if session management is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetSessionManagementSupported")]
    public bool SupportsSessionManagement { get; }
    
    /// <summary>
    /// Gets the configuration for this provider.
    /// </summary>
    /// <value>The provider configuration object.</value>
    public AuthenticationConfiguration Configuration { get; }
    
    /// <summary>
    /// Gets the unique identifier for this service instance.
    /// </summary>
    /// <value>A unique identifier for the service instance.</value>
    public string ServiceId { get; }
    
    /// <summary>
    /// Gets the display name of the service.
    /// </summary>
    /// <value>A human-readable name for the service.</value>
    public string ServiceName => Name;
    
    /// <summary>
    /// Gets a value indicating whether the service is currently available for use.
    /// </summary>
    /// <value><c>true</c> if the service is available; otherwise, <c>false</c>.</value>
    public virtual bool IsAvailable => true;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationProviderBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this authentication provider.</param>
    /// <param name="name">The display name of this authentication provider.</param>
    /// <param name="providerType">The provider type identifier.</param>
    /// <param name="version">The provider version.</param>
    /// <param name="supportedAuthenticationFlows">The authentication flows this provider supports.</param>
    /// <param name="supportedCommandTypes">The command types this provider can execute.</param>
    /// <param name="supportedRealms">The realms this provider can work with.</param>
    /// <param name="configuration">The provider configuration.</param>
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
        int id, 
        string name, 
        string providerType,
        string version,
        IReadOnlyCollection<string> supportedAuthenticationFlows,
        IReadOnlyCollection<string> supportedCommandTypes,
        IReadOnlyCollection<string> supportedRealms,
        AuthenticationConfiguration configuration,
        bool supportsMfa = false,
        bool supportsTokenRefresh = true,
        bool supportsUserInfo = true,
        bool supportsPasswordOperations = false,
        bool supportsSessionManagement = true) 
        : base(id, name, typeof(IAuthenticationProvider), "AuthenticationProvider")
    {
        ArgumentNullException.ThrowIfNull(providerType, nameof(providerType));
        ArgumentNullException.ThrowIfNull(version, nameof(version));
        ArgumentNullException.ThrowIfNull(supportedAuthenticationFlows, nameof(supportedAuthenticationFlows));
        ArgumentNullException.ThrowIfNull(supportedCommandTypes, nameof(supportedCommandTypes));
        ArgumentNullException.ThrowIfNull(supportedRealms, nameof(supportedRealms));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        
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
        
        // Validate configuration
        var configErrors = configuration.Validate();
        if (configErrors.Count > 0)
        {
            throw new ArgumentException($"Configuration validation failed: {string.Join(", ", configErrors)}", nameof(configuration));
        }
        
        ProviderId = configuration.ProviderId;
        ProviderType = providerType;
        Version = version;
        SupportedAuthenticationFlows = supportedAuthenticationFlows;
        SupportedCommandTypes = supportedCommandTypes;
        SupportedRealms = supportedRealms;
        Configuration = configuration;
        SupportsMfa = supportsMfa;
        SupportsTokenRefresh = supportsTokenRefresh;
        SupportsUserInfo = supportsUserInfo;
        SupportsPasswordOperations = supportsPasswordOperations;
        SupportsSessionManagement = supportsSessionManagement;
        ServiceId = $"AuthenticationProvider_{id}_{name}";
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<object?>> Execute(IAuthenticationCommand command, CancellationToken cancellationToken = default);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<TResult>> Execute<TResult>(IAuthenticationCommand<TResult> command, CancellationToken cancellationToken = default);
    
    /// <inheritdoc />
    public virtual async Task<IFdwResult<IAuthenticationBatchResult>> ExecuteBatch(IReadOnlyList<IAuthenticationCommand> commands, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commands, nameof(commands));
        
        if (commands.Count == 0)
        {
            return FdwResult<IAuthenticationBatchResult>.Failure("Command list cannot be empty.");
        }
        
        // Default implementation executes commands sequentially
        // Override in derived classes for optimized batch processing
        return await ExecuteSequentially(commands, cancellationToken);
    }
    
    /// <summary>
    /// Executes commands sequentially for batch operations.
    /// </summary>
    /// <param name="commands">The commands to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The batch result.</returns>
    private async Task<IFdwResult<IAuthenticationBatchResult>> ExecuteSequentially(IReadOnlyList<IAuthenticationCommand> commands, CancellationToken cancellationToken)
    {
        var batchId = Guid.NewGuid().ToString();
        var startTime = DateTimeOffset.UtcNow;
        var commandResults = new List<IAuthenticationCommandResult>();
        
        int successCount = 0;
        int failCount = 0;
        
        try
        {
            for (int i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                var commandStartTime = DateTimeOffset.UtcNow;
                
                try
                {
                    var result = await Execute(command, cancellationToken);
                    var commandEndTime = DateTimeOffset.UtcNow;
                    
                    if (result.IsSuccess)
                    {
                        successCount++;
                        commandResults.Add(CreateCommandResult(command, i, true, result.Value, null, null, null, 
                            commandEndTime - commandStartTime, commandStartTime, commandEndTime));
                    }
                    else
                    {
                        failCount++;
                        commandResults.Add(CreateCommandResult(command, i, false, null, result.ErrorMessage, 
                            result.ErrorDetails, result.Exception, commandEndTime - commandStartTime, 
                            commandStartTime, commandEndTime));
                    }
                }
                catch (Exception ex)
                {
                    var commandEndTime = DateTimeOffset.UtcNow;
                    failCount++;
                    commandResults.Add(CreateCommandResult(command, i, false, null, ex.Message, 
                        new[] { ex.ToString() }, ex, commandEndTime - commandStartTime, 
                        commandStartTime, commandEndTime));
                }
            }
            
            var endTime = DateTimeOffset.UtcNow;
            var batchResult = CreateBatchResult(batchId, commands.Count, successCount, failCount, 0,
                endTime - startTime, startTime, endTime, commandResults, Array.Empty<string>());
            
            return FdwResult<IAuthenticationBatchResult>.Success(batchResult);
        }
        catch (Exception ex)
        {
            var endTime = DateTimeOffset.UtcNow;
            return FdwResult<IAuthenticationBatchResult>.Failure($"Batch execution failed: {ex.Message}", ex);
        }
    }
    
    /// <inheritdoc />
    public virtual IFdwResult ValidateCommand(IAuthenticationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
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
    
    /// <summary>
    /// Performs health check on the service (IFdwService implementation).
    /// </summary>
    /// <returns>A task representing the asynchronous health check operation.</returns>
    async Task<IFdwResult> IFdwService.HealthCheckAsync()
    {
        var healthResult = await HealthCheckAsync();
        if (healthResult.IsSuccess)
        {
            return FdwResult.Success();
        }
        return healthResult.Exception != null 
            ? FdwResult.Failure(healthResult.ErrorMessage ?? "Health check failed", healthResult.Exception)
            : FdwResult.Failure(healthResult.ErrorMessage ?? "Health check failed");
    }
    
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
    
    /// <inheritdoc />
    public override IAuthenticationProvider CreateService(IServiceProvider serviceProvider)
    {
        // Return this instance as it's already an authentication provider
        return this;
    }
    
    /// <summary>
    /// Creates a command result instance.
    /// </summary>
    /// <param name="command">The original command.</param>
    /// <param name="batchPosition">The position in the batch.</param>
    /// <param name="isSuccessful">Whether the command succeeded.</param>
    /// <param name="resultData">The result data.</param>
    /// <param name="errorMessage">The error message if failed.</param>
    /// <param name="errorDetails">Additional error details.</param>
    /// <param name="exception">The exception if one occurred.</param>
    /// <param name="executionTime">The command execution time.</param>
    /// <param name="startedAt">When the command started.</param>
    /// <param name="completedAt">When the command completed.</param>
    /// <returns>A command result instance.</returns>
    protected abstract IAuthenticationCommandResult CreateCommandResult(
        IAuthenticationCommand command, int batchPosition, bool isSuccessful, object? resultData,
        string? errorMessage, IReadOnlyList<string>? errorDetails, Exception? exception,
        TimeSpan executionTime, DateTimeOffset startedAt, DateTimeOffset completedAt);
    
    /// <summary>
    /// Creates a batch result instance.
    /// </summary>
    /// <param name="batchId">The batch identifier.</param>
    /// <param name="totalCommands">The total number of commands.</param>
    /// <param name="successfulCommands">The number of successful commands.</param>
    /// <param name="failedCommands">The number of failed commands.</param>
    /// <param name="skippedCommands">The number of skipped commands.</param>
    /// <param name="executionTime">The total execution time.</param>
    /// <param name="startedAt">When the batch started.</param>
    /// <param name="completedAt">When the batch completed.</param>
    /// <param name="commandResults">The individual command results.</param>
    /// <param name="batchErrors">Any batch-level errors.</param>
    /// <returns>A batch result instance.</returns>
    protected abstract IAuthenticationBatchResult CreateBatchResult(
        string batchId, int totalCommands, int successfulCommands, int failedCommands, int skippedCommands,
        TimeSpan executionTime, DateTimeOffset startedAt, DateTimeOffset completedAt,
        IReadOnlyList<IAuthenticationCommandResult> commandResults, IReadOnlyList<string> batchErrors);
}