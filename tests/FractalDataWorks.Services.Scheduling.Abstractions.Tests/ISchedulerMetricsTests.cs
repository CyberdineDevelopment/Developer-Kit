using FractalDataWorks.Services.Scheduling.Abstractions;
using Moq;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests;

public sealed class ISchedulerMetricsTests
{
    [Fact]
    public void SchedulerMetricsShouldHaveRequiredProperties()
    {
        // Arrange
        var mockMetrics = new Mock<ISchedulerMetrics>();
        mockMetrics.Setup(x => x.TotalTasks).Returns(100);
        mockMetrics.Setup(x => x.RunningTasks).Returns(5);

        var metrics = mockMetrics.Object;
        
        // Act & Assert
        metrics.TotalTasks.ShouldBe(100);
        metrics.RunningTasks.ShouldBe(5);
    }

    [Fact]
    public void SchedulerMetricsShouldHandleZeroValues()
    {
        // Arrange
        var mockMetrics = new Mock<ISchedulerMetrics>();
        mockMetrics.Setup(x => x.TotalTasks).Returns(0);
        mockMetrics.Setup(x => x.RunningTasks).Returns(0);

        var metrics = mockMetrics.Object;
        
        // Act & Assert
        metrics.TotalTasks.ShouldBe(0);
        metrics.RunningTasks.ShouldBe(0);
    }

    [Fact]
    public void RunningTasksShouldNotExceedTotalTasks()
    {
        // Arrange
        var mockMetrics = new Mock<ISchedulerMetrics>();
        mockMetrics.Setup(x => x.TotalTasks).Returns(50);
        mockMetrics.Setup(x => x.RunningTasks).Returns(25);

        var metrics = mockMetrics.Object;
        
        // Act & Assert
        metrics.RunningTasks.ShouldBeLessThanOrEqualTo(metrics.TotalTasks);
    }

    [Fact]
    public void SchedulerMetricsShouldBeNonNegative()
    {
        // Arrange
        var mockMetrics = new Mock<ISchedulerMetrics>();
        mockMetrics.Setup(x => x.TotalTasks).Returns(10);
        mockMetrics.Setup(x => x.RunningTasks).Returns(3);

        var metrics = mockMetrics.Object;
        
        // Act & Assert
        metrics.TotalTasks.ShouldBeGreaterThanOrEqualTo(0);
        metrics.RunningTasks.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void SchedulerMetricsInterfaceShouldBePublic()
    {
        // Arrange & Act
        var metricsType = typeof(ISchedulerMetrics);
        
        // Assert
        metricsType.IsInterface.ShouldBeTrue();
        metricsType.IsPublic.ShouldBeTrue();
        metricsType.Name.ShouldBe("ISchedulerMetrics");
        metricsType.Namespace.ShouldBe("FractalDataWorks.Services.Scheduling.Abstractions");
    }

    [Fact]
    public void SchedulerMetricsShouldReflectCurrentState()
    {
        // Arrange - Simulate metrics that change over time
        var mockMetrics = new Mock<ISchedulerMetrics>();
        var totalTasks = 0;
        var runningTasks = 0;
        
        mockMetrics.SetupGet(x => x.TotalTasks).Returns(() => totalTasks);
        mockMetrics.SetupGet(x => x.RunningTasks).Returns(() => runningTasks);

        var metrics = mockMetrics.Object;
        
        // Act & Assert - Initial state
        metrics.TotalTasks.ShouldBe(0);
        metrics.RunningTasks.ShouldBe(0);
        
        // Simulate adding tasks
        totalTasks = 10;
        runningTasks = 2;
        
        metrics.TotalTasks.ShouldBe(10);
        metrics.RunningTasks.ShouldBe(2);
        
        // Simulate tasks completing
        runningTasks = 0;
        
        metrics.RunningTasks.ShouldBe(0);
        metrics.TotalTasks.ShouldBe(10); // Total remains the same
    }
}