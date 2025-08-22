using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.Scheduling.Abstractions;
using Moq;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests;

public sealed class ITaskExecutionContextTests
{
    [Fact]
    public void TaskExecutionContextShouldHaveRequiredProperties()
    {
        // Arrange
        var mockContext = CreateMockExecutionContext();
        var context = mockContext.Object;
        
        // Act & Assert
        context.ExecutionId.ShouldBe("exec-123");
        context.ScheduledTime.ShouldBe(new DateTimeOffset(2025, 1, 20, 10, 0, 0, TimeSpan.Zero));
        context.StartTime.ShouldBe(new DateTimeOffset(2025, 1, 20, 10, 0, 1, TimeSpan.Zero));
        context.CancellationToken.ShouldNotBe(default);
        context.ServiceProvider.ShouldNotBeNull();
        context.Metrics.ShouldNotBeNull();
        context.Properties.ShouldNotBeNull();
    }

    [Fact]
    public void TaskExecutionContextShouldHaveValidTimestamps()
    {
        // Arrange
        var scheduledTime = DateTimeOffset.UtcNow.AddMinutes(-1);
        var startTime = DateTimeOffset.UtcNow;
        
        var mockContext = new Mock<ITaskExecutionContext>();
        mockContext.Setup(x => x.ScheduledTime).Returns(scheduledTime);
        mockContext.Setup(x => x.StartTime).Returns(startTime);

        var context = mockContext.Object;
        
        // Act & Assert
        context.ScheduledTime.ShouldBeLessThanOrEqualTo(context.StartTime);
        context.StartTime.ShouldBeGreaterThanOrEqualTo(context.ScheduledTime);
    }

    [Fact]
    public void TaskExecutionContextShouldProvideValidCancellationToken()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var mockContext = new Mock<ITaskExecutionContext>();
        mockContext.Setup(x => x.CancellationToken).Returns(cancellationTokenSource.Token);

        var context = mockContext.Object;
        
        // Act & Assert
        context.CancellationToken.IsCancellationRequested.ShouldBeFalse();
        
