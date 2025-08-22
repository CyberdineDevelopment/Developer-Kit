    private static AzureEntraConfiguration CreateValidConfigurationWith(
        string? clientId = null,
        string? clientSecret = null,
        string? tenantId = null,
        string? instance = null,
        string? authority = null,
        string? redirectUri = null,
        string[]? scopes = null,
        string? clientType = null,
        int? tokenCacheLifetimeMinutes = null,
        int? clockSkewToleranceMinutes = null,
        int? httpTimeoutSeconds = null,
        int? maxRetryAttempts = null,
        string? cacheFilePath = null)
    {
        return new AzureEntraConfiguration
        {
            ClientId = clientId ?? "12345678-1234-1234-1234-123456789012",
            ClientSecret = clientSecret ?? "valid-client-secret",
            TenantId = tenantId ?? "common",
            Instance = instance ?? "https://login.microsoftonline.com",
            Authority = authority ?? "https://login.microsoftonline.com/common",
            RedirectUri = redirectUri ?? "https://localhost:8080/callback",
            Scopes = scopes ?? ["openid", "profile"],
            ClientType = clientType ?? "Confidential",
            TokenCacheLifetimeMinutes = tokenCacheLifetimeMinutes ?? 60,
            ClockSkewToleranceMinutes = clockSkewToleranceMinutes ?? 5,
            HttpTimeoutSeconds = httpTimeoutSeconds ?? 30,
            MaxRetryAttempts = maxRetryAttempts ?? 3,
            CacheFilePath = cacheFilePath ?? "/tmp/token_cache"
        };
    }
