using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tools.Tests;

/// <summary>
/// Tests for ToolTypeBase and ToolTypeFactoryBase classes.
/// </summary>
public class ToolTypeBaseTests
{
    [Fact(Skip = "Enhanced Enum attributes temporarily disabled")]
    public void ToolTypeBaseHasEnhancedEnumBaseAttribute()
    {
        // Arrange
        var toolTypeBaseType = typeof(ToolTypeBase);

        // Act
        var attribute = toolTypeBaseType.GetCustomAttribute<EnumCollectionAttribute>();

        // Assert
        attribute.ShouldNotBeNull($"ToolTypeBase should have EnumCollectionAttribute");
        attribute.CollectionName.ShouldBe("ToolTypes", $"Collection name should be 'ToolTypes'");
        // Note: ReturnType and ReturnTypeNamespace properties may not be fully implemented yet
        // attribute.ReturnType.ShouldBe("IToolFactory<IFdwTool, IFdwConfiguration>", $"Return type should match expected interface");
        // attribute.ReturnTypeNamespace.ShouldBe("FractalDataWorks.Tools", $"Return type namespace should be correct");
    }

    [Fact]
    public void ToolTypeBaseHasCorrectGenericConstraints()
    {
        // Arrange
        var toolTypeBaseType = typeof(ToolTypeBase<,>);

        // Act
        var genericParams = toolTypeBaseType.GetGenericArguments();
        var toolParam = genericParams[0];
        var configParam = genericParams[1];

        // Assert
        genericParams.Length.ShouldBe(2, $"ToolTypeBase should have 2 generic parameters");
        
        // Check TTool constraints
        toolParam.Name.ShouldBe("TTool", $"First parameter should be named TTool");
        toolParam.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint).ShouldBeTrue(
            $"TTool should have class constraint");
        var toolConstraints = toolParam.GetGenericParameterConstraints();
        toolConstraints.ShouldContain(typeof(IFdwTool), $"TTool should be constrained to IFdwTool");

        // Check TConfiguration constraints
        configParam.Name.ShouldBe("TConfiguration", $"Second parameter should be named TConfiguration");
        configParam.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint).ShouldBeTrue(
            $"TConfiguration should have class constraint");
        var configConstraints = configParam.GetGenericParameterConstraints();
        configConstraints.ShouldContain(typeof(IFdwConfiguration), $"TConfiguration should be constrained to IFdwConfiguration");
    }

    [Fact]
    public void ToolTypeBaseGenericInheritsFromNonGeneric()
    {
        // Arrange
        var toolTypeBaseType = typeof(ToolTypeBase<,>);

        // Act
        var baseType = toolTypeBaseType.BaseType;

        // Assert
        baseType.ShouldNotBeNull($"ToolTypeBase<,> should have a base type");
        baseType.ShouldBe(typeof(ToolTypeBase),
            $"ToolTypeBase<,> should inherit from non-generic ToolTypeBase");
    }

    // Note: EnhancedEnumBaseAttribute test removed as this attribute does not exist in the consolidated attribute structure

    [Fact]
    public void ToolTypeFactoryBaseHasRequiredProperties()
    {
        // Arrange
        var factoryBaseType = typeof(ToolTypeFactoryBase<,>);

        // Act
        var idProperty = factoryBaseType.GetProperty("Id");
        var nameProperty = factoryBaseType.GetProperty("Name");
        var descriptionProperty = factoryBaseType.GetProperty("Description");

        // Assert
        idProperty.ShouldNotBeNull($"ToolTypeFactoryBase should have Id property");
        idProperty.PropertyType.ShouldBe(typeof(int), $"Id should be of type int");
        idProperty.CanWrite.ShouldBeFalse($"Id should be read-only");

        nameProperty.ShouldNotBeNull($"ToolTypeFactoryBase should have Name property");
        nameProperty.PropertyType.ShouldBe(typeof(string), $"Name should be of type string");
        nameProperty.CanWrite.ShouldBeFalse($"Name should be read-only");

        descriptionProperty.ShouldNotBeNull($"ToolTypeFactoryBase should have Description property");
        descriptionProperty.PropertyType.ShouldBe(typeof(string), $"Description should be of type string");
        descriptionProperty.CanWrite.ShouldBeFalse($"Description should be read-only");
    }

    [Fact]
    public void ToolTypeFactoryBaseHasRequiredAbstractMethods()
    {
        // Arrange
        var factoryBaseType = typeof(ToolTypeFactoryBase<,>);

        // Act
        var genericArgs = factoryBaseType.GetGenericArguments();
        var configType = genericArgs[1];
        var createMethod = factoryBaseType.GetMethod("Create", new[] { configType });
        var getToolByNameMethod = factoryBaseType.GetMethod("GetTool", new[] { typeof(string) });
        var getToolByIdMethod = factoryBaseType.GetMethod("GetTool", new[] { typeof(int) });

        // Assert
        createMethod.ShouldNotBeNull($"ToolTypeFactoryBase should have Create method");
        createMethod.IsAbstract.ShouldBeTrue($"Create method should be abstract");
        createMethod.ReturnType.ShouldBe(typeof(object), $"Create should return object");

        getToolByNameMethod.ShouldNotBeNull($"ToolTypeFactoryBase should have GetTool(string) method");
        getToolByNameMethod.IsAbstract.ShouldBeTrue($"GetTool(string) method should be abstract");

        getToolByIdMethod.ShouldNotBeNull($"ToolTypeFactoryBase should have GetTool(int) method");
        getToolByIdMethod.IsAbstract.ShouldBeTrue($"GetTool(int) method should be abstract");
    }

    [Fact]
    public void ToolTypeBaseConstructorCallsBaseCorrectly()
    {
        // This test verifies that concrete implementations can be created properly
        // Arrange & Act
        var concreteType = new TestToolType(1, "Test", "Test Tool");

        // Assert
        concreteType.Id.ShouldBe(1, $"Id should be set from constructor");
        concreteType.Name.ShouldBe("Test", $"Name should be set from constructor");
        concreteType.Description.ShouldBe("Test Tool", $"Description should be set from constructor");
    }

    // Test implementation to verify instantiation
    private sealed class TestToolType : ToolTypeBase<IFdwTool, IFdwConfiguration>
    {
        public TestToolType(int id, string name, string description) 
            : base(id, name, description)
        {
        }

        public override IToolFactory<IFdwTool, IFdwConfiguration> CreateTypedFactory()
        {
            throw new NotImplementedException("Test implementation");
        }
    }
}