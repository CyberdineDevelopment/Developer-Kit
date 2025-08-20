using System;
using System.Linq;
using System.Reflection;
using Shouldly;
using Xunit;
using FractalDataWorks;

namespace FractalDataWorks.Results.Tests;

/// <summary>
/// Tests for IGenericResult interface.
/// </summary>
public class IGenericResultTests
{
    [Fact]
    public void IGenericResultShouldHaveExpectedProperties()
    {
        // Arrange
        var resultType = typeof(IGenericResult);

        // Act
        var properties = resultType.GetProperties();

        // Assert
        properties.Length.ShouldBe(3);

        var isSuccessProperty = resultType.GetProperty("IsSuccess");
        isSuccessProperty.ShouldNotBeNull();
        isSuccessProperty!.PropertyType.ShouldBe(typeof(bool));
        isSuccessProperty.CanRead.ShouldBeTrue();
        isSuccessProperty.CanWrite.ShouldBeFalse();

        var isFailureProperty = resultType.GetProperty("IsFailure");
        isFailureProperty.ShouldNotBeNull();
        isFailureProperty!.PropertyType.ShouldBe(typeof(bool));
        isFailureProperty.CanRead.ShouldBeTrue();
        isFailureProperty.CanWrite.ShouldBeFalse();

        var messageProperty = resultType.GetProperty("Message");
        messageProperty.ShouldNotBeNull();
        messageProperty!.PropertyType.ShouldBe(typeof(string));
        messageProperty.CanRead.ShouldBeTrue();
        messageProperty.CanWrite.ShouldBeFalse();
    }

    [Fact]
    public void IGenericResultShouldBePublicInterface()
    {
        // Arrange & Act
        var resultType = typeof(IGenericResult);

        // Assert
        resultType.IsInterface.ShouldBeTrue();
        resultType.IsPublic.ShouldBeTrue();
        resultType.IsAbstract.ShouldBeTrue();
    }

    [Fact]
    public void IGenericResultGenericShouldHaveAdditionalMembers()
    {
        // Arrange
        var genericResultType = typeof(IGenericResult<>);

        // Act
        var properties = genericResultType.GetProperties();
        var methods = genericResultType.GetMethods();

        // Assert
        // Should have Value property (declared directly, others are inherited)
        properties.Length.ShouldBe(1); // Value property only

        var valueProperty = genericResultType.GetProperty("Value");
        valueProperty.ShouldNotBeNull();
        valueProperty!.CanRead.ShouldBeTrue();
        valueProperty.CanWrite.ShouldBeFalse();

        // Should have Map and Match methods
        var mapMethod = methods.FirstOrDefault(m => m.Name == "Map");
        mapMethod.ShouldNotBeNull();

        var matchMethod = methods.FirstOrDefault(m => m.Name == "Match");
        matchMethod.ShouldNotBeNull();
    }

    [Fact]
    public void IGenericResultGenericShouldInheritFromBase()
    {
        // Arrange
        var genericResultType = typeof(IGenericResult<>);

        // Act
        var baseInterfaces = genericResultType.GetInterfaces();

        // Assert
        baseInterfaces.ShouldContain(typeof(IGenericResult));
    }

    [Theory]
    [InlineData(true, "")]
    [InlineData(false, "Error message")]
    [InlineData(false, "Another error")]
    public void IGenericResultImplementationShouldWorkCorrectly(bool isSuccess, string message)
    {
        // Act
        IGenericResult result = isSuccess 
            ? FdwResult.Success() 
            : FdwResult.Failure(message);

        // Assert
        result.IsSuccess.ShouldBe(isSuccess);
        result.IsFailure.ShouldBe(!isSuccess);
        result.Message.ShouldBe(message);
    }

    [Fact]
    public void IGenericResultShouldSupportPolymorphicUsage()
    {
        // Arrange
        IGenericResult[] results = [
            FdwResult.Success(),
            FdwResult.Failure("Error 1"),
            FdwResult<int>.Success(42),
            FdwResult<string>.Failure("Error 2")
        ];

        // Act & Assert
        foreach (var result in results)
        {
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeOfType<bool>();
            result.IsFailure.ShouldBeOfType<bool>();
            result.Message.ShouldNotBeNull();
            
            // IsFailure should always be opposite of IsSuccess
            result.IsFailure.ShouldBe(!result.IsSuccess);
        }

        results[0].IsSuccess.ShouldBeTrue();
        results[1].IsSuccess.ShouldBeFalse();
        results[2].IsSuccess.ShouldBeTrue();
        results[3].IsSuccess.ShouldBeFalse();
    }

