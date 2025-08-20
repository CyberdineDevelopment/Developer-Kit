using System;
using Shouldly;
using Xunit;
using Moq;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests.Commands;

/// <summary>
/// Tests for the IExternalConnectionManagementCommand interface.
/// </summary>
public sealed class IExternalConnectionManagementCommandTests
{
    [Fact]
    public void ShouldInheritFromIExternalConnectionCommand()
    {
        // Arrange & Act
        var isAssignable = typeof(IExternalConnectionCommand).IsAssignableFrom(typeof(IExternalConnectionManagementCommand));

        // Assert
        isAssignable.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveOperationProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionManagementCommand).GetProperty(nameof(IExternalConnectionManagementCommand.Operation));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(ConnectionManagementOperation));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveConnectionNameProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionManagementCommand).GetProperty(nameof(IExternalConnectionManagementCommand.ConnectionName));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldAllowMockImplementation()
    {
        // Arrange
        var mock = new Mock<IExternalConnectionManagementCommand>();
        
        mock.Setup(x => x.Operation).Returns(ConnectionManagementOperation.RemoveConnection);
        mock.Setup(x => x.ConnectionName).Returns("TestConnection");

        // Act
        var command = mock.Object;

        // Assert
        command.Operation.ShouldBe(ConnectionManagementOperation.RemoveConnection);
        command.ConnectionName.ShouldBe("TestConnection");
    }

    [Fact]
    public void ShouldSupportNullableConnectionName()
    {
        // Arrange
        var mock = new Mock<IExternalConnectionManagementCommand>();
        
        mock.Setup(x => x.Operation).Returns(ConnectionManagementOperation.ListConnections);
        mock.Setup(x => x.ConnectionName).Returns((string?)null);

        // Act
        var command = mock.Object;

        // Assert
        command.Operation.ShouldBe(ConnectionManagementOperation.ListConnections);
        command.ConnectionName.ShouldBeNull();
    }

    [Fact]
    public void ShouldInheritFromICommand()
    {
        // Arrange & Act
        var isAssignable = typeof(ICommand).IsAssignableFrom(typeof(IExternalConnectionManagementCommand));

        // Assert
        isAssignable.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ConnectionManagementOperation.ListConnections)]
    [InlineData(ConnectionManagementOperation.RemoveConnection)]
    [InlineData(ConnectionManagementOperation.GetConnectionMetadata)]
    [InlineData(ConnectionManagementOperation.RefreshConnectionStatus)]
    [InlineData(ConnectionManagementOperation.TestConnection)]
    public void ShouldAcceptAllConnectionManagementOperations(ConnectionManagementOperation operation)
    {
        // Arrange
        var mock = new Mock<IExternalConnectionManagementCommand>();
        mock.Setup(x => x.Operation).Returns(operation);
        mock.Setup(x => x.ConnectionName).Returns("TestConnection");

        // Act
        var command = mock.Object;

        // Assert
        command.Operation.ShouldBe(operation);
    }

    [Theory]
    [InlineData("DatabaseConnection")]
    [InlineData("APIConnection")]
    [InlineData("FileSystemConnection")]
    [InlineData("")]
    [InlineData(null)]
    public void ShouldAcceptVariousConnectionNames(string? connectionName)
    {
        // Arrange
        var mock = new Mock<IExternalConnectionManagementCommand>();
        mock.Setup(x => x.Operation).Returns(ConnectionManagementOperation.TestConnection);
        mock.Setup(x => x.ConnectionName).Returns(connectionName);

        // Act
        var command = mock.Object;

        // Assert
        command.ConnectionName.ShouldBe(connectionName);
    }

    [Fact]
    public void ShouldSupportListConnectionsOperation()
    {
        // Arrange
        var mock = new Mock<IExternalConnectionManagementCommand>();
        mock.Setup(x => x.Operation).Returns(ConnectionManagementOperation.ListConnections);
        mock.Setup(x => x.ConnectionName).Returns((string?)null); // List operations typically don't need connection name

        // Act
        var command = mock.Object;

        // Assert
        command.Operation.ShouldBe(ConnectionManagementOperation.ListConnections);
        command.ConnectionName.ShouldBeNull();
    }

    [Fact]
    public void ShouldSupportRemoveConnectionOperation()
    {
        // Arrange
        var mock = new Mock<IExternalConnectionManagementCommand>();
        mock.Setup(x => x.Operation).Returns(ConnectionManagementOperation.RemoveConnection);
        mock.Setup(x => x.ConnectionName).Returns("ConnectionToRemove");

        // Act
        var command = mock.Object;

        // Assert
        command.Operation.ShouldBe(ConnectionManagementOperation.RemoveConnection);
        command.ConnectionName.ShouldBe("ConnectionToRemove");
    }

    [Fact]
    public void ShouldSupportGetConnectionMetadataOperation()
    {
        // Arrange
        var mock = new Mock<IExternalConnectionManagementCommand>();
        mock.Setup(x => x.Operation).Returns(ConnectionManagementOperation.GetConnectionMetadata);
        mock.Setup(x => x.ConnectionName).Returns("MetadataConnection");

        // Act
        var command = mock.Object;

        // Assert
        command.Operation.ShouldBe(ConnectionManagementOperation.GetConnectionMetadata);
        command.ConnectionName.ShouldBe("MetadataConnection");
    }

    [Fact]
    public void ShouldSupportRefreshConnectionStatusOperation()
    {
        // Arrange
        var mock = new Mock<IExternalConnectionManagementCommand>();
        mock.Setup(x => x.Operation).Returns(ConnectionManagementOperation.RefreshConnectionStatus);
        mock.Setup(x => x.ConnectionName).Returns("StatusConnection");

        // Act
        var command = mock.Object;

        // Assert
        command.Operation.ShouldBe(ConnectionManagementOperation.RefreshConnectionStatus);
        command.ConnectionName.ShouldBe("StatusConnection");
    }

    [Fact]
    public void ShouldSupportTestConnectionOperation()
    {
        // Arrange
        var mock = new Mock<IExternalConnectionManagementCommand>();
        mock.Setup(x => x.Operation).Returns(ConnectionManagementOperation.TestConnection);
        mock.Setup(x => x.ConnectionName).Returns("TestableConnection");

        // Act
        var command = mock.Object;

        // Assert
        command.Operation.ShouldBe(ConnectionManagementOperation.TestConnection);
        command.ConnectionName.ShouldBe("TestableConnection");
    }

    [Fact]
    public void ShouldWorkWithDifferentOperationsAndConnectionNames()
    {
        // Arrange
        var testCases = new[]
        {
            (ConnectionManagementOperation.ListConnections, (string?)null),
            (ConnectionManagementOperation.RemoveConnection, "Connection1"),
            (ConnectionManagementOperation.GetConnectionMetadata, "Connection2"),
            (ConnectionManagementOperation.RefreshConnectionStatus, "Connection3"),
            (ConnectionManagementOperation.TestConnection, "Connection4")
        };

        foreach (var (operation, connectionName) in testCases)
        {
            var mock = new Mock<IExternalConnectionManagementCommand>();
            mock.Setup(x => x.Operation).Returns(operation);
            mock.Setup(x => x.ConnectionName).Returns(connectionName);

            // Act
            var command = mock.Object;

            // Assert
            command.Operation.ShouldBe(operation);
            command.ConnectionName.ShouldBe(connectionName);
        }
    }

    [Fact]
    public void ShouldHandleMultipleCommandInstances()
    {
        // Arrange
        var mock1 = new Mock<IExternalConnectionManagementCommand>();
        mock1.Setup(x => x.Operation).Returns(ConnectionManagementOperation.ListConnections);
        mock1.Setup(x => x.ConnectionName).Returns((string?)null);

        var mock2 = new Mock<IExternalConnectionManagementCommand>();
        mock2.Setup(x => x.Operation).Returns(ConnectionManagementOperation.TestConnection);
        mock2.Setup(x => x.ConnectionName).Returns("TestConnection");

        // Act
        var command1 = mock1.Object;
        var command2 = mock2.Object;

        // Assert
        command1.Operation.ShouldBe(ConnectionManagementOperation.ListConnections);
        command1.ConnectionName.ShouldBeNull();
        
        command2.Operation.ShouldBe(ConnectionManagementOperation.TestConnection);
        command2.ConnectionName.ShouldBe("TestConnection");
        
        command1.ShouldNotBe(command2);
    }
}