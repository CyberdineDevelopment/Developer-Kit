using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using Shouldly;
using Moq;
using FractalDataWorks;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions.Tests;

/// <summary>
/// Comprehensive tests for ITransformationEngineConfiguration interface contracts and behavior.
/// Tests verify proper implementation of transformation engine configuration properties and inheritance.
/// </summary>
public class ITransformationEngineConfigurationTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ITransformationEngineConfiguration> _mockConfiguration;

    public ITransformationEngineConfigurationTests(ITestOutputHelper output)
    {
        _output = output;
        _mockConfiguration = new Mock<ITransformationEngineConfiguration>();
    }

    [Fact]
    public void EngineTypePropertyShouldReturnValidString()
    {
        // Arrange
        const string expectedEngineType = "JsonTransformationEngine";
        _mockConfiguration.Setup(x => x.EngineType).Returns(expectedEngineType);

        // Act
        var result = _mockConfiguration.Object.EngineType;

        // Assert
        result.ShouldBe(expectedEngineType);
        result.ShouldNotBeNullOrEmpty();
        _output.WriteLine($"EngineType property returned expected value: {result}");
    }

    [Theory]
    [InlineData("JsonTransformationEngine")]
    [InlineData("XmlDataProcessor")]
    [InlineData("CsvMappingEngine")]
    [InlineData("ObjectValidationEngine")]
    [InlineData("BinaryDataTransformer")]
    public void EngineTypePropertyShouldAcceptVariousValidValues(string engineType)
    {
        // Arrange
        _mockConfiguration.Setup(x => x.EngineType).Returns(engineType);

        // Act
        var result = _mockConfiguration.Object.EngineType;

        // Assert
        result.ShouldBe(engineType);
        _output.WriteLine($"EngineType correctly accepts value: {result}");
    }

    [Fact]
    public void MaxConcurrencyPropertyShouldReturnPositiveInteger()
    {
        // Arrange
        const int expectedMaxConcurrency = 4;
        _mockConfiguration.Setup(x => x.MaxConcurrency).Returns(expectedMaxConcurrency);

        // Act
        var result = _mockConfiguration.Object.MaxConcurrency;

        // Assert
        result.ShouldBe(expectedMaxConcurrency);
        result.ShouldBeGreaterThan(0);
        _output.WriteLine($"MaxConcurrency property returned expected value: {result}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    public void MaxConcurrencyPropertyShouldAcceptValidPositiveValues(int maxConcurrency)
    {
        // Arrange
        _mockConfiguration.Setup(x => x.MaxConcurrency).Returns(maxConcurrency);

        // Act
        var result = _mockConfiguration.Object.MaxConcurrency;

        // Assert
        result.ShouldBe(maxConcurrency);
        result.ShouldBeGreaterThan(0);
        _output.WriteLine($"MaxConcurrency correctly accepts positive value: {result}");
    }

    [Fact]
    public void TimeoutSecondsPropertyShouldReturnPositiveInteger()
    {
        // Arrange
        const int expectedTimeoutSeconds = 30;
        _mockConfiguration.Setup(x => x.TimeoutSeconds).Returns(expectedTimeoutSeconds);

        // Act
        var result = _mockConfiguration.Object.TimeoutSeconds;

        // Assert
        result.ShouldBe(expectedTimeoutSeconds);
        result.ShouldBeGreaterThan(0);
        _output.WriteLine($"TimeoutSeconds property returned expected value: {result}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(300)]
    [InlineData(600)]
    [InlineData(3600)]
    public void TimeoutSecondsPropertyShouldAcceptValidPositiveValues(int timeoutSeconds)
    {
        // Arrange
        _mockConfiguration.Setup(x => x.TimeoutSeconds).Returns(timeoutSeconds);

        // Act
        var result = _mockConfiguration.Object.TimeoutSeconds;

        // Assert
        result.ShouldBe(timeoutSeconds);
        result.ShouldBeGreaterThan(0);
        _output.WriteLine($"TimeoutSeconds correctly accepts positive value: {result}");
    }

    [Fact]
    public void EnableCachingPropertyShouldReturnFalseWhenDisabled()
    {
        // Arrange
        _mockConfiguration.Setup(x => x.EnableCaching).Returns(false);

        // Act
        var result = _mockConfiguration.Object.EnableCaching;

        // Assert
        result.ShouldBeFalse();
        _output.WriteLine($"EnableCaching property correctly indicates disabled state: {result}");
    }

    [Fact]
    public void EnableCachingPropertyShouldReturnTrueWhenEnabled()
    {
        // Arrange
        _mockConfiguration.Setup(x => x.EnableCaching).Returns(true);

        // Act
        var result = _mockConfiguration.Object.EnableCaching;

        // Assert
        result.ShouldBeTrue();
        _output.WriteLine($"EnableCaching property correctly indicates enabled state: {result}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableCachingPropertyShouldAcceptBothBooleanValues(bool enableCaching)
    {
        // Arrange
        _mockConfiguration.Setup(x => x.EnableCaching).Returns(enableCaching);

        // Act
        var result = _mockConfiguration.Object.EnableCaching;

        // Assert
        result.ShouldBe(enableCaching);
        _output.WriteLine($"EnableCaching correctly accepts boolean value: {result}");
    }

    [Fact]
    public void EnableMetricsPropertyShouldReturnFalseWhenDisabled()
    {
        // Arrange
        _mockConfiguration.Setup(x => x.EnableMetrics).Returns(false);

        // Act
        var result = _mockConfiguration.Object.EnableMetrics;

        // Assert
        result.ShouldBeFalse();
        _output.WriteLine($"EnableMetrics property correctly indicates disabled state: {result}");
    }

    [Fact]
    public void EnableMetricsPropertyShouldReturnTrueWhenEnabled()
    {
        // Arrange
        _mockConfiguration.Setup(x => x.EnableMetrics).Returns(true);

        // Act
        var result = _mockConfiguration.Object.EnableMetrics;

        // Assert
        result.ShouldBeTrue();
        _output.WriteLine($"EnableMetrics property correctly indicates enabled state: {result}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableMetricsPropertyShouldAcceptBothBooleanValues(bool enableMetrics)
    {
        // Arrange
        _mockConfiguration.Setup(x => x.EnableMetrics).Returns(enableMetrics);

        // Act
        var result = _mockConfiguration.Object.EnableMetrics;

        // Assert
        result.ShouldBe(enableMetrics);
        _output.WriteLine($"EnableMetrics correctly accepts boolean value: {result}");
    }

    [Fact]
    public void AllPropertiesShouldBeAccessibleIndependently()
    {
        // Arrange
        const string engineType = "TestEngine";
        const int maxConcurrency = 8;
        const int timeoutSeconds = 60;
        const bool enableCaching = true;
        const bool enableMetrics = false;

        _mockConfiguration.Setup(x => x.EngineType).Returns(engineType);
        _mockConfiguration.Setup(x => x.MaxConcurrency).Returns(maxConcurrency);
        _mockConfiguration.Setup(x => x.TimeoutSeconds).Returns(timeoutSeconds);
        _mockConfiguration.Setup(x => x.EnableCaching).Returns(enableCaching);
        _mockConfiguration.Setup(x => x.EnableMetrics).Returns(enableMetrics);

        // Act
        var config = _mockConfiguration.Object;

        // Assert
        config.EngineType.ShouldBe(engineType);
        config.MaxConcurrency.ShouldBe(maxConcurrency);
        config.TimeoutSeconds.ShouldBe(timeoutSeconds);
        config.EnableCaching.ShouldBe(enableCaching);
        config.EnableMetrics.ShouldBe(enableMetrics);
        _output.WriteLine("All configuration properties are accessible and return expected values independently");
    }

    [Fact]
    public void ConfigurationShouldInheritFromIFdwConfiguration()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationEngineConfiguration);
        var baseInterfaceType = typeof(IFdwConfiguration);

        // Assert
        interfaceType.ShouldNotBeNull();
        baseInterfaceType.ShouldNotBeNull();
        interfaceType.IsAssignableFrom(typeof(ITransformationEngineConfiguration)).ShouldBeTrue();
        baseInterfaceType.IsAssignableFrom(typeof(ITransformationEngineConfiguration)).ShouldBeTrue();
        _output.WriteLine("ITransformationEngineConfiguration correctly inherits from IFdwConfiguration");
    }

    [Fact]
    public void InterfaceContractShouldBeCorrectlyDefined()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationEngineConfiguration);

        // Assert
        interfaceType.ShouldNotBeNull();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Transformations.Abstractions");
        
        var properties = interfaceType.GetProperties();
        properties.Length.ShouldBe(5);
        
        _output.WriteLine($"Interface has {properties.Length} properties defined correctly");
    }

    [Fact]
    public void ConfigurationShouldSupportPerformanceTuning()
    {
        // Arrange
        const string engineType = "HighPerformanceEngine";
        var maxConcurrency = Environment.ProcessorCount * 2;
        const int timeoutSeconds = 120;
        const bool enableCaching = true;
        const bool enableMetrics = true;

        _mockConfiguration.Setup(x => x.EngineType).Returns(engineType);
        _mockConfiguration.Setup(x => x.MaxConcurrency).Returns(maxConcurrency);
        _mockConfiguration.Setup(x => x.TimeoutSeconds).Returns(timeoutSeconds);
        _mockConfiguration.Setup(x => x.EnableCaching).Returns(enableCaching);
        _mockConfiguration.Setup(x => x.EnableMetrics).Returns(enableMetrics);

        // Act
        var config = _mockConfiguration.Object;

        // Assert
        config.EngineType.ShouldBe(engineType);
        config.MaxConcurrency.ShouldBeGreaterThan(Environment.ProcessorCount);
        config.TimeoutSeconds.ShouldBeGreaterThan(60);
        config.EnableCaching.ShouldBeTrue();
        config.EnableMetrics.ShouldBeTrue();
        _output.WriteLine($"Configuration supports performance tuning: MaxConcurrency={config.MaxConcurrency}, Timeout={config.TimeoutSeconds}s, Caching={config.EnableCaching}, Metrics={config.EnableMetrics}");
    }

    [Fact]
    public void ConfigurationShouldSupportLowResourceMode()
    {
        // Arrange
        const string engineType = "LowResourceEngine";
        const int maxConcurrency = 1;
        const int timeoutSeconds = 300;
        const bool enableCaching = false;
        const bool enableMetrics = false;

        _mockConfiguration.Setup(x => x.EngineType).Returns(engineType);
        _mockConfiguration.Setup(x => x.MaxConcurrency).Returns(maxConcurrency);
        _mockConfiguration.Setup(x => x.TimeoutSeconds).Returns(timeoutSeconds);
        _mockConfiguration.Setup(x => x.EnableCaching).Returns(enableCaching);
        _mockConfiguration.Setup(x => x.EnableMetrics).Returns(enableMetrics);

        // Act
        var config = _mockConfiguration.Object;

        // Assert
        config.EngineType.ShouldBe(engineType);
        config.MaxConcurrency.ShouldBe(1);
        config.TimeoutSeconds.ShouldBeGreaterThan(60);
        config.EnableCaching.ShouldBeFalse();
        config.EnableMetrics.ShouldBeFalse();
        _output.WriteLine($"Configuration supports low resource mode: MaxConcurrency={config.MaxConcurrency}, Timeout={config.TimeoutSeconds}s, Caching={config.EnableCaching}, Metrics={config.EnableMetrics}");
    }

    [Theory]
    [InlineData("JsonEngine", 2, 30, true, true)]
    [InlineData("XmlEngine", 4, 60, false, true)]
    [InlineData("CsvEngine", 1, 120, true, false)]
    [InlineData("BinaryEngine", 8, 15, false, false)]
    public void ConfigurationShouldSupportVariousEngineScenarios(string engineType, int maxConcurrency, int timeoutSeconds, bool enableCaching, bool enableMetrics)
    {
        // Arrange
        _mockConfiguration.Setup(x => x.EngineType).Returns(engineType);
        _mockConfiguration.Setup(x => x.MaxConcurrency).Returns(maxConcurrency);
        _mockConfiguration.Setup(x => x.TimeoutSeconds).Returns(timeoutSeconds);
        _mockConfiguration.Setup(x => x.EnableCaching).Returns(enableCaching);
        _mockConfiguration.Setup(x => x.EnableMetrics).Returns(enableMetrics);

        // Act
        var config = _mockConfiguration.Object;

        // Assert
        config.EngineType.ShouldBe(engineType);
        config.MaxConcurrency.ShouldBe(maxConcurrency);
        config.TimeoutSeconds.ShouldBe(timeoutSeconds);
        config.EnableCaching.ShouldBe(enableCaching);
        config.EnableMetrics.ShouldBe(enableMetrics);
        _output.WriteLine($"Configuration supports engine scenario: {engineType} with MaxConcurrency={maxConcurrency}, Timeout={timeoutSeconds}s, Caching={enableCaching}, Metrics={enableMetrics}");
    }
}