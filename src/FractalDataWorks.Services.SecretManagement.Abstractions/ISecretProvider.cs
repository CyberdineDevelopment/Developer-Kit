using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Interface for secret provider implementations that handle specific secret storage backends.
/// Defines the contract for providers like AWS Secrets Manager, Azure Key Vault, HashiCorp Vault, etc.
/// </summary>
/// <remarks>
/// Secret providers are responsible for implementing the actual communication with
/// secret storage systems. They handle provider-specific authentication, API calls,
/// error handling, and result formatting.
/// </remarks>
public interface ISecretProvider : IFdwService
{
    /// <summary>
    /// Gets the unique identifier for this secret provider.
    /// </summary>
    /// <value>A unique identifier for the provider.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets the display name of this secret provider.
    /// </summary>
    /// <value>A human-readable name for the provider.</value>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the provider type identifier.
    /// </summary>
    /// <value>The provider type (e.g., "AwsSecretsManager", "AzureKeyVault", "HashiCorpVault").</value>
    string ProviderType { get; }
    
    /// <summary>
    /// Gets the version of this provider implementation.
    /// </summary>
    /// <value>The provider version string.</value>
    string Version { get; }
    
    /// <summary>
    /// Gets the supported secret command types for this provider.
    /// </summary>
    /// <value>A collection of command type names supported by this provider.</value>
    /// <remarks>
    /// Common command types include "GetSecret", "SetSecret", "DeleteSecret", 
    /// "ListSecrets", "GetSecretMetadata", "GetSecretVersions", "CreateSecret", "UpdateSecret".
    /// </remarks>
    IReadOnlyCollection<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the supported container types for this provider.
    /// </summary>
    /// <value>A collection of container type names supported by this provider.</value>
    /// <remarks>
    /// Container types vary by provider (e.g., "Vault" for Azure Key Vault, 
    /// "SecretStore" for AWS, "Mount" for HashiCorp Vault).
    /// </remarks>
    IReadOnlyCollection<string> SupportedContainerTypes { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports secret versioning.
    /// </summary>
    /// <value><c>true</c> if versioning is supported; otherwise, <c>false</c>.</value>
    bool SupportsVersioning { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports secret expiration.
    /// </summary>
    /// <value><c>true</c> if expiration is supported; otherwise, <c>false</c>.</value>
    bool SupportsExpiration { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports batch operations.
    /// </summary>
    /// <value><c>true</c> if batch operations are supported; otherwise, <c>false</c>.</value>
    bool SupportsBatchOperations { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider supports binary secret data.
    /// </summary>
    /// <value><c>true</c> if binary data is supported; otherwise, <c>false</c>.</value>
    bool SupportsBinarySecrets { get; }
    
    /// <summary>
    /// Gets the configuration for this provider.
    /// </summary>
    /// <value>The provider configuration object.</value>
    SecretConfiguration Configuration { get; }
    
    /// <summary>
    /// Executes a secret command against this provider.
    /// </summary>
    /// <param name="command">The secret command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the command result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when the command type is not supported by this provider.</exception>
    /// <remarks>
    /// This is the primary method for executing secret operations against the provider.
    /// The provider is responsible for translating the command into provider-specific operations.
    /// </remarks>
    Task<IFdwResult<object?>> Execute(ISecretCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a typed secret command against this provider.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="command">The secret command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the typed command result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when the command type is not supported by this provider.</exception>
    /// <remarks>
    /// This method provides compile-time type safety for secret operations when the
    /// expected result type is known.
    /// </remarks>
    Task<IFdwResult<TResult>> Execute<TResult>(ISecretCommand<TResult> command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes multiple secret commands as a batch operation.
    /// </summary>
    /// <param name="commands">The collection of secret commands to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous batch operation, containing the batch results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commands"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="commands"/> is empty.</exception>
    /// <exception cref="NotSupportedException">Thrown when batch operations are not supported by this provider.</exception>
    /// <remarks>
    /// Batch operations may provide performance benefits and transactional guarantees
    /// depending on the provider implementation. If batch operations are not supported,
    /// the provider should fall back to sequential execution.
    /// </remarks>
    Task<IFdwResult<ISecretBatchResult>> ExecuteBatch(IReadOnlyList<ISecretCommand> commands, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates a secret command for this provider.
    /// </summary>
    /// <param name="command">The secret command to validate.</param>
    /// <returns>A result indicating whether the command is valid for this provider.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <remarks>
    /// This method performs provider-specific validation including command type support,
    /// parameter validation, and capability checking. It allows pre-flight validation
    /// without executing the actual operation.
    /// </remarks>
    IFdwResult ValidateCommand(ISecretCommand command);
    
    /// <summary>
    /// Tests the connection and availability of this provider.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    /// <remarks>
    /// This method performs a lightweight test of the provider's ability to connect
    /// to and communicate with the underlying secret storage system.
    /// </remarks>
    Task<IFdwResult<ISecretProviderHealth>> HealthCheckAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the containers/vaults available in this provider.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the available containers.</returns>
    /// <remarks>
    /// This method returns the containers, vaults, or namespaces that are accessible
    /// through this provider instance. Useful for discovery and configuration validation.
    /// </remarks>
    Task<IFdwResult<IReadOnlyCollection<ISecretContainer>>> GetContainersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets provider-specific metrics and statistics.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing provider metrics.</returns>
    /// <remarks>
    /// This method returns performance metrics, usage statistics, and operational
    /// information specific to this provider instance.
    /// </remarks>
    Task<IFdwResult<ISecretProviderMetrics>> GetMetricsAsync(CancellationToken cancellationToken = default);
}