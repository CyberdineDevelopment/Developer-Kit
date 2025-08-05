# FractalDataWorks.Services.Authentication.Abstractions

The **FractalDataWorks.Services.Authentication.Abstractions** package provides comprehensive authentication capabilities for the FractalDataWorks Framework. This package defines interfaces and base classes for integrating with various authentication providers including Azure Entra, Okta, Auth0, LDAP, OAuth2, SAML, and custom authentication systems.

## Overview

This abstraction layer provides:

- **Multi-Provider Authentication** - Support for Azure Entra, Okta, Auth0, LDAP, and custom providers
- **Multiple Authentication Flows** - OAuth2, SAML, OpenID Connect, Basic Auth, JWT, MFA
- **Provider Discovery** - Automatic discovery and registration via EnhancedEnums
- **Session Management** - User session tracking and lifecycle management
- **Token Management** - Access tokens, refresh tokens, and token validation
- **Multi-Factor Authentication** - Built-in MFA support with multiple factors
- **User Principal Management** - Rich user identity and claims handling

## Quick Start

### Using an Existing Authentication Provider

```csharp
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Framework.Abstractions;

// Define a login command
public sealed class LoginCommand : IAuthenticationCommand<IAuthenticationResult>
{
    public string CommandId { get; } = Guid.NewGuid().ToString();
    public string CommandType => "Login";
    public string AuthenticationFlow => "OAuth2";
    public string? Realm { get; }
    public Type ExpectedResultType => typeof(IAuthenticationResult);
    public TimeSpan? Timeout => TimeSpan.FromSeconds(30);
    public bool RequiresSecureTransport => true;
    public bool RequiresAudit => true;
    
    public IReadOnlyDictionary<string, object?> Parameters { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    public LoginCommand(string username, string password, string? realm = null, string? clientId = null)
    {
        Realm = realm;
        
        var parameters = new Dictionary<string, object?>
        {
            ["Username"] = username,
            ["Password"] = password
        };
        
        if (!string.IsNullOrWhiteSpace(clientId))
            parameters["ClientId"] = clientId;
            
        Parameters = parameters;
        
        Metadata = new Dictionary<string, object>
        {
            ["IpAddress"] = GetClientIpAddress(),
            ["UserAgent"] = GetUserAgent(),
            ["Timestamp"] = DateTime.UtcNow
        };
    }
    
    public IFdwResult Validate()
    {
        var errors = new List<string>();
        
        if (!Parameters.ContainsKey("Username") || string.IsNullOrWhiteSpace(Parameters["Username"]?.ToString()))
            errors.Add("Username is required");
            
        if (!Parameters.ContainsKey("Password") || string.IsNullOrWhiteSpace(Parameters["Password"]?.ToString()))
            errors.Add("Password is required");
            
        return errors.Count == 0 
            ? FdwResult.Success()
            : FdwResult.Failure("Command validation failed", errors);
    }
    
    public IAuthenticationCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return new LoginCommand(
            newParameters.GetValueOrDefault("Username")?.ToString() ?? "",
            newParameters.GetValueOrDefault("Password")?.ToString() ?? "",
            Realm,
            newParameters.GetValueOrDefault("ClientId")?.ToString());
    }
    
    public IAuthenticationCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        // Implementation would create new instance with updated metadata
        return this;
    }
    
    public IAuthenticationCommand<IAuthenticationResult> WithParameters(IReadOnlyDictionary<string, object?> newParameters)
    {
        return (IAuthenticationCommand<IAuthenticationResult>)WithParameters(newParameters);
    }
    
    public IAuthenticationCommand<IAuthenticationResult> WithMetadata(IReadOnlyDictionary<string, object> newMetadata)
    {
        return (IAuthenticationCommand<IAuthenticationResult>)WithMetadata(newMetadata);
    }
    
    private string GetClientIpAddress() => "192.168.1.100"; // Implementation would get actual IP
    private string GetUserAgent() => "MyApp/1.0"; // Implementation would get actual user agent
}

// Using the authentication service
public async Task<IFdwResult<IAuthenticationResult>> AuthenticateUserAsync(string username, string password)
{
    // Find an OAuth2 authentication provider
    var provider = AuthenticationProviders.GetByAuthenticationFlow("OAuth2")
        .FirstOrDefault(p => p.SupportedRealms.Contains("default"));
        
    if (provider == null)
        return FdwResult<IAuthenticationResult>.Failure("No OAuth2 provider found");
    
    // Execute the login command
    var command = new LoginCommand(username, password, "default");
    var result = await provider.Execute(command);
    
    if (result.IsSuccess && result.Value is IAuthenticationResult authResult)
    {
        if (authResult.IsSuccess)
        {
            Console.WriteLine($"User authenticated: {authResult.User?.Name}");
            Console.WriteLine($"Session ID: {authResult.SessionId}");
            Console.WriteLine($"Token expires: {authResult.ExpiresAt}");
            
            if (authResult.RequiresAdditionalAuthentication)
            {
                Console.WriteLine($"Additional auth required: {authResult.AdditionalAuthenticationType}");
                // Handle MFA flow
            }
            
            return FdwResult<IAuthenticationResult>.Success(authResult);
        }
        else
        {
            return FdwResult<IAuthenticationResult>.Failure("Authentication failed", authResult.ErrorMessages);
        }
    }
    
    return FdwResult<IAuthenticationResult>.Failure("Authentication request failed", result.Exception);
}
```

### Creating a Custom Authentication Provider

