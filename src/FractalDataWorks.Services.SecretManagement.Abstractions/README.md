# FractalDataWorks.Services.SecretManagement.Abstractions

The **FractalDataWorks.Services.SecretManagement.Abstractions** package provides comprehensive secret management capabilities for the FractalDataWorks Framework. This package defines interfaces and base classes for integrating with various secret storage providers including AWS Secrets Manager, Azure Key Vault, HashiCorp Vault, and custom secret management systems.

## Overview

This abstraction layer provides:

- **Multi-Provider Support** - AWS Secrets Manager, Azure Key Vault, HashiCorp Vault, and custom providers
- **Unified Command Interface** - Consistent secret operations across different storage technologies
- **Provider Discovery** - Automatic discovery and registration via EnhancedEnums
- **Versioning Support** - Secret versioning and rollback capabilities where supported
- **Batch Operations** - Efficient bulk secret operations
- **Metadata Management** - Rich secret metadata without exposing values
- **Binary Secret Support** - Handle both text and binary secret data
- **Expiration Management** - Automatic secret lifecycle and expiration handling

## Quick Start

### Using an Existing Secret Provider

```csharp
using FractalDataWorks.Services.SecretManagement.Abstractions;
using FractalDataWorks.Framework.Abstractions;

// Define a simple get secret command
public sealed class GetSecretCommand : ISecretCommand<string>
{
    public string CommandId { get; } = Guid.NewGuid().ToString();
    public string CommandType => "GetSecret";
    public string? Container { get; }
    public string? SecretKey { get; }
    public Type ExpectedResultType => typeof(string);
    public TimeSpan? Timeout => TimeSpan.FromSeconds(30);
    public bool IsSecretModifying => false;
    
    public IReadOnlyDictionary<string, object?> Parameters { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    public GetSecretCommand(string secretKey, string? container = null, string? version = null)
    {
        SecretKey = secretKey;
        Container = container;
        
        var parameters = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(version))
            parameters["Version"] = version;
            
        Parameters = parameters;
        
        Metadata = new Dictionary<string, object>
        {
            ["RequestedAt"] = DateTime.UtcNow,
            ["RequestType"] = "Get"
        };
    }
    
    public IFdwResult Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
            return FdwResult.Failure("Secret key is required");
            
        return FdwResult.Success();
    }
    
    public ISecretCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return new GetSecretCommand(SecretKey!, Container); // Implementation would use new parameters
    }
    
    public ISecretCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return new GetSecretCommand(SecretKey!, Container); // Implementation would use new metadata
    }
    
    public ISecretCommand<string> WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return (ISecretCommand<string>)WithParameters(newParameters);
    }
    
    public ISecretCommand<string> WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return (ISecretCommand<string>)WithMetadata(newMetadata);
    }
}

// Using the secret management service
public async Task<IFdwResult<string>> GetDatabasePasswordAsync()
{
    // Find a secret provider that supports the required container type
    var provider = SecretProviders.GetByProviderType("AzureKeyVault")
        .FirstOrDefault();
        
    if (provider == null)
        return FdwResult<string>.Failure("No Azure Key Vault provider found");
    
    // Execute the get secret command
    var command = new GetSecretCommand("database-password", "production-vault");
    var result = await provider.Execute(command);
    
    if (result.IsSuccess && result.Value is string secretValue)
    {
        Console.WriteLine("Secret retrieved successfully");
        return FdwResult<string>.Success(secretValue);
    }
    
    return FdwResult<string>.Failure("Failed to retrieve secret", result.Exception);
}
```

### Creating a Custom Secret Provider

