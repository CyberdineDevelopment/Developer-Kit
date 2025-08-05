using System;
using System.Collections.Generic;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Definitions;
using FractalDataWorks.CodeBuilder.Types;

namespace FractalDataWorks.CodeBuilder.Builders;

/// <summary>
/// True builder pattern implementation for method definitions.
/// Builds immutable MethodDefinition products following the TRUE builder pattern.
/// Each method returns a new builder instance, keeping builders immutable.
/// </summary>
public sealed class MethodBuilder : IAstBuilder<MethodDefinition>, IValidatableBuilder
{
    private readonly MethodBuilderState _state;

    /// <summary>
    /// Initializes a new instance of MethodBuilder.
    /// </summary>
    public MethodBuilder()
    {
        _state = new MethodBuilderState();
    }

    /// <summary>
    /// Initializes a new instance of MethodBuilder with the given state.
    /// </summary>
    /// <param name="state">The builder state.</param>
    private MethodBuilder(MethodBuilderState state)
    {
        _state = state;
    }

    /// <summary>
    /// Sets the name of the method.
    /// </summary>
    /// <param name="name">The method name.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Method name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        newState.Name = name;
        return new MethodBuilder(newState);
    }

    /// <summary>
    /// Sets the access modifier of the method.
    /// </summary>
    /// <param name="access">The access modifier.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder WithAccess(AccessModifier access)
    {
        var newState = _state.Clone();
        newState.Access = access ?? throw new ArgumentNullException(nameof(access));
        return new MethodBuilder(newState);
    }

    /// <summary>
    /// Sets the return type of the method.
    /// </summary>
    /// <param name="returnType">The return type.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder WithReturnType(string returnType)
    {
        if (string.IsNullOrWhiteSpace(returnType))
            throw new ArgumentException("Return type cannot be null or empty.", nameof(returnType));

        var newState = _state.Clone();
        newState.ReturnType = returnType;
        return new MethodBuilder(newState);
    }

    /// <summary>
    /// Adds a modifier to the method.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder WithModifier(Modifiers modifier)
    {
        var newState = _state.Clone();
        newState.Modifiers = newState.Modifiers.AddModifier(modifier);
        return new MethodBuilder(newState);
    }

    /// <summary>
    /// Sets multiple modifiers for the method.
    /// </summary>
    /// <param name="modifiers">The modifiers to set.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder WithModifiers(Modifiers modifiers)
    {
        var newState = _state.Clone();
        newState.Modifiers = modifiers;
        return new MethodBuilder(newState);
    }

    /// <summary>
    /// Makes the method static.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder AsStatic()
    {
        return WithModifier(Modifiers.Static);
    }

    /// <summary>
    /// Makes the method virtual.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder AsVirtual()
    {
        return WithModifier(Modifiers.Virtual);
    }

    /// <summary>
    /// Makes the method abstract.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder AsAbstract()
    {
        return WithModifier(Modifiers.Abstract);
    }

    /// <summary>
    /// Makes the method override.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder AsOverride()
    {
        return WithModifier(Modifiers.Override);
    }

    /// <summary>
    /// Makes the method async.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder AsAsync()
    {
        return WithModifier(Modifiers.Async);
    }

    /// <summary>
    /// Adds a parameter to the method.
    /// </summary>
    /// <param name="type">The parameter type.</param>
    /// <param name="name">The parameter name.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder AddParameter(string type, string name)
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
        return new MethodBuilder(newState);
    }

    /// <summary>
    /// Adds a parameter to the method using a parameter builder configuration.
    /// </summary>
    /// <param name="configure">Configuration action for the parameter.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder AddParameter(Action<ParameterBuilder> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var paramBuilder = new ParameterBuilder();
        configure(paramBuilder);
        var parameter = paramBuilder.Build();
        
        var newState = _state.Clone();
        newState.Parameters.Add(parameter);
        return new MethodBuilder(newState);
    }

    /// <summary>
    /// Sets the method body.
    /// </summary>
    /// <param name="body">The method body code.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder WithBody(string body)
    {
        var newState = _state.Clone();
        newState.Body = body;
        return new MethodBuilder(newState);
    }

    /// <summary>
    /// Adds a generic type parameter to the method.
    /// </summary>
    /// <param name="name">The name of the type parameter.</param>
    /// <param name="constraints">Optional constraints for the type parameter.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder AddGenericParameter(string name, params string[] constraints)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Generic parameter name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        var genericParam = new GenericParameterDefinition(name, constraints);
        newState.GenericParameters.Add(genericParam);
        return new MethodBuilder(newState);
    }

    /// <summary>
    /// Adds an attribute to the method.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="arguments">Optional attribute arguments.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder AddAttribute(string name, params object[] arguments)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or empty.", nameof(name));

        var newState = _state.Clone();
        var attribute = new AttributeDefinition(name, arguments);
        newState.Attributes.Add(attribute);
        return new MethodBuilder(newState);
    }

    /// <summary>
    /// Sets the XML documentation for the method.
    /// </summary>
    /// <param name="documentation">The documentation text.</param>
    /// <returns>A new builder instance.</returns>
    public MethodBuilder WithDocumentation(string documentation)
    {
        var newState = _state.Clone();
        newState.XmlDocumentation = documentation;
        return new MethodBuilder(newState);
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
            errors.Add("Method name is required");
        }

        if (!_state.Modifiers.IsValid())
        {
            errors.Add("Invalid modifier combination");
        }

        if (_state.Modifiers.HasModifier(Modifiers.Abstract) && !string.IsNullOrEmpty(_state.Body))
        {
            errors.Add("Abstract method cannot have a body");
        }

        if (!_state.Modifiers.HasModifier(Modifiers.Abstract) && string.IsNullOrEmpty(_state.Body))
        {
            // Allow empty body for interface methods or extern methods
            if (!_state.Modifiers.HasModifier(Modifiers.Extern))
            {
                errors.Add("Non-abstract method must have a body");
            }
        }

        return errors.ToArray();
    }

    /// <inheritdoc/>
    public MethodDefinition Build()
    {
        if (!IsValid)
        {
            var errors = GetValidationErrors();
            throw new InvalidOperationException($"Cannot build invalid method: {string.Join(", ", errors)}");
        }

        return new MethodDefinition(_state);
    }
}