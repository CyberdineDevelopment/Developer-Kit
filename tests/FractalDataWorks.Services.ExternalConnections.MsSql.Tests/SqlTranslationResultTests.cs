using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Services.ExternalConnections.MsSql;
using Microsoft.Data.SqlClient;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Services.ExternalConnections.MsSql.Tests;

/// <summary>
/// Tests for SqlTranslationResult to ensure proper handling of SQL statements and parameters.
/// </summary>
public sealed class SqlTranslationResultTests
{
    private readonly ITestOutputHelper _output;

    public SqlTranslationResultTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    [Fact]
    public void ShouldCreateWithValidSqlAndParameters()
    {
        // Arrange
        var sql = "SELECT * FROM [dbo].[Products] WHERE [Id] = @p0";
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@p0", 123)
        };

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldBe(sql);
        result.Parameters.ShouldNotBeNull();
        result.Parameters.Count.ShouldBe(1);
        result.Parameters[0].ParameterName.ShouldBe("@p0");
        result.Parameters[0].Value.ShouldBe(123);

        _output.WriteLine($"Created SqlTranslationResult with SQL: {sql}");
        _output.WriteLine($"Parameters count: {parameters.Count}");
    }

    [Fact]
    public void ShouldCreateWithEmptyParametersList()
    {
        // Arrange
        var sql = "SELECT COUNT(*) FROM [dbo].[Products]";
        var parameters = new List<SqlParameter>();

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldBe(sql);
        result.Parameters.ShouldNotBeNull();
        result.Parameters.Count.ShouldBe(0);

        _output.WriteLine($"Created SqlTranslationResult with no parameters: {sql}");
    }

    [Fact]
    public void ShouldCreateWithComplexSqlAndMultipleParameters()
    {
        // Arrange
        var sql = @"UPDATE [sales].[Orders] 
                   SET [Status] = @p0, [ModifiedDate] = @p1 
                   WHERE [CustomerId] = @p2 AND [OrderDate] > @p3";
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@p0", "Completed"),
            new SqlParameter("@p1", DateTime.Now),
            new SqlParameter("@p2", 456),
            new SqlParameter("@p3", new DateTime(2023, 1, 1))
        };

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldBe(sql);
        result.Parameters.ShouldNotBeNull();
        result.Parameters.Count.ShouldBe(4);
        
        result.Parameters[0].ParameterName.ShouldBe("@p0");
        result.Parameters[0].Value.ShouldBe("Completed");
        
        result.Parameters[1].ParameterName.ShouldBe("@p1");
        result.Parameters[1].Value.ShouldBeOfType<DateTime>();
        
        result.Parameters[2].ParameterName.ShouldBe("@p2");
        result.Parameters[2].Value.ShouldBe(456);
        
        result.Parameters[3].ParameterName.ShouldBe("@p3");
        result.Parameters[3].Value.ShouldBe(new DateTime(2023, 1, 1));

        _output.WriteLine($"Created complex SqlTranslationResult with {parameters.Count} parameters");
        _output.WriteLine($"SQL length: {sql.Length} characters");
    }

    [Fact]
    public void ShouldThrowWhenSqlIsNull()
    {
        // Arrange
        var parameters = new List<SqlParameter>();

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new SqlTranslationResult(null!, parameters));
        exception.ParamName.ShouldBe("sql");

        _output.WriteLine("Constructor correctly throws ArgumentNullException when SQL is null");
    }

    [Fact]
    public void ShouldThrowWhenParametersIsNull()
    {
        // Arrange
        var sql = "SELECT * FROM [dbo].[Products]";

        // Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new SqlTranslationResult(sql, null!));
        exception.ParamName.ShouldBe("parameters");

        _output.WriteLine("Constructor correctly throws ArgumentNullException when parameters is null");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldCreateWithEmptyOrWhitespaceSql(string sql)
    {
        // Arrange
        var parameters = new List<SqlParameter>();

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldBe(sql);
        result.Parameters.ShouldNotBeNull();
        result.Parameters.Count.ShouldBe(0);

        _output.WriteLine($"Created SqlTranslationResult with empty/whitespace SQL: '{sql}'");
    }

    [Fact]
    public void ShouldHandleParametersWithNullValues()
    {
        // Arrange
        var sql = "INSERT INTO [dbo].[Products] ([Name], [Description]) VALUES (@p0, @p1)";
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@p0", "Product Name"),
            new SqlParameter("@p1", DBNull.Value)
        };

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldBe(sql);
        result.Parameters.Count.ShouldBe(2);
        result.Parameters[0].Value.ShouldBe("Product Name");
        result.Parameters[1].Value.ShouldBe(DBNull.Value);

        _output.WriteLine("Handles parameters with null/DBNull values correctly");
    }

    [Fact]
    public void ShouldHandleParametersWithVariousDataTypes()
    {
        // Arrange
        var sql = "SELECT * FROM [dbo].[TestTable] WHERE [IntCol] = @p0 AND [StringCol] = @p1 AND [DateCol] = @p2 AND [BoolCol] = @p3 AND [DecimalCol] = @p4";
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@p0", 42),
            new SqlParameter("@p1", "test string"),
            new SqlParameter("@p2", new DateTime(2023, 6, 15)),
            new SqlParameter("@p3", true),
            new SqlParameter("@p4", 123.45m)
        };

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Parameters.Count.ShouldBe(5);
        result.Parameters[0].Value.ShouldBe(42);
        result.Parameters[1].Value.ShouldBe("test string");
        result.Parameters[2].Value.ShouldBe(new DateTime(2023, 6, 15));
        result.Parameters[3].Value.ShouldBe(true);
        result.Parameters[4].Value.ShouldBe(123.45m);

        _output.WriteLine($"Handles various data types correctly: int, string, DateTime, bool, decimal");
        foreach (var param in result.Parameters)
        {
            _output.WriteLine($"  {param.ParameterName}: {param.Value} ({param.Value?.GetType().Name ?? "null"})");
        }
    }

    [Fact]
    public void ShouldHandleLongSqlStatements()
    {
        // Arrange
        var sql = new string('A', 10000); // Very long SQL string
        var parameters = new List<SqlParameter>();

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Sql.ShouldBe(sql);
        result.Sql.Length.ShouldBe(10000);

        _output.WriteLine($"Handles long SQL statement of {sql.Length} characters");
    }

    [Fact]
    public void ShouldHandleManyParameters()
    {
        // Arrange
        var sql = "SELECT * FROM [dbo].[Products] WHERE [Id] IN (" + string.Join(", ", Enumerable.Range(0, 100).Select(i => $"@p{i}")) + ")";
        var parameters = new List<SqlParameter>();
        for (int i = 0; i < 100; i++)
        {
            parameters.Add(new SqlParameter($"@p{i}", i));
        }

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Parameters.Count.ShouldBe(100);
        for (int i = 0; i < 100; i++)
        {
            result.Parameters[i].ParameterName.ShouldBe($"@p{i}");
            result.Parameters[i].Value.ShouldBe(i);
        }

        _output.WriteLine($"Handles {parameters.Count} parameters correctly");
    }

    [Fact]
    public void ParametersPropertyShouldBeReadOnly()
    {
        // Arrange
        var sql = "SELECT * FROM [dbo].[Products]";
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@p0", 123)
        };

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.Parameters.ShouldBeAssignableTo<IReadOnlyList<SqlParameter>>();
        // Verify we can't cast to a mutable list
        result.Parameters.ShouldNotBeAssignableTo<List<SqlParameter>>();

        _output.WriteLine("Parameters property is properly read-only");
    }

    [Fact]
    public void ShouldNotModifyOriginalParametersList()
    {
        // Arrange
        var sql = "SELECT * FROM [dbo].[Products] WHERE [Id] = @p0";
        var originalParameters = new List<SqlParameter>
        {
            new SqlParameter("@p0", 123)
        };
        var originalCount = originalParameters.Count;

        // Act
        var result = new SqlTranslationResult(sql, originalParameters);
        originalParameters.Add(new SqlParameter("@p1", 456)); // Modify original list

        // Assert
        result.Parameters.Count.ShouldBe(originalCount); // Should not be affected by modification
        result.Parameters.Count.ShouldBe(1);
        originalParameters.Count.ShouldBe(2);

        _output.WriteLine("Original parameters list modifications don't affect the result");
    }

    [Fact]
    public void ShouldHandleUnicodeStrings()
    {
        // Arrange
        var sql = "SELECT * FROM [dbo].[Products] WHERE [Name] = @p0";
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@p0", "Тест продукт 测试产品 🚀")
        };

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Parameters[0].Value.ShouldBe("Тест продукт 测试产品 🚀");

        _output.WriteLine($"Handles Unicode strings correctly: {result.Parameters[0].Value}");
    }

    [Fact]
    public void ShouldHandleParametersWithSpecialSqlTypes()
    {
        // Arrange
        var sql = "SELECT * FROM [dbo].[Products] WHERE [Data] = @p0";
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@p0", System.Data.SqlDbType.VarBinary) { Value = new byte[] { 1, 2, 3, 4 } }
        };

        // Act
        var result = new SqlTranslationResult(sql, parameters);

        // Assert
        result.ShouldNotBeNull();
        result.Parameters[0].SqlDbType.ShouldBe(System.Data.SqlDbType.VarBinary);
        ((byte[])result.Parameters[0].Value).ShouldBe(new byte[] { 1, 2, 3, 4 });

        _output.WriteLine("Handles special SQL types like VarBinary correctly");
    }
}