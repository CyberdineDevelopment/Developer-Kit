using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.Definitions;

/// <summary>
/// Immutable attribute definition product.
/// Represents an attribute/annotation applied to a code element.
/// </summary>
public sealed class AttributeDefinition : IAstNode
{
    /// <summary>
    /// Gets the unique identifier for this AST node.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the type of this AST node.
    /// </summary>
    public string NodeType => "attribute";

    /// <summary>
    /// Gets the name of this attribute.
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
    /// Gets the arguments for this attribute.
    /// </summary>
    public IReadOnlyList<object> Arguments { get; }

    /// <summary>
    /// Internal constructor for AttributeDefinition.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="arguments">The attribute arguments.</param>
    /// <param name="parent">The parent node.</param>
    internal AttributeDefinition(string name, IEnumerable<object>? arguments = null, IAstNode? parent = null)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Parent = parent;
        Arguments = arguments?.ToImmutableArray() ?? ImmutableArray<object>.Empty;
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