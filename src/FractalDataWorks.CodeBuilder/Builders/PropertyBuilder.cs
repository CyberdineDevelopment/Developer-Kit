using System;
using System.Collections.Generic;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Definitions;
using FractalDataWorks.CodeBuilder.Types;

namespace FractalDataWorks.CodeBuilder.Builders;

/// <summary>
/// True builder pattern implementation for property definitions.
/// Builds immutable PropertyDefinition products following the TRUE builder pattern.
/// Each method returns a new builder instance, keeping builders immutable.
/// </summary>
public sealed class PropertyBuilder : IAstBuilder<PropertyDefinition>, IValidatableBuilder
{
    private readonly PropertyBuilderState _state;

    /// <summary>
    /// Initializes a new instance of PropertyBuilder.
    /// </summary>
    public PropertyBuilder()
    {
        _state = new PropertyBuilderState();
    }

    /// <summary>
    /// Initializes a new instance of PropertyBuilder with the given state.
    /// </summary>
    /// <param name="state">The builder state.</param>
    private PropertyBuilder(PropertyBuilderState state)
    {
        _state = state;
    }

    /// <summary>
    /// Sets the name of the property.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Property name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        newState.Name = name;
        return new PropertyBuilder(newState);
    }

    /// <summary>
    /// Sets the type of the property.
    /// </summary>
    /// <param name="type">The property type.</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder WithType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Property type cannot be null or empty.", nameof(type));

        var newState = _state.Clone();
        newState.Type = type;
        return new PropertyBuilder(newState);
    }

    /// <summary>
    /// Sets the access modifier of the property.
    /// </summary>
    /// <param name="access">The access modifier.</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder WithAccess(AccessModifier access)
    {
        var newState = _state.Clone();
        newState.Access = access ?? throw new ArgumentNullException(nameof(access));
        return new PropertyBuilder(newState);
    }

    /// <summary>
    /// Adds a modifier to the property.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder WithModifier(Modifiers modifier)
    {
        var newState = _state.Clone();
        newState.Modifiers = newState.Modifiers.AddModifier(modifier);
        return new PropertyBuilder(newState);
    }

    /// <summary>
    /// Sets multiple modifiers for the property.
    /// </summary>
    /// <param name="modifiers">The modifiers to set.</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder WithModifiers(Modifiers modifiers)
    {
        var newState = _state.Clone();
        newState.Modifiers = modifiers;
        return new PropertyBuilder(newState);
    }

    /// <summary>
    /// Makes the property static.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder AsStatic()
    {
        return WithModifier(Modifiers.Static);
    }

    /// <summary>
    /// Makes the property virtual.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder AsVirtual()
    {
        return WithModifier(Modifiers.Virtual);
    }

    /// <summary>
    /// Makes the property override.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder AsOverride()
    {
        return WithModifier(Modifiers.Override);
    }

    /// <summary>
    /// Adds a getter to this property.
    /// </summary>
    /// <param name="body">Optional getter body (null for auto-implemented).</param>
    /// <param name="access">Optional getter access modifier.</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder WithGetter(string? body = null, AccessModifier? access = null)
    {
        var newState = _state.Clone();
        newState.Getter = new AccessorDefinition("get", access, body);
        return new PropertyBuilder(newState);
    }

    /// <summary>
    /// Adds a setter to this property.
    /// </summary>
    /// <param name="body">Optional setter body (null for auto-implemented).</param>
    /// <param name="access">Optional setter access modifier.</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder WithSetter(string? body = null, AccessModifier? access = null)
    {
        var newState = _state.Clone();
        newState.Setter = new AccessorDefinition("set", access, body);
        return new PropertyBuilder(newState);
    }

    /// <summary>
    /// Adds an init accessor to this property (C# 9+).
    /// </summary>
    /// <param name="body">Optional init body (null for auto-implemented).</param>
    /// <param name="access">Optional init access modifier.</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder WithInit(string? body = null, AccessModifier? access = null)
    {
        var newState = _state.Clone();
        newState.Init = new AccessorDefinition("init", access, body);
        return new PropertyBuilder(newState);
    }

    /// <summary>
    /// Makes this property read-only (adds getter only).
    /// </summary>
    /// <param name="body">Optional getter body (null for auto-implemented).</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder MakeReadOnly(string? body = null)
    {
        return WithGetter(body);
    }

    /// <summary>
    /// Makes this property read-write (adds both getter and setter).
    /// </summary>
    /// <param name="getterBody">Optional getter body (null for auto-implemented).</param>
    /// <param name="setterBody">Optional setter body (null for auto-implemented).</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder MakeReadWrite(string? getterBody = null, string? setterBody = null)
    {
        return WithGetter(getterBody).WithSetter(setterBody);
    }

    /// <summary>
    /// Adds an attribute to the property.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="arguments">Optional attribute arguments.</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder AddAttribute(string name, params object[] arguments)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        var attribute = new AttributeDefinition(name, arguments);
        newState.Attributes.Add(attribute);
        return new PropertyBuilder(newState);
    }

    /// <summary>
    /// Sets the XML documentation for the property.
    /// </summary>
    /// <param name="documentation">The documentation text.</param>
    /// <returns>A new builder instance.</returns>
    public PropertyBuilder WithDocumentation(string documentation)
    {
        var newState = _state.Clone();
        newState.XmlDocumentation = documentation;
        return new PropertyBuilder(newState);
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
            errors.Add("Property name is required");
        }

        if (string.IsNullOrWhiteSpace(_state.Type))
        {
            errors.Add("Property type is required");
        }

        if (!_state.Modifiers.IsValid())
        {
            errors.Add("Invalid modifier combination");
        }

        if (_state.Getter == null && _state.Setter == null && _state.Init == null)
        {
            errors.Add("Property must have at least one accessor (getter, setter, or init)");
        }

        return errors.ToArray();
    }

    /// <inheritdoc/>
    public PropertyDefinition Build()
    {
        if (!IsValid)
        {
            var errors = GetValidationErrors();
            throw new InvalidOperationException($"Cannot build invalid property: {string.Join(", ", errors)}");
        }

        return new PropertyDefinition(_state);
    }
}