```csharp
// Define configuration for your authentication provider
public sealed class CustomAuthConfiguration : AuthenticationConfiguration
{
    public string AuthServerUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool EnableMfa { get; set; } = false;
    public int TokenExpirationMinutes { get; set; } = 60;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = base.Validate().ToList();
        
        if (string.IsNullOrWhiteSpace(AuthServerUrl))
            errors.Add("Auth server URL is required");
            
        if (!Uri.TryCreate(AuthServerUrl, UriKind.Absolute, out _))
            errors.Add("Auth server URL must be a valid absolute URI");
            
        if (string.IsNullOrWhiteSpace(ClientId))
            errors.Add("Client ID is required");
            
        if (string.IsNullOrWhiteSpace(ClientSecret))
            errors.Add("Client secret is required");
            
        if (TokenExpirationMinutes <= 0)
            errors.Add("Token expiration must be positive");
            
        return errors;
    }
}

// Implement the authentication provider
public sealed class CustomAuthProvider : AuthenticationProviderBase
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CustomAuthProvider> _logger;
    
    public CustomAuthProvider(
        HttpClient httpClient, 
        IMemoryCache cache, 
        ILogger<CustomAuthProvider> logger,
        CustomAuthConfiguration configuration) : base(
        id: 1,
        name: "Custom Authentication Provider",
        providerType: "CustomAuth",
        version: "1.0.0",
        supportedAuthenticationFlows: new[] { "OAuth2", "BasicAuth" },
        supportedCommandTypes: new[] { "Login", "Logout", "ValidateToken", "RefreshToken", "GetUserInfo" },
        supportedRealms: new[] { "default", "admin", "api" },
        configuration: configuration,
        supportsMfa: configuration.EnableMfa,
        supportsTokenRefresh: true,
        supportsUserInfo: true,
        supportsPasswordOperations: true,
        supportsSessionManagement: true)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(configuration.AuthServerUrl);
        _httpClient.DefaultRequestHeaders.Add("Client-Id", configuration.ClientId);
    }
    
    public override async Task<IFdwResult<object?>> Execute(IAuthenticationCommand command, CancellationToken cancellationToken = default)
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
                "Login" => await ExecuteLogin(command, cancellationToken),
                "Logout" => await ExecuteLogout(command, cancellationToken),
                "ValidateToken" => await ExecuteValidateToken(command, cancellationToken),
                "RefreshToken" => await ExecuteRefreshToken(command, cancellationToken),
                "GetUserInfo" => await ExecuteGetUserInfo(command, cancellationToken),
                _ => FdwResult<object?>.Failure($"Unsupported command type: {command.CommandType}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication command execution failed: {CommandType}", command.CommandType);
            return FdwResult<object?>.Failure($"Command execution failed: {ex.Message}", ex);
        }
    }
    
    public override async Task<IFdwResult<TResult>> Execute<TResult>(IAuthenticationCommand<TResult> command, CancellationToken cancellationToken = default)
    {
        var result = await Execute((IAuthenticationCommand)command, cancellationToken);
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
    
    private async Task<IFdwResult<object?>> ExecuteLogin(IAuthenticationCommand command, CancellationToken cancellationToken)
    {
        var username = command.Parameters.GetValueOrDefault("Username")?.ToString();
        var password = command.Parameters.GetValueOrDefault("Password")?.ToString();
        
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return FdwResult<object?>.Failure("Username and password are required");
        
        try
        {
            // Create login request
            var loginRequest = new
            {
                username,
                password,
                client_id = ((CustomAuthConfiguration)Configuration).ClientId,
                client_secret = ((CustomAuthConfiguration)Configuration).ClientSecret,
                realm = command.Realm ?? "default"
            };
            
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Send login request
            var response = await _httpClient.PostAsync("/auth/login", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);
                
                if (loginResponse != null)
                {
                    // Create authentication result
                    var authResult = CreateAuthenticationResult(loginResponse, command);
                    
                    // Cache session if successful
                    if (authResult.IsSuccess && authResult.SessionId != null)
                    {
                        CacheSession(authResult.SessionId, authResult);
                    }
                    
                    return FdwResult<object?>.Success(authResult);
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return FdwResult<object?>.Success(CreateFailedAuthResult("Invalid username or password"));
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return FdwResult<object?>.Failure($"Authentication failed: {response.StatusCode} - {errorContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            return FdwResult<object?>.Failure($"Authentication request failed: {ex.Message}", ex);
        }
        
        return FdwResult<object?>.Failure("Authentication failed");
    }
    
    private async Task<IFdwResult<object?>> ExecuteLogout(IAuthenticationCommand command, CancellationToken cancellationToken)
    {
        var sessionId = command.Parameters.GetValueOrDefault("SessionId")?.ToString();
        if (string.IsNullOrWhiteSpace(sessionId))
            return FdwResult<object?>.Failure("Session ID is required for logout");
        
        try
        {
            // Remove from cache
            _cache.Remove($"session_{sessionId}");
            
            // Notify auth server
            var logoutRequest = new { session_id = sessionId };
            var json = JsonSerializer.Serialize(logoutRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/auth/logout", content, cancellationToken);
            
            var result = new LogoutResult
            {
                SessionId = sessionId,
                LoggedOutAt = DateTimeOffset.UtcNow,
                IsSuccess = response.IsSuccessStatusCode
            };
            
            return FdwResult<object?>.Success(result);
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"Logout failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<object?>> ExecuteValidateToken(IAuthenticationCommand command, CancellationToken cancellationToken)
    {
        var token = command.Parameters.GetValueOrDefault("Token")?.ToString();
        if (string.IsNullOrWhiteSpace(token))
            return FdwResult<object?>.Failure("Token is required for validation");
        
        try
        {
            // Check cache first
            if (_cache.TryGetValue($"token_{token}", out var cachedValidation))
            {
                return FdwResult<object?>.Success(cachedValidation);
            }
            
            // Validate with auth server
            var validateRequest = new { token };
            var json = JsonSerializer.Serialize(validateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/auth/validate", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var validationResponse = JsonSerializer.Deserialize<TokenValidationResponse>(responseContent);
                
                if (validationResponse != null)
                {
                    // Cache validation result
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    };
                    _cache.Set($"token_{token}", validationResponse, cacheOptions);
                    
                    return FdwResult<object?>.Success(validationResponse);
                }
            }
            
            return FdwResult<object?>.Failure("Token validation failed");
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"Token validation failed: {ex.Message}", ex);
        }
    }
    
    private IAuthenticationResult CreateAuthenticationResult(LoginResponse loginResponse, IAuthenticationCommand command)
    {
        var user = new CustomUserPrincipal
        {
            Id = loginResponse.UserId,
            Username = loginResponse.Username,
            Name = loginResponse.DisplayName,
            Email = loginResponse.Email,
            Roles = loginResponse.Roles
        };
        
        var token = new CustomAuthenticationToken
        {
            Value = loginResponse.AccessToken,
            TokenType = "Bearer",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(((CustomAuthConfiguration)Configuration).TokenExpirationMinutes)
        };
        
        var refreshToken = !string.IsNullOrWhiteSpace(loginResponse.RefreshToken) 
            ? new CustomAuthenticationToken
            {
                Value = loginResponse.RefreshToken,
                TokenType = "Refresh",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
            }
            : null;
        
        return new CustomAuthenticationResult
        {
            IsSuccess = true,
            User = user,
            Token = token,
            RefreshToken = refreshToken,
            SessionId = loginResponse.SessionId,
            AuthenticatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = token.ExpiresAt,
            AuthenticationMethod = command.AuthenticationFlow,
            ProviderId = ProviderId,
            Realm = command.Realm,
            GrantedScopes = loginResponse.Scopes?.ToList() ?? new List<string>(),
            Roles = loginResponse.Roles?.ToList() ?? new List<string>(),
            Claims = CreateClaimsFromUser(user),
            Strength = DetermineAuthenticationStrength(loginResponse)
        };
    }
    
    private IAuthenticationResult CreateFailedAuthResult(string errorMessage)
    {
        return new CustomAuthenticationResult
        {
            IsSuccess = false,
            ErrorMessages = new[] { errorMessage },
            AuthenticatedAt = DateTimeOffset.UtcNow,
            AuthenticationMethod = "OAuth2",
            ProviderId = ProviderId
        };
    }
    
    private void CacheSession(string sessionId, IAuthenticationResult authResult)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = authResult.ExpiresAt,
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
        
        _cache.Set($"session_{sessionId}", authResult, cacheOptions);
    }
    
    private List<Claim> CreateClaimsFromUser(IUserPrincipal user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.Name),
            new(ClaimTypes.Email, user.Email)
        };
        
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        return claims;
    }
    
    private AuthenticationStrength DetermineAuthenticationStrength(LoginResponse loginResponse)
    {
        if (loginResponse.MfaCompleted)
            return AuthenticationStrength.High;
        
        if (loginResponse.HasStrongPassword)
            return AuthenticationStrength.Medium;
            
        return AuthenticationStrength.Low;
    }
    
    public override async Task<IFdwResult<IAuthenticationProviderHealth>> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            
            var health = new CustomAuthProviderHealth
            {
                IsHealthy = response.IsSuccessStatusCode,
                LastChecked = DateTimeOffset.UtcNow,
                ResponseTime = TimeSpan.FromMilliseconds(100), // Would measure actual response time
                EndpointUrl = _httpClient.BaseAddress?.ToString() ?? "",
                StatusCode = (int)response.StatusCode
            };
            
            return FdwResult<IAuthenticationProviderHealth>.Success(health);
        }
        catch (Exception ex)
        {
            var health = new CustomAuthProviderHealth
            {
                IsHealthy = false,
                LastChecked = DateTimeOffset.UtcNow,
                ErrorMessage = ex.Message,
                EndpointUrl = _httpClient.BaseAddress?.ToString() ?? ""
            };
            
            return FdwResult<IAuthenticationProviderHealth>.Success(health);
        }
    }
    
    public override async Task<IFdwResult<IReadOnlyCollection<IAuthenticationRealm>>> GetRealmsAsync(CancellationToken cancellationToken = default)
    {
        var realms = SupportedRealms.Select(realm => new CustomAuthRealm
        {
            Id = realm,
            Name = realm,
            Description = $"Authentication realm: {realm}",
            IsEnabled = true
        }).Cast<IAuthenticationRealm>().ToList();
        
        return FdwResult<IReadOnlyCollection<IAuthenticationRealm>>.Success(realms);
    }
    
    public override async Task<IFdwResult<IAuthenticationProviderMetrics>> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var metrics = new CustomAuthProviderMetrics
        {
            TotalAuthenticationAttempts = 1000, // Would track actual metrics
            SuccessfulAuthentications = 950,
            FailedAuthentications = 50,
            AverageResponseTime = TimeSpan.FromMilliseconds(150),
            ActiveSessions = GetActiveSessions().Count,
            TokensIssued = 2000,
            TokensValidated = 5000
        };
        
        return FdwResult<IAuthenticationProviderMetrics>.Success(metrics);
    }
    
    public override async Task<IFdwResult<IReadOnlyCollection<IAuthenticationSession>>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        var sessions = GetActiveSessions();
        return FdwResult<IReadOnlyCollection<IAuthenticationSession>>.Success(sessions);
    }
    
    public override async Task<IFdwResult<IReadOnlyCollection<IAuthenticationSession>>> GetUserSessionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var sessions = GetActiveSessions().Where(s => s.UserId == userId).ToList();
        return FdwResult<IReadOnlyCollection<IAuthenticationSession>>.Success(sessions);
    }
    
    public override async Task<IFdwResult> RevokeSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Remove($"session_{sessionId}");
            
            // Notify auth server
            var revokeRequest = new { session_id = sessionId };
            var json = JsonSerializer.Serialize(revokeRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/auth/revoke-session", content, cancellationToken);
            
            return response.IsSuccessStatusCode 
                ? FdwResult.Success()
                : FdwResult.Failure($"Session revocation failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return FdwResult.Failure($"Session revocation failed: {ex.Message}", ex);
        }
    }
    
    public override async Task<IFdwResult> RevokeUserSessionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Remove user sessions from cache
            var userSessions = GetActiveSessions().Where(s => s.UserId == userId).ToList();
            foreach (var session in userSessions)
            {
                _cache.Remove($"session_{session.SessionId}");
            }
            
            // Notify auth server
            var revokeRequest = new { user_id = userId };
            var json = JsonSerializer.Serialize(revokeRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/auth/revoke-user-sessions", content, cancellationToken);
            
            return response.IsSuccessStatusCode 
                ? FdwResult.Success()
                : FdwResult.Failure($"User session revocation failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return FdwResult.Failure($"User session revocation failed: {ex.Message}", ex);
        }
    }
    
    private List<IAuthenticationSession> GetActiveSessions()
    {
        // In a real implementation, this would retrieve sessions from cache/storage
        return new List<IAuthenticationSession>();
    }
    
    protected override IAuthenticationCommandResult CreateCommandResult(IAuthenticationCommand command, int batchPosition, 
        bool isSuccessful, object? resultData, string? errorMessage, IReadOnlyList<string>? errorDetails, 
        Exception? exception, TimeSpan executionTime, DateTimeOffset startedAt, DateTimeOffset completedAt)
    {
        return new CustomAuthCommandResult(command, batchPosition, isSuccessful, resultData, errorMessage, 
            errorDetails, exception, executionTime, startedAt, completedAt);
    }
    
    protected override IAuthenticationBatchResult CreateBatchResult(string batchId, int totalCommands, 
        int successfulCommands, int failedCommands, int skippedCommands, TimeSpan executionTime, 
        DateTimeOffset startedAt, DateTimeOffset completedAt, IReadOnlyList<IAuthenticationCommandResult> commandResults, 
        IReadOnlyList<string> batchErrors)
    {
        return new CustomAuthBatchResult(batchId, totalCommands, successfulCommands, failedCommands, 
            skippedCommands, executionTime, startedAt, completedAt, commandResults, batchErrors);
    }
    
    public override void RegisterService(IServiceCollection services)
    {
        services.AddSingleton<CustomAuthProvider>();
        services.AddSingleton<IAuthenticationProvider>(sp => sp.GetRequiredService<CustomAuthProvider>());
        services.AddHttpClient<CustomAuthProvider>();
        services.AddMemoryCache();
    }
}

// Supporting classes for the custom provider
public class LoginResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string[]? Roles { get; set; }
    public string[]? Scopes { get; set; }
    public bool MfaCompleted { get; set; }
    public bool HasStrongPassword { get; set; }
}

public class TokenValidationResponse
{
    public bool IsValid { get; set; }
    public string? UserId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string[]? Scopes { get; set; }
}

public class LogoutResult
{
    public string SessionId { get; set; } = string.Empty;
    public DateTimeOffset LoggedOutAt { get; set; }
    public bool IsSuccess { get; set; }
}
```

