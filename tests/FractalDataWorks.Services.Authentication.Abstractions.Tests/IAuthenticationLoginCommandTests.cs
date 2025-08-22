using System;
using System.Collections.Generic;
using FluentValidation.Results;
using FractalDataWorks.Services.Authentication.Abstractions.Commands;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

/// <summary>
/// Tests for IAuthenticationLoginCommand interface contract verification.
/// </summary>
public sealed class IAuthenticationLoginCommandTests
{
    /// <summary>
    /// Mock implementation of IAuthenticationLoginCommand for testing.
    /// </summary>
    private sealed class MockAuthenticationLoginCommand : IAuthenticationLoginCommand
    {
        public string Username { get; set; } = string.Empty;
        public string FlowType { get; set; } = string.Empty;
        public string[]? AdditionalScopes { get; set; }
        public IReadOnlyDictionary<string, string>? ExtraQueryParameters { get; set; }
        public string? LoginHint { get; set; }
        public string? Prompt { get; set; }
        
        // ICommand implementation
        public Guid CommandId { get; set; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public IFdwConfiguration? Configuration { get; set; }
        
        public ValidationResult Validate() => new();
    }

    [Fact]
    public void UsernamePropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedUsername = "testuser@company.com";
        var command = new MockAuthenticationLoginCommand
        {
            Username = expectedUsername
        };

        // Act & Assert
        command.Username.ShouldBe(expectedUsername);
    }

    [Fact]
    public void FlowTypePropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedFlowType = "Interactive";
        var command = new MockAuthenticationLoginCommand
        {
            FlowType = expectedFlowType
        };

        // Act & Assert
        command.FlowType.ShouldBe(expectedFlowType);
    }

    [Theory]
    [InlineData("Interactive")]
    [InlineData("Silent")]
    [InlineData("ClientCredentials")]
    [InlineData("DeviceCode")]
    public void FlowTypePropertyShouldAcceptValidFlowTypes(string flowType)
    {
        // Arrange
        var command = new MockAuthenticationLoginCommand
        {
            FlowType = flowType
        };

        // Act & Assert
        command.FlowType.ShouldBe(flowType);
    }

    [Fact]
    public void AdditionalScopesPropertyShouldBeAccessible()
    {
        // Arrange
        var expectedScopes = new[] { "mail.read", "calendars.read" };
        var command = new MockAuthenticationLoginCommand
        {
            AdditionalScopes = expectedScopes
        };

        // Act & Assert
        command.AdditionalScopes.ShouldBe(expectedScopes);
    }

    [Fact]
    public void AdditionalScopesPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockAuthenticationLoginCommand
        {
            AdditionalScopes = null
        };

        // Act & Assert
        command.AdditionalScopes.ShouldBeNull();
    }

    [Fact]
    public void AdditionalScopesPropertyShouldAcceptEmptyArray()
    {
        // Arrange
        var command = new MockAuthenticationLoginCommand
        {
            AdditionalScopes = []
        };

        // Act & Assert
        command.AdditionalScopes.ShouldBeEmpty();
    }

    [Fact]
    public void ExtraQueryParametersPropertyShouldBeAccessible()
    {
        // Arrange
        var expectedParams = new Dictionary<string, string>
        {
            { "domain_hint", "organizations" },
            { "prompt", "select_account" }
        };
        var command = new MockAuthenticationLoginCommand
        {
            ExtraQueryParameters = expectedParams
        };

        // Act & Assert
        command.ExtraQueryParameters.ShouldBe(expectedParams);
    }

    [Fact]
    public void ExtraQueryParametersPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockAuthenticationLoginCommand
        {
            ExtraQueryParameters = null
        };

        // Act & Assert
        command.ExtraQueryParameters.ShouldBeNull();
    }

    [Fact]
    public void ExtraQueryParametersPropertyShouldAcceptEmptyDictionary()
    {
        // Arrange
        var emptyParams = new Dictionary<string, string>();
        var command = new MockAuthenticationLoginCommand
        {
            ExtraQueryParameters = emptyParams
        };

        // Act & Assert
        command.ExtraQueryParameters.ShouldBeEmpty();
    }

    [Fact]
    public void LoginHintPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedLoginHint = "user@domain.com";
        var command = new MockAuthenticationLoginCommand
        {
            LoginHint = expectedLoginHint
        };

        // Act & Assert
        command.LoginHint.ShouldBe(expectedLoginHint);
    }

    [Fact]
    public void LoginHintPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockAuthenticationLoginCommand
        {
            LoginHint = null
        };

        // Act & Assert
        command.LoginHint.ShouldBeNull();
    }

    [Fact]
    public void PromptPropertyShouldBeAccessible()
    {
        // Arrange
        const string expectedPrompt = "select_account";
        var command = new MockAuthenticationLoginCommand
        {
            Prompt = expectedPrompt
        };

        // Act & Assert
        command.Prompt.ShouldBe(expectedPrompt);
    }

    [Theory]
    [InlineData("select_account")]
    [InlineData("login")]
    [InlineData("consent")]
    [InlineData("none")]
    public void PromptPropertyShouldAcceptValidPromptValues(string prompt)
    {
        // Arrange
        var command = new MockAuthenticationLoginCommand
        {
            Prompt = prompt
        };

        // Act & Assert
        command.Prompt.ShouldBe(prompt);
    }

    [Fact]
    public void PromptPropertyShouldAcceptNull()
    {
        // Arrange
        var command = new MockAuthenticationLoginCommand
        {
            Prompt = null
        };

        // Act & Assert
        command.Prompt.ShouldBeNull();
    }

    [Fact]
    public void LoginCommandShouldInheritFromIAuthenticationCommand()
    {
        // Arrange
        var command = new MockAuthenticationLoginCommand();

        // Act & Assert
        command.ShouldBeAssignableTo<IAuthenticationLoginCommand>();
        command.ShouldBeAssignableTo<IAuthenticationCommand>();
    }

    [Fact]
    public void LoginCommandShouldSupportCompleteConfiguration()
    {
        // Arrange
        var extraParams = new Dictionary<string, string>
        {
            { "domain_hint", "organizations" },
            { "response_mode", "form_post" }
        };

        var command = new MockAuthenticationLoginCommand
        {
            Username = "admin@contoso.com",
            FlowType = "Interactive",
            AdditionalScopes = ["mail.send", "files.read"],
            ExtraQueryParameters = extraParams,
            LoginHint = "admin@contoso.com",
            Prompt = "select_account"
        };

        // Act & Assert
        command.Username.ShouldBe("admin@contoso.com");
        command.FlowType.ShouldBe("Interactive");
        command.AdditionalScopes.ShouldBe(["mail.send", "files.read"]);
        command.ExtraQueryParameters.ShouldBe(extraParams);
        command.LoginHint.ShouldBe("admin@contoso.com");
        command.Prompt.ShouldBe("select_account");
    }

    [Fact]
    public void LoginCommandShouldSupportMinimalConfiguration()
    {
        // Arrange
        var command = new MockAuthenticationLoginCommand
        {
            Username = "user@domain.com",
            FlowType = "Silent"
        };

        // Act & Assert
        command.Username.ShouldBe("user@domain.com");
        command.FlowType.ShouldBe("Silent");
        command.AdditionalScopes.ShouldBeNull();
        command.ExtraQueryParameters.ShouldBeNull();
        command.LoginHint.ShouldBeNull();
        command.Prompt.ShouldBeNull();
    }
}