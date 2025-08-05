using System;
using System.Collections.Generic;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Represents an immutable node in an Abstract Syntax Tree.
/// This is the fundamental building block for all code constructs.
/// </summary>
public interface IAstNode
{
    /// <summary>
    /// Gets the unique identifier for this AST node.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the type of this AST node (e.g., "class", "method", "property").
    /// </summary>
    string NodeType { get; }

    /// <summary>
    /// Gets the name of this node, if applicable.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the parent node of this node, if any.
    /// </summary>
    IAstNode? Parent { get; }

    /// <summary>
    /// Gets the child nodes of this node.
    /// </summary>
    IReadOnlyList<IAstNode> Children { get; }

    /// <summary>
    /// Gets the metadata associated with this node.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the source location information for this node.
    /// </summary>
    SourceLocation? Location { get; }

    /// <summary>
    /// Gets a specific child node by name.
    /// </summary>
    /// <param name="name">The name of the child node to retrieve.</param>
    /// <returns>The child node if found, otherwise null.</returns>
    IAstNode? GetChild(string name);

    /// <summary>
    /// Gets all child nodes of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of child nodes to retrieve.</typeparam>
    /// <returns>An enumerable of child nodes of the specified type.</returns>
    IEnumerable<T> GetChildren<T>() where T : class, IAstNode;

    /// <summary>
    /// Accepts a visitor for traversing the AST.
    /// </summary>
    /// <typeparam name="T">The return type of the visitor.</typeparam>
    /// <param name="visitor">The visitor to accept.</param>
    /// <returns>The result of the visitor operation.</returns>
    T Accept<T>(IAstVisitor<T> visitor);
}

/// <summary>
/// Represents source location information for an AST node.
/// </summary>
public sealed record SourceLocation
{
    /// <summary>
    /// Gets the file path, if available.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets the start line number (1-based).
    /// </summary>
    public int StartLine { get; init; }

    /// <summary>
    /// Gets the start column number (1-based).
    /// </summary>
    public int StartColumn { get; init; }

    /// <summary>
    /// Gets the end line number (1-based).
    /// </summary>
    public int EndLine { get; init; }

    /// <summary>
    /// Gets the end column number (1-based).
    /// </summary>
    public int EndColumn { get; init; }

    /// <summary>
    /// Gets the start position in the source text (0-based).
    /// </summary>
    public int StartPosition { get; init; }

    /// <summary>
    /// Gets the end position in the source text (0-based).
    /// </summary>
    public int EndPosition { get; init; }
}

/// <summary>
/// Visitor pattern interface for traversing AST nodes.
/// </summary>
/// <typeparam name="T">The return type of visitor operations.</typeparam>
public interface IAstVisitor<out T>
{
    /// <summary>
    /// Visits an AST node.
    /// </summary>
    /// <param name="node">The node to visit.</param>
    /// <returns>The result of visiting the node.</returns>
    T Visit(IAstNode node);
}