## Implementation Examples

### Azure Entra Provider

```csharp
public sealed class AzureEntraProvider : AuthenticationProviderBase
{
    public static readonly AzureEntraProvider Instance = new();
    
    private AzureEntraProvider() : base(
        id: 2,
        name: "Azure Entra Authentication Provider",
        providerType: "AzureEntra",
        version: "1.0.0",
        supportedAuthenticationFlows: new[] { "OAuth2", "OpenIDConnect", "SAML" },
        supportedCommandTypes: new[] { "Login", "Logout", "ValidateToken", "RefreshToken", "GetUserInfo" },
        supportedRealms: new[] { "organizations", "common", "consumers" },
        configuration: CreateDefaultConfiguration(),
        supportsMfa: true,
        supportsTokenRefresh: true,
        supportsUserInfo: true,
        supportsPasswordOperations: false,
        supportsSessionManagement: true)
    {
    }
    
    private static AuthenticationConfiguration CreateDefaultConfiguration()
    {
        return new AzureEntraConfiguration
        {
            ProviderId = "azure-entra",
            TenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? "",
            ClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? "",
            ClientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ?? "",
            RedirectUri = "https://localhost:5001/auth/callback"
        };
    }
    
    public override async Task<IFdwResult<object?>> Execute(IAuthenticationCommand command, CancellationToken cancellationToken = default)
    {
        // Azure Entra-specific implementation using Microsoft.Graph SDK
        return command.CommandType switch
        {
            "Login" => await ExecuteAzureLogin(command, cancellationToken),
            "ValidateToken" => await ValidateAzureToken(command, cancellationToken),
            _ => FdwResult<object?>.Failure($"Unsupported command: {command.CommandType}")
        };
    }
    
    private async Task<IFdwResult<object?>> ExecuteAzureLogin(IAuthenticationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Use Microsoft Authentication Library (MSAL)
            var app = ConfidentialClientApplicationBuilder
                .Create(((AzureEntraConfiguration)Configuration).ClientId)
                .WithClientSecret(((AzureEntraConfiguration)Configuration).ClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{((AzureEntraConfiguration)Configuration).TenantId}"))
                .Build();
            
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync(cancellationToken);
            
            // Create authentication result
            var authResult = new AzureEntraAuthenticationResult
            {
                IsSuccess = true,
                Token = new AzureAuthenticationToken { Value = result.AccessToken, ExpiresAt = result.ExpiresOn },
                AuthenticatedAt = DateTimeOffset.UtcNow,
                ProviderId = ProviderId,
                Strength = AuthenticationStrength.High
            };
            
            return FdwResult<object?>.Success(authResult);
        }
        catch (MsalException ex)
        {
            return FdwResult<object?>.Failure($"Azure authentication failed: {ex.Message}", ex);
        }
    }
    
    // Additional Azure Entra implementation methods...
}

public sealed class AzureEntraConfiguration : AuthenticationConfiguration
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = base.Validate().ToList();
        
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

### Okta Provider

```csharp
public sealed class OktaProvider : AuthenticationProviderBase
{
    public static readonly OktaProvider Instance = new();
    
