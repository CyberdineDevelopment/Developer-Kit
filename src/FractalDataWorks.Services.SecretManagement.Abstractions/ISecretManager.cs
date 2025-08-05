using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Framework.Abstractions;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Main interface for secret management operations in the FractalDataWorks framework.
/// Provides a unified facade for secret operations across different provider implementations.
/// </summary>
/// <remarks>
/// This interface abstracts secret management operations using a command-based pattern,
/// allowing different secret storage providers (AWS Secrets Manager, Azure Key Vault, etc.)
/// to be used interchangeably through a consistent API.
/// </remarks>
public interface ISecretManager : IFdwService
{
    /// <summary>
    /// Executes a secret management command.
    /// </summary>
    /// <param name="command">The secret command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the command result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <remarks>
    /// This is the primary method for executing secret operations. The command pattern
    /// allows for consistent handling of different operation types while maintaining
    /// flexibility for provider-specific implementations.
    /// </remarks>
    Task<IFdwResult<object?>> Execute(ISecretCommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a typed secret management command.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="command">The secret command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the typed command result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <remarks>
    /// This method provides compile-time type safety for secret operations when the
    /// expected result type is known. It eliminates the need for runtime type checking.
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
    /// <remarks>
    /// Batch operations allow for efficient processing of multiple secret operations
    /// and may provide transactional guarantees depending on the provider implementation.
    /// If one command fails, the behavior depends on the provider's batch handling strategy.
    /// </remarks>
    Task<IFdwResult<ISecretBatchResult>> ExecuteBatch(IReadOnlyList<ISecretCommand> commands, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates a secret command before execution.
    /// </summary>
    /// <param name="command">The secret command to validate.</param>
    /// <returns>A result indicating whether the command is valid for execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <remarks>
    /// This method performs validation of the command including parameter checking,
    /// access control verification, and provider capability assessment.
    /// It allows pre-flight validation without executing the actual operation.
    /// </remarks>
    IFdwResult ValidateCommand(ISecretCommand command);
    
    /// <summary>
    /// Gets the secret providers available to this manager.
    /// </summary>
    /// <returns>A collection of available secret providers.</returns>
    /// <remarks>
    /// This method returns the providers that have been registered and are available
    /// for handling secret operations. Useful for capability discovery and provider selection.
    /// </remarks>
    IReadOnlyCollection<ISecretProvider> GetAvailableProviders();
    
    /// <summary>
    /// Gets a secret provider by its identifier.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <returns>The secret provider if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="providerId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="providerId"/> is empty or whitespace.</exception>
    /// <remarks>
    /// This method allows direct access to specific providers when fine-grained
    /// control over provider selection is required.
    /// </remarks>
    ISecretProvider? GetProvider(string providerId);
    
    /// <summary>
    /// Performs a health check on all registered secret providers.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    /// <remarks>
    /// This method checks the health and availability of all registered secret providers,
    /// providing insight into the overall health of the secret management system.
    /// </remarks>
    Task<IFdwResult<ISecretManagerHealth>> HealthCheckAsync(CancellationToken cancellationToken = default);
}