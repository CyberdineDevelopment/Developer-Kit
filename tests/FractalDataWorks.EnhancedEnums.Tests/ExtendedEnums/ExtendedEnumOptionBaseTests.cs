using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests.ExtendedEnums;

/// <summary>
/// Tests for ExtendedEnumOptionBase functionality.
/// </summary>
public sealed class ExtendedEnumOptionBaseTests
{
    private readonly ITestOutputHelper _output;

    public ExtendedEnumOptionBaseTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstructorWithEnumValueStoresValue()
    {
        // Arrange
        const TestStatus expectedStatus = TestStatus.Processing;

        // Act
        var option = new ConcreteTestStatusOption(expectedStatus);

        // Assert
        option.EnumValue.ShouldBe(expectedStatus);
        _output.WriteLine($"Created option with enum value: {option.EnumValue}");
    }

    [Theory]
    [InlineData(TestStatus.Pending, 1)]
    [InlineData(TestStatus.Processing, 2)]
    [InlineData(TestStatus.Completed, 3)]
    [InlineData(TestStatus.Failed, 4)]
    public void IdPropertyReturnsIntValueOfEnumValue(TestStatus enumValue, int expectedId)
    {
        // Arrange
        var option = new ConcreteTestStatusOption(enumValue);

        // Act
        var actualId = option.Id;

        // Assert
        actualId.ShouldBe(expectedId);
        _output.WriteLine($"Enum value {enumValue} has Id: {actualId}");
    }

    [Theory]
    [InlineData(TestStatus.Pending, "Pending")]
    [InlineData(TestStatus.Processing, "Processing")]
    [InlineData(TestStatus.Completed, "Completed")]
    [InlineData(TestStatus.Failed, "Failed")]
    public void NamePropertyReturnsEnumValueToString(TestStatus enumValue, string expectedName)
    {
        // Arrange
        var option = new ConcreteTestStatusOption(enumValue);

        // Act
        var actualName = option.Name;

        // Assert
        actualName.ShouldBe(expectedName);
        _output.WriteLine($"Enum value {enumValue} has Name: {actualName}");
    }

    [Fact]
    public void EnumValuePropertyReturnsUnderlyingEnum()
    {
        // Arrange
        const TestStatus expectedStatus = TestStatus.Completed;
        var option = new ConcreteTestStatusOption(expectedStatus);

        // Act
        var actualStatus = option.EnumValue;

        // Assert
        actualStatus.ShouldBe(expectedStatus);
        _output.WriteLine($"EnumValue property returned: {actualStatus}");
    }

    [Theory]
    [InlineData(TestStatus.Pending)]
    [InlineData(TestStatus.Processing)]
    [InlineData(TestStatus.Completed)]
    [InlineData(TestStatus.Failed)]
    public void ImplicitConversionToUnderlyingEnumWorks(TestStatus enumValue)
    {
        // Arrange
        var option = new ConcreteTestStatusOption(enumValue);

        // Act
        TestStatus convertedValue = option;

        // Assert
        convertedValue.ShouldBe(enumValue);
        _output.WriteLine($"Implicit conversion from option to enum: {convertedValue}");
    }

    [Fact]
    public void EqualsWithSameExtendedEnumOptionReturnsTrue()
    {
        // Arrange
        var option1 = new ConcreteTestStatusOption(TestStatus.Processing);
        var option2 = new ConcreteTestStatusOption(TestStatus.Processing);

        // Act
        var areEqual = option1.Equals(option2);

        // Assert
        areEqual.ShouldBeTrue();
        _output.WriteLine($"Two options with same enum value are equal: {areEqual}");
    }

    [Fact]
    public void EqualsWithDifferentExtendedEnumOptionReturnsFalse()
    {
        // Arrange
        var option1 = new ConcreteTestStatusOption(TestStatus.Processing);
        var option2 = new ConcreteTestStatusOption(TestStatus.Completed);

        // Act
        var areEqual = option1.Equals(option2);

        // Assert
        areEqual.ShouldBeFalse();
        _output.WriteLine($"Two options with different enum values are not equal: {areEqual}");
    }