    private OktaProvider() : base(
        id: 3,
        name: "Okta Authentication Provider",
        providerType: "Okta",
        version: "1.0.0",
        supportedAuthenticationFlows: new[] { "OAuth2", "SAML", "OIDC" },
        supportedCommandTypes: new[] { "Login", "Logout", "ValidateToken", "RefreshToken", "GetUserInfo", "EnableMFA" },
        supportedRealms: new[] { "default" },
        configuration: CreateDefaultConfiguration(),
        supportsMfa: true,
        supportsTokenRefresh: true,
        supportsUserInfo: true,
        supportsPasswordOperations: true,
        supportsSessionManagement: true)
    {
    }
    
    private static AuthenticationConfiguration CreateDefaultConfiguration()
    {
        return new OktaConfiguration
        {
            ProviderId = "okta",
            Domain = Environment.GetEnvironmentVariable("OKTA_DOMAIN") ?? "",
            ClientId = Environment.GetEnvironmentVariable("OKTA_CLIENT_ID") ?? "",
            ClientSecret = Environment.GetEnvironmentVariable("OKTA_CLIENT_SECRET") ?? "",
            AuthorizationServerId = "default"
        };
    }
    
    public override async Task<IFdwResult<object?>> Execute(IAuthenticationCommand command, CancellationToken cancellationToken = default)
    {
        // Okta-specific implementation using Okta SDK
        return command.CommandType switch
        {
            "Login" => await ExecuteOktaLogin(command, cancellationToken),
            "EnableMFA" => await EnableOktaMfa(command, cancellationToken),
            _ => FdwResult<object?>.Failure($"Unsupported command: {command.CommandType}")
        };
    }
    
