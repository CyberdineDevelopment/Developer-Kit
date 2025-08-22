using System;
using FluentValidation.Results;
using FractalDataWorks.Services.Authentication.Abstractions.Commands;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

/// <summary>
/// Tests for ITokenRefreshCommand interface contract verification.
/// </summary>
public sealed class ITokenRefreshCommandTests
{
    /// <summary>
    /// Mock implementation of ITokenRefreshCommand for testing.
    /// </summary>
    private sealed class MockTokenRefreshCommand : ITokenRefreshCommand
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string[]? Scopes { get; set; }
        public bool ForceRefresh { get; set; }
        public string? ClientAssertion { get; set; }
        public string? ClientAssertionType { get; set; }
        
        // ICommand implementation
        public Guid CommandId { get; set; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public IFdwConfiguration? Configuration { get; set; }
        
        public ValidationResult Validate() => new();
    }

    [Fact]
    public void RefreshTokenPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedRefreshToken = "refresh_token_value_123";
        var command = new MockTokenRefreshCommand
        {
            RefreshToken = expectedRefreshToken
        };

        // Act & Assert
        command.RefreshToken.ShouldBe(expectedRefreshToken);
    }

    [Fact]
    public void AccountIdPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedAccountId = "account-123-456";
        var command = new MockTokenRefreshCommand
        {
            AccountId = expectedAccountId
        };

        // Act & Assert
        command.AccountId.ShouldBe(expectedAccountId);
    }

    [Fact]
    public void ScopesPropertyShouldBeAccessible()
    {
        // Arrange
        var expectedScopes = new[] { "user.read", "mail.read", "profile" };
        var command = new MockTokenRefreshCommand
        {
            Scopes = expectedScopes
        };

        // Act & Assert
        command.Scopes.ShouldBe(expectedScopes);
    }

    [Fact]
    public void ScopesPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockTokenRefreshCommand
        {
            Scopes = null
        };

        // Act & Assert
        command.Scopes.ShouldBeNull();
    }

    [Fact]
    public void ScopesPropertyShouldAcceptEmptyArray()
    {
        // Arrange
        var command = new MockTokenRefreshCommand
        {
            Scopes = []
        };

        // Act & Assert
        command.Scopes.ShouldBeEmpty();
    }

    [Fact]
    public void ForceRefreshPropertyShouldBeAccessible()
    {
        // Arrange
        var command = new MockTokenRefreshCommand
        {
            ForceRefresh = true
        };

        // Act & Assert
        command.ForceRefresh.ShouldBeTrue();
    }

    [Fact]
    public void ForceRefreshPropertyShouldDefaultToFalse()
    {
        // Arrange
        var command = new MockTokenRefreshCommand();

        // Act & Assert
        command.ForceRefresh.ShouldBeFalse();
    }

    [Fact]
    public void ClientAssertionPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedAssertion = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlBpQ...";
        var command = new MockTokenRefreshCommand
        {
            ClientAssertion = expectedAssertion
        };

        // Act & Assert
        command.ClientAssertion.ShouldBe(expectedAssertion);
    }

    [Fact]
    public void ClientAssertionPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockTokenRefreshCommand
        {
            ClientAssertion = null
        };

        // Act & Assert
        command.ClientAssertion.ShouldBeNull();
    }

    [Fact]
    public void ClientAssertionTypePropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
        var command = new MockTokenRefreshCommand
        {
            ClientAssertionType = expectedType
        };

        // Act & Assert
        command.ClientAssertionType.ShouldBe(expectedType);
    }

    [Fact]
    public void ClientAssertionTypePropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockTokenRefreshCommand
        {
            ClientAssertionType = null
        };

        // Act & Assert
        command.ClientAssertionType.ShouldBeNull();
    }

    [Fact]
    public void TokenRefreshCommandShouldInheritFromIAuthenticationCommand()
    {
        // Arrange
        var command = new MockTokenRefreshCommand();

        // Act & Assert
        command.ShouldBeAssignableTo<ITokenRefreshCommand>();
        command.ShouldBeAssignableTo<IAuthenticationCommand>();
    }

    [Fact]
    public void TokenRefreshCommandShouldSupportCompleteConfiguration()
    {
        // Arrange
        var command = new MockTokenRefreshCommand
        {
            RefreshToken = "rt_abcd1234efgh5678ijkl",
            AccountId = "user-account-789",
            Scopes = ["openid", "profile", "user.read", "mail.send"],
            ForceRefresh = true,
            ClientAssertion = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlBpQ...",
            ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"
        };

        // Act & Assert
        command.RefreshToken.ShouldBe("rt_abcd1234efgh5678ijkl");
        command.AccountId.ShouldBe("user-account-789");
        command.Scopes.ShouldBe(["openid", "profile", "user.read", "mail.send"]);
        command.ForceRefresh.ShouldBeTrue();
        command.ClientAssertion.ShouldBe("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlBpQ...");
        command.ClientAssertionType.ShouldBe("urn:ietf:params:oauth:client-assertion-type:jwt-bearer");
    }

    [Fact]
    public void TokenRefreshCommandShouldSupportMinimalConfiguration()
    {
        // Arrange
        var command = new MockTokenRefreshCommand
        {
            RefreshToken = "simple-refresh-token",
            AccountId = "simple-account"
        };

        // Act & Assert
        command.RefreshToken.ShouldBe("simple-refresh-token");
        command.AccountId.ShouldBe("simple-account");
        command.Scopes.ShouldBeNull();
        command.ForceRefresh.ShouldBeFalse();
        command.ClientAssertion.ShouldBeNull();
        command.ClientAssertionType.ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ForceRefreshPropertyShouldAcceptBothBooleanValues(bool forceRefresh)
    {
        // Arrange
        var command = new MockTokenRefreshCommand
        {
            ForceRefresh = forceRefresh
        };

        // Act & Assert
        command.ForceRefresh.ShouldBe(forceRefresh);
    }

    [Fact]
    public void ClientAssertionAndTypeShouldWorkTogether()
    {
        // Arrange
        const string assertion = "jwt_assertion_token";
        const string assertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
        
        var command = new MockTokenRefreshCommand
        {
            ClientAssertion = assertion,
            ClientAssertionType = assertionType
        };

        // Act & Assert
        command.ClientAssertion.ShouldBe(assertion);
        command.ClientAssertionType.ShouldBe(assertionType);
    }

    [Fact]
    public void ScopesPropertyShouldSupportVariousArraySizes()
    {
        // Arrange & Act & Assert
        var command1 = new MockTokenRefreshCommand { Scopes = ["single"] };
        command1.Scopes.ShouldHaveSingleItem();
        command1.Scopes![0].ShouldBe("single");

        var command2 = new MockTokenRefreshCommand { Scopes = ["scope1", "scope2", "scope3"] };
        command2.Scopes.Length.ShouldBe(3);
        command2.Scopes.ShouldContain("scope1");
        command2.Scopes.ShouldContain("scope2");
        command2.Scopes.ShouldContain("scope3");
    }
}