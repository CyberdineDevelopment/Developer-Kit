using System;
using System.Collections.Generic;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Base class for secret management service type definitions.
/// Used by Enhanced Enums to register different secret management providers.
/// </summary>
/// <typeparam name="TService">The secret management service type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the secret management service.</typeparam>
/// <remarks>
/// This base class enables the service discovery pattern where the SecretManager can discover
/// and route commands to appropriate secret providers based on configuration requirements.
/// Each secret provider type (AzureKeyVault, HashiCorpVault, AWS Secrets Manager, etc.) inherits from this base to
/// provide metadata about what authentication methods it supports and how to create instances.
/// </remarks>
public abstract class SecretManagementServiceTypeBase<TService, TConfiguration>
    : ServiceTypeBase<TService, TConfiguration>
    where TService : class, IFdwService
    where TConfiguration : class, ISecretManagementConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretManagementServiceTypeBase{TService, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this secret management service type.</param>
    /// <param name="name">The name of this secret management service type.</param>
    /// <param name="description">The description of this secret management service type.</param>
    protected SecretManagementServiceTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }

    /// <summary>
    /// Gets the secret store types supported by this provider.
    /// </summary>
    /// <value>An array of secret store identifiers this provider can handle.</value>
    /// <remarks>
    /// Examples: ["AzureKeyVault", "Azure Key Vault"] for Azure Key Vault,
    /// ["HashiCorpVault", "Vault", "HashiCorp Vault"] for HashiCorp Vault.
    /// Used by the SecretManager to route commands to appropriate providers
    /// based on configuration or command metadata.
    /// </remarks>
    public abstract string[] SupportedSecretStores { get; }

    /// <summary>
    /// Gets the provider name for this secret management type.
    /// </summary>
    /// <value>The technical name of the underlying provider or client library.</value>
    /// <remarks>
    /// Examples: "Azure.Security.KeyVault.Secrets" for Azure Key Vault,
    /// "VaultSharp" for HashiCorp Vault.
    /// Used for logging, debugging, and provider identification.
    /// </remarks>
    public abstract string ProviderName { get; }

    /// <summary>
    /// Gets the authentication methods supported by this provider.
    /// </summary>
    /// <value>A list of authentication method identifiers supported by this provider.</value>
    /// <remarks>
    /// Examples: "ManagedIdentity", "ServicePrincipal", "Certificate", "Token"
    /// Used to validate configuration and guide authentication setup.
    /// </remarks>
    public abstract IReadOnlyList<string> SupportedAuthenticationMethods { get; }

    /// <summary>
    /// Gets the secret operations supported by this provider.
    /// </summary>
    /// <value>A list of operation types this provider can execute.</value>
    /// <remarks>
    /// Examples: "Get", "Set", "Delete", "List", "GetVersions", "Backup", "Restore"
    /// Used to validate commands before execution and provide capability discovery.
    /// </remarks>
    public abstract IReadOnlyList<string> SupportedOperations { get; }

    /// <summary>
    /// Gets the priority of this provider when multiple providers support the same secret store.
    /// </summary>
    /// <value>The priority value where higher numbers indicate higher priority.</value>
    /// <remarks>
    /// Used by the service discovery mechanism to select the preferred provider
    /// when multiple providers can handle the same secret store type.
    /// Standard priorities: 100 (highest), 50 (medium), 10 (low).
    /// </remarks>
    public abstract int Priority { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports encryption at rest.
    /// </summary>
    /// <value><c>true</c> if the provider supports encryption at rest; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Used for security compliance validation and provider selection
    /// based on encryption requirements.
    /// </remarks>
    public abstract bool SupportsEncryptionAtRest { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports audit logging.
    /// </summary>
    /// <value><c>true</c> if the provider supports audit logging; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Used for compliance validation and provider selection
    /// based on audit trail requirements.
    /// </remarks>
    public abstract bool SupportsAuditLogging { get; }
}