```csharp
// Define configuration for your secret provider
public sealed class CustomSecretConfiguration : SecretConfiguration
{
    public string VaultUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool EnableEncryption { get; set; } = true;
    public string EncryptionKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = base.Validate().ToList();
        
        if (string.IsNullOrWhiteSpace(VaultUrl))
            errors.Add("Vault URL is required");
            
        if (!Uri.TryCreate(VaultUrl, UriKind.Absolute, out _))
            errors.Add("Vault URL must be a valid absolute URI");
            
        if (string.IsNullOrWhiteSpace(ApiKey))
            errors.Add("API key is required");
            
        if (EnableEncryption && string.IsNullOrWhiteSpace(EncryptionKey))
            errors.Add("Encryption key is required when encryption is enabled");
            
        if (TimeoutSeconds <= 0)
            errors.Add("Timeout must be positive");
            
        return errors;
    }
}

// Implement the secret provider
public sealed class CustomSecretProvider : SecretProviderBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomSecretProvider> _logger;
    private readonly IAesEncryptionService _encryptionService;
    
    public CustomSecretProvider(
        HttpClient httpClient,
        ILogger<CustomSecretProvider> logger,
        IAesEncryptionService encryptionService,
        CustomSecretConfiguration configuration) : base(
        id: 1,
        name: "Custom Secret Provider",
        providerType: "CustomVault",
        version: "1.0.0",
        supportedCommandTypes: new[] { "GetSecret", "SetSecret", "DeleteSecret", "ListSecrets", "GetSecretMetadata" },
        supportedContainerTypes: new[] { "Vault", "Container" },
        configuration: configuration,
        supportsVersioning: true,
        supportsExpiration: true,
        supportsBatchOperations: true,
        supportsBinarySecrets: true)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        
        // Configure HTTP client
        var config = (CustomSecretConfiguration)configuration;
        _httpClient.BaseAddress = new Uri(config.VaultUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
        _httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
    }
    
    public override async Task<IFdwResult<object?>> Execute(ISecretCommand command, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return FdwResult<object?>.Failure("Provider is not available");
            
        var validationResult = ValidateCommand(command);
        if (validationResult.IsFailure)
            return FdwResult<object?>.Failure(validationResult.ErrorMessage);
        
        try
        {
            return command.CommandType switch
            {
                "GetSecret" => await ExecuteGetSecret(command, cancellationToken),
                "SetSecret" => await ExecuteSetSecret(command, cancellationToken),
                "DeleteSecret" => await ExecuteDeleteSecret(command, cancellationToken),
                "ListSecrets" => await ExecuteListSecrets(command, cancellationToken),
                "GetSecretMetadata" => await ExecuteGetSecretMetadata(command, cancellationToken),
                _ => FdwResult<object?>.Failure($"Unsupported command type: {command.CommandType}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Secret command execution failed: {CommandType} for key {SecretKey}", 
                command.CommandType, command.SecretKey);
            return FdwResult<object?>.Failure($"Command execution failed: {ex.Message}", ex);
        }
    }
    
    public override async Task<IFdwResult<TResult>> Execute<TResult>(ISecretCommand<TResult> command, CancellationToken cancellationToken = default)
    {
        var result = await Execute((ISecretCommand)command, cancellationToken);
        if (result.IsFailure)
            return FdwResult<TResult>.Failure(result.ErrorMessage, result.Exception);
        
        if (result.Value is TResult typedResult)
            return FdwResult<TResult>.Success(typedResult);
        
        try
        {
            var convertedResult = (TResult)Convert.ChangeType(result.Value, typeof(TResult));
            return FdwResult<TResult>.Success(convertedResult);
        }
        catch (Exception ex)
        {
            return FdwResult<TResult>.Failure($"Failed to convert result to {typeof(TResult).Name}", ex);
        }
    }
    
    private async Task<IFdwResult<object?>> ExecuteGetSecret(ISecretCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SecretKey))
            return FdwResult<object?>.Failure("Secret key is required");
        
        try
        {
            var url = BuildSecretUrl(command.Container, command.SecretKey);
            
            // Add version parameter if specified
            if (command.Parameters.TryGetValue("Version", out var version) && version != null)
            {
                url += $"?version={Uri.EscapeDataString(version.ToString())}";
            }
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var secretResponse = JsonSerializer.Deserialize<SecretResponse>(content);
                
                if (secretResponse != null)
                {
                    // Decrypt if encryption is enabled
                    var secretValue = secretResponse.Value;
                    if (((CustomSecretConfiguration)Configuration).EnableEncryption && secretResponse.IsEncrypted)
                    {
                        secretValue = await _encryptionService.DecryptAsync(secretValue);
                    }
                    
                    var secretData = new CustomSecretValue
                    {
                        Key = command.SecretKey,
                        Value = secretValue,
                        Container = command.Container,
                        Version = secretResponse.Version,
                        CreatedAt = secretResponse.CreatedAt,
                        ModifiedAt = secretResponse.ModifiedAt,
                        ExpiresAt = secretResponse.ExpiresAt,
                        IsBinary = secretResponse.IsBinary,
                        Tags = secretResponse.Tags?.ToList() ?? new List<string>(),
                        Properties = secretResponse.Properties ?? new Dictionary<string, object>()
                    };
                    
                    return FdwResult<object?>.Success(secretValue); // Return just the value for simple scenarios
                }
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return FdwResult<object?>.Failure($"Secret '{command.SecretKey}' not found");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return FdwResult<object?>.Failure($"Failed to retrieve secret: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            return FdwResult<object?>.Failure($"HTTP request failed: {ex.Message}", ex);
        }
        
        return FdwResult<object?>.Failure("Failed to retrieve secret");
    }
    
    private async Task<IFdwResult<object?>> ExecuteSetSecret(ISecretCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SecretKey))
            return FdwResult<object?>.Failure("Secret key is required");
        
        if (!command.Parameters.TryGetValue("SecretValue", out var secretValue) || secretValue == null)
            return FdwResult<object?>.Failure("Secret value is required");
        
        try
        {
            var url = BuildSecretUrl(command.Container, command.SecretKey);
            var value = secretValue.ToString();
            
            // Encrypt if encryption is enabled
            if (((CustomSecretConfiguration)Configuration).EnableEncryption && !string.IsNullOrEmpty(value))
            {
                value = await _encryptionService.EncryptAsync(value);
            }
            
            var secretRequest = new SetSecretRequest
            {
                Value = value,
                IsEncrypted = ((CustomSecretConfiguration)Configuration).EnableEncryption,
                Description = command.Parameters.GetValueOrDefault("Description")?.ToString(),
                Tags = command.Parameters.GetValueOrDefault("Tags") as string[],
                ExpiresAt = command.Parameters.GetValueOrDefault("ExpiresAt") as DateTimeOffset?,
                Properties = command.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
            
            var json = JsonSerializer.Serialize(secretRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(url, content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var setResponse = JsonSerializer.Deserialize<SetSecretResponse>(responseContent);
                
                var result = new CustomSecretOperationResult
                {
                    Success = true,
                    SecretKey = command.SecretKey,
                    Version = setResponse?.Version,
                    Operation = "Set",
                    Timestamp = DateTimeOffset.UtcNow
                };
                
                return FdwResult<object?>.Success(result);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return FdwResult<object?>.Failure($"Failed to set secret: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"Set secret operation failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<object?>> ExecuteDeleteSecret(ISecretCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SecretKey))
            return FdwResult<object?>.Failure("Secret key is required");
        
        try
        {
            var url = BuildSecretUrl(command.Container, command.SecretKey);
            
            // Add version parameter if specified
            if (command.Parameters.TryGetValue("Version", out var version) && version != null)
            {
                url += $"?version={Uri.EscapeDataString(version.ToString())}";
            }
            
            var response = await _httpClient.DeleteAsync(url, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = new CustomSecretOperationResult
                {
                    Success = true,
                    SecretKey = command.SecretKey,
                    Operation = "Delete",
                    Timestamp = DateTimeOffset.UtcNow
                };
                
                return FdwResult<object?>.Success(result);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return FdwResult<object?>.Failure($"Secret '{command.SecretKey}' not found");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return FdwResult<object?>.Failure($"Failed to delete secret: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"Delete secret operation failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<object?>> ExecuteListSecrets(ISecretCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var url = BuildContainerUrl(command.Container);
            
            // Add query parameters
            var queryParams = new List<string>();
            if (command.Parameters.TryGetValue("Filter", out var filter) && filter != null)
            {
                queryParams.Add($"filter={Uri.EscapeDataString(filter.ToString())}");
            }
            if (command.Parameters.TryGetValue("Limit", out var limit) && limit != null)
            {
                queryParams.Add($"limit={limit}");
            }
            if (command.Parameters.TryGetValue("ContinuationToken", out var token) && token != null)
            {
                queryParams.Add($"continuation={Uri.EscapeDataString(token.ToString())}");
            }
            
            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var listResponse = JsonSerializer.Deserialize<ListSecretsResponse>(content);
                
                if (listResponse != null)
                {
                    var secretList = new CustomSecretList
                    {
                        Secrets = listResponse.Secrets?.Select(s => new CustomSecretMetadata
                        {
                            Key = s.Key,
                            Container = command.Container,
                            Version = s.Version,
                            CreatedAt = s.CreatedAt,
                            ModifiedAt = s.ModifiedAt,
                            ExpiresAt = s.ExpiresAt,
                            IsEnabled = s.IsEnabled,
                            IsBinary = s.IsBinary,
                            SizeInBytes = s.SizeInBytes,
                            Tags = s.Tags?.ToList() ?? new List<string>(),
                            Properties = s.Properties ?? new Dictionary<string, object>()
                        }).Cast<ISecretMetadata>().ToList() ?? new List<ISecretMetadata>(),
                        ContinuationToken = listResponse.ContinuationToken,
                        TotalCount = listResponse.TotalCount
                    };
                    
                    return FdwResult<object?>.Success(secretList);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return FdwResult<object?>.Failure($"Failed to list secrets: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"List secrets operation failed: {ex.Message}", ex);
        }
        
        return FdwResult<object?>.Failure("Failed to list secrets");
    }
    
    private async Task<IFdwResult<object?>> ExecuteGetSecretMetadata(ISecretCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SecretKey))
            return FdwResult<object?>.Failure("Secret key is required");
        
        try
        {
            var url = BuildSecretUrl(command.Container, command.SecretKey) + "/metadata";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var metadataResponse = JsonSerializer.Deserialize<SecretMetadataResponse>(content);
                
                if (metadataResponse != null)
                {
                    var metadata = new CustomSecretMetadata
                    {
                        Key = command.SecretKey,
                        Container = command.Container,
                        Version = metadataResponse.Version,
                        CreatedAt = metadataResponse.CreatedAt,
                        ModifiedAt = metadataResponse.ModifiedAt,
                        ExpiresAt = metadataResponse.ExpiresAt,
                        CreatedBy = metadataResponse.CreatedBy,
                        ModifiedBy = metadataResponse.ModifiedBy,
                        IsEnabled = metadataResponse.IsEnabled,
                        IsBinary = metadataResponse.IsBinary,
                        SizeInBytes = metadataResponse.SizeInBytes,
                        Tags = metadataResponse.Tags?.ToList() ?? new List<string>(),
                        Properties = metadataResponse.Properties ?? new Dictionary<string, object>(),
                        AvailableVersions = metadataResponse.AvailableVersions?.ToList() ?? new List<string>(),
                        AccessPolicy = metadataResponse.AccessPolicy,
                        EncryptionMethod = metadataResponse.EncryptionMethod
                    };
                    
                    return FdwResult<object?>.Success(metadata);
                }
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return FdwResult<object?>.Failure($"Secret '{command.SecretKey}' not found");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return FdwResult<object?>.Failure($"Failed to get secret metadata: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"Get secret metadata operation failed: {ex.Message}", ex);
        }
        
        return FdwResult<object?>.Failure("Failed to get secret metadata");
    }
    
    private string BuildSecretUrl(string? container, string secretKey)
    {
        var cleanContainer = string.IsNullOrWhiteSpace(container) ? "default" : container;
        return $"/api/v1/containers/{Uri.EscapeDataString(cleanContainer)}/secrets/{Uri.EscapeDataString(secretKey)}";
    }
    
    private string BuildContainerUrl(string? container)
    {
        var cleanContainer = string.IsNullOrWhiteSpace(container) ? "default" : container;
        return $"/api/v1/containers/{Uri.EscapeDataString(cleanContainer)}/secrets";
    }
    
    public override async Task<IFdwResult<ISecretProviderHealth>> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            
            var health = new CustomSecretProviderHealth
            {
                IsHealthy = response.IsSuccessStatusCode,
                LastChecked = DateTimeOffset.UtcNow,
                ResponseTime = TimeSpan.FromMilliseconds(100), // Would measure actual response time
                EndpointUrl = _httpClient.BaseAddress?.ToString() ?? "",
                StatusCode = (int)response.StatusCode,
                ContainersAccessible = response.IsSuccessStatusCode ? 5 : 0, // Would check actual containers
                EncryptionStatus = ((CustomSecretConfiguration)Configuration).EnableEncryption ? "Enabled" : "Disabled"
            };
            
            return FdwResult<ISecretProviderHealth>.Success(health);
        }
        catch (Exception ex)
        {
            var health = new CustomSecretProviderHealth
            {
                IsHealthy = false,
                LastChecked = DateTimeOffset.UtcNow,
                ErrorMessage = ex.Message,
                EndpointUrl = _httpClient.BaseAddress?.ToString() ?? ""
            };
            
            return FdwResult<ISecretProviderHealth>.Success(health);
        }
    }
    
    public override async Task<IFdwResult<IReadOnlyCollection<ISecretContainer>>> GetContainersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/containers", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var containersResponse = JsonSerializer.Deserialize<ContainersResponse>(content);
                
                var containers = containersResponse?.Containers?.Select(c => new CustomSecretContainer
                {
                    Name = c.Name,
                    Description = c.Description,
                    IsEnabled = c.IsEnabled,
                    CreatedAt = c.CreatedAt,
                    SecretCount = c.SecretCount,
                    SizeInBytes = c.SizeInBytes,
                    Properties = c.Properties ?? new Dictionary<string, object>()
                }).Cast<ISecretContainer>().ToList() ?? new List<ISecretContainer>();
                
                return FdwResult<IReadOnlyCollection<ISecretContainer>>.Success(containers);
            }
            else
            {
                return FdwResult<IReadOnlyCollection<ISecretContainer>>.Failure($"Failed to get containers: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return FdwResult<IReadOnlyCollection<ISecretContainer>>.Failure("Get containers operation failed", ex);
        }
    }
    
    public override async Task<IFdwResult<ISecretProviderMetrics>> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var metrics = new CustomSecretProviderMetrics
        {
            TotalSecretsManaged = 1000, // Would track actual metrics
            SecretsRetrieved = 5000,
            SecretsStored = 200,
            SecretsDeleted = 50,
            AverageResponseTime = TimeSpan.FromMilliseconds(120),
            EncryptedSecretsCount = 950,
            ExpiredSecretsCount = 25,
            ContainersCount = 5,
            HealthyContainers = 5
        };
        
        return FdwResult<ISecretProviderMetrics>.Success(metrics);
    }
    
    protected override ISecretCommandResult CreateCommandResult(ISecretCommand command, int batchPosition, 
        bool isSuccessful, object? resultData, string? errorMessage, IReadOnlyList<string>? errorDetails, 
        Exception? exception, TimeSpan executionTime, DateTimeOffset startedAt, DateTimeOffset completedAt)
    {
        return new CustomSecretCommandResult(command, batchPosition, isSuccessful, resultData, errorMessage, 
            errorDetails, exception, executionTime, startedAt, completedAt);
    }
    
    protected override ISecretBatchResult CreateBatchResult(string batchId, int totalCommands, 
        int successfulCommands, int failedCommands, int skippedCommands, TimeSpan executionTime, 
        DateTimeOffset startedAt, DateTimeOffset completedAt, IReadOnlyList<ISecretCommandResult> commandResults, 
        IReadOnlyList<string> batchErrors)
    {
        return new CustomSecretBatchResult(batchId, totalCommands, successfulCommands, failedCommands, 
            skippedCommands, executionTime, startedAt, completedAt, commandResults, batchErrors);
    }
    
    public override void RegisterService(IServiceCollection services)
    {
        services.AddSingleton<CustomSecretProvider>();
        services.AddSingleton<ISecretProvider>(sp => sp.GetRequiredService<CustomSecretProvider>());
        services.AddHttpClient<CustomSecretProvider>();
        services.AddSingleton<IAesEncryptionService, AesEncryptionService>();
    }
}

// Supporting classes for the custom provider
public class SecretResponse
{
    public string Value { get; set; } = string.Empty;
    public string? Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsBinary { get; set; }
    public string[]? Tags { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

public class SetSecretRequest
{
    public string Value { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class SetSecretResponse
{
    public string? Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ListSecretsResponse
{
    public SecretListItem[]? Secrets { get; set; }
    public string? ContinuationToken { get; set; }
    public int TotalCount { get; set; }
}

public class SecretListItem
{
    public string Key { get; set; } = string.Empty;
    public string? Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsBinary { get; set; }
    public long SizeInBytes { get; set; }
    public string[]? Tags { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

public class SecretMetadataResponse
{
    public string? Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsBinary { get; set; }
    public long SizeInBytes { get; set; }
    public string[]? Tags { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
    public string[]? AvailableVersions { get; set; }
    public string? AccessPolicy { get; set; }
    public string? EncryptionMethod { get; set; }
}

public class ContainersResponse
{
    public ContainerInfo[]? Containers { get; set; }
}

public class ContainerInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int SecretCount { get; set; }
    public long SizeInBytes { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}
```

