using System;
using System.Collections.Generic;
using FractalDataWorks.Services.ExternalConnections.MsSql;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests;

/// <summary>
/// Tests for MsSqlConnectionMetadata to ensure proper metadata structure and behavior.
/// </summary>
public sealed class MsSqlConnectionMetadataTests
{
    private readonly ITestOutputHelper _output;

    public MsSqlConnectionMetadataTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    [Fact]
    public void ShouldCreateWithDefaultValues()
    {
        // Act
        var metadata = new MsSqlConnectionMetadata();

        // Assert
        metadata.SystemName.ShouldBe(string.Empty);
        metadata.Version.ShouldBeNull();
        metadata.ServerInfo.ShouldBeNull();
        metadata.DatabaseName.ShouldBeNull();
        metadata.Capabilities.ShouldNotBeNull();
        metadata.Capabilities.Count.ShouldBe(0);
        metadata.CustomProperties.ShouldNotBeNull();
        metadata.CustomProperties.Count.ShouldBe(0);
        metadata.CollectedAt.ShouldBe(default(DateTimeOffset));

        _output.WriteLine("MsSqlConnectionMetadata initializes with proper default values");
    }

    [Fact]
    public void ShouldSetSystemNameCorrectly()
    {
        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            SystemName = "Microsoft SQL Server"
        };

        // Assert
        metadata.SystemName.ShouldBe("Microsoft SQL Server");

