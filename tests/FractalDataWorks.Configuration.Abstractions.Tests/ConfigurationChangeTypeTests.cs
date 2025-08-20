using System;
using System.Linq;
using Shouldly;
using Xunit;
using Xunit.v3;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Configuration.Abstractions.Tests;

/// <summary>
/// Tests for ConfigurationChangeType enum.
/// </summary>
public class ConfigurationChangeTypeTests
{
    private readonly ITestOutputHelper _output;

    public ConfigurationChangeTypeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConfigurationChangeTypeShouldHaveExpectedValues()
    {
        // Arrange
        var expectedValues = new[]
        {
            ConfigurationChangeType.Added,
            ConfigurationChangeType.Updated,
            ConfigurationChangeType.Deleted,
            ConfigurationChangeType.Reloaded
        };

        // Act
        var actualValues = Enum.GetValues<ConfigurationChangeType>();

        // Assert
        actualValues.ShouldNotBeEmpty();
        actualValues.Length.ShouldBe(4);
        expectedValues.ShouldAllBe(value => actualValues.Contains(value));
        
        _output.WriteLine($"ConfigurationChangeType values: {string.Join(", ", actualValues)}");
    }

    [Theory]
    [InlineData(ConfigurationChangeType.Added, 0)]
    [InlineData(ConfigurationChangeType.Updated, 1)]
    [InlineData(ConfigurationChangeType.Deleted, 2)]
    [InlineData(ConfigurationChangeType.Reloaded, 3)]
    public void ConfigurationChangeTypeShouldHaveExpectedIntegerValues(ConfigurationChangeType changeType, int expectedValue)
    {
        // Act & Assert
        ((int)changeType).ShouldBe(expectedValue);
        _output.WriteLine($"{changeType} = {(int)changeType}");
    }

    [Theory]
    [InlineData(ConfigurationChangeType.Added, "Added")]
    [InlineData(ConfigurationChangeType.Updated, "Updated")]
    [InlineData(ConfigurationChangeType.Deleted, "Deleted")]
    [InlineData(ConfigurationChangeType.Reloaded, "Reloaded")]
    public void ConfigurationChangeTypeShouldHaveExpectedStringRepresentation(ConfigurationChangeType changeType, string expectedString)
    {
        // Act & Assert
        changeType.ToString().ShouldBe(expectedString);
        _output.WriteLine($"{changeType}.ToString() = {changeType}");
    }

    [Fact]
    public void ConfigurationChangeTypeShouldBeValidEnumWhenParsedFromString()
    {
        // Arrange
        var validStrings = new[] { "Added", "Updated", "Deleted", "Reloaded" };

        // Act & Assert
        foreach (var validString in validStrings)
        {
            var parsed = Enum.Parse<ConfigurationChangeType>(validString);
            Enum.IsDefined(typeof(ConfigurationChangeType), parsed).ShouldBeTrue();
            _output.WriteLine($"Successfully parsed '{validString}' to {parsed}");
        }
    }

    [Fact]
    public void ConfigurationChangeTypeShouldThrowWhenParsingInvalidString()
    {
        // Arrange
        var invalidString = "InvalidChangeType";

        // Act & Assert
        Should.Throw<ArgumentException>(() => Enum.Parse<ConfigurationChangeType>(invalidString));
        _output.WriteLine($"Correctly threw exception when parsing invalid string: {invalidString}");
    }
}