## Implementation Examples

### AWS Secrets Manager Provider

```csharp
public sealed class AwsSecretsManagerProvider : SecretProviderBase
{
    public static readonly AwsSecretsManagerProvider Instance = new();
    
    private AwsSecretsManagerProvider() : base(
        id: 2,
        name: "AWS Secrets Manager Provider",
        providerType: "AwsSecretsManager",
        version: "1.0.0",
        supportedCommandTypes: new[] { "GetSecret", "SetSecret", "DeleteSecret", "ListSecrets", "GetSecretVersions" },
        supportedContainerTypes: new[] { "SecretsManager" },
        configuration: CreateDefaultConfiguration(),
        supportsVersioning: true,
        supportsExpiration: false,
        supportsBatchOperations: false,
        supportsBinarySecrets: true)
    {
    }
    
    private static SecretConfiguration CreateDefaultConfiguration()
    {
        return new AwsSecretsManagerConfiguration
        {
            ProviderId = "aws-secrets-manager",
            Region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1",
            AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? "",
            SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? "",
            SessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN")
        };
    }
    
    public override async Task<IFdwResult<object?>> Execute(ISecretCommand command, CancellationToken cancellationToken = default)
    {
        // AWS-specific implementation using AWS SDK
        return command.CommandType switch
        {
            "GetSecret" => await ExecuteAwsGetSecret(command, cancellationToken),
            "SetSecret" => await ExecuteAwsSetSecret(command, cancellationToken),
            _ => FdwResult<object?>.Failure($"Unsupported command: {command.CommandType}")
        };
    }
    
    private async Task<IFdwResult<object?>> ExecuteAwsGetSecret(ISecretCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var config = (AwsSecretsManagerConfiguration)Configuration;
            using var client = new AmazonSecretsManagerClient(config.AccessKeyId, config.SecretAccessKey, 
                config.SessionToken, Amazon.RegionEndpoint.GetBySystemName(config.Region));
            
            var request = new GetSecretValueRequest
            {
                SecretId = command.SecretKey,
                VersionId = command.Parameters.GetValueOrDefault("Version")?.ToString()
            };
            
            var response = await client.GetSecretValueAsync(request, cancellationToken);
            
            var secretValue = new AwsSecretValue
            {
                Key = response.Name,
                Value = response.SecretString ?? Convert.ToBase64String(response.SecretBinary?.ToArray() ?? Array.Empty<byte>()),
                Version = response.VersionId,
                CreatedAt = response.CreatedDate,
                IsBinary = response.SecretBinary != null,
                Arn = response.ARN
            };
            
            return FdwResult<object?>.Success(secretValue.Value); // Return just the value
        }
        catch (ResourceNotFoundException)
        {
            return FdwResult<object?>.Failure($"Secret '{command.SecretKey}' not found");
        }
        catch (AmazonSecretsManagerException ex)
        {
            return FdwResult<object?>.Failure($"AWS Secrets Manager error: {ex.Message}", ex);
        }
    }
    
    // Additional AWS implementation methods...
}

public sealed class AwsSecretsManagerConfiguration : SecretConfiguration
{
    public string Region { get; set; } = "us-east-1";
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = base.Validate().ToList();
        
        if (string.IsNullOrWhiteSpace(Region))
            errors.Add("AWS region is required");
            
        if (string.IsNullOrWhiteSpace(AccessKeyId))
            errors.Add("AWS access key ID is required");
            
        if (string.IsNullOrWhiteSpace(SecretAccessKey))
            errors.Add("AWS secret access key is required");
            
        return errors;
    }
}
```

