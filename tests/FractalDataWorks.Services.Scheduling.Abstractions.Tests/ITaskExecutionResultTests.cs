using FractalDataWorks.Services.Scheduling.Abstractions;
using Moq;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests;

public sealed class ITaskExecutionResultTests
{
    [Fact]
    public void TaskExecutionResultShouldHaveRequiredProperties()
    {
        // Arrange
        var mockResult = new Mock<ITaskExecutionResult>();
        mockResult.Setup(x => x.IsSuccessful).Returns(true);
        mockResult.Setup(x => x.Result).Returns("execution completed successfully");
        mockResult.Setup(x => x.ErrorMessage).Returns((string?)null);

        var result = mockResult.Object;
        
        // Act & Assert
        result.IsSuccessful.ShouldBeTrue();
        result.Result.ShouldBe("execution completed successfully");
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void TaskExecutionResultShouldHandleSuccessfulExecution()
    {
        // Arrange
        var mockResult = new Mock<ITaskExecutionResult>();
        var resultData = new { processed = 100, status = "complete" };
        
        mockResult.Setup(x => x.IsSuccessful).Returns(true);
        mockResult.Setup(x => x.Result).Returns(resultData);
        mockResult.Setup(x => x.ErrorMessage).Returns((string?)null);

        var result = mockResult.Object;
        
        // Act & Assert
        result.IsSuccessful.ShouldBeTrue();
        result.Result.ShouldBe(resultData);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void TaskExecutionResultShouldHandleFailedExecution()
    {
        // Arrange
        var mockResult = new Mock<ITaskExecutionResult>();
        
        mockResult.Setup(x => x.IsSuccessful).Returns(false);
        mockResult.Setup(x => x.Result).Returns((object?)null);
        mockResult.Setup(x => x.ErrorMessage).Returns("Task execution failed due to timeout");

        var result = mockResult.Object;
        
        // Act & Assert
        result.IsSuccessful.ShouldBeFalse();
        result.Result.ShouldBeNull();
        result.ErrorMessage.ShouldBe("Task execution failed due to timeout");
    }

    [Fact]
    public void TaskExecutionResultShouldAllowNullResult()
    {
        // Arrange
        var mockResult = new Mock<ITaskExecutionResult>();
        
        mockResult.Setup(x => x.IsSuccessful).Returns(true);
        mockResult.Setup(x => x.Result).Returns((object?)null);
        mockResult.Setup(x => x.ErrorMessage).Returns((string?)null);

        var result = mockResult.Object;
        
        // Act & Assert
        result.IsSuccessful.ShouldBeTrue();
        result.Result.ShouldBeNull();
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void TaskExecutionResultShouldSupportDifferentResultTypes()
    {
        // Arrange - String result
        var mockStringResult = new Mock<ITaskExecutionResult>();
        mockStringResult.Setup(x => x.IsSuccessful).Returns(true);
        mockStringResult.Setup(x => x.Result).Returns("string result");

        // Arrange - Numeric result
        var mockNumericResult = new Mock<ITaskExecutionResult>();
        mockNumericResult.Setup(x => x.IsSuccessful).Returns(true);
        mockNumericResult.Setup(x => x.Result).Returns(42);

        // Arrange - Complex object result
        var complexResult = new { id = 123, name = "test", values = new[] { 1, 2, 3 } };
        var mockComplexResult = new Mock<ITaskExecutionResult>();
        mockComplexResult.Setup(x => x.IsSuccessful).Returns(true);
        mockComplexResult.Setup(x => x.Result).Returns(complexResult);

        // Act & Assert
        mockStringResult.Object.Result.ShouldBe("string result");
        mockNumericResult.Object.Result.ShouldBe(42);
        mockComplexResult.Object.Result.ShouldBe(complexResult);
    }

    [Fact]
    public void TaskExecutionResultFailureStatesShouldBeConsistent()
    {
        // Arrange - Failed execution should have error message
        var mockFailedResult = new Mock<ITaskExecutionResult>();
        mockFailedResult.Setup(x => x.IsSuccessful).Returns(false);
        mockFailedResult.Setup(x => x.ErrorMessage).Returns("Operation failed");
        mockFailedResult.Setup(x => x.Result).Returns((object?)null);

        var failedResult = mockFailedResult.Object;
        
        // Act & Assert
        failedResult.IsSuccessful.ShouldBeFalse();
        failedResult.ErrorMessage.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void TaskExecutionResultInterfaceShouldBePublic()
    {
        // Arrange & Act
        var resultType = typeof(ITaskExecutionResult);
        
        // Assert
        resultType.IsInterface.ShouldBeTrue();
        resultType.IsPublic.ShouldBeTrue();
        resultType.Name.ShouldBe("ITaskExecutionResult");
        resultType.Namespace.ShouldBe("FractalDataWorks.Services.Scheduling.Abstractions");
    }

    [Fact]
    public void TaskExecutionResultPropertiesShouldHaveCorrectTypes()
    {
        // Arrange & Act
        var resultType = typeof(ITaskExecutionResult);
        var isSuccessfulProperty = resultType.GetProperty(nameof(ITaskExecutionResult.IsSuccessful));
        var resultProperty = resultType.GetProperty(nameof(ITaskExecutionResult.Result));
        var errorMessageProperty = resultType.GetProperty(nameof(ITaskExecutionResult.ErrorMessage));
        
        // Assert
        isSuccessfulProperty.ShouldNotBeNull();
        isSuccessfulProperty!.PropertyType.ShouldBe(typeof(bool));
        
        resultProperty.ShouldNotBeNull();
        resultProperty!.PropertyType.ShouldBe(typeof(object));
        
        errorMessageProperty.ShouldNotBeNull();
        errorMessageProperty!.PropertyType.ShouldBe(typeof(string));
    }

    [Fact]
    public void TaskExecutionResultShouldHandlePartialFailures()
    {
        // Arrange - Task that partially succeeded but has warnings
        var mockPartialResult = new Mock<ITaskExecutionResult>();
        var partialData = new { processed = 80, failed = 20, warnings = 5 };
        
        mockPartialResult.Setup(x => x.IsSuccessful).Returns(true); // Still considered successful
        mockPartialResult.Setup(x => x.Result).Returns(partialData);
        mockPartialResult.Setup(x => x.ErrorMessage).Returns((string?)null); // No error, just warnings

        var result = mockPartialResult.Object;
        
        // Act & Assert
        result.IsSuccessful.ShouldBeTrue();
        result.Result.ShouldBe(partialData);
        result.ErrorMessage.ShouldBeNull();
    }
}