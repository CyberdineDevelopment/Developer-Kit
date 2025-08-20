using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Moq;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks;
using FractalDataWorks.Configuration;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests;

/// <summary>
/// Tests for the IExternalConnectionFactory interface.
/// </summary>
public sealed class IExternalConnectionFactoryTests
{
    [Fact]
    public void ShouldHaveProviderNameProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionFactory).GetProperty(nameof(IExternalConnectionFactory.ProviderName));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveSupportedConnectionTypesProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionFactory).GetProperty(nameof(IExternalConnectionFactory.SupportedConnectionTypes));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(IReadOnlyList<string>));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveConfigurationTypeProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionFactory).GetProperty(nameof(IExternalConnectionFactory.ConfigurationType));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(Type));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveCreateConnectionAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IExternalConnectionFactory).GetMethod(nameof(IExternalConnectionFactory.CreateConnectionAsync), new[] { typeof(FdwConfigurationBase) });

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IFdwResult<IExternalConnection>>));
        var parameters = methodInfo.GetParameters();
        parameters.Length.ShouldBe(1);
        parameters[0].ParameterType.ShouldBe(typeof(FdwConfigurationBase));
        parameters[0].Name.ShouldBe("configuration");
    }

    [Fact]
    public void ShouldHaveCreateConnectionAsyncWithTypeMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IExternalConnectionFactory).GetMethod(nameof(IExternalConnectionFactory.CreateConnectionAsync), new[] { typeof(FdwConfigurationBase), typeof(string) });

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IFdwResult<IExternalConnection>>));
        var parameters = methodInfo.GetParameters();
        parameters.Length.ShouldBe(2);
        parameters[0].ParameterType.ShouldBe(typeof(FdwConfigurationBase));
        parameters[0].Name.ShouldBe("configuration");
        parameters[1].ParameterType.ShouldBe(typeof(string));
        parameters[1].Name.ShouldBe("connectionType");
    }

    [Fact]
    public void ShouldHaveValidateConfigurationAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IExternalConnectionFactory).GetMethod(nameof(IExternalConnectionFactory.ValidateConfigurationAsync));

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IFdwResult>));
        var parameters = methodInfo.GetParameters();
        parameters.Length.ShouldBe(1);
        parameters[0].ParameterType.ShouldBe(typeof(FdwConfigurationBase));
        parameters[0].Name.ShouldBe("configuration");
    }

    [Fact]
    public void ShouldHaveTestConnectivityAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IExternalConnectionFactory).GetMethod(nameof(IExternalConnectionFactory.TestConnectivityAsync));

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IFdwResult>));
        var parameters = methodInfo.GetParameters();
        parameters.Length.ShouldBe(1);
        parameters[0].ParameterType.ShouldBe(typeof(FdwConfigurationBase));
        parameters[0].Name.ShouldBe("configuration");
    }

    [Fact]
    public async Task ShouldAllowMockImplementation()
    {
        // Arrange
        var supportedTypes = new List<string> { "ReadWrite", "ReadOnly" };
        var mockConfig = new Mock<FdwConfigurationBase>();
        var mockConnection = new Mock<IExternalConnection>();
        var mockConnectionResult = new Mock<IFdwResult<IExternalConnection>>();
        mockConnectionResult.Setup(x => x.Success).Returns(true);
        mockConnectionResult.Setup(x => x.Value).Returns(mockConnection.Object);

        var mockValidationResult = new Mock<IFdwResult>();
        mockValidationResult.Setup(x => x.Success).Returns(true);

        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.ProviderName).Returns("TestProvider");
        mock.Setup(x => x.SupportedConnectionTypes).Returns(supportedTypes);
        mock.Setup(x => x.ConfigurationType).Returns(typeof(FdwConfigurationBase));
        mock.Setup(x => x.CreateConnectionAsync(mockConfig.Object))
            .Returns(Task.FromResult(mockConnectionResult.Object));
        mock.Setup(x => x.CreateConnectionAsync(mockConfig.Object, "ReadWrite"))
            .Returns(Task.FromResult(mockConnectionResult.Object));
        mock.Setup(x => x.ValidateConfigurationAsync(mockConfig.Object))
            .Returns(Task.FromResult(mockValidationResult.Object));
        mock.Setup(x => x.TestConnectivityAsync(mockConfig.Object))
            .Returns(Task.FromResult(mockValidationResult.Object));

        // Act
        var factory = mock.Object;
        var connectionResult = await factory.CreateConnectionAsync(mockConfig.Object);
        var typedConnectionResult = await factory.CreateConnectionAsync(mockConfig.Object, "ReadWrite");
        var validationResult = await factory.ValidateConfigurationAsync(mockConfig.Object);
        var testResult = await factory.TestConnectivityAsync(mockConfig.Object);

        // Assert
        factory.ProviderName.ShouldBe("TestProvider");
        factory.SupportedConnectionTypes.ShouldBe(supportedTypes);
        factory.ConfigurationType.ShouldBe(typeof(FdwConfigurationBase));
        
        connectionResult.Success.ShouldBeTrue();
        connectionResult.Value.ShouldBe(mockConnection.Object);
        
        typedConnectionResult.Success.ShouldBeTrue();
        typedConnectionResult.Value.ShouldBe(mockConnection.Object);
        
        validationResult.Success.ShouldBeTrue();
        testResult.Success.ShouldBeTrue();
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("PostgreSQL")]
    [InlineData("MongoDB")]
    [InlineData("Oracle")]
    [InlineData("")]
    public void ShouldAcceptVariousProviderNames(string providerName)
    {
        // Arrange
        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.ProviderName).Returns(providerName);
        mock.Setup(x => x.SupportedConnectionTypes).Returns(new List<string>());
        mock.Setup(x => x.ConfigurationType).Returns(typeof(FdwConfigurationBase));

        // Act
        var factory = mock.Object;

        // Assert
        factory.ProviderName.ShouldBe(providerName);
    }

    [Fact]
    public void ShouldAcceptVariousSupportedConnectionTypes()
    {
        // Arrange
        var connectionTypes = new List<string> { "ReadWrite", "ReadOnly", "Bulk", "Streaming" };
        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.ProviderName).Returns("TestProvider");
        mock.Setup(x => x.SupportedConnectionTypes).Returns(connectionTypes);
        mock.Setup(x => x.ConfigurationType).Returns(typeof(FdwConfigurationBase));

        // Act
        var factory = mock.Object;

        // Assert
        factory.SupportedConnectionTypes.ShouldBe(connectionTypes);
        factory.SupportedConnectionTypes.Count.ShouldBe(4);
        factory.SupportedConnectionTypes.ShouldContain("ReadWrite");
        factory.SupportedConnectionTypes.ShouldContain("Bulk");
    }

    [Fact]
    public void ShouldAcceptEmptyConnectionTypes()
    {
        // Arrange
        var emptyTypes = new List<string>();
        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.ProviderName).Returns("TestProvider");
        mock.Setup(x => x.SupportedConnectionTypes).Returns(emptyTypes);
        mock.Setup(x => x.ConfigurationType).Returns(typeof(FdwConfigurationBase));

        // Act
        var factory = mock.Object;

        // Assert
        factory.SupportedConnectionTypes.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(typeof(FdwConfigurationBase))]
    [InlineData(typeof(IExternalConnectionConfiguration))]
    [InlineData(typeof(string))] // Invalid but should work in mock
    public void ShouldAcceptVariousConfigurationTypes(Type configurationType)
    {
        // Arrange
        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.ProviderName).Returns("TestProvider");
        mock.Setup(x => x.SupportedConnectionTypes).Returns(new List<string>());
        mock.Setup(x => x.ConfigurationType).Returns(configurationType);

        // Act
        var factory = mock.Object;

        // Assert
        factory.ConfigurationType.ShouldBe(configurationType);
    }

    [Fact]
    public async Task ShouldSupportCreateConnectionAsyncReturningSuccess()
    {
        // Arrange
        var mockConfig = new Mock<FdwConfigurationBase>();
        var mockConnection = new Mock<IExternalConnection>();
        mockConnection.Setup(x => x.ConnectionId).Returns("test-conn-id");
        
        var successResult = new Mock<IFdwResult<IExternalConnection>>();
        successResult.Setup(x => x.Success).Returns(true);
        successResult.Setup(x => x.Value).Returns(mockConnection.Object);

        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.CreateConnectionAsync(mockConfig.Object))
            .Returns(Task.FromResult(successResult.Object));

        // Act
        var factory = mock.Object;
        var result = await factory.CreateConnectionAsync(mockConfig.Object);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ConnectionId.ShouldBe("test-conn-id");
    }

    [Fact]
    public async Task ShouldSupportCreateConnectionAsyncReturningFailure()
    {
        // Arrange
        var mockConfig = new Mock<FdwConfigurationBase>();
        var failureResult = new Mock<IFdwResult<IExternalConnection>>();
        failureResult.Setup(x => x.Success).Returns(false);
        failureResult.Setup(x => x.Value).Returns((IExternalConnection?)null);

        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.CreateConnectionAsync(mockConfig.Object))
            .Returns(Task.FromResult(failureResult.Object));

        // Act
        var factory = mock.Object;
        var result = await factory.CreateConnectionAsync(mockConfig.Object);

        // Assert
        result.Success.ShouldBeFalse();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldSupportCreateConnectionAsyncWithConnectionType()
    {
        // Arrange
        var mockConfig = new Mock<FdwConfigurationBase>();
        var mockConnection = new Mock<IExternalConnection>();
        var successResult = new Mock<IFdwResult<IExternalConnection>>();
        successResult.Setup(x => x.Success).Returns(true);
        successResult.Setup(x => x.Value).Returns(mockConnection.Object);

        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.CreateConnectionAsync(mockConfig.Object, "ReadOnly"))
            .Returns(Task.FromResult(successResult.Object));

        // Act
        var factory = mock.Object;
        var result = await factory.CreateConnectionAsync(mockConfig.Object, "ReadOnly");

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        mock.Verify(x => x.CreateConnectionAsync(mockConfig.Object, "ReadOnly"), Times.Once);
    }

    [Fact]
    public async Task ShouldSupportValidateConfigurationAsyncReturningSuccess()
    {
        // Arrange
        var mockConfig = new Mock<FdwConfigurationBase>();
        var successResult = new Mock<IFdwResult>();
        successResult.Setup(x => x.Success).Returns(true);

        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.ValidateConfigurationAsync(mockConfig.Object))
            .Returns(Task.FromResult(successResult.Object));

        // Act
        var factory = mock.Object;
        var result = await factory.ValidateConfigurationAsync(mockConfig.Object);

        // Assert
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldSupportValidateConfigurationAsyncReturningFailure()
    {
        // Arrange
        var mockConfig = new Mock<FdwConfigurationBase>();
        var failureResult = new Mock<IFdwResult>();
        failureResult.Setup(x => x.Success).Returns(false);

        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.ValidateConfigurationAsync(mockConfig.Object))
            .Returns(Task.FromResult(failureResult.Object));

        // Act
        var factory = mock.Object;
        var result = await factory.ValidateConfigurationAsync(mockConfig.Object);

        // Assert
        result.Success.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldSupportTestConnectivityAsyncReturningSuccess()
    {
        // Arrange
        var mockConfig = new Mock<FdwConfigurationBase>();
        var successResult = new Mock<IFdwResult>();
        successResult.Setup(x => x.Success).Returns(true);

        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.TestConnectivityAsync(mockConfig.Object))
            .Returns(Task.FromResult(successResult.Object));

        // Act
        var factory = mock.Object;
        var result = await factory.TestConnectivityAsync(mockConfig.Object);

        // Assert
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldSupportTestConnectivityAsyncReturningFailure()
    {
        // Arrange
        var mockConfig = new Mock<FdwConfigurationBase>();
        var failureResult = new Mock<IFdwResult>();
        failureResult.Setup(x => x.Success).Returns(false);

        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.TestConnectivityAsync(mockConfig.Object))
            .Returns(Task.FromResult(failureResult.Object));

        // Act
        var factory = mock.Object;
        var result = await factory.TestConnectivityAsync(mockConfig.Object);

        // Assert
        result.Success.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldSupportMultipleAsyncOperations()
    {
        // Arrange
        var mockConfig = new Mock<FdwConfigurationBase>();
        var mockConnection = new Mock<IExternalConnection>();
        
        var connectionResult = new Mock<IFdwResult<IExternalConnection>>();
        connectionResult.Setup(x => x.Success).Returns(true);
        connectionResult.Setup(x => x.Value).Returns(mockConnection.Object);
        
        var validationResult = new Mock<IFdwResult>();
        validationResult.Setup(x => x.Success).Returns(true);

        var mock = new Mock<IExternalConnectionFactory>();
        mock.Setup(x => x.CreateConnectionAsync(mockConfig.Object)).Returns(Task.FromResult(connectionResult.Object));
        mock.Setup(x => x.ValidateConfigurationAsync(mockConfig.Object)).Returns(Task.FromResult(validationResult.Object));
        mock.Setup(x => x.TestConnectivityAsync(mockConfig.Object)).Returns(Task.FromResult(validationResult.Object));

        // Act
        var factory = mock.Object;
        var createTask = factory.CreateConnectionAsync(mockConfig.Object);
        var validateTask = factory.ValidateConfigurationAsync(mockConfig.Object);
        var testTask = factory.TestConnectivityAsync(mockConfig.Object);

        await Task.WhenAll(createTask, validateTask, testTask);

        // Assert
        createTask.Result.Success.ShouldBeTrue();
        validateTask.Result.Success.ShouldBeTrue();
        testTask.Result.Success.ShouldBeTrue();
    }
}