### Azure Key Vault Provider

```csharp
public sealed class AzureKeyVaultProvider : SecretProviderBase
{
    public static readonly AzureKeyVaultProvider Instance = new();
    
    private AzureKeyVaultProvider() : base(
        id: 3,
        name: "Azure Key Vault Provider",
        providerType: "AzureKeyVault",
        version: "1.0.0",
        supportedCommandTypes: new[] { "GetSecret", "SetSecret", "DeleteSecret", "ListSecrets", "GetSecretVersions" },
        supportedContainerTypes: new[] { "KeyVault" },
        configuration: CreateDefaultConfiguration(),
        supportsVersioning: true,
        supportsExpiration: true,
        supportsBatchOperations: false,
        supportsBinarySecrets: false) // Azure Key Vault only supports text secrets
    {
    }
    
    private static SecretConfiguration CreateDefaultConfiguration()
    {
        return new AzureKeyVaultConfiguration
        {
            ProviderId = "azure-key-vault",
            VaultUrl = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URL") ?? "",
            TenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? "",
            ClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? "",
            ClientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ?? ""
        };
    }
    
    public override async Task<IFdwResult<object?>> Execute(ISecretCommand command, CancellationToken cancellationToken = default)
    {
        return command.CommandType switch
        {
            "GetSecret" => await ExecuteAzureGetSecret(command, cancellationToken),
            "SetSecret" => await ExecuteAzureSetSecret(command, cancellationToken),
            _ => FdwResult<object?>.Failure($"Unsupported command: {command.CommandType}")
        };
    }
    
    private async Task<IFdwResult<object?>> ExecuteAzureGetSecret(ISecretCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var config = (AzureKeyVaultConfiguration)Configuration;
            var credential = new ClientSecretCredential(config.TenantId, config.ClientId, config.ClientSecret);
            var client = new SecretClient(new Uri(config.VaultUrl), credential);
            
            KeyVaultSecret secret;
            if (command.Parameters.TryGetValue("Version", out var version) && version != null)
            {
                secret = await client.GetSecretAsync(command.SecretKey, version.ToString(), cancellationToken);
            }
            else
            {
                secret = await client.GetSecretAsync(command.SecretKey, cancellationToken);
            }
            
            var secretValue = new AzureSecretValue
            {
                Key = secret.Name,
                Value = secret.Value,
                Version = secret.Properties.Version,
                CreatedAt = secret.Properties.CreatedOn ?? DateTimeOffset.UtcNow,
                ModifiedAt = secret.Properties.UpdatedOn ?? DateTimeOffset.UtcNow,
                ExpiresAt = secret.Properties.ExpiresOn,
                IsEnabled = secret.Properties.Enabled ?? true,
                Tags = secret.Properties.Tags.Keys.ToList(),
                Properties = secret.Properties.Tags.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)
            };
            
            return FdwResult<object?>.Success(secretValue.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return FdwResult<object?>.Failure($"Secret '{command.SecretKey}' not found");
        }
        catch (RequestFailedException ex)
        {
            return FdwResult<object?>.Failure($"Azure Key Vault error: {ex.Message}", ex);
        }
    }
    
    // Additional Azure implementation methods...
}

public sealed class AzureKeyVaultConfiguration : SecretConfiguration
{
    public string VaultUrl { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = base.Validate().ToList();
        
        if (string.IsNullOrWhiteSpace(VaultUrl))
            errors.Add("Azure Key Vault URL is required");
            
        if (!Uri.TryCreate(VaultUrl, UriKind.Absolute, out _))
            errors.Add("Azure Key Vault URL must be a valid absolute URI");
            
        if (string.IsNullOrWhiteSpace(TenantId))
            errors.Add("Azure tenant ID is required");
            
        if (string.IsNullOrWhiteSpace(ClientId))
            errors.Add("Azure client ID is required");
            
        if (string.IsNullOrWhiteSpace(ClientSecret))
            errors.Add("Azure client secret is required");
            
        return errors;
    }
}
```

