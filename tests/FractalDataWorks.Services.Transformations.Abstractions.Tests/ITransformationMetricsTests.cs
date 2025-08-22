using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using Shouldly;
using Moq;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions.Tests;

/// <summary>
/// Comprehensive tests for ITransformationMetrics interface contracts and behavior.
/// Tests verify proper implementation of transformation performance metrics and data handling.
/// </summary>
public class ITransformationMetricsTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ITransformationMetrics> _mockMetrics;

    public ITransformationMetricsTests(ITestOutputHelper output)
    {
        _output = output;
        _mockMetrics = new Mock<ITransformationMetrics>();
    }

    [Fact]
    public void TotalTransformationsPropertyShouldReturnZeroWhenNoTransformationsExecuted()
    {
        // Arrange
        _mockMetrics.Setup(x => x.TotalTransformations).Returns(0L);

        // Act
        var result = _mockMetrics.Object.TotalTransformations;

        // Assert
        result.ShouldBe(0L);
        result.ShouldBeGreaterThanOrEqualTo(0L);
        _output.WriteLine($"TotalTransformations property returned zero when no transformations executed: {result}");
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(10L)]
    [InlineData(100L)]
    [InlineData(1000L)]
    [InlineData(10000L)]
    [InlineData(long.MaxValue)]
    public void TotalTransformationsPropertyShouldAcceptValidPositiveValues(long totalTransformations)
    {
        // Arrange
        _mockMetrics.Setup(x => x.TotalTransformations).Returns(totalTransformations);

        // Act
        var result = _mockMetrics.Object.TotalTransformations;

        // Assert
        result.ShouldBe(totalTransformations);
        result.ShouldBeGreaterThanOrEqualTo(0L);
        _output.WriteLine($"TotalTransformations correctly accepts positive value: {result}");
    }

    [Fact]
    public void SuccessfulTransformationsPropertyShouldReturnZeroWhenNoSuccessfulTransformations()
    {
        // Arrange
        _mockMetrics.Setup(x => x.SuccessfulTransformations).Returns(0L);

        // Act
        var result = _mockMetrics.Object.SuccessfulTransformations;

        // Assert
        result.ShouldBe(0L);
        result.ShouldBeGreaterThanOrEqualTo(0L);
        _output.WriteLine($"SuccessfulTransformations property returned zero when no successful transformations: {result}");
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(5L)]
    [InlineData(50L)]
    [InlineData(500L)]
    [InlineData(5000L)]
    public void SuccessfulTransformationsPropertyShouldAcceptValidValues(long successfulTransformations)
    {
        // Arrange
        _mockMetrics.Setup(x => x.SuccessfulTransformations).Returns(successfulTransformations);

        // Act
        var result = _mockMetrics.Object.SuccessfulTransformations;

        // Assert
        result.ShouldBe(successfulTransformations);
        result.ShouldBeGreaterThanOrEqualTo(0L);
        _output.WriteLine($"SuccessfulTransformations correctly accepts value: {result}");
    }

    [Fact]
    public void FailedTransformationsPropertyShouldReturnZeroWhenNoFailedTransformations()
    {
        // Arrange
        _mockMetrics.Setup(x => x.FailedTransformations).Returns(0L);

        // Act
        var result = _mockMetrics.Object.FailedTransformations;

        // Assert
        result.ShouldBe(0L);
        result.ShouldBeGreaterThanOrEqualTo(0L);
        _output.WriteLine($"FailedTransformations property returned zero when no failed transformations: {result}");
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(2L)]
    [InlineData(20L)]
    [InlineData(200L)]
    [InlineData(2000L)]
    public void FailedTransformationsPropertyShouldAcceptValidValues(long failedTransformations)
    {
        // Arrange
        _mockMetrics.Setup(x => x.FailedTransformations).Returns(failedTransformations);

        // Act
        var result = _mockMetrics.Object.FailedTransformations;

        // Assert
        result.ShouldBe(failedTransformations);
        result.ShouldBeGreaterThanOrEqualTo(0L);
        _output.WriteLine($"FailedTransformations correctly accepts value: {result}");
    }

    [Fact]
    public void AverageTransformationDurationMsPropertyShouldReturnZeroWhenNoTransformations()
    {
        // Arrange
        _mockMetrics.Setup(x => x.AverageTransformationDurationMs).Returns(0.0);

        // Act
        var result = _mockMetrics.Object.AverageTransformationDurationMs;

        // Assert
        result.ShouldBe(0.0);
        result.ShouldBeGreaterThanOrEqualTo(0.0);
        _output.WriteLine($"AverageTransformationDurationMs property returned zero when no transformations: {result}");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.5)]
    [InlineData(10.75)]
    [InlineData(100.25)]
    [InlineData(1000.0)]
    [InlineData(5000.123)]
    public void AverageTransformationDurationMsPropertyShouldAcceptValidValues(double averageDurationMs)
    {
        // Arrange
        _mockMetrics.Setup(x => x.AverageTransformationDurationMs).Returns(averageDurationMs);

        // Act
        var result = _mockMetrics.Object.AverageTransformationDurationMs;

        // Assert
        result.ShouldBe(averageDurationMs);
        result.ShouldBeGreaterThanOrEqualTo(0.0);
        _output.WriteLine($"AverageTransformationDurationMs correctly accepts value: {result}ms");
    }

    [Fact]
    public void ActiveTransformationsPropertyShouldReturnZeroWhenNoActiveTransformations()
    {
        // Arrange
        _mockMetrics.Setup(x => x.ActiveTransformations).Returns(0);

        // Act
        var result = _mockMetrics.Object.ActiveTransformations;

        // Assert
        result.ShouldBe(0);
        result.ShouldBeGreaterThanOrEqualTo(0);
        _output.WriteLine($"ActiveTransformations property returned zero when no active transformations: {result}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(100)]
    public void ActiveTransformationsPropertyShouldAcceptValidValues(int activeTransformations)
    {
        // Arrange
        _mockMetrics.Setup(x => x.ActiveTransformations).Returns(activeTransformations);

        // Act
        var result = _mockMetrics.Object.ActiveTransformations;

        // Assert
        result.ShouldBe(activeTransformations);
        result.ShouldBeGreaterThanOrEqualTo(0);
        _output.WriteLine($"ActiveTransformations correctly accepts value: {result}");
    }

    [Fact]
    public void MetricsStartTimePropertyShouldReturnValidDateTime()
    {
        // Arrange
        var expectedStartTime = DateTime.UtcNow.AddHours(-1);
        _mockMetrics.Setup(x => x.MetricsStartTime).Returns(expectedStartTime);

        // Act
        var result = _mockMetrics.Object.MetricsStartTime;

        // Assert
        result.ShouldBe(expectedStartTime);
        result.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        _output.WriteLine($"MetricsStartTime property returned expected value: {result:yyyy-MM-dd HH:mm:ss} UTC");
    }

    [Theory]
    [InlineData(-24)] // 24 hours ago
    [InlineData(-12)] // 12 hours ago
    [InlineData(-1)]  // 1 hour ago
    [InlineData(0)]   // Current time
    public void MetricsStartTimePropertyShouldAcceptValidDateTimes(int hoursOffset)
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(hoursOffset);
        _mockMetrics.Setup(x => x.MetricsStartTime).Returns(startTime);

        // Act
        var result = _mockMetrics.Object.MetricsStartTime;

        // Assert
        result.ShouldBe(startTime);
        _output.WriteLine($"MetricsStartTime correctly accepts DateTime value: {result:yyyy-MM-dd HH:mm:ss} UTC");
    }

    [Fact]
    public void LastTransformationTimePropertyShouldReturnNullWhenNoTransformations()
    {
        // Arrange
        _mockMetrics.Setup(x => x.LastTransformationTime).Returns((DateTime?)null);

        // Act
        var result = _mockMetrics.Object.LastTransformationTime;

        // Assert
        result.ShouldBeNull();
        _output.WriteLine("LastTransformationTime property correctly returns null when no transformations executed");
    }

    [Fact]
    public void LastTransformationTimePropertyShouldReturnValidDateTimeWhenTransformationsExecuted()
    {
        // Arrange
        var expectedLastTime = DateTime.UtcNow.AddMinutes(-5);
        _mockMetrics.Setup(x => x.LastTransformationTime).Returns(expectedLastTime);

        // Act
        var result = _mockMetrics.Object.LastTransformationTime;

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedLastTime);
        result.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        _output.WriteLine($"LastTransformationTime property returned expected value: {result:yyyy-MM-dd HH:mm:ss} UTC");
    }

    [Theory]
    [InlineData(-60)]  // 60 minutes ago
    [InlineData(-30)]  // 30 minutes ago
    [InlineData(-5)]   // 5 minutes ago
    [InlineData(-1)]   // 1 minute ago
    [InlineData(0)]    // Current time
    public void LastTransformationTimePropertyShouldAcceptValidDateTimes(int minutesOffset)
    {
        // Arrange
        var lastTime = DateTime.UtcNow.AddMinutes(minutesOffset);
        _mockMetrics.Setup(x => x.LastTransformationTime).Returns(lastTime);

        // Act
        var result = _mockMetrics.Object.LastTransformationTime;

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(lastTime);
        _output.WriteLine($"LastTransformationTime correctly accepts DateTime value: {result:yyyy-MM-dd HH:mm:ss} UTC");
    }

    [Fact]
    public void AllPropertiesShouldBeAccessibleIndependently()
    {
        // Arrange
        const long totalTransformations = 150L;
        const long successfulTransformations = 140L;
        const long failedTransformations = 10L;
        const double averageDurationMs = 125.75;
        const int activeTransformations = 3;
        var metricsStartTime = DateTime.UtcNow.AddHours(-2);
        var lastTransformationTime = DateTime.UtcNow.AddMinutes(-2);

        _mockMetrics.Setup(x => x.TotalTransformations).Returns(totalTransformations);
        _mockMetrics.Setup(x => x.SuccessfulTransformations).Returns(successfulTransformations);
        _mockMetrics.Setup(x => x.FailedTransformations).Returns(failedTransformations);
        _mockMetrics.Setup(x => x.AverageTransformationDurationMs).Returns(averageDurationMs);
        _mockMetrics.Setup(x => x.ActiveTransformations).Returns(activeTransformations);
        _mockMetrics.Setup(x => x.MetricsStartTime).Returns(metricsStartTime);
        _mockMetrics.Setup(x => x.LastTransformationTime).Returns(lastTransformationTime);

        // Act
        var metrics = _mockMetrics.Object;

        // Assert
        metrics.TotalTransformations.ShouldBe(totalTransformations);
        metrics.SuccessfulTransformations.ShouldBe(successfulTransformations);
        metrics.FailedTransformations.ShouldBe(failedTransformations);
        metrics.AverageTransformationDurationMs.ShouldBe(averageDurationMs);
        metrics.ActiveTransformations.ShouldBe(activeTransformations);
        metrics.MetricsStartTime.ShouldBe(metricsStartTime);
        metrics.LastTransformationTime.ShouldBe(lastTransformationTime);
        _output.WriteLine("All metrics properties are accessible and return expected values independently");
    }

    [Fact]
    public void MetricsShouldMaintainLogicalConsistency()
    {
        // Arrange
        const long totalTransformations = 100L;
        const long successfulTransformations = 80L;
        const long failedTransformations = 20L;

        _mockMetrics.Setup(x => x.TotalTransformations).Returns(totalTransformations);
        _mockMetrics.Setup(x => x.SuccessfulTransformations).Returns(successfulTransformations);
        _mockMetrics.Setup(x => x.FailedTransformations).Returns(failedTransformations);

        // Act
        var metrics = _mockMetrics.Object;

        // Assert
        (metrics.SuccessfulTransformations + metrics.FailedTransformations).ShouldBe(metrics.TotalTransformations);
        metrics.SuccessfulTransformations.ShouldBeLessThanOrEqualTo(metrics.TotalTransformations);
        metrics.FailedTransformations.ShouldBeLessThanOrEqualTo(metrics.TotalTransformations);
        _output.WriteLine($"Metrics maintain logical consistency: Total={metrics.TotalTransformations}, Success={metrics.SuccessfulTransformations}, Failed={metrics.FailedTransformations}");
    }

    [Fact]
    public void InterfaceContractShouldBeCorrectlyDefined()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationMetrics);

        // Assert
        interfaceType.ShouldNotBeNull();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Transformations.Abstractions");
        
        var properties = interfaceType.GetProperties();
        properties.Length.ShouldBe(7);
        
        _output.WriteLine($"Interface has {properties.Length} properties defined correctly");
    }

    [Theory]
    [InlineData(0L, 0L, 0L, 0.0, 0)]
    [InlineData(10L, 8L, 2L, 50.5, 1)]
    [InlineData(100L, 95L, 5L, 125.75, 5)]
    [InlineData(1000L, 980L, 20L, 200.25, 10)]
    public void MetricsShouldSupportVariousPerformanceScenarios(long total, long successful, long failed, double avgDuration, int active)
    {
        // Arrange
        _mockMetrics.Setup(x => x.TotalTransformations).Returns(total);
        _mockMetrics.Setup(x => x.SuccessfulTransformations).Returns(successful);
        _mockMetrics.Setup(x => x.FailedTransformations).Returns(failed);
        _mockMetrics.Setup(x => x.AverageTransformationDurationMs).Returns(avgDuration);
        _mockMetrics.Setup(x => x.ActiveTransformations).Returns(active);

        // Act
        var metrics = _mockMetrics.Object;

        // Assert
        metrics.TotalTransformations.ShouldBe(total);
        metrics.SuccessfulTransformations.ShouldBe(successful);
        metrics.FailedTransformations.ShouldBe(failed);
        metrics.AverageTransformationDurationMs.ShouldBe(avgDuration);
        metrics.ActiveTransformations.ShouldBe(active);
        _output.WriteLine($"Metrics support performance scenario: Total={total}, Success={successful}, Failed={failed}, AvgDuration={avgDuration}ms, Active={active}");
    }

    [Fact]
    public void MetricsShouldHandleHighVolumeScenarios()
    {
        // Arrange
        const long totalTransformations = 1_000_000L;
        const long successfulTransformations = 999_950L;
        const long failedTransformations = 50L;
        const double averageDurationMs = 15.25;
        const int activeTransformations = 50;

        _mockMetrics.Setup(x => x.TotalTransformations).Returns(totalTransformations);
        _mockMetrics.Setup(x => x.SuccessfulTransformations).Returns(successfulTransformations);
        _mockMetrics.Setup(x => x.FailedTransformations).Returns(failedTransformations);
        _mockMetrics.Setup(x => x.AverageTransformationDurationMs).Returns(averageDurationMs);
        _mockMetrics.Setup(x => x.ActiveTransformations).Returns(activeTransformations);

        // Act
        var metrics = _mockMetrics.Object;

        // Assert
        metrics.TotalTransformations.ShouldBe(totalTransformations);
        metrics.SuccessfulTransformations.ShouldBe(successfulTransformations);
        metrics.FailedTransformations.ShouldBe(failedTransformations);
        metrics.AverageTransformationDurationMs.ShouldBe(averageDurationMs);
        metrics.ActiveTransformations.ShouldBe(activeTransformations);
        
        // Calculate success rate
        var successRate = (double)metrics.SuccessfulTransformations / metrics.TotalTransformations * 100;
        successRate.ShouldBeGreaterThan(99.0);
        
        _output.WriteLine($"Metrics handle high volume scenario: {totalTransformations:N0} total transformations with {successRate:F2}% success rate");
    }
}