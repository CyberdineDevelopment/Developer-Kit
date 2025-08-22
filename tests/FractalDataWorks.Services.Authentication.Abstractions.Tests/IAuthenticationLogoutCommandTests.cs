using System;
using FluentValidation.Results;
using FractalDataWorks.Services.Authentication.Abstractions.Commands;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

/// <summary>
/// Tests for IAuthenticationLogoutCommand interface contract verification.
/// </summary>
public sealed class IAuthenticationLogoutCommandTests
{
    /// <summary>
    /// Mock implementation of IAuthenticationLogoutCommand for testing.
    /// </summary>
    private sealed class MockAuthenticationLogoutCommand : IAuthenticationLogoutCommand
    {
        public string AccountId { get; set; } = string.Empty;
        public string LogoutType { get; set; } = string.Empty;
        public string? PostLogoutRedirectUri { get; set; }
        public bool ClearTokenCache { get; set; }
        public bool GlobalLogout { get; set; }
        
        // ICommand implementation
        public Guid CommandId { get; set; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public IFdwConfiguration? Configuration { get; set; }
        
        public ValidationResult Validate() => new();
    }

    [Fact]
    public void AccountIdPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedAccountId = "user-account-456";
        var command = new MockAuthenticationLogoutCommand
        {
            AccountId = expectedAccountId
        };

        // Act & Assert
        command.AccountId.ShouldBe(expectedAccountId);
    }

    [Fact]
    public void LogoutTypePropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedLogoutType = "SignOut";
        var command = new MockAuthenticationLogoutCommand
        {
            LogoutType = expectedLogoutType
        };

        // Act & Assert
        command.LogoutType.ShouldBe(expectedLogoutType);
    }

    [Theory]
    [InlineData("SignOut")]
    [InlineData("ClearCache")]
    [InlineData("GlobalSignOut")]
    [InlineData("FrontChannelSignOut")]
    public void LogoutTypePropertyShouldAcceptValidLogoutTypes(string logoutType)
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand
        {
            LogoutType = logoutType
        };

        // Act & Assert
        command.LogoutType.ShouldBe(logoutType);
    }

    [Fact]
    public void PostLogoutRedirectUriPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedUri = "https://myapp.com/logout-success";
        var command = new MockAuthenticationLogoutCommand
        {
            PostLogoutRedirectUri = expectedUri
        };

        // Act & Assert
        command.PostLogoutRedirectUri.ShouldBe(expectedUri);
    }

    [Fact]
    public void PostLogoutRedirectUriPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand
        {
            PostLogoutRedirectUri = null
        };

        // Act & Assert
        command.PostLogoutRedirectUri.ShouldBeNull();
    }

    [Fact]
    public void ClearTokenCachePropertyShouldBeAccessible()
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand
        {
            ClearTokenCache = true
        };

        // Act & Assert
        command.ClearTokenCache.ShouldBeTrue();
    }

    [Fact]
    public void ClearTokenCachePropertyShouldDefaultToFalse()
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand();

        // Act & Assert
        command.ClearTokenCache.ShouldBeFalse();
    }

    [Fact]
    public void GlobalLogoutPropertyShouldBeAccessible()
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand
        {
            GlobalLogout = true
        };

        // Act & Assert
        command.GlobalLogout.ShouldBeTrue();
    }

    [Fact]
    public void GlobalLogoutPropertyShouldDefaultToFalse()
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand();

        // Act & Assert
        command.GlobalLogout.ShouldBeFalse();
    }

    [Fact]
    public void AuthenticationLogoutCommandShouldInheritFromIAuthenticationCommand()
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand();

        // Act & Assert
        command.ShouldBeAssignableTo<IAuthenticationLogoutCommand>();
        command.ShouldBeAssignableTo<IAuthenticationCommand>();
    }

    [Fact]
    public void LogoutCommandShouldSupportCompleteConfiguration()
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand
        {
            AccountId = "account-789",
            LogoutType = "GlobalSignOut",
            PostLogoutRedirectUri = "https://contoso.com/logout-complete",
            ClearTokenCache = true,
            GlobalLogout = true
        };

        // Act & Assert
        command.AccountId.ShouldBe("account-789");
        command.LogoutType.ShouldBe("GlobalSignOut");
        command.PostLogoutRedirectUri.ShouldBe("https://contoso.com/logout-complete");
        command.ClearTokenCache.ShouldBeTrue();
        command.GlobalLogout.ShouldBeTrue();
    }

    [Fact]
    public void LogoutCommandShouldSupportMinimalConfiguration()
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand
        {
            AccountId = "simple-account",
            LogoutType = "SignOut"
        };

        // Act & Assert
        command.AccountId.ShouldBe("simple-account");
        command.LogoutType.ShouldBe("SignOut");
        command.PostLogoutRedirectUri.ShouldBeNull();
        command.ClearTokenCache.ShouldBeFalse();
        command.GlobalLogout.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void BooleanPropertiesShouldAcceptAllCombinations(bool clearTokenCache, bool globalLogout)
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand
        {
            ClearTokenCache = clearTokenCache,
            GlobalLogout = globalLogout
        };

        // Act & Assert
        command.ClearTokenCache.ShouldBe(clearTokenCache);
        command.GlobalLogout.ShouldBe(globalLogout);
    }

    [Fact]
    public void LogoutCommandShouldSupportCacheOnlyOperation()
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand
        {
            AccountId = "cache-user",
            LogoutType = "ClearCache",
            ClearTokenCache = true,
            GlobalLogout = false
        };

        // Act & Assert
        command.LogoutType.ShouldBe("ClearCache");
        command.ClearTokenCache.ShouldBeTrue();
        command.GlobalLogout.ShouldBeFalse();
    }

    [Fact]
    public void LogoutCommandShouldSupportGlobalLogoutOperation()
    {
        // Arrange
        var command = new MockAuthenticationLogoutCommand
        {
            AccountId = "global-user",
            LogoutType = "GlobalSignOut",
            ClearTokenCache = true,
            GlobalLogout = true,
            PostLogoutRedirectUri = "https://enterprise.com/global-logout"
        };

        // Act & Assert
        command.LogoutType.ShouldBe("GlobalSignOut");
        command.ClearTokenCache.ShouldBeTrue();
        command.GlobalLogout.ShouldBeTrue();
        command.PostLogoutRedirectUri.ShouldBe("https://enterprise.com/global-logout");
    }
}