### HashiCorp Vault Provider

```csharp
public sealed class HashiCorpVaultProvider : SecretProviderBase
{
    public static readonly HashiCorpVaultProvider Instance = new();
    
    private HashiCorpVaultProvider() : base(
        id: 4,
        name: "HashiCorp Vault Provider",
        providerType: "HashiCorpVault",
        version: "1.0.0",
        supportedCommandTypes: new[] { "GetSecret", "SetSecret", "DeleteSecret", "ListSecrets", "GetSecretVersions" },
        supportedContainerTypes: new[] { "Mount", "Engine" },
        configuration: CreateDefaultConfiguration(),
        supportsVersioning: true,
        supportsExpiration: false,
        supportsBatchOperations: true,
        supportsBinarySecrets: true)
    {
    }
    
    private static SecretConfiguration CreateDefaultConfiguration()
    {
        return new HashiCorpVaultConfiguration
        {
            ProviderId = "hashicorp-vault",
            VaultUrl = Environment.GetEnvironmentVariable("VAULT_ADDR") ?? "http://localhost:8200",
            Token = Environment.GetEnvironmentVariable("VAULT_TOKEN") ?? "",
            Mount = Environment.GetEnvironmentVariable("VAULT_MOUNT") ?? "secret",
            ApiVersion = "v1"
        };
    }
    
    public override async Task<IFdwResult<object?>> Execute(ISecretCommand command, CancellationToken cancellationToken = default)
    {
        return command.CommandType switch
        {
            "GetSecret" => await ExecuteVaultGetSecret(command, cancellationToken),
            "SetSecret" => await ExecuteVaultSetSecret(command, cancellationToken),
            _ => FdwResult<object?>.Failure($"Unsupported command: {command.CommandType}")
        };
    }
    
    private async Task<IFdwResult<object?>> ExecuteVaultGetSecret(ISecretCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var config = (HashiCorpVaultConfiguration)Configuration;
            var vaultClient = new VaultClient(new VaultClientSettings(config.VaultUrl, new TokenAuthMethodInfo(config.Token)));
            
            var secretPath = BuildSecretPath(command.Container ?? config.Mount, command.SecretKey);
            Secret<SecretData> secret;
            
            if (command.Parameters.TryGetValue("Version", out var version) && version != null)
            {
                secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretPath, int.Parse(version.ToString()), mountPoint: config.Mount);
            }
            else
            {
                secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretPath, mountPoint: config.Mount);
            }
            
            var secretValue = new HashiCorpSecretValue
            {
                Key = command.SecretKey,
                Value = JsonSerializer.Serialize(secret.Data.Data), // Vault returns JSON objects
                Version = secret.Data.Metadata.Version.ToString(),
                CreatedAt = secret.Data.Metadata.CreatedTime,
                ModifiedAt = secret.Data.Metadata.CreatedTime,
                Container = command.Container ?? config.Mount,
                Properties = secret.Data.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
            
            // If only one key-value pair and it's named "value", return just the value
            if (secret.Data.Data.Count == 1 && secret.Data.Data.ContainsKey("value"))
            {
                return FdwResult<object?>.Success(secret.Data.Data["value"]);
            }
            
            return FdwResult<object?>.Success(secret.Data.Data);
        }
        catch (VaultApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            return FdwResult<object?>.Failure($"Secret '{command.SecretKey}' not found");
        }
        catch (VaultApiException ex)
        {
            return FdwResult<object?>.Failure($"HashiCorp Vault error: {ex.Message}", ex);
        }
    }
    
    private string BuildSecretPath(string mount, string? secretKey)
    {
        return $"{mount}/data/{secretKey}"; // KV v2 engine format
    }
    
    // Additional HashiCorp Vault implementation methods...
}

public sealed class HashiCorpVaultConfiguration : SecretConfiguration
{
    public string VaultUrl { get; set; } = "http://localhost:8200";
    public string Token { get; set; } = string.Empty;
    public string Mount { get; set; } = "secret";
    public string ApiVersion { get; set; } = "v1";
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = base.Validate().ToList();
        
        if (string.IsNullOrWhiteSpace(VaultUrl))
            errors.Add("Vault URL is required");
            
        if (!Uri.TryCreate(VaultUrl, UriKind.Absolute, out _))
            errors.Add("Vault URL must be a valid absolute URI");
            
        if (string.IsNullOrWhiteSpace(Token))
            errors.Add("Vault token is required");
            
        if (string.IsNullOrWhiteSpace(Mount))
            errors.Add("Vault mount is required");
            
        return errors;
    }
}
```

