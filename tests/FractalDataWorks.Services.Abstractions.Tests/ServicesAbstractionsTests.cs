using System.Reflection;

namespace FractalDataWorks.Services.Abstractions.Tests;

/// <summary>
/// Tests for Services Abstractions interface contracts and assembly structure.
/// </summary>
/// <remarks>
/// These tests verify the interface definitions, namespaces, and assembly structure
/// of the Services Abstractions library to ensure proper API contracts.
/// </remarks>
public sealed class ServicesAbstractionsTests
{
    [Fact]
    public void AssemblyContainsExpectedInterfaces()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(IFdwService));

        // Act
        var publicTypes = assembly?.GetExportedTypes().Where(t => t.IsInterface).ToList();

        // Assert
        assembly.ShouldNotBeNull();
        publicTypes.ShouldNotBeNull();
        publicTypes.Count.ShouldBeGreaterThan(0);
        
        // Verify core interfaces exist
        publicTypes.ShouldContain(t => t.Name == nameof(IFdwService));
        publicTypes.ShouldContain(t => t.Name == nameof(IFdwTool));
        publicTypes.ShouldContain(t => t.Name == nameof(ICommand));
        publicTypes.ShouldContain(t => t.Name == nameof(IServiceFactory));
        publicTypes.ShouldContain(t => t.Name == nameof(IToolFactory));
        publicTypes.ShouldContain(t => t.Name == nameof(IFdwServiceProvider));
    }

    [Fact]
    public void IFdwServiceHasExpectedProperties()
    {
        // Arrange
        var interfaceType = typeof(IFdwService);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        properties.Length.ShouldBe(3);
        properties.ShouldContain(p => p.Name == "Id" && p.PropertyType == typeof(string));
        properties.ShouldContain(p => p.Name == "ServiceType" && p.PropertyType == typeof(string));
        properties.ShouldContain(p => p.Name == "IsAvailable" && p.PropertyType == typeof(bool));
        
        // All properties should be readable
        properties.ShouldAllBe(p => p.CanRead);
    }

    [Fact]
    public void IFdwToolHasExpectedProperties()
    {
        // Arrange
        var interfaceType = typeof(IFdwTool);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        properties.Length.ShouldBe(3);
        properties.ShouldContain(p => p.Name == "Id" && p.PropertyType == typeof(string));
        properties.ShouldContain(p => p.Name == "Name" && p.PropertyType == typeof(string));
        properties.ShouldContain(p => p.Name == "Version" && p.PropertyType == typeof(string));
        
        // All properties should be readable
        properties.ShouldAllBe(p => p.CanRead);
    }

    [Fact]
    public void ICommandHasExpectedPropertiesAndMethods()
    {
        // Arrange
        var interfaceType = typeof(ICommand);

        // Act
        var properties = interfaceType.GetProperties();
        var methods = interfaceType.GetMethods().Where(m => !m.IsSpecialName).ToArray();

        // Assert
        properties.Length.ShouldBe(4);
        properties.ShouldContain(p => p.Name == "CommandId" && p.PropertyType == typeof(Guid));
        properties.ShouldContain(p => p.Name == "CorrelationId" && p.PropertyType == typeof(Guid));
        properties.ShouldContain(p => p.Name == "Timestamp" && p.PropertyType == typeof(DateTimeOffset));
        properties.ShouldContain(p => p.Name == "Configuration" && p.PropertyType == typeof(IFdwConfiguration));
        
        methods.Length.ShouldBe(1);
        methods.ShouldContain(m => m.Name == "Validate");
    }

    [Fact]
    public void ICommandGenericExtendsICommand()
    {
        // Arrange
        var genericInterfaceType = typeof(ICommand<>);

        // Act
        var baseInterfaces = genericInterfaceType.GetInterfaces();

        // Assert
        baseInterfaces.ShouldContain(typeof(ICommand));
        
        // Should have Payload property
        var properties = genericInterfaceType.GetProperties();
        properties.ShouldContain(p => p.Name == "Payload");
    }

    [Theory]
    [InlineData(typeof(IServiceFactory), "Create")]
    [InlineData(typeof(IServiceFactory<>), "Create")]
    [InlineData(typeof(IToolFactory), "Create")]
    [InlineData(typeof(IToolFactory<>), "Create")]
    [InlineData(typeof(IFdwServiceProvider), "Get")]
    [InlineData(typeof(IFdwServiceProvider<>), "Get")]
    public void FactoryAndProviderInterfacesHaveExpectedMethods(Type interfaceType, string expectedMethodName)
    {
        // Arrange & Act
        var methods = interfaceType.GetMethods();

        // Assert
        methods.ShouldContain(m => m.Name == expectedMethodName);
    }

    [Fact]
    public void DataCommandInterfacesExistInCorrectNamespace()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(IFdwService));

        // Act
        var dataCommandTypes = assembly?.GetExportedTypes()
            .Where(t => t.IsInterface && t.Name.Contains("DataCommand"))
            .ToList();

        // Assert
        dataCommandTypes.ShouldNotBeNull();
        dataCommandTypes.Count.ShouldBe(2);
        
        var dataCommandInterface = dataCommandTypes.Single(t => !t.IsGenericType);
        var genericDataCommandInterface = dataCommandTypes.Single(t => t.IsGenericType);
        
        dataCommandInterface.Name.ShouldBe("IDataCommand");
        genericDataCommandInterface.Name.ShouldBe("IDataCommand`1");
        
        // Both should be in the correct namespace
        dataCommandInterface.Namespace.ShouldBe("FractalDataWorks.Services.Data");
        genericDataCommandInterface.Namespace.ShouldBe("FractalDataWorks.Services.Data");
    }

    [Fact]
    public void InterfacesFromDataProviderAbstractionsNamespaceExist()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(IFdwService));

        // Act
        var dataProviderTypes = assembly?.GetExportedTypes()
            .Where(t => t.IsInterface && t.Namespace?.Contains("DataProvider.Abstractions") == true)
            .ToList();

        // Assert
        dataProviderTypes.ShouldNotBeNull();
        dataProviderTypes.Count.ShouldBeGreaterThan(0);
        
        // Should contain expected interfaces
        var typeNames = dataProviderTypes.Select(t => t.Name).ToList();
        typeNames.ShouldContain("ICommandBuilder`1");
        typeNames.ShouldContain("ICommandResult");
        typeNames.ShouldContain("ICommandTypeMetrics");
    }

    [Fact]
    public void AllInterfacesArePublic()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(IFdwService));

        // Act
        var publicInterfaces = assembly?.GetExportedTypes().Where(t => t.IsInterface).ToList();

        // Assert
        publicInterfaces.ShouldNotBeNull();
        publicInterfaces.ShouldAllBe(t => t.IsPublic, "All interfaces should be public");
    }

    [Fact]
    public void ValidatorInterfaceExistsInCorrectNamespace()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(IFdwService));

        // Act
        var validatorInterface = assembly?.GetExportedTypes()
            .FirstOrDefault(t => t.IsInterface && t.Name == "IFdwValidator`1");

        // Assert
        validatorInterface.ShouldNotBeNull();
        validatorInterface.Namespace.ShouldBe("FractalDataWorks.Validation");
        validatorInterface.IsGenericTypeDefinition.ShouldBeTrue();
    }

    [Fact]
    public void ServiceInterfacesHaveCorrectGenericConstraints()
    {
        // Arrange & Act
        var serviceInterfaceGeneric = typeof(IFdwService<>);
        var constraints = serviceInterfaceGeneric.GetGenericArguments()[0].GetGenericParameterConstraints();

        // Assert
        constraints.ShouldContain(typeof(ICommand));
    }

    [Fact]
    public void AssemblyHasCorrectTargetFramework()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(IFdwService));

        // Act
        var targetFrameworkAttribute = assembly?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();

        // Assert
        assembly.ShouldNotBeNull();
        targetFrameworkAttribute.ShouldNotBeNull();
        targetFrameworkAttribute.FrameworkName.ShouldStartWith(".NETCoreApp,Version=v");
    }
}