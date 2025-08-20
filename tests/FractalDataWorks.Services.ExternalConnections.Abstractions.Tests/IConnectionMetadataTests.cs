using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;
using Moq;
using FractalDataWorks.Services.ExternalConnections.Abstractions;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests;

/// <summary>
/// Tests for the IConnectionMetadata interface.
/// </summary>
public sealed class IConnectionMetadataTests
{
    [Fact]
    public void ShouldHaveSystemNameProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IConnectionMetadata).GetProperty(nameof(IConnectionMetadata.SystemName));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveVersionProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IConnectionMetadata).GetProperty(nameof(IConnectionMetadata.Version));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveServerInfoProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IConnectionMetadata).GetProperty(nameof(IConnectionMetadata.ServerInfo));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveDatabaseNameProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IConnectionMetadata).GetProperty(nameof(IConnectionMetadata.DatabaseName));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveCapabilitiesProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IConnectionMetadata).GetProperty(nameof(IConnectionMetadata.Capabilities));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(IReadOnlyDictionary<string, object>));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveCollectedAtProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IConnectionMetadata).GetProperty(nameof(IConnectionMetadata.CollectedAt));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(DateTimeOffset));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveCustomPropertiesProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IConnectionMetadata).GetProperty(nameof(IConnectionMetadata.CustomProperties));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(IReadOnlyDictionary<string, object>));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldAllowMockImplementation()
    {
        // Arrange
        var capabilities = new Dictionary<string, object>
        {
            ["SupportsTransactions"] = true,
            ["MaxBatchSize"] = 1000
        };
        var customProperties = new Dictionary<string, object>
        {
            ["ConnectionPool"] = "Active",
            ["AuthMethod"] = "Integrated"
        };
        var collectedAt = DateTimeOffset.UtcNow;

        var mock = new Mock<IConnectionMetadata>();
        mock.Setup(x => x.SystemName).Returns("Microsoft SQL Server");
        mock.Setup(x => x.Version).Returns("15.0.2000");
        mock.Setup(x => x.ServerInfo).Returns("localhost:1433");
        mock.Setup(x => x.DatabaseName).Returns("TestDatabase");
        mock.Setup(x => x.Capabilities).Returns(capabilities);
        mock.Setup(x => x.CollectedAt).Returns(collectedAt);
        mock.Setup(x => x.CustomProperties).Returns(customProperties);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.SystemName.ShouldBe("Microsoft SQL Server");
        metadata.Version.ShouldBe("15.0.2000");
        metadata.ServerInfo.ShouldBe("localhost:1433");
        metadata.DatabaseName.ShouldBe("TestDatabase");
        metadata.Capabilities.ShouldBe(capabilities);
        metadata.CollectedAt.ShouldBe(collectedAt);
        metadata.CustomProperties.ShouldBe(customProperties);
    }

    [Fact]
    public void ShouldSupportNullableProperties()
    {
        // Arrange
        var capabilities = new Dictionary<string, object>();
        var customProperties = new Dictionary<string, object>();
        var collectedAt = DateTimeOffset.UtcNow;

        var mock = new Mock<IConnectionMetadata>();
        mock.Setup(x => x.SystemName).Returns("Test System");
        mock.Setup(x => x.Version).Returns((string?)null);
        mock.Setup(x => x.ServerInfo).Returns((string?)null);
        mock.Setup(x => x.DatabaseName).Returns((string?)null);
        mock.Setup(x => x.Capabilities).Returns(capabilities);
        mock.Setup(x => x.CollectedAt).Returns(collectedAt);
        mock.Setup(x => x.CustomProperties).Returns(customProperties);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.SystemName.ShouldBe("Test System");
        metadata.Version.ShouldBeNull();
        metadata.ServerInfo.ShouldBeNull();
        metadata.DatabaseName.ShouldBeNull();
        metadata.Capabilities.ShouldNotBeNull();
        metadata.CollectedAt.ShouldBe(collectedAt);
        metadata.CustomProperties.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("Microsoft SQL Server")]
    [InlineData("PostgreSQL")]
    [InlineData("MongoDB")]
    [InlineData("Oracle Database")]
    [InlineData("REST API")]
    public void ShouldAcceptVariousSystemNames(string systemName)
    {
        // Arrange
        var mock = new Mock<IConnectionMetadata>();
        mock.Setup(x => x.SystemName).Returns(systemName);
        mock.Setup(x => x.Capabilities).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);
        mock.Setup(x => x.CustomProperties).Returns(new Dictionary<string, object>());

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.SystemName.ShouldBe(systemName);
    }

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("15.0.2000.5")]
    [InlineData("13.2")]
    [InlineData("v2.4.1")]
    [InlineData(null)]
    public void ShouldAcceptVariousVersions(string? version)
    {
        // Arrange
        var mock = new Mock<IConnectionMetadata>();
        mock.Setup(x => x.SystemName).Returns("Test System");
        mock.Setup(x => x.Version).Returns(version);
        mock.Setup(x => x.Capabilities).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);
        mock.Setup(x => x.CustomProperties).Returns(new Dictionary<string, object>());

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.Version.ShouldBe(version);
    }

    [Theory]
    [InlineData("localhost:1433")]
    [InlineData("192.168.1.100:5432")]
    [InlineData("https://api.example.com")]
    [InlineData("server.domain.com")]
    [InlineData(null)]
    public void ShouldAcceptVariousServerInfo(string? serverInfo)
    {
        // Arrange
        var mock = new Mock<IConnectionMetadata>();
        mock.Setup(x => x.SystemName).Returns("Test System");
        mock.Setup(x => x.ServerInfo).Returns(serverInfo);
        mock.Setup(x => x.Capabilities).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);
        mock.Setup(x => x.CustomProperties).Returns(new Dictionary<string, object>());

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.ServerInfo.ShouldBe(serverInfo);
    }

    [Fact]
    public void ShouldWorkWithComplexCapabilities()
    {
        // Arrange
        var capabilities = new Dictionary<string, object>
        {
            ["SupportsTransactions"] = true,
            ["MaxBatchSize"] = 1000,
            ["SupportedIsolationLevels"] = new[] { "ReadCommitted", "Serializable" },
            ["Features"] = new Dictionary<string, object>
            {
                ["BulkOperations"] = true,
                ["Streaming"] = false
            }
        };

        var mock = new Mock<IConnectionMetadata>();
        mock.Setup(x => x.SystemName).Returns("Test System");
        mock.Setup(x => x.Capabilities).Returns(capabilities);
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);
        mock.Setup(x => x.CustomProperties).Returns(new Dictionary<string, object>());

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.Capabilities.ShouldBe(capabilities);
        metadata.Capabilities["SupportsTransactions"].ShouldBe(true);
        metadata.Capabilities["MaxBatchSize"].ShouldBe(1000);
        metadata.Capabilities.ShouldContainKey("SupportedIsolationLevels");
        metadata.Capabilities.ShouldContainKey("Features");
    }

    [Fact]
    public void ShouldWorkWithEmptyCollections()
    {
        // Arrange
        var emptyCapabilities = new Dictionary<string, object>();
        var emptyCustomProperties = new Dictionary<string, object>();
        
        var mock = new Mock<IConnectionMetadata>();
        mock.Setup(x => x.SystemName).Returns("Test System");
        mock.Setup(x => x.Capabilities).Returns(emptyCapabilities);
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);
        mock.Setup(x => x.CustomProperties).Returns(emptyCustomProperties);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.Capabilities.ShouldNotBeNull();
        metadata.Capabilities.Count.ShouldBe(0);
        metadata.CustomProperties.ShouldNotBeNull();
        metadata.CustomProperties.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldSupportDateTimeOffsetCollectedAt()
    {
        // Arrange
        var specificTime = new DateTimeOffset(2023, 10, 15, 14, 30, 0, TimeSpan.FromHours(-5));
        
        var mock = new Mock<IConnectionMetadata>();
        mock.Setup(x => x.SystemName).Returns("Test System");
        mock.Setup(x => x.Capabilities).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(specificTime);
        mock.Setup(x => x.CustomProperties).Returns(new Dictionary<string, object>());

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.CollectedAt.ShouldBe(specificTime);
        metadata.CollectedAt.Offset.ShouldBe(TimeSpan.FromHours(-5));
    }
}