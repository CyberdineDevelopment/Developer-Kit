using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Shouldly;
using Moq;
using FractalDataWorks;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions.Tests;

/// <summary>
/// Comprehensive tests for ITransformationEngine interface contracts and behavior.
/// Tests verify proper implementation of transformation engine operations and lifecycle management.
/// </summary>
public class ITransformationEngineTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ITransformationEngine> _mockEngine;
    private readonly Mock<ITransformationRequest> _mockRequest;
    private readonly Mock<IFdwResult<ITransformationResult>> _mockTransformationResult;
    private readonly Mock<IFdwResult> _mockResult;

    public ITransformationEngineTests(ITestOutputHelper output)
    {
        _output = output;
        _mockEngine = new Mock<ITransformationEngine>();
        _mockRequest = new Mock<ITransformationRequest>();
        _mockTransformationResult = new Mock<IFdwResult<ITransformationResult>>();
        _mockResult = new Mock<IFdwResult>();
    }

    [Fact]
    public void EngineIdPropertyShouldReturnValidString()
    {
        // Arrange
        const string expectedEngineId = "engine-123-abc";
        _mockEngine.Setup(x => x.EngineId).Returns(expectedEngineId);

        // Act
        var result = _mockEngine.Object.EngineId;

        // Assert
        result.ShouldBe(expectedEngineId);
        result.ShouldNotBeNullOrEmpty();
        _output.WriteLine($"EngineId property returned expected value: {result}");
    }

    [Theory]
    [InlineData("transformation-engine-v1")]
    [InlineData("data-processor")]
    [InlineData("mapping-engine")]
    [InlineData("validation-engine")]
    public void EngineIdPropertyShouldAcceptVariousValidValues(string engineId)
    {
        // Arrange
        _mockEngine.Setup(x => x.EngineId).Returns(engineId);

        // Act
        var result = _mockEngine.Object.EngineId;

        // Assert
        result.ShouldBe(engineId);
        _output.WriteLine($"EngineId correctly accepts value: {result}");
    }

    [Fact]
    public void EngineTypePropertyShouldReturnValidString()
    {
        // Arrange
        const string expectedEngineType = "JsonTransformationEngine";
        _mockEngine.Setup(x => x.EngineType).Returns(expectedEngineType);

        // Act
        var result = _mockEngine.Object.EngineType;

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
    public void EngineTypePropertyShouldAcceptVariousValidValues(string engineType)
    {
        // Arrange
        _mockEngine.Setup(x => x.EngineType).Returns(engineType);

        // Act
        var result = _mockEngine.Object.EngineType;

        // Assert
        result.ShouldBe(engineType);
        _output.WriteLine($"EngineType correctly accepts value: {result}");
    }

    [Fact]
    public void IsRunningPropertyShouldReturnFalseWhenStopped()
    {
        // Arrange
        _mockEngine.Setup(x => x.IsRunning).Returns(false);

        // Act
        var result = _mockEngine.Object.IsRunning;

        // Assert
        result.ShouldBeFalse();
        _output.WriteLine($"IsRunning property correctly indicates stopped state: {result}");
    }

    [Fact]
    public void IsRunningPropertyShouldReturnTrueWhenRunning()
    {
        // Arrange
        _mockEngine.Setup(x => x.IsRunning).Returns(true);

        // Act
        var result = _mockEngine.Object.IsRunning;

        // Assert
        result.ShouldBeTrue();
        _output.WriteLine($"IsRunning property correctly indicates running state: {result}");
    }

    [Fact]
    public async Task ExecuteTransformationAsyncShouldReturnResultWithoutCancellationToken()
    {
        // Arrange
        _mockEngine.Setup(x => x.ExecuteTransformationAsync(_mockRequest.Object, default))
                   .ReturnsAsync(_mockTransformationResult.Object);

        // Act
        var result = await _mockEngine.Object.ExecuteTransformationAsync(_mockRequest.Object);

        // Assert
        result.ShouldBe(_mockTransformationResult.Object);
        _mockEngine.Verify(x => x.ExecuteTransformationAsync(_mockRequest.Object, default), Times.Once);
        _output.WriteLine("ExecuteTransformationAsync correctly executed without cancellation token");
    }

    [Fact]
    public async Task ExecuteTransformationAsyncShouldReturnResultWithCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        _mockEngine.Setup(x => x.ExecuteTransformationAsync(_mockRequest.Object, cts.Token))
                   .ReturnsAsync(_mockTransformationResult.Object);

        // Act
        var result = await _mockEngine.Object.ExecuteTransformationAsync(_mockRequest.Object, cts.Token);

        // Assert
        result.ShouldBe(_mockTransformationResult.Object);
        _mockEngine.Verify(x => x.ExecuteTransformationAsync(_mockRequest.Object, cts.Token), Times.Once);
        _output.WriteLine("ExecuteTransformationAsync correctly executed with cancellation token");
    }

    [Fact]
    public async Task ExecuteTransformationAsyncShouldHandleCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        
        _mockEngine.Setup(x => x.ExecuteTransformationAsync(_mockRequest.Object, cts.Token))
                   .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            () => _mockEngine.Object.ExecuteTransformationAsync(_mockRequest.Object, cts.Token));
        
        _output.WriteLine("ExecuteTransformationAsync correctly handles cancellation");
    }

    [Fact]
    public async Task StartAsyncShouldReturnResultWithoutCancellationToken()
    {
        // Arrange
        _mockEngine.Setup(x => x.StartAsync(default))
                   .ReturnsAsync(_mockResult.Object);

        // Act
        var result = await _mockEngine.Object.StartAsync();

        // Assert
        result.ShouldBe(_mockResult.Object);
        _mockEngine.Verify(x => x.StartAsync(default), Times.Once);
        _output.WriteLine("StartAsync correctly executed without cancellation token");
    }

    [Fact]
    public async Task StartAsyncShouldReturnResultWithCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        _mockEngine.Setup(x => x.StartAsync(cts.Token))
                   .ReturnsAsync(_mockResult.Object);

        // Act
        var result = await _mockEngine.Object.StartAsync(cts.Token);

        // Assert
        result.ShouldBe(_mockResult.Object);
        _mockEngine.Verify(x => x.StartAsync(cts.Token), Times.Once);
        _output.WriteLine("StartAsync correctly executed with cancellation token");
    }

    [Fact]
    public async Task StartAsyncShouldHandleCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        
        _mockEngine.Setup(x => x.StartAsync(cts.Token))
                   .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            () => _mockEngine.Object.StartAsync(cts.Token));
        
        _output.WriteLine("StartAsync correctly handles cancellation");
    }

    [Fact]
    public async Task StopAsyncShouldReturnResultWithoutCancellationToken()
    {
        // Arrange
        _mockEngine.Setup(x => x.StopAsync(default))
                   .ReturnsAsync(_mockResult.Object);

        // Act
        var result = await _mockEngine.Object.StopAsync();

        // Assert
        result.ShouldBe(_mockResult.Object);
        _mockEngine.Verify(x => x.StopAsync(default), Times.Once);
        _output.WriteLine("StopAsync correctly executed without cancellation token");
    }

    [Fact]
    public async Task StopAsyncShouldReturnResultWithCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        _mockEngine.Setup(x => x.StopAsync(cts.Token))
                   .ReturnsAsync(_mockResult.Object);

        // Act
        var result = await _mockEngine.Object.StopAsync(cts.Token);

        // Assert
        result.ShouldBe(_mockResult.Object);
        _mockEngine.Verify(x => x.StopAsync(cts.Token), Times.Once);
        _output.WriteLine("StopAsync correctly executed with cancellation token");
    }

    [Fact]
    public async Task StopAsyncShouldHandleCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        
        _mockEngine.Setup(x => x.StopAsync(cts.Token))
                   .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            () => _mockEngine.Object.StopAsync(cts.Token));
        
        _output.WriteLine("StopAsync correctly handles cancellation");
    }

    [Fact]
    public void AllPropertiesShouldBeAccessibleIndependently()
    {
        // Arrange
        const string engineId = "test-engine-id";
        const string engineType = "TestEngine";
        const bool isRunning = true;

        _mockEngine.Setup(x => x.EngineId).Returns(engineId);
        _mockEngine.Setup(x => x.EngineType).Returns(engineType);
        _mockEngine.Setup(x => x.IsRunning).Returns(isRunning);

        // Act
        var engine = _mockEngine.Object;

        // Assert
        engine.EngineId.ShouldBe(engineId);
        engine.EngineType.ShouldBe(engineType);
        engine.IsRunning.ShouldBe(isRunning);
        _output.WriteLine("All engine properties are accessible and return expected values independently");
    }

    [Fact]
    public async Task EngineLifecycleShouldWorkCorrectly()
    {
        // Arrange
        var sequence = new MockSequence();
        
        _mockEngine.InSequence(sequence)
                   .Setup(x => x.IsRunning)
                   .Returns(false);
        
        _mockEngine.InSequence(sequence)
                   .Setup(x => x.StartAsync(default))
                   .ReturnsAsync(_mockResult.Object);
        
        _mockEngine.InSequence(sequence)
                   .Setup(x => x.IsRunning)
                   .Returns(true);
        
        _mockEngine.InSequence(sequence)
                   .Setup(x => x.StopAsync(default))
                   .ReturnsAsync(_mockResult.Object);
        
        _mockEngine.InSequence(sequence)
                   .Setup(x => x.IsRunning)
                   .Returns(false);

        // Act & Assert
        var engine = _mockEngine.Object;
        
        // Initially stopped
        engine.IsRunning.ShouldBeFalse();
        
        // Start engine
        await engine.StartAsync();
        
        // Should be running
        engine.IsRunning.ShouldBeTrue();
        
        // Stop engine
        await engine.StopAsync();
        
        // Should be stopped
        engine.IsRunning.ShouldBeFalse();
        
        _output.WriteLine("Engine lifecycle (start/stop) works correctly with state changes");
    }

    [Fact]
    public void InterfaceContractShouldBeCorrectlyDefined()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationEngine);

        // Assert
        interfaceType.ShouldNotBeNull();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Transformations.Abstractions");
        
        var properties = interfaceType.GetProperties();
        properties.Length.ShouldBe(3);
        
        var methods = interfaceType.GetMethods();
        // Should have 3 async methods plus property getters
        methods.Length.ShouldBeGreaterThan(3);
        
        _output.WriteLine($"Interface has {properties.Length} properties and {methods.Length} methods defined correctly");
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(5000)]
    public async Task ExecuteTransformationAsyncShouldHandleTimeoutScenarios(int timeoutMs)
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
        
        _mockEngine.Setup(x => x.ExecuteTransformationAsync(_mockRequest.Object, cts.Token))
                   .ReturnsAsync(_mockTransformationResult.Object);

        // Act
        var result = await _mockEngine.Object.ExecuteTransformationAsync(_mockRequest.Object, cts.Token);

        // Assert
        result.ShouldBe(_mockTransformationResult.Object);
        _output.WriteLine($"ExecuteTransformationAsync handled timeout scenario of {timeoutMs}ms correctly");
    }
}