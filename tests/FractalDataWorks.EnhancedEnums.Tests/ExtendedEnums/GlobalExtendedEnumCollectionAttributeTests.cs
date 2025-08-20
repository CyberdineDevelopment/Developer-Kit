using System;
using FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests.ExtendedEnums;

/// <summary>
/// Tests for GlobalExtendedEnumCollectionAttribute functionality.
/// </summary>
public sealed class GlobalExtendedEnumCollectionAttributeTests
{
    private readonly ITestOutputHelper _output;

    public GlobalExtendedEnumCollectionAttributeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstructorWithValidEnumTypeSetsProperties()
    {
        // Arrange
        var enumType = typeof(TestStatus);

        // Act
        var attribute = new GlobalExtendedEnumCollectionAttribute(enumType);

        // Assert
        attribute.EnumType.ShouldBe(enumType);
        attribute.CollectionName.ShouldBe("GlobalTestStatusCollection");
        attribute.NameComparison.ShouldBe(StringComparison.Ordinal);
        attribute.GenerateFactoryMethods.ShouldBeTrue();
        attribute.GenerateStaticCollection.ShouldBeTrue();
        attribute.UseSingletonInstances.ShouldBeTrue();
        attribute.Generic.ShouldBeFalse();
        attribute.DefaultReturnType.ShouldBeNull();

        _output.WriteLine($"GlobalExtendedEnumCollectionAttribute created with enum type: {attribute.EnumType.Name}");
        _output.WriteLine($"Default collection name: {attribute.CollectionName}");
    }

