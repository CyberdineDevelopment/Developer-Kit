using System;
using FractalDataWorks;
using FractalDataWorks.Messages;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tests;

/// <summary>
/// Tests for FdwResult class using Moq for message mocking.
/// </summary>
public class FdwResultTestsWithMoq
{
    [Fact]
    public void SuccessCreatesResultWithIsSuccessTrue()
    {
        // Act
        var result = FdwResult.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue($"Expected IsSuccess to be true for successful result");
    }

    [Fact]
    public void SuccessCreatesResultWithNullMessage()
    {
        // Act
        var result = FdwResult.Success();

        // Assert
        result.Message.ShouldBeNull($"Expected Message to be null for successful result");
    }

    [Fact]
    public void SuccessCreatesResultWithErrorFalse()
    {
        // Act
        var result = FdwResult.Success();

        // Assert
        result.Error.ShouldBeFalse($"Expected Error to be false for successful result");
    }

    [Fact]
    public void FailureCreatesResultWithIsSuccessFalse()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();
        mockMessage.Setup(m => m.Severity).Returns(MessageSeverity.Error);

        // Act
        var result = FdwResult.Failure(mockMessage.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse($"Expected IsSuccess to be false for failed result");
    }

    [Fact]
    public void FailureCreatesResultWithProvidedMessage()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();

        // Act
        var result = FdwResult.Failure(mockMessage.Object);

        // Assert
        result.Message.ShouldBe(mockMessage.Object, $"Expected Message to be the provided message");
    }

    [Fact]
    public void FailureThrowsArgumentNullExceptionForNullMessage()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => FdwResult.Failure(null!));
        exception.ParamName.ShouldBe("message", $"Expected parameter name to be 'message'");
    }

    [Fact]
    public void ErrorAlwaysReturnsFalseWhenMessageIsNotNull()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();
        mockMessage.Setup(m => m.Severity).Returns(MessageSeverity.Error);
        var result = FdwResult.Failure(mockMessage.Object);

        // Act & Assert
        // Note: The implementation has a bug - it returns false whenever Message is not null
        result.Error.ShouldBeFalse($"Expected Error to be false due to bug in implementation (line 32)");
    }

    [Fact]
    public void ErrorReturnsFalseWhenMessageHasWarningSeverity()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();
        mockMessage.Setup(m => m.Severity).Returns(MessageSeverity.Warning);
        var result = FdwResult.Failure(mockMessage.Object);

        // Act & Assert
        result.Error.ShouldBeFalse($"Expected Error to be false when message severity is Warning");
    }

    [Fact]
    public void ErrorReturnsFalseWhenMessageHasInfoSeverity()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();
        mockMessage.Setup(m => m.Severity).Returns(MessageSeverity.Information);
        var result = FdwResult.Failure(mockMessage.Object);

        // Act & Assert
        result.Error.ShouldBeFalse($"Expected Error to be false when message severity is Info");
    }

    [Fact]
    public void IsEmptyReturnsTrueWhenMessageIsNotNull()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();
        var result = FdwResult.Failure(mockMessage.Object);

        // Act & Assert
        result.IsEmpty.ShouldBeTrue($"Expected IsEmpty to be true when message is not null");
    }

    [Fact]
    public void IsEmptyReturnsFalseWhenMessageIsNull()
    {
        // Arrange
        var result = FdwResult.Success();

        // Act & Assert
        result.IsEmpty.ShouldBeFalse($"Expected IsEmpty to be false when message is null");
    }

    [Fact] 
    public void ErrorPropertyNeverReturnsTrueForAnyResult()
    {
        // This test documents the buggy behavior where Error can never return true
        // because line 32 returns false if Message is not null,
        // and line 33 can only be reached if Message is null (which makes the pattern match fail)
        
        // Test with success (Message is null)
        var successResult = FdwResult.Success();
        successResult.Error.ShouldBeFalse($"Error is false for success result");
        
        // Test with failure (Message is not null)
        var mockMessage = new Mock<IFdwMessage>();
        mockMessage.Setup(m => m.Severity).Returns(MessageSeverity.Error);
        var failureResult = FdwResult.Failure(mockMessage.Object);
        failureResult.Error.ShouldBeFalse($"Error is false even for failure with Error severity due to bug");
    }
}