using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;
using Moq;
using FractalDataWorks.Services.ExternalConnections.Abstractions;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions.Tests;

/// <summary>
/// Tests for the IProviderMetadata interface.
/// </summary>
public sealed class IProviderMetadataTests
{
    [Fact]
    public void ShouldHaveProviderNameProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IProviderMetadata).GetProperty(nameof(IProviderMetadata.ProviderName));

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
        var propertyInfo = typeof(IProviderMetadata).GetProperty(nameof(IProviderMetadata.Version));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveDriverVersionProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IProviderMetadata).GetProperty(nameof(IProviderMetadata.DriverVersion));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(string));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveSupportedProtocolVersionsProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IProviderMetadata).GetProperty(nameof(IProviderMetadata.SupportedProtocolVersions));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(IReadOnlyList<string>));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHavePerformanceCharacteristicsProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IProviderMetadata).GetProperty(nameof(IProviderMetadata.PerformanceCharacteristics));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(IReadOnlyDictionary<string, object>));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveFeatureFlagsProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IProviderMetadata).GetProperty(nameof(IProviderMetadata.FeatureFlags));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(IReadOnlyDictionary<string, object>));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveSecurityFeaturesProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IProviderMetadata).GetProperty(nameof(IProviderMetadata.SecurityFeatures));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(IReadOnlyList<string>));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveLimitationsProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IProviderMetadata).GetProperty(nameof(IProviderMetadata.Limitations));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(IReadOnlyDictionary<string, object>));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldHaveCustomMetadataProperty()
    {
        // Arrange & Act
        var propertyInfo = typeof(IProviderMetadata).GetProperty(nameof(IProviderMetadata.CustomMetadata));

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
        var propertyInfo = typeof(IProviderMetadata).GetProperty(nameof(IProviderMetadata.CollectedAt));

        // Assert
        propertyInfo.ShouldNotBeNull();
        propertyInfo.PropertyType.ShouldBe(typeof(DateTimeOffset));
        propertyInfo.CanRead.ShouldBeTrue();
        propertyInfo.CanWrite.ShouldBeFalse(); // Should be read-only
    }

    [Fact]
    public void ShouldAllowMockImplementation()
    {
        // Arrange
        var protocolVersions = new List<string> { "TDS 7.4", "TDS 8.0" };
        var performanceCharacteristics = new Dictionary<string, object>
        {
            ["MaxConnections"] = 1000,
            ["DefaultTimeout"] = 30,
            ["MaxBatchSize"] = 5000
        };
        var featureFlags = new Dictionary<string, object>
        {
            ["Transactions"] = true,
            ["BulkOperations"] = true,
            ["Streaming"] = false
        };
        var securityFeatures = new List<string> { "IntegratedAuth", "SSL", "CertificateAuth" };
        var limitations = new Dictionary<string, object>
        {
            ["MaxQueryLength"] = 65536,
            ["MaxParameterCount"] = 2100
        };
        var customMetadata = new Dictionary<string, object>
        {
            ["VendorSpecific"] = "CustomValue",
            ["InternalVersion"] = "1.2.3.4"
        };
        var collectedAt = DateTimeOffset.UtcNow;

        var mock = new Mock<IProviderMetadata>();
        mock.Setup(x => x.ProviderName).Returns("SQL Server Provider");
        mock.Setup(x => x.Version).Returns("1.0.0");
        mock.Setup(x => x.DriverVersion).Returns("Microsoft.Data.SqlClient 5.0.0");
        mock.Setup(x => x.SupportedProtocolVersions).Returns(protocolVersions);
        mock.Setup(x => x.PerformanceCharacteristics).Returns(performanceCharacteristics);
        mock.Setup(x => x.FeatureFlags).Returns(featureFlags);
        mock.Setup(x => x.SecurityFeatures).Returns(securityFeatures);
        mock.Setup(x => x.Limitations).Returns(limitations);
        mock.Setup(x => x.CustomMetadata).Returns(customMetadata);
        mock.Setup(x => x.CollectedAt).Returns(collectedAt);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.ProviderName.ShouldBe("SQL Server Provider");
        metadata.Version.ShouldBe("1.0.0");
        metadata.DriverVersion.ShouldBe("Microsoft.Data.SqlClient 5.0.0");
        metadata.SupportedProtocolVersions.ShouldBe(protocolVersions);
        metadata.PerformanceCharacteristics.ShouldBe(performanceCharacteristics);
        metadata.FeatureFlags.ShouldBe(featureFlags);
        metadata.SecurityFeatures.ShouldBe(securityFeatures);
        metadata.Limitations.ShouldBe(limitations);
        metadata.CustomMetadata.ShouldBe(customMetadata);
        metadata.CollectedAt.ShouldBe(collectedAt);
    }

    [Fact]
    public void ShouldSupportNullableDriverVersion()
    {
        // Arrange
        var mock = new Mock<IProviderMetadata>();
        mock.Setup(x => x.ProviderName).Returns("Test Provider");
        mock.Setup(x => x.Version).Returns("1.0.0");
        mock.Setup(x => x.DriverVersion).Returns((string?)null);
        mock.Setup(x => x.SupportedProtocolVersions).Returns(new List<string>());
        mock.Setup(x => x.PerformanceCharacteristics).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.FeatureFlags).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.SecurityFeatures).Returns(new List<string>());
        mock.Setup(x => x.Limitations).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CustomMetadata).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.DriverVersion.ShouldBeNull();
    }

    [Theory]
    [InlineData("SQL Server Provider")]
    [InlineData("PostgreSQL Provider")]
    [InlineData("MongoDB Provider")]
    [InlineData("Oracle Provider")]
    public void ShouldAcceptVariousProviderNames(string providerName)
    {
        // Arrange
        var mock = new Mock<IProviderMetadata>();
        mock.Setup(x => x.ProviderName).Returns(providerName);
        mock.Setup(x => x.Version).Returns("1.0.0");
        mock.Setup(x => x.SupportedProtocolVersions).Returns(new List<string>());
        mock.Setup(x => x.PerformanceCharacteristics).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.FeatureFlags).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.SecurityFeatures).Returns(new List<string>());
        mock.Setup(x => x.Limitations).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CustomMetadata).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.ProviderName.ShouldBe(providerName);
    }

    [Fact]
    public void ShouldWorkWithComplexProtocolVersions()
    {
        // Arrange
        var protocolVersions = new List<string>
        {
            "HTTP/1.1",
            "HTTP/2.0",
            "TDS 7.4",
            "TDS 8.0",
            "PostgreSQL 3.0",
            "MongoDB Wire Protocol 6.0"
        };

        var mock = new Mock<IProviderMetadata>();
        mock.Setup(x => x.ProviderName).Returns("Multi-Protocol Provider");
        mock.Setup(x => x.Version).Returns("1.0.0");
        mock.Setup(x => x.SupportedProtocolVersions).Returns(protocolVersions);
        mock.Setup(x => x.PerformanceCharacteristics).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.FeatureFlags).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.SecurityFeatures).Returns(new List<string>());
        mock.Setup(x => x.Limitations).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CustomMetadata).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.SupportedProtocolVersions.ShouldBe(protocolVersions);
        metadata.SupportedProtocolVersions.Count.ShouldBe(6);
        metadata.SupportedProtocolVersions.ShouldContain("HTTP/2.0");
        metadata.SupportedProtocolVersions.ShouldContain("TDS 8.0");
    }

    [Fact]
    public void ShouldWorkWithComplexPerformanceCharacteristics()
    {
        // Arrange
        var performanceCharacteristics = new Dictionary<string, object>
        {
            ["MaxConnections"] = 1000,
            ["DefaultTimeout"] = TimeSpan.FromSeconds(30),
            ["MaxBatchSize"] = 5000,
            ["ConnectionPoolSettings"] = new Dictionary<string, object>
            {
                ["MinSize"] = 10,
                ["MaxSize"] = 100,
                ["GrowthIncrement"] = 5
            },
            ["ThroughputMbps"] = 1000.0,
            ["LatencyMs"] = 1.5
        };

        var mock = new Mock<IProviderMetadata>();
        mock.Setup(x => x.ProviderName).Returns("High-Performance Provider");
        mock.Setup(x => x.Version).Returns("1.0.0");
        mock.Setup(x => x.SupportedProtocolVersions).Returns(new List<string>());
        mock.Setup(x => x.PerformanceCharacteristics).Returns(performanceCharacteristics);
        mock.Setup(x => x.FeatureFlags).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.SecurityFeatures).Returns(new List<string>());
        mock.Setup(x => x.Limitations).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CustomMetadata).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.PerformanceCharacteristics.ShouldBe(performanceCharacteristics);
        metadata.PerformanceCharacteristics["MaxConnections"].ShouldBe(1000);
        metadata.PerformanceCharacteristics["ThroughputMbps"].ShouldBe(1000.0);
        metadata.PerformanceCharacteristics.ShouldContainKey("ConnectionPoolSettings");
    }

    [Fact]
    public void ShouldWorkWithVariousFeatureFlags()
    {
        // Arrange
        var featureFlags = new Dictionary<string, object>
        {
            ["Transactions"] = true,
            ["BulkOperations"] = true,
            ["Streaming"] = false,
            ["Compression"] = "gzip",
            ["AsyncOperations"] = true,
            ["BatchingSupport"] = "v2",
            ["CachingLevel"] = 3
        };

        var mock = new Mock<IProviderMetadata>();
        mock.Setup(x => x.ProviderName).Returns("Feature-Rich Provider");
        mock.Setup(x => x.Version).Returns("1.0.0");
        mock.Setup(x => x.SupportedProtocolVersions).Returns(new List<string>());
        mock.Setup(x => x.PerformanceCharacteristics).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.FeatureFlags).Returns(featureFlags);
        mock.Setup(x => x.SecurityFeatures).Returns(new List<string>());
        mock.Setup(x => x.Limitations).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CustomMetadata).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.FeatureFlags.ShouldBe(featureFlags);
        metadata.FeatureFlags["Transactions"].ShouldBe(true);
        metadata.FeatureFlags["Streaming"].ShouldBe(false);
        metadata.FeatureFlags["Compression"].ShouldBe("gzip");
        metadata.FeatureFlags["CachingLevel"].ShouldBe(3);
    }

    [Fact]
    public void ShouldWorkWithComplexSecurityFeatures()
    {
        // Arrange
        var securityFeatures = new List<string>
        {
            "IntegratedAuth",
            "SSL",
            "TLS 1.3",
            "CertificateAuth",
            "TokenAuth",
            "SAML",
            "OAuth2",
            "MFA",
            "Encryption-at-Rest",
            "Encryption-in-Transit"
        };

        var mock = new Mock<IProviderMetadata>();
        mock.Setup(x => x.ProviderName).Returns("Secure Provider");
        mock.Setup(x => x.Version).Returns("1.0.0");
        mock.Setup(x => x.SupportedProtocolVersions).Returns(new List<string>());
        mock.Setup(x => x.PerformanceCharacteristics).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.FeatureFlags).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.SecurityFeatures).Returns(securityFeatures);
        mock.Setup(x => x.Limitations).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CustomMetadata).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.SecurityFeatures.ShouldBe(securityFeatures);
        metadata.SecurityFeatures.Count.ShouldBe(10);
        metadata.SecurityFeatures.ShouldContain("TLS 1.3");
        metadata.SecurityFeatures.ShouldContain("OAuth2");
        metadata.SecurityFeatures.ShouldContain("Encryption-at-Rest");
    }

    [Fact]
    public void ShouldWorkWithEmptyCollections()
    {
        // Arrange
        var emptyList = new List<string>();
        var emptyDict = new Dictionary<string, object>();

        var mock = new Mock<IProviderMetadata>();
        mock.Setup(x => x.ProviderName).Returns("Minimal Provider");
        mock.Setup(x => x.Version).Returns("1.0.0");
        mock.Setup(x => x.SupportedProtocolVersions).Returns(emptyList);
        mock.Setup(x => x.PerformanceCharacteristics).Returns(emptyDict);
        mock.Setup(x => x.FeatureFlags).Returns(emptyDict);
        mock.Setup(x => x.SecurityFeatures).Returns(emptyList);
        mock.Setup(x => x.Limitations).Returns(emptyDict);
        mock.Setup(x => x.CustomMetadata).Returns(emptyDict);
        mock.Setup(x => x.CollectedAt).Returns(DateTimeOffset.UtcNow);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.SupportedProtocolVersions.Count.ShouldBe(0);
        metadata.PerformanceCharacteristics.Count.ShouldBe(0);
        metadata.FeatureFlags.Count.ShouldBe(0);
        metadata.SecurityFeatures.Count.ShouldBe(0);
        metadata.Limitations.Count.ShouldBe(0);
        metadata.CustomMetadata.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldSupportDateTimeOffsetCollectedAt()
    {
        // Arrange
        var specificTime = new DateTimeOffset(2023, 10, 15, 14, 30, 0, TimeSpan.FromHours(2));
        
        var mock = new Mock<IProviderMetadata>();
        mock.Setup(x => x.ProviderName).Returns("Test Provider");
        mock.Setup(x => x.Version).Returns("1.0.0");
        mock.Setup(x => x.SupportedProtocolVersions).Returns(new List<string>());
        mock.Setup(x => x.PerformanceCharacteristics).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.FeatureFlags).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.SecurityFeatures).Returns(new List<string>());
        mock.Setup(x => x.Limitations).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CustomMetadata).Returns(new Dictionary<string, object>());
        mock.Setup(x => x.CollectedAt).Returns(specificTime);

        // Act
        var metadata = mock.Object;

        // Assert
        metadata.CollectedAt.ShouldBe(specificTime);
        metadata.CollectedAt.Offset.ShouldBe(TimeSpan.FromHours(2));
    }
}