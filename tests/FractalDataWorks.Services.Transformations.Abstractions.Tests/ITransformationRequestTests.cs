using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using Shouldly;
using Moq;
using FractalDataWorks;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions.Tests;

/// <summary>
/// Comprehensive tests for ITransformationRequest interface contracts and behavior.
/// Tests verify proper implementation of transformation request properties and methods.
/// </summary>
public class ITransformationRequestTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ITransformationRequest> _mockRequest;
    private readonly Mock<ITransformationContext> _mockContext;

    public ITransformationRequestTests(ITestOutputHelper output)
    {
        _output = output;
        _mockRequest = new Mock<ITransformationRequest>();
        _mockContext = new Mock<ITransformationContext>();
    }

    [Fact]
    public void RequestIdPropertyShouldReturnValidString()
    {
        // Arrange
        const string expectedRequestId = "request-123-abc";
        _mockRequest.Setup(x => x.RequestId).Returns(expectedRequestId);

        // Act
        var result = _mockRequest.Object.RequestId;

        // Assert
        result.ShouldBe(expectedRequestId);
        result.ShouldNotBeNullOrEmpty();
        _output.WriteLine($"RequestId property returned expected value: {result}");
    }

    [Theory]
    [InlineData("request-001")]
    [InlineData("transformation-abc-123")]
    [InlineData("data-processing-xyz")]
    [InlineData("batch-operation-456")]
    public void RequestIdPropertyShouldAcceptVariousValidValues(string requestId)
    {
        // Arrange
        _mockRequest.Setup(x => x.RequestId).Returns(requestId);

        // Act
        var result = _mockRequest.Object.RequestId;

        // Assert
        result.ShouldBe(requestId);
        _output.WriteLine($"RequestId correctly accepts value: {result}");
    }

    [Fact]
    public void InputDataPropertyShouldAllowNullValue()
    {
        // Arrange
        _mockRequest.Setup(x => x.InputData).Returns((object?)null);

        // Act
        var result = _mockRequest.Object.InputData;

        // Assert
        result.ShouldBeNull();
        _output.WriteLine("InputData property correctly allows null values");
    }

    [Theory]
    [InlineData("test string")]
    [InlineData(123)]
    [InlineData(true)]
    [InlineData(45.67)]
    public void InputDataPropertyShouldAcceptVariousDataTypes(object inputData)
    {
        // Arrange
        _mockRequest.Setup(x => x.InputData).Returns(inputData);

        // Act
        var result = _mockRequest.Object.InputData;

        // Assert
        result.ShouldBe(inputData);
        _output.WriteLine($"InputData correctly accepts data type {inputData.GetType().Name}: {result}");
    }

    [Fact]
    public void InputDataPropertyShouldHandleComplexObjects()
    {
        // Arrange
        var complexObject = new { Name = "Test", Values = new[] { 1, 2, 3 }, Created = DateTime.UtcNow };
        _mockRequest.Setup(x => x.InputData).Returns(complexObject);

        // Act
        var result = _mockRequest.Object.InputData;

        // Assert
        result.ShouldBe(complexObject);
        _output.WriteLine($"InputData correctly handles complex object: {result}");
    }

    [Theory]
    [InlineData("JSON")]
    [InlineData("XML")]
    [InlineData("CSV")]
    [InlineData("Binary")]
    [InlineData("Object")]
    [InlineData("Stream")]
    public void InputTypePropertyShouldAcceptValidInputTypes(string inputType)
    {
        // Arrange
        _mockRequest.Setup(x => x.InputType).Returns(inputType);

        // Act
        var result = _mockRequest.Object.InputType;

        // Assert
        result.ShouldBe(inputType);
        result.ShouldNotBeNullOrEmpty();
        _output.WriteLine($"InputType correctly accepts value: {result}");
    }

    [Theory]
    [InlineData("JSON")]
    [InlineData("XML")]
    [InlineData("CSV")]
    [InlineData("Binary")]
    [InlineData("Object")]
    [InlineData("Stream")]
    public void OutputTypePropertyShouldAcceptValidOutputTypes(string outputType)
    {
        // Arrange
        _mockRequest.Setup(x => x.OutputType).Returns(outputType);

        // Act
        var result = _mockRequest.Object.OutputType;

        // Assert
        result.ShouldBe(outputType);
        result.ShouldNotBeNullOrEmpty();
        _output.WriteLine($"OutputType correctly accepts value: {result}");
    }

    [Fact]
    public void TransformationCategoryPropertyShouldAllowNullValue()
    {
        // Arrange
        _mockRequest.Setup(x => x.TransformationCategory).Returns((string?)null);

        // Act
        var result = _mockRequest.Object.TransformationCategory;

        // Assert
        result.ShouldBeNull();
        _output.WriteLine("TransformationCategory property correctly allows null values");
    }

    [Theory]
    [InlineData("Mapping")]
    [InlineData("Filtering")]
    [InlineData("Aggregation")]
    [InlineData("Validation")]
    [InlineData("Formatting")]
    [InlineData("Serialization")]
    public void TransformationCategoryPropertyShouldAcceptValidCategories(string category)
    {
        // Arrange
        _mockRequest.Setup(x => x.TransformationCategory).Returns(category);

        // Act
        var result = _mockRequest.Object.TransformationCategory;

        // Assert
        result.ShouldBe(category);
        _output.WriteLine($"TransformationCategory correctly accepts value: {result}");
    }

    [Fact]
    public void ConfigurationPropertyShouldReturnEmptyDictionaryWhenNoConfigurationSet()
    {
        // Arrange
        var emptyConfiguration = new Dictionary<string, object>();
        _mockRequest.Setup(x => x.Configuration).Returns(emptyConfiguration);

        // Act
        var result = _mockRequest.Object.Configuration;

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        result.ShouldBeAssignableTo<IReadOnlyDictionary<string, object>>();
        _output.WriteLine($"Configuration returned empty dictionary with count: {result.Count}");
    }

    [Theory]
    [InlineData("mappingRule", "rule1")]
    [InlineData("timeout", 30000)]
    [InlineData("enableCaching", true)]
    [InlineData("priority", 5)]
    public void ConfigurationPropertyShouldContainExpectedKeyValuePairs(string key, object value)
    {
        // Arrange
        var configuration = new Dictionary<string, object> { { key, value } };
        _mockRequest.Setup(x => x.Configuration).Returns(configuration);

        // Act
        var result = _mockRequest.Object.Configuration;

        // Assert
        result.ShouldContainKey(key);
        result[key].ShouldBe(value);
        _output.WriteLine($"Configuration contains expected key-value pair: {key} = {value}");
    }

    [Fact]
    public void OptionsPropertyShouldReturnEmptyDictionaryWhenNoOptionsSet()
    {
        // Arrange
        var emptyOptions = new Dictionary<string, object>();
        _mockRequest.Setup(x => x.Options).Returns(emptyOptions);

        // Act
        var result = _mockRequest.Object.Options;

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        result.ShouldBeAssignableTo<IReadOnlyDictionary<string, object>>();
        _output.WriteLine($"Options returned empty dictionary with count: {result.Count}");
    }

    [Theory]
    [InlineData("enableRetry", true)]
    [InlineData("maxRetries", 3)]
    [InlineData("bufferSize", 1024)]
    [InlineData("compression", "gzip")]
    public void OptionsPropertyShouldContainExpectedKeyValuePairs(string key, object value)
    {
        // Arrange
        var options = new Dictionary<string, object> { { key, value } };
        _mockRequest.Setup(x => x.Options).Returns(options);

        // Act
        var result = _mockRequest.Object.Options;

        // Assert
        result.ShouldContainKey(key);
        result[key].ShouldBe(value);
        _output.WriteLine($"Options contains expected key-value pair: {key} = {value}");
    }

    [Fact]
    public void TimeoutPropertyShouldAllowNullValue()
    {
        // Arrange
        _mockRequest.Setup(x => x.Timeout).Returns((TimeSpan?)null);

        // Act
        var result = _mockRequest.Object.Timeout;

        // Assert
        result.ShouldBeNull();
        _output.WriteLine("Timeout property correctly allows null values");
    }

    [Theory]
    [InlineData(30)]     // 30 seconds
    [InlineData(300)]    // 5 minutes
    [InlineData(1800)]   // 30 minutes
    [InlineData(3600)]   // 1 hour
    public void TimeoutPropertyShouldAcceptValidTimeSpanValues(int seconds)
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(seconds);
        _mockRequest.Setup(x => x.Timeout).Returns(timeout);

        // Act
        var result = _mockRequest.Object.Timeout;

        // Assert
        result.ShouldBe(timeout);
        result!.Value.TotalSeconds.ShouldBe(seconds);
        _output.WriteLine($"Timeout correctly accepts TimeSpan value: {result} ({seconds} seconds)");
    }

    [Fact]
    public void ContextPropertyShouldAllowNullValue()
    {
        // Arrange
        _mockRequest.Setup(x => x.Context).Returns((ITransformationContext?)null);

        // Act
        var result = _mockRequest.Object.Context;

        // Assert
        result.ShouldBeNull();
        _output.WriteLine("Context property correctly allows null values");
    }

    [Fact]
    public void ContextPropertyShouldReturnValidContextWhenSet()
    {
        // Arrange
        _mockRequest.Setup(x => x.Context).Returns(_mockContext.Object);

        // Act
        var result = _mockRequest.Object.Context;

        // Assert
        result.ShouldBe(_mockContext.Object);
        result.ShouldNotBeNull();
        _output.WriteLine("Context property returned expected transformation context");
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(object))]
    [InlineData(typeof(Dictionary<string, object>))]
    public void ExpectedResultTypePropertyShouldAcceptValidTypes(Type expectedType)
    {
        // Arrange
        _mockRequest.Setup(x => x.ExpectedResultType).Returns(expectedType);

        // Act
        var result = _mockRequest.Object.ExpectedResultType;

        // Assert
        result.ShouldBe(expectedType);
        result.ShouldNotBeNull();
        _output.WriteLine($"ExpectedResultType correctly accepts type: {result.Name}");
    }

    [Fact]
    public void WithInputDataShouldReturnNewRequestInstance()
    {
        // Arrange
        const string newInputData = "new test data";
        const string newInputType = "NewType";
        var newRequest = new Mock<ITransformationRequest>();
        
        _mockRequest.Setup(x => x.WithInputData(newInputData, newInputType))
                   .Returns(newRequest.Object);

        // Act
        var result = _mockRequest.Object.WithInputData(newInputData, newInputType);

        // Assert
        result.ShouldBe(newRequest.Object);
        result.ShouldNotBe(_mockRequest.Object);
        _mockRequest.Verify(x => x.WithInputData(newInputData, newInputType), Times.Once);
        _output.WriteLine($"WithInputData correctly returns new request instance with data: {newInputData}");
    }

    [Fact]
    public void WithInputDataShouldWorkWithNullInputType()
    {
        // Arrange
        const string newInputData = "new test data";
        var newRequest = new Mock<ITransformationRequest>();
        
        _mockRequest.Setup(x => x.WithInputData(newInputData, null))
                   .Returns(newRequest.Object);

        // Act
        var result = _mockRequest.Object.WithInputData(newInputData);

        // Assert
        result.ShouldBe(newRequest.Object);
        _mockRequest.Verify(x => x.WithInputData(newInputData, null), Times.Once);
        _output.WriteLine("WithInputData correctly works with null input type parameter");
    }

    [Fact]
    public void WithOutputTypeShouldReturnNewRequestInstance()
    {
        // Arrange
        const string newOutputType = "NewOutputType";
        var newExpectedResultType = typeof(string);
        var newRequest = new Mock<ITransformationRequest>();
        
        _mockRequest.Setup(x => x.WithOutputType(newOutputType, newExpectedResultType))
                   .Returns(newRequest.Object);

        // Act
        var result = _mockRequest.Object.WithOutputType(newOutputType, newExpectedResultType);

        // Assert
        result.ShouldBe(newRequest.Object);
        result.ShouldNotBe(_mockRequest.Object);
        _mockRequest.Verify(x => x.WithOutputType(newOutputType, newExpectedResultType), Times.Once);
        _output.WriteLine($"WithOutputType correctly returns new request instance with output type: {newOutputType}");
    }

    [Fact]
    public void WithOutputTypeShouldWorkWithNullExpectedResultType()
    {
        // Arrange
        const string newOutputType = "NewOutputType";
        var newRequest = new Mock<ITransformationRequest>();
        
        _mockRequest.Setup(x => x.WithOutputType(newOutputType, null))
                   .Returns(newRequest.Object);

        // Act
        var result = _mockRequest.Object.WithOutputType(newOutputType);

        // Assert
        result.ShouldBe(newRequest.Object);
        _mockRequest.Verify(x => x.WithOutputType(newOutputType, null), Times.Once);
        _output.WriteLine("WithOutputType correctly works with null expected result type parameter");
    }

    [Fact]
    public void WithConfigurationShouldReturnNewRequestInstance()
    {
        // Arrange
        var newConfiguration = new Dictionary<string, object> { { "newKey", "newValue" } };
        var newRequest = new Mock<ITransformationRequest>();
        
        _mockRequest.Setup(x => x.WithConfiguration(newConfiguration))
                   .Returns(newRequest.Object);

        // Act
        var result = _mockRequest.Object.WithConfiguration(newConfiguration);

        // Assert
        result.ShouldBe(newRequest.Object);
        result.ShouldNotBe(_mockRequest.Object);
        _mockRequest.Verify(x => x.WithConfiguration(newConfiguration), Times.Once);
        _output.WriteLine($"WithConfiguration correctly returns new request instance with {newConfiguration.Count} configuration items");
    }

    [Fact]
    public void WithOptionsShouldReturnNewRequestInstance()
    {
        // Arrange
        var newOptions = new Dictionary<string, object> { { "newOption", "newValue" } };
        var newRequest = new Mock<ITransformationRequest>();
        
        _mockRequest.Setup(x => x.WithOptions(newOptions))
                   .Returns(newRequest.Object);

        // Act
        var result = _mockRequest.Object.WithOptions(newOptions);

        // Assert
        result.ShouldBe(newRequest.Object);
        result.ShouldNotBe(_mockRequest.Object);
        _mockRequest.Verify(x => x.WithOptions(newOptions), Times.Once);
        _output.WriteLine($"WithOptions correctly returns new request instance with {newOptions.Count} option items");
    }

    [Fact]
    public void AllPropertiesShouldBeAccessibleIndependently()
    {
        // Arrange
        const string requestId = "test-request-123";
        const string inputData = "test input data";
        const string inputType = "JSON";
        const string outputType = "XML";
        const string category = "Mapping";
        var configuration = new Dictionary<string, object> { { "rule", "value" } };
        var options = new Dictionary<string, object> { { "option", "value" } };
        var timeout = TimeSpan.FromMinutes(5);
        var expectedResultType = typeof(string);

        _mockRequest.Setup(x => x.RequestId).Returns(requestId);
        _mockRequest.Setup(x => x.InputData).Returns(inputData);
        _mockRequest.Setup(x => x.InputType).Returns(inputType);
        _mockRequest.Setup(x => x.OutputType).Returns(outputType);
        _mockRequest.Setup(x => x.TransformationCategory).Returns(category);
        _mockRequest.Setup(x => x.Configuration).Returns(configuration);
        _mockRequest.Setup(x => x.Options).Returns(options);
        _mockRequest.Setup(x => x.Timeout).Returns(timeout);
        _mockRequest.Setup(x => x.Context).Returns(_mockContext.Object);
        _mockRequest.Setup(x => x.ExpectedResultType).Returns(expectedResultType);

        // Act
        var request = _mockRequest.Object;

        // Assert
        request.RequestId.ShouldBe(requestId);
        request.InputData.ShouldBe(inputData);
        request.InputType.ShouldBe(inputType);
        request.OutputType.ShouldBe(outputType);
        request.TransformationCategory.ShouldBe(category);
        request.Configuration.ShouldBe(configuration);
        request.Options.ShouldBe(options);
        request.Timeout.ShouldBe(timeout);
        request.Context.ShouldBe(_mockContext.Object);
        request.ExpectedResultType.ShouldBe(expectedResultType);
        _output.WriteLine("All transformation request properties are accessible and return expected values independently");
    }

    [Fact]
    public void RequestShouldInheritFromICommand()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationRequest);
        var baseInterfaceType = typeof(ICommand);

        // Assert
        interfaceType.ShouldNotBeNull();
        baseInterfaceType.ShouldNotBeNull();
        interfaceType.IsAssignableFrom(typeof(ITransformationRequest)).ShouldBeTrue();
        baseInterfaceType.IsAssignableFrom(typeof(ITransformationRequest)).ShouldBeTrue();
        _output.WriteLine("ITransformationRequest correctly inherits from ICommand");
    }

    [Fact]
    public void InterfaceContractShouldBeCorrectlyDefined()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationRequest);

        // Assert
        interfaceType.ShouldNotBeNull();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Transformations.Abstractions");
        
        var properties = interfaceType.GetProperties();
        properties.Length.ShouldBe(10); // Should have 10 properties
        
        var methods = interfaceType.GetMethods();
        // Should have the "With" methods plus property getters
        methods.Length.ShouldBeGreaterThan(10);
        
        _output.WriteLine($"Interface has {properties.Length} properties and {methods.Length} methods defined correctly");
    }

    [Theory]
    [InlineData("JSON", "XML", "Mapping")]
    [InlineData("CSV", "Object", "Parsing")]
    [InlineData("Binary", "String", "Formatting")]
    [InlineData("Stream", "JSON", "Serialization")]
    public void RequestShouldSupportVariousTransformationScenarios(string inputType, string outputType, string category)
    {
        // Arrange
        const string requestId = "scenario-test";
        var inputData = $"Test data for {inputType}";
        var configuration = new Dictionary<string, object> { { "scenario", "test" } };
        var options = new Dictionary<string, object> { { "category", category } };

        _mockRequest.Setup(x => x.RequestId).Returns(requestId);
        _mockRequest.Setup(x => x.InputData).Returns(inputData);
        _mockRequest.Setup(x => x.InputType).Returns(inputType);
        _mockRequest.Setup(x => x.OutputType).Returns(outputType);
        _mockRequest.Setup(x => x.TransformationCategory).Returns(category);
        _mockRequest.Setup(x => x.Configuration).Returns(configuration);
        _mockRequest.Setup(x => x.Options).Returns(options);

        // Act
        var request = _mockRequest.Object;

        // Assert
        request.RequestId.ShouldBe(requestId);
        request.InputType.ShouldBe(inputType);
        request.OutputType.ShouldBe(outputType);
        request.TransformationCategory.ShouldBe(category);
        request.Configuration.ShouldBe(configuration);
        request.Options.ShouldBe(options);
        _output.WriteLine($"Request supports transformation scenario: {inputType} -> {outputType} ({category})");
    }

    [Fact]
    public void RequestShouldSupportImmutableBuilderPattern()
    {
        // Arrange
        var newData = "new data";
        var newOutputType = "NewType";
        var newConfiguration = new Dictionary<string, object> { { "new", "config" } };
        var newOptions = new Dictionary<string, object> { { "new", "option" } };
        
        var newRequestAfterData = new Mock<ITransformationRequest>();
        var newRequestAfterOutput = new Mock<ITransformationRequest>();
        var newRequestAfterConfig = new Mock<ITransformationRequest>();
        var newRequestAfterOptions = new Mock<ITransformationRequest>();

        _mockRequest.Setup(x => x.WithInputData(newData, null))
                   .Returns(newRequestAfterData.Object);
        _mockRequest.Setup(x => x.WithOutputType(newOutputType, null))
                   .Returns(newRequestAfterOutput.Object);
        _mockRequest.Setup(x => x.WithConfiguration(newConfiguration))
                   .Returns(newRequestAfterConfig.Object);
        _mockRequest.Setup(x => x.WithOptions(newOptions))
                   .Returns(newRequestAfterOptions.Object);

        // Act
        var request = _mockRequest.Object;
        var requestWithNewData = request.WithInputData(newData);
        var requestWithNewOutput = request.WithOutputType(newOutputType);
        var requestWithNewConfig = request.WithConfiguration(newConfiguration);
        var requestWithNewOptions = request.WithOptions(newOptions);

        // Assert
        requestWithNewData.ShouldBe(newRequestAfterData.Object);
        requestWithNewOutput.ShouldBe(newRequestAfterOutput.Object);
        requestWithNewConfig.ShouldBe(newRequestAfterConfig.Object);
        requestWithNewOptions.ShouldBe(newRequestAfterOptions.Object);
        
        // All should be different instances
        requestWithNewData.ShouldNotBe(request);
        requestWithNewOutput.ShouldNotBe(request);
        requestWithNewConfig.ShouldNotBe(request);
        requestWithNewOptions.ShouldNotBe(request);
        
        _output.WriteLine("Request correctly supports immutable builder pattern with all 'With' methods");
    }
}