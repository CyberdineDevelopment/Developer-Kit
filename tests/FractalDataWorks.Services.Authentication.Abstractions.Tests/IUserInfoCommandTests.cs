using System;
using FluentValidation.Results;
using FractalDataWorks.Services.Authentication.Abstractions.Commands;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

/// <summary>
/// Tests for IUserInfoCommand interface contract verification.
/// </summary>
public sealed class IUserInfoCommandTests
{
    /// <summary>
    /// Mock implementation of IUserInfoCommand for testing.
    /// </summary>
    private sealed class MockUserInfoCommand : IUserInfoCommand
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string[]? RequestedClaims { get; set; }
        public bool IncludeExtendedProfile { get; set; }
        public string UserInfoSource { get; set; } = string.Empty;
        
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
        const string expectedToken = "access_token_xyz789";
        var command = new MockUserInfoCommand
        {
            AccessToken = expectedToken
        };

        // Act & Assert
        command.AccessToken.ShouldBe(expectedToken);
    }

    [Fact]
    public void UserIdPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedUserId = "user-123-456";
        var command = new MockUserInfoCommand
        {
            UserId = expectedUserId
        };

        // Act & Assert
        command.UserId.ShouldBe(expectedUserId);
    }

    [Fact]
    public void UserIdPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            UserId = null
        };

        // Act & Assert
        command.UserId.ShouldBeNull();
    }

    [Fact]
    public void RequestedClaimsPropertyShouldBeAccessible()
    {
        // Arrange
        var expectedClaims = new[] { "name", "email", "given_name", "family_name" };
        var command = new MockUserInfoCommand
        {
            RequestedClaims = expectedClaims
        };

        // Act & Assert
        command.RequestedClaims.ShouldBe(expectedClaims);
    }

    [Fact]
    public void RequestedClaimsPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            RequestedClaims = null
        };

        // Act & Assert
        command.RequestedClaims.ShouldBeNull();
    }

    [Fact]
    public void RequestedClaimsPropertyShouldAcceptEmptyArray()
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            RequestedClaims = []
        };

        // Act & Assert
        command.RequestedClaims.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("email")]
    [InlineData("given_name")]
    [InlineData("family_name")]
    [InlineData("preferred_username")]
    public void RequestedClaimsPropertyShouldAcceptCommonClaimTypes(string claimType)
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            RequestedClaims = [claimType]
        };

        // Act & Assert
        command.RequestedClaims.ShouldContain(claimType);
    }

    [Fact]
    public void IncludeExtendedProfilePropertyShouldBeAccessible()
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            IncludeExtendedProfile = true
        };

        // Act & Assert
        command.IncludeExtendedProfile.ShouldBeTrue();
    }

    [Fact]
    public void IncludeExtendedProfilePropertyShouldDefaultToFalse()
    {
        // Arrange
        var command = new MockUserInfoCommand();

        // Act & Assert
        command.IncludeExtendedProfile.ShouldBeFalse();
    }

    [Fact]
    public void UserInfoSourcePropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedSource = "UserInfoEndpoint";
        var command = new MockUserInfoCommand
        {
            UserInfoSource = expectedSource
        };

        // Act & Assert
        command.UserInfoSource.ShouldBe(expectedSource);
    }

    [Theory]
    [InlineData("Token")]
    [InlineData("UserInfoEndpoint")]
    [InlineData("Graph")]
    [InlineData("Cache")]
    public void UserInfoSourcePropertyShouldAcceptValidSources(string source)
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            UserInfoSource = source
        };

        // Act & Assert
        command.UserInfoSource.ShouldBe(source);
    }

    [Fact]
    public void UserInfoCommandShouldInheritFromIAuthenticationCommand()
    {
        // Arrange
        var command = new MockUserInfoCommand();

        // Act & Assert
        command.ShouldBeAssignableTo<IUserInfoCommand>();
        command.ShouldBeAssignableTo<IAuthenticationCommand>();
    }

    [Fact]
    public void UserInfoCommandShouldSupportCompleteConfiguration()
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiI...",
            UserId = "user-456-789",
            RequestedClaims = ["name", "email", "preferred_username", "given_name", "family_name"],
            IncludeExtendedProfile = true,
            UserInfoSource = "Graph"
        };

        // Act & Assert
        command.AccessToken.ShouldBe("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiI...");
        command.UserId.ShouldBe("user-456-789");
        command.RequestedClaims.ShouldBe(["name", "email", "preferred_username", "given_name", "family_name"]);
        command.IncludeExtendedProfile.ShouldBeTrue();
        command.UserInfoSource.ShouldBe("Graph");
    }

    [Fact]
    public void UserInfoCommandShouldSupportMinimalConfiguration()
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            AccessToken = "simple-token",
            UserInfoSource = "Token"
        };

        // Act & Assert
        command.AccessToken.ShouldBe("simple-token");
        command.UserInfoSource.ShouldBe("Token");
        command.UserId.ShouldBeNull();
        command.RequestedClaims.ShouldBeNull();
        command.IncludeExtendedProfile.ShouldBeFalse();
    }

    [Fact]
    public void UserInfoCommandShouldSupportTokenOnlyMode()
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            AccessToken = "token-based-info",
            UserInfoSource = "Token",
            IncludeExtendedProfile = false
        };

        // Act & Assert
        command.UserInfoSource.ShouldBe("Token");
        command.IncludeExtendedProfile.ShouldBeFalse();
        command.RequestedClaims.ShouldBeNull();
    }

    [Fact]
    public void UserInfoCommandShouldSupportCachedMode()
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            AccessToken = "cached-token",
            UserId = "cached-user-123",
            UserInfoSource = "Cache",
            IncludeExtendedProfile = false
        };

        // Act & Assert
        command.UserInfoSource.ShouldBe("Cache");
        command.UserId.ShouldBe("cached-user-123");
        command.IncludeExtendedProfile.ShouldBeFalse();
    }

    [Fact]
    public void UserInfoCommandShouldSupportExtendedProfileMode()
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            AccessToken = "extended-profile-token",
            UserInfoSource = "Graph",
            IncludeExtendedProfile = true,
            RequestedClaims = ["name", "email", "job_title", "department", "manager"]
        };

        // Act & Assert
        command.UserInfoSource.ShouldBe("Graph");
        command.IncludeExtendedProfile.ShouldBeTrue();
        command.RequestedClaims.ShouldContain("job_title");
        command.RequestedClaims.ShouldContain("department");
        command.RequestedClaims.ShouldContain("manager");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IncludeExtendedProfilePropertyShouldAcceptBothBooleanValues(bool includeExtended)
    {
        // Arrange
        var command = new MockUserInfoCommand
        {
            IncludeExtendedProfile = includeExtended
        };

        // Act & Assert
        command.IncludeExtendedProfile.ShouldBe(includeExtended);
    }

    [Fact]
    public void RequestedClaimsPropertyShouldSupportMultipleClaims()
    {
        // Arrange
        var claims = new[] { "sub", "name", "email", "roles", "permissions", "groups" };
        var command = new MockUserInfoCommand
        {
            RequestedClaims = claims
        };

        // Act & Assert
        command.RequestedClaims.Length.ShouldBe(6);
        command.RequestedClaims.ShouldContain("sub");
        command.RequestedClaims.ShouldContain("roles");
        command.RequestedClaims.ShouldContain("permissions");
        command.RequestedClaims.ShouldContain("groups");
    }
}