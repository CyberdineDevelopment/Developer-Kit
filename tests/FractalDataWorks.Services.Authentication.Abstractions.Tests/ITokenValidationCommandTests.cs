using System;
using FluentValidation.Results;
using FractalDataWorks.Services.Authentication.Abstractions.Commands;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

/// <summary>
/// Tests for ITokenValidationCommand interface contract verification.
/// </summary>
public sealed class ITokenValidationCommandTests
{
    /// <summary>
    /// Mock implementation of ITokenValidationCommand for testing.
    /// </summary>
    private sealed class MockTokenValidationCommand : ITokenValidationCommand
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public string? ExpectedAudience { get; set; }
        public string? ExpectedIssuer { get; set; }
        public string[]? RequiredScopes { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool ValidateSignature { get; set; }
        
        // ICommand implementation
        public Guid CommandId { get; set; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public IFdwConfiguration? Configuration { get; set; }
        
        public ValidationResult Validate() => new();
    }

    [Fact]
    public void AccessTokenPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIs...";
        var command = new MockTokenValidationCommand
        {
            AccessToken = expectedToken
        };

        // Act & Assert
        command.AccessToken.ShouldBe(expectedToken);
    }

    [Fact]
    public void TokenTypePropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedTokenType = "Bearer";
        var command = new MockTokenValidationCommand
        {
            TokenType = expectedTokenType
        };

        // Act & Assert
        command.TokenType.ShouldBe(expectedTokenType);
    }

    [Theory]
    [InlineData("Bearer")]
    [InlineData("Pop")]
    [InlineData("JWT")]
    public void TokenTypePropertyShouldAcceptValidTokenTypes(string tokenType)
    {
        // Arrange
        var command = new MockTokenValidationCommand
        {
            TokenType = tokenType
        };

        // Act & Assert
        command.TokenType.ShouldBe(tokenType);
    }

    [Fact]
    public void ExpectedAudiencePropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedAudience = "api://my-app";
        var command = new MockTokenValidationCommand
        {
            ExpectedAudience = expectedAudience
        };

        // Act & Assert
        command.ExpectedAudience.ShouldBe(expectedAudience);
    }

    [Fact]
    public void ExpectedAudiencePropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockTokenValidationCommand
        {
            ExpectedAudience = null
        };

        // Act & Assert
        command.ExpectedAudience.ShouldBeNull();
    }

    [Fact]
    public void ExpectedIssuerPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedIssuer = "https://login.microsoftonline.com/tenant-id/v2.0";
        var command = new MockTokenValidationCommand
        {
            ExpectedIssuer = expectedIssuer
        };

        // Act & Assert
        command.ExpectedIssuer.ShouldBe(expectedIssuer);
    }

    [Fact]
    public void ExpectedIssuerPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockTokenValidationCommand
        {
            ExpectedIssuer = null
        };

        // Act & Assert
        command.ExpectedIssuer.ShouldBeNull();
    }

    [Fact]
    public void RequiredScopesPropertyShouldBeAccessible()
    {
        // Arrange
        var expectedScopes = new[] { "user.read", "mail.read" };
        var command = new MockTokenValidationCommand
        {
            RequiredScopes = expectedScopes
        };

        // Act & Assert
        command.RequiredScopes.ShouldBe(expectedScopes);
    }

    [Fact]
    public void RequiredScopesPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockTokenValidationCommand
        {
            RequiredScopes = null
        };

        // Act & Assert
        command.RequiredScopes.ShouldBeNull();
    }

    [Fact]
    public void RequiredScopesPropertyShouldAcceptEmptyArray()
    {
        // Arrange
        var command = new MockTokenValidationCommand
        {
            RequiredScopes = []
        };

        // Act & Assert
        command.RequiredScopes.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateLifetimePropertyShouldBeAccessible()
    {
        // Arrange
        var command = new MockTokenValidationCommand
        {
            ValidateLifetime = true
        };

        // Act & Assert
        command.ValidateLifetime.ShouldBeTrue();
    }

    [Fact]
    public void ValidateLifetimePropertyShouldDefaultToFalse()
    {
        // Arrange
        var command = new MockTokenValidationCommand();

        // Act & Assert
        command.ValidateLifetime.ShouldBeFalse();
    }

    [Fact]
    public void ValidateSignaturePropertyShouldBeAccessible()
    {
        // Arrange
        var command = new MockTokenValidationCommand
        {
            ValidateSignature = true
        };

        // Act & Assert
        command.ValidateSignature.ShouldBeTrue();
    }

    [Fact]
    public void ValidateSignaturePropertyShouldDefaultToFalse()
    {
        // Arrange
        var command = new MockTokenValidationCommand();

        // Act & Assert
        command.ValidateSignature.ShouldBeFalse();
    }

    [Fact]
    public void TokenValidationCommandShouldInheritFromIAuthenticationCommand()
    {
        // Arrange
        var command = new MockTokenValidationCommand();

        // Act & Assert
        command.ShouldBeAssignableTo<ITokenValidationCommand>();
        command.ShouldBeAssignableTo<IAuthenticationCommand>();
    }

    [Fact]
    public void TokenValidationCommandShouldSupportCompleteConfiguration()
    {
        // Arrange
        var command = new MockTokenValidationCommand
        {
            AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImRJK3p...",
            TokenType = "Bearer",
            ExpectedAudience = "api://my-application",
            ExpectedIssuer = "https://login.microsoftonline.com/common/v2.0",
            RequiredScopes = ["user.read", "profile", "openid"],
            ValidateLifetime = true,
            ValidateSignature = true
        };

        // Act & Assert
        command.AccessToken.ShouldBe("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImRJK3p...");
        command.TokenType.ShouldBe("Bearer");
        command.ExpectedAudience.ShouldBe("api://my-application");
        command.ExpectedIssuer.ShouldBe("https://login.microsoftonline.com/common/v2.0");
        command.RequiredScopes.ShouldBe(["user.read", "profile", "openid"]);
        command.ValidateLifetime.ShouldBeTrue();
        command.ValidateSignature.ShouldBeTrue();
    }

    [Fact]
    public void TokenValidationCommandShouldSupportMinimalConfiguration()
    {
        // Arrange
        var command = new MockTokenValidationCommand
        {
            AccessToken = "simple-token",
            TokenType = "Bearer"
        };

        // Act & Assert
        command.AccessToken.ShouldBe("simple-token");
        command.TokenType.ShouldBe("Bearer");
        command.ExpectedAudience.ShouldBeNull();
        command.ExpectedIssuer.ShouldBeNull();
        command.RequiredScopes.ShouldBeNull();
        command.ValidateLifetime.ShouldBeFalse();
        command.ValidateSignature.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void ValidationPropertiesShouldAcceptAllBooleanCombinations(bool validateLifetime, bool validateSignature)
    {
        // Arrange
        var command = new MockTokenValidationCommand
        {
            ValidateLifetime = validateLifetime,
            ValidateSignature = validateSignature
        };

        // Act & Assert
        command.ValidateLifetime.ShouldBe(validateLifetime);
        command.ValidateSignature.ShouldBe(validateSignature);
    }
}