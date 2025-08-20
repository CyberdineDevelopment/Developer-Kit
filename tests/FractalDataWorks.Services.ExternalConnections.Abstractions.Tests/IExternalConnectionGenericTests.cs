using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Moq;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests;

/// <summary>
/// Tests for the IExternalConnection&lt;TConfiguration&gt; interface.
/// </summary>
public sealed class IExternalConnectionGenericTests
{
    [Fact]
    public void ShouldInheritFromBaseIExternalConnection()
    {
        // Arrange & Act
        var isAssignable = typeof(IExternalConnection).IsAssignableFrom(typeof(IExternalConnection<IExternalConnectionConfiguration>));

        // Assert
        isAssignable.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveConfigurationProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnection<IExternalConnectionConfiguration>).GetProperty("Configuration");

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(IExternalConnectionConfiguration));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveInitializeAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IExternalConnection<IExternalConnectionConfiguration>).GetMethod("InitializeAsync");

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IFdwResult>));
        var parameters = methodInfo.GetParameters();
        parameters.Length.ShouldBe(1);
        parameters[0].ParameterType.ShouldBe(typeof(IExternalConnectionConfiguration));
        parameters[0].Name.ShouldBe("configuration");
    }

    [Fact]
    public void ShouldHaveGenericTypeConstraint()
    {
        // Arrange & Act
        var interfaceType = typeof(IExternalConnection<>);
        var genericParameter = interfaceType.GetGenericArguments()[0];
        var constraints = genericParameter.GetGenericParameterConstraints();

        // Assert
        constraints.Length.ShouldBe(1);
        constraints[0].ShouldBe(typeof(IExternalConnectionConfiguration));
    }

    [Fact]
    public async Task ShouldAllowMockImplementationWithConfiguration()
    {
        // Arrange
        var mockConfiguration = new Mock<IExternalConnectionConfiguration>();
        var mockResult = new Mock<IFdwResult>();
        mockResult.Setup(x => x.Success).Returns(true);

        var mock = new Mock<IExternalConnection<IExternalConnectionConfiguration>>();
        mock.Setup(x => x.ConnectionId).Returns("test-connection-id");
        mock.Setup(x => x.ProviderName).Returns("TestProvider");
        mock.Setup(x => x.State).Returns(FdwConnectionState.Created);
        mock.Setup(x => x.ConnectionString).Returns("test-connection-string");
        mock.Setup(x => x.Configuration).Returns(mockConfiguration.Object);
        mock.Setup(x => x.InitializeAsync(It.IsAny<IExternalConnectionConfiguration>()))
            .Returns(Task.FromResult(mockResult.Object));

        // Act
        var connection = mock.Object;
        var initResult = await connection.InitializeAsync(mockConfiguration.Object);

        // Assert
        connection.Configuration.ShouldBe(mockConfiguration.Object);
        initResult.ShouldBe(mockResult.Object);
        initResult.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldSupportInitializeAsyncWithValidConfiguration()
    {
        // Arrange
        var mockConfiguration = new Mock<IExternalConnectionConfiguration>();
        var successResult = new Mock<IFdwResult>();
        successResult.Setup(x => x.Success).Returns(true);

        var mock = new Mock<IExternalConnection<IExternalConnectionConfiguration>>();
        mock.Setup(x => x.InitializeAsync(mockConfiguration.Object))
            .Returns(Task.FromResult(successResult.Object));

        // Act
        var connection = mock.Object;
        var result = await connection.InitializeAsync(mockConfiguration.Object);

        // Assert
        result.Success.ShouldBeTrue();
        mock.Verify(x => x.InitializeAsync(mockConfiguration.Object), Times.Once);
    }

    [Fact]
    public async Task ShouldSupportInitializeAsyncWithFailure()
    {
        // Arrange
        var mockConfiguration = new Mock<IExternalConnectionConfiguration>();
        var failureResult = new Mock<IFdwResult>();
        failureResult.Setup(x => x.Success).Returns(false);

        var mock = new Mock<IExternalConnection<IExternalConnectionConfiguration>>();
        mock.Setup(x => x.InitializeAsync(mockConfiguration.Object))
            .Returns(Task.FromResult(failureResult.Object));

        // Act
        var connection = mock.Object;
        var result = await connection.InitializeAsync(mockConfiguration.Object);

        // Assert
        result.Success.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldAllowNullConfigurationInMock()
    {
        // Arrange
        var mock = new Mock<IExternalConnection<IExternalConnectionConfiguration>>();
        mock.Setup(x => x.Configuration).Returns((IExternalConnectionConfiguration?)null);

        // Act
        var connection = mock.Object;

        // Assert
        connection.Configuration.ShouldBeNull();
    }

    [Fact]
    public void ShouldWorkWithCustomConfigurationTypes()
    {
        // Arrange - Create a custom configuration interface that extends IExternalConnectionConfiguration
        var customConfigType = typeof(IExternalConnection<TestCustomConfiguration>);

        // Act & Assert
        customConfigType.ShouldNotBeNull();
        
        var configProperty = customConfigType.GetProperty("Configuration");
        configProperty.ShouldNotBeNull();
        configProperty.PropertyType.ShouldBe(typeof(TestCustomConfiguration));

        var initMethod = customConfigType.GetMethod("InitializeAsync");
        initMethod.ShouldNotBeNull();
        var parameters = initMethod.GetParameters();
        parameters.Length.ShouldBe(1);
        parameters[0].ParameterType.ShouldBe(typeof(TestCustomConfiguration));
    }

    [Fact]
    public async Task ShouldSupportCustomConfigurationImplementation()
    {
        // Arrange
        var customConfig = new Mock<TestCustomConfiguration>();
        var result = new Mock<IFdwResult>();
        result.Setup(x => x.Success).Returns(true);

        var mock = new Mock<IExternalConnection<TestCustomConfiguration>>();
        mock.Setup(x => x.Configuration).Returns(customConfig.Object);
        mock.Setup(x => x.InitializeAsync(customConfig.Object))
            .Returns(Task.FromResult(result.Object));

        // Act
        var connection = mock.Object;
        var initResult = await connection.InitializeAsync(customConfig.Object);

        // Assert
        connection.Configuration.ShouldBe(customConfig.Object);
        initResult.Success.ShouldBeTrue();
    }

    [Fact]
    public void ShouldInheritAllBaseInterfaceMembers()
    {
        // Arrange
        var baseInterface = typeof(IExternalConnection);
        var genericInterface = typeof(IExternalConnection<IExternalConnectionConfiguration>);

        // Act
        var baseProperties = baseInterface.GetProperties();
        var baseMethods = baseInterface.GetMethods().Where(m => !m.IsSpecialName).ToArray(); // Exclude property getters/setters

        // Assert - Verify generic interface inherits all base members
        foreach (var baseProp in baseProperties)
        {
            var prop = genericInterface.GetProperty(baseProp.Name);
            prop.ShouldNotBeNull($"Property {baseProp.Name} should be inherited");
        }

        foreach (var baseMethod in baseMethods)
        {
            var method = genericInterface.GetMethod(baseMethod.Name, baseMethod.GetParameters().Select(p => p.ParameterType).ToArray());
            method.ShouldNotBeNull($"Method {baseMethod.Name} should be inherited");
        }
    }

    [Fact]
    public async Task ShouldSupportAllBaseInterfaceOperations()
    {
        // Arrange
        var mockConfig = new Mock<IExternalConnectionConfiguration>();
        var mockResult = new Mock<IFdwResult>();
        mockResult.Setup(x => x.Success).Returns(true);

        var mockMetadata = new Mock<IConnectionMetadata>();
        var mockMetadataResult = new Mock<IFdwResult<IConnectionMetadata>>();
        mockMetadataResult.Setup(x => x.Success).Returns(true);
        mockMetadataResult.Setup(x => x.Value).Returns(mockMetadata.Object);

        var mock = new Mock<IExternalConnection<IExternalConnectionConfiguration>>();
        
        // Setup base interface members
        mock.Setup(x => x.ConnectionId).Returns("test-id");
        mock.Setup(x => x.ProviderName).Returns("TestProvider");
        mock.Setup(x => x.State).Returns(FdwConnectionState.Open);
        mock.Setup(x => x.ConnectionString).Returns("test-connection");
        mock.Setup(x => x.OpenAsync()).Returns(Task.FromResult(mockResult.Object));
        mock.Setup(x => x.CloseAsync()).Returns(Task.FromResult(mockResult.Object));
        mock.Setup(x => x.TestConnectionAsync()).Returns(Task.FromResult(mockResult.Object));
        mock.Setup(x => x.GetMetadataAsync()).Returns(Task.FromResult(mockMetadataResult.Object));
        
        // Setup generic interface members
        mock.Setup(x => x.Configuration).Returns(mockConfig.Object);
        mock.Setup(x => x.InitializeAsync(mockConfig.Object)).Returns(Task.FromResult(mockResult.Object));

        // Act
        var connection = mock.Object;
        var openResult = await connection.OpenAsync();
        var closeResult = await connection.CloseAsync();
        var testResult = await connection.TestConnectionAsync();
        var metadataResult = await connection.GetMetadataAsync();
        var initResult = await connection.InitializeAsync(mockConfig.Object);

        // Assert
        connection.ConnectionId.ShouldBe("test-id");
        connection.ProviderName.ShouldBe("TestProvider");
        connection.State.ShouldBe(FdwConnectionState.Open);
        connection.ConnectionString.ShouldBe("test-connection");
        connection.Configuration.ShouldBe(mockConfig.Object);
        
        openResult.Success.ShouldBeTrue();
        closeResult.Success.ShouldBeTrue();
        testResult.Success.ShouldBeTrue();
        metadataResult.Success.ShouldBeTrue();
        initResult.Success.ShouldBeTrue();
    }

    // Test configuration interface for custom configuration type testing
    public interface TestCustomConfiguration : IExternalConnectionConfiguration
    {
        string CustomProperty { get; }
    }
}