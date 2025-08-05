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
    private readonly ClassBuilderState _state = new();

    /// <summary>
    /// Sets the name of the class.
    /// </summary>
    /// <param name="name">The class name.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithName(string name)
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.Name = name ?? throw new ArgumentNullException(nameof(name));
        return newBuilder;
    }

    /// <summary>
    /// Sets the access modifier of the class.
    /// </summary>
    /// <param name="access">The access modifier.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithAccess(AccessModifier access)
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.Access = access;
        return newBuilder;
    }

    /// <summary>
    /// Sets the base class.
    /// </summary>
    /// <param name="baseClass">The base class name.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithBaseClass(string baseClass)
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.BaseClass = baseClass ?? throw new ArgumentNullException(nameof(baseClass));
        return newBuilder;
    }

    /// <summary>
    /// Adds an interface implementation.
    /// </summary>
    /// <param name="interfaceName">The interface name.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder ImplementsInterface(string interfaceName)
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.Interfaces.Add(interfaceName ?? throw new ArgumentNullException(nameof(interfaceName)));
        return newBuilder;
    }

    /// <summary>
    /// Makes the class abstract.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AsAbstract()
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.IsAbstract = true;
        return newBuilder;
    }

    /// <summary>
    /// Makes the class sealed.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AsSealed()
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.IsSealed = true;
        return newBuilder;
    }

    /// <summary>
    /// Makes the class static.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AsStatic()
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.IsStatic = true;
        return newBuilder;
    }

    /// <summary>
    /// Makes the class partial.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AsPartial()
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.IsPartial = true;
        return newBuilder;
    }

    /// <summary>
    /// Adds a method to the class.
    /// </summary>
    /// <param name="method">The method definition.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddMethod(MethodDefinition method)
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.Methods.Add(method ?? throw new ArgumentNullException(nameof(method)));
        return newBuilder;
    }

    /// <summary>
    /// Adds a property to the class.
    /// </summary>
    /// <param name="property">The property definition.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddProperty(PropertyDefinition property)
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.Properties.Add(property ?? throw new ArgumentNullException(nameof(property)));
        return newBuilder;
    }

    /// <summary>
    /// Adds a field to the class.
    /// </summary>
    /// <param name="field">The field definition.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddField(FieldDefinition field)
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.Fields.Add(field ?? throw new ArgumentNullException(nameof(field)));
        return newBuilder;
    }

    /// <summary>
    /// Adds an attribute to the class.
    /// </summary>
    /// <param name="attribute">The attribute definition.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder AddAttribute(AttributeDefinition attribute)
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.Attributes.Add(attribute ?? throw new ArgumentNullException(nameof(attribute)));
        return newBuilder;
    }

    /// <summary>
    /// Sets the documentation for the class.
    /// </summary>
    /// <param name="documentation">The documentation text.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithDocumentation(string documentation)
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        newBuilder._state.Documentation = documentation;
        return newBuilder;
    }

    /// <summary>
    /// Adds generic type parameters to the class.
    /// </summary>
    /// <param name="parameters">The generic parameters.</param>
    /// <returns>A new builder instance.</returns>
    public ClassBuilder WithGenericParameters(params GenericParameterDefinition[] parameters)
    {
        var newBuilder = new ClassBuilder();
        newBuilder._state.CopyFrom(_state);
        foreach (var param in parameters ?? throw new ArgumentNullException(nameof(parameters)))
        {
            newBuilder._state.GenericParameters.Add(param);
        }
        return newBuilder;
    }

    /// <inheritdoc/>
    public bool IsValid => !string.IsNullOrEmpty(_state.Name);

    /// <inheritdoc/>
    public string[] GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(_state.Name))
        {
            errors.Add("Class name is required");
        }

        if (_state.IsAbstract && _state.IsSealed)
        {
            errors.Add("Class cannot be both abstract and sealed");
        }

        if (_state.IsStatic && (_state.IsAbstract || _state.IsSealed))
        {
            errors.Add("Static class cannot be abstract or sealed");
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

        return new ClassDefinition
        {
            Name = _state.Name,
            Access = _state.Access,
            BaseClass = _state.BaseClass,
            Interfaces = _state.Interfaces.ToArray(),
            IsAbstract = _state.IsAbstract,
            IsSealed = _state.IsSealed,
            IsStatic = _state.IsStatic,
            IsPartial = _state.IsPartial,
            GenericParameters = _state.GenericParameters.ToArray(),
            Methods = _state.Methods.ToArray(),
            Properties = _state.Properties.ToArray(),
            Fields = _state.Fields.ToArray(),
            Attributes = _state.Attributes.ToArray(),
            Documentation = _state.Documentation
        };
    }
}

/// <summary>
/// Internal state holder for ClassBuilder.
/// This keeps the builder immutable while allowing efficient state copying.
/// </summary>
internal sealed class ClassBuilderState
{
    public string? Name { get; set; }
    public AccessModifier Access { get; set; } = AccessModifier.None;
    public string? BaseClass { get; set; }
    public List<string> Interfaces { get; set; } = new(StringComparer.Ordinal);
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }
    public bool IsStatic { get; set; }
    public bool IsPartial { get; set; }
    public List<GenericParameterDefinition> GenericParameters { get; set; } = new();
    public List<MethodDefinition> Methods { get; set; } = new();
    public List<PropertyDefinition> Properties { get; set; } = new();
    public List<FieldDefinition> Fields { get; set; } = new();
    public List<AttributeDefinition> Attributes { get; set; } = new();
    public string? Documentation { get; set; }

    public void CopyFrom(ClassBuilderState other)
    {
        Name = other.Name;
        Access = other.Access;
        BaseClass = other.BaseClass;
        Interfaces = new List<string>(other.Interfaces);
        IsAbstract = other.IsAbstract;
        IsSealed = other.IsSealed;
        IsStatic = other.IsStatic;
        IsPartial = other.IsPartial;
        GenericParameters = new List<GenericParameterDefinition>(other.GenericParameters);
        Methods = new List<MethodDefinition>(other.Methods);
        Properties = new List<PropertyDefinition>(other.Properties);
        Fields = new List<FieldDefinition>(other.Fields);
        Attributes = new List<AttributeDefinition>(other.Attributes);
        Documentation = other.Documentation;
    }
}