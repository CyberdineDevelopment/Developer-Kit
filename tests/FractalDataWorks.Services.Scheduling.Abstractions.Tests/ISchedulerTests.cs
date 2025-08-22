using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Scheduling.Abstractions;
using Moq;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests;

public sealed class ISchedulerTests
{
    [Fact]
    public void SchedulerShouldInheritFromIFdwService()
    {
        // Arrange & Act
        var schedulerType = typeof(IScheduler);
        
        // Assert
        typeof(IFdwService).IsAssignableFrom(schedulerType).ShouldBeTrue();
        schedulerType.IsInterface.ShouldBeTrue();
        schedulerType.Name.ShouldBe("IScheduler");
        schedulerType.Namespace.ShouldBe("FractalDataWorks.Services.Scheduling.Abstractions");
    }

    [Fact]
    public void GenericSchedulerShouldHaveCorrectConstraints()
    {
        // Arrange & Act
        var genericSchedulerType = typeof(IScheduler<>);
        var constraints = genericSchedulerType.GetGenericArguments()[0].GetGenericParameterConstraints();
        
        // Assert
        constraints.ShouldContain(typeof(IScheduleCommand));
        genericSchedulerType.GetInterfaces().ShouldContain(typeof(IScheduler));
        genericSchedulerType.IsGenericTypeDefinition.ShouldBeTrue();
    }

    [Fact]
    public void SchedulerShouldHaveRequiredProperties()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        mockScheduler.SetupGet(x => x.SupportedSchedulingStrategies)
            .Returns(new List<string> { "Cron", "Interval", "Once" });
        mockScheduler.SetupGet(x => x.SupportedExecutionModes)
            .Returns(new List<string> { "Sequential", "Parallel" });
        mockScheduler.SetupGet(x => x.MaxConcurrentTasks).Returns(10);
        mockScheduler.SetupGet(x => x.ActiveTaskCount).Returns(3);
        mockScheduler.SetupGet(x => x.QueuedTaskCount).Returns(5);

        var scheduler = mockScheduler.Object;
        
