using System;
using System.Collections.Generic;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Definitions;
using FractalDataWorks.CodeBuilder.Types;

namespace FractalDataWorks.CodeBuilder.Builders;

/// <summary>
/// True builder pattern implementation for parameter definitions.
/// Builds immutable ParameterDefinition products following the TRUE builder pattern.
/// Each method returns a new builder instance, keeping builders immutable.
/// </summary>
public sealed class ParameterBuilder : IAstBuilder<ParameterDefinition>, IValidatableBuilder
{
    private readonly ParameterBuilderState _state;

    /// <summary>
    /// Initializes a new instance of ParameterBuilder.
    /// </summary>
    public ParameterBuilder()
    {
        _state = new ParameterBuilderState();
    }

    /// <summary>
    /// Initializes a new instance of ParameterBuilder with the given state.
    /// </summary>
    /// <param name="state">The builder state.</param>
    private ParameterBuilder(ParameterBuilderState state)
    {
        _state = state;
    }

    /// <summary>
    /// Sets the name of the parameter.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Parameter name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        newState.Name = name;
        return new ParameterBuilder(newState);
    }

    /// <summary>
    /// Sets the type of the parameter.
    /// </summary>
    /// <param name="type">The parameter type.</param>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder WithType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Parameter type cannot be null or empty.", nameof(type));

        var newState = _state.Clone();
        newState.Type = type;
        return new ParameterBuilder(newState);
    }

    /// <summary>
    /// Adds a modifier to the parameter.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder WithModifier(Modifiers modifier)
    {
        var newState = _state.Clone();
        newState.Modifiers = newState.Modifiers.AddModifier(modifier);
        return new ParameterBuilder(newState);
    }

    /// <summary>
    /// Sets multiple modifiers for the parameter.
    /// </summary>
    /// <param name="modifiers">The modifiers to set.</param>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder WithModifiers(Modifiers modifiers)
    {
        var newState = _state.Clone();
        newState.Modifiers = modifiers;
        return new ParameterBuilder(newState);
    }

    /// <summary>
    /// Sets the default value for this parameter.
    /// </summary>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder WithDefaultValue(string defaultValue)
    {
        var newState = _state.Clone();
        newState.DefaultValue = defaultValue;
        return new ParameterBuilder(newState);
    }

    /// <summary>
    /// Makes this parameter optional by adding a default value.
    /// </summary>
    /// <param name="defaultValue">The default value (defaults to "default" if not specified).</param>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder AsOptional(string? defaultValue = "default")
    {
        return WithDefaultValue(defaultValue ?? "default");
    }

    /// <summary>
    /// Makes this parameter a reference parameter.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder AsRef()
    {
        return WithModifier(Modifiers.Ref);
    }

    /// <summary>
    /// Makes this parameter an output parameter.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder AsOut()
    {
        return WithModifier(Modifiers.Out);
    }

    /// <summary>
    /// Makes this parameter an input parameter.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder AsIn()
    {
        return WithModifier(Modifiers.In);
    }

    /// <summary>
    /// Makes this parameter a params array parameter.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder AsParams()
    {
        return WithModifier(Modifiers.Params);
    }

    /// <summary>
    /// Adds an attribute to the parameter.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="arguments">Optional attribute arguments.</param>
    /// <returns>A new builder instance.</returns>
    public ParameterBuilder AddAttribute(string name, params object[] arguments)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        var attribute = new AttributeDefinition(name, arguments);
        newState.Attributes.Add(attribute);
        return new ParameterBuilder(newState);
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
            errors.Add("Parameter name is required");
        }

        if (string.IsNullOrWhiteSpace(_state.Type))
        {
            errors.Add("Parameter type is required");
        }

        if (!_state.Modifiers.IsValid())
        {
            errors.Add("Invalid modifier combination");
        }

        // Check for conflicting parameter modifiers
        var refOutInCount = 0;
        if (_state.Modifiers.HasModifier(Modifiers.Ref)) refOutInCount++;
        if (_state.Modifiers.HasModifier(Modifiers.Out)) refOutInCount++;
        if (_state.Modifiers.HasModifier(Modifiers.In)) refOutInCount++;

        if (refOutInCount > 1)
        {
            errors.Add("Parameter cannot have multiple ref/out/in modifiers");
        }

        if (_state.Modifiers.HasModifier(Modifiers.Out) && _state.DefaultValue != null)
        {
            errors.Add("Out parameter cannot have a default value");
        }

        return errors.ToArray();
    }

    /// <inheritdoc/>
    public ParameterDefinition Build()
    {
        if (!IsValid)
        {
            var errors = GetValidationErrors();
            throw new InvalidOperationException($"Cannot build invalid parameter: {string.Join(", ", errors)}");
        }

        return new ParameterDefinition(_state);
    }
}