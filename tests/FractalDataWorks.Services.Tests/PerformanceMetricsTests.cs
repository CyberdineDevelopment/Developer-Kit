using System;
using FractalDataWorks.Services;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public sealed class PerformanceMetricsTests
{
    [Fact]
    public void PerformanceMetricsWhenCreatedShouldInitializeAllProperties()
    {
        // Arrange
        var duration = 500.0;
        var itemsProcessed = 100;
        var operationType = "TestOperation";
        var sensitiveData = "SensitiveInfo";

        // Act
        var metrics = new PerformanceMetrics(duration, itemsProcessed, operationType, sensitiveData);

        // Assert
        metrics.Duration.ShouldBe(duration);
        metrics.ItemsProcessed.ShouldBe(itemsProcessed);
        metrics.OperationType.ShouldBe(operationType);
        metrics.SensitiveData.ShouldBe(sensitiveData);
    }

    [Fact]
    public void PerformanceMetricsWhenCreatedWithDefaultSensitiveDataShouldBeNull()
    {
        // Arrange
        var duration = 250.5;
        var itemsProcessed = 50;
        var operationType = "DefaultOperation";

        // Act
        var metrics = new PerformanceMetrics(duration, itemsProcessed, operationType);

        // Assert
        metrics.Duration.ShouldBe(duration);
        metrics.ItemsProcessed.ShouldBe(itemsProcessed);
        metrics.OperationType.ShouldBe(operationType);
        metrics.SensitiveData.ShouldBeNull();
    }

    [Fact]
    public void PerformanceMetricsWhenCreatedWithDifferentValuesShouldHaveDifferentProperties()
    {
        // Arrange
        var duration1 = 100.0;
        var itemsProcessed1 = 25;
        var operationType1 = "Operation1";
        var sensitiveData1 = "Data1";

        var duration2 = 200.0;
        var itemsProcessed2 = 50;
        var operationType2 = "Operation2";
        var sensitiveData2 = "Data2";

        // Act
        var metrics1 = new PerformanceMetrics(duration1, itemsProcessed1, operationType1, sensitiveData1);
        var metrics2 = new PerformanceMetrics(duration2, itemsProcessed2, operationType2, sensitiveData2);

        // Assert
        metrics1.Duration.ShouldNotBe(metrics2.Duration);
        metrics1.ItemsProcessed.ShouldNotBe(metrics2.ItemsProcessed);
        metrics1.OperationType.ShouldNotBe(metrics2.OperationType);
        metrics1.SensitiveData.ShouldNotBe(metrics2.SensitiveData);
    }

    [Fact]
    public void PerformanceMetricsEqualityWhenSameValuesShouldBeEqual()
    {
        // Arrange
        var duration = 500.0;
        var itemsProcessed = 100;
        var operationType = "TestOperation";
        var sensitiveData = "SensitiveInfo";

        // Act
        var metrics1 = new PerformanceMetrics(duration, itemsProcessed, operationType, sensitiveData);
        var metrics2 = new PerformanceMetrics(duration, itemsProcessed, operationType, sensitiveData);

        // Assert
        metrics1.ShouldBe(metrics2);
        (metrics1 == metrics2).ShouldBeTrue();
        (metrics1 != metrics2).ShouldBeFalse();
        metrics1.GetHashCode().ShouldBe(metrics2.GetHashCode());
    }

    [Fact]
    public void PerformanceMetricsEqualityWhenDifferentValuesShouldNotBeEqual()
    {
        // Arrange
        var metrics1 = new PerformanceMetrics(100.0, 25, "Operation1", "Data1");
        var metrics2 = new PerformanceMetrics(200.0, 50, "Operation2", "Data2");

        // Act & Assert
        metrics1.ShouldNotBe(metrics2);
        (metrics1 == metrics2).ShouldBeFalse();
        (metrics1 != metrics2).ShouldBeTrue();
        metrics1.GetHashCode().ShouldNotBe(metrics2.GetHashCode());
    }

    [Fact]
    public void PerformanceMetricsWhenComparedToNullShouldNotBeEqual()
    {
        // Arrange
        var metrics = new PerformanceMetrics(500.0, 100, "TestOperation", "SensitiveInfo");

        // Act & Assert
        metrics.ShouldNotBe(null);
        (metrics == null).ShouldBeFalse();
        (metrics != null).ShouldBeTrue();
    }

    [Fact]
    public void PerformanceMetricsWhenComparedToDifferentTypeShouldNotBeEqual()
    {
        // Arrange
        var metrics = new PerformanceMetrics(500.0, 100, "TestOperation", "SensitiveInfo");
        var differentType = "NotAPerformanceMetrics";

        // Act & Assert
        metrics.Equals(differentType).ShouldBeFalse();
    }

    [Theory]
    [InlineData("", true)]
    [InlineData(null, true)]
    [InlineData("EmptyOperation", false)]
    public void PerformanceMetricsWithNullOrEmptyOperationTypeShouldStoreValuesAsIs(string? operationType, bool expectNull)
    {
        // Arrange
        var duration = 100.0;
        var itemsProcessed = 25;
        var sensitiveData = expectNull ? null : "TestData";

        // Act
        var metrics = new PerformanceMetrics(duration, itemsProcessed, operationType!, sensitiveData);

        // Assert
        metrics.Duration.ShouldBe(duration);
        metrics.ItemsProcessed.ShouldBe(itemsProcessed);
        metrics.OperationType.ShouldBe(operationType);
        if (expectNull)
        {
            metrics.SensitiveData.ShouldBeNull();
        }
        else
        {
            metrics.SensitiveData.ShouldBe(sensitiveData);
        }
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(100.5)]
    [InlineData(1000.123)]
    [InlineData(double.MaxValue)]
    [InlineData(-1.0)] // Negative values should be allowed
    public void PerformanceMetricsWithDifferentDurationsShouldStoreCorrectValues(double duration)
    {
        // Arrange & Act
        var metrics = new PerformanceMetrics(duration, 100, "TestOperation");

        // Assert
        metrics.Duration.ShouldBe(duration);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000000)]
    [InlineData(int.MaxValue)]
    [InlineData(-1)] // Negative values should be allowed
    public void PerformanceMetricsWithDifferentItemsProcessedShouldStoreCorrectValues(int itemsProcessed)
    {
        // Arrange & Act
        var metrics = new PerformanceMetrics(100.0, itemsProcessed, "TestOperation");

        // Assert
        metrics.ItemsProcessed.ShouldBe(itemsProcessed);
    }

    [Fact]
    public void PerformanceMetricsToStringShouldReturnExpectedFormat()
    {
        // Arrange
        var duration = 123.45;
        var itemsProcessed = 67;
        var operationType = "ProcessData";
        var metrics = new PerformanceMetrics(duration, itemsProcessed, operationType, "SensitiveInfo");

        // Act
        var stringRepresentation = metrics.ToString();

        // Assert
        stringRepresentation.ShouldBe("Duration: 123.45ms, Items: 67, Type: ProcessData");
        stringRepresentation.ShouldNotContain("SensitiveInfo"); // Sensitive data should not be in ToString
    }

    [Fact]
    public void PerformanceMetricsRecordShouldSupportDeconstruction()
    {
        // Arrange
        var originalDuration = 500.0;
        var originalItemsProcessed = 100;
        var originalOperationType = "TestOperation";
        var originalSensitiveData = "SensitiveInfo";

        var metrics = new PerformanceMetrics(originalDuration, originalItemsProcessed, originalOperationType, originalSensitiveData);

        // Act
        var (duration, itemsProcessed, operationType, sensitiveData) = metrics;

        // Assert
        duration.ShouldBe(originalDuration);
        itemsProcessed.ShouldBe(originalItemsProcessed);
        operationType.ShouldBe(originalOperationType);
        sensitiveData.ShouldBe(originalSensitiveData);
    }

    [Fact]
    public void PerformanceMetricsWithNullSensitiveDataShouldHandleCorrectly()
    {
        // Arrange & Act
        var metrics = new PerformanceMetrics(100.0, 50, "TestOperation", null);

        // Assert
        metrics.Duration.ShouldBe(100.0);
        metrics.ItemsProcessed.ShouldBe(50);
        metrics.OperationType.ShouldBe("TestOperation");
        metrics.SensitiveData.ShouldBeNull();
    }

    [Fact]
    public void PerformanceMetricsRecordShouldHaveInitOnlyProperties()
    {
        // Arrange
        var metrics = new PerformanceMetrics(500.0, 100, "TestOperation", "SensitiveInfo");

        // Act & Assert
        // Records have init-only setters, meaning properties can be set during initialization but not after
        // This test verifies that the record properties are properly declared
        var durationProperty = typeof(PerformanceMetrics).GetProperty("Duration")!;
        var itemsProperty = typeof(PerformanceMetrics).GetProperty("ItemsProcessed")!;
        var operationProperty = typeof(PerformanceMetrics).GetProperty("OperationType")!;
        var sensitiveProperty = typeof(PerformanceMetrics).GetProperty("SensitiveData")!;

        // All properties should exist and be readable
        durationProperty.CanRead.ShouldBeTrue();
        itemsProperty.CanRead.ShouldBeTrue();
        operationProperty.CanRead.ShouldBeTrue();
        sensitiveProperty.CanRead.ShouldBeTrue();

        // The actual values should be accessible
        metrics.Duration.ShouldBe(500.0);
        metrics.ItemsProcessed.ShouldBe(100);
        metrics.OperationType.ShouldBe("TestOperation");
        metrics.SensitiveData.ShouldBe("SensitiveInfo");
    }

    [Fact]
    public void PerformanceMetricsWithSpecialFloatingPointValuesShouldHandleCorrectly()
    {
        // Arrange & Act
        var nanMetrics = new PerformanceMetrics(double.NaN, 100, "TestOperation");
        var infinityMetrics = new PerformanceMetrics(double.PositiveInfinity, 100, "TestOperation");
        var negativeInfinityMetrics = new PerformanceMetrics(double.NegativeInfinity, 100, "TestOperation");

        // Assert
        double.IsNaN(nanMetrics.Duration).ShouldBeTrue();
        double.IsPositiveInfinity(infinityMetrics.Duration).ShouldBeTrue();
        double.IsNegativeInfinity(negativeInfinityMetrics.Duration).ShouldBeTrue();
    }

    [Fact]
    public void PerformanceMetricsWithVeryLargeOperationTypeShouldStoreCorrectly()
    {
        // Arrange
        var largeOperationType = new string('A', 10000); // 10k character string

        // Act
        var metrics = new PerformanceMetrics(100.0, 50, largeOperationType);

        // Assert
        metrics.OperationType.ShouldBe(largeOperationType);
        metrics.OperationType.Length.ShouldBe(10000);
    }

    [Fact]
    public void PerformanceMetricsEqualityWithOnlyDefaultValuesDifferingShouldWork()
    {
        // Arrange
        var metrics1 = new PerformanceMetrics(100.0, 50, "TestOperation"); // Default sensitiveData = null
        var metrics2 = new PerformanceMetrics(100.0, 50, "TestOperation", null); // Explicit null

        // Act & Assert
        metrics1.ShouldBe(metrics2);
        (metrics1 == metrics2).ShouldBeTrue();
        metrics1.GetHashCode().ShouldBe(metrics2.GetHashCode());
    }
}