    [Theory]
    [InlineData(TestStatus.Pending)]
    [InlineData(TestStatus.Processing)]
    [InlineData(TestStatus.Completed)]
    [InlineData(TestStatus.Failed)]
    public void EqualsWithMatchingEnumValueReturnsTrue(TestStatus enumValue)
    {
        // Arrange
        var option = new ConcreteTestStatusOption(enumValue);

        // Act
        var areEqual = option.Equals(enumValue);

        // Assert
        areEqual.ShouldBeTrue();
        _output.WriteLine($"Option equals matching enum value {enumValue}: {areEqual}");
    }

    [Fact]
    public void EqualsWithDifferentEnumValueReturnsFalse()
    {
        // Arrange
        var option = new ConcreteTestStatusOption(TestStatus.Processing);
        const TestStatus differentValue = TestStatus.Completed;

        // Act
        var areEqual = option.Equals(differentValue);

        // Assert
        areEqual.ShouldBeFalse();
        _output.WriteLine($"Option does not equal different enum value {differentValue}: {areEqual}");
    }

    [Fact]
    public void EqualsWithNullReturnsFalse()
    {
        // Arrange
        var option = new ConcreteTestStatusOption(TestStatus.Processing);

        // Act
        var areEqual = option.Equals(null);

        // Assert
        areEqual.ShouldBeFalse();
        _output.WriteLine($"Option does not equal null: {areEqual}");
    }

    [Fact]
    public void EqualsWithDifferentTypeReturnsFalse()
    {
        // Arrange
        var option = new ConcreteTestStatusOption(TestStatus.Processing);
        var differentObject = "not an enum option";

        // Act
        var areEqual = option.Equals(differentObject);

        // Assert
        areEqual.ShouldBeFalse();
        _output.WriteLine($"Option does not equal different type: {areEqual}");
    }

    [Theory]
    [InlineData(TestStatus.Pending)]
    [InlineData(TestStatus.Processing)]
    [InlineData(TestStatus.Completed)]
    [InlineData(TestStatus.Failed)]
    public void GetHashCodeReturnsEnumValueHashCode(TestStatus enumValue)
    {
        // Arrange
        var option = new ConcreteTestStatusOption(enumValue);
        var expectedHashCode = enumValue.GetHashCode();

        // Act
        var actualHashCode = option.GetHashCode();

        // Assert
        actualHashCode.ShouldBe(expectedHashCode);
        _output.WriteLine($"HashCode for {enumValue}: {actualHashCode}");
    }

    [Fact]
    public void GetHashCodeConsistentForSameEnumValue()
    {
        // Arrange
        var option1 = new ConcreteTestStatusOption(TestStatus.Processing);
        var option2 = new ConcreteTestStatusOption(TestStatus.Processing);

        // Act
        var hashCode1 = option1.GetHashCode();
        var hashCode2 = option2.GetHashCode();

        // Assert
        hashCode1.ShouldBe(hashCode2);
        _output.WriteLine($"Consistent hash codes for same enum value: {hashCode1}");
    }

    [Theory]
    [InlineData(TestStatus.Pending, "Pending")]
    [InlineData(TestStatus.Processing, "Processing")]
    [InlineData(TestStatus.Completed, "Completed")]
    [InlineData(TestStatus.Failed, "Failed")]
    public void ToStringReturnsName(TestStatus enumValue, string expectedString)
    {
        // Arrange
        var option = new ConcreteTestStatusOption(enumValue);

        // Act
        var actualString = option.ToString();

        // Assert
        actualString.ShouldBe(expectedString);
        _output.WriteLine($"ToString() for {enumValue}: '{actualString}'");
    }

    [Fact]
    public void ToStringMatchesNameProperty()
    {
        // Arrange
        var option = new ConcreteTestStatusOption(TestStatus.Processing);

        // Act
        var toStringResult = option.ToString();
        var nameProperty = option.Name;

        // Assert
        toStringResult.ShouldBe(nameProperty);
        _output.WriteLine($"ToString() matches Name property: '{toStringResult}'");
    }
}