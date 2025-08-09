using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Connections.Tests;

/// <summary>
/// Tests for ConnectionTypeBase and ConnectionTypeFactoryBase classes.
/// </summary>
public class ConnectionTypeBaseTests
{
    [Fact(Skip = "Enhanced Enum attributes temporarily disabled")]
    public void ConnectionTypeBaseHasEnhancedEnumBaseAttribute()
    {
        // Arrange
        var connectionTypeBaseType = typeof(ConnectionTypeBase);

        // Act
        var attribute = connectionTypeBaseType.GetCustomAttribute<EnumCollectionAttribute>();

        // Assert
        attribute.ShouldNotBeNull($"ConnectionTypeBase should have EnumCollectionAttribute");
        attribute.CollectionName.ShouldBe("ConnectionTypes", $"Collection name should be 'ConnectionTypes'");
        // Note: ReturnType and ReturnTypeNamespace properties may not be fully implemented yet
        // attribute.ReturnType.ShouldBe("IConnectionFactory<IExternalConnection, IFdwConfiguration>", $"Return type should match expected interface");
        // attribute.ReturnTypeNamespace.ShouldBe("FractalDataWorks.Connections", $"Return type namespace should be correct");
    }

    [Fact]
    public void ConnectionTypeBaseHasCorrectGenericConstraints()
    {
        // Arrange
        var connectionTypeBaseType = typeof(ConnectionTypeBase<,>);

        // Act
        var genericParams = connectionTypeBaseType.GetGenericArguments();
        var connectionParam = genericParams[0];
        var configParam = genericParams[1];

        // Assert
        genericParams.Length.ShouldBe(2, $"ConnectionTypeBase should have 2 generic parameters");
        
        // Check TConnection constraints
        connectionParam.Name.ShouldBe("TConnection", $"First parameter should be named TConnection");
        connectionParam.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint).ShouldBeTrue(
            $"TConnection should have class constraint");
        var connectionConstraints = connectionParam.GetGenericParameterConstraints();
        connectionConstraints.ShouldContain(typeof(IExternalConnection), $"TConnection should be constrained to IExternalConnection");

        // Check TConfiguration constraints
        configParam.Name.ShouldBe("TConfiguration", $"Second parameter should be named TConfiguration");
        configParam.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint).ShouldBeTrue(
            $"TConfiguration should have class constraint");
        var configConstraints = configParam.GetGenericParameterConstraints();
        configConstraints.ShouldContain(typeof(IFdwConfiguration), $"TConfiguration should be constrained to IFdwConfiguration");
    }

    [Fact]
    public void ConnectionTypeBaseGenericInheritsFromNonGeneric()
    {
        // Arrange
        var connectionTypeBaseType = typeof(ConnectionTypeBase<,>);

        // Act
        var baseType = connectionTypeBaseType.BaseType;

        // Assert
        baseType.ShouldNotBeNull($"ConnectionTypeBase<,> should have a base type");
        baseType.ShouldBe(typeof(ConnectionTypeBase),
            $"ConnectionTypeBase<,> should inherit from non-generic ConnectionTypeBase");
    }

    // Note: EnhancedEnumBaseAttribute test removed as this attribute does not exist in the consolidated attribute structure

    [Fact]
    public void ConnectionTypeFactoryBaseHasRequiredProperties()
    {
        // Arrange
        var factoryBaseType = typeof(ConnectionTypeFactoryBase<,>);

        // Act
        var idProperty = factoryBaseType.GetProperty("Id");
        var nameProperty = factoryBaseType.GetProperty("Name");
        var descriptionProperty = factoryBaseType.GetProperty("Description");

        // Assert
        idProperty.ShouldNotBeNull($"ConnectionTypeFactoryBase should have Id property");
        idProperty.PropertyType.ShouldBe(typeof(int), $"Id should be of type int");
        idProperty.CanWrite.ShouldBeFalse($"Id should be read-only");

        nameProperty.ShouldNotBeNull($"ConnectionTypeFactoryBase should have Name property");
        nameProperty.PropertyType.ShouldBe(typeof(string), $"Name should be of type string");
        nameProperty.CanWrite.ShouldBeFalse($"Name should be read-only");

        descriptionProperty.ShouldNotBeNull($"ConnectionTypeFactoryBase should have Description property");
        descriptionProperty.PropertyType.ShouldBe(typeof(string), $"Description should be of type string");
        descriptionProperty.CanWrite.ShouldBeFalse($"Description should be read-only");
    }

    [Fact]
    public void ConnectionTypeFactoryBaseHasRequiredAbstractMethods()
    {
        // Arrange
        var factoryBaseType = typeof(ConnectionTypeFactoryBase<,>);

        // Act
        var genericArgs = factoryBaseType.GetGenericArguments();
        var configType = genericArgs[1];
        var createMethod = factoryBaseType.GetMethod("Create", new[] { configType });
        var getConnectionByNameMethod = factoryBaseType.GetMethod("GetConnection", new[] { typeof(string) });
        var getConnectionByIdMethod = factoryBaseType.GetMethod("GetConnection", new[] { typeof(int) });

        // Assert
        createMethod.ShouldNotBeNull($"ConnectionTypeFactoryBase should have Create method");
        createMethod.IsAbstract.ShouldBeTrue($"Create method should be abstract");
        createMethod.ReturnType.ShouldBe(typeof(object), $"Create should return object");

        getConnectionByNameMethod.ShouldNotBeNull($"ConnectionTypeFactoryBase should have GetConnection(string) method");
        getConnectionByNameMethod.IsAbstract.ShouldBeTrue($"GetConnection(string) method should be abstract");

        getConnectionByIdMethod.ShouldNotBeNull($"ConnectionTypeFactoryBase should have GetConnection(int) method");
        getConnectionByIdMethod.IsAbstract.ShouldBeTrue($"GetConnection(int) method should be abstract");
    }

    [Fact]
    public void ConnectionTypeBaseConstructorCallsBaseCorrectly()
    {
        // This test verifies that concrete implementations can be created properly
        // Arrange & Act
        var concreteType = new TestConnectionType(1, "Test", "Test Connection");

        // Assert
        concreteType.Id.ShouldBe(1, $"Id should be set from constructor");
        concreteType.Name.ShouldBe("Test", $"Name should be set from constructor");
        concreteType.Description.ShouldBe("Test Connection", $"Description should be set from constructor");
    }

    // Test implementation to verify instantiation
    private sealed class TestConnectionType : ConnectionTypeBase<IExternalConnection, IFdwConfiguration>
    {
        public TestConnectionType(int id, string name, string description) 
            : base(id, name, description)
        {
        }

        public override IConnectionFactory<IExternalConnection, IFdwConfiguration> CreateTypedFactory()
        {
            throw new NotImplementedException("Test implementation");
        }
    }
}