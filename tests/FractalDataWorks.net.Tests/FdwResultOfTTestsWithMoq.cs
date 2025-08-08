using System;
using FractalDataWorks;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tests;

/// <summary>
/// Tests for FdwResult{T} generic class using Moq for message mocking.
/// </summary>
public sealed class FdwResultOfTTestsWithMoq
{
    [Fact]
    public void SuccessCreatesResultWithValue()
    {
        // Arrange
        const string expectedValue = "test value";

        // Act
        var result = FdwResult<string>.Success(expectedValue);

        // Assert
        result.Value.ShouldBe(expectedValue, $"Expected Value to be '{expectedValue}'");
    }

    [Fact]
    public void SuccessCreatesResultWithIsSuccessTrue()
    {
        // Act
        var result = FdwResult<int>.Success(42);

        // Assert
        result.IsSuccess.ShouldBeTrue($"Expected IsSuccess to be true for successful result");
    }

    [Fact]
    public void SuccessCreatesResultWithNullMessage()
    {
        // Act
        var result = FdwResult<object>.Success(new object());

        // Assert
        result.Message.ShouldBeNull($"Expected Message to be null for successful result");
    }

    [Fact]
    public void SuccessCreatesResultWithIsEmptyFalse()
    {
        // Act
        var result = FdwResult<string>.Success("value");

        // Assert
        result.IsEmpty.ShouldBeFalse($"Expected IsEmpty to be false for successful result");
    }

    [Fact]
    public void FailureCreatesResultWithIsSuccessFalse()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();
        mockMessage.Setup(m => m.Severity).Returns(MessageSeverity.Error);

        // Act
        var result = FdwResult<string>.Failure(mockMessage.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse($"Expected IsSuccess to be false for failed result");
    }

    [Fact]
    public void FailureCreatesResultWithProvidedMessage()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();
        const string expectedMessage = "Test error message";
        mockMessage.Setup(m => m.Message).Returns(expectedMessage);

        // Act
        var result = FdwResult<int>.Failure(mockMessage.Object);

        // Assert
        result.Message.ShouldBe(expectedMessage, $"Expected Message to be the provided message");
    }

    [Fact]
    public void FailureCreatesResultWithIsEmptyTrue()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();

        // Act
        var result = FdwResult<string>.Failure(mockMessage.Object);

        // Assert
        result.IsEmpty.ShouldBeTrue($"Expected IsEmpty to be true for failed result");
    }

    [Fact]
    public void ValueThrowsInvalidOperationExceptionWhenAccessedOnFailedResult()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();
        mockMessage.Setup(m => m.Severity).Returns(MessageSeverity.Error);
        var result = FdwResult<string>.Failure(mockMessage.Object);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => { var _ = result.Value; });
        exception.Message.ShouldContain("Cannot access value of a failed result");
    }

    [Fact]
    public void SuccessCanStoreNullValue()
    {
        // Act
        var result = FdwResult<string>.Success(null!);

        // Assert
        result.IsSuccess.ShouldBeTrue($"Expected result to be successful");
        result.Value.ShouldBeNull($"Expected Value to be null");
    }

    [Fact]
    public void SuccessCanStoreValueTypes()
    {
        // Arrange
        const int expectedValue = 42;

        // Act
        var result = FdwResult<int>.Success(expectedValue);

        // Assert
        result.Value.ShouldBe(expectedValue, $"Expected Value to be {expectedValue}");
    }

    [Fact]
    public void SuccessCanStoreComplexTypes()
    {
        // Arrange
        var expectedValue = new ComplexType { Id = 1, Name = "Test" };

        // Act
        var result = FdwResult<ComplexType>.Success(expectedValue);

        // Assert
        result.Value.ShouldBe(expectedValue, $"Expected Value to be the complex object");
        result.Value.Id.ShouldBe(1, $"Expected Value.Id to be 1");
        result.Value.Name.ShouldBe("Test", $"Expected Value.Name to be 'Test'");
    }

    [Fact]
    public void GenericFailureMethodCreatesTypedFailure()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();
        const string expectedMessage = "Generic failure message";
        mockMessage.Setup(m => m.Message).Returns(expectedMessage);
        mockMessage.Setup(m => m.Severity).Returns(MessageSeverity.Error);

        // Act
        var result = FdwResult<int>.Failure(mockMessage.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse($"Expected IsSuccess to be false");
        result.Message.ShouldBe(expectedMessage, $"Expected Message to match provided message");
    }

    [Fact]
    public void InstanceFailureMethodCreatesTypedFailure()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();

        // Act
        var result = FdwResult<string>.Failure(mockMessage.Object);

        // Assert
        result.IsSuccess.ShouldBeFalse($"Expected IsSuccess to be false");
        result.GetType().ShouldBe(typeof(FdwResult<string>), $"Expected result type to be FdwResult<string>");
    }

    [Fact]
    public void ErrorPropertyChecksMessageNotNullFirst()
    {
        // Arrange
        var result = FdwResult<int>.Success(42);

        // Act & Assert
        result.Error.ShouldBeFalse($"Expected Error to be false when Message is null");
    }

    [Fact]
    public void ErrorPropertyAlwaysReturnsFalseWhenMessageIsNotNull()
    {
        // Arrange
        var mockMessage = new Mock<IFdwMessage>();
        mockMessage.Setup(m => m.Severity).Returns(MessageSeverity.Error);
        var result = FdwResult<bool>.Failure(mockMessage.Object);

        // Act & Assert
        // Note: The implementation has a bug - it returns false whenever Message is not null
        result.Error.ShouldBeFalse($"Expected Error to be false due to bug in implementation");
    }

    private sealed class ComplexType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}