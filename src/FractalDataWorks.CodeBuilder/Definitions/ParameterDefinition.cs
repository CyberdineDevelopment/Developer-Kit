using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Types;

namespace FractalDataWorks.CodeBuilder.Definitions;

/// <summary>
/// Immutable parameter definition product created by ParameterBuilder.
/// Represents a method/constructor parameter with its type, modifiers, and default value.
/// </summary>
public sealed class ParameterDefinition : IAstNode
{
    /// <summary>
    /// Gets the unique identifier for this AST node.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the type of this AST node.
    /// </summary>
    public string NodeType => "parameter";

    /// <summary>
    /// Gets the name of this parameter.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the parent node of this definition.
    /// </summary>
    public IAstNode? Parent { get; }

    /// <summary>
    /// Gets the child nodes of this definition.
    /// </summary>
    public IReadOnlyList<IAstNode> Children { get; }

    /// <summary>
    /// Gets the metadata associated with this definition.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the source location information for this definition.
    /// </summary>
    public SourceLocation? Location { get; }

    /// <summary>
    /// Gets the type of this parameter.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the parameter modifiers.
    /// </summary>
    public Modifiers Modifiers { get; }

    /// <summary>
    /// Gets the default value for this parameter.
    /// </summary>
    public string? DefaultValue { get; }

    /// <summary>
    /// Gets the attributes applied to this parameter.
    /// </summary>
    public IReadOnlyList<AttributeDefinition> Attributes { get; }

    /// <summary>
    /// Gets whether this parameter is optional.
    /// </summary>
    public bool IsOptional => DefaultValue != null;

    /// <summary>
    /// Gets whether this parameter is a reference parameter.
    /// </summary>
    public bool IsRef => Modifiers.HasModifier(Types.Modifiers.Ref);

    /// <summary>
    /// Gets whether this parameter is an output parameter.
    /// </summary>
    public bool IsOut => Modifiers.HasModifier(Types.Modifiers.Out);

    /// <summary>
    /// Gets whether this parameter is an input parameter.
    /// </summary>
    public bool IsIn => Modifiers.HasModifier(Types.Modifiers.In);

    /// <summary>
    /// Gets whether this parameter is a params array parameter.
    /// </summary>
    public bool IsParams => Modifiers.HasModifier(Types.Modifiers.Params);

    /// <summary>
    /// Internal constructor used by ParameterBuilder.
    /// </summary>
    /// <param name="state">The builder state to create the definition from.</param>
    internal ParameterDefinition(ParameterBuilderState state)
    {
        Id = Guid.NewGuid();
        Name = state.Name;
        Parent = state.Parent;
        Type = state.Type ?? "object";
        Modifiers = state.Modifiers;
        DefaultValue = state.DefaultValue;
        Attributes = state.Attributes.ToImmutableArray();
        Metadata = state.Metadata.ToImmutableDictionary(StringComparer.Ordinal);
        Location = state.Location;
        Children = ImmutableArray<IAstNode>.Empty;
    }

    /// <summary>
    /// Gets a specific child node by name.
    /// </summary>
    /// <param name="name">The name of the child node to retrieve.</param>
    /// <returns>The child node if found, otherwise null.</returns>
    public IAstNode? GetChild(string name) => null;

    /// <summary>
    /// Gets all child nodes of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of child nodes to retrieve.</typeparam>
    /// <returns>An enumerable of child nodes of the specified type.</returns>
    public IEnumerable<T> GetChildren<T>() where T : class, IAstNode => Enumerable.Empty<T>();

    /// <summary>
    /// Accepts a visitor for traversing the AST.
    /// </summary>
    /// <typeparam name="T">The return type of the visitor.</typeparam>
    /// <param name="visitor">The visitor to accept.</param>
    /// <returns>The result of the visitor operation.</returns>
    public T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}

/// <summary>
/// Internal state holder for ParameterBuilder.
/// This class is mutable and used to accumulate state during building.
/// </summary>
internal sealed class ParameterBuilderState
{
    public string? Name { get; set; }
    public IAstNode? Parent { get; set; }
    public string? Type { get; set; }
    public Modifiers Modifiers { get; set; } = Types.Modifiers.None;
    public string? DefaultValue { get; set; }
    public List<AttributeDefinition> Attributes { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new(StringComparer.Ordinal);
    public SourceLocation? Location { get; set; }

    /// <summary>
    /// Creates a deep copy of this state.
    /// </summary>
    /// <returns>A new state instance with the same values.</returns>
    public ParameterBuilderState Clone()
    {
        return new ParameterBuilderState
        {
            Name = Name,
            Parent = Parent,
            Type = Type,
            Modifiers = Modifiers,
            DefaultValue = DefaultValue,
            Attributes = new List<AttributeDefinition>(Attributes),
            Metadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal),
            Location = Location
        };
    }
}