using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Types;

namespace FractalDataWorks.CodeBuilder.Definitions;

/// <summary>
/// Immutable property definition product created by PropertyBuilder.
/// Represents a complete property declaration with getters, setters, and metadata.
/// </summary>
public sealed class PropertyDefinition : IAstNode, IMemberDefinition, IVirtualizableMember
{
    /// <summary>
    /// Gets the unique identifier for this AST node.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the type of this AST node.
    /// </summary>
    public string NodeType => "property";

    /// <summary>
    /// Gets the name of this property.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the parent node of this definition.
    /// </summary>
    public IAstNode? Parent { get; }

    /// <summary>
    /// Gets the child nodes of this definition (accessor definitions).
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
    /// Gets the access modifier for this property.
    /// </summary>
    public AccessModifier Access { get; }

    /// <summary>
    /// Gets the modifiers applied to this property.
    /// </summary>
    public Modifiers Modifiers { get; }

    /// <summary>
    /// Gets the type of this property.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the getter accessor, if any.
    /// </summary>
    public AccessorDefinition? Getter { get; }

    /// <summary>
    /// Gets the setter accessor, if any.
    /// </summary>
    public AccessorDefinition? Setter { get; }

    /// <summary>
    /// Gets the init accessor, if any (C# 9+).
    /// </summary>
    public AccessorDefinition? Init { get; }

    /// <summary>
    /// Gets the attributes applied to this property.
    /// </summary>
    public IReadOnlyList<AttributeDefinition> Attributes { get; }

    /// <summary>
    /// Gets the XML documentation for this property.
    /// </summary>
    public string? XmlDocumentation { get; }

    /// <summary>
    /// Gets whether this property is static.
    /// </summary>
    public bool IsStatic => Modifiers.HasModifier(Types.Modifiers.Static);

    /// <summary>
    /// Gets whether this property is virtual.
    /// </summary>
    public bool IsVirtual => Modifiers.HasModifier(Types.Modifiers.Virtual);

    /// <summary>
    /// Gets whether this property is an override.
    /// </summary>
    public bool IsOverride => Modifiers.HasModifier(Types.Modifiers.Override);

    /// <summary>
    /// Gets whether this property is read-only (has getter but no setter).
    /// </summary>
    public bool IsReadOnly => Getter != null && Setter == null && Init == null;

    /// <summary>
    /// Gets whether this property is write-only (has setter but no getter).
    /// </summary>
    public bool IsWriteOnly => Getter == null && (Setter != null || Init != null);

    /// <summary>
    /// Gets the documentation for this member (alias for XmlDocumentation).
    /// </summary>
    string? IMemberDefinition.Documentation => XmlDocumentation;

    /// <summary>
    /// Internal constructor used by PropertyBuilder.
    /// </summary>
    /// <param name="state">The builder state to create the definition from.</param>
    internal PropertyDefinition(PropertyBuilderState state)
    {
        Id = Guid.NewGuid();
        Name = state.Name;
        Parent = state.Parent;
        Access = state.Access;
        Modifiers = state.Modifiers;
        Type = state.Type ?? "object";
        Getter = state.Getter;
        Setter = state.Setter;
        Init = state.Init;
        Attributes = state.Attributes.ToImmutableArray();
        XmlDocumentation = state.XmlDocumentation;
        Metadata = state.Metadata.ToImmutableDictionary(StringComparer.Ordinal);
        Location = state.Location;

        // Create children list from accessors
        var children = new List<IAstNode>();
        if (Getter != null) children.Add(Getter);
        if (Setter != null) children.Add(Setter);
        if (Init != null) children.Add(Init);
        Children = children.ToImmutableArray();
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
/// Immutable accessor definition (getter, setter, init).
/// </summary>
public sealed class AccessorDefinition : IAstNode
{
    /// <summary>
    /// Gets the unique identifier for this AST node.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the type of this AST node.
    /// </summary>
    public string NodeType => "accessor";

    /// <summary>
    /// Gets the name of this accessor (get, set, init).
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
    /// Gets the access modifier for this accessor (if different from property).
    /// </summary>
    public AccessModifier? Access { get; }

    /// <summary>
    /// Gets the body of this accessor (null for auto-implemented).
    /// </summary>
    public string? Body { get; }

    /// <summary>
    /// Gets whether this is an auto-implemented accessor.
    /// </summary>
    public bool IsAutoImplemented => Body == null;

    /// <summary>
    /// Internal constructor for AccessorDefinition.
    /// </summary>
    /// <param name="name">The accessor name (get, set, init).</param>
    /// <param name="access">The access modifier, if any.</param>
    /// <param name="body">The accessor body, if any.</param>
    /// <param name="parent">The parent node.</param>
    internal AccessorDefinition(string name, AccessModifier? access = null, string? body = null, IAstNode? parent = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Access = access;
        Body = body;
        Parent = parent;
        Children = ImmutableArray<IAstNode>.Empty;
        Metadata = ImmutableDictionary<string, object>.Empty;
        Location = null;
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
/// Internal state holder for PropertyBuilder.
/// This class is mutable and used to accumulate state during building.
/// </summary>
internal sealed class PropertyBuilderState
{
    public string? Name { get; set; }
    public IAstNode? Parent { get; set; }
    public AccessModifier Access { get; set; } = AccessModifier.None;
    public Modifiers Modifiers { get; set; } = Types.Modifiers.None;
    public string? Type { get; set; }
    public AccessorDefinition? Getter { get; set; }
    public AccessorDefinition? Setter { get; set; }
    public AccessorDefinition? Init { get; set; }
    public List<AttributeDefinition> Attributes { get; set; } = new();
    public string? XmlDocumentation { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new(StringComparer.Ordinal);
    public SourceLocation? Location { get; set; }

    /// <summary>
    /// Creates a deep copy of this state.
    /// </summary>
    /// <returns>A new state instance with the same values.</returns>
    public PropertyBuilderState Clone()
    {
        return new PropertyBuilderState
        {
            Name = Name,
            Parent = Parent,
            Access = Access,
            Modifiers = Modifiers,
            Type = Type,
            Getter = Getter,
            Setter = Setter,
            Init = Init,
            Attributes = new List<AttributeDefinition>(Attributes),
            XmlDocumentation = XmlDocumentation,
            Metadata = new Dictionary<string, object>(Metadata, StringComparer.Ordinal),
            Location = Location
        };
    }
}