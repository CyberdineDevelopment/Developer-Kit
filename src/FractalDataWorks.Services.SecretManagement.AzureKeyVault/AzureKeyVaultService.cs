using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FractalDataWorks.Results;
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Services.SecretManagement.Abstractions.Commands;
using FractalDataWorks.Services.SecretManagement.AzureKeyVault.Configuration;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault;

/// <summary>
/// Azure Key Vault implementation of the secret management service.
/// </summary>
/// <remarks>
/// Provides secure secret storage and retrieval using Microsoft Azure Key Vault.
/// Supports multiple authentication methods including managed identity, service principal,
/// and certificate-based authentication.
/// </remarks>
public sealed class AzureKeyVaultService : SecretManagementServiceBase<ISecretCommand, AzureKeyVaultConfiguration, AzureKeyVaultService>
{
    private readonly SecretClient _secretClient;
    private readonly AzureKeyVaultConfigurationValidator _configurationValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The Azure Key Vault configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or configuration is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public AzureKeyVaultService(ILogger<AzureKeyVaultService> logger, AzureKeyVaultConfiguration configuration)
        : base(logger, configuration)
    {
        _configurationValidator = new AzureKeyVaultConfigurationValidator();
        
        ValidateConfiguration();
        _secretClient = CreateSecretClient();
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult> Execute(ISecretCommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            return FdwResult.Failure("Command cannot be null.");

        try
        {
            Logger.LogDebug("Executing {CommandType} command with ID {CommandId}", 
                command.CommandType, command.CommandId);

            var result = command.CommandType switch
            {
                "GetSecret" => await ExecuteGetSecret(command, cancellationToken),
                "SetSecret" => await ExecuteSetSecret(command, cancellationToken),
                "DeleteSecret" => await ExecuteDeleteSecret(command, cancellationToken),
                "ListSecrets" => await ExecuteListSecrets(command, cancellationToken),
                "GetSecretVersions" => await ExecuteGetSecretVersions(command, cancellationToken),
                _ => FdwResult.Failure($"Unsupported command type: {command.CommandType}")
            };

            Logger.LogDebug("Command {CommandId} completed with success: {IsSuccess}", 
                command.CommandId, result.IsSuccess);

            return result;
        }
        catch (RequestFailedException ex)
        {
            Logger.LogError(ex, "Azure Key Vault request failed for command {CommandId}: {ErrorCode} - {ErrorMessage}", 
                command.CommandId, ex.ErrorCode, ex.Message);
            
            return FdwResult.Failure($"Azure Key Vault error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error executing command {CommandId}", command.CommandId);
            return FdwResult.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult<TOut>> Execute<TOut>(ISecretCommand command, CancellationToken cancellationToken)
    {
        var result = await ExecuteCore<TOut>(command).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(ISecretCommand command)
    {
        if (command == null)
            return FdwResult<T>.Failure("Command cannot be null.");

        var validationResult = command.Validate();
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return FdwResult<T>.Failure($"Command validation failed: {errors}");
        }

        try
        {
            var basicResult = await ExecuteCommandInternal(command, CancellationToken.None).ConfigureAwait(false);
            
            if (!basicResult.IsSuccess)
            {
                return FdwResult<T>.Failure(basicResult.Message);
            }

            // For non-generic results, we need to handle the return differently
            // This is a design issue - ExecuteCommandInternal returns IFdwResult (no value)
            // but ExecuteCore<T> needs to return IFdwResult<T> with a value
            // For now, return success with default value - this needs architectural review
            return FdwResult<T>.Success(default!);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing command {CommandType} with key {SecretKey}", 
                command.GetType().Name, command.SecretKey);
            return FdwResult<T>.Failure($"Command execution failed: {ex.Message}");
        }
    }

    private async Task<IFdwResult> ExecuteCommandInternal(ISecretCommand command, CancellationToken cancellationToken)
    {
        return command.CommandType switch
        {
            "GetSecret" => await ExecuteGetSecret(command, cancellationToken),
            "SetSecret" => await ExecuteSetSecret(command, cancellationToken),
            "DeleteSecret" => await ExecuteDeleteSecret(command, cancellationToken),
            "ListSecrets" => await ExecuteListSecrets(command, cancellationToken),
            "GetSecretVersions" => await ExecuteGetSecretVersions(command, cancellationToken),
            _ => FdwResult.Failure($"Unknown command type: {command.CommandType}")
        };
    }

    private async Task<IFdwResult> ExecuteGetSecret(ISecretCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SecretKey))
            return FdwResult.Failure("SecretKey is required for GetSecret operation.");

        try
        {
            var secretName = SanitizeSecretName(command.SecretKey);
            
            // Check if a specific version is requested
            var version = command.Parameters.TryGetValue("Version", out var versionObj) ? 
                versionObj?.ToString() : null;

            KeyVaultSecret secret;
            if (!string.IsNullOrWhiteSpace(version))
            {
                secret = await _secretClient.GetSecretAsync(secretName, version, cancellationToken);
            }
            else
            {
                secret = await _secretClient.GetSecretAsync(secretName, null, cancellationToken);
            }

            var includeMetadata = command.Parameters.TryGetValue("IncludeMetadata", out var includeObj) && 
                                 includeObj is bool include && include;

            var secretValue = CreateSecretValue(secret, includeMetadata);
            
            Logger.LogDebug("Successfully retrieved secret {SecretName} with version {Version}", 
                secretName, secret.Properties.Version);

            return FdwResult.Success();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Logger.LogWarning("Secret {SecretKey} not found", command.SecretKey);
            return FdwResult.Failure($"Secret '{command.SecretKey}' not found.");
        }
    }

