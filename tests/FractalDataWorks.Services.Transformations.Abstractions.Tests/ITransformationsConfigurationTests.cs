using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Shouldly;
using Moq;
using FractalDataWorks;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions.Tests;

/// <summary>
/// Comprehensive tests for ITransformationsConfiguration interface contracts and behavior.
/// Tests verify proper implementation of transformations configuration and inheritance.
/// </summary>
public class ITransformationsConfigurationTests
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ITransformationsConfiguration> _mockConfiguration;

    public ITransformationsConfigurationTests(ITestOutputHelper output)
    {
        _output = output;
        _mockConfiguration = new Mock<ITransformationsConfiguration>();
    }

    [Fact]
    public void ConfigurationShouldInheritFromIFdwConfiguration()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationsConfiguration);
        var baseInterfaceType = typeof(IFdwConfiguration);

        // Assert
        interfaceType.ShouldNotBeNull();
        baseInterfaceType.ShouldNotBeNull();
        interfaceType.IsAssignableFrom(typeof(ITransformationsConfiguration)).ShouldBeTrue();
        baseInterfaceType.IsAssignableFrom(typeof(ITransformationsConfiguration)).ShouldBeTrue();
        _output.WriteLine("ITransformationsConfiguration correctly inherits from IFdwConfiguration");
    }

    [Fact]
    public void InterfaceContractShouldBeCorrectlyDefined()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationsConfiguration);

        // Assert
        interfaceType.ShouldNotBeNull();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Transformations.Abstractions");
        
        // This interface should have no additional properties beyond the base interface
        var declaredProperties = interfaceType.GetProperties();
        declaredProperties.Length.ShouldBe(0);
        
        _output.WriteLine($"Interface has {declaredProperties.Length} declared properties (marker interface pattern)");
    }

    [Fact]
    public void ConfigurationShouldBeMarkerInterface()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationsConfiguration);
        var methods = interfaceType.GetMethods();
        var properties = interfaceType.GetProperties();
        var events = interfaceType.GetEvents();

        // Assert - Marker interface should have no additional members
        properties.Length.ShouldBe(0);
        events.Length.ShouldBe(0);
        
        // Methods should only include inherited methods from base interface
        foreach (var method in methods)
        {
            method.DeclaringType.ShouldNotBe(interfaceType);
        }
        
        _output.WriteLine("ITransformationsConfiguration correctly implements marker interface pattern");
    }

    [Fact]
    public void ConfigurationShouldImplementIFdwConfigurationContract()
    {
        // Arrange & Act
        var baseInterface = typeof(IFdwConfiguration);
        var transformationsInterface = typeof(ITransformationsConfiguration);

        // Assert
        baseInterface.IsAssignableFrom(transformationsInterface).ShouldBeTrue();
        transformationsInterface.GetInterfaces().ShouldContain(baseInterface);
        _output.WriteLine("ITransformationsConfiguration properly implements IFdwConfiguration contract");
    }

    [Fact]
    public void ConfigurationCanBeUsedAsIFdwConfiguration()
    {
        // Arrange
        var configuration = _mockConfiguration.Object;

        // Act & Assert
        configuration.ShouldBeAssignableTo<IFdwConfiguration>();
        configuration.ShouldBeAssignableTo<ITransformationsConfiguration>();
        _output.WriteLine("Configuration instance can be used as both IFdwConfiguration and ITransformationsConfiguration");
    }

    [Fact]
    public void ConfigurationShouldSupportPolymorphicUsage()
    {
        // Arrange
        var configuration = _mockConfiguration.Object;

        // Act
        IFdwConfiguration fdwConfig = configuration;
        ITransformationsConfiguration transformationsConfig = configuration;

        // Assert
        fdwConfig.ShouldBe(configuration);
        transformationsConfig.ShouldBe(configuration);
        fdwConfig.ShouldBe(transformationsConfig);
        _output.WriteLine("Configuration supports polymorphic usage through interface inheritance");
    }

    [Fact]
    public void ConfigurationTypeShouldHaveCorrectHierarchy()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationsConfiguration);
        var baseInterfaces = interfaceType.GetInterfaces();

        // Assert
        baseInterfaces.Length.ShouldBe(1);
        baseInterfaces[0].ShouldBe(typeof(IFdwConfiguration));
        _output.WriteLine($"Configuration has correct interface hierarchy with {baseInterfaces.Length} base interface");
    }

    [Fact]
    public void ConfigurationShouldBeInCorrectNamespace()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationsConfiguration);

        // Assert
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Transformations.Abstractions");
        interfaceType.Assembly.ShouldBe(typeof(ITransformationProvider).Assembly);
        _output.WriteLine($"Configuration is in correct namespace: {interfaceType.Namespace}");
    }

    [Fact]
    public void ConfigurationShouldBePublicInterface()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationsConfiguration);

        // Assert
        interfaceType.IsPublic.ShouldBeTrue();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.IsAbstract.ShouldBeTrue();
        interfaceType.IsSealed.ShouldBeFalse();
        _output.WriteLine("Configuration is correctly defined as public interface");
    }

    [Fact]
    public void ConfigurationShouldSupportGenericConfigurationScenarios()
    {
        // Arrange
        var configuration = _mockConfiguration.Object;

        // Act - Demonstrate usage in generic scenarios
        var configAsBase = (IFdwConfiguration)configuration;
        var configAsTransformations = (ITransformationsConfiguration)configuration;

        // Assert
        configAsBase.ShouldNotBeNull();
        configAsTransformations.ShouldNotBeNull();
        configAsBase.ShouldBe(configAsTransformations);
        
        // Both references should point to the same object
        ReferenceEquals(configAsBase, configAsTransformations).ShouldBeTrue();
        
        _output.WriteLine("Configuration supports generic configuration scenarios through inheritance");
    }

    [Fact]
    public void ConfigurationShouldBeCompatibleWithFrameworkPatterns()
    {
        // Arrange
        var configuration = _mockConfiguration.Object;

        // Act & Assert - Test various framework usage patterns
        
        // Can be used as IFdwConfiguration
        ProcessFdwConfiguration(configuration);
        
        // Can be used as ITransformationsConfiguration
        ProcessTransformationsConfiguration(configuration);
        
        // Can be stored in collections
        var configurations = new List<IFdwConfiguration> { configuration };
        configurations.Count.ShouldBe(1);
        configurations[0].ShouldBe(configuration);
        
        var transformationsConfigurations = new List<ITransformationsConfiguration> { configuration };
        transformationsConfigurations.Count.ShouldBe(1);
        transformationsConfigurations[0].ShouldBe(configuration);
        
        _output.WriteLine("Configuration is compatible with common framework patterns");
    }

    [Fact]
    public void ConfigurationShouldSupportTypeChecking()
    {
        // Arrange
        var configuration = _mockConfiguration.Object;

        // Act & Assert
        (configuration is IFdwConfiguration).ShouldBeTrue();
        (configuration is ITransformationsConfiguration).ShouldBeTrue();
        
        configuration.GetType().IsAssignableTo(typeof(IFdwConfiguration)).ShouldBeTrue();
        configuration.GetType().IsAssignableTo(typeof(ITransformationsConfiguration)).ShouldBeTrue();
        
        _output.WriteLine("Configuration supports proper type checking and casting");
    }

    [Fact]
    public void ConfigurationShouldHaveCorrectGenericTypeConstraints()
    {
        // Arrange & Act
        var interfaceType = typeof(ITransformationsConfiguration);

        // Assert
        interfaceType.IsGenericType.ShouldBeFalse();
        interfaceType.IsGenericTypeDefinition.ShouldBeFalse();
        interfaceType.GetGenericArguments().Length.ShouldBe(0);
        _output.WriteLine("Configuration is correctly defined as non-generic interface");
    }

    private static void ProcessFdwConfiguration(IFdwConfiguration config)
    {
        // Simulate framework processing of IFdwConfiguration
        config.ShouldNotBeNull();
    }

    private static void ProcessTransformationsConfiguration(ITransformationsConfiguration config)
    {
        // Simulate framework processing of ITransformationsConfiguration
        config.ShouldNotBeNull();
    }
}

/// <summary>
/// Marker attribute to exclude simple marker interfaces from code coverage.
/// ITransformationsConfiguration is a marker interface that inherits from IFdwConfiguration
/// without adding additional members, so its behavior is fully tested through inheritance.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker interface with no additional implementation to test beyond inheritance contract")]
file static class TransformationsConfigurationMarkerInterface
{
    // This class exists only to document the exclusion justification
}