    [Fact]
    public void ConstructorWithNullEnumTypeThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new GlobalExtendedEnumCollectionAttribute(null!));
        exception.ParamName.ShouldBe("enumType");

        _output.WriteLine($"ArgumentNullException thrown as expected: {exception.Message}");
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(object))]
    public void ConstructorWithNonEnumTypeThrowsArgumentException(Type nonEnumType)
    {
        // Arrange, Act & Assert
        var exception = Should.Throw<ArgumentException>(() => new GlobalExtendedEnumCollectionAttribute(nonEnumType));
        exception.ParamName.ShouldBe("enumType");
        exception.Message.ShouldContain($"Type {nonEnumType.Name} must be an enum type.");

        _output.WriteLine($"ArgumentException thrown for non-enum type {nonEnumType.Name}: {exception.Message}");
    }

    [Fact]
    public void CollectionNameCanBeModified()
    {
        // Arrange
        var attribute = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));
        const string customName = "CustomGlobalStatusCollection";

        // Act
        attribute.CollectionName = customName;

        // Assert
        attribute.CollectionName.ShouldBe(customName);
        _output.WriteLine($"Collection name changed to: {attribute.CollectionName}");
    }

    [Theory]
    [InlineData(StringComparison.OrdinalIgnoreCase)]
    [InlineData(StringComparison.CurrentCulture)]
    [InlineData(StringComparison.InvariantCulture)]
    [InlineData(StringComparison.CurrentCultureIgnoreCase)]
    [InlineData(StringComparison.InvariantCultureIgnoreCase)]
    public void NameComparisonCanBeModified(StringComparison comparison)
    {
        // Arrange
        var attribute = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));

        // Act
        attribute.NameComparison = comparison;

        // Assert
        attribute.NameComparison.ShouldBe(comparison);
        _output.WriteLine($"Name comparison set to: {attribute.NameComparison}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GenerateFactoryMethodsCanBeModified(bool value)
    {
        // Arrange
        var attribute = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));

        // Act
        attribute.GenerateFactoryMethods = value;

        // Assert
        attribute.GenerateFactoryMethods.ShouldBe(value);
        _output.WriteLine($"Generate factory methods set to: {attribute.GenerateFactoryMethods}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GenerateStaticCollectionCanBeModified(bool value)
    {
        // Arrange
        var attribute = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));

        // Act
        attribute.GenerateStaticCollection = value;

        // Assert
        attribute.GenerateStaticCollection.ShouldBe(value);
        _output.WriteLine($"Generate static collection set to: {attribute.GenerateStaticCollection}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UseSingletonInstancesCanBeModified(bool value)
    {
        // Arrange
        var attribute = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));

        // Act
        attribute.UseSingletonInstances = value;

        // Assert
        attribute.UseSingletonInstances.ShouldBe(value);
        _output.WriteLine($"Use singleton instances set to: {attribute.UseSingletonInstances}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GenericCanBeModified(bool value)
    {
        // Arrange
        var attribute = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));

        // Act
        attribute.Generic = value;

        // Assert
        attribute.Generic.ShouldBe(value);
        _output.WriteLine($"Generic set to: {attribute.Generic}");
    }

    [Fact]
    public void DefaultReturnTypeCanBeModified()
    {
        // Arrange
        var attribute = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));
        var returnType = typeof(string);

        // Act
        attribute.DefaultReturnType = returnType;

        // Assert
        attribute.DefaultReturnType.ShouldBe(returnType);
        _output.WriteLine($"Default return type set to: {attribute.DefaultReturnType?.Name}");
    }

    [Fact]
    public void DefaultReturnTypeCanBeSetToNull()
    {
        // Arrange
        var attribute = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus))
        {
            DefaultReturnType = typeof(string)
        };

        // Act
        attribute.DefaultReturnType = null;

        // Assert
        attribute.DefaultReturnType.ShouldBeNull();
        _output.WriteLine("Default return type set to null");
    }

    [Fact]
    public void AttributeCanBeAppliedToClass()
    {
        // Arrange & Act - This test verifies the AttributeUsage is configured correctly
        var attributes = typeof(GlobalExtendedEnumCollectionAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false);

        // Assert
        attributes.Length.ShouldBe(1);
        var usage = (AttributeUsageAttribute)attributes[0];
        usage.ValidOn.ShouldBe(AttributeTargets.Class);
        usage.AllowMultiple.ShouldBeFalse();

        _output.WriteLine($"GlobalExtendedEnumCollectionAttribute can be applied to: {usage.ValidOn}");
        _output.WriteLine($"Allow multiple: {usage.AllowMultiple}");
    }

    /// <summary>
    /// Test enum for verifying global collection naming.
    /// </summary>
    private enum PriorityLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    [Fact]
    public void ConstructorWithDifferentEnumGeneratesCorrectCollectionName()
    {
        // Arrange
        var enumType = typeof(PriorityLevel);

        // Act
        var attribute = new GlobalExtendedEnumCollectionAttribute(enumType);

        // Assert
        attribute.EnumType.ShouldBe(enumType);
        attribute.CollectionName.ShouldBe("GlobalPriorityLevelCollection");

        _output.WriteLine($"GlobalExtendedEnumCollectionAttribute works with different enum: {attribute.EnumType.Name}");
        _output.WriteLine($"Generated collection name: {attribute.CollectionName}");
    }

    [Fact]
    public void DefaultValuesMatchExpectedConfiguration()
    {
        // Arrange
        var attribute = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));

        // Act & Assert - Verify all default values are as expected for global collections
        attribute.NameComparison.ShouldBe(StringComparison.Ordinal);
        attribute.GenerateFactoryMethods.ShouldBeTrue();
        attribute.GenerateStaticCollection.ShouldBeTrue();
        attribute.UseSingletonInstances.ShouldBeTrue();
        attribute.Generic.ShouldBeFalse();

        _output.WriteLine("All default values match expected global collection configuration");
        _output.WriteLine($"NameComparison: {attribute.NameComparison}");
        _output.WriteLine($"GenerateFactoryMethods: {attribute.GenerateFactoryMethods}");
        _output.WriteLine($"GenerateStaticCollection: {attribute.GenerateStaticCollection}");
        _output.WriteLine($"UseSingletonInstances: {attribute.UseSingletonInstances}");
        _output.WriteLine($"Generic: {attribute.Generic}");
    }

    [Fact]
    public void GlobalCollectionNameFormatIsCorrect()
    {
        // Arrange
        var testCases = new[]
        {
            (typeof(TestStatus), "GlobalTestStatusCollection"),
            (typeof(PriorityLevel), "GlobalPriorityLevelCollection")
        };

        foreach (var (enumType, expectedName) in testCases)
        {
            // Act
            var attribute = new GlobalExtendedEnumCollectionAttribute(enumType);

            // Assert
            attribute.CollectionName.ShouldBe(expectedName);
            attribute.CollectionName.ShouldStartWith("Global");
            attribute.CollectionName.ShouldEndWith("Collection");

            _output.WriteLine($"Enum {enumType.Name} -> Collection name: {attribute.CollectionName}");
        }
    }

    [Fact]
    public void PropertyModificationIsIndependent()
    {
        // Arrange
        var attribute1 = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));
        var attribute2 = new GlobalExtendedEnumCollectionAttribute(typeof(TestStatus));

        // Act
        attribute1.CollectionName = "Modified1";
        attribute1.GenerateFactoryMethods = false;
        attribute2.CollectionName = "Modified2";
        attribute2.UseSingletonInstances = false;

        // Assert
        attribute1.CollectionName.ShouldBe("Modified1");
        attribute1.GenerateFactoryMethods.ShouldBeFalse();
        attribute1.UseSingletonInstances.ShouldBeTrue(); // Should remain default

        attribute2.CollectionName.ShouldBe("Modified2");
        attribute2.GenerateFactoryMethods.ShouldBeTrue(); // Should remain default
        attribute2.UseSingletonInstances.ShouldBeFalse();

        _output.WriteLine("Property modifications on separate instances are independent");
        _output.WriteLine($"Attribute1: CollectionName='{attribute1.CollectionName}', GenerateFactoryMethods={attribute1.GenerateFactoryMethods}");
        _output.WriteLine($"Attribute2: CollectionName='{attribute2.CollectionName}', UseSingletonInstances={attribute2.UseSingletonInstances}");
    }
}