## Configuration Examples

### JSON Configuration for Multiple Providers

```json
{
  "SecretManagement": {
    "Providers": {
      "AzureKeyVault": {
        "ProviderId": "azure-key-vault",
        "VaultUrl": "https://your-vault.vault.azure.net/",
        "TenantId": "your-tenant-id",
        "ClientId": "your-client-id",
        "ClientSecret": "your-client-secret",
        "Enabled": true
      },
      "AwsSecretsManager": {
        "ProviderId": "aws-secrets-manager",
        "Region": "us-east-1",
        "AccessKeyId": "your-access-key-id",
        "SecretAccessKey": "your-secret-access-key",
        "Enabled": true
      },
      "HashiCorpVault": {
        "ProviderId": "hashicorp-vault",
        "VaultUrl": "https://vault.company.com:8200",
        "Token": "your-vault-token",
        "Mount": "secret",
        "ApiVersion": "v1",
        "Enabled": true
      },
      "CustomVault": {
        "ProviderId": "custom-vault",
        "VaultUrl": "https://secrets.company.com",
        "ApiKey": "your-api-key",
        "EnableEncryption": true,
        "EncryptionKey": "your-encryption-key",
        "TimeoutSeconds": 30,
        "Enabled": true
      }
    },
    "DefaultProvider": "AzureKeyVault",
    "EnableCaching": true,
    "CacheExpirationMinutes": 15,
    "EnableAuditLogging": true
  }
}
```

### Dependency Injection Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure secret provider settings
    services.Configure<AzureKeyVaultConfiguration>(
        Configuration.GetSection("SecretManagement:Providers:AzureKeyVault"));
    services.Configure<AwsSecretsManagerConfiguration>(
        Configuration.GetSection("SecretManagement:Providers:AwsSecretsManager"));
    services.Configure<HashiCorpVaultConfiguration>(
        Configuration.GetSection("SecretManagement:Providers:HashiCorpVault"));
    services.Configure<CustomSecretConfiguration>(
        Configuration.GetSection("SecretManagement:Providers:CustomVault"));
    
    // Register HTTP clients for providers that need them
    services.AddHttpClient<CustomSecretProvider>();
    
    // Register encryption services
    services.AddSingleton<IAesEncryptionService, AesEncryptionService>();
    
    // Register secret providers (auto-discovered via SecretProviders collection)
    services.AddSecretProviders();
    
    // Register secret management services
    services.AddScoped<ISecretManager, SecretManager>();
    services.AddSingleton<ISecretProviderRouter, SecretProviderRouter>();
    services.AddSingleton<ISecretCache, MemorySecretCache>();
    
    // Add background services for secret management
    services.AddHostedService<SecretExpirationMonitor>();
    services.AddHostedService<SecretHealthMonitor>();
}

// Extension method for bulk registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecretProviders(this IServiceCollection services)
    {
        // Register all discovered secret providers
        foreach (var provider in SecretProviders.All)
        {
            provider.RegisterService(services);
        }
        
        return services;
    }
}
```

## Advanced Usage

### Secret Manager with Caching

```csharp
public sealed class SecretManager
{
    private readonly ISecretProviderRouter _router;
    private readonly ISecretCache _cache;
    private readonly ILogger<SecretManager> _logger;
    
    public SecretManager(
        ISecretProviderRouter router,
        ISecretCache cache,
        ILogger<SecretManager> logger)
    {
        _router = router;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<IFdwResult<string>> GetSecretAsync(string secretKey, string? container = null, bool useCache = true)
    {
        var cacheKey = $"{container ?? "default"}:{secretKey}";
        
        // Check cache first if enabled
        if (useCache && _cache.TryGetValue(cacheKey, out var cachedValue))
        {
            _logger.LogDebug("Retrieved secret from cache: {SecretKey}", secretKey);
            return FdwResult<string>.Success(cachedValue);
        }
        
        // Get from provider
        var command = new GetSecretCommand(secretKey, container);
        var result = await _router.ExecuteCommandAsync<string>(command);
        
        if (result.IsSuccess && useCache)
        {
            // Cache the result
            await _cache.SetAsync(cacheKey, result.Value, TimeSpan.FromMinutes(15));
            _logger.LogDebug("Cached secret: {SecretKey}", secretKey);
        }
        
        return result;
    }
    
    public async Task<IFdwResult> SetSecretAsync(string secretKey, string secretValue, string? container = null, DateTimeOffset? expiresAt = null)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["SecretValue"] = secretValue
        };
        
        if (expiresAt.HasValue)
            parameters["ExpiresAt"] = expiresAt.Value;
        
        var command = new SetSecretCommand(secretKey, parameters, container);
        var result = await _router.ExecuteCommandAsync(command);
        
        if (result.IsSuccess)
        {
            // Invalidate cache
            var cacheKey = $"{container ?? "default"}:{secretKey}";
            await _cache.RemoveAsync(cacheKey);
            _logger.LogInformation("Secret set successfully: {SecretKey}", secretKey);
        }
        
