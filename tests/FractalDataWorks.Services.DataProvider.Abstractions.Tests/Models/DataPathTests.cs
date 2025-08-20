using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Shouldly;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Tests.Models;

public class DataPathTests
{
    private readonly ITestOutputHelper _output;

    public DataPathTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstructorShouldInitializeWithValidSegments()
    {
        // Arrange
        var segments = new[] { "database", "schema", "table" };
        var separator = ".";

        // Act
        var path = new DataPath(segments, separator);

        // Assert
        path.Segments.ShouldBe(segments);
        path.Separator.ShouldBe(separator);
        path.Segments.Count.ShouldBe(3);
        path.Segments[0].ShouldBe("database");
        path.Segments[1].ShouldBe("schema");
        path.Segments[2].ShouldBe("table");
    }

    [Fact]
    public void ConstructorShouldUseDefaultSeparatorWhenNotProvided()
    {
        // Arrange
        var segments = new[] { "api", "v1", "users" };

        // Act
        var path = new DataPath(segments);

        // Assert
        path.Separator.ShouldBe("/");
        path.ToString().ShouldBe("api/v1/users");
    }

    [Fact]
    public void ConstructorShouldThrowWhenSegmentsIsNull()
    {
        // Arrange & Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new DataPath(null!));
        exception.ParamName.ShouldBe("segments");
    }

