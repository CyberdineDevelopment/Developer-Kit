using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.SecretManagement.Abstractions.Commands;

/// <summary>
/// Command for retrieving a secret value from a secret provider.
/// </summary>
/// <remarks>
/// GetSecretCommand retrieves the current or a specific version of a secret
/// from the configured secret store. The command can specify version requirements
/// and metadata filters through its parameters.
/// </remarks>
public sealed class GetSecretCommand : SecretCommandBase, ISecretCommand<SecretValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSecretCommand"/> class.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="parameters">Command parameters (e.g., Version, IncludeMetadata).</param>
    /// <param name="metadata">Additional command metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentException">Thrown when secretKey is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public GetSecretCommand(
        string? container,
        string secretKey,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("GetSecret", container, secretKey, typeof(SecretValue), parameters, metadata, timeout)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentException("Secret key cannot be null or empty for GetSecret operation.", nameof(secretKey));
    }

    /// <inheritdoc/>
    public override bool IsSecretModifying => false;

    /// <summary>
    /// Gets the version of the secret to retrieve.
    /// </summary>
    /// <value>The version identifier, or null to get the latest version.</value>
    public string? Version => Parameters.TryGetValue("Version", out var version) ? version?.ToString() : null;

    /// <summary>
    /// Gets a value indicating whether to include secret metadata in the result.
    /// </summary>
    /// <value><c>true</c> to include metadata; otherwise, <c>false</c>.</value>
    public bool IncludeMetadata => Parameters.TryGetValue("IncludeMetadata", out var include) && 
                                   include is bool includeMetadata && includeMetadata;

    /// <summary>
    /// Creates a GetSecretCommand for the latest version of a secret.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="includeMetadata">Whether to include secret metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <returns>A new GetSecretCommand instance.</returns>
    public static GetSecretCommand Latest(string? container, string secretKey, bool includeMetadata = false, TimeSpan? timeout = null)
    {
        var parameters = includeMetadata 
            ? new Dictionary<string, object?>(StringComparer.Ordinal) { ["IncludeMetadata"] = true }
            : null;

        return new GetSecretCommand(container, secretKey, parameters, null, timeout);
    }

    /// <summary>
    /// Creates a GetSecretCommand for a specific version of a secret.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="version">The version identifier.</param>
    /// <param name="includeMetadata">Whether to include secret metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <returns>A new GetSecretCommand instance.</returns>
    /// <exception cref="ArgumentException">Thrown when version is null or empty.</exception>
    public static GetSecretCommand ForVersion(string? container, string secretKey, string version, bool includeMetadata = false, TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version cannot be null or empty.", nameof(version));

        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Version"] = version
        };

        if (includeMetadata)
        {
            parameters["IncludeMetadata"] = true;
        }

        return new GetSecretCommand(container, secretKey, parameters, null, timeout);
    }

    /// <inheritdoc/>
    protected override ISecretCommand CreateCopyWithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return new GetSecretCommand(Container, SecretKey!, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    protected override ISecretCommand CreateCopyWithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return new GetSecretCommand(Container, SecretKey!, Parameters, newMetadata, Timeout);
    }

    /// <inheritdoc/>
    ISecretCommand<SecretValue> ISecretCommand<SecretValue>.WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        if (newParameters == null)
            throw new ArgumentNullException(nameof(newParameters));

        return new GetSecretCommand(Container, SecretKey!, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    ISecretCommand<SecretValue> ISecretCommand<SecretValue>.WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        if (newMetadata == null)
            throw new ArgumentNullException(nameof(newMetadata));

        return new GetSecretCommand(Container, SecretKey!, Parameters, newMetadata, Timeout);
    }
}