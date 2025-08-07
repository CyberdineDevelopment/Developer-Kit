using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.EnhancedEnums.Attributes;

using Microsoft.Extensions.DependencyInjection;


namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Base class for secret providers that generates the SecretProviders collection.
/// Integrates with the EnhancedEnums system for automatic service discovery and registration.
/// </summary>
/// <remarks>
/// This base class provides the foundation for all secret providers in the framework.
/// It handles provider registration, command validation, and integration with the EnhancedEnums
/// system for automatic collection generation and lookup capabilities.
/// </remarks>
[EnumCollection(
    CollectionName = "SecretProviders", 
    IncludeReferencedAssemblies = true,
    ReturnType = typeof(ISecretProvider))]
public abstract class SecretProviderBase : ServiceTypeBase, ISecretProvider
{
    /// <summary>
    /// Gets the unique identifier for this secret provider.
    /// </summary>
    /// <value>A unique identifier for the provider.</value>
    public string ProviderId { get; }
    
    /// <summary>
    /// Gets the display name of this secret provider.
    /// </summary>
    /// <value>A human-readable name for the provider.</value>
    public string ProviderName => Name;
    
    /// <summary>
    /// Gets the provider type identifier.
    /// </summary>
    /// <value>The provider type (e.g., "AwsSecretsManager", "AzureKeyVault", "HashiCorpVault").</value>
    [EnumLookup("GetByProviderType")]
    public string ProviderType { get; }
    
    /// <summary>
    /// Gets the version of this provider implementation.
    /// </summary>
    /// <value>The provider version string.</value>
    public string Version { get; }
    
