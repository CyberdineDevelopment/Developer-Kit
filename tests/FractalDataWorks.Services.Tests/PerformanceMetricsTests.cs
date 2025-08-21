using System;
using FractalDataWorks.Services;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

public class PerformanceMetricsTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceMetricsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstructorShouldInitializeAllProperties()
    {
        // Arrange
        var duration = 123.45;
        var itemsProcessed = 100;
        var operationType = "TestOperation";
        var sensitiveData = "SensitiveInformation";

        // Act
        var metrics = new PerformanceMetrics(duration, itemsProcessed, operationType, sensitiveData);

        // Assert
        metrics.Duration.ShouldBe(duration);
        metrics.ItemsProcessed.ShouldBe(itemsProcessed);
        metrics.OperationType.ShouldBe(operationType);
        metrics.SensitiveData.ShouldBe(sensitiveData);
        
        _output.WriteLine($"PerformanceMetrics created: {metrics}");
    }

    [Fact]
    public void ConstructorWithoutSensitiveDataShouldSetSensitiveDataToNull()
    {
        // Arrange
        var duration = 67.89;
        var itemsProcessed = 50;
        var operationType = "AnotherOperation";

        // Act
        var metrics = new PerformanceMetrics(duration, itemsProcessed, operationType);

        // Assert
        metrics.Duration.ShouldBe(duration);
        metrics.ItemsProcessed.ShouldBe(itemsProcessed);
        metrics.OperationType.ShouldBe(operationType);
        metrics.SensitiveData.ShouldBeNull();
        
        _output.WriteLine($"PerformanceMetrics without sensitive data: {metrics}");
    }

    [Fact]
    public void ToStringShouldReturnCleanFormatWithoutSensitiveData()
    {
        // Arrange
        var duration = 100.0;
        var itemsProcessed = 25;
        var operationType = "DataProcessing";
        var sensitiveData = "SecretKey123";
        var metrics = new PerformanceMetrics(duration, itemsProcessed, operationType, sensitiveData);

        // Act
        var result = metrics.ToString();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain("Duration: 100ms");
        result.ShouldContain("Items: 25");
        result.ShouldContain("Type: DataProcessing");
        result.ShouldNotContain("SecretKey123"); // Sensitive data should not appear in ToString
        
        _output.WriteLine($"ToString result: {result}");
    }

    [Theory]
    [InlineData(0.0, 0, "EmptyOperation")]
    [InlineData(1.234, 1, "SingleItem")]
    [InlineData(9999.999, 999999, "LargeOperation")]
    [InlineData(-1.0, -5, "NegativeValues")] // Edge case with negative values
    public void ToStringShouldFormatDifferentValuesCorrectly(double duration, int itemsProcessed, string operationType)
    {
        // Arrange
        var metrics = new PerformanceMetrics(duration, itemsProcessed, operationType);

        // Act
        var result = metrics.ToString();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain($"Duration: {duration}ms");
        result.ShouldContain($"Items: {itemsProcessed}");
        result.ShouldContain($"Type: {operationType}");
        
        _output.WriteLine($"ToString for {operationType}: {result}");
    }

    [Fact]
    public void RecordEqualityShouldWorkCorrectly()
    {
        // Arrange
        var metrics1 = new PerformanceMetrics(100.0, 50, "Operation", "Sensitive");
        var metrics2 = new PerformanceMetrics(100.0, 50, "Operation", "Sensitive");
        var metrics3 = new PerformanceMetrics(100.0, 50, "Operation", null);

        // Act & Assert
        metrics1.ShouldBe(metrics2); // Same values should be equal
        metrics1.ShouldNotBe(metrics3); // Different sensitive data should not be equal
        metrics1.Equals(metrics2).ShouldBeTrue();
        (metrics1 == metrics2).ShouldBeTrue();
        (metrics1 != metrics3).ShouldBeTrue();
        
        _output.WriteLine($"Equality test: metrics1 == metrics2: {metrics1 == metrics2}");
        _output.WriteLine($"Equality test: metrics1 == metrics3: {metrics1 == metrics3}");
    }

    [Fact]
    public void GetHashCodeShouldBeConsistentForEqualObjects()
    {
        // Arrange
        var metrics1 = new PerformanceMetrics(100.0, 50, "Operation", "Sensitive");
        var metrics2 = new PerformanceMetrics(100.0, 50, "Operation", "Sensitive");

        // Act
        var hash1 = metrics1.GetHashCode();
        var hash2 = metrics2.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2); // Equal objects should have the same hash code
        
        _output.WriteLine($"Hash codes: metrics1={hash1}, metrics2={hash2}");
    }

    [Fact]
    public void RecordDeconstructionShouldWork()
    {
        // Arrange
        var originalDuration = 150.5;
        var originalItems = 75;
        var originalOperationType = "BatchProcess";
        var originalSensitiveData = "ApiKey456";
        var metrics = new PerformanceMetrics(originalDuration, originalItems, originalOperationType, originalSensitiveData);

        // Act
        var (duration, itemsProcessed, operationType, sensitiveData) = metrics;

        // Assert
        duration.ShouldBe(originalDuration);
        itemsProcessed.ShouldBe(originalItems);
        operationType.ShouldBe(originalOperationType);
        sensitiveData.ShouldBe(originalSensitiveData);
        
        _output.WriteLine($"Deconstructed values - Duration: {duration}, Items: {itemsProcessed}, Type: {operationType}, Sensitive: {sensitiveData}");
    }

    [Fact]
    public void WithExpressionsShouldCreateModifiedCopies()
    {
        // Arrange
        var original = new PerformanceMetrics(100.0, 50, "Original", "Secret");

        // Act
        var modifiedDuration = original with { Duration = 200.0 };
        var modifiedItems = original with { ItemsProcessed = 100 };
        var modifiedOperation = original with { OperationType = "Modified" };
        var modifiedSensitive = original with { SensitiveData = null };

        // Assert
        modifiedDuration.Duration.ShouldBe(200.0);
        modifiedDuration.ItemsProcessed.ShouldBe(50); // Unchanged
        modifiedDuration.OperationType.ShouldBe("Original"); // Unchanged
        
        modifiedItems.ItemsProcessed.ShouldBe(100);
        modifiedItems.Duration.ShouldBe(100.0); // Unchanged
        
        modifiedOperation.OperationType.ShouldBe("Modified");
        modifiedOperation.Duration.ShouldBe(100.0); // Unchanged
        
        modifiedSensitive.SensitiveData.ShouldBeNull();
        modifiedSensitive.Duration.ShouldBe(100.0); // Unchanged
        
        _output.WriteLine($"Original: {original}");
        _output.WriteLine($"Modified duration: {modifiedDuration}");
        _output.WriteLine($"Modified items: {modifiedItems}");
        _output.WriteLine($"Modified operation: {modifiedOperation}");
        _output.WriteLine($"Modified sensitive: {modifiedSensitive}");
    }

    [Fact]
    public void PropertiesShouldBeReadOnly()
    {
        // Arrange
        var metrics = new PerformanceMetrics(100.0, 50, "Test", "Data");

        // Act & Assert
        // Properties should be read-only (get-only), so we can't assign to them
        // This is enforced by the compiler, but we can verify they have values
        metrics.Duration.ShouldBe(100.0);
        metrics.ItemsProcessed.ShouldBe(50);
        metrics.OperationType.ShouldBe("Test");
        metrics.SensitiveData.ShouldBe("Data");
        
        // Verify that the record is immutable (no setters)
        var type = typeof(PerformanceMetrics);
        var durationProperty = type.GetProperty(nameof(PerformanceMetrics.Duration));
        var itemsProperty = type.GetProperty(nameof(PerformanceMetrics.ItemsProcessed));
        var operationProperty = type.GetProperty(nameof(PerformanceMetrics.OperationType));
        var sensitiveProperty = type.GetProperty(nameof(PerformanceMetrics.SensitiveData));
        
        durationProperty?.CanWrite.ShouldBeFalse();
        itemsProperty?.CanWrite.ShouldBeFalse();
        operationProperty?.CanWrite.ShouldBeFalse();
        sensitiveProperty?.CanWrite.ShouldBeFalse();
        
        _output.WriteLine("All properties are read-only as expected for a record type");
    }

    [Fact]
    public void RecordShouldSupportStructuredLogging()
    {
        // Arrange
        var metrics = new PerformanceMetrics(250.75, 150, "DatabaseQuery", null);

        // Act - Simulate structured logging scenarios
        var structuredData = new
        {
            metrics.Duration,
            metrics.ItemsProcessed,
            metrics.OperationType,
            metrics.SensitiveData
        };

        // Assert
        structuredData.Duration.ShouldBe(250.75);
        structuredData.ItemsProcessed.ShouldBe(150);
        structuredData.OperationType.ShouldBe("DatabaseQuery");
        structuredData.SensitiveData.ShouldBeNull();
        
        _output.WriteLine($"Structured logging data: Duration={structuredData.Duration}, " +
                         $"Items={structuredData.ItemsProcessed}, " +
                         $"Type={structuredData.OperationType}, " +
                         $"Sensitive={structuredData.SensitiveData ?? "null"}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ValidOperationType")]
    public void OperationTypeCanBeNullOrEmpty(string? operationType)
    {
        // Arrange & Act
        var metrics = new PerformanceMetrics(100.0, 10, operationType!);

        // Assert
        metrics.OperationType.ShouldBe(operationType);
        
        // ToString should still work even with null/empty operation type
        var stringResult = metrics.ToString();
        stringResult.ShouldNotBeNull();
        stringResult.ShouldContain($"Type: {operationType ?? ""}");
        
        _output.WriteLine($"Operation type '{operationType ?? "null"}' handled correctly: {stringResult}");
    }

    [Fact]
    public void ExtremeValuesShouldBeHandled()
    {
        // Arrange & Act
        var extremeMetrics = new PerformanceMetrics(
            double.MaxValue,
            int.MaxValue,
            new string('A', 1000), // Very long string
            new string('S', 500)   // Long sensitive data
        );

        // Assert
        extremeMetrics.Duration.ShouldBe(double.MaxValue);
        extremeMetrics.ItemsProcessed.ShouldBe(int.MaxValue);
        extremeMetrics.OperationType.Length.ShouldBe(1000);
        extremeMetrics.SensitiveData!.Length.ShouldBe(500);
        
        // ToString should still work with extreme values
        var result = extremeMetrics.ToString();
        result.ShouldNotBeNull();
        result.ShouldContain("Duration:");
        result.ShouldContain("Items:");
        result.ShouldContain("Type:");
        
        _output.WriteLine($"Extreme values handled - String length: {result.Length}");
    }
}