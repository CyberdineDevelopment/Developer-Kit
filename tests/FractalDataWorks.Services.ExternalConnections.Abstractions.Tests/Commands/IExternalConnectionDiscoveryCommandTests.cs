using System;
using Shouldly;
using Xunit;
using Moq;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests.Commands;

/// <summary>
/// Tests for the IExternalConnectionDiscoveryCommand interface.
/// </summary>
public sealed class IExternalConnectionDiscoveryCommandTests
{
    [Fact]
    public void ShouldInheritFromIExternalConnectionCommand()
    {
        // Arrange & Act
        var isAssignable = typeof(IExternalConnectionCommand).IsAssignableFrom(typeof(IExternalConnectionDiscoveryCommand));

        // Assert
        isAssignable.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveConnectionNameProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionDiscoveryCommand).GetProperty(nameof(IExternalConnectionDiscoveryCommand.ConnectionName));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveStartPathProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionDiscoveryCommand).GetProperty(nameof(IExternalConnectionDiscoveryCommand.StartPath));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveOptionsProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IExternalConnectionDiscoveryCommand).GetProperty(nameof(IExternalConnectionDiscoveryCommand.Options));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(ConnectionDiscoveryOptions));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldAllowMockImplementation()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();
        var mock = new Mock<IExternalConnectionDiscoveryCommand>();
        
        mock.Setup(x => x.ConnectionName).Returns("TestConnection");
        mock.Setup(x => x.StartPath).Returns("/database/schema");
        mock.Setup(x => x.Options).Returns(options);

        // Act
        var command = mock.Object;

        // Assert
        command.ConnectionName.ShouldBe("TestConnection");
        command.StartPath.ShouldBe("/database/schema");
        command.Options.ShouldBe(options);
    }

    [Fact]
    public void ShouldSupportNullableStartPath()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();
        var mock = new Mock<IExternalConnectionDiscoveryCommand>();
        
        mock.Setup(x => x.ConnectionName).Returns("TestConnection");
        mock.Setup(x => x.StartPath).Returns((string?)null);
        mock.Setup(x => x.Options).Returns(options);

        // Act
        var command = mock.Object;

        // Assert
        command.ConnectionName.ShouldBe("TestConnection");
        command.StartPath.ShouldBeNull();
        command.Options.ShouldBe(options);
    }

    [Fact]
    public void ShouldInheritFromICommand()
    {
        // Arrange & Act
        var isAssignable = typeof(ICommand).IsAssignableFrom(typeof(IExternalConnectionDiscoveryCommand));

        // Assert
        isAssignable.ShouldBeTrue();
    }

    [Theory]
    [InlineData("DatabaseConnection")]
    [InlineData("APIConnection")]
    [InlineData("FileSystemConnection")]
    [InlineData("")]
    public void ShouldAcceptVariousConnectionNames(string connectionName)
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();
        var mock = new Mock<IExternalConnectionDiscoveryCommand>();
        mock.Setup(x => x.ConnectionName).Returns(connectionName);
        mock.Setup(x => x.StartPath).Returns("/root");
        mock.Setup(x => x.Options).Returns(options);

        // Act
        var command = mock.Object;

        // Assert
        command.ConnectionName.ShouldBe(connectionName);
    }

    [Theory]
    [InlineData("/database")]
    [InlineData("/database/schema")]
    [InlineData("/database/schema/table")]
    [InlineData("C:\\Files")]
    [InlineData("")]
    [InlineData(null)]
    public void ShouldAcceptVariousStartPaths(string? startPath)
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();
        var mock = new Mock<IExternalConnectionDiscoveryCommand>();
        mock.Setup(x => x.ConnectionName).Returns("TestConnection");
        mock.Setup(x => x.StartPath).Returns(startPath);
        mock.Setup(x => x.Options).Returns(options);

        // Act
        var command = mock.Object;

        // Assert
        command.StartPath.ShouldBe(startPath);
    }

    [Fact]
    public void ShouldWorkWithDifferentDiscoveryOptions()
    {
        // Arrange
        var options1 = new ConnectionDiscoveryOptions
        {
            IncludeMetadata = true,
            IncludeColumns = false,
            MaxDepth = 5
        };
        
        var options2 = new ConnectionDiscoveryOptions
        {
            IncludeMetadata = false,
            IncludeColumns = true,
            IncludeRelationships = true,
            MaxDepth = 1
        };
        
        var mock = new Mock<IExternalConnectionDiscoveryCommand>();
        mock.Setup(x => x.ConnectionName).Returns("TestConnection");
        mock.Setup(x => x.StartPath).Returns("/root");

        // Act & Assert - Should work with different option configurations
        mock.Setup(x => x.Options).Returns(options1);
        var command1 = mock.Object;
        command1.Options.ShouldBe(options1);
        command1.Options.MaxDepth.ShouldBe(5);

        mock.Setup(x => x.Options).Returns(options2);
        var command2 = mock.Object;
        command2.Options.ShouldBe(options2);
        command2.Options.MaxDepth.ShouldBe(1);
        command2.Options.IncludeRelationships.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHandleNullConnectionName()
    {
        // Arrange
        var options = new ConnectionDiscoveryOptions();
        var mock = new Mock<IExternalConnectionDiscoveryCommand>();
        
        mock.Setup(x => x.ConnectionName).Returns((string)null!);
        mock.Setup(x => x.StartPath).Returns("/root");
        mock.Setup(x => x.Options).Returns(options);

        // Act
        var command = mock.Object;

        // Assert
        command.ConnectionName.ShouldBeNull();
        command.StartPath.ShouldBe("/root");
        command.Options.ShouldBe(options);
    }

    [Fact]
    public void ShouldHandleNullOptions()
    {
        // Arrange
        var mock = new Mock<IExternalConnectionDiscoveryCommand>();
        
        mock.Setup(x => x.ConnectionName).Returns("TestConnection");
        mock.Setup(x => x.StartPath).Returns("/root");
        mock.Setup(x => x.Options).Returns((ConnectionDiscoveryOptions)null!);

        // Act
        var command = mock.Object;

        // Assert
        command.ConnectionName.ShouldBe("TestConnection");
        command.StartPath.ShouldBe("/root");
        command.Options.ShouldBeNull();
    }

    [Fact]
    public void ShouldSupportComplexDiscoveryScenario()
    {
        // Arrange
        var complexOptions = new ConnectionDiscoveryOptions
        {
            IncludeMetadata = true,
            IncludeColumns = true,
            IncludeRelationships = true,
            IncludeIndexes = true,
            MaxDepth = 10
        };
        
        var mock = new Mock<IExternalConnectionDiscoveryCommand>();
        mock.Setup(x => x.ConnectionName).Returns("ProductionDatabase");
        mock.Setup(x => x.StartPath).Returns("/production/schema/tables");
        mock.Setup(x => x.Options).Returns(complexOptions);

        // Act
        var command = mock.Object;

        // Assert
        command.ConnectionName.ShouldBe("ProductionDatabase");
        command.StartPath.ShouldBe("/production/schema/tables");
        command.Options.ShouldBe(complexOptions);
        command.Options.IncludeMetadata.ShouldBeTrue();
        command.Options.IncludeColumns.ShouldBeTrue();
        command.Options.IncludeRelationships.ShouldBeTrue();
        command.Options.IncludeIndexes.ShouldBeTrue();
        command.Options.MaxDepth.ShouldBe(10);
    }
}