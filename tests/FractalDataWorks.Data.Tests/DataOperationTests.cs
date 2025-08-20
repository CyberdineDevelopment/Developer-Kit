using System;
using System.Linq;
using FractalDataWorks.Data;
using Shouldly;
using Xunit;
using Xunit.v3;

namespace FractalDataWorks.Data.Tests;

/// <summary>
/// Tests for DataOperation enum.
/// </summary>
public class DataOperationTests
{
    private readonly ITestOutputHelper _output;

    public DataOperationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DataOperationShouldHaveExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            DataOperation.Query,
            DataOperation.Insert,
            DataOperation.Update,
            DataOperation.Upsert,
            DataOperation.Delete,
            DataOperation.BulkInsert,
            DataOperation.BulkUpdate,
            DataOperation.BulkDelete
        };

        // Act
        var actualValues = Enum.GetValues<DataOperation>();

        // Assert
        actualValues.ShouldNotBeEmpty();
        actualValues.Length.ShouldBe(8);
        expectedValues.ShouldAllBe(value => actualValues.Contains(value));

        _output.WriteLine($"DataOperation values: {string.Join(", ", actualValues)}");
    }

    [Theory]
    [InlineData(DataOperation.Query, 0)]
    [InlineData(DataOperation.Insert, 1)]
    [InlineData(DataOperation.Update, 2)]
    [InlineData(DataOperation.Upsert, 3)]
    [InlineData(DataOperation.Delete, 4)]
    [InlineData(DataOperation.BulkInsert, 5)]
    [InlineData(DataOperation.BulkUpdate, 6)]
    [InlineData(DataOperation.BulkDelete, 7)]
    public void DataOperationShouldHaveExpectedIntegerValues(DataOperation operation, int expectedValue)
    {
        // Act & Assert
        ((int)operation).ShouldBe(expectedValue);
        _output.WriteLine($"{operation} = {(int)operation}");
    }

    [Theory]
    [InlineData(DataOperation.Query, "Query")]
    [InlineData(DataOperation.Insert, "Insert")]
    [InlineData(DataOperation.Update, "Update")]
    [InlineData(DataOperation.Upsert, "Upsert")]
    [InlineData(DataOperation.Delete, "Delete")]
    [InlineData(DataOperation.BulkInsert, "BulkInsert")]
    [InlineData(DataOperation.BulkUpdate, "BulkUpdate")]
    [InlineData(DataOperation.BulkDelete, "BulkDelete")]
    public void DataOperationShouldHaveExpectedStringRepresentation(DataOperation operation, string expectedString)
    {
        // Act & Assert
        operation.ToString().ShouldBe(expectedString);
        _output.WriteLine($"{operation}.ToString() = {operation}");
    }

    [Fact]
    public void DataOperationShouldBeValidEnumWhenParsedFromString()
    {
        // Arrange
        var validStrings = new[] { "Query", "Insert", "Update", "Upsert", "Delete", "BulkInsert", "BulkUpdate", "BulkDelete" };

        // Act & Assert
        foreach (var validString in validStrings)
        {
            var parsed = Enum.Parse<DataOperation>(validString);
            Enum.IsDefined(typeof(DataOperation), parsed).ShouldBeTrue();
            _output.WriteLine($"Successfully parsed '{validString}' to {parsed}");
        }
    }

    [Fact]
    public void DataOperationShouldThrowWhenParsingInvalidString()
    {
        // Arrange
        var invalidString = "InvalidOperation";

        // Act & Assert
        Should.Throw<ArgumentException>(() => Enum.Parse<DataOperation>(invalidString));
        _output.WriteLine($"Correctly threw exception when parsing invalid string: {invalidString}");
    }

    [Fact]
    public void DataOperationShouldSupportCaseInsensitiveParsing()
    {
        // Arrange
        var lowerCaseString = "query";
        var mixedCaseString = "BulkInsert";

        // Act & Assert
        var parsedLower = Enum.Parse<DataOperation>(lowerCaseString, true);
        var parsedMixed = Enum.Parse<DataOperation>(mixedCaseString, true);

        parsedLower.ShouldBe(DataOperation.Query);
        parsedMixed.ShouldBe(DataOperation.BulkInsert);

        _output.WriteLine($"Case insensitive parsing: '{lowerCaseString}' -> {parsedLower}, '{mixedCaseString}' -> {parsedMixed}");
    }

    [Theory]
    [InlineData(DataOperation.BulkInsert)]
    [InlineData(DataOperation.BulkUpdate)]
    [InlineData(DataOperation.BulkDelete)]
    public void BulkOperationsShouldBeIdentifiable(DataOperation operation)
    {
        // Act & Assert
        operation.ToString().ShouldStartWith("Bulk");
        _output.WriteLine($"{operation} is identified as a bulk operation");
    }

    [Theory]
    [InlineData(DataOperation.Query)]
    [InlineData(DataOperation.Insert)]
    [InlineData(DataOperation.Update)]
    [InlineData(DataOperation.Upsert)]
    [InlineData(DataOperation.Delete)]
    public void NonBulkOperationsShouldNotStartWithBulk(DataOperation operation)
    {
        // Act & Assert
        operation.ToString().ShouldNotStartWith("Bulk");
        _output.WriteLine($"{operation} is identified as a non-bulk operation");
    }
}