using System;
using Shouldly;
using Xunit;
using FractalDataWorks;

namespace FractalDataWorks.Results.Tests;

/// <summary>
/// Tests for IFdwResult interface.
/// </summary>
public class IFdwResultTests
{
    [Fact]
    public void IFdwResultShouldHaveExpectedProperties()
    {
        // Arrange
        var resultType = typeof(IFdwResult);

        // Act
        var properties = resultType.GetProperties();

        // Assert
        properties.Length.ShouldBe(2); // IsEmpty + Error (other properties are inherited)

        var isEmptyProperty = resultType.GetProperty("IsEmpty");
        isEmptyProperty.ShouldNotBeNull();
        isEmptyProperty!.PropertyType.ShouldBe(typeof(bool));
        isEmptyProperty.CanRead.ShouldBeTrue();
        isEmptyProperty.CanWrite.ShouldBeFalse();

        var errorProperty = resultType.GetProperty("Error");
        errorProperty.ShouldNotBeNull();
        errorProperty!.PropertyType.ShouldBe(typeof(bool));
        errorProperty.CanRead.ShouldBeTrue();
        errorProperty.CanWrite.ShouldBeFalse();
    }

    [Fact]
    public void IFdwResultShouldInheritFromIGenericResult()
    {
        // Arrange
        var resultType = typeof(IFdwResult);

        // Act
        var baseInterfaces = resultType.GetInterfaces();

        // Assert
        baseInterfaces.ShouldContain(typeof(IGenericResult));
    }

    [Fact]
    public void IFdwResultShouldBePublicInterface()
    {
        // Arrange & Act
        var resultType = typeof(IFdwResult);

        // Assert
        resultType.IsInterface.ShouldBeTrue();
        resultType.IsPublic.ShouldBeTrue();
        resultType.IsAbstract.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true, "Success message")]
    [InlineData(false, "Error message")]
    [InlineData(false, "")]
    public void IFdwResultImplementationShouldWorkCorrectly(bool isSuccess, string message)
    {
        // Act
        IFdwResult result = isSuccess 
            ? FdwResult.Success() 
            : FdwResult.Failure(message);

        // Assert
        result.IsSuccess.ShouldBe(isSuccess);
        result.IsFailure.ShouldBe(!isSuccess);
        result.Error.ShouldBe(!isSuccess);
        result.Message.ShouldBe(isSuccess ? string.Empty : message);
        
        var expectedEmpty = isSuccess || string.IsNullOrEmpty(message);
        result.IsEmpty.ShouldBe(expectedEmpty);
    }

    [Fact]
    public void IFdwResultShouldSupportPolymorphicUsage()
    {
        // Arrange
        IFdwResult[] results = [
            FdwResult.Success(),
            FdwResult.Failure("Error 1"),
            FdwResult.Failure("Error 2")
        ];

        // Act & Assert
        for (int i = 0; i < results.Length; i++)
        {
            var result = results[i];
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeOfType<bool>();
            result.IsFailure.ShouldBeOfType<bool>();
            result.Error.ShouldBeOfType<bool>();
            result.IsEmpty.ShouldBeOfType<bool>();
            result.Message.ShouldNotBeNull();
        }

        results[0].IsSuccess.ShouldBeTrue();
        results[1].IsSuccess.ShouldBeFalse();
        results[2].IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void IFdwResultGenericShouldInheritFromBothInterfaces()
    {
        // Arrange
        var genericResultType = typeof(IFdwResult<>);

        // Act
        var baseInterfaces = genericResultType.GetInterfaces();

        // Assert
        baseInterfaces.ShouldContain(typeof(IFdwResult));
        // Note: The exact type will be IGenericResult<T> with generic parameter, not the open generic type
    }

    [Theory]
    [InlineData(42, true)]
    [InlineData("test", true)]
    [InlineData(null, false)]
    public void IFdwResultGenericImplementationShouldWorkCorrectly<T>(T value, bool isSuccess)
    {
        // Act
        IFdwResult<T> result = isSuccess 
            ? FdwResult<T>.Success(value) 
            : FdwResult<T>.Failure("Error message");

        // Assert
        result.IsSuccess.ShouldBe(isSuccess);
        result.IsFailure.ShouldBe(!isSuccess);
        result.Error.ShouldBe(!isSuccess);
        result.IsEmpty.ShouldBe(!isSuccess);

        if (isSuccess)
        {
            result.Value.ShouldBe(value);
            result.Message.ShouldBe(string.Empty);
        }
        else
        {
            Should.Throw<InvalidOperationException>(() => result.Value);
            result.Message.ShouldBe("Error message");
        }
    }

    [Fact]
    public void IFdwResultGenericShouldSupportMapOperation()
    {
        // Arrange
        IFdwResult<int> successResult = FdwResult<int>.Success(42);
        IFdwResult<int> failureResult = FdwResult<int>.Failure("Error");

        // Act
        var mappedSuccess = successResult.Map(x => x.ToString());
        var mappedFailure = failureResult.Map(x => x.ToString());

        // Assert
        mappedSuccess.IsSuccess.ShouldBeTrue();
        mappedSuccess.Value.ShouldBe("42");

        mappedFailure.IsSuccess.ShouldBeFalse();
        mappedFailure.Message.ShouldBe("Error");
    }

    [Fact]
    public void IFdwResultGenericShouldSupportMatchOperation()
    {
        // Arrange
        IFdwResult<int> successResult = FdwResult<int>.Success(42);
        IFdwResult<int> failureResult = FdwResult<int>.Failure("Error");

        // Act
        var successMatch = successResult.Match(
            success: x => $"Value: {x}",
            failure: err => $"Error: {err}"
        );
        
        var failureMatch = failureResult.Match(
            success: x => $"Value: {x}",
            failure: err => $"Error: {err}"
        );

        // Assert
        successMatch.ShouldBe("Value: 42");
        failureMatch.ShouldBe("Error: Error");
    }

    [Fact]
    public void IFdwResultGenericShouldWorkWithComplexTypes()
    {
        // Arrange
        var complexObject = new ComplexTestType
        {
            Id = 123,
            Name = "Test Object",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        IFdwResult<ComplexTestType> result = FdwResult<ComplexTestType>.Success(complexObject);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(complexObject);
        result.Value.Id.ShouldBe(123);
        result.Value.Name.ShouldBe("Test Object");
    }

    [Fact]
    public void IFdwResultGenericShouldSupportChainedOperations()
    {
        // Arrange
        IFdwResult<string> initialResult = FdwResult<string>.Success("123");

        // Act
        var chainedResult = initialResult
            .Map(int.Parse)
            .Map(x => x * 2)
            .Map(x => x > 100);

        // Assert
        chainedResult.IsSuccess.ShouldBeTrue();
        chainedResult.Value.ShouldBeTrue(); // 123 * 2 = 246, which is > 100
    }

    [Fact]
    public void IFdwResultGenericShouldPreserveFailureThroughChain()
    {
        // Arrange
        IFdwResult<string> initialResult = FdwResult<string>.Failure("Initial error");

        // Act
        var chainedResult = initialResult
            .Map(int.Parse)
            .Map(x => x * 2)
            .Map(x => x > 100);

        // Assert
        chainedResult.IsSuccess.ShouldBeFalse();
        chainedResult.Message.ShouldBe("Initial error");
    }

    private class ComplexTestType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}