    [Fact]
    public void ConstructorShouldThrowWhenSegmentsIsEmpty()
    {
        // Arrange
        var emptySegments = Array.Empty<string>();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => new DataPath(emptySegments));
        exception.ParamName.ShouldBe("segments");
        exception.Message.ShouldContain("Path must contain at least one segment");
    }

    [Theory]
    [InlineData(new[] { "valid", "" }, 1)]
    [InlineData(new[] { "", "valid" }, 0)]
    [InlineData(new[] { "valid", "   ", "another" }, 1)]
    public void ConstructorShouldThrowWhenSegmentIsNullOrEmpty(string[] segments, int expectedIndex)
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => new DataPath(segments));
        exception.ParamName.ShouldBe("segments");
        exception.Message.ShouldContain($"Segment at index {expectedIndex} cannot be null or empty");
    }

    [Fact]
    public void ConstructorShouldThrowWhenSeparatorIsNull()
    {
        // Arrange
        var segments = new[] { "valid", "segment" };

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new DataPath(segments, null!));
        exception.ParamName.ShouldBe("separator");
    }

    [Fact]
    public void ToStringShouldJoinSegmentsWithSeparator()
    {
        // Arrange
        var segments = new[] { "data", "customers", "archive" };
        var separator = "\\";
        var path = new DataPath(segments, separator);

        // Act
        var result = path.ToString();

        // Assert
        result.ShouldBe("data\\customers\\archive");
        _output.WriteLine($"Path string: {result}");
    }

    [Fact]
    public void ToStringShouldHandleSingleSegment()
    {
        // Arrange
        var segments = new[] { "users" };
        var path = new DataPath(segments, ":");

        // Act
        var result = path.ToString();

        // Assert
        result.ShouldBe("users");
    }

    [Fact]
    public void EqualsShouldReturnTrueForIdenticalPaths()
    {
        // Arrange
        var segments = new[] { "db", "schema", "table" };
        var path1 = new DataPath(segments, ".");
        var path2 = new DataPath(segments, ".");

        // Act & Assert
        path1.Equals(path2).ShouldBeTrue();
        (path1 == path2).ShouldBeTrue();
        (path1 != path2).ShouldBeFalse();
    }

    [Fact]
    public void EqualsShouldReturnFalseForDifferentSegments()
    {
        // Arrange
        var path1 = new DataPath(new[] { "db", "schema", "table" }, ".");
        var path2 = new DataPath(new[] { "db", "schema", "view" }, ".");

        // Act & Assert
        path1.Equals(path2).ShouldBeFalse();
        (path1 == path2).ShouldBeFalse();
        (path1 != path2).ShouldBeTrue();
    }

    [Fact]
    public void EqualsShouldReturnTrueForSameSegmentsDifferentSeparators()
    {
        // Arrange - DataPath equality is based on segments only, not separator
        var segments = new[] { "db", "table" };
        var path1 = new DataPath(segments, ".");
        var path2 = new DataPath(segments, "/");

        // Act & Assert
        path1.Equals(path2).ShouldBeTrue();
        (path1 == path2).ShouldBeTrue();
        (path1 != path2).ShouldBeFalse();
        
        // But string representation should be different
        path1.ToString().ShouldBe("db.table");
        path2.ToString().ShouldBe("db/table");
    }

    [Fact]
    public void EqualsShouldReturnFalseWhenComparingWithNull()
    {
        // Arrange
        var path = new DataPath(new[] { "test" });

        // Act & Assert
        path.Equals(null).ShouldBeFalse();
        (path == null).ShouldBeFalse();
        (null == path).ShouldBeFalse();
        (path != null).ShouldBeTrue();
    }

    [Fact]
    public void EqualsShouldReturnFalseWhenComparingWithObject()
    {
        // Arrange
        var path = new DataPath(new[] { "test" });
        var obj = new object();

        // Act & Assert
        path.Equals(obj).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCodeShouldBeConsistentForEqualPaths()
    {
        // Arrange
        var segments = new[] { "db", "schema", "table" };
        var path1 = new DataPath(segments, ".");
        var path2 = new DataPath(segments, ".");

        // Act
        var hash1 = path1.GetHashCode();
        var hash2 = path2.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void GetHashCodeShouldBeDifferentForDifferentPaths()
    {
        // Arrange
        var path1 = new DataPath(new[] { "db", "schema", "table" }, ".");
        var path2 = new DataPath(new[] { "db", "schema", "view" }, ".");

        // Act
        var hash1 = path1.GetHashCode();
        var hash2 = path2.GetHashCode();

        // Assert
        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void ShouldWorkInHashBasedCollections()
    {
        // Arrange
        var path1 = new DataPath(new[] { "db", "users" });
        var path2 = new DataPath(new[] { "db", "products" });
        var path3 = new DataPath(new[] { "db", "users" }); // Same as path1

        var hashSet = new HashSet<DataPath> { path1, path2 };

        // Act & Assert
        hashSet.Count.ShouldBe(2);
        hashSet.Contains(path1).ShouldBeTrue();
        hashSet.Contains(path2).ShouldBeTrue();
        hashSet.Contains(path3).ShouldBeTrue(); // Should find path1
        hashSet.Add(path3).ShouldBeFalse(); // Should not add duplicate

        _output.WriteLine($"HashSet contains {hashSet.Count} unique paths");
    }

    [Fact]
    public void ShouldSupportVariousPathStyles()
    {
        // Arrange & Act
        var sqlPath = new DataPath(new[] { "sales", "customers" }, ".");
        var filePath = new DataPath(new[] { "data", "customers", "archive" }, "\\");
        var apiPath = new DataPath(new[] { "api", "v1", "customers" }, "/");
        var sftpPath = new DataPath(new[] { "uploads", "daily" }, "/");

        // Assert
        sqlPath.ToString().ShouldBe("sales.customers");
        filePath.ToString().ShouldBe("data\\customers\\archive");
        apiPath.ToString().ShouldBe("api/v1/customers");
        sftpPath.ToString().ShouldBe("uploads/daily");

        _output.WriteLine($"SQL style: {sqlPath}");
        _output.WriteLine($"File style: {filePath}");
        _output.WriteLine($"API style: {apiPath}");
        _output.WriteLine($"SFTP style: {sftpPath}");
    }

    [Theory]
    [InlineData(":", "api:v1:users")]
    [InlineData("|", "db|schema|table")]
    [InlineData(" -> ", "step1 -> step2 -> step3")]
    public void ShouldSupportCustomSeparators(string separator, string expectedOutput)
    {
        // Arrange
        var segments = expectedOutput.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim())
                                   .ToArray();

        // Act
        var path = new DataPath(segments, separator);
        var result = path.ToString();

        // Assert
        result.ShouldBe(expectedOutput);
    }

    [Fact]
    public void SegmentsShouldBeReadOnly()
    {
        // Arrange
        var segments = new[] { "test", "path" };
        var path = new DataPath(segments);

        // Act
        var pathSegments = path.Segments;

        // Assert
        pathSegments.ShouldBeOfType<string[]>();
        pathSegments.Count.ShouldBe(2);
        
        // Modifying original array should not affect path
        segments[0] = "modified";
        path.Segments[0].ShouldBe("test"); // Should remain unchanged
    }

    [Fact]
    public void ShouldHandleComplexRealWorldScenarios()
    {
        // Arrange - various real-world path scenarios
        var databasePath = new DataPath(new[] { "ProductionDB", "Sales", "CustomerOrders" }, ".");
        var restApiPath = new DataPath(new[] { "api", "v2", "customers", "{id}", "orders" }, "/");
        var filePath = new DataPath(new[] { "C:", "Data", "Exports", "Daily", "customers.csv" }, "\\");
        var messagingPath = new DataPath(new[] { "orders", "processing", "high-priority" }, ".");

        // Act & Assert
        databasePath.ToString().ShouldBe("ProductionDB.Sales.CustomerOrders");
        restApiPath.ToString().ShouldBe("api/v2/customers/{id}/orders");
        filePath.ToString().ShouldBe("C:\\Data\\Exports\\Daily\\customers.csv");
        messagingPath.ToString().ShouldBe("orders.processing.high-priority");

        _output.WriteLine($"Database path: {databasePath}");
        _output.WriteLine($"REST API path: {restApiPath}");
        _output.WriteLine($"File path: {filePath}");
        _output.WriteLine($"Messaging path: {messagingPath}");
    }
}