using System;
using Shouldly;
using Xunit;
using FractalDataWorks;

namespace FractalDataWorks.Results.Tests;

/// <summary>
/// Tests for FdwResult&lt;T&gt; class.
/// </summary>
public class FdwResultGenericTests
{

    [Fact]
    public void SuccessWithValueCreatesSuccessfulResult()
    {
        // Arrange
        var value = 42;

        // Act
        var result = FdwResult<int>.Success(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeFalse();
        result.IsEmpty.ShouldBeFalse();
        result.Value.ShouldBe(value);
        result.Message.ShouldBe(string.Empty);

        // Output($"Success with value result: IsSuccess={result.IsSuccess}, Value={result.Value}");
    }

    [Fact]
    public void SuccessWithNullValueCreatesSuccessfulResult()
    {
        // Act
        var result = FdwResult<string>.Success(null!);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeFalse();
        result.IsEmpty.ShouldBeFalse();
        result.Value.ShouldBeNull();
        result.Message.ShouldBe(string.Empty);

        // Output($"Success with null value result: IsSuccess={result.IsSuccess}, Value={result.Value}");
    }

    [Fact]
    public void SuccessWithComplexObjectCreatesSuccessfulResult()
    {
        // Arrange
        var value = new TestObject { Id = 1, Name = "Test" };

        // Act
        var result = FdwResult<TestObject>.Success(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeFalse();
        result.IsEmpty.ShouldBeFalse();
        result.Value.ShouldBe(value);
        result.Value.Id.ShouldBe(1);
        result.Value.Name.ShouldBe("Test");

        // Output($"Success with object result: IsSuccess={result.IsSuccess}, Value.Id={result.Value.Id}, Value.Name='{result.Value.Name}'");
    }

    [Fact]
    public void FailureWithStringCreatesFailedResult()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = FdwResult<int>.Failure(errorMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeTrue();
        result.Message.ShouldBe(errorMessage);

        // Output($"Failure result: IsSuccess={result.IsSuccess}, Message='{result.Message}'");
    }

    [Fact]
    public void FailureWithIFdwMessageCreatesFailedResult()
    {
        // Arrange
        var message = new TestMessage("Test error message");

        // Act
        var result = FdwResult<int>.Failure(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeTrue();
        result.Message.ShouldBe("Test error message");

        // Output($"Failure with IFdwMessage result: IsSuccess={result.IsSuccess}, Message='{result.Message}'");
    }

    [Fact]
    public void FailureWithNullIFdwMessageCreatesFailedResultWithEmptyMessage()
    {
        // Act
        var result = FdwResult<int>.Failure((IFdwMessage)null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeTrue();
        result.Message.ShouldBe(string.Empty);

        // Output("Failure with null IFdwMessage correctly handled");
    }

    [Fact]
    public void GenericFailureMethodCreatesFailedResult()
    {
        // Arrange
        var errorMessage = "Generic failure";

        // Act
        var result = FdwResult<string>.Failure<string>(errorMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeTrue();
        result.Message.ShouldBe(errorMessage);

        // Output($"Generic failure result: IsSuccess={result.IsSuccess}, Message='{result.Message}'");
    }

    [Fact]
    public void ValuePropertyThrowsExceptionWhenResultIsFailed()
    {
        // Arrange
        var result = FdwResult<int>.Failure("Error");

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => result.Value);
        exception.Message.ShouldBe("Cannot access value of a failed result.");

        // Output($"Exception thrown correctly: {exception.Message}");
    }

    [Fact]
    public void MapTransformsSuccessfulResult()
    {
        // Arrange
        var original = FdwResult<int>.Success(42);

        // Act
        var mapped = original.Map(x => x.ToString());

        // Assert
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe("42");

        // Output($"Mapped result: IsSuccess={mapped.IsSuccess}, Value='{mapped.Value}'");
    }

    [Fact]
    public void MapPreservesFailedResult()
    {
        // Arrange
        var original = FdwResult<int>.Failure("Error message");

        // Act
        var mapped = original.Map(x => x.ToString());

        // Assert
        mapped.IsSuccess.ShouldBeFalse();
        mapped.Message.ShouldBe("Error message");

        // Output($"Mapped failed result: IsSuccess={mapped.IsSuccess}, Message='{mapped.Message}'");
    }

    [Fact]
    public void MapWithNullMapperThrowsArgumentNullException()
    {
        // Arrange
        var result = FdwResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Map<string>(null!));

        // Output("Map with null mapper correctly threw ArgumentNullException");
    }

    [Fact]
    public void MatchExecutesSuccessPathForSuccessfulResult()
    {
        // Arrange
        var result = FdwResult<int>.Success(42);

        // Act
        var matched = result.Match(
            success: value => $"Success: {value}",
            failure: error => $"Failure: {error}"
        );

        // Assert
        matched.ShouldBe("Success: 42");

        // Output($"Match result: {matched}");
    }

    [Fact]
    public void MatchExecutesFailurePathForFailedResult()
    {
        // Arrange
        var result = FdwResult<int>.Failure("Error occurred");

        // Act
        var matched = result.Match(
            success: value => $"Success: {value}",
            failure: error => $"Failure: {error}"
        );

        // Assert
        matched.ShouldBe("Failure: Error occurred");

        // Output($"Match result: {matched}");
    }

    [Fact]
    public void MatchWithNullSuccessFunctionThrowsArgumentNullException()
    {
        // Arrange
        var result = FdwResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Match(null!, error => ""));

        // Output("Match with null success function correctly threw ArgumentNullException");
    }

    [Fact]
    public void MatchWithNullFailureFunctionThrowsArgumentNullException()
    {
        // Arrange
        var result = FdwResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Match(value => "", null!));

        // Output("Match with null failure function correctly threw ArgumentNullException");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsEmptyReturnsTrueOnlyForFailedResults(bool isSuccess)
    {
        // Arrange & Act
        var result = isSuccess 
            ? FdwResult<int>.Success(42) 
            : FdwResult<int>.Failure("Error");

        // Assert
        result.IsEmpty.ShouldBe(!isSuccess);

        // Output($"IsSuccess: {isSuccess}, IsEmpty: {result.IsEmpty}");
    }

    [Fact]
    public void MapCanTransformBetweenDifferentTypes()
    {
        // Arrange
        var stringResult = FdwResult<string>.Success("123");

        // Act
        var intResult = stringResult.Map(int.Parse);
        var boolResult = intResult.Map(x => x > 100);

        // Assert
        stringResult.IsSuccess.ShouldBeTrue();
        stringResult.Value.ShouldBe("123");
        
        intResult.IsSuccess.ShouldBeTrue();
        intResult.Value.ShouldBe(123);
        
        boolResult.IsSuccess.ShouldBeTrue();
        boolResult.Value.ShouldBeTrue();

        // Output($"String: '{stringResult.Value}' -> Int: {intResult.Value} -> Bool: {boolResult.Value}");
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
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