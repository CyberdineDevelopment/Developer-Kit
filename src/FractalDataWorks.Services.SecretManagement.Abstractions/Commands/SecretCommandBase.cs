using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace FractalDataWorks.Services.SecretManagement.Abstractions.Commands;

/// <summary>
/// Base class for all secret management commands providing provider-agnostic secret operations.
/// </summary>
/// <remarks>
/// SecretCommandBase represents universal secret operations that can be translated to provider-specific
/// implementations. This enables the same commands to work across Azure Key Vault, HashiCorp Vault,
/// AWS Secrets Manager, and other secret providers.
/// </remarks>
public abstract class SecretCommandBase : ISecretCommand
{
    private readonly Dictionary<string, object?> _parameters;
    private readonly Dictionary<string, object> _metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretCommandBase"/> class.
    /// </summary>
    /// <param name="commandType">The type of command (GetSecret, SetSecret, DeleteSecret, ListSecrets).</param>
    /// <param name="container">The secret container or vault name.</param>
    /// <param name="secretKey">The secret key or identifier.</param>
    /// <param name="expectedResultType">The expected result type.</param>
    /// <param name="parameters">Command parameters.</param>
    /// <param name="metadata">Additional command metadata.</param>
    /// <param name="timeout">Command timeout.</param>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    protected SecretCommandBase(
        string commandType,
        string? container,
        string? secretKey,
        Type expectedResultType,
        IReadOnlyDictionary<string, object?>? parameters = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(commandType))
            throw new ArgumentException("Command type cannot be null or empty.", nameof(commandType));

        var commandGuid = Guid.NewGuid();
        BaseCommandId = commandGuid;
        CommandId = commandGuid.ToString("D");
        CorrelationId = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
        Configuration = null; // Secret commands don't typically carry configuration
        CommandType = commandType;
        Container = container;
        SecretKey = secretKey;
        ExpectedResultType = expectedResultType ?? throw new ArgumentNullException(nameof(expectedResultType));
        Timeout = timeout;

        _parameters = parameters != null 
            ? new Dictionary<string, object?>(parameters, StringComparer.Ordinal)
            : new Dictionary<string, object?>(StringComparer.Ordinal);

        _metadata = metadata != null 
            ? new Dictionary<string, object>(metadata, StringComparer.Ordinal)
            : new Dictionary<string, object>(StringComparer.Ordinal);
    }

    /// <inheritdoc/>
    Guid ICommand.CommandId => BaseCommandId;

    /// <inheritdoc/>
    public Guid CorrelationId { get; }

    /// <inheritdoc/>
    public DateTimeOffset Timestamp { get; }

    /// <inheritdoc/>
    public IFdwConfiguration? Configuration { get; }

    /// <summary>
    /// Gets the base command identifier as a Guid.
    /// </summary>
    protected Guid BaseCommandId { get; }

    /// <inheritdoc/>
    string ISecretCommand.CommandId => CommandId;

    /// <summary>
    /// Gets the command identifier as a string.
    /// </summary>
    public string CommandId { get; }

    /// <inheritdoc/>
    public string CommandType { get; }

    /// <inheritdoc/>
    public string? Container { get; }

    /// <inheritdoc/>
    public string? SecretKey { get; }

    /// <inheritdoc/>
    public Type ExpectedResultType { get; }

    /// <inheritdoc/>
    public TimeSpan? Timeout { get; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> Parameters => _parameters;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <inheritdoc/>
    public abstract bool IsSecretModifying { get; }

    /// <inheritdoc/>
    public virtual ValidationResult Validate()
    {
        var result = new ValidationResult();

        // Validate command type
        if (string.IsNullOrWhiteSpace(CommandType))
        {
            result.Errors.Add(new ValidationFailure(nameof(CommandType), "Command type cannot be null or empty."));
        }

        // Validate secret key for operations that require it
        if (RequiresSecretKey() && string.IsNullOrWhiteSpace(SecretKey))
        {
            result.Errors.Add(new ValidationFailure(nameof(SecretKey), "Secret key is required for this operation."));
        }

        // Validate container for operations that require it
        if (RequiresContainer() && string.IsNullOrWhiteSpace(Container))
        {
            result.Errors.Add(new ValidationFailure(nameof(Container), "Container is required for this operation."));
        }

        // Validate parameters for modifying operations
        if (IsSecretModifying && !ValidateModifyingParameters())
        {
            result.Errors.Add(new ValidationFailure(nameof(Parameters), "Invalid parameters for secret modifying operation."));
        }

        return result;
    }

    /// <inheritdoc/>
    public virtual ISecretCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        if (newParameters == null)
            throw new ArgumentNullException(nameof(newParameters));

        return CreateCopyWithParameters(newParameters);
    }

    /// <inheritdoc/>
    public virtual ISecretCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        if (newMetadata == null)
            throw new ArgumentNullException(nameof(newMetadata));

        return CreateCopyWithMetadata(newMetadata);
    }

    /// <summary>
    /// Determines whether this command type requires a secret key.
    /// </summary>
    /// <returns><c>true</c> if a secret key is required; otherwise, <c>false</c>.</returns>
    protected virtual bool RequiresSecretKey()
    {
        return CommandType switch
        {
            "GetSecret" => true,
            "SetSecret" => true,
            "DeleteSecret" => true,
            "GetSecretVersions" => true,
            "ListSecrets" => false,
            _ => true
        };
    }

    /// <summary>
    /// Determines whether this command type requires a container.
    /// </summary>
    /// <returns><c>true</c> if a container is required; otherwise, <c>false</c>.</returns>
    protected virtual bool RequiresContainer()
    {
        // Most operations require a container/vault specification
        return true;
    }

    /// <summary>
    /// Validates parameters for secret modifying operations.
    /// </summary>
    /// <returns><c>true</c> if parameters are valid; otherwise, <c>false</c>.</returns>
    protected virtual bool ValidateModifyingParameters()
    {
        if (!IsSecretModifying)
            return true;

        // For SetSecret operations, ensure SecretValue parameter exists
        if (string.Equals(CommandType, "SetSecret", StringComparison.Ordinal))
        {
            return Parameters.ContainsKey("SecretValue") && Parameters["SecretValue"] != null;
        }

        return true;
    }

    /// <summary>
    /// Creates a copy of this command with new parameters.
    /// </summary>
    /// <param name="newParameters">The new parameters.</param>
    /// <returns>A new command instance with the specified parameters.</returns>
    protected abstract ISecretCommand CreateCopyWithParameters(IReadOnlyDictionary<string, object?> newParameters);

    /// <summary>
    /// Creates a copy of this command with new metadata.
    /// </summary>
    /// <param name="newMetadata">The new metadata.</param>
    /// <returns>A new command instance with the specified metadata.</returns>
    protected abstract ISecretCommand CreateCopyWithMetadata(IReadOnlyDictionary<string, object> newMetadata);
}