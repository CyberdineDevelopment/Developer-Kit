using System;
using Shouldly;
using Xunit;
using FractalDataWorks;

namespace FractalDataWorks.Results.Tests;

/// <summary>
/// Tests for FdwResult class.
/// </summary>
public class FdwResultTests
{
    [Fact]
    public void SuccessCreatesSuccessfulResult()
    {
        // Act
        var result = FdwResult.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeFalse();
        result.IsEmpty.ShouldBeTrue();
        result.Message.ShouldBe(string.Empty);
    }

    [Fact]
    public void FailureWithStringCreatesFailedResult()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = FdwResult.Failure(errorMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.Message.ShouldBe(errorMessage);
    }

    [Fact]
    public void FailureWithNullMessageCreatesFailedResultWithEmptyMessage()
    {
        // Act
        var result = FdwResult.Failure((string)null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeTrue();
        result.Message.ShouldBe(string.Empty);
    }

    [Fact]
    public void FailureWithEmptyStringCreatesFailedResultWithEmptyMessage()
    {
        // Act
        var result = FdwResult.Failure(string.Empty);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeTrue();
        result.Message.ShouldBe(string.Empty);
    }

    [Fact]
    public void FailureWithIFdwMessageCreatesFailedResult()
    {
        // Arrange
        var message = new TestMessage("Test error message");

        // Act
        var result = FdwResult.Failure(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.Message.ShouldBe("Test error message");
    }

    [Fact]
    public void FailureWithNullIFdwMessageCreatesFailedResultWithEmptyMessage()
    {
        // Act
        var result = FdwResult.Failure((IFdwMessage)null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeTrue();
        result.Message.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("Error message")]
    [InlineData("")]
    [InlineData("   ")]
    public void IsEmptyReturnsTrueWhenMessageIsNullOrEmpty(string message)
    {
        // Act
        var result = FdwResult.Failure(message);

        // Assert
        var expectedIsEmpty = string.IsNullOrEmpty(message);
        result.IsEmpty.ShouldBe(expectedIsEmpty);
    }

    [Fact]
    public void ErrorPropertyReturnsOppositeOfIsSuccess()
    {
        // Arrange & Act
        var successResult = FdwResult.Success();
        var failureResult = FdwResult.Failure("error");

        // Assert
        successResult.Error.ShouldBe(!successResult.IsSuccess);
        failureResult.Error.ShouldBe(!failureResult.IsSuccess);
    }

    [Fact]
    public void IsFailurePropertyReturnsOppositeOfIsSuccess()
    {
        // Arrange & Act
        var successResult = FdwResult.Success();
        var failureResult = FdwResult.Failure("error");

        // Assert
        successResult.IsFailure.ShouldBe(!successResult.IsSuccess);
        failureResult.IsFailure.ShouldBe(!failureResult.IsSuccess);
    }

    private class TestMessage : IFdwMessage
    {
        public TestMessage(string message)
        {
            Message = message;
        }

        public MessageSeverity Severity => MessageSeverity.Error;
        public string Message { get; }
        public string? Code => null;
        public string? Source => null;
    }
}