        // Act & Assert
        scheduler.SupportedSchedulingStrategies.ShouldContain("Cron");
        scheduler.SupportedSchedulingStrategies.ShouldContain("Interval");
        scheduler.SupportedSchedulingStrategies.ShouldContain("Once");
        scheduler.SupportedExecutionModes.ShouldContain("Sequential");
        scheduler.SupportedExecutionModes.ShouldContain("Parallel");
        scheduler.MaxConcurrentTasks.ShouldBe(10);
        scheduler.ActiveTaskCount.ShouldBe(3);
        scheduler.QueuedTaskCount.ShouldBe(5);
    }

    [Fact]
    public async Task ScheduleTaskShouldAcceptValidParameters()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        var mockTask = new Mock<IScheduledTask>();
        var mockSchedule = new Mock<ITaskSchedule>();
        var expectedResult = FdwResult<string>.Success("task-123");
        
        mockScheduler.Setup(x => x.ScheduleTask(It.IsAny<IScheduledTask>(), It.IsAny<ITaskSchedule>()))
            .ReturnsAsync(expectedResult);

        var scheduler = mockScheduler.Object;
        
        // Act
        var result = await scheduler.ScheduleTask(mockTask.Object, mockSchedule.Object);
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("task-123");
    }

    [Fact]
    public async Task CancelTaskShouldAcceptTaskId()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        var expectedResult = FdwResult.Success();
        
        mockScheduler.Setup(x => x.CancelTask("task-123"))
            .ReturnsAsync(expectedResult);

        var scheduler = mockScheduler.Object;
        
        // Act
        var result = await scheduler.CancelTask("task-123");
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task PauseTaskShouldAcceptTaskId()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        var expectedResult = FdwResult.Success();
        
        mockScheduler.Setup(x => x.PauseTask("task-123"))
            .ReturnsAsync(expectedResult);

        var scheduler = mockScheduler.Object;
        
        // Act
        var result = await scheduler.PauseTask("task-123");
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ResumeTaskShouldAcceptTaskId()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        var expectedResult = FdwResult.Success();
        
        mockScheduler.Setup(x => x.ResumeTask("task-123"))
            .ReturnsAsync(expectedResult);

        var scheduler = mockScheduler.Object;
        
        // Act
        var result = await scheduler.ResumeTask("task-123");
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteTaskNowShouldReturnExecutionResult()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        var mockExecutionResult = new Mock<ITaskExecutionResult>();
        var expectedResult = FdwResult<ITaskExecutionResult>.Success(mockExecutionResult.Object);
        
        mockScheduler.Setup(x => x.ExecuteTaskNow("task-123"))
            .ReturnsAsync(expectedResult);

        var scheduler = mockScheduler.Object;
        
        // Act
        var result = await scheduler.ExecuteTaskNow("task-123");
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(mockExecutionResult.Object);
    }

    [Fact]
    public async Task GetTaskInfoShouldReturnTaskInformation()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        var mockTaskInfo = new Mock<ITaskInfo>();
        var expectedResult = FdwResult<ITaskInfo>.Success(mockTaskInfo.Object);
        
        mockScheduler.Setup(x => x.GetTaskInfo("task-123"))
            .ReturnsAsync(expectedResult);

        var scheduler = mockScheduler.Object;
        
        // Act
        var result = await scheduler.GetTaskInfo("task-123");
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(mockTaskInfo.Object);
    }

    [Fact]
    public async Task GetAllTasksShouldReturnTaskCollection()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        var mockTaskInfoList = new List<ITaskInfo>
        {
            new Mock<ITaskInfo>().Object,
            new Mock<ITaskInfo>().Object
        };
        var expectedResult = FdwResult<IReadOnlyList<ITaskInfo>>.Success(mockTaskInfoList);
        
        mockScheduler.Setup(x => x.GetAllTasks())
            .ReturnsAsync(expectedResult);

        var scheduler = mockScheduler.Object;
        
        // Act
        var result = await scheduler.GetAllTasks();
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetSchedulerMetricsAsyncShouldReturnMetrics()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        var mockMetrics = new Mock<ISchedulerMetrics>();
        var expectedResult = FdwResult<ISchedulerMetrics>.Success(mockMetrics.Object);
        
        mockScheduler.Setup(x => x.GetSchedulerMetricsAsync())
            .ReturnsAsync(expectedResult);

        var scheduler = mockScheduler.Object;
        
        // Act
        var result = await scheduler.GetSchedulerMetricsAsync();
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(mockMetrics.Object);
    }

    [Fact]
    public async Task CreateExecutionContextAsyncShouldReturnTaskExecutor()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        var mockConfiguration = new Mock<ITaskExecutorConfiguration>();
        var mockExecutor = new Mock<ITaskExecutor>();
        var expectedResult = FdwResult<ITaskExecutor>.Success(mockExecutor.Object);
        
        mockScheduler.Setup(x => x.CreateExecutionContextAsync(mockConfiguration.Object))
            .ReturnsAsync(expectedResult);

        var scheduler = mockScheduler.Object;
        
        // Act
        var result = await scheduler.CreateExecutionContextAsync(mockConfiguration.Object);
        
        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(mockExecutor.Object);
    }

    [Fact]
    public void SchedulerPropertiesShouldHandleNullValues()
    {
        // Arrange
        var mockScheduler = new Mock<IScheduler<IScheduleCommand>>();
        mockScheduler.SetupGet(x => x.MaxConcurrentTasks).Returns((int?)null);
        mockScheduler.SetupGet(x => x.ActiveTaskCount).Returns(0);
        mockScheduler.SetupGet(x => x.QueuedTaskCount).Returns(0);
        mockScheduler.SetupGet(x => x.SupportedSchedulingStrategies).Returns(new List<string>());
        mockScheduler.SetupGet(x => x.SupportedExecutionModes).Returns(new List<string>());

        var scheduler = mockScheduler.Object;
        
        // Act & Assert
        scheduler.MaxConcurrentTasks.ShouldBeNull();
        scheduler.ActiveTaskCount.ShouldBe(0);
        scheduler.QueuedTaskCount.ShouldBe(0);
        scheduler.SupportedSchedulingStrategies.ShouldBeEmpty();
        scheduler.SupportedExecutionModes.ShouldBeEmpty();
    }
}