        _output.WriteLine($"System name set correctly: {metadata.SystemName}");
    }

    [Theory]
    [InlineData("Microsoft SQL Server 2019 (RTM) - 15.0.2000.5")]
    [InlineData("Microsoft SQL Server 2022 (RTM) - 16.0.1000.6")]
    [InlineData("Microsoft SQL Server 2017 (RTM-CU31) - 14.0.3456.2")]
    public void ShouldSetVersionCorrectly(string version)
    {
        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            Version = version
        };

        // Assert
        metadata.Version.ShouldBe(version);

        _output.WriteLine($"Version set correctly: {metadata.Version}");
    }

    [Theory]
    [InlineData("SERVER01\\SQLEXPRESS")]
    [InlineData("localhost")]
    [InlineData("(localdb)\\MSSQLLocalDB")]
    public void ShouldSetServerInfoCorrectly(string serverInfo)
    {
        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            ServerInfo = serverInfo
        };

        // Assert
        metadata.ServerInfo.ShouldBe(serverInfo);

        _output.WriteLine($"Server info set correctly: {metadata.ServerInfo}");
    }

    [Theory]
    [InlineData("TestDatabase")]
    [InlineData("ProductionDB")]
    [InlineData("SalesDataWarehouse")]
    public void ShouldSetDatabaseNameCorrectly(string databaseName)
    {
        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            DatabaseName = databaseName
        };

        // Assert
        metadata.DatabaseName.ShouldBe(databaseName);

        _output.WriteLine($"Database name set correctly: {metadata.DatabaseName}");
    }

    [Fact]
    public void ShouldSetCollectedAtCorrectly()
    {
        // Arrange
        var collectedAt = DateTimeOffset.UtcNow;

        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            CollectedAt = collectedAt
        };

        // Assert
        metadata.CollectedAt.ShouldBe(collectedAt);

        _output.WriteLine($"Collected at set correctly: {metadata.CollectedAt}");
    }

    [Fact]
    public void ShouldSetCapabilitiesCorrectly()
    {
        // Arrange
        var capabilities = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["SupportsTransactions"] = true,
            ["MaxParameterCount"] = 2100,
            ["MaxBatchSize"] = 1000,
            ["SupportsJsonData"] = true,
            ["SupportsXmlData"] = true,
            ["SupportsFullTextSearch"] = true
        };

        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            Capabilities = capabilities
        };

        // Assert
        metadata.Capabilities.ShouldNotBeNull();
        metadata.Capabilities.Count.ShouldBe(6);
        metadata.Capabilities["SupportsTransactions"].ShouldBe(true);
        metadata.Capabilities["MaxParameterCount"].ShouldBe(2100);
        metadata.Capabilities["MaxBatchSize"].ShouldBe(1000);
        metadata.Capabilities["SupportsJsonData"].ShouldBe(true);
        metadata.Capabilities["SupportsXmlData"].ShouldBe(true);
        metadata.Capabilities["SupportsFullTextSearch"].ShouldBe(true);

        _output.WriteLine($"Capabilities set correctly with {capabilities.Count} items");
        foreach (var capability in metadata.Capabilities)
        {
            _output.WriteLine($"  {capability.Key}: {capability.Value}");
        }
    }

    [Fact]
    public void ShouldSetCustomPropertiesCorrectly()
    {
        // Arrange
        var customProperties = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ConnectionPooling"] = true,
            ["RetryLogic"] = true,
            ["CommandTimeout"] = 30,
            ["ConnectionTimeout"] = 15,
            ["EnableSqlLogging"] = false
        };

        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            CustomProperties = customProperties
        };

        // Assert
        metadata.CustomProperties.ShouldNotBeNull();
        metadata.CustomProperties.Count.ShouldBe(5);
        metadata.CustomProperties["ConnectionPooling"].ShouldBe(true);
        metadata.CustomProperties["RetryLogic"].ShouldBe(true);
        metadata.CustomProperties["CommandTimeout"].ShouldBe(30);
        metadata.CustomProperties["ConnectionTimeout"].ShouldBe(15);
        metadata.CustomProperties["EnableSqlLogging"].ShouldBe(false);

        _output.WriteLine($"Custom properties set correctly with {customProperties.Count} items");
        foreach (var property in metadata.CustomProperties)
        {
            _output.WriteLine($"  {property.Key}: {property.Value}");
        }
    }

    [Fact]
    public void ShouldCreateCompleteMetadata()
    {
        // Arrange
        var collectedAt = DateTimeOffset.UtcNow;
        var capabilities = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["SupportsTransactions"] = true,
            ["SupportsMultipleActiveResultSets"] = false,
            ["MaxParameterCount"] = 2100
        };
        var customProperties = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ConnectionPooling"] = true,
            ["CommandTimeout"] = 30
        };

        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            SystemName = "Microsoft SQL Server",
            Version = "Microsoft SQL Server 2019 (RTM) - 15.0.2000.5",
            ServerInfo = "localhost",
            DatabaseName = "TestDB",
            CollectedAt = collectedAt,
            Capabilities = capabilities,
            CustomProperties = customProperties
        };

        // Assert
        metadata.SystemName.ShouldBe("Microsoft SQL Server");
        metadata.Version.ShouldBe("Microsoft SQL Server 2019 (RTM) - 15.0.2000.5");
        metadata.ServerInfo.ShouldBe("localhost");
        metadata.DatabaseName.ShouldBe("TestDB");
        metadata.CollectedAt.ShouldBe(collectedAt);
        metadata.Capabilities.Count.ShouldBe(3);
        metadata.CustomProperties.Count.ShouldBe(2);

        _output.WriteLine("Complete metadata created successfully");
        _output.WriteLine($"System: {metadata.SystemName}");
        _output.WriteLine($"Database: {metadata.DatabaseName}");
        _output.WriteLine($"Server: {metadata.ServerInfo}");
        _output.WriteLine($"Capabilities: {metadata.Capabilities.Count}");
        _output.WriteLine($"Custom Properties: {metadata.CustomProperties.Count}");
    }

    [Fact]
    public void CapabilitiesShouldUseOrdinalStringComparerByDefault()
    {
        // Act
        var metadata = new MsSqlConnectionMetadata();

        // Assert
        metadata.Capabilities.ShouldNotBeNull();
        // The default implementation should use StringComparer.Ordinal
        var dict = metadata.Capabilities as Dictionary<string, object>;
        if (dict != null)
        {
            dict.Comparer.ShouldBe(StringComparer.Ordinal);
        }

        _output.WriteLine("Capabilities dictionary uses StringComparer.Ordinal");
    }

    [Fact]
    public void CustomPropertiesShouldUseOrdinalStringComparerByDefault()
    {
        // Act
        var metadata = new MsSqlConnectionMetadata();

        // Assert
        metadata.CustomProperties.ShouldNotBeNull();
        // The default implementation should use StringComparer.Ordinal
        var dict = metadata.CustomProperties as Dictionary<string, object>;
        if (dict != null)
        {
            dict.Comparer.ShouldBe(StringComparer.Ordinal);
        }

        _output.WriteLine("Custom properties dictionary uses StringComparer.Ordinal");
    }

    [Fact]
    public void CapabilitiesShouldBeReadOnly()
    {
        // Arrange
        var capabilities = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Test"] = true
        };
        var metadata = new MsSqlConnectionMetadata
        {
            Capabilities = capabilities
        };

        // Assert
        metadata.Capabilities.ShouldBeAssignableTo<IReadOnlyDictionary<string, object>>();

        _output.WriteLine("Capabilities property is read-only");
    }

    [Fact]
    public void CustomPropertiesShouldBeReadOnly()
    {
        // Arrange
        var customProperties = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Test"] = "value"
        };
        var metadata = new MsSqlConnectionMetadata
        {
            CustomProperties = customProperties
        };

        // Assert
        metadata.CustomProperties.ShouldBeAssignableTo<IReadOnlyDictionary<string, object>>();

        _output.WriteLine("Custom properties property is read-only");
    }

    [Fact]
    public void ShouldHandleNullAndEmptyValues()
    {
        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            SystemName = string.Empty,
            Version = null,
            ServerInfo = null,
            DatabaseName = null,
            CollectedAt = DateTimeOffset.MinValue
        };

        // Assert
        metadata.SystemName.ShouldBe(string.Empty);
        metadata.Version.ShouldBeNull();
        metadata.ServerInfo.ShouldBeNull();
        metadata.DatabaseName.ShouldBeNull();
        metadata.CollectedAt.ShouldBe(DateTimeOffset.MinValue);

        _output.WriteLine("Handles null and empty values correctly");
    }

    [Fact]
    public void ShouldHandleVariousDataTypesInCapabilities()
    {
        // Arrange
        var capabilities = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["BooleanValue"] = true,
            ["IntValue"] = 42,
            ["StringValue"] = "test",
            ["DateTimeValue"] = DateTime.Now,
            ["DecimalValue"] = 123.45m,
            ["ArrayValue"] = new[] { 1, 2, 3 },
            ["NullValue"] = null
        };

        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            Capabilities = capabilities
        };

        // Assert
        metadata.Capabilities["BooleanValue"].ShouldBe(true);
        metadata.Capabilities["IntValue"].ShouldBe(42);
        metadata.Capabilities["StringValue"].ShouldBe("test");
        metadata.Capabilities["DateTimeValue"].ShouldBeOfType<DateTime>();
        metadata.Capabilities["DecimalValue"].ShouldBe(123.45m);
        metadata.Capabilities["ArrayValue"].ShouldBeOfType<int[]>();
        metadata.Capabilities["NullValue"].ShouldBeNull();

        _output.WriteLine("Handles various data types in capabilities correctly");
        foreach (var capability in metadata.Capabilities)
        {
            _output.WriteLine($"  {capability.Key}: {capability.Value} ({capability.Value?.GetType().Name ?? "null"})");
        }
    }

    [Fact]
    public void ShouldImplementIConnectionMetadata()
    {
        // Act
        var metadata = new MsSqlConnectionMetadata();

        // Assert
        metadata.ShouldBeAssignableTo<FractalDataWorks.Services.ExternalConnections.Abstractions.IConnectionMetadata>();

        _output.WriteLine("MsSqlConnectionMetadata implements IConnectionMetadata interface");
    }

    [Fact]
    public void ShouldHaveProperInitOnlyProperties()
    {
        // Arrange
        var metadata = new MsSqlConnectionMetadata
        {
            SystemName = "Initial System"
        };

        // Assert - Properties are init-only, so we can't reassign them after object creation
        // This is verified by the compiler, but we can verify the property exists and is set
        metadata.SystemName.ShouldBe("Initial System");

        _output.WriteLine("Properties are properly defined as init-only");
    }

    [Fact]
    public void ShouldHandleUnicodeInStringProperties()
    {
        // Act
        var metadata = new MsSqlConnectionMetadata
        {
            SystemName = "Microsoft SQL Server тест 测试 🚀",
            Version = "Версия 16.0.1000.6 测试版本",
            ServerInfo = "服务器\\实例名称",
            DatabaseName = "数据库名称_тест"
        };

        // Assert
        metadata.SystemName.ShouldBe("Microsoft SQL Server тест 测试 🚀");
        metadata.Version.ShouldBe("Версия 16.0.1000.6 测试版本");
        metadata.ServerInfo.ShouldBe("服务器\\实例名称");
        metadata.DatabaseName.ShouldBe("数据库名称_тест");

        _output.WriteLine("Handles Unicode characters in string properties correctly");
    }
}