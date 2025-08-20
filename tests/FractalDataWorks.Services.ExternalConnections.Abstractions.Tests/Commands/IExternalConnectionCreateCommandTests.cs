using System;
using Shouldly;
using Xunit;
using Moq;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests.Commands;

/// <summary>
/// Tests for the IExternalConnectionCreateCommand interface.
/// </summary>
public sealed class IExternalConnectionCreateCommandTests
{
    [Fact]
    public void ShouldInheritFromIExternalConnectionCommand()
    {
        // Arrange & Act
        var isAssignable = typeof(IExternalConnectionCommand).IsAssignableFrom(typeof(IExternalConnectionCreateCommand));

        // Assert
        isAssignable.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveConnectionNameProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionCreateCommand).GetProperty(nameof(IExternalConnectionCreateCommand.ConnectionName));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveProviderTypeProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionCreateCommand).GetProperty(nameof(IExternalConnectionCreateCommand.ProviderType));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveConnectionConfigurationProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionCreateCommand).GetProperty(nameof(IExternalConnectionCreateCommand.ConnectionConfiguration));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(IExternalConnectionConfiguration));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldAllowMockImplementation()
    {
        // Arrange
        var mockConfiguration = new Mock<IExternalConnectionConfiguration>();
        var mock = new Mock<IExternalConnectionCreateCommand>();
        
        mock.Setup(x => x.ConnectionName).Returns("TestConnection");
        mock.Setup(x => x.ProviderType).Returns("MsSql");
        mock.Setup(x => x.ConnectionConfiguration).Returns(mockConfiguration.Object);

        // Act
        var command = mock.Object;

        // Assert
        command.ConnectionName.ShouldBe("TestConnection");
        command.ProviderType.ShouldBe("MsSql");
        command.ConnectionConfiguration.ShouldBe(mockConfiguration.Object);
    }

    [Fact]
    public void ShouldSupportNullableProperties()
    {
        // Arrange
        var mock = new Mock<IExternalConnectionCreateCommand>();
        
        mock.Setup(x => x.ConnectionName).Returns((string)null!);
        mock.Setup(x => x.ProviderType).Returns((string)null!);
        mock.Setup(x => x.ConnectionConfiguration).Returns((IExternalConnectionConfiguration)null!);

        // Act
        var command = mock.Object;

        // Assert
        command.ConnectionName.ShouldBeNull();
        command.ProviderType.ShouldBeNull();
        command.ConnectionConfiguration.ShouldBeNull();
    }

    [Fact]
    public void ShouldInheritFromICommand()
    {
        // Arrange & Act
        var isAssignable = typeof(ICommand).IsAssignableFrom(typeof(IExternalConnectionCreateCommand));

        // Assert
        isAssignable.ShouldBeTrue();
    }

    [Theory]
    [InlineData("SqlConnection")]
    [InlineData("PostgreSQLConnection")]
    [InlineData("MongoConnection")]
    [InlineData("")]
    public void ShouldAcceptVariousConnectionNames(string connectionName)
    {
        // Arrange
        var mockConfiguration = new Mock<IExternalConnectionConfiguration>();
        var mock = new Mock<IExternalConnectionCreateCommand>();
        mock.Setup(x => x.ConnectionName).Returns(connectionName);
        mock.Setup(x => x.ProviderType).Returns("TestProvider");
        mock.Setup(x => x.ConnectionConfiguration).Returns(mockConfiguration.Object);

        // Act
        var command = mock.Object;

        // Assert
        command.ConnectionName.ShouldBe(connectionName);
    }

    [Theory]
    [InlineData("MsSql")]
    [InlineData("PostgreSQL")]
    [InlineData("MongoDB")]
    [InlineData("Oracle")]
    [InlineData("")]
    public void ShouldAcceptVariousProviderTypes(string providerType)
    {
        // Arrange
        var mockConfiguration = new Mock<IExternalConnectionConfiguration>();
        var mock = new Mock<IExternalConnectionCreateCommand>();
        mock.Setup(x => x.ConnectionName).Returns("TestConnection");
        mock.Setup(x => x.ProviderType).Returns(providerType);
        mock.Setup(x => x.ConnectionConfiguration).Returns(mockConfiguration.Object);

        // Act
        var command = mock.Object;

        // Assert
        command.ProviderType.ShouldBe(providerType);
    }

    [Fact]
    public void ShouldWorkWithDifferentConfigurationTypes()
    {
        // Arrange
        var mockConfiguration1 = new Mock<IExternalConnectionConfiguration>();
        var mockConfiguration2 = new Mock<IExternalConnectionConfiguration>();
        
        var mock = new Mock<IExternalConnectionCreateCommand>();
        mock.Setup(x => x.ConnectionName).Returns("TestConnection");
        mock.Setup(x => x.ProviderType).Returns("TestProvider");

        // Act & Assert - Should work with different configuration instances
        mock.Setup(x => x.ConnectionConfiguration).Returns(mockConfiguration1.Object);
        var command1 = mock.Object;
        command1.ConnectionConfiguration.ShouldBe(mockConfiguration1.Object);

        mock.Setup(x => x.ConnectionConfiguration).Returns(mockConfiguration2.Object);
        var command2 = mock.Object;
        command2.ConnectionConfiguration.ShouldBe(mockConfiguration2.Object);
    }
}