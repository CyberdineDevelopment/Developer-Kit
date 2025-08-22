using System.Threading.Tasks;
using FractalDataWorks.Services.Scheduling.Abstractions;
using Moq;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests;

public sealed class ITaskExecutorTests
{
    [Fact]
    public void TaskExecutorInterfaceShouldBePublic()
    {
        // Arrange & Act
        var executorType = typeof(ITaskExecutor);
        
        // Assert
        executorType.IsInterface.ShouldBeTrue();
        executorType.IsPublic.ShouldBeTrue();
        executorType.Name.ShouldBe("ITaskExecutor");
        executorType.Namespace.ShouldBe("FractalDataWorks.Services.Scheduling.Abstractions");
    }

    [Fact]
    public async Task ExecuteAsyncShouldReturnTaskExecutionResult()
    {
        // Arrange
        var mockExecutor = new Mock<ITaskExecutor>();
        var mockResult = new Mock<ITaskExecutionResult>();
        mockResult.Setup(x => x.IsSuccessful).Returns(true);
        mockResult.Setup(x => x.Result).Returns("task executed successfully");
        
        mockExecutor.Setup(x => x.ExecuteAsync()).ReturnsAsync(mockResult.Object);

        var executor = mockExecutor.Object;
        
        // Act
        var result = await executor.ExecuteAsync();
        
        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(mockResult.Object);
        result.IsSuccessful.ShouldBeTrue();
        result.Result.ShouldBe("task executed successfully");
    }

    [Fact]
    public async Task ExecuteAsyncShouldHandleSuccessfulExecution()
    {
        // Arrange
        var mockExecutor = new Mock<ITaskExecutor>();
        var mockResult = new Mock<ITaskExecutionResult>();
        mockResult.Setup(x => x.IsSuccessful).Returns(true);
        mockResult.Setup(x => x.Result).Returns(new { status = "completed", items = 100 });
        mockResult.Setup(x => x.ErrorMessage).Returns((string?)null);
        
        mockExecutor.Setup(x => x.ExecuteAsync()).ReturnsAsync(mockResult.Object);

        var executor = mockExecutor.Object;
        
        // Act
        var result = await executor.ExecuteAsync();
        
        // Assert
        result.IsSuccessful.ShouldBeTrue();
        result.Result.ShouldNotBeNull();
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsyncShouldHandleFailedExecution()
    {
        // Arrange
        var mockExecutor = new Mock<ITaskExecutor>();
        var mockResult = new Mock<ITaskExecutionResult>();
        mockResult.Setup(x => x.IsSuccessful).Returns(false);
        mockResult.Setup(x => x.Result).Returns((object?)null);
        mockResult.Setup(x => x.ErrorMessage).Returns("Task execution failed: Database connection timeout");
        
        mockExecutor.Setup(x => x.ExecuteAsync()).ReturnsAsync(mockResult.Object);

        var executor = mockExecutor.Object;
        
        // Act
        var result = await executor.ExecuteAsync();
        
        // Assert
        result.IsSuccessful.ShouldBeFalse();
        result.Result.ShouldBeNull();
        result.ErrorMessage.ShouldBe("Task execution failed: Database connection timeout");
    }

    [Fact]
    public async Task ExecuteAsyncShouldSupportAsynchronousExecution()
    {
        // Arrange
        var mockExecutor = new Mock<ITaskExecutor>();
        var completionSource = new TaskCompletionSource<ITaskExecutionResult>();
        var mockResult = new Mock<ITaskExecutionResult>();
        mockResult.Setup(x => x.IsSuccessful).Returns(true);
        
        mockExecutor.Setup(x => x.ExecuteAsync()).Returns(completionSource.Task);

        var executor = mockExecutor.Object;
        
        // Act - Start execution
        var executionTask = executor.ExecuteAsync();
        
        // Verify task is not completed yet
        executionTask.IsCompleted.ShouldBeFalse();
        
        // Complete the task
        completionSource.SetResult(mockResult.Object);
        var result = await executionTask;
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccessful.ShouldBeTrue();
        executionTask.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsyncShouldHandleExceptions()
    {
        // Arrange
        var mockExecutor = new Mock<ITaskExecutor>();
        mockExecutor.Setup(x => x.ExecuteAsync()).ThrowsAsync(new System.InvalidOperationException("Task executor is not initialized"));

        var executor = mockExecutor.Object;
        
        // Act & Assert
        var exception = await Should.ThrowAsync<System.InvalidOperationException>(
            async () => await executor.ExecuteAsync()
        );
        
        exception.Message.ShouldBe("Task executor is not initialized");
    }

    [Fact]
    public void TaskExecutorShouldHaveCorrectMethodSignature()
    {
        // Arrange & Act
        var executorType = typeof(ITaskExecutor);
        var executeAsyncMethod = executorType.GetMethod(nameof(ITaskExecutor.ExecuteAsync));
        
        // Assert
        executeAsyncMethod.ShouldNotBeNull();
        executeAsyncMethod!.ReturnType.ShouldBe(typeof(Task<ITaskExecutionResult>));
        executeAsyncMethod.GetParameters().Length.ShouldBe(0);
    }

    [Fact]
    public async Task MultipleExecuteAsyncCallsShouldBeSupported()
    {
        // Arrange
        var mockExecutor = new Mock<ITaskExecutor>();
        var callCount = 0;
        
        mockExecutor.Setup(x => x.ExecuteAsync()).ReturnsAsync(() =>
        {
            callCount++;
            var mockResult = new Mock<ITaskExecutionResult>();
            mockResult.Setup(r => r.IsSuccessful).Returns(true);
            mockResult.Setup(r => r.Result).Returns($"execution_{callCount}");
            return mockResult.Object;
        });

        var executor = mockExecutor.Object;
        
        // Act
        var result1 = await executor.ExecuteAsync();
        var result2 = await executor.ExecuteAsync();
        var result3 = await executor.ExecuteAsync();
        
        // Assert
        result1.Result.ShouldBe("execution_1");
        result2.Result.ShouldBe("execution_2");
        result3.Result.ShouldBe("execution_3");
        callCount.ShouldBe(3);
    }

    [Fact]
    public async Task ExecuteAsyncShouldAllowLongRunningOperations()
    {
        // Arrange
        var mockExecutor = new Mock<ITaskExecutor>();
        var mockResult = new Mock<ITaskExecutionResult>();
        mockResult.Setup(x => x.IsSuccessful).Returns(true);
        mockResult.Setup(x => x.Result).Returns("long operation completed");
        
        mockExecutor.Setup(x => x.ExecuteAsync()).Returns(async () =>
        {
            // Simulate long-running operation
            await Task.Delay(50); // Small delay for test purposes
            return mockResult.Object;
        });

        var executor = mockExecutor.Object;
        var startTime = System.DateTime.UtcNow;
        
        // Act
        var result = await executor.ExecuteAsync();
        var endTime = System.DateTime.UtcNow;
        
        // Assert
        result.IsSuccessful.ShouldBeTrue();
        result.Result.ShouldBe("long operation completed");
        (endTime - startTime).TotalMilliseconds.ShouldBeGreaterThan(40); // Allow some variance
    }
}