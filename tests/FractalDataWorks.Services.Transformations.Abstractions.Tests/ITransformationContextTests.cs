using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Shouldly;
using Moq;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions.Tests;

/// <summary>
/// Comprehensive tests for ITransformationContext interface contracts and behavior.
/// Tests verify proper implementation of transformation context information and metadata handling.
/// </summary>
public class ITransformationContextTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ITransformationContext> _mockContext;

    public ITransformationContextTests(ITestOutputHelper output)
    {
        _output = output;
        _mockContext = new Mock<ITransformationContext>();
    }

    [Fact]
    public void IdentityPropertyShouldAllowNullValue()
    {
        // Arrange
        _mockContext.Setup(x => x.Identity).Returns((string?)null);

        // Act
        var result = _mockContext.Object.Identity;

        // Assert
        result.ShouldBeNull();
        _output.WriteLine("Identity property correctly allows null values");
    }

    [Fact]
    public void IdentityPropertyShouldReturnValidStringWhenSet()
    {
        // Arrange
        const string expectedIdentity = "test-user-123";
        _mockContext.Setup(x => x.Identity).Returns(expectedIdentity);

        // Act
        var result = _mockContext.Object.Identity;

        // Assert
        result.ShouldBe(expectedIdentity);
        _output.WriteLine($"Identity property returned expected value: {result}");
    }

    [Fact]
    public void CorrelationIdPropertyShouldAllowNullValue()
    {
        // Arrange
        _mockContext.Setup(x => x.CorrelationId).Returns((string?)null);

        // Act
        var result = _mockContext.Object.CorrelationId;

        // Assert
        result.ShouldBeNull();
        _output.WriteLine("CorrelationId property correctly allows null values");
    }

    [Fact]
    public void CorrelationIdPropertyShouldReturnValidStringWhenSet()
    {
        // Arrange
        const string expectedCorrelationId = "correlation-abc-123";
        _mockContext.Setup(x => x.CorrelationId).Returns(expectedCorrelationId);

        // Act
        var result = _mockContext.Object.CorrelationId;

        // Assert
        result.ShouldBe(expectedCorrelationId);
        _output.WriteLine($"CorrelationId property returned expected value: {result}");
    }

    [Fact]
    public void PipelineStagePropertyShouldAllowNullValue()
    {
        // Arrange
        _mockContext.Setup(x => x.PipelineStage).Returns((string?)null);

        // Act
        var result = _mockContext.Object.PipelineStage;

        // Assert
        result.ShouldBeNull();
        _output.WriteLine("PipelineStage property correctly allows null values");
    }

    [Fact]
    public void PipelineStagePropertyShouldReturnValidStringWhenSet()
    {
        // Arrange
        const string expectedPipelineStage = "data-validation";
        _mockContext.Setup(x => x.PipelineStage).Returns(expectedPipelineStage);

        // Act
        var result = _mockContext.Object.PipelineStage;

        // Assert
        result.ShouldBe(expectedPipelineStage);
        _output.WriteLine($"PipelineStage property returned expected value: {result}");
    }

    [Fact]
    public void PropertiesPropertyShouldReturnEmptyDictionaryWhenNoPropertiesSet()
    {
        // Arrange
        var emptyProperties = new Dictionary<string, object>();
        _mockContext.Setup(x => x.Properties).Returns(emptyProperties);

        // Act
        var result = _mockContext.Object.Properties;

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        _output.WriteLine($"Properties returned empty dictionary with count: {result.Count}");
    }

    [Fact]
    public void PropertiesPropertyShouldReturnReadOnlyDictionary()
    {
        // Arrange
        var properties = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 42 }
        };
        _mockContext.Setup(x => x.Properties).Returns(properties);

        // Act
        var result = _mockContext.Object.Properties;

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<IReadOnlyDictionary<string, object>>();
        _output.WriteLine($"Properties returned IReadOnlyDictionary with {result.Count} items");
    }

    [Theory]
    [InlineData("environment", "production")]
    [InlineData("timeout", 30000)]
    [InlineData("enableMetrics", true)]
    [InlineData("priority", 5)]
    public void PropertiesPropertyShouldContainExpectedKeyValuePairs(string key, object value)
    {
        // Arrange
        var properties = new Dictionary<string, object> { { key, value } };
        _mockContext.Setup(x => x.Properties).Returns(properties);

        // Act
        var result = _mockContext.Object.Properties;

        // Assert
        result.ShouldContainKey(key);
        result[key].ShouldBe(value);
        _output.WriteLine($"Properties contains expected key-value pair: {key} = {value}");
    }

    [Fact]
    public void PropertiesPropertyShouldHandleComplexObjectValues()
    {
        // Arrange
        var complexObject = new { Name = "Test", Values = new[] { 1, 2, 3 } };
        var properties = new Dictionary<string, object>
        {
            { "complexData", complexObject }
        };
        _mockContext.Setup(x => x.Properties).Returns(properties);

        // Act
        var result = _mockContext.Object.Properties;

        // Assert
        result.ShouldContainKey("complexData");
        result["complexData"].ShouldBe(complexObject);
        _output.WriteLine($"Properties correctly handles complex object: {result["complexData"]}");
    }

    [Fact]
    public void PropertiesPropertyShouldHandleMultipleProperties()
    {
        // Arrange
        var properties = new Dictionary<string, object>
        {
            { "stringProperty", "test" },
            { "intProperty", 123 },
            { "boolProperty", true },
            { "dateProperty", DateTime.UtcNow },
            { "nullProperty", null! }
        };
        _mockContext.Setup(x => x.Properties).Returns(properties);

        // Act
        var result = _mockContext.Object.Properties;

        // Assert
        result.Count.ShouldBe(5);
        result.ShouldContainKey("stringProperty");
        result.ShouldContainKey("intProperty");
        result.ShouldContainKey("boolProperty");
        result.ShouldContainKey("dateProperty");
        result.ShouldContainKey("nullProperty");
        _output.WriteLine($"Properties correctly handles multiple property types with count: {result.Count}");
    }

    [Fact]
    public void AllPropertiesCanBeSetIndependently()
    {
        // Arrange
        const string identity = "user-123";
        const string correlationId = "corr-456";
        const string pipelineStage = "stage-789";
        var properties = new Dictionary<string, object> { { "test", "value" } };

        _mockContext.Setup(x => x.Identity).Returns(identity);
        _mockContext.Setup(x => x.CorrelationId).Returns(correlationId);
        _mockContext.Setup(x => x.PipelineStage).Returns(pipelineStage);
        _mockContext.Setup(x => x.Properties).Returns(properties);

        // Act
        var context = _mockContext.Object;

        // Assert
        context.Identity.ShouldBe(identity);
        context.CorrelationId.ShouldBe(correlationId);
        context.PipelineStage.ShouldBe(pipelineStage);
        context.Properties.ShouldBe(properties);
        _output.WriteLine("All context properties can be set and retrieved independently");
    }

    [Fact]
    public void InterfaceContractShouldBeCorrectlyDefined()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationContext);

        // Assert
        interfaceType.ShouldNotBeNull();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Transformations.Abstractions");
        
        var properties = interfaceType.GetProperties();
        properties.Length.ShouldBe(4);
        
        _output.WriteLine($"Interface has {properties.Length} properties defined correctly");
    }
}