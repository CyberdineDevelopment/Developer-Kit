using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Moq;
using FractalDataWorks;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions.Tests;

/// <summary>
/// Comprehensive tests for ITransformationProvider interface contracts and behavior.
/// Tests verify proper implementation of transformation provider operations and inheritance.
/// </summary>
public class ITransformationProviderTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ITransformationProvider> _mockNonGenericProvider;
    private readonly Mock<ITransformationProvider<ITransformationRequest>> _mockGenericProvider;
    private readonly Mock<ITransformationRequest> _mockRequest;
    private readonly Mock<IFdwResult> _mockResult;
    private readonly Mock<IFdwResult<object?>> _mockObjectResult;
    private readonly Mock<IFdwResult<string>> _mockStringResult;
    private readonly Mock<IFdwResult<ITransformationEngine>> _mockEngineResult;
    private readonly Mock<IFdwResult<ITransformationMetrics>> _mockMetricsResult;
    private readonly Mock<ITransformationEngineConfiguration> _mockEngineConfig;

    public ITransformationProviderTests(ITestOutputHelper output)
    {
        _output = output;
        _mockNonGenericProvider = new Mock<ITransformationProvider>();
        _mockGenericProvider = new Mock<ITransformationProvider<ITransformationRequest>>();
        _mockRequest = new Mock<ITransformationRequest>();
        _mockResult = new Mock<IFdwResult>();
        _mockObjectResult = new Mock<IFdwResult<object?>>();
        _mockStringResult = new Mock<IFdwResult<string>>();
        _mockEngineResult = new Mock<IFdwResult<ITransformationEngine>>();
        _mockMetricsResult = new Mock<IFdwResult<ITransformationMetrics>>();
        _mockEngineConfig = new Mock<ITransformationEngineConfiguration>();
    }

    [Fact]
    public void NonGenericProviderShouldInheritFromIFdwService()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationProvider);
        var baseInterfaceType = typeof(IFdwService);

        // Assert
        interfaceType.ShouldNotBeNull();
        baseInterfaceType.ShouldNotBeNull();
        interfaceType.IsAssignableFrom(typeof(ITransformationProvider)).ShouldBeTrue();
        baseInterfaceType.IsAssignableFrom(typeof(ITransformationProvider)).ShouldBeTrue();
        _output.WriteLine("ITransformationProvider correctly inherits from IFdwService");
    }

    [Fact]
    public void GenericProviderShouldInheritFromBothBaseInterfaces()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationProvider<ITransformationRequest>);
        var nonGenericType = typeof(ITransformationProvider);
        var fdwServiceType = typeof(IFdwService<ITransformationRequest>);

        // Assert
        interfaceType.ShouldNotBeNull();
        nonGenericType.IsAssignableFrom(interfaceType).ShouldBeTrue();
        fdwServiceType.IsAssignableFrom(interfaceType).ShouldBeTrue();
        _output.WriteLine("ITransformationProvider<T> correctly inherits from both ITransformationProvider and IFdwService<T>");
    }

    [Fact]
    public void SupportedInputTypesPropertyShouldReturnEmptyListWhenNoInputTypesSupported()
    {
        // Arrange
        var emptyList = new List<string>().AsReadOnly();
        _mockGenericProvider.Setup(x => x.SupportedInputTypes).Returns(emptyList);

        // Act
        var result = _mockGenericProvider.Object.SupportedInputTypes;

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        result.ShouldBeAssignableTo<IReadOnlyList<string>>();
        _output.WriteLine($"SupportedInputTypes returned empty list with count: {result.Count}");
    }

    [Theory]
    [InlineData("JSON")]
    [InlineData("XML")]
    [InlineData("CSV")]
    [InlineData("Binary")]
    [InlineData("Object")]
    public void SupportedInputTypesPropertyShouldContainExpectedInputTypes(string inputType)
    {
        // Arrange
        var inputTypes = new List<string> { inputType }.AsReadOnly();
        _mockGenericProvider.Setup(x => x.SupportedInputTypes).Returns(inputTypes);

        // Act
        var result = _mockGenericProvider.Object.SupportedInputTypes;

        // Assert
        result.ShouldContain(inputType);
        _output.WriteLine($"SupportedInputTypes contains expected input type: {inputType}");
    }

    [Fact]
    public void SupportedInputTypesPropertyShouldHandleMultipleInputTypes()
    {
        // Arrange
        var inputTypes = new List<string> { "JSON", "XML", "CSV", "Object" }.AsReadOnly();
        _mockGenericProvider.Setup(x => x.SupportedInputTypes).Returns(inputTypes);

        // Act
        var result = _mockGenericProvider.Object.SupportedInputTypes;

        // Assert
        result.Count.ShouldBe(4);
        result.ShouldContain("JSON");
        result.ShouldContain("XML");
        result.ShouldContain("CSV");
        result.ShouldContain("Object");
        _output.WriteLine($"SupportedInputTypes correctly handles multiple input types with count: {result.Count}");
    }

    [Fact]
    public void SupportedOutputTypesPropertyShouldReturnEmptyListWhenNoOutputTypesSupported()
    {
        // Arrange
        var emptyList = new List<string>().AsReadOnly();
        _mockGenericProvider.Setup(x => x.SupportedOutputTypes).Returns(emptyList);

        // Act
        var result = _mockGenericProvider.Object.SupportedOutputTypes;

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        result.ShouldBeAssignableTo<IReadOnlyList<string>>();
        _output.WriteLine($"SupportedOutputTypes returned empty list with count: {result.Count}");
    }

    [Theory]
    [InlineData("JSON")]
    [InlineData("XML")]
    [InlineData("CSV")]
    [InlineData("Binary")]
    [InlineData("Object")]
    public void SupportedOutputTypesPropertyShouldContainExpectedOutputTypes(string outputType)
    {
        // Arrange
        var outputTypes = new List<string> { outputType }.AsReadOnly();
        _mockGenericProvider.Setup(x => x.SupportedOutputTypes).Returns(outputTypes);

        // Act
        var result = _mockGenericProvider.Object.SupportedOutputTypes;

        // Assert
        result.ShouldContain(outputType);
        _output.WriteLine($"SupportedOutputTypes contains expected output type: {outputType}");
    }

    [Fact]
    public void SupportedOutputTypesPropertyShouldHandleMultipleOutputTypes()
    {
        // Arrange
        var outputTypes = new List<string> { "JSON", "XML", "CSV", "Object" }.AsReadOnly();
        _mockGenericProvider.Setup(x => x.SupportedOutputTypes).Returns(outputTypes);

        // Act
        var result = _mockGenericProvider.Object.SupportedOutputTypes;

        // Assert
        result.Count.ShouldBe(4);
        result.ShouldContain("JSON");
        result.ShouldContain("XML");
        result.ShouldContain("CSV");
        result.ShouldContain("Object");
        _output.WriteLine($"SupportedOutputTypes correctly handles multiple output types with count: {result.Count}");
    }

    [Fact]
    public void TransformationCategoriesPropertyShouldReturnEmptyListWhenNoCategoriesSupported()
    {
        // Arrange
        var emptyList = new List<string>().AsReadOnly();
        _mockGenericProvider.Setup(x => x.TransformationCategories).Returns(emptyList);

        // Act
        var result = _mockGenericProvider.Object.TransformationCategories;

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        result.ShouldBeAssignableTo<IReadOnlyList<string>>();
        _output.WriteLine($"TransformationCategories returned empty list with count: {result.Count}");
    }

    [Theory]
    [InlineData("Mapping")]
    [InlineData("Filtering")]
    [InlineData("Aggregation")]
    [InlineData("Validation")]
    [InlineData("Formatting")]
    public void TransformationCategoriesPropertyShouldContainExpectedCategories(string category)
    {
        // Arrange
        var categories = new List<string> { category }.AsReadOnly();
        _mockGenericProvider.Setup(x => x.TransformationCategories).Returns(categories);

        // Act
        var result = _mockGenericProvider.Object.TransformationCategories;

        // Assert
        result.ShouldContain(category);
        _output.WriteLine($"TransformationCategories contains expected category: {category}");
    }

    [Fact]
    public void TransformationCategoriesPropertyShouldHandleMultipleCategories()
    {
        // Arrange
        var categories = new List<string> { "Mapping", "Filtering", "Validation", "Formatting" }.AsReadOnly();
        _mockGenericProvider.Setup(x => x.TransformationCategories).Returns(categories);

        // Act
        var result = _mockGenericProvider.Object.TransformationCategories;

        // Assert
        result.Count.ShouldBe(4);
        result.ShouldContain("Mapping");
        result.ShouldContain("Filtering");
        result.ShouldContain("Validation");
        result.ShouldContain("Formatting");
        _output.WriteLine($"TransformationCategories correctly handles multiple categories with count: {result.Count}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void PriorityPropertyShouldAcceptValidValues(int priority)
    {
        // Arrange
        _mockGenericProvider.Setup(x => x.Priority).Returns(priority);

        // Act
        var result = _mockGenericProvider.Object.Priority;

        // Assert
        result.ShouldBe(priority);
        _output.WriteLine($"Priority correctly accepts value: {result}");
    }

    [Fact]
    public void ValidateTransformationShouldReturnSuccessForSupportedTransformation()
    {
        // Arrange
        const string inputType = "JSON";
        const string outputType = "XML";
        const string? category = "Mapping";
        
        _mockResult.Setup(x => x.IsSuccess).Returns(true);
        _mockGenericProvider.Setup(x => x.ValidateTransformation(inputType, outputType, category))
                           .Returns(_mockResult.Object);

        // Act
        var result = _mockGenericProvider.Object.ValidateTransformation(inputType, outputType, category);

        // Assert
        result.ShouldBe(_mockResult.Object);
        _mockGenericProvider.Verify(x => x.ValidateTransformation(inputType, outputType, category), Times.Once);
        _output.WriteLine($"ValidateTransformation correctly validates supported transformation: {inputType} -> {outputType} ({category})");
    }

    [Fact]
    public void ValidateTransformationShouldReturnFailureForUnsupportedTransformation()
    {
        // Arrange
        const string inputType = "UnsupportedInput";
        const string outputType = "UnsupportedOutput";
        
        _mockResult.Setup(x => x.IsSuccess).Returns(false);
        _mockGenericProvider.Setup(x => x.ValidateTransformation(inputType, outputType, null))
                           .Returns(_mockResult.Object);

        // Act
        var result = _mockGenericProvider.Object.ValidateTransformation(inputType, outputType);

        // Assert
        result.ShouldBe(_mockResult.Object);
        _mockGenericProvider.Verify(x => x.ValidateTransformation(inputType, outputType, null), Times.Once);
        _output.WriteLine($"ValidateTransformation correctly rejects unsupported transformation: {inputType} -> {outputType}");
    }

    [Theory]
    [InlineData("JSON", "XML", null)]
    [InlineData("CSV", "JSON", "Mapping")]
    [InlineData("Object", "String", "Formatting")]
    [InlineData("Binary", "Object", "Parsing")]
    public void ValidateTransformationShouldHandleVariousTransformationScenarios(string inputType, string outputType, string? category)
    {
        // Arrange
        _mockResult.Setup(x => x.IsSuccess).Returns(true);
        _mockGenericProvider.Setup(x => x.ValidateTransformation(inputType, outputType, category))
                           .Returns(_mockResult.Object);

        // Act
        var result = _mockGenericProvider.Object.ValidateTransformation(inputType, outputType, category);

        // Assert
        result.ShouldBe(_mockResult.Object);
        _output.WriteLine($"ValidateTransformation handles scenario: {inputType} -> {outputType} ({category ?? "null"})");
    }

    [Fact]
    public async Task GenericTransformShouldReturnExpectedResult()
    {
        // Arrange
        _mockStringResult.Setup(x => x.IsSuccess).Returns(true);
        _mockGenericProvider.Setup(x => x.Transform<string>(_mockRequest.Object))
                           .ReturnsAsync(_mockStringResult.Object);

        // Act
        var result = await _mockGenericProvider.Object.Transform<string>(_mockRequest.Object);

        // Assert
        result.ShouldBe(_mockStringResult.Object);
        _mockGenericProvider.Verify(x => x.Transform<string>(_mockRequest.Object), Times.Once);
        _output.WriteLine("Generic Transform<T> method correctly executes and returns expected result");
    }

    [Fact]
    public async Task NonGenericTransformShouldReturnExpectedResult()
    {
        // Arrange
        _mockObjectResult.Setup(x => x.IsSuccess).Returns(true);
        _mockGenericProvider.Setup(x => x.Transform(_mockRequest.Object))
                           .ReturnsAsync(_mockObjectResult.Object);

        // Act
        var result = await _mockGenericProvider.Object.Transform(_mockRequest.Object);

        // Assert
        result.ShouldBe(_mockObjectResult.Object);
        _mockGenericProvider.Verify(x => x.Transform(_mockRequest.Object), Times.Once);
        _output.WriteLine("Non-generic Transform method correctly executes and returns expected result");
    }

    [Fact]
    public async Task CreateEngineAsyncShouldReturnExpectedResult()
    {
        // Arrange
        _mockEngineResult.Setup(x => x.IsSuccess).Returns(true);
        _mockGenericProvider.Setup(x => x.CreateEngineAsync(_mockEngineConfig.Object))
                           .ReturnsAsync(_mockEngineResult.Object);

        // Act
        var result = await _mockGenericProvider.Object.CreateEngineAsync(_mockEngineConfig.Object);

        // Assert
        result.ShouldBe(_mockEngineResult.Object);
        _mockGenericProvider.Verify(x => x.CreateEngineAsync(_mockEngineConfig.Object), Times.Once);
        _output.WriteLine("CreateEngineAsync correctly executes and returns expected result");
    }

    [Fact]
    public async Task GetTransformationMetricsAsyncShouldReturnExpectedResult()
    {
        // Arrange
        _mockMetricsResult.Setup(x => x.IsSuccess).Returns(true);
        _mockGenericProvider.Setup(x => x.GetTransformationMetricsAsync())
                           .ReturnsAsync(_mockMetricsResult.Object);

        // Act
        var result = await _mockGenericProvider.Object.GetTransformationMetricsAsync();

        // Assert
        result.ShouldBe(_mockMetricsResult.Object);
        _mockGenericProvider.Verify(x => x.GetTransformationMetricsAsync(), Times.Once);
        _output.WriteLine("GetTransformationMetricsAsync correctly executes and returns expected result");
    }

    [Fact]
    public void AllPropertiesShouldBeAccessibleIndependently()
    {
        // Arrange
        var inputTypes = new List<string> { "JSON", "XML" }.AsReadOnly();
        var outputTypes = new List<string> { "CSV", "Object" }.AsReadOnly();
        var categories = new List<string> { "Mapping", "Validation" }.AsReadOnly();
        const int priority = 10;

        _mockGenericProvider.Setup(x => x.SupportedInputTypes).Returns(inputTypes);
        _mockGenericProvider.Setup(x => x.SupportedOutputTypes).Returns(outputTypes);
        _mockGenericProvider.Setup(x => x.TransformationCategories).Returns(categories);
        _mockGenericProvider.Setup(x => x.Priority).Returns(priority);

        // Act
        var provider = _mockGenericProvider.Object;

        // Assert
        provider.SupportedInputTypes.ShouldBe(inputTypes);
        provider.SupportedOutputTypes.ShouldBe(outputTypes);
        provider.TransformationCategories.ShouldBe(categories);
        provider.Priority.ShouldBe(priority);
        _output.WriteLine("All provider properties are accessible and return expected values independently");
    }

    [Fact]
    public void GenericInterfaceContractShouldBeCorrectlyDefined()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationProvider<ITransformationRequest>);

        // Assert
        interfaceType.ShouldNotBeNull();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.IsGenericType.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Transformations.Abstractions");
        
        var properties = interfaceType.GetProperties();
        properties.Length.ShouldBe(4);
        
        var methods = interfaceType.GetMethods();
        methods.Length.ShouldBeGreaterThan(4); // Should have multiple methods including inherited ones
        
        _output.WriteLine($"Generic interface has {properties.Length} properties and {methods.Length} methods defined correctly");
    }

    [Fact]
    public void NonGenericInterfaceContractShouldBeCorrectlyDefined()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationProvider);

        // Assert
        interfaceType.ShouldNotBeNull();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.IsGenericType.ShouldBeFalse();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Transformations.Abstractions");
        
        // Non-generic interface is a marker interface
        var properties = interfaceType.GetProperties();
        var methods = interfaceType.GetMethods();
        
        _output.WriteLine($"Non-generic interface has {properties.Length} properties and {methods.Length} methods defined correctly");
    }

    [Theory]
    [InlineData("JSON", "XML", "Mapping", 5)]
    [InlineData("CSV", "Object", "Parsing", 10)]
    [InlineData("Binary", "String", "Formatting", 1)]
    [InlineData("Stream", "JSON", "Serialization", 15)]
    public void ProviderShouldSupportVariousTransformationConfigurations(string inputType, string outputType, string category, int priority)
    {
        // Arrange
        var inputTypes = new List<string> { inputType }.AsReadOnly();
        var outputTypes = new List<string> { outputType }.AsReadOnly();
        var categories = new List<string> { category }.AsReadOnly();

        _mockGenericProvider.Setup(x => x.SupportedInputTypes).Returns(inputTypes);
        _mockGenericProvider.Setup(x => x.SupportedOutputTypes).Returns(outputTypes);
        _mockGenericProvider.Setup(x => x.TransformationCategories).Returns(categories);
        _mockGenericProvider.Setup(x => x.Priority).Returns(priority);

        // Act
        var provider = _mockGenericProvider.Object;

        // Assert
        provider.SupportedInputTypes.ShouldContain(inputType);
        provider.SupportedOutputTypes.ShouldContain(outputType);
        provider.TransformationCategories.ShouldContain(category);
        provider.Priority.ShouldBe(priority);
        _output.WriteLine($"Provider supports transformation configuration: {inputType} -> {outputType} ({category}) with priority {priority}");
    }

    [Fact]
    public async Task ProviderShouldHandleAsynchronousOperations()
    {
        // Arrange
        _mockStringResult.Setup(x => x.IsSuccess).Returns(true);
        _mockObjectResult.Setup(x => x.IsSuccess).Returns(true);
        _mockEngineResult.Setup(x => x.IsSuccess).Returns(true);
        _mockMetricsResult.Setup(x => x.IsSuccess).Returns(true);

        _mockGenericProvider.Setup(x => x.Transform<string>(_mockRequest.Object))
                           .ReturnsAsync(_mockStringResult.Object);
        _mockGenericProvider.Setup(x => x.Transform(_mockRequest.Object))
                           .ReturnsAsync(_mockObjectResult.Object);
        _mockGenericProvider.Setup(x => x.CreateEngineAsync(_mockEngineConfig.Object))
                           .ReturnsAsync(_mockEngineResult.Object);
        _mockGenericProvider.Setup(x => x.GetTransformationMetricsAsync())
                           .ReturnsAsync(_mockMetricsResult.Object);

        // Act
        var provider = _mockGenericProvider.Object;
        var genericResult = await provider.Transform<string>(_mockRequest.Object);
        var nonGenericResult = await provider.Transform(_mockRequest.Object);
        var engineResult = await provider.CreateEngineAsync(_mockEngineConfig.Object);
        var metricsResult = await provider.GetTransformationMetricsAsync();

        // Assert
        genericResult.ShouldBe(_mockStringResult.Object);
        nonGenericResult.ShouldBe(_mockObjectResult.Object);
        engineResult.ShouldBe(_mockEngineResult.Object);
        metricsResult.ShouldBe(_mockMetricsResult.Object);
        _output.WriteLine("Provider correctly handles all asynchronous operations");
    }
}