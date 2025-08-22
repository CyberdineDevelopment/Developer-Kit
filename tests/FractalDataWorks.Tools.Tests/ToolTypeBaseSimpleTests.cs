using FractalDataWorks.Tools.Tests.TestImplementations;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tools.Tests;

/// <summary>
/// Tests for the ToolTypeBase abstract class.
/// </summary>
public sealed class ToolTypeBaseSimpleTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetProperties()
    {
        // Arrange
        const int expectedId = 123;
        const string expectedName = "TestTool";
        const string expectedDescription = "A test tool for testing";
        var factory = new TestToolFactory();
        
        // Act
        var toolType = new TestToolType(expectedId, expectedName, expectedDescription, factory);
        
        // Assert
        toolType.Id.ShouldBe(expectedId);
        toolType.Name.ShouldBe(expectedName);
        toolType.Description.ShouldBe(expectedDescription);
    }
    
    [Fact]
    public void Constructor_WithNullName_ShouldAllowNull()
    {
        // Arrange
        const int id = 1;
        string? name = null;
        const string description = "Test description";
        var factory = new TestToolFactory();
        
        // Act
        var toolType = new TestToolType(id, name!, description, factory);
        
        // Assert
        toolType.Name.ShouldBe(null);
        toolType.Id.ShouldBe(id);
        toolType.Description.ShouldBe(description);
    }
    
    [Fact]
    public void CreateFactory_ShouldReturnProvidedFactory()
    {
        // Arrange
        var expectedFactory = new TestToolFactory();
        var toolType = new TestToolType(1, "Test", "Description", expectedFactory);
        
        // Act
        var actualFactory = toolType.CreateFactory();
        
        // Assert
        actualFactory.ShouldBeSameAs(expectedFactory);
    }
    
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void Constructor_WithVariousIds_ShouldAcceptAllValues(int id)
    {
        // Arrange
        const string name = "TestTool";
        const string description = "Test description";
        var factory = new TestToolFactory();
        
        // Act
        var toolType = new TestToolType(id, name, description, factory);
        
        // Assert
        toolType.Id.ShouldBe(id);
    }
}