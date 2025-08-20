using System;
using FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;
using Shouldly;
using Xunit;

namespace FractalDataWorks.EnhancedEnums.Tests.ExtendedEnums;

/// <summary>
/// Tests for ExtendEnumAttribute functionality.
/// </summary>
public sealed class ExtendEnumAttributeTests
{
    private readonly ITestOutputHelper _output;

    public ExtendEnumAttributeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstructorWithValidEnumTypeSetsProperties()
    {
        // Arrange
        var enumType = typeof(TestStatus);

        // Act
        var attribute = new ExtendEnumAttribute(enumType);

        // Assert
        attribute.EnumType.ShouldBe(enumType);
        attribute.CollectionName.ShouldBe("TestStatusCollection");
        attribute.NameComparison.ShouldBe(StringComparison.Ordinal);
        attribute.GenerateFactoryMethods.ShouldBeTrue();
        attribute.GenerateStaticCollection.ShouldBeTrue();
        attribute.UseSingletonInstances.ShouldBeTrue();
        attribute.IncludeReferencedAssemblies.ShouldBeFalse();
        attribute.Generic.ShouldBeFalse();
        attribute.DefaultReturnType.ShouldBeNull();

        _output.WriteLine($"ExtendEnumAttribute created with enum type: {attribute.EnumType.Name}");
        _output.WriteLine($"Default collection name: {attribute.CollectionName}");
    }

    [Fact]
    public void ConstructorWithNullEnumTypeThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Should.Throw<ArgumentNullException>(() => new ExtendEnumAttribute(null!));
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
        var exception = Should.Throw<ArgumentException>(() => new ExtendEnumAttribute(nonEnumType));
        exception.ParamName.ShouldBe("enumType");
        exception.Message.ShouldContain($"Type {nonEnumType.Name} must be an enum type.");
        
        _output.WriteLine($"ArgumentException thrown for non-enum type {nonEnumType.Name}: {exception.Message}");
    }

    [Fact]
    public void CollectionNameCanBeModified()
    {
        // Arrange
        var attribute = new ExtendEnumAttribute(typeof(TestStatus));
        const string customName = "CustomStatusCollection";

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
    public void NameComparisonCanBeModified(StringComparison comparison)
    {
        // Arrange
        var attribute = new ExtendEnumAttribute(typeof(TestStatus));

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
        var attribute = new ExtendEnumAttribute(typeof(TestStatus));

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
        var attribute = new ExtendEnumAttribute(typeof(TestStatus));

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
        var attribute = new ExtendEnumAttribute(typeof(TestStatus));

        // Act
        attribute.UseSingletonInstances = value;

        // Assert
        attribute.UseSingletonInstances.ShouldBe(value);
        _output.WriteLine($"Use singleton instances set to: {attribute.UseSingletonInstances}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IncludeReferencedAssembliesCanBeModified(bool value)
    {
        // Arrange
        var attribute = new ExtendEnumAttribute(typeof(TestStatus));

        // Act
        attribute.IncludeReferencedAssemblies = value;

        // Assert
        attribute.IncludeReferencedAssemblies.ShouldBe(value);
        _output.WriteLine($"Include referenced assemblies set to: {attribute.IncludeReferencedAssemblies}");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GenericCanBeModified(bool value)
    {
        // Arrange
        var attribute = new ExtendEnumAttribute(typeof(TestStatus));

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
        var attribute = new ExtendEnumAttribute(typeof(TestStatus));
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
        var attribute = new ExtendEnumAttribute(typeof(TestStatus))
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
        var attributes = typeof(ExtendEnumAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false);

        // Assert
        attributes.Length.ShouldBe(1);
        var usage = (AttributeUsageAttribute)attributes[0];
        usage.ValidOn.ShouldBe(AttributeTargets.Class);
        usage.AllowMultiple.ShouldBeFalse();
        
        _output.WriteLine($"ExtendEnumAttribute can be applied to: {usage.ValidOn}");
        _output.WriteLine($"Allow multiple: {usage.AllowMultiple}");
    }

    /// <summary>
    /// Test enum that mimics DateTime for testing invalid enum types.
    /// </summary>
    private enum TestDateTimeEnum
    {
        Today = 1,
        Tomorrow = 2
    }

    [Fact]
    public void ConstructorWithValidEnumWorksForDifferentEnumTypes()
    {
        // Arrange
        var enumType = typeof(TestDateTimeEnum);

        // Act
        var attribute = new ExtendEnumAttribute(enumType);

        // Assert
        attribute.EnumType.ShouldBe(enumType);
        attribute.CollectionName.ShouldBe("TestDateTimeEnumCollection");
        
        _output.WriteLine($"ExtendEnumAttribute works with custom enum: {attribute.EnumType.Name}");
        _output.WriteLine($"Generated collection name: {attribute.CollectionName}");
    }
}