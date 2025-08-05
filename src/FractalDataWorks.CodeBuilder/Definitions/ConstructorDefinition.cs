using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Types;

namespace FractalDataWorks.CodeBuilder.Definitions;

/// <summary>
/// Immutable constructor definition product created by ConstructorBuilder.
/// Represents a complete constructor declaration with parameters, body, and metadata.
/// </summary>
public sealed class ConstructorDefinition : IAstNode, IMemberDefinition
{
    /// <summary>
    /// Gets the unique identifier for this AST node.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the type of this AST node.
    /// </summary>
    public string NodeType => "constructor";

    /// <summary>
    /// Gets the name of this constructor (typically the class name).
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the parent node of this definition.
    /// </summary>
    public IAstNode? Parent { get; }

    /// <summary>
    /// Gets the child nodes of this definition (parameters).
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
    /// Gets the access modifier for this constructor.
    /// </summary>
    public AccessModifier Access { get; }

    /// <summary>
    /// Gets the modifiers applied to this constructor.
    /// </summary>
    public Modifiers Modifiers { get; }

    /// <summary>
    /// Gets the parameters of this constructor.
    /// </summary>
    public IReadOnlyList<ParameterDefinition> Parameters { get; }

    /// <summary>
    /// Gets the base constructor call (e.g., "base(param1, param2)" or "this(param1)").
    /// </summary>
    public string? BaseCall { get; }

    /// <summary>
    /// Gets the body of this constructor.
    /// </summary>
    public string? Body { get; }

    /// <summary>
    /// Gets the attributes applied to this constructor.
    /// </summary>
    public IReadOnlyList<AttributeDefinition> Attributes { get; }

    /// <summary>
    /// Gets the XML documentation for this constructor.
    /// </summary>
    public string? XmlDocumentation { get; }

    /// <summary>
    /// Gets whether this constructor is static.
    /// </summary>
    public bool IsStatic => Modifiers.HasModifier(Types.Modifiers.Static);

    /// <summary>
    /// Gets the documentation for this member (alias for XmlDocumentation).
    /// </summary>
    string? IMemberDefinition.Documentation => XmlDocumentation;

    /// <summary>
    /// Internal constructor used by ConstructorBuilder.
    /// </summary>
    /// <param name="state">The builder state to create the definition from.</param>
    internal ConstructorDefinition(ConstructorBuilderState state)
    {
        Id = Guid.NewGuid();
        Name = state.Name;
        Parent = state.Parent;
        Access = state.Access;
        Modifiers = state.Modifiers;
        Parameters = state.Parameters.ToImmutableArray();
        BaseCall = state.BaseCall;
        Body = state.Body;
        Attributes = state.Attributes.ToImmutableArray();
        XmlDocumentation = state.XmlDocumentation;
        Metadata = state.Metadata.ToImmutableDictionary(StringComparer.Ordinal);
        Location = state.Location;

        // Create children list from parameters
        Children = Parameters.Cast<IAstNode>().ToImmutableArray();
    }

    /// <summary>
    /// Gets a specific child node by name.
    /// </summary>
    /// <param name="name">The name of the child node to retrieve.</param>
    /// <returns>The child node if found, otherwise null.</returns>
    public IAstNode? GetChild(string name)
    {
        foreach (var child in Children)
        {
            if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets all child nodes of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of child nodes to retrieve.</typeparam>
    /// <returns>An enumerable of child nodes of the specified type.</returns>
    public IEnumerable<T> GetChildren<T>() where T : class, IAstNode
    {
        return Children.OfType<T>();
    }

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
/// Internal state holder for ConstructorBuilder.
/// This class is mutable and used to accumulate state during building.
/// </summary>
internal sealed class ConstructorBuilderState
{
    public string? Name { get; set; }
    public IAstNode? Parent { get; set; }
    public AccessModifier Access { get; set; } = AccessModifier.None;
    public Modifiers Modifiers { get; set; } = Types.Modifiers.None;
    public List<ParameterDefinition> Parameters { get; set; } = new();
    public string? BaseCall { get; set; }
    public string? Body { get; set; }
    public List<AttributeDefinition> Attributes { get; set; } = new();
    public string? XmlDocumentation { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new(StringComparer.Ordinal);
    public SourceLocation? Location { get; set; }

    /// <summary>
    /// Creates a deep copy of this state.
    /// </summary>
    /// <returns>A new state instance with the same values.</returns>
    public ConstructorBuilderState Clone()
    {
        return new ConstructorBuilderState
        {
            Name = Name,
            Parent = Parent,
            Access = Access,
            Modifiers = Modifiers,
            Parameters = new List<ParameterDefinition>(Parameters),
            BaseCall = BaseCall,
            Body = Body,
            Attributes = new List<AttributeDefinition>(Attributes),
            XmlDocumentation = XmlDocumentation,
            Metadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal),
            Location = Location
        };
    }
}