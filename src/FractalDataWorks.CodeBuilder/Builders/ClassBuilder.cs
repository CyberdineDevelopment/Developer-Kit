using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Definitions;
using FractalDataWorks.CodeBuilder.Types;

namespace FractalDataWorks.CodeBuilder.Builders;

/// <summary>
/// True builder pattern implementation for class definitions.
/// Builds immutable ClassDefinition products following the TRUE builder pattern.
/// Each method returns a new builder instance, keeping builders immutable.
/// </summary>
public sealed class ClassBuilder : IAstBuilder<ClassDefinition>, IValidatableBuilder
{
    private readonly ClassBuilderState _state;

    /// <summary>
    /// Initializes a new instance of ClassBuilder.
    /// </summary>
    public ClassBuilder()
    {
        _state = new ClassBuilderState();
    }

    /// <summary>
    /// Initializes a new instance of ClassBuilder with the given state.
    /// </summary>
    /// <param name="state">The builder state.</param>
    private ClassBuilder(ClassBuilderState state)
    {
        _state = state;
    }

    /// <summary>
    /// Sets the name of the class.
    /// </summary>
    /// <param name="name">The class name.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Class name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        newState.Name = name;
        return new ClassBuilder(newState);
    }

    /// <summary>
    /// Sets the access modifier of the class.
    /// </summary>
    /// <param name="access">The access modifier.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithAccess(AccessModifier access)
    {
        var newState = _state.Clone();
        newState.Access = access ?? throw new ArgumentNullException(nameof(access));
        return new ClassBuilder(newState);
    }

    /// <summary>
    /// Adds a modifier to the class.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithModifier(Modifiers modifier)
    {
        var newState = _state.Clone();
        newState.Modifiers = newState.Modifiers.AddModifier(modifier);
        return new ClassBuilder(newState);
    }

    /// <summary>
    /// Sets multiple modifiers for the class.
    /// </summary>
    /// <param name="modifiers">The modifiers to set.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithModifiers(Modifiers modifiers)
    {
        var newState = _state.Clone();
        newState.Modifiers = modifiers;
        return new ClassBuilder(newState);
    }

    /// <summary>
    /// Sets the base class.
    /// </summary>
    /// <param name="baseClass">The base class name.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithBaseClass(string baseClass)
    {
        if (string.IsNullOrWhiteSpace(baseClass))
            throw new ArgumentException("Base class name cannot be null or empty.", nameof(baseClass));

        var newState = _state.Clone();
        newState.BaseClass = baseClass;
        // Also add to base types if not already present
        if (!newState.BaseTypes.Contains(baseClass))
        {
            newState.BaseTypes.Insert(0, baseClass); // Base class should be first
        }
        return new ClassBuilder(newState);
    }

    /// <summary>
    /// Adds a base type (interface or base class).
    /// </summary>
    /// <param name="baseType">The base type name.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddBaseType(string baseType)
    {
        if (string.IsNullOrWhiteSpace(baseType))
            throw new ArgumentException("Base type name cannot be null or empty.", nameof(baseType));

        var newState = _state.Clone();
        if (!newState.BaseTypes.Contains(baseType))
        {
            newState.BaseTypes.Add(baseType);
        }
        return new ClassBuilder(newState);
    }

    /// <summary>
    /// Adds an interface implementation.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder ImplementsInterface(string interfaceName)
    {
        return AddBaseType(interfaceName);
    }

    /// <summary>
    /// Makes the class abstract.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AsAbstract()
    {
        return WithModifier(Modifiers.Abstract);
    }

    /// <summary>
    /// Makes the class sealed.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AsSealed()
    {
        return WithModifier(Modifiers.Sealed);
    }

    /// <summary>
    /// Makes the class static.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AsStatic()
    {
        return WithModifier(Modifiers.Static);
    }

    /// <summary>
    /// Makes the class partial.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AsPartial()
    {
        return WithModifier(Modifiers.Partial);
    }

    /// <summary>
    /// Adds a member to the class.
    /// </summary>
    /// <param name="member">The member definition.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddMember(IMemberDefinition member)
    {
        if (member == null)
            throw new ArgumentNullException(nameof(member));

        var newState = _state.Clone();
        newState.Members.Add(member);
        return new ClassBuilder(newState);
    }

    /// <summary>
    /// Adds a method to the class using a method builder configuration.
    /// </summary>
    /// <param name="configure">Configuration action for the method.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddMethod(Action<MethodBuilder> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var methodBuilder = new MethodBuilder();
        configure(methodBuilder);
        var method = methodBuilder.Build();
        return AddMember(method);
    }

    /// <summary>
    /// Adds a property to the class using a property builder configuration.
    /// </summary>
    /// <param name="configure">Configuration action for the property.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddProperty(Action<PropertyBuilder> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var propertyBuilder = new PropertyBuilder();
        configure(propertyBuilder);
        var property = propertyBuilder.Build();
        return AddMember(property);
    }

    /// <summary>
    /// Adds a constructor to the class using a constructor builder configuration.
    /// </summary>
    /// <param name="configure">Configuration action for the constructor.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddConstructor(Action<ConstructorBuilder> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var constructorBuilder = new ConstructorBuilder();
        configure(constructorBuilder);
        var constructor = constructorBuilder.Build();
        return AddMember(constructor);
    }

    /// <summary>
    /// Adds an attribute to the class.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="arguments">Optional attribute arguments.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddAttribute(string name, params object[] arguments)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        var attribute = new AttributeDefinition(name, arguments);
        newState.Attributes.Add(attribute);
        return new ClassBuilder(newState);
    }

    /// <summary>
    /// Sets the XML documentation for the class.
    /// </summary>
    /// <param name="documentation">The documentation text.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithDocumentation(string documentation)
    {
        var newState = _state.Clone();
        newState.XmlDocumentation = documentation;
        return new ClassBuilder(newState);
    }

    /// <summary>
    /// Adds a generic type parameter to the class.
    /// </summary>
    /// <param name="name">The name of the type parameter.</param>
    /// <param name="constraints">Optional constraints for the type parameter.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddGenericParameter(string name, params string[] constraints)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Generic parameter name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        var genericParam = new GenericParameterDefinition(name, constraints);
        newState.GenericParameters.Add(genericParam);
        return new ClassBuilder(newState);
    }

    /// <inheritdoc/>
    public bool IsValid
    {
        get
        {
            var errors = GetValidationErrors();
            return errors.Length == 0;
        }
    }

    /// <inheritdoc/>
    public string[] GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_state.Name))
        {
            errors.Add("Class name is required");
        }

        if (!_state.Modifiers.IsValid())
        {
            errors.Add("Invalid modifier combination");
        }

        if (_state.Modifiers.HasModifier(Modifiers.Abstract) && _state.Modifiers.HasModifier(Modifiers.Sealed))
        {
            errors.Add("Class cannot be both abstract and sealed");
        }

        if (_state.Modifiers.HasModifier(Modifiers.Static) && 
            (_state.Modifiers.HasModifier(Modifiers.Abstract) || _state.Modifiers.HasModifier(Modifiers.Sealed)))
        {
            errors.Add("Static class cannot be abstract or sealed");
        }

        if (_state.Modifiers.HasModifier(Modifiers.Static) && !string.IsNullOrEmpty(_state.BaseClass))
        {
            errors.Add("Static class cannot have a base class");
        }

        return errors.ToArray();
    }

    /// <inheritdoc/>
    public ClassDefinition Build()
    {
        if (!IsValid)
        {
            var errors = GetValidationErrors();
            throw new InvalidOperationException($"Cannot build invalid class: {string.Join(", ", errors)}");
        }

        return new ClassDefinition(_state);
    }
}