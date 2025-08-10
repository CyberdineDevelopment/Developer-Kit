using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Connections.Stream.Tests;

/// <summary>
/// Tests for StreamConnection class.
/// </summary>
public class StreamConnectionTests : IDisposable
{
    private readonly Mock<ILogger<StreamConnection>> _mockLogger;
    private readonly Mock<IConfigurationRegistry<StreamConnectionConfiguration>> _mockConfigRegistry;
    private readonly StreamConnectionConfiguration _fileConfig;
    private readonly StreamConnectionConfiguration _memoryConfig;
    private readonly string _testFilePath;

    public StreamConnectionTests()
    {
        _mockLogger = new Mock<ILogger<StreamConnection>>();
        _mockConfigRegistry = new Mock<IConfigurationRegistry<StreamConnectionConfiguration>>();
        _testFilePath = Path.GetTempFileName();
        
        _fileConfig = new StreamConnectionConfiguration
        {
            IsEnabled = true,
            StreamType = StreamType.File,
            Path = _testFilePath,
            FileMode = FileMode.OpenOrCreate,
            FileAccess = FileAccess.ReadWrite
        };
        
        _memoryConfig = new StreamConnectionConfiguration
        {
            IsEnabled = true,
            StreamType = StreamType.Memory,
            InitialCapacity = 1024
        };
    }

    [Fact]
    public void ConstructorCreatesInstanceWithValidConfiguration()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _fileConfig });

        // Act
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);

        // Assert
        connection.ShouldNotBeNull();
        // Configuration is now accessible via the base class
    }

    [Fact]
    public async Task ConnectAsyncSucceedsWithFileStream()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _fileConfig });
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);

        // Act
        var result = await connection.ConnectAsync(_testFilePath, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ConnectAsyncSucceedsWithMemoryStream()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _memoryConfig });
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);

        // Act
        var result = await connection.ConnectAsync("memory", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ConnectAsyncReturnsSucessWhenAlreadyConnected()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _memoryConfig });
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);
        await connection.ConnectAsync("memory", TestContext.Current.CancellationToken);

        // Act
        var result = await connection.ConnectAsync("memory", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task DisconnectAsyncSucceeds()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _memoryConfig });
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);
        await connection.ConnectAsync("memory", TestContext.Current.CancellationToken);

        // Act
        var result = await connection.DisconnectAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteFailsWhenNotConnected()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _memoryConfig });
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);
        var command = new StreamCommand { Operation = StreamOperation.GetInfo };

        // Act
        var result = await connection.Execute<StreamInfo>(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteWriteSucceeds()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _memoryConfig });
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);
        await connection.ConnectAsync("stream", TestContext.Current.CancellationToken);
        
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var command = new StreamCommand 
        { 
            Operation = StreamOperation.Write,
            Data = data
        };

        // Act
        var result = await connection.Execute<int>(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(data.Length);
    }

    [Fact]
    public async Task ExecuteReadSucceeds()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _memoryConfig });
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);
        await connection.ConnectAsync("stream", TestContext.Current.CancellationToken);
        
        // Write some data first
        var data = new byte[] { 1, 2, 3, 4, 5 };
        await connection.Execute<int>(new StreamCommand 
        { 
            Operation = StreamOperation.Write,
            Data = data
        }, TestContext.Current.CancellationToken);
        
        // Seek to beginning
        await connection.Execute<long>(new StreamCommand
        {
            Operation = StreamOperation.Seek,
            Position = 0,
            SeekOrigin = SeekOrigin.Begin
        }, TestContext.Current.CancellationToken);

        // Act
        var result = await connection.Execute<byte[]>(new StreamCommand 
        { 
            Operation = StreamOperation.Read,
            BufferSize = 10
        }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(data);
    }

    [Fact]
    public async Task ExecuteGetInfoSucceeds()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _memoryConfig });
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);
        await connection.ConnectAsync("stream", TestContext.Current.CancellationToken);

        // Act
        var result = await connection.Execute<StreamInfo>(new StreamCommand 
        { 
            Operation = StreamOperation.GetInfo
        }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CanRead.ShouldBeTrue();
        result.Value.CanWrite.ShouldBeTrue();
        result.Value.CanSeek.ShouldBeTrue();
        result.Value.StreamType.ShouldBe(StreamType.Memory);
    }

    [Fact]
    public async Task ExecuteSeekSucceeds()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _memoryConfig });
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);
        await connection.ConnectAsync("stream", TestContext.Current.CancellationToken);
        
        // Write some data to have a position to seek to
        await connection.Execute<int>(new StreamCommand 
        { 
            Operation = StreamOperation.Write,
            Data = new byte[100]
        }, TestContext.Current.CancellationToken);

        // Act
        var result = await connection.Execute<long>(new StreamCommand 
        { 
            Operation = StreamOperation.Seek,
            Position = 50,
            SeekOrigin = SeekOrigin.Begin
        }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(50);
    }

    [Fact]
    public async Task DisposeClosesStream()
    {
        // Arrange
        _mockConfigRegistry.Setup(c => c.GetAll()).Returns(new[] { _memoryConfig });
        var connection = new StreamConnection(_mockLogger.Object, _fileConfig);
        await connection.ConnectAsync("stream", TestContext.Current.CancellationToken);

        // Act
        connection.Dispose();

        // Assert
        // Try to execute a command after dispose - should fail
        var result = await connection.Execute<StreamInfo>(new StreamCommand 
        { 
            Operation = StreamOperation.GetInfo
        }, TestContext.Current.CancellationToken);
        
        result.IsSuccess.ShouldBeFalse();
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
        catch
        {
            // Best effort cleanup
        }
        
        GC.SuppressFinalize(this);
    }
}