using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums.Attributes;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for ServiceTypeBase and ServiceTypeFactoryBase classes.
/// </summary>
public class ServiceTypeBaseTests
{
    [Fact]
    public void ServiceTypeBaseHasEnhancedEnumBaseAttribute()
    {
        // Arrange
        var serviceTypeBaseType = typeof(ServiceTypeBase<,>);

        // Act
        var attribute = serviceTypeBaseType.GetCustomAttribute<EnhancedEnumBaseAttribute>();

        // Assert
        attribute.ShouldNotBeNull($"ServiceTypeBase should have EnhancedEnumBaseAttribute");
        attribute.CollectionName.ShouldBe("ServiceTypes", $"Collection name should be 'ServiceTypes'");
        attribute.ReturnType.ShouldBe("IServiceFactory<IFdwService, IFdwConfiguration>", $"Return type should match expected interface");
        attribute.ReturnTypeNamespace.ShouldBe("FractalDataWorks.Services", $"Return type namespace should be correct");
    }

    [Fact]
    public void ServiceTypeBaseHasCorrectGenericConstraints()
    {
        // Arrange
        var serviceTypeBaseType = typeof(ServiceTypeBase<,>);

        // Act
        var genericParams = serviceTypeBaseType.GetGenericArguments();
        var serviceParam = genericParams[0];
        var configParam = genericParams[1];

        // Assert
        genericParams.Length.ShouldBe(2, $"ServiceTypeBase should have 2 generic parameters");
        
        // Check TService constraints
        serviceParam.Name.ShouldBe("TService", $"First parameter should be named TService");
        serviceParam.GenericParameterAttributes.ShouldHaveFlag(GenericParameterAttributes.ReferenceTypeConstraint, 
            $"TService should have class constraint");
        var serviceConstraints = serviceParam.GetGenericParameterConstraints();
        serviceConstraints.ShouldContain(typeof(IFdwService), $"TService should be constrained to IFdwService");

        // Check TConfiguration constraints
        configParam.Name.ShouldBe("TConfiguration", $"Second parameter should be named TConfiguration");
        configParam.GenericParameterAttributes.ShouldHaveFlag(GenericParameterAttributes.ReferenceTypeConstraint,
            $"TConfiguration should have class constraint");
        var configConstraints = configParam.GetGenericParameterConstraints();
        configConstraints.ShouldContain(typeof(IFdwConfiguration), $"TConfiguration should be constrained to IFdwConfiguration");
    }

    [Fact]
    public void ServiceTypeBaseInheritsFromServiceTypeFactoryBase()
    {
        // Arrange
        var serviceTypeBaseType = typeof(ServiceTypeBase<,>);

        // Act
        var baseType = serviceTypeBaseType.BaseType;

        // Assert
        baseType.ShouldNotBeNull($"ServiceTypeBase should have a base type");
        baseType.IsGenericType.ShouldBeTrue($"Base type should be generic");
        baseType.GetGenericTypeDefinition().ShouldBe(typeof(ServiceTypeFactoryBase<,>),
            $"ServiceTypeBase should inherit from ServiceTypeFactoryBase");
    }

    [Fact]
    public void ServiceTypeFactoryBaseHasNoEnhancedEnumAttributes()
    {
        // Arrange
        var factoryBaseType = typeof(ServiceTypeFactoryBase<,>);

        // Act
        var attribute = factoryBaseType.GetCustomAttribute<EnhancedEnumBaseAttribute>();

        // Assert
        attribute.ShouldBeNull($"ServiceTypeFactoryBase should NOT have EnhancedEnumBaseAttribute");
    }

    [Fact]
    public void ServiceTypeFactoryBaseHasRequiredProperties()
    {
        // Arrange
        var factoryBaseType = typeof(ServiceTypeFactoryBase<,>);

        // Act
        var idProperty = factoryBaseType.GetProperty("Id");
        var nameProperty = factoryBaseType.GetProperty("Name");
        var descriptionProperty = factoryBaseType.GetProperty("Description");

        // Assert
        idProperty.ShouldNotBeNull($"ServiceTypeFactoryBase should have Id property");
        idProperty.PropertyType.ShouldBe(typeof(int), $"Id should be of type int");
        idProperty.CanWrite.ShouldBeFalse($"Id should be read-only");

        nameProperty.ShouldNotBeNull($"ServiceTypeFactoryBase should have Name property");
        nameProperty.PropertyType.ShouldBe(typeof(string), $"Name should be of type string");
        nameProperty.CanWrite.ShouldBeFalse($"Name should be read-only");

        descriptionProperty.ShouldNotBeNull($"ServiceTypeFactoryBase should have Description property");
        descriptionProperty.PropertyType.ShouldBe(typeof(string), $"Description should be of type string");
        descriptionProperty.CanWrite.ShouldBeFalse($"Description should be read-only");
    }

    [Fact]
    public void ServiceTypeFactoryBaseHasRequiredAbstractMethods()
    {
        // Arrange
        var factoryBaseType = typeof(ServiceTypeFactoryBase<,>);

        // Act
        var createMethod = factoryBaseType.GetMethod("Create", new[] { typeof(IFdwConfiguration).MakeGenericType(factoryBaseType.GetGenericArguments()[1]) });
        var getServiceByNameMethod = factoryBaseType.GetMethod("GetService", new[] { typeof(string) });
        var getServiceByIdMethod = factoryBaseType.GetMethod("GetService", new[] { typeof(int) });

        // Assert
        createMethod.ShouldNotBeNull($"ServiceTypeFactoryBase should have Create method");
        createMethod.IsAbstract.ShouldBeTrue($"Create method should be abstract");
        createMethod.ReturnType.ShouldBe(typeof(object), $"Create should return object");

        getServiceByNameMethod.ShouldNotBeNull($"ServiceTypeFactoryBase should have GetService(string) method");
        getServiceByNameMethod.IsAbstract.ShouldBeTrue($"GetService(string) method should be abstract");

        getServiceByIdMethod.ShouldNotBeNull($"ServiceTypeFactoryBase should have GetService(int) method");
        getServiceByIdMethod.IsAbstract.ShouldBeTrue($"GetService(int) method should be abstract");
    }

    [Fact]
    public void ServiceTypeBaseConstructorCallsBaseCorrectly()
    {
        // This test verifies that concrete implementations can be created properly
        // Arrange & Act
        var concreteType = new TestServiceType(1, "Test", "Test Service");

        // Assert
        concreteType.Id.ShouldBe(1, $"Id should be set from constructor");
        concreteType.Name.ShouldBe("Test", $"Name should be set from constructor");
        concreteType.Description.ShouldBe("Test Service", $"Description should be set from constructor");
    }

    // Test implementation to verify instantiation
    private class TestServiceType : ServiceTypeBase<IFdwService, IFdwConfiguration>
    {
        public TestServiceType(int id, string name, string description) 
            : base(id, name, description)
        {
        }

        public override IServiceFactory<IFdwService, IFdwConfiguration> CreateTypedFactory()
        {
            throw new NotImplementedException("Test implementation");
        }
    }
}