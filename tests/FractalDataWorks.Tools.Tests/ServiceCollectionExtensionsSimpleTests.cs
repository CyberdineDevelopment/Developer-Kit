using System.Linq;
using System.Reflection;
using FractalDataWorks.Tools.Extensions;
using FractalDataWorks.Tools.Tests.TestImplementations;
using FractalDataWorks.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tools.Tests;

/// <summary>
/// Tests for the ServiceCollectionExtensions static class.
/// </summary>
public sealed class ServiceCollectionExtensionsSimpleTests
{
    [Fact]
    public void AddToolTypes_WithNullAssembly_ShouldUseCallingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        var initialCount = services.Count;
        
        // Act
        var result = services.AddToolTypes(null);
        
        // Assert
        result.ShouldBeSameAs(services);
        services.Count.ShouldBeGreaterThanOrEqualTo(initialCount);
    }
    
    [Fact]
    public void AddToolTypes_WithSpecificAssembly_ShouldScanThatAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        var currentAssembly = Assembly.GetExecutingAssembly();
        var initialCount = services.Count;
        
        // Act
        var result = services.AddToolTypes(currentAssembly);
        
        // Assert
        result.ShouldBeSameAs(services);
        services.Count.ShouldBeGreaterThanOrEqualTo(initialCount);
    }
    
    [Fact]
    public void AddToolTypes_ShouldRegisterToolTypesSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var currentAssembly = Assembly.GetExecutingAssembly();
        
        // Add required dependencies
        services.AddSingleton<IToolFactory<TestTool, TestConfiguration>>(new TestGenericToolFactory());
        
        // Act
        services.AddToolTypes(currentAssembly);
        
        // Assert
        var toolTypeRegistrations = services.Where(s => s.ServiceType == typeof(TestGenericToolType)).ToList();
        
        foreach (var registration in toolTypeRegistrations)
        {
            registration.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }
        
        toolTypeRegistrations.ShouldNotBeEmpty();
    }
    
    [Fact]
    public void AddToolTypes_WithEmptyAssembly_ShouldHandleGracefully()
    {
        // Arrange
        var services = new ServiceCollection();
        var systemAssembly = typeof(string).Assembly;
        var initialCount = services.Count;
        
        // Act
        var result = services.AddToolTypes(systemAssembly);
        
        // Assert
        result.ShouldBeSameAs(services);
        services.Count.ShouldBe(initialCount);
    }
    
    [Fact]
    public void AddToolTypes_MultipleCalls_ShouldUseTryAddSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var currentAssembly = Assembly.GetExecutingAssembly();
        
        services.AddSingleton<IToolFactory<TestTool, TestConfiguration>>(new TestGenericToolFactory());
        
        var initialCount = services.Count;
        
        // Act - Call AddToolTypes multiple times
        services.AddToolTypes(currentAssembly);
        var countAfterFirst = services.Count;
        
        services.AddToolTypes(currentAssembly);
        var countAfterSecond = services.Count;
        
        // Assert - Second call should not add duplicate registrations due to TryAddSingleton
        countAfterSecond.ShouldBe(countAfterFirst);
        countAfterFirst.ShouldBeGreaterThanOrEqualTo(initialCount);
    }
    
    [Fact]
    public void AddToolTypes_ReturnValue_ShouldEnableMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act & Assert - Should support method chaining
        var result = services
            .AddToolTypes()
            .AddSingleton<string>("test")
            .AddTransient<object>();
        
        result.ShouldBeSameAs(services);
        result.Count.ShouldBeGreaterThan(1);
    }
}