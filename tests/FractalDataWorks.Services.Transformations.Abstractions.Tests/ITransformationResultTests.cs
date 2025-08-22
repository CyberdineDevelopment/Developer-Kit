using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Shouldly;
using Moq;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions.Tests;

/// <summary>
/// Comprehensive tests for ITransformationResult interface contracts and behavior.
/// Tests verify proper implementation of transformation result properties and data handling.
/// </summary>
public class ITransformationResultTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ITransformationResult> _mockResult;

    public ITransformationResultTests(ITestOutputHelper output)
    {
        _output = output;
        _mockResult = new Mock<ITransformationResult>();
    }

    [Fact]
    public void DataPropertyShouldAllowNullValue()
    {
        // Arrange
        _mockResult.Setup(x => x.Data).Returns((object?)null);

        // Act
        var result = _mockResult.Object.Data;

        // Assert
        result.ShouldBeNull();
        _output.WriteLine("Data property correctly allows null values");
    }

    [Theory]
    [InlineData("test string")]
    [InlineData(123)]
    [InlineData(true)]
    [InlineData(45.67)]
    [InlineData('A')]
    public void DataPropertyShouldAcceptVariousDataTypes(object data)
    {
        // Arrange
        _mockResult.Setup(x => x.Data).Returns(data);

        // Act
        var result = _mockResult.Object.Data;

        // Assert
        result.ShouldBe(data);
        _output.WriteLine($"Data correctly accepts data type {data.GetType().Name}: {result}");
    }

    [Fact]
    public void DataPropertyShouldHandleComplexObjects()
    {
        // Arrange
        var complexObject = new { Name = "Test", Values = new[] { 1, 2, 3 }, Created = DateTime.UtcNow };
        _mockResult.Setup(x => x.Data).Returns(complexObject);

        // Act
        var result = _mockResult.Object.Data;

        // Assert
        result.ShouldBe(complexObject);
        _output.WriteLine($"Data correctly handles complex object: {result}");
    }

    [Fact]
    public void DataPropertyShouldHandleCollections()
    {
        // Arrange
        var collection = new List<string> { "item1", "item2", "item3" };
        _mockResult.Setup(x => x.Data).Returns(collection);

        // Act
        var result = _mockResult.Object.Data;

        // Assert
        result.ShouldBe(collection);
        result.ShouldBeAssignableTo<IList<string>>();
        _output.WriteLine($"Data correctly handles collection with {((IList<string>)result!).Count} items");
    }

    [Fact]
    public void DataPropertyShouldHandleDictionaries()
    {
        // Arrange
        var dictionary = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 },
            { "key3", true }
        };
        _mockResult.Setup(x => x.Data).Returns(dictionary);

        // Act
        var result = _mockResult.Object.Data;

        // Assert
        result.ShouldBe(dictionary);
        result.ShouldBeAssignableTo<IDictionary<string, object>>();
        _output.WriteLine($"Data correctly handles dictionary with {((IDictionary<string, object>)result!).Count} items");
    }

    [Theory]
    [InlineData("JSON")]
    [InlineData("XML")]
    [InlineData("CSV")]
    [InlineData("Binary")]
    [InlineData("Object")]
    [InlineData("String")]
    [InlineData("Stream")]
    public void OutputTypePropertyShouldAcceptValidOutputTypes(string outputType)
    {
        // Arrange
        _mockResult.Setup(x => x.OutputType).Returns(outputType);

        // Act
        var result = _mockResult.Object.OutputType;

        // Assert
        result.ShouldBe(outputType);
        result.ShouldNotBeNullOrEmpty();
        _output.WriteLine($"OutputType correctly accepts value: {result}");
    }

    [Fact]
    public void MetadataPropertyShouldReturnEmptyDictionaryWhenNoMetadataSet()
    {
        // Arrange
        var emptyMetadata = new Dictionary<string, object?>();
        _mockResult.Setup(x => x.Metadata).Returns(emptyMetadata);

        // Act
        var result = _mockResult.Object.Metadata;

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        result.ShouldBeAssignableTo<IReadOnlyDictionary<string, object?>>();
        _output.WriteLine($"Metadata returned empty dictionary with count: {result.Count}");
    }

    [Theory]
    [InlineData("transformationType", "JsonToXml")]
    [InlineData("version", "1.0")]
    [InlineData("processingTime", 125)]
    [InlineData("success", true)]
    [InlineData("warnings", null)]
    public void MetadataPropertyShouldContainExpectedKeyValuePairs(string key, object? value)
    {
        // Arrange
        var metadata = new Dictionary<string, object?> { { key, value } };
        _mockResult.Setup(x => x.Metadata).Returns(metadata);

        // Act
        var result = _mockResult.Object.Metadata;

        // Assert
        result.ShouldContainKey(key);
        result[key].ShouldBe(value);
        _output.WriteLine($"Metadata contains expected key-value pair: {key} = {value}");
    }

    [Fact]
    public void MetadataPropertyShouldHandleMultipleMetadataItems()
    {
        // Arrange
        var metadata = new Dictionary<string, object?>
        {
            { "transformationType", "JsonToXml" },
            { "version", "1.0" },
            { "processingTime", 125 },
            { "success", true },
            { "warnings", null },
            { "itemsProcessed", 1000L },
            { "memoryUsed", 2048.5 }
        };
        _mockResult.Setup(x => x.Metadata).Returns(metadata);

        // Act
        var result = _mockResult.Object.Metadata;

        // Assert
        result.Count.ShouldBe(7);
        result.ShouldContainKey("transformationType");
        result.ShouldContainKey("version");
        result.ShouldContainKey("processingTime");
        result.ShouldContainKey("success");
        result.ShouldContainKey("warnings");
        result.ShouldContainKey("itemsProcessed");
        result.ShouldContainKey("memoryUsed");
        _output.WriteLine($"Metadata correctly handles multiple metadata items with count: {result.Count}");
    }

    [Fact]
    public void MetadataPropertyShouldHandleComplexMetadataValues()
    {
        // Arrange
        var complexMetadata = new
        {
            ProcessingSteps = new[] { "Parse", "Transform", "Validate" },
            Statistics = new { ItemsProcessed = 100, Errors = 2 },
            Timestamp = DateTime.UtcNow
        };
        var metadata = new Dictionary<string, object?> { { "complexData", complexMetadata } };
        _mockResult.Setup(x => x.Metadata).Returns(metadata);

        // Act
        var result = _mockResult.Object.Metadata;

        // Assert
        result.ShouldContainKey("complexData");
        result["complexData"].ShouldBe(complexMetadata);
        _output.WriteLine($"Metadata correctly handles complex metadata values: {result["complexData"]}");
    }

    [Fact]
    public void DurationMsPropertyShouldReturnZeroForInstantaneousOperations()
    {
        // Arrange
        _mockResult.Setup(x => x.DurationMs).Returns(0L);

        // Act
        var result = _mockResult.Object.DurationMs;

        // Assert
        result.ShouldBe(0L);
        result.ShouldBeGreaterThanOrEqualTo(0L);
        _output.WriteLine($"DurationMs property returned zero for instantaneous operation: {result}ms");
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(10L)]
    [InlineData(100L)]
    [InlineData(1000L)]
    [InlineData(5000L)]
    [InlineData(30000L)]
    [InlineData(long.MaxValue)]
    public void DurationMsPropertyShouldAcceptValidDurationValues(long durationMs)
    {
        // Arrange
        _mockResult.Setup(x => x.DurationMs).Returns(durationMs);

        // Act
        var result = _mockResult.Object.DurationMs;

        // Assert
        result.ShouldBe(durationMs);
        result.ShouldBeGreaterThanOrEqualTo(0L);
        _output.WriteLine($"DurationMs correctly accepts duration value: {result}ms");
    }

    [Theory]
    [InlineData(50L)]    // Fast operation
    [InlineData(500L)]   // Moderate operation
    [InlineData(5000L)]  // Slow operation
    [InlineData(30000L)] // Very slow operation
    public void DurationMsPropertyShouldRepresentRealisticProcessingTimes(long durationMs)
    {
        // Arrange
        _mockResult.Setup(x => x.DurationMs).Returns(durationMs);

        // Act
        var result = _mockResult.Object.DurationMs;

        // Assert
        result.ShouldBe(durationMs);
        
        var category = durationMs switch
        {
            < 100 => "Fast",
            < 1000 => "Moderate",
            < 10000 => "Slow",
            _ => "Very Slow"
        };
        
        _output.WriteLine($"DurationMs represents {category} processing time: {result}ms");
    }

    [Fact]
    public void AllPropertiesShouldBeAccessibleIndependently()
    {
        // Arrange
        var data = new { Result = "Transformed data", Count = 42 };
        const string outputType = "JSON";
        var metadata = new Dictionary<string, object?>
        {
            { "transformation", "successful" },
            { "itemsProcessed", 100 },
            { "warnings", null }
        };
        const long durationMs = 1250L;

        _mockResult.Setup(x => x.Data).Returns(data);
        _mockResult.Setup(x => x.OutputType).Returns(outputType);
        _mockResult.Setup(x => x.Metadata).Returns(metadata);
        _mockResult.Setup(x => x.DurationMs).Returns(durationMs);

        // Act
        var result = _mockResult.Object;

        // Assert
        result.Data.ShouldBe(data);
        result.OutputType.ShouldBe(outputType);
        result.Metadata.ShouldBe(metadata);
        result.DurationMs.ShouldBe(durationMs);
        _output.WriteLine("All transformation result properties are accessible and return expected values independently");
    }

    [Fact]
    public void InterfaceContractShouldBeCorrectlyDefined()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationResult);

        // Assert
        interfaceType.ShouldNotBeNull();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Transformations.Abstractions");
        
        var properties = interfaceType.GetProperties();
        properties.Length.ShouldBe(4);
        
        _output.WriteLine($"Interface has {properties.Length} properties defined correctly");
    }

    [Theory]
    [InlineData("JSON", 0L)]
    [InlineData("XML", 100L)]
    [InlineData("CSV", 500L)]
    [InlineData("Object", 1000L)]
    [InlineData("Binary", 2500L)]
    public void ResultShouldSupportVariousOutputTypesAndDurations(string outputType, long durationMs)
    {
        // Arrange
        var data = $"Transformed data in {outputType} format";
        var metadata = new Dictionary<string, object?>
        {
            { "outputFormat", outputType },
            { "processingTime", durationMs }
        };

        _mockResult.Setup(x => x.Data).Returns(data);
        _mockResult.Setup(x => x.OutputType).Returns(outputType);
        _mockResult.Setup(x => x.Metadata).Returns(metadata);
        _mockResult.Setup(x => x.DurationMs).Returns(durationMs);

        // Act
        var result = _mockResult.Object;

        // Assert
        result.Data.ShouldBe(data);
        result.OutputType.ShouldBe(outputType);
        result.Metadata.ShouldBe(metadata);
        result.DurationMs.ShouldBe(durationMs);
        _output.WriteLine($"Result supports output type {outputType} with duration {durationMs}ms");
    }

    [Fact]
    public void ResultShouldHandleSuccessfulTransformationScenario()
    {
        // Arrange
        var transformedData = new Dictionary<string, object>
        {
            { "customers", new[] { "Alice", "Bob", "Charlie" } },
            { "totalCount", 3 },
            { "timestamp", DateTime.UtcNow }
        };
        const string outputType = "JSON";
        var metadata = new Dictionary<string, object?>
        {
            { "transformationType", "CsvToJson" },
            { "success", true },
            { "recordsProcessed", 3 },
            { "errors", 0 },
            { "warnings", null }
        };
        const long durationMs = 125L;

        _mockResult.Setup(x => x.Data).Returns(transformedData);
        _mockResult.Setup(x => x.OutputType).Returns(outputType);
        _mockResult.Setup(x => x.Metadata).Returns(metadata);
        _mockResult.Setup(x => x.DurationMs).Returns(durationMs);

        // Act
        var result = _mockResult.Object;

        // Assert
        result.Data.ShouldBe(transformedData);
        result.OutputType.ShouldBe(outputType);
        result.Metadata["success"].ShouldBe(true);
        result.Metadata["errors"].ShouldBe(0);
        result.DurationMs.ShouldBe(durationMs);
        _output.WriteLine($"Result correctly represents successful transformation scenario with {durationMs}ms duration");
    }

    [Fact]
    public void ResultShouldHandleEmptyDataScenario()
    {
        // Arrange
        var emptyData = Array.Empty<object>();
        const string outputType = "Array";
        var metadata = new Dictionary<string, object?>
        {
            { "transformationType", "FilterOperation" },
            { "success", true },
            { "inputRecords", 1000 },
            { "outputRecords", 0 },
            { "filterCriteria", "age > 100" }
        };
        const long durationMs = 50L;

        _mockResult.Setup(x => x.Data).Returns(emptyData);
        _mockResult.Setup(x => x.OutputType).Returns(outputType);
        _mockResult.Setup(x => x.Metadata).Returns(metadata);
        _mockResult.Setup(x => x.DurationMs).Returns(durationMs);

        // Act
        var result = _mockResult.Object;

        // Assert
        result.Data.ShouldBe(emptyData);
        result.OutputType.ShouldBe(outputType);
        result.Metadata["inputRecords"].ShouldBe(1000);
        result.Metadata["outputRecords"].ShouldBe(0);
        result.DurationMs.ShouldBe(durationMs);
        _output.WriteLine($"Result correctly handles empty data scenario after filtering operation");
    }

    [Fact]
    public void ResultShouldHandleLargeDataScenario()
    {
        // Arrange
        var largeDataset = new List<int>();
        for (int i = 0; i < 10000; i++)
        {
            largeDataset.Add(i);
        }
        
        const string outputType = "Array";
        var metadata = new Dictionary<string, object?>
        {
            { "transformationType", "LargeDataProcessing" },
            { "success", true },
            { "recordsProcessed", 10000 },
            { "memoryUsageMb", 15.5 },
            { "compressionRatio", 0.75 }
        };
        const long durationMs = 2500L;

        _mockResult.Setup(x => x.Data).Returns(largeDataset);
        _mockResult.Setup(x => x.OutputType).Returns(outputType);
        _mockResult.Setup(x => x.Metadata).Returns(metadata);
        _mockResult.Setup(x => x.DurationMs).Returns(durationMs);

        // Act
        var result = _mockResult.Object;

        // Assert
        result.Data.ShouldBe(largeDataset);
        result.OutputType.ShouldBe(outputType);
        result.Metadata["recordsProcessed"].ShouldBe(10000);
        result.DurationMs.ShouldBe(durationMs);
        ((List<int>)result.Data!).Count.ShouldBe(10000);
        _output.WriteLine($"Result correctly handles large dataset with {((List<int>)result.Data!).Count:N0} items processed in {durationMs}ms");
    }
}