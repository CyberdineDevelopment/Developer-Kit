using System;
using System.Linq;
using Shouldly;
using Xunit;
using Xunit.v3;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Configuration.Abstractions.Tests;

/// <summary>
/// Tests for ConfigurationSourceType enum.
/// </summary>
public class ConfigurationSourceTypeTests
{
    private readonly ITestOutputHelper _output;

    public ConfigurationSourceTypeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConfigurationSourceTypeShouldHaveExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            ConfigurationSourceType.File,
            ConfigurationSourceType.Environment,
            ConfigurationSourceType.Database,
            ConfigurationSourceType.Remote,
            ConfigurationSourceType.Memory,
            ConfigurationSourceType.CommandLine,
            ConfigurationSourceType.Custom
        };

        // Act
        var actualValues = Enum.GetValues<ConfigurationSourceType>();

        // Assert
        actualValues.ShouldNotBeEmpty();
        actualValues.Length.ShouldBe(7);
        expectedValues.ShouldAllBe(value => actualValues.Contains(value));
        
        _output.WriteLine($"ConfigurationSourceType values: {string.Join(", ", actualValues)}");
    }

    [Theory]
    [InlineData(ConfigurationSourceType.File, 0)]
    [InlineData(ConfigurationSourceType.Environment, 1)]
    [InlineData(ConfigurationSourceType.Database, 2)]
    [InlineData(ConfigurationSourceType.Remote, 3)]
    [InlineData(ConfigurationSourceType.Memory, 4)]
    [InlineData(ConfigurationSourceType.CommandLine, 5)]
    [InlineData(ConfigurationSourceType.Custom, 6)]
    public void ConfigurationSourceTypeShouldHaveExpectedIntegerValues(ConfigurationSourceType sourceType, int expectedValue)
    {
        // Act & Assert
        ((int)sourceType).ShouldBe(expectedValue);
        _output.WriteLine($"{sourceType} = {(int)sourceType}");
    }

    [Theory]
    [InlineData(ConfigurationSourceType.File, "File")]
    [InlineData(ConfigurationSourceType.Environment, "Environment")]
    [InlineData(ConfigurationSourceType.Database, "Database")]
    [InlineData(ConfigurationSourceType.Remote, "Remote")]
    [InlineData(ConfigurationSourceType.Memory, "Memory")]
    [InlineData(ConfigurationSourceType.CommandLine, "CommandLine")]
    [InlineData(ConfigurationSourceType.Custom, "Custom")]
    public void ConfigurationSourceTypeShouldHaveExpectedStringRepresentation(ConfigurationSourceType sourceType, string expectedString)
    {
        // Act & Assert
        sourceType.ToString().ShouldBe(expectedString);
        _output.WriteLine($"{sourceType}.ToString() = {sourceType}");
    }

    [Fact]
    public void ConfigurationSourceTypeShouldBeValidEnumWhenParsedFromString()
    {
        // Arrange
        var validStrings = new[] { "File", "Environment", "Database", "Remote", "Memory", "CommandLine", "Custom" };

        // Act & Assert
        foreach (var validString in validStrings)
        {
            var parsed = Enum.Parse<ConfigurationSourceType>(validString);
            Enum.IsDefined(typeof(ConfigurationSourceType), parsed).ShouldBeTrue();
            _output.WriteLine($"Successfully parsed '{validString}' to {parsed}");
        }
    }

    [Fact]
    public void ConfigurationSourceTypeShouldThrowWhenParsingInvalidString()
    {
        // Arrange
        var invalidString = "InvalidSourceType";

        // Act & Assert
        Should.Throw<ArgumentException>(() => Enum.Parse<ConfigurationSourceType>(invalidString));
        _output.WriteLine($"Correctly threw exception when parsing invalid string: {invalidString}");
    }

    [Fact]
    public void ConfigurationSourceTypeShouldSupportCaseInsensitiveParsing()
    {
        // Arrange
        var lowerCaseString = "file";
        var mixedCaseString = "DataBase";

        // Act & Assert
        var parsedLower = Enum.Parse<ConfigurationSourceType>(lowerCaseString, true);
        var parsedMixed = Enum.Parse<ConfigurationSourceType>(mixedCaseString, true);

        parsedLower.ShouldBe(ConfigurationSourceType.File);
        parsedMixed.ShouldBe(ConfigurationSourceType.Database);
        
        _output.WriteLine($"Case insensitive parsing: '{lowerCaseString}' -> {parsedLower}, '{mixedCaseString}' -> {parsedMixed}");
    }
}