    /// <summary>
    /// Gets the supported secret command types for this provider.
    /// </summary>
    /// <value>A collection of command type names supported by this provider.</value>
    [EnumLookup("GetByCommandType", allowMultiple: true)]
    public IReadOnlyCollection<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the supported container types for this provider.
    /// </summary>
    /// <value>A collection of container type names supported by this provider.</value>
    [EnumLookup("GetByContainerType", allowMultiple: true)]
    public IReadOnlyCollection<string> SupportedContainerTypes { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports secret versioning.
    /// </summary>
    /// <value><c>true</c> if versioning is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetVersioningSupported")]
    public bool SupportsVersioning { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports secret expiration.
    /// </summary>
    /// <value><c>true</c> if expiration is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetExpirationSupported")]
    public bool SupportsExpiration { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports batch operations.
    /// </summary>
    /// <value><c>true</c> if batch operations are supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetBatchSupported")]
    public bool SupportsBatchOperations { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports binary secret data.
    /// </summary>
    /// <value><c>true</c> if binary data is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetBinarySupported")]
    public bool SupportsBinarySecrets { get; }
    
    /// <summary>
    /// Gets the configuration for this provider.
    /// </summary>
    /// <value>The provider configuration object.</value>
    public SecretConfiguration Configuration { get; }
    
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
    /// Initializes a new instance of the <see cref="SecretProviderBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this secret provider.</param>
    /// <param name="name">The display name of this secret provider.</param>
    /// <param name="providerType">The provider type identifier.</param>
    /// <param name="version">The provider version.</param>
    /// <param name="supportedCommandTypes">The command types this provider can execute.</param>
    /// <param name="supportedContainerTypes">The container types this provider can work with.</param>
    /// <param name="configuration">The provider configuration.</param>
    /// <param name="supportsVersioning">Whether versioning is supported.</param>
    /// <param name="supportsExpiration">Whether expiration is supported.</param>
    /// <param name="supportsBatchOperations">Whether batch operations are supported.</param>
    /// <param name="supportsBinarySecrets">Whether binary secrets are supported.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any string parameter is empty or whitespace, or when collections are empty.
    /// </exception>
    protected SecretProviderBase(
        int id, 
        string name, 
        string providerType,
        string version,
        IReadOnlyCollection<string> supportedCommandTypes,
        IReadOnlyCollection<string> supportedContainerTypes,
        SecretConfiguration configuration,
        bool supportsVersioning = false,
        bool supportsExpiration = false,
        bool supportsBatchOperations = false,
        bool supportsBinarySecrets = true) 
        : base(id, name, typeof(ISecretProvider), typeof(SecretConfiguration), "SecretProvider")
    {
        ArgumentNullException.ThrowIfNull(providerType, nameof(providerType));
        ArgumentNullException.ThrowIfNull(version, nameof(version));
        ArgumentNullException.ThrowIfNull(supportedCommandTypes, nameof(supportedCommandTypes));
        ArgumentNullException.ThrowIfNull(supportedContainerTypes, nameof(supportedContainerTypes));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        
        if (string.IsNullOrWhiteSpace(providerType))
        {
            throw new ArgumentException("Provider type cannot be empty or whitespace.", nameof(providerType));
        }
        
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Version cannot be empty or whitespace.", nameof(version));
        }
        
        if (supportedCommandTypes.Count == 0)
        {
            throw new ArgumentException("At least one supported command type must be specified.", nameof(supportedCommandTypes));
        }
        
        if (supportedContainerTypes.Count == 0)
        {
            throw new ArgumentException("At least one supported container type must be specified.", nameof(supportedContainerTypes));
        }
        
        // Validate that all command types are not null or empty
        foreach (var commandType in supportedCommandTypes)
        {
            if (string.IsNullOrWhiteSpace(commandType))
            {
                throw new ArgumentException("Command types cannot be null, empty, or whitespace.", nameof(supportedCommandTypes));
            }
        }
        
        // Validate that all container types are not null or empty
        foreach (var containerType in supportedContainerTypes)
        {
            if (string.IsNullOrWhiteSpace(containerType))
            {
                throw new ArgumentException("Container types cannot be null, empty, or whitespace.", nameof(supportedContainerTypes));
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
        SupportedCommandTypes = supportedCommandTypes;
        SupportedContainerTypes = supportedContainerTypes;
        Configuration = configuration;
        SupportsVersioning = supportsVersioning;
        SupportsExpiration = supportsExpiration;
        SupportsBatchOperations = supportsBatchOperations;
        SupportsBinarySecrets = supportsBinarySecrets;
        ServiceId = $"SecretProvider_{id}_{name}";
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<object?>> Execute(ISecretCommand command, CancellationToken cancellationToken = default);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<TResult>> Execute<TResult>(ISecretCommand<TResult> command, CancellationToken cancellationToken = default);
    
    /// <inheritdoc />
    public virtual async Task<IFdwResult<ISecretBatchResult>> ExecuteBatch(IReadOnlyList<ISecretCommand> commands, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commands, nameof(commands));
        
        if (commands.Count == 0)
        {
            return Logger.FailureWithLog<ISecretBatchResult>("Command list cannot be empty.");
        }
        
        if (!SupportsBatchOperations)
        {
            // Fall back to sequential execution
            return await ExecuteSequentially(commands, cancellationToken);
        }
        
        // Default implementation for providers that support batch operations
        // Override in derived classes for optimized batch processing
        return await ExecuteSequentially(commands, cancellationToken);
    }
    
    /// <summary>
    /// Executes commands sequentially when batch operations are not supported.
    /// </summary>
    /// <param name="commands">The commands to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The batch result.</returns>
    private async Task<IFdwResult<ISecretBatchResult>> ExecuteSequentially(IReadOnlyList<ISecretCommand> commands, CancellationToken cancellationToken)
    {
        var batchId = Guid.NewGuid().ToString();
        var startTime = DateTimeOffset.UtcNow;
        var commandResults = new List<ISecretCommandResult>();
        
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
            
            return FdwResult<ISecretBatchResult>.Success(batchResult);
        }
        catch (Exception ex)
        {
            var endTime = DateTimeOffset.UtcNow;
            return Logger.FailureWithLog<ISecretBatchResult>($"Batch execution failed: {ex.Message}");
        }
    }
    
    /// <inheritdoc />
    public virtual IFdwResult ValidateCommand(ISecretCommand command)
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
            return Logger.FailureWithLog($"Provider '{Name}' does not support command type '{command.CommandType}'.");
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
    /// validation logic beyond basic command type checking.
    /// </remarks>
    protected virtual IFdwResult ValidateProviderSpecificCommand(ISecretCommand command)
    {
        return FdwResult.Success();
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<ISecretProviderHealth>> HealthCheckAsync(CancellationToken cancellationToken = default);
    
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
        return Logger.FailureWithLog(healthResult.ErrorMessage ?? "Health check failed");
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<IReadOnlyCollection<ISecretContainer>>> GetContainersAsync(CancellationToken cancellationToken = default);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<ISecretProviderMetrics>> GetMetricsAsync(CancellationToken cancellationToken = default);
    
    
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
    protected abstract ISecretCommandResult CreateCommandResult(
        ISecretCommand command, int batchPosition, bool isSuccessful, object? resultData,
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
    protected abstract ISecretBatchResult CreateBatchResult(
        string batchId, int totalCommands, int successfulCommands, int failedCommands, int skippedCommands,
        TimeSpan executionTime, DateTimeOffset startedAt, DateTimeOffset completedAt,
        IReadOnlyList<ISecretCommandResult> commandResults, IReadOnlyList<string> batchErrors);
}