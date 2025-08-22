using System;
using System.Collections.Generic;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.SecretManagement.Abstractions.Commands;

/// <summary>
/// Command for deleting a secret from a secret provider.
/// </summary>
/// <remarks>
/// DeleteSecretCommand removes a secret from the configured secret store.
/// The command can specify whether to perform a soft delete (if supported)
/// or a permanent deletion through its parameters.
/// </remarks>
public sealed class DeleteSecretCommand : SecretCommandBase, ISecretCommand<IFdwResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteSecretCommand"/> class.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="parameters">Command parameters (e.g., PermanentDelete, RecoveryWindow).</param>
    /// <param name="metadata">Additional command metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentException">Thrown when secretKey is null or empty.</exception>
    public DeleteSecretCommand(
        string? container,
        string secretKey,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("DeleteSecret", container, secretKey, typeof(IFdwResult), parameters, metadata, timeout)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentException("Secret key cannot be null or empty for DeleteSecret operation.", nameof(secretKey));
    }

    /// <inheritdoc/>
    public override bool IsSecretModifying => true;

    /// <summary>
    /// Gets a value indicating whether to perform a permanent deletion.
    /// </summary>
    /// <value><c>true</c> for permanent deletion; <c>false</c> for soft deletion (if supported).</value>
    /// <remarks>
    /// Soft deletion allows secret recovery within a specified time window.
    /// Permanent deletion immediately removes the secret with no recovery option.
    /// </remarks>
    public bool PermanentDelete => Parameters.TryGetValue("PermanentDelete", out var permanent) && 
                                   permanent is bool isPermanent && isPermanent;

    /// <summary>
    /// Gets the recovery window for soft-deleted secrets.
    /// </summary>
    /// <value>The time period during which a soft-deleted secret can be recovered, or null for default.</value>
    public TimeSpan? RecoveryWindow => Parameters.TryGetValue("RecoveryWindow", out var window) && 
                                       window is TimeSpan recoveryWindow ? recoveryWindow : null;

    /// <summary>
    /// Creates a DeleteSecretCommand for soft deletion.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <returns>A new DeleteSecretCommand instance for soft deletion.</returns>
    public static DeleteSecretCommand SoftDelete(string? container, string secretKey, TimeSpan? timeout = null)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["PermanentDelete"] = false
        };

        return new DeleteSecretCommand(container, secretKey, parameters, null, timeout);
    }

    /// <summary>
    /// Creates a DeleteSecretCommand for permanent deletion.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <returns>A new DeleteSecretCommand instance for permanent deletion.</returns>
    public static DeleteSecretCommand PermanentlyDelete(string? container, string secretKey, TimeSpan? timeout = null)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["PermanentDelete"] = true
        };

        return new DeleteSecretCommand(container, secretKey, parameters, null, timeout);
    }

    /// <summary>
    /// Creates a DeleteSecretCommand for soft deletion with custom recovery window.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="recoveryWindow">The time period during which the secret can be recovered.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <returns>A new DeleteSecretCommand instance for soft deletion with specified recovery window.</returns>
    public static DeleteSecretCommand SoftDeleteWithRecovery(string? container, string secretKey, TimeSpan recoveryWindow, TimeSpan? timeout = null)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["PermanentDelete"] = false,
            ["RecoveryWindow"] = recoveryWindow
        };

        return new DeleteSecretCommand(container, secretKey, parameters, null, timeout);
    }

    /// <inheritdoc/>
    protected override ISecretCommand CreateCopyWithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return new DeleteSecretCommand(Container, SecretKey!, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    protected override ISecretCommand CreateCopyWithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return new DeleteSecretCommand(Container, SecretKey!, Parameters, newMetadata, Timeout);
    }

    /// <inheritdoc/>
    ISecretCommand<IFdwResult> ISecretCommand<IFdwResult>.WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return new DeleteSecretCommand(Container, SecretKey!, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    ISecretCommand<IFdwResult> ISecretCommand<IFdwResult>.WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return new DeleteSecretCommand(Container, SecretKey!, Parameters, newMetadata, Timeout);
    }
}