using System;
using System.Collections.Generic;
using FractalDataWorks.Services.SecretManagement.Abstractions;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault.Configuration;

/// <summary>
/// Configuration for Azure Key Vault secret management service.
/// </summary>
/// <remarks>
/// Provides configuration options for connecting to and authenticating with Azure Key Vault,
/// including support for managed identity, service principal, and certificate-based authentication.
/// </remarks>
public sealed class AzureKeyVaultConfiguration : ISecretManagementConfiguration
{
    /// <inheritdoc/>
    public string SectionName => "AzureKeyVault";

    /// <summary>
    /// Gets or sets the Azure Key Vault URI.
    /// </summary>
    /// <value>The full URI to the Azure Key Vault instance.</value>
    /// <example>https://myvault.vault.azure.net/</example>
    public string? VaultUri { get; set; }

    /// <summary>
    /// Gets or sets the authentication method to use.
    /// </summary>
    /// <value>The authentication method identifier.</value>
    /// <remarks>
    /// Supported values: "ManagedIdentity", "ServicePrincipal", "Certificate", "DeviceCode"
    /// </remarks>
    public string? AuthenticationMethod { get; set; }

    /// <summary>
    /// Gets or sets the Azure tenant ID.
    /// </summary>
    /// <value>The Azure Active Directory tenant ID.</value>
    /// <remarks>
    /// Required for service principal and certificate authentication.
    /// </remarks>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the Azure client ID (application ID).
    /// </summary>
    /// <value>The Azure application (client) ID.</value>
    /// <remarks>
    /// Required for service principal and certificate authentication.
    /// </remarks>
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the Azure client secret.
    /// </summary>
    /// <value>The Azure application client secret.</value>
    /// <remarks>
    /// Required for service principal authentication with client secret.
    /// Should be stored securely and not logged.
    /// </remarks>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the path to the client certificate file.
    /// </summary>
    /// <value>The file path to the X.509 certificate file (.pfx or .p12).</value>
    /// <remarks>
    /// Required for certificate-based authentication.
    /// </remarks>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// Gets or sets the certificate password.
    /// </summary>
    /// <value>The password for the certificate file.</value>
    /// <remarks>
    /// Required if the certificate file is password-protected.
    /// Should be stored securely and not logged.
    /// </remarks>
    public string? CertificatePassword { get; set; }

    /// <summary>
    /// Gets or sets the connection timeout for Key Vault operations.
    /// </summary>
    /// <value>The timeout duration for individual operations.</value>
    /// <remarks>
    /// Defaults to 30 seconds if not specified.
    /// </remarks>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets the retry policy configuration.
    /// </summary>
    /// <value>A dictionary of retry policy settings.</value>
    /// <remarks>
    /// Common settings include: MaxRetries, InitialDelay, MaxDelay, BackoffMultiplier
    /// </remarks>
    public IReadOnlyDictionary<string, object>? RetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets additional headers to include in Key Vault requests.
    /// </summary>
    /// <value>A dictionary of HTTP headers to add to requests.</value>
    /// <remarks>
    /// Useful for adding custom tracking headers or compliance requirements.
    /// </remarks>
    public IReadOnlyDictionary<string, string>? AdditionalHeaders { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable distributed tracing.
    /// </summary>
    /// <value><c>true</c> to enable distributed tracing; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// When enabled, Key Vault operations will be traced for observability.
    /// </remarks>
    public bool EnableTracing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate the Key Vault URI on startup.
    /// </summary>
    /// <value><c>true</c> to validate the URI on startup; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// When enabled, the service will attempt to connect to Key Vault during initialization
    /// to validate configuration and permissions.
    /// </remarks>
    public bool ValidateOnStartup { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of secrets to retrieve in a single list operation.
    /// </summary>
    /// <value>The maximum number of secrets per page.</value>
    /// <remarks>
    /// Defaults to 25 if not specified. Azure Key Vault supports up to 25 items per page.
    /// </remarks>
    public int? MaxSecretsPerPage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include deleted secrets in list operations.
    /// </summary>
    /// <value><c>true</c> to include deleted secrets by default; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// This setting affects the default behavior of list operations.
    /// Individual commands can override this setting.
    /// </remarks>
    public bool IncludeDeletedByDefault { get; set; }

    /// <summary>
    /// Gets or sets the resource identifier for managed identity authentication.
    /// </summary>
    /// <value>The managed identity resource identifier.</value>
    /// <remarks>
    /// Used when multiple managed identities are available.
    /// Can be a client ID, object ID, or resource ID.
    /// </remarks>
    public string? ManagedIdentityId { get; set; }

    /// <summary>
    /// Gets the configuration name.
    /// </summary>
    /// <value>A descriptive name for this configuration.</value>
    public string ConfigurationName => "AzureKeyVault";

    /// <summary>
    /// Gets a value indicating whether this configuration is valid.
    /// </summary>
    /// <value><c>true</c> if the configuration is valid; otherwise, <c>false</c>.</value>
    public bool IsValid => !string.IsNullOrWhiteSpace(VaultUri) && 
                           !string.IsNullOrWhiteSpace(AuthenticationMethod);

    /// <summary>
    /// Gets additional configuration properties as key-value pairs.
    /// </summary>
    /// <value>A dictionary of additional configuration properties.</value>
    public IReadOnlyDictionary<string, object> Properties => CreatePropertiesDictionary();

    private IReadOnlyDictionary<string, object> CreatePropertiesDictionary()
    {
        var properties = new Dictionary<string, object>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(VaultUri))
            properties["VaultUri"] = VaultUri;

        if (!string.IsNullOrWhiteSpace(AuthenticationMethod))
            properties["AuthenticationMethod"] = AuthenticationMethod;

        if (!string.IsNullOrWhiteSpace(TenantId))
            properties["TenantId"] = TenantId;

        if (!string.IsNullOrWhiteSpace(ClientId))
            properties["ClientId"] = ClientId;

        if (Timeout.HasValue)
            properties["Timeout"] = Timeout.Value;

        if (EnableTracing)
            properties["EnableTracing"] = EnableTracing;

        if (ValidateOnStartup)
            properties["ValidateOnStartup"] = ValidateOnStartup;

        if (MaxSecretsPerPage.HasValue)
            properties["MaxSecretsPerPage"] = MaxSecretsPerPage.Value;

        if (IncludeDeletedByDefault)
            properties["IncludeDeletedByDefault"] = IncludeDeletedByDefault;

        if (!string.IsNullOrWhiteSpace(ManagedIdentityId))
            properties["ManagedIdentityId"] = ManagedIdentityId;

        return properties;
    }

    /// <inheritdoc/>
    public FluentValidation.Results.ValidationResult Validate()
    {
        var validator = new AzureKeyVaultConfigurationValidator();
        return validator.Validate(this);
    }
}