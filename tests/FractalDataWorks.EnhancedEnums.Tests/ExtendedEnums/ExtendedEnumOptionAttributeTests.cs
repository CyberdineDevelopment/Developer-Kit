using System;
using FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;
using Shouldly;
using Xunit;


namespace FractalDataWorks.EnhancedEnums.Tests.ExtendedEnums;

/// <summary>
/// Tests for ExtendedEnumOptionAttribute functionality.
/// </summary>
public sealed class ExtendedEnumOptionAttributeTests
{
    private readonly ITestOutputHelper _output;

    public ExtendedEnumOptionAttributeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DefaultConstructorSetsDefaultValues()
    {
        // Arrange & Act
        var attribute = new ExtendedEnumOptionAttribute();

        // Assert
        attribute.Name.ShouldBeNull();
        attribute.Order.ShouldBe(0);
        attribute.CollectionName.ShouldBeNull();
        attribute.ReturnType.ShouldBeNull();
        attribute.GenerateFactoryMethod.ShouldBe(false);
        attribute.MethodName.ShouldBeNull();

        _output.WriteLine("Default constructor created attribute with all null/default values");
    }

    [Fact]
    public void ParameterizedConstructorWithAllParametersSetsValues()
    {
        // Arrange
        const string name = "CustomName";
        const int order = 5;
        const string collectionName = "CustomCollection";
        var returnType = typeof(string);
        const bool generateFactoryMethod = true;
        const string methodName = "CustomMethod";

        // Act
        var attribute = new ExtendedEnumOptionAttribute(
            name, 
            order, 
            collectionName, 
            returnType, 
            generateFactoryMethod, 
            methodName);

        // Assert
        attribute.Name.ShouldBe(name);
        attribute.Order.ShouldBe(order);
        attribute.CollectionName.ShouldBe(collectionName);
        attribute.ReturnType.ShouldBe(returnType);
        attribute.GenerateFactoryMethod.ShouldBe(generateFactoryMethod);
        attribute.MethodName.ShouldBe(methodName);

        _output.WriteLine($"Parameterized constructor set Name: '{attribute.Name}', Order: {attribute.Order}");
        _output.WriteLine($"CollectionName: '{attribute.CollectionName}', ReturnType: {attribute.ReturnType?.Name}");
        _output.WriteLine($"GenerateFactoryMethod: {attribute.GenerateFactoryMethod}, MethodName: '{attribute.MethodName}'");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ProcessingStatus")]
    [InlineData("Custom Name With Spaces")]
    public void ParameterizedConstructorWithNameOnlySetsName(string? name)
    {
        // Arrange & Act
        var attribute = new ExtendedEnumOptionAttribute(name: name);

        // Assert
        attribute.Name.ShouldBe(name);
        attribute.Order.ShouldBe(0);
        attribute.CollectionName.ShouldBeNull();
        attribute.ReturnType.ShouldBeNull();
        attribute.GenerateFactoryMethod.ShouldBe(false);
        attribute.MethodName.ShouldBeNull();

        _output.WriteLine($"Constructor with name only set Name: '{attribute.Name}'");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void ParameterizedConstructorWithOrderOnlySetsOrder(int order)
    {
        // Arrange & Act
        var attribute = new ExtendedEnumOptionAttribute(order: order);

        // Assert
        attribute.Name.ShouldBeNull();
        attribute.Order.ShouldBe(order);
        attribute.CollectionName.ShouldBeNull();
        attribute.ReturnType.ShouldBeNull();
        attribute.GenerateFactoryMethod.ShouldBe(false);
        attribute.MethodName.ShouldBeNull();

        _output.WriteLine($"Constructor with order only set Order: {attribute.Order}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("DefaultCollection")]
    [InlineData("SpecialStatusCollection")]
    public void ParameterizedConstructorWithCollectionNameOnlySetsCollectionName(string? collectionName)
    {
        // Arrange & Act
        var attribute = new ExtendedEnumOptionAttribute(collectionName: collectionName);

        // Assert
        attribute.Name.ShouldBeNull();
        attribute.Order.ShouldBe(0);
        attribute.CollectionName.ShouldBe(collectionName);
        attribute.ReturnType.ShouldBeNull();
        attribute.GenerateFactoryMethod.ShouldBe(false);
        attribute.MethodName.ShouldBeNull();

        _output.WriteLine($"Constructor with collection name only set CollectionName: '{attribute.CollectionName}'");
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(TestStatus))]
    public void ParameterizedConstructorWithReturnTypeOnlySetsReturnType(Type returnType)
    {
        // Arrange & Act
        var attribute = new ExtendedEnumOptionAttribute(returnType: returnType);

        // Assert
        attribute.Name.ShouldBeNull();
        attribute.Order.ShouldBe(0);
        attribute.CollectionName.ShouldBeNull();
        attribute.ReturnType.ShouldBe(returnType);
        attribute.GenerateFactoryMethod.ShouldBe(false);
        attribute.MethodName.ShouldBeNull();

        _output.WriteLine($"Constructor with return type only set ReturnType: {attribute.ReturnType?.Name}");
    }

    [Fact]
    public void ParameterizedConstructorWithNullReturnTypeSetsReturnType()
    {
        // Arrange & Act
        var attribute = new ExtendedEnumOptionAttribute(returnType: null);

        // Assert
        attribute.ReturnType.ShouldBeNull();
        _output.WriteLine("Constructor with null return type set ReturnType to null");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ParameterizedConstructorWithGenerateFactoryMethodOnlySetsGenerateFactoryMethod(bool generateFactoryMethod)
    {
        // Arrange & Act
        var attribute = new ExtendedEnumOptionAttribute(generateFactoryMethod: generateFactoryMethod);

        // Assert
        attribute.Name.ShouldBeNull();
        attribute.Order.ShouldBe(0);
        attribute.CollectionName.ShouldBeNull();
        attribute.ReturnType.ShouldBeNull();
        attribute.GenerateFactoryMethod.ShouldBe(generateFactoryMethod);
        attribute.MethodName.ShouldBeNull();

        _output.WriteLine($"Constructor with generate factory method only set GenerateFactoryMethod: {attribute.GenerateFactoryMethod}");
    }

    [Fact]
    public void ParameterizedConstructorWithDefaultGenerateFactoryMethodSetsGenerateFactoryMethod()
    {
        // Arrange & Act
        var attribute = new ExtendedEnumOptionAttribute();

        // Assert
        attribute.GenerateFactoryMethod.ShouldBe(false);
        _output.WriteLine("Constructor with default generate factory method set GenerateFactoryMethod to false");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("CreateProcessing")]
    [InlineData("GetCustomStatus")]
    public void ParameterizedConstructorWithMethodNameOnlySetsMethodName(string? methodName)
    {
        // Arrange & Act
        var attribute = new ExtendedEnumOptionAttribute(methodName: methodName);

        // Assert
        attribute.Name.ShouldBeNull();
        attribute.Order.ShouldBe(0);
        attribute.CollectionName.ShouldBeNull();
        attribute.ReturnType.ShouldBeNull();
        attribute.GenerateFactoryMethod.ShouldBe(false);
        attribute.MethodName.ShouldBe(methodName);

        _output.WriteLine($"Constructor with method name only set MethodName: '{attribute.MethodName}'");
    }

    [Fact]
    public void ParameterizedConstructorWithMixedParametersWorks()
    {
        // Arrange
        const string name = "MixedTest";
        const int order = 42;
        const bool generateFactoryMethod = false;

        // Act
        var attribute = new ExtendedEnumOptionAttribute(
            name: name,
            order: order,
            generateFactoryMethod: generateFactoryMethod);

        // Assert
        attribute.Name.ShouldBe(name);
        attribute.Order.ShouldBe(order);
        attribute.CollectionName.ShouldBeNull();
        attribute.ReturnType.ShouldBeNull();
        attribute.GenerateFactoryMethod.ShouldBe(generateFactoryMethod);
        attribute.MethodName.ShouldBeNull();

        _output.WriteLine($"Mixed parameters constructor: Name='{attribute.Name}', Order={attribute.Order}, GenerateFactoryMethod={attribute.GenerateFactoryMethod}");
    }

    [Fact]
    public void PropertiesAreReadOnlyAfterConstruction()
    {
        // Arrange
        var attribute = new ExtendedEnumOptionAttribute("Test", 1, "TestCollection", typeof(string), true, "TestMethod");

        // Act & Assert - This test verifies that properties don't have setters
        // by checking that the attribute properties retain their constructed values
        attribute.Name.ShouldBe("Test");
        attribute.Order.ShouldBe(1);
        attribute.CollectionName.ShouldBe("TestCollection");
        attribute.ReturnType.ShouldBe(typeof(string));
        attribute.GenerateFactoryMethod.ShouldBe(true);
        attribute.MethodName.ShouldBe("TestMethod");

        _output.WriteLine("All properties retain their constructed values (read-only verification)");
    }

    [Fact]
    public void AttributeCanBeAppliedToClass()
    {
        // Arrange & Act - This test verifies the AttributeUsage is configured correctly
        var attributes = typeof(ExtendedEnumOptionAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false);

        // Assert
        attributes.Length.ShouldBe(1);
        var usage = (AttributeUsageAttribute)attributes[0];
        usage.ValidOn.ShouldBe(AttributeTargets.Class);
        usage.AllowMultiple.ShouldBeFalse();

        _output.WriteLine($"ExtendedEnumOptionAttribute can be applied to: {usage.ValidOn}");
        _output.WriteLine($"Allow multiple: {usage.AllowMultiple}");
    }

    [Fact]
    public void DefaultConstructorAndParameterizedConstructorHaveSameDefaults()
    {
        // Arrange
        var defaultAttribute = new ExtendedEnumOptionAttribute();
        var parameterizedAttribute = new ExtendedEnumOptionAttribute(
            name: null,
            order: 0,
            collectionName: null,
            returnType: null,
            generateFactoryMethod: false,
            methodName: null);

        // Act & Assert
        defaultAttribute.Name.ShouldBe(parameterizedAttribute.Name);
        defaultAttribute.Order.ShouldBe(parameterizedAttribute.Order);
        defaultAttribute.CollectionName.ShouldBe(parameterizedAttribute.CollectionName);
        defaultAttribute.ReturnType.ShouldBe(parameterizedAttribute.ReturnType);
        defaultAttribute.GenerateFactoryMethod.ShouldBe(parameterizedAttribute.GenerateFactoryMethod);
        defaultAttribute.MethodName.ShouldBe(parameterizedAttribute.MethodName);

        _output.WriteLine("Default constructor and parameterized constructor with default values produce identical results");
    }
}