    private async Task<IFdwResult<object?>> ExecuteOktaLogin(IAuthenticationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var oktaConfig = (OktaConfiguration)Configuration;
            var client = new AuthenticationClient(new OktaClientConfiguration
            {
                OktaDomain = oktaConfig.Domain,
                ClientId = oktaConfig.ClientId,
                ClientSecret = oktaConfig.ClientSecret
            });
            
            var username = command.Parameters.GetValueOrDefault("Username")?.ToString();
            var password = command.Parameters.GetValueOrDefault("Password")?.ToString();
            
            var authRequest = new AuthenticateRequest
            {
                Username = username,
                Password = password
            };
            
            var authResponse = await client.AuthenticateAsync(authRequest, cancellationToken);
            
            var authResult = new OktaAuthenticationResult
            {
                IsSuccess = authResponse.AuthenticationStatus == AuthenticationStatus.Success,
                RequiresAdditionalAuthentication = authResponse.AuthenticationStatus == AuthenticationStatus.MfaRequired,
                AdditionalAuthenticationType = authResponse.AuthenticationStatus == AuthenticationStatus.MfaRequired ? "MFA" : null,
                SessionId = authResponse.SessionToken,
                AuthenticatedAt = DateTimeOffset.UtcNow,
                ProviderId = ProviderId,
                Strength = authResponse.AuthenticationStatus == AuthenticationStatus.MfaRequired 
                    ? AuthenticationStrength.High 
                    : AuthenticationStrength.Medium
            };
            
            return FdwResult<object?>.Success(authResult);
        }
        catch (OktaApiException ex)
        {
            return FdwResult<object?>.Failure($"Okta authentication failed: {ex.Message}", ex);
        }
    }
    
    // Additional Okta implementation methods...
}

public sealed class OktaConfiguration : AuthenticationConfiguration
{
    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthorizationServerId { get; set; } = "default";
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = base.Validate().ToList();
        
        if (string.IsNullOrWhiteSpace(Domain))
            errors.Add("Okta domain is required");
            
        if (string.IsNullOrWhiteSpace(ClientId))
            errors.Add("Okta client ID is required");
            
        return errors;
    }
}
```

### JWT Provider

```csharp
public sealed class JwtProvider : AuthenticationProviderBase
{
    public static readonly JwtProvider Instance = new();
    
    private JwtProvider() : base(
        id: 4,
        name: "JWT Authentication Provider",
        providerType: "JWT",
        version: "1.0.0",
        supportedAuthenticationFlows: new[] { "JWT", "Bearer" },
        supportedCommandTypes: new[] { "ValidateToken", "RefreshToken", "CreateToken" },
        supportedRealms: new[] { "api", "web", "mobile" },
        configuration: CreateDefaultConfiguration(),
        supportsMfa: false,
        supportsTokenRefresh: true,
        supportsUserInfo: true,
        supportsPasswordOperations: false,
        supportsSessionManagement: false)
    {
    }
    
    private static AuthenticationConfiguration CreateDefaultConfiguration()
    {
        return new JwtConfiguration
        {
            ProviderId = "jwt",
            SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? GenerateSecretKey(),
            Issuer = "FractalDataWorks",
            Audience = "api.fractaldataworks.com",
            ExpirationMinutes = 60
        };
    }
    
    public override async Task<IFdwResult<object?>> Execute(IAuthenticationCommand command, CancellationToken cancellationToken = default)
    {
        return command.CommandType switch
        {
            "ValidateToken" => await ValidateJwtToken(command, cancellationToken),
            "CreateToken" => await CreateJwtToken(command, cancellationToken),
            "RefreshToken" => await RefreshJwtToken(command, cancellationToken),
            _ => FdwResult<object?>.Failure($"Unsupported command: {command.CommandType}")
        };
    }
    
