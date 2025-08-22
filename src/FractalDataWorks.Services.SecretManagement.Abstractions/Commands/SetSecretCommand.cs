using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.SecretManagement.Abstractions.Commands;

/// <summary>
/// Command for storing or updating a secret value in a secret provider.
/// </summary>
/// <remarks>
/// SetSecretCommand stores a new secret or updates an existing secret
/// in the configured secret store. The command can specify expiration dates,
/// tags, and other metadata through its parameters.
/// </remarks>
public sealed class SetSecretCommand : SecretCommandBase, ISecretCommand<SecretValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetSecretCommand"/> class.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="secretValue">The secret value to store.</param>
    /// <param name="parameters">Command parameters (e.g., ExpirationDate, Tags, Description).</param>
    /// <param name="metadata">Additional command metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentException">Thrown when secretKey is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when secretValue is null.</exception>
    public SetSecretCommand(
        string? container,
        string secretKey,
        string secretValue,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
        : base("SetSecret", container, secretKey, typeof(SecretValue), CreateParametersWithValue(secretValue, parameters), metadata, timeout)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentException("Secret key cannot be null or empty for SetSecret operation.", nameof(secretKey));
    }

    /// <inheritdoc/>
    public override bool IsSecretModifying => true;

    /// <summary>
    /// Gets the secret value to store.
    /// </summary>
    /// <value>The secret value.</value>
    public string SecretValue => Parameters["SecretValue"]?.ToString() ?? string.Empty;

    /// <summary>
    /// Gets the expiration date for the secret.
    /// </summary>
    /// <value>The expiration date, or null if no expiration is set.</value>
    public DateTimeOffset? ExpirationDate => Parameters.TryGetValue("ExpirationDate", out var expiry) && 
                                             expiry is DateTimeOffset expirationDate ? expirationDate : null;

    /// <summary>
    /// Gets the description for the secret.
    /// </summary>
    /// <value>The description, or null if no description is set.</value>
    public string? Description => Parameters.TryGetValue("Description", out var desc) ? desc?.ToString() : null;

    /// <summary>
    /// Gets the tags for the secret.
    /// </summary>
    /// <value>A dictionary of tags, or empty dictionary if no tags are set.</value>
    public IReadOnlyDictionary<string, string> Tags
    {
        get
        {
            if (Parameters.TryGetValue("Tags", out var tagsObj) && tagsObj is IReadOnlyDictionary<string, string> tags)
            {
                return tags;
            }
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    /// <summary>
    /// Creates a SetSecretCommand with basic secret value.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="secretValue">The secret value to store.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <returns>A new SetSecretCommand instance.</returns>
    public static SetSecretCommand Create(string? container, string secretKey, string secretValue, TimeSpan? timeout = null)
    {
        return new SetSecretCommand(container, secretKey, secretValue, null, null, timeout);
    }

    /// <summary>
    /// Creates a SetSecretCommand with description.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="secretValue">The secret value to store.</param>
    /// <param name="description">The description for the secret.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <returns>A new SetSecretCommand instance.</returns>
    public static SetSecretCommand WithDescription(string? container, string secretKey, string secretValue, string description, TimeSpan? timeout = null)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Description"] = description
        };

        return new SetSecretCommand(container, secretKey, secretValue, parameters, null, timeout);
    }

    /// <summary>
    /// Creates a SetSecretCommand with expiration date.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="secretValue">The secret value to store.</param>
    /// <param name="expirationDate">The expiration date for the secret.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <returns>A new SetSecretCommand instance.</returns>
    public static SetSecretCommand WithExpiration(string? container, string secretKey, string secretValue, DateTimeOffset expirationDate, TimeSpan? timeout = null)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["ExpirationDate"] = expirationDate
        };

        return new SetSecretCommand(container, secretKey, secretValue, parameters, null, timeout);
    }

    /// <summary>
    /// Creates a SetSecretCommand with tags.
    /// </summary>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="secretValue">The secret value to store.</param>
    /// <param name="tags">The tags for the secret.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <returns>A new SetSecretCommand instance.</returns>
    public static SetSecretCommand WithTags(string? container, string secretKey, string secretValue, IReadOnlyDictionary<string, string> tags, TimeSpan? timeout = null)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Tags"] = tags
        };

        return new SetSecretCommand(container, secretKey, secretValue, parameters, null, timeout);
    }

    /// <inheritdoc/>
    protected override ISecretCommand CreateCopyWithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return new SetSecretCommand(Container, SecretKey!, SecretValue, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    protected override ISecretCommand CreateCopyWithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return new SetSecretCommand(Container, SecretKey!, SecretValue, Parameters, newMetadata, Timeout);
    }

    /// <inheritdoc/>
    ISecretCommand<SecretValue> ISecretCommand<SecretValue>.WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return new SetSecretCommand(Container, SecretKey!, SecretValue, newParameters, Metadata, Timeout);
    }

    /// <inheritdoc/>
    ISecretCommand<SecretValue> ISecretCommand<SecretValue>.WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return new SetSecretCommand(Container, SecretKey!, SecretValue, Parameters, newMetadata, Timeout);
    }

    private static IReadOnlyDictionary<string, object?> CreateParametersWithValue(string secretValue, IReadOnlyDictionary<string, object?>? additionalParameters)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["SecretValue"] = secretValue
        };

        if (additionalParameters?.Count > 0)
        {
            foreach (var parameter in additionalParameters)
            {
                parameters[parameter.Key] = parameter.Value;
            }
        }

        return parameters;
    }
}