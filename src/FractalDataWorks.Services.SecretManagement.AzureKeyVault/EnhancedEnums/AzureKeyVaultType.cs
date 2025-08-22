using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement.AzureKeyVault.Configuration;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault.EnhancedEnums;

/// <summary>
/// Enhanced enum type for Azure Key Vault secret management service.
/// </summary>
[EnumOption]
public sealed class AzureKeyVaultType : SecretManagementServiceTypeBase<AzureKeyVaultService, AzureKeyVaultConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultType"/> class.
    /// </summary>
    public AzureKeyVaultType() : base(1, "AzureKeyVault", "Microsoft Azure Key Vault secret management service")
    {
    }

    /// <inheritdoc/>
    public override string[] SupportedSecretStores => new[] 
    { 
        "AzureKeyVault", 
        "Azure Key Vault", 
        "KeyVault", 
        "Microsoft Azure Key Vault" 
    };

    /// <inheritdoc/>
    public override string ProviderName => "Azure.Security.KeyVault.Secrets";

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedAuthenticationMethods => new[]
    {
        "ManagedIdentity",
        "ServicePrincipal",
        "Certificate",
        "DeviceCode"
    };

    /// <inheritdoc/>
    public override IReadOnlyList<string> SupportedOperations => new[]
    {
        "GetSecret",
        "SetSecret",
        "DeleteSecret",
        "ListSecrets",
        "GetSecretVersions",
        "RestoreSecret",
        "BackupSecret",
        "PurgeSecret"
    };

    /// <inheritdoc/>
    public override int Priority => 100;

    /// <inheritdoc/>
    public override bool SupportsEncryptionAtRest => true;

    /// <inheritdoc/>
    public override bool SupportsAuditLogging => true;

    /// <inheritdoc/>
    public override IServiceFactory<AzureKeyVaultService, AzureKeyVaultConfiguration> CreateTypedFactory()
    {
        return new AzureKeyVaultServiceFactory();
    }
}