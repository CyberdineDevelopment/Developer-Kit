using System;
using System.Linq;
using Xunit;

namespace FractalDataWorks.Messages.Tests;

public class MessageSeverityTests
{

    [Fact]
    public void MessageSeverityShouldHaveExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<MessageSeverity>();

        // Assert
        values.Length.ShouldBe(4);
    }

    [Theory]
    [InlineData(MessageSeverity.Information, 0)]
    [InlineData(MessageSeverity.Warning, 1)]
    [InlineData(MessageSeverity.Error, 2)]
    [InlineData(MessageSeverity.Critical, 3)]
    public void MessageSeverityShouldHaveExpectedIntegerValues(MessageSeverity severity, int expectedValue)
    {
        // Act
        var actualValue = (int)severity;

        // Assert
        actualValue.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(MessageSeverity.Information, "Information")]
    [InlineData(MessageSeverity.Warning, "Warning")]
    [InlineData(MessageSeverity.Error, "Error")]
    [InlineData(MessageSeverity.Critical, "Critical")]
    public void MessageSeverityShouldHaveExpectedStringRepresentation(MessageSeverity severity, string expectedString)
    {
        // Act
        var actualString = severity.ToString();
        // Output($"MessageSeverity.{severity}.ToString() = '{actualString}'");

        // Assert
        actualString.ShouldBe(expectedString);
    }

    [Fact]
    public void MessageSeverityValuesShouldBeInAscendingOrder()
    {
        // Arrange
        var values = Enum.GetValues<MessageSeverity>().Cast<int>().ToArray();
        // Output($"MessageSeverity integer values: [{string.Join(", ", values)}]");

        // Act & Assert
        for (int i = 1; i < values.Length; i++)
        {
            values[i].ShouldBeGreaterThan(values[i - 1]);
        }
    }

    [Fact]
    public void MessageSeverityInformationShouldBeDefaultValue()
    {
        // Act
        var defaultValue = default(MessageSeverity);
        // Output($"default(MessageSeverity) = {defaultValue}");

        // Assert
        defaultValue.ShouldBe(MessageSeverity.Information);
    }

    [Theory]
    [InlineData(0, MessageSeverity.Information)]
    [InlineData(1, MessageSeverity.Warning)]
    [InlineData(2, MessageSeverity.Error)]
    [InlineData(3, MessageSeverity.Critical)]
    public void MessageSeverityShouldCastFromInteger(int value, MessageSeverity expectedSeverity)
    {
        // Act
        var actualSeverity = (MessageSeverity)value;
        // Output($"(MessageSeverity){value} = {actualSeverity}");

        // Assert
        actualSeverity.ShouldBe(expectedSeverity);
    }

    [Theory]
    [InlineData("Information", MessageSeverity.Information)]
    [InlineData("Warning", MessageSeverity.Warning)]
    [InlineData("Error", MessageSeverity.Error)]
    [InlineData("Critical", MessageSeverity.Critical)]
    public void MessageSeverityShouldParseFromString(string value, MessageSeverity expectedSeverity)
    {
        // Act
        var parseResult = Enum.Parse<MessageSeverity>(value);
        // Output($"Enum.Parse<MessageSeverity>(\"{value}\") = {parseResult}");

        // Assert
        parseResult.ShouldBe(expectedSeverity);
    }

    [Theory]
    [InlineData("information", MessageSeverity.Information)]
    [InlineData("WARNING", MessageSeverity.Warning)]
    [InlineData("eRrOr", MessageSeverity.Error)]
    [InlineData("CRITICAL", MessageSeverity.Critical)]
    public void MessageSeverityShouldParseFromStringIgnoreCase(string value, MessageSeverity expectedSeverity)
    {
        // Act
        var parseResult = Enum.Parse<MessageSeverity>(value, true);
        // Output($"Enum.Parse<MessageSeverity>(\"{value}\", ignoreCase: true) = {parseResult}");

        // Assert
        parseResult.ShouldBe(expectedSeverity);
    }

    [Theory]
    [InlineData("Information", true, MessageSeverity.Information)]
    [InlineData("Warning", true, MessageSeverity.Warning)]
    [InlineData("Error", true, MessageSeverity.Error)]
    [InlineData("Critical", true, MessageSeverity.Critical)]
    [InlineData("Invalid", false, default(MessageSeverity))]
    [InlineData("", false, default(MessageSeverity))]
    public void MessageSeverityShouldTryParseFromString(string value, bool expectedSuccess, MessageSeverity expectedSeverity)
    {
        // Act
        var success = Enum.TryParse<MessageSeverity>(value, out var result);
        // Output($"Enum.TryParse<MessageSeverity>(\"{value}\") = success: {success}, result: {result}");

        // Assert
        success.ShouldBe(expectedSuccess);
        if (expectedSuccess)
        {
            result.ShouldBe(expectedSeverity);
        }
    }

    [Fact]
    public void MessageSeverityShouldBeDefinedForAllExpectedValues()
    {
        // Arrange
        var expectedValues = new[] { 0, 1, 2, 3 };
        // Output($"Expected defined values: [{string.Join(", ", expectedValues)}]");

        // Act & Assert
        foreach (var value in expectedValues)
        {
            var isDefined = Enum.IsDefined(typeof(MessageSeverity), value);
            // Output($"Enum.IsDefined(typeof(MessageSeverity), {value}) = {isDefined}");
            isDefined.ShouldBeTrue();
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(100)]
    public void MessageSeverityShouldNotBeDefinedForInvalidValues(int value)
    {
        // Act
        var isDefined = Enum.IsDefined(typeof(MessageSeverity), value);
        // Output($"Enum.IsDefined(typeof(MessageSeverity), {value}) = {isDefined}");

        // Assert
        isDefined.ShouldBeFalse();
    }

    [Fact]
    public void MessageSeverityShouldSupportComparisonOperations()
    {
        // Act & Assert
        (MessageSeverity.Information < MessageSeverity.Warning).ShouldBeTrue();
        (MessageSeverity.Warning < MessageSeverity.Error).ShouldBeTrue();
        (MessageSeverity.Error < MessageSeverity.Critical).ShouldBeTrue();

        (MessageSeverity.Critical > MessageSeverity.Error).ShouldBeTrue();
        (MessageSeverity.Error > MessageSeverity.Warning).ShouldBeTrue();
        (MessageSeverity.Warning > MessageSeverity.Information).ShouldBeTrue();

        MessageSeverity.Information.Equals(MessageSeverity.Information).ShouldBeTrue();
        (MessageSeverity.Warning != MessageSeverity.Error).ShouldBeTrue();

        // Output("All comparison operations work as expected");
    }

    [Fact]
    public void MessageSeverityShouldSupportGetHashCode()
    {
        // Arrange
        var severity1 = MessageSeverity.Error;
        var severity2 = MessageSeverity.Error;
        var severity3 = MessageSeverity.Warning;

        // Act
        var hash1 = severity1.GetHashCode();
        var hash2 = severity2.GetHashCode();
        var hash3 = severity3.GetHashCode();

        // Output($"MessageSeverity.Error.GetHashCode() = {hash1}");
        // Output($"MessageSeverity.Error.GetHashCode() = {hash2}");
        // Output($"MessageSeverity.Warning.GetHashCode() = {hash3}");

        // Assert
        hash1.ShouldBe(hash2);
        hash1.ShouldNotBe(hash3);
    }
}