    private async Task<IFdwResult<object?>> ValidateJwtToken(IAuthenticationCommand command, CancellationToken cancellationToken)
    {
        var token = command.Parameters.GetValueOrDefault("Token")?.ToString();
        if (string.IsNullOrWhiteSpace(token))
            return FdwResult<object?>.Failure("Token is required");
        
        try
        {
            var jwtConfig = (JwtConfiguration)Configuration;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtConfig.SecretKey);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            var validation = new JwtValidationResult
            {
                IsValid = true,
                UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = principal.FindFirst(ClaimTypes.Name)?.Value,
                ExpiresAt = validatedToken.ValidTo,
                Claims = principal.Claims.ToList()
            };
            
            return FdwResult<object?>.Success(validation);
        }
        catch (SecurityTokenException ex)
        {
            return FdwResult<object?>.Success(new JwtValidationResult 
            { 
                IsValid = false, 
                ErrorMessage = ex.Message 
            });
        }
    }
    
    private async Task<IFdwResult<object?>> CreateJwtToken(IAuthenticationCommand command, CancellationToken cancellationToken)
    {
        var userId = command.Parameters.GetValueOrDefault("UserId")?.ToString();
        var username = command.Parameters.GetValueOrDefault("Username")?.ToString();
        
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(username))
            return FdwResult<object?>.Failure("UserId and Username are required");
        
        try
        {
            var jwtConfig = (JwtConfiguration)Configuration;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtConfig.SecretKey);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, username),
                new("realm", command.Realm ?? "api")
            };
            
            // Add additional claims from parameters
            foreach (var param in command.Parameters.Where(p => p.Key.StartsWith("Claim:")))
            {
                var claimType = param.Key[6..]; // Remove "Claim:" prefix
                if (param.Value != null)
                {
                    claims.Add(new Claim(claimType, param.Value.ToString()));
                }
            }
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(jwtConfig.ExpirationMinutes),
                Issuer = jwtConfig.Issuer,
                Audience = jwtConfig.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            var result = new JwtCreationResult
            {
                Token = tokenString,
                ExpiresAt = tokenDescriptor.Expires.Value,
                TokenType = "Bearer"
            };
            
            return FdwResult<object?>.Success(result);
        }
        catch (Exception ex)
        {
            return FdwResult<object?>.Failure($"Token creation failed: {ex.Message}", ex);
        }
    }
    
    private static string GenerateSecretKey()
    {
        var key = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return Convert.ToBase64String(key);
    }
    
    // Additional JWT implementation methods...
}

public sealed class JwtConfiguration : AuthenticationConfiguration
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = base.Validate().ToList();
        
        if (string.IsNullOrWhiteSpace(SecretKey))
            errors.Add("JWT secret key is required");
            
        if (string.IsNullOrWhiteSpace(Issuer))
            errors.Add("JWT issuer is required");
            
        if (string.IsNullOrWhiteSpace(Audience))
            errors.Add("JWT audience is required");
            
        if (ExpirationMinutes <= 0)
            errors.Add("JWT expiration must be positive");
            
        return errors;
    }
}
```

## Configuration Examples

### JSON Configuration for Multiple Providers

```json
{
  "Authentication": {
    "Providers": {
      "AzureEntra": {
        "ProviderId": "azure-entra",
        "TenantId": "your-tenant-id",
        "ClientId": "your-client-id",
        "ClientSecret": "your-client-secret",
        "RedirectUri": "https://yourdomain.com/auth/callback",
        "Enabled": true
      },
      "Okta": {
        "ProviderId": "okta",
        "Domain": "your-domain.okta.com",
        "ClientId": "your-okta-client-id",
        "ClientSecret": "your-okta-client-secret",
        "AuthorizationServerId": "default",
        "Enabled": true
      },
      "JWT": {
        "ProviderId": "jwt",
        "SecretKey": "your-jwt-secret-key",
        "Issuer": "YourApp",
        "Audience": "api.yourapp.com",
        "ExpirationMinutes": 60,
        "Enabled": true
      },
      "CustomAuth": {
        "ProviderId": "custom-auth",
        "AuthServerUrl": "https://auth.yourcompany.com",
        "ClientId": "your-custom-client-id",
        "ClientSecret": "your-custom-client-secret",
        "EnableMfa": true,
        "TokenExpirationMinutes": 60,
        "Enabled": true
      }
    },
    "DefaultProvider": "AzureEntra",
    "EnableMultiProvider": true,
    "SessionTimeout": "01:00:00",
    "RequireHttps": true
  }
}
```

### Dependency Injection Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure authentication provider settings
    services.Configure<AzureEntraConfiguration>(
        Configuration.GetSection("Authentication:Providers:AzureEntra"));
    services.Configure<OktaConfiguration>(
        Configuration.GetSection("Authentication:Providers:Okta"));
    services.Configure<JwtConfiguration>(
        Configuration.GetSection("Authentication:Providers:JWT"));
    services.Configure<CustomAuthConfiguration>(
        Configuration.GetSection("Authentication:Providers:CustomAuth"));
    
    // Register HTTP clients for providers that need them
    services.AddHttpClient<CustomAuthProvider>();
    services.AddHttpClient<OktaProvider>();
    
    // Register authentication providers (auto-discovered via AuthenticationProviders collection)
    services.AddAuthenticationProviders();
    
    // Register authentication-related services
    services.AddScoped<IAuthenticationService, AuthenticationService>();
    services.AddSingleton<IAuthenticationProviderRouter, AuthenticationProviderRouter>();
    
    // Add authentication middleware
    services.Configure<CookiePolicyOptions>(options =>
    {
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });
    
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
        });
}

// Extension method for bulk registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticationProviders(this IServiceCollection services)
    {
        // Register all discovered authentication providers
        foreach (var provider in AuthenticationProviders.All)
        {
            provider.RegisterService(services);
        }
        
        return services;
    }
}
```

## Advanced Usage

### Multi-Factor Authentication Flow

