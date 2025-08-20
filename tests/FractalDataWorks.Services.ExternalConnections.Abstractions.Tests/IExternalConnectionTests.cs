using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Moq;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests;

/// <summary>
/// Tests for the IExternalConnection interface.
/// </summary>
public sealed class IExternalConnectionTests
{
    [Fact]
    public void ShouldInheritFromIDisposable()
    {
        // Arrange & Act
        var isAssignable = typeof(IDisposable).IsAssignableFrom(typeof(IExternalConnection));

        // Assert
        isAssignable.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveConnectionIdProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnection).GetProperty(nameof(IExternalConnection.ConnectionId));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveProviderNameProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnection).GetProperty(nameof(IExternalConnection.ProviderName));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveStateProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnection).GetProperty(nameof(IExternalConnection.State));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(FdwConnectionState));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveConnectionStringProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnection).GetProperty(nameof(IExternalConnection.ConnectionString));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveOpenAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IExternalConnection).GetMethod(nameof(IExternalConnection.OpenAsync));

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IFdwResult>));
        methodInfo.GetParameters().Length.ShouldBe(0);
    }

    [Fact]
    public void ShouldHaveCloseAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IExternalConnection).GetMethod(nameof(IExternalConnection.CloseAsync));

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IFdwResult>));
        methodInfo.GetParameters().Length.ShouldBe(0);
    }

    [Fact]
    public void ShouldHaveTestConnectionAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IExternalConnection).GetMethod(nameof(IExternalConnection.TestConnectionAsync));

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IFdwResult>));
        methodInfo.GetParameters().Length.ShouldBe(0);
    }

    [Fact]
    public void ShouldHaveGetMetadataAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IExternalConnection).GetMethod(nameof(IExternalConnection.GetMetadataAsync));

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IFdwResult<IConnectionMetadata>>));
        methodInfo.GetParameters().Length.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldAllowMockImplementation()
    {
        // Arrange
        var connectionId = Guid.NewGuid().ToString();
        var mockResult = new Mock<IFdwResult>();
        mockResult.Setup(x => x.Success).Returns(true);
        
        var mockMetadata = new Mock<IConnectionMetadata>();
        var mockMetadataResult = new Mock<IFdwResult<IConnectionMetadata>>();
        mockMetadataResult.Setup(x => x.Success).Returns(true);
        mockMetadataResult.Setup(x => x.Value).Returns(mockMetadata.Object);

        var mock = new Mock<IExternalConnection>();
        mock.Setup(x => x.ConnectionId).Returns(connectionId);
        mock.Setup(x => x.ProviderName).Returns("TestProvider");
        mock.Setup(x => x.State).Returns(FdwConnectionState.Created);
        mock.Setup(x => x.ConnectionString).Returns("Server=localhost;Database=Test");
        mock.Setup(x => x.OpenAsync()).Returns(Task.FromResult(mockResult.Object));
        mock.Setup(x => x.CloseAsync()).Returns(Task.FromResult(mockResult.Object));
        mock.Setup(x => x.TestConnectionAsync()).Returns(Task.FromResult(mockResult.Object));
        mock.Setup(x => x.GetMetadataAsync()).Returns(Task.FromResult(mockMetadataResult.Object));

        // Act
        var connection = mock.Object;
        var openResult = await connection.OpenAsync();
        var closeResult = await connection.CloseAsync();
        var testResult = await connection.TestConnectionAsync();
        var metadataResult = await connection.GetMetadataAsync();

        // Assert
        connection.ConnectionId.ShouldBe(connectionId);
        connection.ProviderName.ShouldBe("TestProvider");
        connection.State.ShouldBe(FdwConnectionState.Created);
        connection.ConnectionString.ShouldBe("Server=localhost;Database=Test");
        
        openResult.ShouldBe(mockResult.Object);
        closeResult.ShouldBe(mockResult.Object);
        testResult.ShouldBe(mockResult.Object);
        metadataResult.ShouldBe(mockMetadataResult.Object);
    }

    [Theory]
    [InlineData(FdwConnectionState.Unknown)]
    [InlineData(FdwConnectionState.Created)]
    [InlineData(FdwConnectionState.Opening)]
    [InlineData(FdwConnectionState.Open)]
    [InlineData(FdwConnectionState.Executing)]
    [InlineData(FdwConnectionState.Closing)]
    [InlineData(FdwConnectionState.Closed)]
    [InlineData(FdwConnectionState.Broken)]
    [InlineData(FdwConnectionState.Disposed)]
    public void ShouldAcceptAllConnectionStates(FdwConnectionState state)
    {
        // Arrange
        var mock = new Mock<IExternalConnection>();
        mock.Setup(x => x.ConnectionId).Returns("test-id");
        mock.Setup(x => x.ProviderName).Returns("TestProvider");
        mock.Setup(x => x.State).Returns(state);
        mock.Setup(x => x.ConnectionString).Returns("test-connection");

        // Act
        var connection = mock.Object;

        // Assert
        connection.State.ShouldBe(state);
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("PostgreSQL")]
    [InlineData("MongoDB")]
    [InlineData("Oracle")]
    [InlineData("REST API")]
    [InlineData("")]
    public void ShouldAcceptVariousProviderNames(string providerName)
    {
        // Arrange
        var mock = new Mock<IExternalConnection>();
        mock.Setup(x => x.ConnectionId).Returns("test-id");
        mock.Setup(x => x.ProviderName).Returns(providerName);
        mock.Setup(x => x.State).Returns(FdwConnectionState.Created);
        mock.Setup(x => x.ConnectionString).Returns("test-connection");

        // Act
        var connection = mock.Object;

        // Assert
        connection.ProviderName.ShouldBe(providerName);
    }

    [Theory]
    [InlineData("Server=localhost;Database=Test")]
    [InlineData("https://api.example.com")]
    [InlineData("mongodb://localhost:27017/test")]
    [InlineData("Data Source=test.db")]
    [InlineData("")]
    public void ShouldAcceptVariousConnectionStrings(string connectionString)
    {
        // Arrange
        var mock = new Mock<IExternalConnection>();
        mock.Setup(x => x.ConnectionId).Returns("test-id");
        mock.Setup(x => x.ProviderName).Returns("TestProvider");
        mock.Setup(x => x.State).Returns(FdwConnectionState.Created);
        mock.Setup(x => x.ConnectionString).Returns(connectionString);

        // Act
        var connection = mock.Object;

        // Assert
        connection.ConnectionString.ShouldBe(connectionString);
    }

    [Fact]
    public async Task ShouldSupportAsyncOperationsReturningSuccess()
    {
        // Arrange
        var successResult = new Mock<IFdwResult>();
        successResult.Setup(x => x.Success).Returns(true);

        var mock = new Mock<IExternalConnection>();
        mock.Setup(x => x.OpenAsync()).Returns(Task.FromResult(successResult.Object));
        mock.Setup(x => x.CloseAsync()).Returns(Task.FromResult(successResult.Object));
        mock.Setup(x => x.TestConnectionAsync()).Returns(Task.FromResult(successResult.Object));

        // Act
        var connection = mock.Object;
        var openResult = await connection.OpenAsync();
        var closeResult = await connection.CloseAsync();
        var testResult = await connection.TestConnectionAsync();

        // Assert
        openResult.Success.ShouldBeTrue();
        closeResult.Success.ShouldBeTrue();
        testResult.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldSupportAsyncOperationsReturningFailure()
    {
        // Arrange
        var failureResult = new Mock<IFdwResult>();
        failureResult.Setup(x => x.Success).Returns(false);

        var mock = new Mock<IExternalConnection>();
        mock.Setup(x => x.OpenAsync()).Returns(Task.FromResult(failureResult.Object));
        mock.Setup(x => x.CloseAsync()).Returns(Task.FromResult(failureResult.Object));
        mock.Setup(x => x.TestConnectionAsync()).Returns(Task.FromResult(failureResult.Object));

        // Act
        var connection = mock.Object;
        var openResult = await connection.OpenAsync();
        var closeResult = await connection.CloseAsync();
        var testResult = await connection.TestConnectionAsync();

        // Assert
        openResult.Success.ShouldBeFalse();
        closeResult.Success.ShouldBeFalse();
        testResult.Success.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldSupportGetMetadataAsyncReturningSuccess()
    {
        // Arrange
        var mockMetadata = new Mock<IConnectionMetadata>();
        mockMetadata.Setup(x => x.SystemName).Returns("Test System");
        
        var metadataResult = new Mock<IFdwResult<IConnectionMetadata>>();
        metadataResult.Setup(x => x.Success).Returns(true);
        metadataResult.Setup(x => x.Value).Returns(mockMetadata.Object);

        var mock = new Mock<IExternalConnection>();
        mock.Setup(x => x.GetMetadataAsync()).Returns(Task.FromResult(metadataResult.Object));

        // Act
        var connection = mock.Object;
        var result = await connection.GetMetadataAsync();

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.SystemName.ShouldBe("Test System");
    }

    [Fact]
    public async Task ShouldSupportGetMetadataAsyncReturningFailure()
    {
        // Arrange
        var metadataResult = new Mock<IFdwResult<IConnectionMetadata>>();
        metadataResult.Setup(x => x.Success).Returns(false);
        metadataResult.Setup(x => x.Value).Returns((IConnectionMetadata?)null);

        var mock = new Mock<IExternalConnection>();
        mock.Setup(x => x.GetMetadataAsync()).Returns(Task.FromResult(metadataResult.Object));

        // Act
        var connection = mock.Object;
        var result = await connection.GetMetadataAsync();

        // Assert
        result.Success.ShouldBeFalse();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void ShouldSupportDisposePattern()
    {
        // Arrange
        var mock = new Mock<IExternalConnection>();
        var wasDisposed = false;
        mock.Setup(x => x.Dispose()).Callback(() => wasDisposed = true);

        // Act
        var connection = mock.Object;
        connection.Dispose();

        // Assert
        wasDisposed.ShouldBeTrue();
        mock.Verify(x => x.Dispose(), Times.Once);
    }
}