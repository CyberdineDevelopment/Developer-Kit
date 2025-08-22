using FractalDataWorks.Services.Authentication.AzureEntra.Configuration;
using FractalDataWorks.Services.Authentication.Abstractions;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.AzureEntra.Tests;

/// <summary>
/// Tests for AzureEntraConfiguration class.
/// </summary>
public sealed class AzureEntraConfigurationTests
{
    [Fact]
    public void SectionNameShouldReturnAzureEntra()
    {
        // Arrange
        var config = new AzureEntraConfiguration();

        // Act & Assert
        config.SectionName.ShouldBe("AzureEntra");
    }

    [Fact]
    public void ConfigurationShouldImplementIAuthenticationConfiguration()
    {
        // Arrange
        var config = new AzureEntraConfiguration();

        // Act & Assert
        config.ShouldBeAssignableTo<IAuthenticationConfiguration>();
    }

    [Fact]
    public void ClientIdPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedClientId = "12345678-1234-1234-1234-123456789012";
        
        // Act
        var config = new AzureEntraConfiguration { ClientId = expectedClientId };

        // Assert
        config.ClientId.ShouldBe(expectedClientId);
    }

    [Fact]
    public void ClientSecretPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedSecret = "super-secret-value";
        
        // Act
        var config = new AzureEntraConfiguration { ClientSecret = expectedSecret };

        // Assert
        config.ClientSecret.ShouldBe(expectedSecret);
    }

    [Fact]
    public void TenantIdPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedTenantId = "87654321-4321-4321-4321-210987654321";
        
        // Act
        var config = new AzureEntraConfiguration { TenantId = expectedTenantId };

        // Assert
        config.TenantId.ShouldBe(expectedTenantId);
    }

    [Fact]
    public void AuthorityPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedAuthority = "https://login.microsoftonline.com/tenant-id";
        
        // Act
        var config = new AzureEntraConfiguration { Authority = expectedAuthority };

        // Assert
        config.Authority.ShouldBe(expectedAuthority);
    }

    [Fact]
    public void RedirectUriPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedRedirectUri = "https://myapp.com/auth/callback";
        
        // Act
        var config = new AzureEntraConfiguration { RedirectUri = expectedRedirectUri };

        // Assert
        config.RedirectUri.ShouldBe(expectedRedirectUri);
    }

    [Fact]
    public void ScopesPropertyShouldBeAccessible()
    {
        // Arrange
        var expectedScopes = new[] { "openid", "profile", "user.read" };
        
        // Act
        var config = new AzureEntraConfiguration { Scopes = expectedScopes };

        // Assert
        config.Scopes.ShouldBe(expectedScopes);
    }

    [Fact]
    public void ScopesPropertyShouldDefaultToEmptyArray()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.Scopes.ShouldBeEmpty();
    }

    [Fact]
    public void EnableTokenCachingPropertyShouldDefaultToTrue()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.EnableTokenCaching.ShouldBeTrue();
    }

    [Fact]
    public void TokenCacheLifetimeMinutesPropertyShouldDefaultTo60()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.TokenCacheLifetimeMinutes.ShouldBe(60);
    }

    [Fact]
    public void InstancePropertyShouldDefaultToMicrosoftOnline()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.Instance.ShouldBe("https://login.microsoftonline.com");
    }

    [Theory]
    [InlineData("https://login.microsoftonline.com")]
    [InlineData("https://login.microsoftonline.us")]
    [InlineData("https://login.microsoftonline.de")]
    public void InstancePropertyShouldAcceptValidCloudInstances(string instance)
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration { Instance = instance };

        // Assert
        config.Instance.ShouldBe(instance);
    }

    [Fact]
    public void ClientTypePropertyShouldDefaultToConfidential()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.ClientType.ShouldBe("Confidential");
    }

    [Theory]
    [InlineData("Public")]
    [InlineData("Confidential")]
    public void ClientTypePropertyShouldAcceptValidClientTypes(string clientType)
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration { ClientType = clientType };

        // Assert
        config.ClientType.ShouldBe(clientType);
    }

    [Fact]
    public void ValidationPropertiesShouldDefaultToTrue()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.ValidateIssuer.ShouldBeTrue();
        config.ValidateAudience.ShouldBeTrue();
        config.ValidateLifetime.ShouldBeTrue();
        config.ValidateIssuerSigningKey.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true, false, true, false)]
    [InlineData(false, true, false, true)]
    [InlineData(true, true, true, true)]
    [InlineData(false, false, false, false)]
    public void ValidationPropertiesShouldBeConfigurable(bool validateIssuer, bool validateAudience, bool validateLifetime, bool validateSigningKey)
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration
        {
            ValidateIssuer = validateIssuer,
            ValidateAudience = validateAudience,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = validateSigningKey
        };

        // Assert
        config.ValidateIssuer.ShouldBe(validateIssuer);
        config.ValidateAudience.ShouldBe(validateAudience);
        config.ValidateLifetime.ShouldBe(validateLifetime);
        config.ValidateIssuerSigningKey.ShouldBe(validateSigningKey);
    }

    [Fact]
    public void ClockSkewToleranceMinutesShouldDefaultTo5()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.ClockSkewToleranceMinutes.ShouldBe(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(30)]
    public void ClockSkewToleranceMinutesShouldAcceptValidValues(int minutes)
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration { ClockSkewToleranceMinutes = minutes };

        // Assert
        config.ClockSkewToleranceMinutes.ShouldBe(minutes);
    }

    [Fact]
    public void CacheFilePathShouldDefaultToNull()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.CacheFilePath.ShouldBeNull();
    }

    [Fact]
    public void CacheFilePathShouldBeAccessible()
    {
        // Arrange
        const string expectedPath = @"C:\tokens\cache.bin";
        
        // Act
        var config = new AzureEntraConfiguration { CacheFilePath = expectedPath };

        // Assert
        config.CacheFilePath.ShouldBe(expectedPath);
    }

    [Fact]
    public void EnablePiiLoggingShouldDefaultToFalse()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.EnablePiiLogging.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnablePiiLoggingShouldBeConfigurable(bool enablePii)
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration { EnablePiiLogging = enablePii };

        // Assert
        config.EnablePiiLogging.ShouldBe(enablePii);
    }

    [Fact]
    public void HttpTimeoutSecondsShouldDefaultTo30()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.HttpTimeoutSeconds.ShouldBe(30);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(60)]
    [InlineData(120)]
    [InlineData(300)]
    public void HttpTimeoutSecondsShouldAcceptValidValues(int seconds)
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration { HttpTimeoutSeconds = seconds };

        // Assert
        config.HttpTimeoutSeconds.ShouldBe(seconds);
    }

    [Fact]
    public void MaxRetryAttemptsShouldDefaultTo3()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.MaxRetryAttempts.ShouldBe(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void MaxRetryAttemptsShouldAcceptValidValues(int attempts)
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration { MaxRetryAttempts = attempts };

        // Assert
        config.MaxRetryAttempts.ShouldBe(attempts);
    }

    [Fact]
    public void AdditionalValidAudiencesShouldDefaultToEmptyArray()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.AdditionalValidAudiences.ShouldBeEmpty();
    }

    [Fact]
    public void AdditionalValidAudiencesShouldBeAccessible()
    {
        // Arrange
        var expectedAudiences = new[] { "api://app1", "api://app2" };
        
        // Act
        var config = new AzureEntraConfiguration { AdditionalValidAudiences = expectedAudiences };

        // Assert
        config.AdditionalValidAudiences.ShouldBe(expectedAudiences);
    }

    [Fact]
    public void AdditionalValidIssuersShouldDefaultToEmptyArray()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration();

        // Assert
        config.AdditionalValidIssuers.ShouldBeEmpty();
    }

    [Fact]
    public void AdditionalValidIssuersShouldBeAccessible()
    {
        // Arrange
        var expectedIssuers = new[] { "https://sts.windows.net/tenant1/", "https://sts.windows.net/tenant2/" };
        
        // Act
        var config = new AzureEntraConfiguration { AdditionalValidIssuers = expectedIssuers };

        // Assert
        config.AdditionalValidIssuers.ShouldBe(expectedIssuers);
    }

    [Fact]
    public void ValidateMethodShouldReturnValidationResult()
    {
        // Arrange
        var config = new AzureEntraConfiguration
        {
            ClientId = "12345678-1234-1234-1234-123456789012",
            TenantId = "common",
            Authority = "https://login.microsoftonline.com/common",
            RedirectUri = "https://localhost:8080",
            Scopes = ["openid"]
        };

        // Act
        var result = config.Validate();

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ValidateMethodShouldDetectInvalidConfiguration()
    {
        // Arrange
        var config = new AzureEntraConfiguration
        {
            ClientId = "invalid-client-id", // Not a GUID
            TenantId = "", // Empty
            Authority = "invalid-uri", // Not a valid URI
            RedirectUri = "invalid-uri", // Not a valid URI
            Scopes = [] // Empty scopes
        };

        // Act
        var result = config.Validate();

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CompleteConfigurationShouldSetAllProperties()
    {
        // Arrange & Act
        var config = new AzureEntraConfiguration
        {
            ClientId = "11111111-1111-1111-1111-111111111111",
            ClientSecret = "my-secret-key",
            TenantId = "22222222-2222-2222-2222-222222222222",
            Authority = "https://login.microsoftonline.com/22222222-2222-2222-2222-222222222222",
            RedirectUri = "https://myapp.example.com/auth/callback",
            Scopes = ["openid", "profile", "email", "user.read", "mail.read"],
            EnableTokenCaching = false,
            TokenCacheLifetimeMinutes = 120,
            Instance = "https://login.microsoftonline.us",
            ClientType = "Public",
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            ClockSkewToleranceMinutes = 10,
            CacheFilePath = @"C:\MyApp\Tokens\cache.dat",
            EnablePiiLogging = true,
            HttpTimeoutSeconds = 60,
            MaxRetryAttempts = 5,
            AdditionalValidAudiences = ["api://myapp", "api://partner-app"],
            AdditionalValidIssuers = ["https://sts.windows.net/tenant1/", "https://login.partner.com/"]
        };

        // Assert
        config.ClientId.ShouldBe("11111111-1111-1111-1111-111111111111");
        config.ClientSecret.ShouldBe("my-secret-key");
        config.TenantId.ShouldBe("22222222-2222-2222-2222-222222222222");
        config.Authority.ShouldBe("https://login.microsoftonline.com/22222222-2222-2222-2222-222222222222");
        config.RedirectUri.ShouldBe("https://myapp.example.com/auth/callback");
        config.Scopes.ShouldBe(["openid", "profile", "email", "user.read", "mail.read"]);
        config.EnableTokenCaching.ShouldBeFalse();
        config.TokenCacheLifetimeMinutes.ShouldBe(120);
        config.Instance.ShouldBe("https://login.microsoftonline.us");
        config.ClientType.ShouldBe("Public");
        config.ValidateIssuer.ShouldBeFalse();
        config.ValidateAudience.ShouldBeFalse();
        config.ValidateLifetime.ShouldBeFalse();
        config.ValidateIssuerSigningKey.ShouldBeFalse();
        config.ClockSkewToleranceMinutes.ShouldBe(10);
        config.CacheFilePath.ShouldBe(@"C:\MyApp\Tokens\cache.dat");
        config.EnablePiiLogging.ShouldBeTrue();
        config.HttpTimeoutSeconds.ShouldBe(60);
        config.MaxRetryAttempts.ShouldBe(5);
        config.AdditionalValidAudiences.ShouldBe(["api://myapp", "api://partner-app"]);
        config.AdditionalValidIssuers.ShouldBe(["https://sts.windows.net/tenant1/", "https://login.partner.com/"]);
    }
}