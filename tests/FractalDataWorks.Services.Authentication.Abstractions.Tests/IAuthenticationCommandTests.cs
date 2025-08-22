using System;
using FluentValidation.Results;
using FractalDataWorks.Services.Authentication.Abstractions;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

/// <summary>
/// Tests for IAuthenticationCommand interface contract verification.
/// </summary>
public sealed class IAuthenticationCommandTests
{
    /// <summary>
    /// Mock implementation of IAuthenticationCommand for testing.
    /// </summary>
    private sealed class MockAuthenticationCommand : IAuthenticationCommand
    {
        public Guid CommandId { get; set; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public IFdwConfiguration? Configuration { get; set; }
        
        public ValidationResult Validate() => new();
    }

    [Fact]
    public void MockAuthenticationCommandShouldImplementIAuthenticationCommand()
    {
        // Arrange & Act
        var command = new MockAuthenticationCommand();

        // Assert
        command.ShouldBeAssignableTo<IAuthenticationCommand>();
    }

    [Fact]
    public void IAuthenticationCommandShouldInheritFromICommand()
    {
        // Arrange
        var command = new MockAuthenticationCommand();

        // Act & Assert
        command.ShouldBeAssignableTo<ICommand>();
    }

    [Fact]
    public void CanCreateMultipleInstancesOfAuthenticationCommand()
    {
        // Arrange & Act
        var command1 = new MockAuthenticationCommand();
        var command2 = new MockAuthenticationCommand();

        // Assert
        command1.ShouldNotBeSameAs(command2);
        command1.ShouldBeAssignableTo<IAuthenticationCommand>();
        command2.ShouldBeAssignableTo<IAuthenticationCommand>();
    }

    [Fact]
    public void AuthenticationCommandShouldBeReferenceType()
    {
        // Arrange & Act
        var command = new MockAuthenticationCommand();

        // Assert
        command.ShouldNotBeNull();
        command.GetType().IsClass.ShouldBeTrue();
    }
}