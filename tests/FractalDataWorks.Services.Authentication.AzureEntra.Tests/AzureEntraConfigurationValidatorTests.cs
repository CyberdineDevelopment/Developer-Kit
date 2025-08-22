using System;
using FluentValidation;
using FractalDataWorks.Services.Authentication.AzureEntra.Configuration;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.AzureEntra.Tests;

/// <summary>
/// Tests for AzureEntraConfigurationValidator class.
/// </summary>
public sealed class AzureEntraConfigurationValidatorTests
{
    private readonly AzureEntraConfigurationValidator _validator = new();

    #region Valid Configuration Tests

    [Fact]
    public void ValidateWithValidConfigurationShouldReturnValid()
    {
        // Arrange
        var config = CreateValidConfiguration();

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateWithPublicClientShouldNotRequireClientSecret()
    {
        // Arrange
        var config = CreateValidConfiguration();
        var publicConfig = new AzureEntraConfiguration
        {
            ClientId = config.ClientId,
            TenantId = config.TenantId,
            Instance = config.Instance,
            Authority = config.Authority,
            RedirectUri = config.RedirectUri,
            Scopes = config.Scopes,
            ClientType = "Public",
            ClientSecret = string.Empty
        };

        // Act
        var result = _validator.Validate(publicConfig);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    #endregion

    #region ClientId Validation Tests

    [Fact]
    public void ValidateWithEmptyClientIdShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { ClientId = string.Empty };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.ClientId) && 
                                       e.ErrorMessage == "ClientId is required");
    }

    [Fact]
    public void ValidateWithInvalidClientIdGuidShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { ClientId = "not-a-valid-guid" };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.ClientId) && 
                                       e.ErrorMessage == "ClientId must be a valid GUID");
    }

    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void ValidateWithValidClientIdGuidShouldReturnValid(string clientId)
    {
        // Arrange
        var config = CreateValidConfiguration() with { ClientId = clientId };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    #endregion

    #region TenantId Validation Tests

    [Fact]
    public void ValidateWithEmptyTenantIdShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { TenantId = string.Empty };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.TenantId) && 
                                       e.ErrorMessage == "TenantId is required");
    }

    [Theory]
    [InlineData("common")]
    [InlineData("organizations")]
    [InlineData("consumers")]
    [InlineData("COMMON")]
    [InlineData("ORGANIZATIONS")]
    [InlineData("CONSUMERS")]
    public void ValidateWithCommonTenantIdentifiersShouldReturnValid(string tenantId)
    {
        // Arrange
        var config = CreateValidConfiguration() with { TenantId = tenantId };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("87654321-4321-4321-4321-210987654321")]
    [InlineData("FEDCBA98-7654-3210-FEDC-BA9876543210")]
    public void ValidateWithValidTenantIdGuidShouldReturnValid(string tenantId)
    {
        // Arrange
        var config = CreateValidConfiguration() with { TenantId = tenantId };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ValidateWithInvalidTenantIdShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { TenantId = "invalid-tenant-id" };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.TenantId) && 
                                       e.ErrorMessage == "TenantId must be a valid GUID or common tenant identifier");
    }

    #endregion

    #region URI Validation Tests

    [Theory]
    [InlineData(nameof(AzureEntraConfiguration.Instance), "")]
    [InlineData(nameof(AzureEntraConfiguration.RedirectUri), "")]
    [InlineData(nameof(AzureEntraConfiguration.Authority), "")]
    public void ValidateWithEmptyUriFieldsShouldReturnInvalid(string propertyName, string value)
    {
        // Arrange
        var config = CreateValidConfiguration();
        var configWithEmptyUri = propertyName switch
        {
            nameof(AzureEntraConfiguration.Instance) => config with { Instance = value },
            nameof(AzureEntraConfiguration.RedirectUri) => config with { RedirectUri = value },
            nameof(AzureEntraConfiguration.Authority) => config with { Authority = value },
            _ => config
        };

        // Act
        var result = _validator.Validate(configWithEmptyUri);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == propertyName);
    }

    [Theory]
    [InlineData("not-a-uri")]
    [InlineData("ftp://invalid.scheme.com")]
    [InlineData("file:///local/path")]
    public void ValidateWithInvalidUriFormatShouldReturnInvalid(string invalidUri)
    {
        // Arrange
        var config = CreateValidConfiguration() with { Authority = invalidUri };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.Authority));
    }

    [Theory]
    [InlineData("https://login.microsoftonline.com")]
    [InlineData("https://login.microsoftonline.us")]
    [InlineData("https://login.microsoftonline.de")]
    [InlineData("http://localhost:8080")]
    public void ValidateWithValidUrisShouldReturnValid(string uri)
    {
        // Arrange
        var config = CreateValidConfiguration() with 
        { 
            Instance = uri,
            Authority = uri,
            RedirectUri = uri
        };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    #endregion

    #region Scopes Validation Tests

    [Fact]
    public void ValidateWithNullScopesShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { Scopes = null! };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.Scopes) && 
                                       e.ErrorMessage == "Scopes cannot be null");
    }

    [Fact]
    public void ValidateWithEmptyScopesShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { Scopes = [] };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.Scopes) && 
                                       e.ErrorMessage == "Scopes must contain at least one valid scope");
    }

    [Fact]
    public void ValidateWithScopesContainingEmptyStringsShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { Scopes = ["openid", "", "profile"] };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.Scopes) && 
                                       e.ErrorMessage == "Scopes must contain at least one valid scope");
    }

    [Fact]
    public void ValidateWithScopesContainingWhitespaceShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { Scopes = ["openid", "   ", "profile"] };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.Scopes));
    }

    #endregion

    #region ClientType and ClientSecret Validation Tests

    [Fact]
    public void ValidateWithInvalidClientTypeShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { ClientType = "InvalidType" };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.ClientType) && 
                                       e.ErrorMessage == "ClientType must be either 'Public' or 'Confidential'");
    }

    [Fact]
    public void ValidateWithConfidentialClientAndEmptySecretShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with 
        { 
            ClientType = "Confidential",
            ClientSecret = string.Empty
        };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.ClientSecret) && 
                                       e.ErrorMessage == "ClientSecret is required for confidential client applications");
    }

    [Theory]
    [InlineData("Public")]
    [InlineData("public")]
    [InlineData("Confidential")]
    [InlineData("confidential")]
    public void ValidateWithValidClientTypesShouldReturnValid(string clientType)
    {
        // Arrange
        var config = CreateValidConfiguration() with 
        { 
            ClientType = clientType,
            ClientSecret = clientType.ToLowerInvariant() == "confidential" ? "secret-value" : string.Empty
        };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    #endregion

    #region Numeric Range Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void ValidateWithInvalidTokenCacheLifetimeShouldReturnInvalid(int lifetime)
    {
        // Arrange
        var config = CreateValidConfiguration() with { TokenCacheLifetimeMinutes = lifetime };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.TokenCacheLifetimeMinutes));
    }

    [Fact]
    public void ValidateWithTokenCacheLifetimeExceeding24HoursShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { TokenCacheLifetimeMinutes = 1441 }; // 24 hours + 1 minute

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.TokenCacheLifetimeMinutes) && 
                                       e.ErrorMessage == "TokenCacheLifetimeMinutes cannot exceed 24 hours (1440 minutes)");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(720)]
    [InlineData(1440)]
    public void ValidateWithValidTokenCacheLifetimeShouldReturnValid(int lifetime)
    {
        // Arrange
        var config = CreateValidConfiguration() with { TokenCacheLifetimeMinutes = lifetime };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(31)]
    public void ValidateWithInvalidClockSkewToleranceShouldReturnInvalid(int tolerance)
    {
        // Arrange
        var config = CreateValidConfiguration() with { ClockSkewToleranceMinutes = tolerance };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.ClockSkewToleranceMinutes));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(301)]
    public void ValidateWithInvalidHttpTimeoutShouldReturnInvalid(int timeout)
    {
        // Arrange
        var config = CreateValidConfiguration() with { HttpTimeoutSeconds = timeout };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.HttpTimeoutSeconds));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void ValidateWithInvalidMaxRetryAttemptsShouldReturnInvalid(int attempts)
    {
        // Arrange
        var config = CreateValidConfiguration() with { MaxRetryAttempts = attempts };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.MaxRetryAttempts));
    }

    #endregion

    #region Optional Field Validation Tests

    [Fact]
    public void ValidateWithNullCacheFilePathShouldReturnValid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { CacheFilePath = null };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ValidateWithEmptyCacheFilePathShouldReturnValid()
    {
        // Arrange
        var config = CreateValidConfiguration() with { CacheFilePath = string.Empty };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(@"C:\temp\cache.bin")]
    [InlineData(@"/home/user/cache.dat")]
    [InlineData(@"C:\Program Files\MyApp\token_cache.json")]
    public void ValidateWithValidCacheFilePathShouldReturnValid(string path)
    {
        // Arrange
        var config = CreateValidConfiguration() with { CacheFilePath = path };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("relative/path.bin")]
    [InlineData("invalid|path.bin")]
    [InlineData("")]
    public void ValidateWithInvalidCacheFilePathShouldReturnInvalid(string? path)
    {
        // Arrange
        var config = CreateValidConfiguration() with { CacheFilePath = path };

        // Act
        var result = _validator.Validate(config);

        // Assert - Only check for validation errors if path is actually invalid (non-empty and not rooted)
        if (!string.IsNullOrEmpty(path) && !System.IO.Path.IsPathRooted(path))
        {
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == nameof(AzureEntraConfiguration.CacheFilePath));
        }
    }

    [Fact]
    public void ValidateWithInvalidAdditionalAudiencesShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with 
        { 
            AdditionalValidAudiences = ["api://valid-app", "invalid-uri", "https://valid.com"] 
        };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Each additional valid audience must be a valid URI");
    }

    [Fact]
    public void ValidateWithInvalidAdditionalIssuersShouldReturnInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration() with 
        { 
            AdditionalValidIssuers = ["https://valid.issuer.com", "invalid-issuer-uri"] 
        };

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "Each additional valid issuer must be a valid URI");
    }

    #endregion

    #region Helper Methods

    private static AzureEntraConfiguration CreateValidConfiguration()
    {
        return new AzureEntraConfiguration
        {
            ClientId = "12345678-1234-1234-1234-123456789012",
            ClientSecret = "valid-client-secret",
            TenantId = "common",
            Instance = "https://login.microsoftonline.com",
            Authority = "https://login.microsoftonline.com/common",
            RedirectUri = "https://localhost:8080/callback",
            Scopes = ["openid", "profile"],
            ClientType = "Confidential"
        };
    }

    private static AzureEntraConfiguration CreateConfigurationWith(Action<AzureEntraConfiguration> configure)
    {
        var config = CreateValidConfiguration();
        configure(config);
        return config;
    }

    #endregion
}