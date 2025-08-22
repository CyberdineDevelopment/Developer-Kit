using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Scheduling.Abstractions;
using Moq;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests;

public sealed class IScheduledTaskTests
{
    [Fact]
    public void ScheduledTaskShouldHaveRequiredProperties()
    {
        // Arrange
        var mockTask = CreateMockScheduledTask();
        var task = mockTask.Object;
        
        // Act & Assert
        task.TaskId.ShouldBe("test-task-123");
        task.TaskName.ShouldBe("Test Task");
        task.TaskCategory.ShouldBe("Testing");
        task.Priority.ShouldBe(5);
        task.ExpectedExecutionTime.ShouldBe(TimeSpan.FromMinutes(10));
        task.MaxExecutionTime.ShouldBe(TimeSpan.FromMinutes(30));
        task.AllowsConcurrentExecution.ShouldBeTrue();
    }

    [Fact]
    public void ScheduledTaskShouldHandleNullableProperties()
    {
        // Arrange
        var mockTask = new Mock<IScheduledTask>();
        mockTask.Setup(x => x.TaskId).Returns("task-null-test");
        mockTask.Setup(x => x.TaskName).Returns("Null Test Task");
        mockTask.Setup(x => x.TaskCategory).Returns("Testing");
        mockTask.Setup(x => x.Priority).Returns(1);
        mockTask.Setup(x => x.ExpectedExecutionTime).Returns((TimeSpan?)null);
        mockTask.Setup(x => x.MaxExecutionTime).Returns((TimeSpan?)null);
        mockTask.Setup(x => x.Dependencies).Returns(new List<string>());
        mockTask.Setup(x => x.Configuration).Returns(new Dictionary<string, object>());
        mockTask.Setup(x => x.Metadata).Returns(new Dictionary<string, object>());
        mockTask.Setup(x => x.AllowsConcurrentExecution).Returns(false);

        var task = mockTask.Object;
        
        // Act & Assert
        task.ExpectedExecutionTime.ShouldBeNull();
        task.MaxExecutionTime.ShouldBeNull();
        task.AllowsConcurrentExecution.ShouldBeFalse();
    }

    [Fact]
    public void ScheduledTaskShouldHaveValidDependencies()
    {
        // Arrange
        var dependencies = new List<string> { "task-1", "task-2", "task-3" };
        var mockTask = new Mock<IScheduledTask>();
        mockTask.Setup(x => x.Dependencies).Returns(dependencies);

        var task = mockTask.Object;
        
        // Act & Assert
        task.Dependencies.Count.ShouldBe(3);
        task.Dependencies.ShouldContain("task-1");
        task.Dependencies.ShouldContain("task-2");
        task.Dependencies.ShouldContain("task-3");
    }

    [Fact]
    public void ScheduledTaskShouldHaveConfiguration()
    {
        // Arrange
        var configuration = new Dictionary<string, object>
        {
            ["timeout"] = 300,
            ["retryCount"] = 3,
            ["enableLogging"] = true
        };
        var mockTask = new Mock<IScheduledTask>();
        mockTask.Setup(x => x.Configuration).Returns(configuration);

        var task = mockTask.Object;
        
        // Act & Assert
        task.Configuration.Count.ShouldBe(3);
        task.Configuration["timeout"].ShouldBe(300);
        task.Configuration["retryCount"].ShouldBe(3);
        task.Configuration["enableLogging"].ShouldBe(true);
    }

