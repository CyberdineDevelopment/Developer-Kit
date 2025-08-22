using System.Threading.Tasks;
using FractalDataWorks.Tools.Tests.TestImplementations;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tools.Tests;

/// <summary>
/// Tests for the ToolTypeFactoryBase{TTool, TConfiguration} abstract class.
/// </summary>
public sealed class ToolTypeFactoryBaseSimpleTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetProperties()
    {
        // Arrange & Act
        var factory = new TestToolTypeFactory(999, "FactoryTest", "A test factory for testing");
        
        // Assert
        factory.Id.ShouldBe(999);
        factory.Name.ShouldBe("FactoryTest");
        factory.Description.ShouldBe("A test factory for testing");
    }
    
    [Fact]
    public void Create_WithValidConfiguration_ShouldCallImplementationAndReturnTool()
    {
        // Arrange
        var factory = new TestToolTypeFactory(1, "TestFactory", "Test factory");
        var configuration = new TestConfiguration { Name = "TestConfig", Timeout = 60 };
        
        // Act
        var result = factory.Create(configuration);
        
        // Assert
        factory.CreateCalled.ShouldBeTrue();
        factory.LastConfiguration.ShouldBeSameAs(configuration);
        result.ShouldBeOfType<TestTool>();
        
        var tool = (TestTool)result;
        tool.Name.ShouldBe("Created from TestConfig");
    }
    
    [Fact]
    public async Task GetTool_WithConfigurationName_ShouldCallImplementationAndReturnTool()
    {
        // Arrange
        var factory = new TestToolTypeFactory(1, "TestFactory", "Test factory");
        const string configName = "MyConfiguration";
        
        // Act
        var result = await factory.GetTool(configName);
        
        // Assert
        factory.GetToolByNameCalled.ShouldBeTrue();
        factory.LastConfigurationName.ShouldBe(configName);
        result.ShouldNotBeNull();
        result.Name.ShouldBe(configName);
    }
    
    [Fact]
    public async Task GetTool_WithConfigurationId_ShouldCallImplementationAndReturnTool()
    {
        // Arrange
        var factory = new TestToolTypeFactory(1, "TestFactory", "Test factory");
        const int configId = 42;
        
        // Act
        var result = await factory.GetTool(configId);
        
        // Assert
        factory.GetToolByIdCalled.ShouldBeTrue();
        factory.LastConfigurationId.ShouldBe(configId);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Config_42");
    }
    
    [Fact]
    public void Create_WithNullConfiguration_ShouldPassNullToImplementation()
    {
        // Arrange
        var factory = new TestToolTypeFactory(1, "NullTest", "Testing null configuration");
        
        // Act
        var result = factory.Create(null!);
        
        // Assert
        factory.CreateCalled.ShouldBeTrue();
        factory.LastConfiguration.ShouldBeNull();
        result.ShouldBeOfType<TestTool>();
    }
}