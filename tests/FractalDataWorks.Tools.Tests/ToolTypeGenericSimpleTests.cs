using FractalDataWorks.Tools.Tests.TestImplementations;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tools.Tests;

/// <summary>
/// Tests for the generic ToolTypeBase{TTool, TConfiguration} abstract class.
/// </summary>
public sealed class ToolTypeGenericSimpleTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetProperties()
    {
        // Arrange & Act
        var factory = new TestGenericToolFactory();
        var toolType = new TestGenericToolType(456, "GenericTestTool", "A generic test tool", factory);
        
        // Assert
        toolType.Id.ShouldBe(456);
        toolType.Name.ShouldBe("GenericTestTool");
        toolType.Description.ShouldBe("A generic test tool");
    }
    
    [Fact]
    public void CreateTypedFactory_ShouldReturnProvidedFactory()
    {
        // Arrange
        var expectedFactory = new TestGenericToolFactory();
        var toolType = new TestGenericToolType(1, "Test", "Description", expectedFactory);
        
        // Act
        var actualFactory = toolType.CreateTypedFactory();
        
        // Assert
        actualFactory.ShouldBeSameAs(expectedFactory);
    }
    
    [Fact]
    public void CreateFactory_ShouldReturnSameAsCreateTypedFactory()
    {
        // Arrange
        var expectedFactory = new TestGenericToolFactory();
        var toolType = new TestGenericToolType(1, "Test", "Description", expectedFactory);
        
        // Act
        var typedFactory = toolType.CreateTypedFactory();
        var baseFactory = toolType.CreateFactory();
        
        // Assert
        baseFactory.ShouldBeSameAs(typedFactory);
        baseFactory.ShouldBeSameAs(expectedFactory);
    }
    
    [Fact]
    public void Constructor_InheritsFromBaseClass_ShouldHaveBaseClassBehavior()
    {
        // Arrange
        var factory = new TestGenericToolFactory();
        
        // Act
        var toolType = new TestGenericToolType(789, "InheritanceTest", "Testing inheritance", factory);
        
        // Assert - Should behave as ToolTypeBase
        toolType.Id.ShouldBe(789);
        toolType.Name.ShouldBe("InheritanceTest");
        toolType.Description.ShouldBe("Testing inheritance");
        
        // Should be able to call both base and derived methods
        var baseFactory = toolType.CreateFactory();
        var typedFactory = toolType.CreateTypedFactory();
        
        baseFactory.ShouldBeSameAs(typedFactory);
    }
}