    [Fact]
    public void ScheduledTaskShouldHaveMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["owner"] = "admin",
            ["version"] = "1.2.3",
            ["created"] = DateTime.UtcNow
        };
        var mockTask = new Mock<IScheduledTask>();
        mockTask.Setup(x => x.Metadata).Returns(metadata);

        var task = mockTask.Object;
        
        // Act & Assert
        task.Metadata.Count.ShouldBe(3);
        task.Metadata["owner"].ShouldBe("admin");
        task.Metadata["version"].ShouldBe("1.2.3");
        task.Metadata.ShouldContainKey("created");
    }

    [Fact]
    public async Task ExecuteAsyncShouldAcceptContextAndReturnResult()
    {
        // Arrange
        var mockTask = new Mock<IScheduledTask>();
        var mockContext = new Mock<ITaskExecutionContext>();
        var expectedResult = FdwResult<object?>.Success("execution completed");
        
        mockTask.Setup(x => x.ExecuteAsync(It.IsAny<ITaskExecutionContext>()))
            .ReturnsAsync(expectedResult);

        var task = mockTask.Object;
        
        // Act
        var result = await task.ExecuteAsync(mockContext.Object);
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("execution completed");
    }

    [Fact]
    public async Task ExecuteAsyncShouldHandleFailureResults()
    {
        // Arrange
        var mockTask = new Mock<IScheduledTask>();
        var mockContext = new Mock<ITaskExecutionContext>();
        var expectedResult = FdwResult<object?>.Failure("execution failed");
        
        mockTask.Setup(x => x.ExecuteAsync(It.IsAny<ITaskExecutionContext>()))
            .ReturnsAsync(expectedResult);

        var task = mockTask.Object;
        
        // Act
        var result = await task.ExecuteAsync(mockContext.Object);
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("execution failed");
    }

    [Fact]
    public void ValidateTaskShouldReturnValidationResult()
    {
        // Arrange
        var mockTask = new Mock<IScheduledTask>();
        var expectedResult = FdwResult.Success();
        
        mockTask.Setup(x => x.ValidateTask()).Returns(expectedResult);

        var task = mockTask.Object;
        
        // Act
        var result = task.ValidateTask();
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTaskShouldHandleValidationFailure()
    {
        // Arrange
        var mockTask = new Mock<IScheduledTask>();
        var expectedResult = FdwResult.Failure("validation failed - missing required configuration");
        
        mockTask.Setup(x => x.ValidateTask()).Returns(expectedResult);

        var task = mockTask.Object;
        
        // Act
        var result = task.ValidateTask();
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("validation failed - missing required configuration");
    }

    [Fact]
    public async Task OnCleanupAsyncShouldAcceptContextAndReason()
    {
        // Arrange
        var mockTask = new Mock<IScheduledTask>();
        var mockContext = new Mock<ITaskExecutionContext>();
        
        mockTask.Setup(x => x.OnCleanupAsync(It.IsAny<ITaskExecutionContext>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var task = mockTask.Object;
        
        // Act & Assert (should not throw)
        await task.OnCleanupAsync(mockContext.Object, "cancellation");
        
        // Verify the method was called
        mockTask.Verify(x => x.OnCleanupAsync(mockContext.Object, "cancellation"), Times.Once);
    }

    [Fact]
    public void ScheduledTaskPriorityRangeShouldBeValid()
    {
        // Arrange & Act
        var lowPriorityTask = CreateMockScheduledTaskWithPriority(1);
        var mediumPriorityTask = CreateMockScheduledTaskWithPriority(5);
        var highPriorityTask = CreateMockScheduledTaskWithPriority(10);
        
        // Assert
        lowPriorityTask.Object.Priority.ShouldBe(1);
        mediumPriorityTask.Object.Priority.ShouldBe(5);
        highPriorityTask.Object.Priority.ShouldBe(10);
        
        // Higher numbers should indicate higher priority
        highPriorityTask.Object.Priority.ShouldBeGreaterThan(mediumPriorityTask.Object.Priority);
        mediumPriorityTask.Object.Priority.ShouldBeGreaterThan(lowPriorityTask.Object.Priority);
    }

    [Fact]
    public void ScheduledTaskCollectionsShouldBeReadOnly()
    {
        // Arrange
        var mockTask = CreateMockScheduledTask();
        var task = mockTask.Object;
        
        // Act & Assert
        task.Dependencies.GetType().Name.ShouldContain("ReadOnly");
        task.Configuration.GetType().Name.ShouldContain("ReadOnly");
        task.Metadata.GetType().Name.ShouldContain("ReadOnly");
    }

    private static Mock<IScheduledTask> CreateMockScheduledTask()
    {
        var mockTask = new Mock<IScheduledTask>();
        mockTask.Setup(x => x.TaskId).Returns("test-task-123");
        mockTask.Setup(x => x.TaskName).Returns("Test Task");
        mockTask.Setup(x => x.TaskCategory).Returns("Testing");
        mockTask.Setup(x => x.Priority).Returns(5);
        mockTask.Setup(x => x.ExpectedExecutionTime).Returns(TimeSpan.FromMinutes(10));
        mockTask.Setup(x => x.MaxExecutionTime).Returns(TimeSpan.FromMinutes(30));
        mockTask.Setup(x => x.Dependencies).Returns(new List<string> { "dependency-1" }.AsReadOnly());
        mockTask.Setup(x => x.Configuration).Returns(new ReadOnlyDictionary<string, object>(new Dictionary<string, object> { ["key"] = "value" }));
        mockTask.Setup(x => x.Metadata).Returns(new ReadOnlyDictionary<string, object>(new Dictionary<string, object> { ["meta"] = "data" }));
        mockTask.Setup(x => x.AllowsConcurrentExecution).Returns(true);
        
        return mockTask;
    }

    private static Mock<IScheduledTask> CreateMockScheduledTaskWithPriority(int priority)
    {
        var mockTask = new Mock<IScheduledTask>();
        mockTask.Setup(x => x.Priority).Returns(priority);
        return mockTask;
    }
}