```csharp
public sealed class MfaAuthenticationService
{
    private readonly IAuthenticationProvider _provider;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    
    public MfaAuthenticationService(
        IAuthenticationProvider provider,
        ISmsService smsService,
        IEmailService emailService)
    {
        _provider = provider;
        _smsService = smsService;
        _emailService = emailService;
    }
    
    public async Task<IFdwResult<IAuthenticationResult>> AuthenticateWithMfaAsync(string username, string password)
    {
        // Step 1: Initial authentication
        var loginCommand = new LoginCommand(username, password);
        var loginResult = await _provider.Execute(loginCommand);
        
        if (loginResult.IsFailure)
            return FdwResult<IAuthenticationResult>.Failure("Login failed", loginResult.Exception);
        
        var authResult = (IAuthenticationResult)loginResult.Value!;
        
        if (!authResult.RequiresAdditionalAuthentication)
            return FdwResult<IAuthenticationResult>.Success(authResult);
        
        // Step 2: Handle MFA requirement
        switch (authResult.AdditionalAuthenticationType)
        {
            case "SMS":
                return await HandleSmsMfa(authResult);
            case "Email":
                return await HandleEmailMfa(authResult);
            case "TOTP":
                return await HandleTotpMfa(authResult);
            default:
                return FdwResult<IAuthenticationResult>.Failure($"Unsupported MFA type: {authResult.AdditionalAuthenticationType}");
        }
    }
    
    private async Task<IFdwResult<IAuthenticationResult>> HandleSmsMfa(IAuthenticationResult authResult)
    {
        try
        {
            // Generate and send SMS code
            var mfaCode = GenerateMfaCode();
            var phoneNumber = authResult.User?.PhoneNumber;
            
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return FdwResult<IAuthenticationResult>.Failure("Phone number not available for SMS MFA");
            
            await _smsService.SendAsync(phoneNumber, $"Your verification code is: {mfaCode}");
            
            // Create MFA challenge result
            var mfaResult = new MfaAuthenticationResult
            {
                IsSuccess = false,
                RequiresAdditionalAuthentication = true,
                AdditionalAuthenticationType = "SMS_CODE",
                SessionId = authResult.SessionId,
                MfaChallenge = new MfaChallenge
                {
                    ChallengeId = Guid.NewGuid().ToString(),
                    ChallengeType = "SMS",
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                    Metadata = new Dictionary<string, object>
                    {
                        ["MaskedPhoneNumber"] = MaskPhoneNumber(phoneNumber),
                        ["CodeLength"] = mfaCode.Length
                    }
                }
            };
            
            // Store challenge for validation
            await StoreMfaChallenge(mfaResult.MfaChallenge.ChallengeId, mfaCode, authResult.SessionId);
            
            return FdwResult<IAuthenticationResult>.Success(mfaResult);
        }
        catch (Exception ex)
        {
            return FdwResult<IAuthenticationResult>.Failure("SMS MFA setup failed", ex);
        }
    }
    
    public async Task<IFdwResult<IAuthenticationResult>> CompleteMfaAsync(string challengeId, string code)
    {
        try
        {
            // Validate MFA code
            var storedChallenge = await GetMfaChallenge(challengeId);
            if (storedChallenge == null)
                return FdwResult<IAuthenticationResult>.Failure("Invalid or expired MFA challenge");
            
            if (storedChallenge.Code != code)
                return FdwResult<IAuthenticationResult>.Failure("Invalid MFA code");
            
            if (storedChallenge.ExpiresAt < DateTimeOffset.UtcNow)
                return FdwResult<IAuthenticationResult>.Failure("MFA code has expired");
            
            // Complete authentication
            var completeMfaCommand = new CompleteMfaCommand(challengeId, code, storedChallenge.SessionId);
            var result = await _provider.Execute(completeMfaCommand);
            
            if (result.IsSuccess && result.Value is IAuthenticationResult authResult)
            {
                // Clean up challenge
                await RemoveMfaChallenge(challengeId);
                return FdwResult<IAuthenticationResult>.Success(authResult);
            }
            
            return FdwResult<IAuthenticationResult>.Failure("MFA completion failed");
        }
        catch (Exception ex)
        {
            return FdwResult<IAuthenticationResult>.Failure("MFA completion failed", ex);
        }
    }
    
    private string GenerateMfaCode() => Random.Shared.Next(100000, 999999).ToString();
    private string MaskPhoneNumber(string phoneNumber) => $"***-***-{phoneNumber[^4..]}";
    
    private async Task StoreMfaChallenge(string challengeId, string code, string? sessionId)
    {
        // Store in cache or database
    }
    
    private async Task<MfaChallengeInfo?> GetMfaChallenge(string challengeId)
    {
        // Retrieve from cache or database
        return null;
    }
    
    private async Task RemoveMfaChallenge(string challengeId)
    {
        // Remove from cache or database
    }
}

public class MfaChallenge
{
    public string ChallengeId { get; set; } = string.Empty;
    public string ChallengeType { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class MfaChallengeInfo
{
    public string Code { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
```

### Provider Routing and Fallback

```csharp
public sealed class AuthenticationProviderRouter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthenticationProviderRouter> _logger;
    
    public AuthenticationProviderRouter(IServiceProvider serviceProvider, ILogger<AuthenticationProviderRouter> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task<IFdwResult<IAuthenticationResult>> AuthenticateAsync(
        IAuthenticationCommand command, 
        string? preferredProvider = null,
        bool enableFallback = true)
    {
        // Get candidate providers
        var providers = GetCandidateProviders(command, preferredProvider);
        
        foreach (var providerInfo in providers)
        {
            try
            {
                _logger.LogDebug("Attempting authentication with provider {Provider}", providerInfo.Name);
                
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
                
                // Execute authentication
                var result = await provider.Execute<IAuthenticationResult>(command);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Authentication successful with provider {Provider}", providerInfo.Name);
                    return result;
                }
                
                _logger.LogWarning("Authentication failed with provider {Provider}: {Error}", 
                    providerInfo.Name, result.ErrorMessage);
                
                if (!enableFallback)
                {
                    return result; // Return failure without trying other providers
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during authentication with provider {Provider}", providerInfo.Name);
                
                if (!enableFallback)
                {
                    return FdwResult<IAuthenticationResult>.Failure("Authentication failed", ex);
                }
            }
        }
        
        return FdwResult<IAuthenticationResult>.Failure("No available authentication provider could handle the request");
    }
    
    private IEnumerable<AuthenticationProviderBase> GetCandidateProviders(IAuthenticationCommand command, string? preferredProvider)
    {
        var allProviders = AuthenticationProviders.All.AsEnumerable();
        
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
            .Where(p => p.SupportedAuthenticationFlows.Contains(command.AuthenticationFlow))
            .Where(p => command.Realm == null || p.SupportedRealms.Contains(command.Realm))
            .OrderByDescending(p => p.Id); // Use ID as priority
        
        foreach (var provider in compatibleProviders)
        {
            yield return provider;
        }
    }
}
```