        return result;
    }
    
    public async Task<IFdwResult<IReadOnlyList<ISecretMetadata>>> ListSecretsAsync(string? container = null, string? filter = null)
    {
        var parameters = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(filter))
            parameters["Filter"] = filter;
        
        var command = new ListSecretsCommand(container, parameters);
        var result = await _router.ExecuteCommandAsync<CustomSecretList>(command);
        
        if (result.IsSuccess)
        {
            return FdwResult<IReadOnlyList<ISecretMetadata>>.Success(result.Value.Secrets.ToList());
        }
        
        return FdwResult<IReadOnlyList<ISecretMetadata>>.Failure(result.ErrorMessage, result.Exception);
    }
    
    public async Task<IFdwResult> DeleteSecretAsync(string secretKey, string? container = null)
    {
        var command = new DeleteSecretCommand(secretKey, container);
        var result = await _router.ExecuteCommandAsync(command);
        
        if (result.IsSuccess)
        {
            // Invalidate cache
            var cacheKey = $"{container ?? "default"}:{secretKey}";
            await _cache.RemoveAsync(cacheKey);
            _logger.LogInformation("Secret deleted successfully: {SecretKey}", secretKey);
        }
        
        return result;
    }
}
```

### Provider Routing with Fallback

```csharp
public sealed class SecretProviderRouter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SecretProviderRouter> _logger;
    
    public SecretProviderRouter(IServiceProvider serviceProvider, ILogger<SecretProviderRouter> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task<IFdwResult<TResult>> ExecuteCommandAsync<TResult>(
        ISecretCommand<TResult> command, 
        string? preferredProvider = null,
        bool enableFallback = true)
    {
        // Get candidate providers
        var providers = GetCandidateProviders(command, preferredProvider);
        
        foreach (var providerInfo in providers)
        {
            try
            {
                _logger.LogDebug("Attempting secret operation with provider {Provider}", providerInfo.Name);
                
                // Get provider instance
                var provider = providerInfo.CreateService(_serviceProvider);
                if (!provider.IsAvailable)
                {
                    _logger.LogWarning("Provider {Provider} is not available", providerInfo.Name);
                    continue;
                }
                
                // Validate command against provider
                var validationResult = provider.ValidateCommand(command);
                if (validationResult.IsFailure)
                {
                    _logger.LogWarning("Command validation failed for provider {Provider}: {Error}", 
                        providerInfo.Name, validationResult.ErrorMessage);
                    continue;
                }
                
                // Execute command
                var result = await provider.Execute(command);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Secret operation successful with provider {Provider}", providerInfo.Name);
                    return result;
                }
                
                _logger.LogWarning("Secret operation failed with provider {Provider}: {Error}", 
                    providerInfo.Name, result.ErrorMessage);
                
                if (!enableFallback)
                {
                    return result; // Return failure without trying other providers
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during secret operation with provider {Provider}", providerInfo.Name);
                
                if (!enableFallback)
                {
                    return FdwResult<TResult>.Failure("Secret operation failed", ex);
                }
            }
        }
        
        return FdwResult<TResult>.Failure("No available secret provider could handle the request");
    }
    
    private IEnumerable<SecretProviderBase> GetCandidateProviders(ISecretCommand command, string? preferredProvider)
    {
        var allProviders = SecretProviders.All.AsEnumerable();
        
        // Filter by preferred provider if specified
        if (!string.IsNullOrWhiteSpace(preferredProvider))
        {
            var preferred = allProviders.FirstOrDefault(p => 
                string.Equals(p.ProviderType, preferredProvider, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name, preferredProvider, StringComparison.OrdinalIgnoreCase));
                
            if (preferred != null)
            {
                yield return preferred;
            }
        }
        
        // Filter by command requirements
        var compatibleProviders = allProviders
            .Where(p => p.SupportedCommandTypes.Contains(command.CommandType))
            .Where(p => command.Container == null || p.SupportedContainerTypes.Any(ct => 
                string.Equals(ct, GetContainerType(command.Container), StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(p => p.Id); // Use ID as priority
        
        foreach (var provider in compatibleProviders)
        {
            yield return provider;
        }
    }
    
    private string GetContainerType(string? container)
    {
        // Logic to determine container type from container name
        // This is simplified - real implementation would have more sophisticated logic
        return container?.Contains("vault", StringComparison.OrdinalIgnoreCase) == true ? "Vault" : "Container";
    }
}
```

### Secret Expiration Monitoring

```csharp
public sealed class SecretExpirationMonitor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SecretExpirationMonitor> _logger;
    
    public SecretExpirationMonitor(IServiceProvider serviceProvider, ILogger<SecretExpirationMonitor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiringSecrets();
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Check every hour
        }
    }
    
    private async Task CheckExpiringSecrets()
    {
        foreach (var providerInfo in SecretProviders.All)
        {
            try
            {
                var provider = providerInfo.CreateService(_serviceProvider);
                if (!provider.IsAvailable)
                    continue;
                
                // Get all containers
                var containersResult = await provider.GetContainersAsync();
                if (containersResult.IsFailure)
                    continue;
                
                foreach (var container in containersResult.Value)
                {
                    await CheckContainerSecrets(provider, container.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during expiration check for provider {Provider}", providerInfo.Name);
            }
        }
    }
    
    private async Task CheckContainerSecrets(ISecretProvider provider, string containerName)
    {
        try
        {
            // List secrets in container
            var listCommand = new ListSecretsCommand(containerName);
            var listResult = await provider.Execute<CustomSecretList>(listCommand);
            
            if (listResult.IsFailure)
                return;
            
            var secretList = listResult.Value;
            var expiringSecrets = secretList.Secrets
                .Where(s => s.ExpiresAt.HasValue)
                .Where(s => s.ExpiresAt.Value <= DateTimeOffset.UtcNow.AddDays(7)) // Expiring within 7 days
                .ToList();
            
            foreach (var expiringSecret in expiringSecrets)
            {
                if (expiringSecret.IsExpired)
                {
                    _logger.LogWarning("Secret has expired: {Container}/{SecretKey} expired at {ExpiresAt}", 
                        containerName, expiringSecret.Key, expiringSecret.ExpiresAt);
                    
                    // Optionally disable or delete expired secrets
                    await HandleExpiredSecret(provider, containerName, expiringSecret);
                }
                else
                {
                    var daysUntilExpiration = (expiringSecret.ExpiresAt.Value - DateTimeOffset.UtcNow).Days;
                    _logger.LogInformation("Secret expiring soon: {Container}/{SecretKey} expires in {Days} days", 
                        containerName, expiringSecret.Key, daysUntilExpiration);
                    
                    // Send notification about upcoming expiration
                    await NotifyExpiringSecret(containerName, expiringSecret, daysUntilExpiration);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during secret expiration check for container {Container}", containerName);
        }
    }
    
    private async Task HandleExpiredSecret(ISecretProvider provider, string containerName, ISecretMetadata secret)
    {
        // Implementation would handle expired secrets based on policy
        // Options: disable, delete, rotate, etc.
    }
    
    private async Task NotifyExpiringSecret(string containerName, ISecretMetadata secret, int daysUntilExpiration)
    {
        // Implementation would send notifications (email, Slack, etc.)
        // about secrets that are about to expire
    }
}
```

### Batch Secret Operations

```csharp
public sealed class BatchSecretManager
{
    private readonly ISecretProvider _provider;
    
    public BatchSecretManager(ISecretProvider provider)
    {
        _provider = provider;
    }
    
    public async Task<IFdwResult<ISecretBatchResult>> SetMultipleSecretsAsync(Dictionary<string, string> secrets, string? container = null)
    {
        if (!_provider.SupportsBatchOperations)
        {
            return await SetSecretsSequentially(secrets, container);
        }
        
        var commands = secrets.Select(kvp => new SetSecretCommand(kvp.Key, new Dictionary<string, object?> { ["SecretValue"] = kvp.Value }, container))
            .Cast<ISecretCommand>()
            .ToList();
        
        return await _provider.ExecuteBatch(commands);
    }
    
    public async Task<IFdwResult<Dictionary<string, string>>> GetMultipleSecretsAsync(IReadOnlyList<string> secretKeys, string? container = null)
    {
        var commands = secretKeys.Select(key => new GetSecretCommand(key, container))
            .Cast<ISecretCommand>()
            .ToList();
        
        IFdwResult<ISecretBatchResult> batchResult;
        
        if (_provider.SupportsBatchOperations)
        {
            batchResult = await _provider.ExecuteBatch(commands);
        }
        else
        {
            // Fall back to sequential execution
            var results = new List<ISecretCommandResult>();
            
            foreach (var command in commands)
            {
                var result = await _provider.Execute(command);
                results.Add(new CustomSecretCommandResult(command, results.Count, result.IsSuccess, result.Value, 
                    result.ErrorMessage, result.ErrorDetails, result.Exception, TimeSpan.Zero, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
            }
            
            var customBatchResult = new CustomSecretBatchResult(Guid.NewGuid().ToString(), commands.Count, 
                results.Count(r => r.IsSuccessful), results.Count(r => !r.IsSuccessful), 0,
                TimeSpan.Zero, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, results, Array.Empty<string>());
                
            batchResult = FdwResult<ISecretBatchResult>.Success(customBatchResult);
        }
        
        if (batchResult.IsFailure)
            return FdwResult<Dictionary<string, string>>.Failure(batchResult.ErrorMessage, batchResult.Exception);
        
        var secretValues = new Dictionary<string, string>();
        foreach (var commandResult in batchResult.Value.CommandResults.Where(r => r.IsSuccessful))
        {
            if (commandResult.ResultData is string secretValue)
            {
                var secretKey = ((ISecretCommand)commandResult.Command).SecretKey;
                if (!string.IsNullOrEmpty(secretKey))
                {
                    secretValues[secretKey] = secretValue;
                }
            }
        }
        
        return FdwResult<Dictionary<string, string>>.Success(secretValues);
    }
    
    private async Task<IFdwResult<ISecretBatchResult>> SetSecretsSequentially(Dictionary<string, string> secrets, string? container)
    {
        var results = new List<ISecretCommandResult>();
        var startTime = DateTimeOffset.UtcNow;
        
        foreach (var (key, value) in secrets)
        {
            var command = new SetSecretCommand(key, new Dictionary<string, object?> { ["SecretValue"] = value }, container);
            var commandStartTime = DateTimeOffset.UtcNow;
            
            try
            {
                var result = await _provider.Execute(command);
                var commandEndTime = DateTimeOffset.UtcNow;
                
                results.Add(new CustomSecretCommandResult(command, results.Count, result.IsSuccess, result.Value,
                    result.ErrorMessage, result.ErrorDetails, result.Exception, 
                    commandEndTime - commandStartTime, commandStartTime, commandEndTime));
            }
            catch (Exception ex)
            {
                var commandEndTime = DateTimeOffset.UtcNow;
                results.Add(new CustomSecretCommandResult(command, results.Count, false, null, ex.Message,
                    new[] { ex.ToString() }, ex, commandEndTime - commandStartTime, commandStartTime, commandEndTime));
            }
        }
        
        var endTime = DateTimeOffset.UtcNow;
        var batchResult = new CustomSecretBatchResult(Guid.NewGuid().ToString(), secrets.Count,
            results.Count(r => r.IsSuccessful), results.Count(r => !r.IsSuccessful), 0,
            endTime - startTime, startTime, endTime, results, Array.Empty<string>());
        
        return FdwResult<ISecretBatchResult>.Success(batchResult);
    }
}
```

## Best Practices

1. **Use provider discovery** for automatic registration and management
2. **Implement proper encryption** for sensitive data at rest and in transit
3. **Cache secrets appropriately** while respecting security requirements
4. **Monitor secret expiration** proactively to prevent service disruptions
5. **Use batch operations** when available for better performance
6. **Implement proper access controls** and audit logging
7. **Handle provider fallback** gracefully for high availability
8. **Validate commands thoroughly** before execution
9. **Use versioning** when supported for secret rollback capabilities
10. **Monitor provider health** and performance metrics

## Integration with Other Framework Components

This abstraction layer works seamlessly with other FractalDataWorks packages:

- **Authentication**: Authentication providers use secrets for API keys and certificates
- **ExternalConnections**: Connection strings and credentials are managed as secrets
- **DataProviders**: Database passwords and connection secrets are retrieved securely
- **Transformations**: API keys and service credentials for external transformation services
- **Scheduling**: Scheduled tasks can rotate and update secrets automatically

## License

This package is part of the FractalDataWorks Framework and is licensed under the Apache 2.0 License.