    private async Task<IFdwResult> ExecuteSetSecret(ISecretCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SecretKey))
            return FdwResult.Failure("SecretKey is required for SetSecret operation.");

        if (!command.Parameters.TryGetValue("SecretValue", out var secretValueObj) || 
            secretValueObj?.ToString() is not string secretValue)
        {
            return FdwResult.Failure("SecretValue parameter is required for SetSecret operation.");
        }

        try
        {
            var secretName = SanitizeSecretName(command.SecretKey);
            var secretOptions = new KeyVaultSecret(secretName, secretValue);

            // Apply optional parameters
            if (command.Parameters.TryGetValue("Description", out var descObj) && 
                descObj?.ToString() is string description)
            {
                secretOptions.Properties.ContentType = description;
            }

            if (command.Parameters.TryGetValue("ExpirationDate", out var expiryObj) && 
                expiryObj is DateTimeOffset expirationDate)
            {
                secretOptions.Properties.ExpiresOn = expirationDate;
            }

            if (command.Parameters.TryGetValue("Tags", out var tagsObj) && 
                tagsObj is IReadOnlyDictionary<string, string> tags)
            {
                foreach (var tag in tags)
                {
                    secretOptions.Properties.Tags[tag.Key] = tag.Value;
                }
            }

            var response = await _secretClient.SetSecretAsync(secretOptions, cancellationToken);
            var resultValue = CreateSecretValue(response.Value, true);
            
            Logger.LogInformation("Successfully set secret {SecretName} with version {Version}", 
                secretName, response.Value.Properties.Version);

            return FdwResult.Success();
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            Logger.LogError("Access denied setting secret {SecretKey}: {ErrorMessage}", 
                command.SecretKey, ex.Message);
            return FdwResult.Failure($"Access denied setting secret '{command.SecretKey}': {ex.Message}");
        }
    }

    private async Task<IFdwResult> ExecuteDeleteSecret(ISecretCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SecretKey))
            return FdwResult.Failure("SecretKey is required for DeleteSecret operation.");

        try
        {
            var secretName = SanitizeSecretName(command.SecretKey);
            var isPermanent = command.Parameters.TryGetValue("PermanentDelete", out var permanentObj) && 
                             permanentObj is bool permanent && permanent;

            if (isPermanent)
            {
                // First delete, then purge
                await _secretClient.StartDeleteSecretAsync(secretName, cancellationToken);
                
                // Wait a bit for the delete to process
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                
                await _secretClient.PurgeDeletedSecretAsync(secretName, cancellationToken);
                
                Logger.LogInformation("Successfully purged secret {SecretName}", secretName);
            }
            else
            {
                await _secretClient.StartDeleteSecretAsync(secretName, cancellationToken);
                
                Logger.LogInformation("Successfully deleted secret {SecretName} (soft delete)", secretName);
            }

            return FdwResult.Success();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Logger.LogWarning("Secret {SecretKey} not found for deletion", command.SecretKey);
            return FdwResult.Failure($"Secret '{command.SecretKey}' not found.");
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            Logger.LogError("Access denied deleting secret {SecretKey}: {ErrorMessage}", 
                command.SecretKey, ex.Message);
            return FdwResult.Failure($"Access denied deleting secret '{command.SecretKey}': {ex.Message}");
        }
    }

    private async Task<IFdwResult> ExecuteListSecrets(ISecretCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var maxResults = command.Parameters.TryGetValue("MaxResults", out var maxObj) && 
                            maxObj is int max ? max : Configuration.MaxSecretsPerPage ?? 25;

            var includeDeleted = command.Parameters.TryGetValue("IncludeDeleted", out var includeDeletedObj) && 
                                includeDeletedObj is bool includeDeletedValue && includeDeletedValue;

            var filter = command.Parameters.TryGetValue("Filter", out var filterObj) ? 
                        filterObj?.ToString() : null;

            var secretMetadataList = new List<ISecretMetadata>();
            
            await foreach (var secretProperties in _secretClient.GetPropertiesOfSecretsAsync(cancellationToken))
            {
                if (secretMetadataList.Count >= maxResults)
                    break;

                // Apply filter if specified
                if (!string.IsNullOrWhiteSpace(filter) && 
                    !secretProperties.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var metadata = CreateSecretMetadata(secretProperties);
                secretMetadataList.Add(metadata);
            }

            Logger.LogDebug("Retrieved {Count} secrets", secretMetadataList.Count);

            return FdwResult<IReadOnlyList<ISecretMetadata>>.Success(secretMetadataList.AsReadOnly());
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            Logger.LogError("Access denied listing secrets: {ErrorMessage}", ex.Message);
            return FdwResult.Failure($"Access denied listing secrets: {ex.Message}");
        }
    }

    private async Task<IFdwResult> ExecuteGetSecretVersions(ISecretCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SecretKey))
            return FdwResult.Failure("SecretKey is required for GetSecretVersions operation.");

        try
        {
            var secretName = SanitizeSecretName(command.SecretKey);
            var versionMetadataList = new List<ISecretMetadata>();

            await foreach (var versionProperties in _secretClient.GetPropertiesOfSecretVersionsAsync(secretName, cancellationToken))
            {
                var metadata = CreateSecretMetadata(versionProperties);
                versionMetadataList.Add(metadata);
            }

            Logger.LogDebug("Retrieved {Count} versions for secret {SecretName}", 
                versionMetadataList.Count, secretName);

            return FdwResult<IReadOnlyList<ISecretMetadata>>.Success(versionMetadataList.AsReadOnly());
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Logger.LogWarning("Secret {SecretKey} not found for version listing", command.SecretKey);
            return FdwResult.Failure($"Secret '{command.SecretKey}' not found.");
        }
    }

    private SecretClient CreateSecretClient()
    {
        var vaultUri = new Uri(Configuration.VaultUri!);
        var credential = CreateCredential();
        
        var options = new SecretClientOptions();
        
        if (Configuration.EnableTracing)
        {
            // Configure tracing if enabled
            options.Diagnostics.IsDistributedTracingEnabled = true;
        }

        return new SecretClient(vaultUri, credential, options);
    }

    private TokenCredential CreateCredential()
    {
        return Configuration.AuthenticationMethod switch
        {
            "ManagedIdentity" => CreateManagedIdentityCredential(),
            "ServicePrincipal" => CreateServicePrincipalCredential(),
            "Certificate" => CreateCertificateCredential(),
            "DeviceCode" => CreateDeviceCodeCredential(),
            _ => throw new InvalidOperationException($"Unsupported authentication method: {Configuration.AuthenticationMethod}")
        };
    }

    private TokenCredential CreateManagedIdentityCredential()
    {
        if (!string.IsNullOrWhiteSpace(Configuration.ManagedIdentityId))
        {
            return new ManagedIdentityCredential(Configuration.ManagedIdentityId);
        }

        return new ManagedIdentityCredential();
    }

    private TokenCredential CreateServicePrincipalCredential()
    {
        if (string.IsNullOrWhiteSpace(Configuration.TenantId) ||
            string.IsNullOrWhiteSpace(Configuration.ClientId) ||
            string.IsNullOrWhiteSpace(Configuration.ClientSecret))
        {
            throw new InvalidOperationException("TenantId, ClientId, and ClientSecret are required for ServicePrincipal authentication.");
        }

        return new ClientSecretCredential(Configuration.TenantId, Configuration.ClientId, Configuration.ClientSecret);
    }

    private TokenCredential CreateCertificateCredential()
    {
        if (string.IsNullOrWhiteSpace(Configuration.TenantId) ||
            string.IsNullOrWhiteSpace(Configuration.ClientId) ||
            string.IsNullOrWhiteSpace(Configuration.CertificatePath))
        {
            throw new InvalidOperationException("TenantId, ClientId, and CertificatePath are required for Certificate authentication.");
        }

        return new ClientCertificateCredential(
            Configuration.TenantId,
            Configuration.ClientId,
            Configuration.CertificatePath,
            new ClientCertificateCredentialOptions
            {
                // Add certificate password if provided
                // Note: In a real implementation, you might want to load the certificate differently
            });
    }

    private TokenCredential CreateDeviceCodeCredential()
    {
        return new DeviceCodeCredential();
    }

    private void ValidateConfiguration()
    {
        var validationResult = _configurationValidator.Validate(Configuration);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException($"Configuration validation failed: {errors}");
        }
    }

    private static string SanitizeSecretName(string secretKey)
    {
        // Azure Key Vault secret names can only contain alphanumeric characters and hyphens
        // and must be 1-127 characters long
        return secretKey.Replace("_", "-").Replace(" ", "-").ToLowerInvariant();
    }

    private static SecretValue CreateSecretValue(KeyVaultSecret secret, bool includeMetadata)
    {
        var metadata = includeMetadata ? CreateSecretMetadata(secret.Properties) : null;
        
        var metadataDict = includeMetadata 
            ? new Dictionary<string, object>(StringComparer.Ordinal) { ["Metadata"] = metadata! }
            : null;

        return new SecretValue(
            secret.Name,
            secret.Value,
            secret.Properties.Version,
            secret.Properties.CreatedOn,
            secret.Properties.UpdatedOn,
            secret.Properties.ExpiresOn,
            metadataDict);
    }

    private static ISecretMetadata CreateSecretMetadata(SecretProperties properties)
    {
        return new AzureKeyVaultSecretMetadata(
            properties.Name,
            properties.Version,
            properties.CreatedOn,
            properties.UpdatedOn,
            properties.ExpiresOn,
            properties.Enabled ?? true,
            properties.Tags as IReadOnlyDictionary<string, string>);
    }
}