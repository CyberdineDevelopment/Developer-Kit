using FluentValidation.Results;
using FractalDataWorks.Services.Authentication.Abstractions;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

/// <summary>
/// Tests for IAuthenticationConfiguration interface contract verification.
/// </summary>
public sealed class IAuthenticationConfigurationTests
{
    /// <summary>
    /// Mock implementation of IAuthenticationConfiguration for testing.
    /// </summary>
    private sealed class MockAuthenticationConfiguration : IAuthenticationConfiguration
    {
        public string SectionName { get; set; } = "Test";
        public string ClientId { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = [];
        public bool EnableTokenCaching { get; set; }
        public int TokenCacheLifetimeMinutes { get; set; }
        
        public ValidationResult Validate() => new();
    }

    [Fact]
    public void ClientIdPropertyShouldBeAccessible()
    {
        // Arrange
        var config = new MockAuthenticationConfiguration
        {
            ClientId = "test-client-id"
        };

        // Act & Assert
        config.ClientId.ShouldBe("test-client-id");
    }

    [Fact]
    public void AuthorityPropertyShouldBeAccessible()
    {
        // Arrange
        var expectedAuthority = "https://login.microsoftonline.com/common";
        var config = new MockAuthenticationConfiguration
        {
            Authority = expectedAuthority
        };

        // Act & Assert
        config.Authority.ShouldBe(expectedAuthority);
    }

    [Fact]
    public void RedirectUriPropertyShouldBeAccessible()
    {
        // Arrange
        var expectedRedirectUri = "https://localhost:8080/callback";
        var config = new MockAuthenticationConfiguration
        {
            RedirectUri = expectedRedirectUri
        };

        // Act & Assert
        config.RedirectUri.ShouldBe(expectedRedirectUri);
    }

    [Fact]
    public void ScopesPropertyShouldBeAccessible()
    {
        // Arrange
        var expectedScopes = new[] { "openid", "profile", "email", "offline_access" };
        var config = new MockAuthenticationConfiguration
        {
            Scopes = expectedScopes
        };

        // Act & Assert
        config.Scopes.ShouldBe(expectedScopes);
    }

    [Fact]
    public void ScopesPropertyShouldHandleEmptyArray()
    {
        // Arrange
        var config = new MockAuthenticationConfiguration
        {
            Scopes = []
        };

        // Act & Assert
        config.Scopes.ShouldBeEmpty();
    }

    [Fact]
    public void EnableTokenCachingPropertyShouldBeAccessible()
    {
        // Arrange
        var config = new MockAuthenticationConfiguration
        {
            EnableTokenCaching = true
        };

        // Act & Assert
        config.EnableTokenCaching.ShouldBeTrue();
    }

    [Fact]
    public void EnableTokenCachingPropertyShouldDefaultToFalse()
    {
        // Arrange
        var config = new MockAuthenticationConfiguration();

        // Act & Assert
        config.EnableTokenCaching.ShouldBeFalse();
    }

    [Fact]
    public void TokenCacheLifetimeMinutesPropertyShouldBeAccessible()
    {
        // Arrange
        const int expectedLifetime = 30;
        var config = new MockAuthenticationConfiguration
        {
            TokenCacheLifetimeMinutes = expectedLifetime
        };

        // Act & Assert
        config.TokenCacheLifetimeMinutes.ShouldBe(expectedLifetime);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(60)]
    [InlineData(120)]
    [InlineData(1440)] // 24 hours
    public void TokenCacheLifetimeMinutesPropertyShouldAcceptValidValues(int minutes)
    {
        // Arrange
        var config = new MockAuthenticationConfiguration
        {
            TokenCacheLifetimeMinutes = minutes
        };

        // Act & Assert
        config.TokenCacheLifetimeMinutes.ShouldBe(minutes);
    }

    [Fact]
    public void SectionNamePropertyShouldBeInheritedFromIFdwConfiguration()
    {
        // Arrange
        var expectedSectionName = "AuthConfig";
        var config = new MockAuthenticationConfiguration
        {
            SectionName = expectedSectionName
        };

        // Act & Assert
        config.SectionName.ShouldBe(expectedSectionName);
    }

    [Fact]
    public void ConfigurationShouldSupportCompleteConfiguration()
    {
        // Arrange
        var config = new MockAuthenticationConfiguration
        {
            SectionName = "TestAuth",
            ClientId = "app-client-123",
            Authority = "https://login.microsoftonline.com/tenant-id",
            RedirectUri = "https://myapp.com/auth/callback",
            Scopes = ["user.read", "mail.read", "calendars.read"],
            EnableTokenCaching = true,
            TokenCacheLifetimeMinutes = 45
        };

        // Act & Assert
        config.SectionName.ShouldBe("TestAuth");
        config.ClientId.ShouldBe("app-client-123");
        config.Authority.ShouldBe("https://login.microsoftonline.com/tenant-id");
        config.RedirectUri.ShouldBe("https://myapp.com/auth/callback");
        config.Scopes.ShouldBe(["user.read", "mail.read", "calendars.read"]);
        config.EnableTokenCaching.ShouldBeTrue();
        config.TokenCacheLifetimeMinutes.ShouldBe(45);
    }
}