### Session Management

```csharp
public sealed class AuthenticationSessionManager
{
    private readonly IMemoryCache _cache;
    private readonly IAuthenticationProvider _provider;
    private readonly ILogger<AuthenticationSessionManager> _logger;
    
    public AuthenticationSessionManager(
        IMemoryCache cache,
        IAuthenticationProvider provider,
        ILogger<AuthenticationSessionManager> logger)
    {
        _cache = cache;
        _provider = provider;
        _logger = logger;
    }
    
    public async Task<IFdwResult<IAuthenticationSession>> CreateSessionAsync(IAuthenticationResult authResult)
    {
        if (!authResult.IsSuccess || authResult.User == null)
            return FdwResult<IAuthenticationSession>.Failure("Cannot create session from failed authentication");
        
        try
        {
            var session = new AuthenticationSession
            {
                SessionId = authResult.SessionId ?? Guid.NewGuid().ToString(),
                UserId = authResult.User.Id,
                Username = authResult.User.Username,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = authResult.ExpiresAt ?? DateTimeOffset.UtcNow.AddHours(1),
                LastActivityAt = DateTimeOffset.UtcNow,
                IsActive = true,
                AuthenticationMethod = authResult.AuthenticationMethod,
                ProviderId = authResult.ProviderId,
                Realm = authResult.Realm,
                Roles = authResult.Roles.ToList(),
                Claims = authResult.Claims.ToDictionary(c => c.Type, c => c.Value),
                AuthenticationStrength = authResult.Strength
            };
            
            // Cache session
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = session.ExpiresAt,
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };
            
            _cache.Set($"session_{session.SessionId}", session, cacheOptions);
            
            _logger.LogInformation("Session created for user {UserId}: {SessionId}", session.UserId, session.SessionId);
            
            return FdwResult<IAuthenticationSession>.Success(session);
        }
        catch (Exception ex)
        {
            return FdwResult<IAuthenticationSession>.Failure("Session creation failed", ex);
        }
    }
    
    public async Task<IFdwResult<IAuthenticationSession>> GetSessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return FdwResult<IAuthenticationSession>.Failure("Session ID is required");
        
        try
        {
            if (_cache.TryGetValue($"session_{sessionId}", out var cachedSession) && 
                cachedSession is IAuthenticationSession session)
            {
                // Update last activity
                if (session is AuthenticationSession authSession)
                {
                    authSession.LastActivityAt = DateTimeOffset.UtcNow;
                    _cache.Set($"session_{sessionId}", authSession, TimeSpan.FromMinutes(30));
                }
                
                return FdwResult<IAuthenticationSession>.Success(session);
            }
            
            // Try to get from provider
            var providerSessions = await _provider.GetActiveSessionsAsync();
            if (providerSessions.IsSuccess)
            {
                var providerSession = providerSessions.Value.FirstOrDefault(s => s.SessionId == sessionId);
                if (providerSession != null)
                {
                    // Cache retrieved session
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = providerSession.ExpiresAt,
                        SlidingExpiration = TimeSpan.FromMinutes(30)
                    };
                    _cache.Set($"session_{sessionId}", providerSession, cacheOptions);
                    
                    return FdwResult<IAuthenticationSession>.Success(providerSession);
                }
            }
            
            return FdwResult<IAuthenticationSession>.Failure("Session not found");
        }
        catch (Exception ex)
        {
            return FdwResult<IAuthenticationSession>.Failure("Session retrieval failed", ex);
        }
    }
    
    public async Task<IFdwResult> RevokeSessionAsync(string sessionId)
    {
        try
        {
            // Remove from cache
            _cache.Remove($"session_{sessionId}");
            
            // Revoke through provider
            var result = await _provider.RevokeSessionAsync(sessionId);
            
            _logger.LogInformation("Session revoked: {SessionId}", sessionId);
            
            return result;
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("Session revocation failed", ex);
        }
    }
    
    public async Task<IFdwResult<IReadOnlyList<IAuthenticationSession>>> GetUserSessionsAsync(string userId)
    {
        try
        {
            var result = await _provider.GetUserSessionsAsync(userId);
            return result.IsSuccess 
                ? FdwResult<IReadOnlyList<IAuthenticationSession>>.Success(result.Value.ToList())
                : FdwResult<IReadOnlyList<IAuthenticationSession>>.Failure(result.ErrorMessage, result.Exception);
        }
        catch (Exception ex)
        {
            return FdwResult<IReadOnlyList<IAuthenticationSession>>.Failure("User session retrieval failed", ex);
        }
    }
}
```

## Best Practices

1. **Use provider discovery** for automatic registration and management
2. **Implement proper MFA flows** for enhanced security
3. **Validate commands thoroughly** before execution
4. **Handle token expiration gracefully** with refresh mechanisms
5. **Cache authentication results** appropriately to improve performance
6. **Log authentication events** for security auditing
7. **Use secure transport** for all authentication operations
8. **Implement session management** for user tracking
9. **Support multiple authentication flows** for flexibility
10. **Monitor provider health** proactively

## Integration with Other Framework Components

This abstraction layer works seamlessly with other FractalDataWorks packages:

- **ExternalConnections**: Authentication providers may use external connections to auth servers
- **SecretManagement**: API keys, certificates, and secrets are managed securely
- **DataProviders**: User data and authentication logs are stored via data providers
- **Transformations**: User data transformations between different provider formats
- **Scheduling**: Scheduled token refresh and session cleanup operations

## License

This package is part of the FractalDataWorks Framework and is licensed under the Apache 2.0 License.