        // Cancel the token and verify it's reflected
        cancellationTokenSource.Cancel();
        context.CancellationToken.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact]
    public void TaskExecutionContextShouldProvideServiceProvider()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockContext = new Mock<ITaskExecutionContext>();
        mockContext.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var context = mockContext.Object;
        
        // Act & Assert
        context.ServiceProvider.ShouldBe(mockServiceProvider.Object);
        context.ServiceProvider.ShouldNotBeNull();
    }

    [Fact]
    public void TaskExecutionContextShouldProvideMetrics()
    {
        // Arrange
        var mockMetrics = new Mock<ITaskExecutionMetrics>();
        var mockContext = new Mock<ITaskExecutionContext>();
        mockContext.Setup(x => x.Metrics).Returns(mockMetrics.Object);

        var context = mockContext.Object;
        
        // Act & Assert
        context.Metrics.ShouldBe(mockMetrics.Object);
        context.Metrics.ShouldNotBeNull();
    }

    [Fact]
    public void TaskExecutionContextShouldHaveProperties()
    {
        // Arrange
        var properties = new Dictionary<string, object>
        {
            ["scheduler"] = "TestScheduler",
            ["environment"] = "Development",
            ["retry_count"] = 0
        };
        
        var mockContext = new Mock<ITaskExecutionContext>();
        mockContext.Setup(x => x.Properties).Returns(properties);

        var context = mockContext.Object;
        
        // Act & Assert
        context.Properties.Count.ShouldBe(3);
        context.Properties["scheduler"].ShouldBe("TestScheduler");
        context.Properties["environment"].ShouldBe("Development");
        context.Properties["retry_count"].ShouldBe(0);
    }

    [Fact]
    public void ReportProgressShouldAcceptValidPercentage()
    {
        // Arrange
        var mockContext = new Mock<ITaskExecutionContext>();
        mockContext.Setup(x => x.ReportProgress(It.IsInRange(0, 100, Moq.Range.Inclusive), It.IsAny<string>()));

        var context = mockContext.Object;
        
        // Act & Assert (should not throw)
        context.ReportProgress(0);
        context.ReportProgress(50, "Half complete");
        context.ReportProgress(100, "Complete");
        
        // Verify calls were made
        mockContext.Verify(x => x.ReportProgress(0, null), Times.Once);
        mockContext.Verify(x => x.ReportProgress(50, "Half complete"), Times.Once);
        mockContext.Verify(x => x.ReportProgress(100, "Complete"), Times.Once);
    }

    [Fact]
    public void ReportProgressShouldHandleNullMessage()
    {
        // Arrange
        var mockContext = new Mock<ITaskExecutionContext>();
        mockContext.Setup(x => x.ReportProgress(It.IsAny<int>(), It.IsAny<string>()));

        var context = mockContext.Object;
        
        // Act & Assert (should not throw)
        context.ReportProgress(25);
        context.ReportProgress(75, null);
        
        // Verify calls were made
        mockContext.Verify(x => x.ReportProgress(25, null), Times.Once);
        mockContext.Verify(x => x.ReportProgress(75, null), Times.Once);
    }

    [Fact]
    public async Task SetCheckpointAsyncShouldAcceptCheckpointData()
    {
        // Arrange
        var checkpointData = new { step = 5, data = "checkpoint_state" };
        var mockContext = new Mock<ITaskExecutionContext>();
        mockContext.Setup(x => x.SetCheckpointAsync(It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        var context = mockContext.Object;
        
        // Act & Assert (should not throw)
        await context.SetCheckpointAsync(checkpointData);
        
        // Verify the method was called
        mockContext.Verify(x => x.SetCheckpointAsync(checkpointData), Times.Once);
    }

    [Fact]
    public async Task SetCheckpointAsyncShouldHandleNullCheckpointData()
    {
        // Arrange
        var mockContext = new Mock<ITaskExecutionContext>();
        mockContext.Setup(x => x.SetCheckpointAsync(It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        var context = mockContext.Object;
        
        // Act & Assert (should not throw)
        await context.SetCheckpointAsync(null!);
        
        // Verify the method was called
        mockContext.Verify(x => x.SetCheckpointAsync(null!), Times.Once);
    }

    [Fact]
    public void ExecutionIdShouldBeUniqueAndNonEmpty()
    {
        // Arrange
        var mockContext1 = new Mock<ITaskExecutionContext>();
        var mockContext2 = new Mock<ITaskExecutionContext>();
        mockContext1.Setup(x => x.ExecutionId).Returns(Guid.NewGuid().ToString());
        mockContext2.Setup(x => x.ExecutionId).Returns(Guid.NewGuid().ToString());

        var context1 = mockContext1.Object;
        var context2 = mockContext2.Object;
        
        // Act & Assert
        context1.ExecutionId.ShouldNotBeNullOrEmpty();
        context2.ExecutionId.ShouldNotBeNullOrEmpty();
        context1.ExecutionId.ShouldNotBe(context2.ExecutionId);
    }

    [Fact]
    public void TaskExecutionContextShouldHandleEmptyProperties()
    {
        // Arrange
        var mockContext = new Mock<ITaskExecutionContext>();
        mockContext.Setup(x => x.Properties).Returns(new Dictionary<string, object>());

        var context = mockContext.Object;
        
        // Act & Assert
        context.Properties.ShouldBeEmpty();
        context.Properties.Count.ShouldBe(0);
    }

    [Fact]
    public void TaskExecutionContextPropertiesShouldBeReadOnly()
    {
        // Arrange
        var mockContext = CreateMockExecutionContext();
        var context = mockContext.Object;
        
        // Act & Assert
        context.Properties.GetType().Name.ShouldContain("ReadOnly");
    }

    private static Mock<ITaskExecutionContext> CreateMockExecutionContext()
    {
        var mockContext = new Mock<ITaskExecutionContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockMetrics = new Mock<ITaskExecutionMetrics>();
        var properties = new Dictionary<string, object>
        {
            ["scheduler"] = "TestScheduler"
        } as IReadOnlyDictionary<string, object>;

        mockContext.Setup(x => x.ExecutionId).Returns("exec-123");
        mockContext.Setup(x => x.ScheduledTime).Returns(new DateTimeOffset(2025, 1, 20, 10, 0, 0, TimeSpan.Zero));
        mockContext.Setup(x => x.StartTime).Returns(new DateTimeOffset(2025, 1, 20, 10, 0, 1, TimeSpan.Zero));
        mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
        mockContext.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        mockContext.Setup(x => x.Metrics).Returns(mockMetrics.Object);
        mockContext.Setup(x => x.Properties).Returns(properties);
        
        return mockContext;
    }
}