    [Theory]
    [InlineData(42)]
    [InlineData("test string")]
    [InlineData(null)]
    public void IGenericResultGenericImplementationShouldWorkCorrectly<T>(T value)
    {
        // Arrange
        var isSuccess = value != null;

        // Act
        IGenericResult<T> result = isSuccess 
            ? FdwResult<T>.Success(value) 
            : FdwResult<T>.Failure("Value was null");

        // Assert
        result.IsSuccess.ShouldBe(isSuccess);
        result.IsFailure.ShouldBe(!isSuccess);

        if (isSuccess)
        {
            result.Value.ShouldBe(value);
            result.Message.ShouldBe(string.Empty);
        }
        else
        {
            Should.Throw<InvalidOperationException>(() => result.Value);
            result.Message.ShouldBe("Value was null");
        }
    }

    [Fact]
    public void IGenericResultGenericMapShouldTransformValue()
    {
        // Arrange
        IGenericResult<int> successResult = FdwResult<int>.Success(42);
        IGenericResult<int> failureResult = FdwResult<int>.Failure("Error");

        // Act
        var mappedSuccess = successResult.Map(x => $"Number: {x}");
        var mappedFailure = failureResult.Map(x => $"Number: {x}");

        // Assert
        mappedSuccess.IsSuccess.ShouldBeTrue();
        mappedSuccess.Value.ShouldBe("Number: 42");

        mappedFailure.IsSuccess.ShouldBeFalse();
        mappedFailure.Message.ShouldBe("Error");
    }

    [Fact]
    public void IGenericResultGenericMapShouldThrowForNullMapper()
    {
        // Arrange
        IGenericResult<int> result = FdwResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Map<string>(null!));
    }

    [Fact]
    public void IGenericResultGenericMatchShouldExecuteCorrectFunction()
    {
        // Arrange
        IGenericResult<int> successResult = FdwResult<int>.Success(42);
        IGenericResult<int> failureResult = FdwResult<int>.Failure("Error");

        // Act
        var successMatch = successResult.Match(
            success: x => $"Got value: {x}",
            failure: err => $"Got error: {err}"
        );
        
        var failureMatch = failureResult.Match(
            success: x => $"Got value: {x}",
            failure: err => $"Got error: {err}"
        );

        // Assert
        successMatch.ShouldBe("Got value: 42");
        failureMatch.ShouldBe("Got error: Error");
    }

    [Fact]
    public void IGenericResultGenericMatchShouldThrowForNullFunctions()
    {
        // Arrange
        IGenericResult<int> result = FdwResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Match(null!, err => ""));
        Should.Throw<ArgumentNullException>(() => result.Match(val => "", null!));
    }

    [Fact]
    public void IGenericResultGenericShouldWorkWithValueTypes()
    {
        // Arrange & Act
        IGenericResult<DateTime> dateResult = FdwResult<DateTime>.Success(DateTime.Today);
        IGenericResult<bool> boolResult = FdwResult<bool>.Success(true);
        IGenericResult<decimal> decimalResult = FdwResult<decimal>.Success(123.45m);

        // Assert
        dateResult.IsSuccess.ShouldBeTrue();
        dateResult.Value.ShouldBe(DateTime.Today);

        boolResult.IsSuccess.ShouldBeTrue();
        boolResult.Value.ShouldBeTrue();

        decimalResult.IsSuccess.ShouldBeTrue();
        decimalResult.Value.ShouldBe(123.45m);
    }

    [Fact]
    public void IGenericResultGenericShouldWorkWithReferenceTypes()
    {
        // Arrange
        var testObject = new TestReferenceType { Id = 1, Name = "Test" };

        // Act
        IGenericResult<TestReferenceType> result = FdwResult<TestReferenceType>.Success(testObject);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(testObject);
        result.Value.Id.ShouldBe(1);
        result.Value.Name.ShouldBe("Test");
    }

    [Fact]
    public void IGenericResultGenericShouldSupportComplexMappingChains()
    {
        // Arrange
        IGenericResult<string> initialResult = FdwResult<string>.Success("Hello World");

        // Act
        var finalResult = initialResult
            .Map(s => s.Length)          // string -> int
            .Map(i => i > 5)             // int -> bool
            .Map(b => b ? "Long" : "Short"); // bool -> string

        // Assert
        finalResult.IsSuccess.ShouldBeTrue();
        finalResult.Value.ShouldBe("Long"); // "Hello World" has 11 chars, > 5, so "Long"
    }

    [Fact]
    public void IGenericResultGenericShouldHandleExceptionsDuringMapping()
    {
        // Arrange
        IGenericResult<string> result = FdwResult<string>.Success("not a number");

        // Act & Assert
        // The Map method itself shouldn't throw - it should let the exception bubble up
        // or depend on implementation. Let's test that it correctly handles the mapping function.
        Should.Throw<FormatException>(() => result.Map(int.Parse));
    }

    private class TestReferenceType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}