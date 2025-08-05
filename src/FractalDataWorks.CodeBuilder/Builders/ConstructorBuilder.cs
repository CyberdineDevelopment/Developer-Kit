using System;
using System.Collections.Generic;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Definitions;
using FractalDataWorks.CodeBuilder.Types;

namespace FractalDataWorks.CodeBuilder.Builders;

/// <summary>
/// True builder pattern implementation for constructor definitions.
/// Builds immutable ConstructorDefinition products following the TRUE builder pattern.
/// Each method returns a new builder instance, keeping builders immutable.
/// </summary>
public sealed class ConstructorBuilder : IAstBuilder<ConstructorDefinition>, IValidatableBuilder
{
    private readonly ConstructorBuilderState _state;

    /// <summary>
    /// Initializes a new instance of ConstructorBuilder.
    /// </summary>
    public ConstructorBuilder()
    {
        _state = new ConstructorBuilderState();
    }

    /// <summary>
    /// Initializes a new instance of ConstructorBuilder with the given state.
    /// </summary>
    /// <param name="state">The builder state.</param>
    private ConstructorBuilder(ConstructorBuilderState state)
    {
        _state = state;
    }

    /// <summary>
    /// Sets the access modifier of the constructor.
    /// </summary>
    /// <param name="access">The access modifier.</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder WithAccess(AccessModifier access)
    {
        var newState = _state.Clone();
        newState.Access = access ?? throw new ArgumentNullException(nameof(access));
        return new ConstructorBuilder(newState);
    }

    /// <summary>
    /// Adds a modifier to the constructor.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder WithModifier(Modifiers modifier)
    {
        var newState = _state.Clone();
        newState.Modifiers = newState.Modifiers.AddModifier(modifier);
        return new ConstructorBuilder(newState);
    }

    /// <summary>
    /// Sets multiple modifiers for the constructor.
    /// </summary>
    /// <param name="modifiers">The modifiers to set.</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder WithModifiers(Modifiers modifiers)
    {
        var newState = _state.Clone();
        newState.Modifiers = modifiers;
        return new ConstructorBuilder(newState);
    }

    /// <summary>
    /// Makes the constructor static.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder AsStatic()
    {
        return WithModifier(Modifiers.Static);
    }

    /// <summary>
    /// Adds a parameter to the constructor.
    /// </summary>
    /// <param name="type">The parameter type.</param>
    /// <param name="name">The parameter name.</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder AddParameter(string type, string name)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Parameter type cannot be null or empty.", nameof(type));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Parameter name cannot be null or empty.", nameof(name));

        var paramBuilder = new ParameterBuilder()
            .WithName(name)
            .WithType(type);
        
        var parameter = paramBuilder.Build();
        var newState = _state.Clone();
        newState.Parameters.Add(parameter);
        return new ConstructorBuilder(newState);
    }

    /// <summary>
    /// Adds a parameter to the constructor using a parameter builder configuration.
    /// </summary>
    /// <param name="configure">Configuration action for the parameter.</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder AddParameter(Action<ParameterBuilder> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var paramBuilder = new ParameterBuilder();
        configure(paramBuilder);
        var parameter = paramBuilder.Build();
        
        var newState = _state.Clone();
        newState.Parameters.Add(parameter);
        return new ConstructorBuilder(newState);
    }

    /// <summary>
    /// Sets a base constructor call.
    /// </summary>
    /// <param name="baseCall">The base constructor call (e.g., "base(param1, param2)" or "this(param1)").</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder WithBaseCall(string baseCall)
    {
        if (string.IsNullOrWhiteSpace(baseCall))
            throw new ArgumentException("Base call cannot be null or empty.", nameof(baseCall));

        var newState = _state.Clone();
        newState.BaseCall = baseCall;
        return new ConstructorBuilder(newState);
    }

    /// <summary>
    /// Sets a call to the base class constructor.
    /// </summary>
    /// <param name="arguments">Arguments to pass to the base constructor.</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder CallBase(params string[] arguments)
    {
        var args = arguments != null && arguments.Length > 0 ? string.Join(", ", arguments) : "";
        return WithBaseCall($"base({args})");
    }

    /// <summary>
    /// Sets a call to another constructor in the same class.
    /// </summary>
    /// <param name="arguments">Arguments to pass to the other constructor.</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder CallThis(params string[] arguments)
    {
        var args = arguments != null && arguments.Length > 0 ? string.Join(", ", arguments) : "";
        return WithBaseCall($"this({args})");
    }

    /// <summary>
    /// Sets the constructor body.
    /// </summary>
    /// <param name="body">The constructor body code.</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder WithBody(string body)
    {
        var newState = _state.Clone();
        newState.Body = body;
        return new ConstructorBuilder(newState);
    }

    /// <summary>
    /// Adds an attribute to the constructor.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="arguments">Optional attribute arguments.</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder AddAttribute(string name, params object[] arguments)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        var attribute = new AttributeDefinition(name, arguments);
        newState.Attributes.Add(attribute);
        return new ConstructorBuilder(newState);
    }

    /// <summary>
    /// Sets the XML documentation for the constructor.
    /// </summary>
    /// <param name="documentation">The documentation text.</param>
    /// <returns>A new builder instance.</returns>
    public ConstructorBuilder WithDocumentation(string documentation)
    {
        var newState = _state.Clone();
        newState.XmlDocumentation = documentation;
        return new ConstructorBuilder(newState);
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

        if (!_state.Modifiers.IsValid())
        {
            errors.Add("Invalid modifier combination");
        }

        if (_state.Modifiers.HasModifier(Modifiers.Static) && _state.Parameters.Count > 0)
        {
            errors.Add("Static constructor cannot have parameters");
        }

        if (_state.Modifiers.HasModifier(Modifiers.Static) && !string.IsNullOrEmpty(_state.BaseCall))
        {
            errors.Add("Static constructor cannot have base/this calls");
        }

        if (_state.Modifiers.HasModifier(Modifiers.Static) && _state.Access != AccessModifier.None)
        {
            errors.Add("Static constructor cannot have access modifiers");
        }

        return errors.ToArray();
    }

    /// <inheritdoc/>
    public ConstructorDefinition Build()
    {
        if (!IsValid)
        {
            var errors = GetValidationErrors();
            throw new InvalidOperationException($"Cannot build invalid constructor: {string.Join(", ", errors)}");
        }

        return new ConstructorDefinition(_state);
    }
}