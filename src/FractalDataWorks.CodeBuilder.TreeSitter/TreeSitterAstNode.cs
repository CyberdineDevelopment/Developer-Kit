using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.TreeSitter;

/// <summary>
/// Tree-sitter implementation of an AST node.
/// Adapts tree-sitter parse trees to the common AST interface.
/// </summary>
public sealed class TreeSitterAstNode : IAstNode
{
    private readonly Dictionary<string, object> _metadata;
    private readonly List<IAstNode> _children;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeSitterAstNode"/> class.
    /// </summary>
    /// <param name="nodeType">The type of this node.</param>
    /// <param name="name">The name of this node, if applicable.</param>
    /// <param name="location">The source location of this node.</param>
    public TreeSitterAstNode(string nodeType, string? name = null, SourceLocation? location = null)
    {
        Id = Guid.NewGuid();
        NodeType = nodeType ?? throw new ArgumentNullException(nameof(nodeType));
        Name = name;
        Location = location;
        _metadata = new Dictionary<string, object>(StringComparer.Ordinal);
        _children = new List<IAstNode>();
    }

    /// <inheritdoc/>
    public Guid Id { get; }

    /// <inheritdoc/>
    public string NodeType { get; }

    /// <inheritdoc/>
    public string? Name { get; }

    /// <inheritdoc/>
    public IAstNode? Parent { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<IAstNode> Children => _children;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <inheritdoc/>
    public SourceLocation? Location { get; }

    /// <summary>
    /// Gets the raw text content of this node, if available.
    /// </summary>
    public string? Text { get; private set; }

    /// <summary>
    /// Gets whether this node represents an error in the parse tree.
    /// </summary>
    public bool IsError { get; private set; }

    /// <summary>
    /// Gets whether this node is missing from the source (inserted by error recovery).
    /// </summary>
    public bool IsMissing { get; private set; }

    /// <summary>
    /// Sets the text content of this node.
    /// </summary>
    /// <param name="text">The text content.</param>
    public void SetText(string? text)
    {
        Text = text;
    }

    /// <summary>
    /// Sets whether this node represents an error.
    /// </summary>
    /// <param name="isError">True if this node is an error; otherwise, false.</param>
    public void SetError(bool isError)
    {
        IsError = isError;
    }

    /// <summary>
    /// Sets whether this node is missing.
    /// </summary>
    /// <param name="isMissing">True if this node is missing; otherwise, false.</param>
    public void SetMissing(bool isMissing)
    {
        IsMissing = isMissing;
    }

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    /// <param name="child">The child node to add.</param>
    public void AddChild(IAstNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        
        if (child is TreeSitterAstNode tsChild)
        {
            tsChild.Parent = this;
        }
        
        _children.Add(child);
    }

    /// <summary>
    /// Adds multiple child nodes to this node.
    /// </summary>
    /// <param name="children">The child nodes to add.</param>
    public void AddChildren(IEnumerable<IAstNode> children)
    {
        ArgumentNullException.ThrowIfNull(children);
        
        foreach (var child in children)
        {
            AddChild(child);
        }
    }

    /// <summary>
    /// Removes a child node from this node.
    /// </summary>
    /// <param name="child">The child node to remove.</param>
    /// <returns>True if the child was removed; otherwise, false.</returns>
    public bool RemoveChild(IAstNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        
        var removed = _children.Remove(child);
        if (removed && child is TreeSitterAstNode tsChild)
        {
            tsChild.Parent = null;
        }
        
        return removed;
    }

    /// <summary>
    /// Sets metadata for this node.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    public void SetMetadata(string key, object value)
    {
        ArgumentNullException.ThrowIfNull(key);
        _metadata[key] = value;
    }

    /// <summary>
    /// Gets metadata for this node.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>The metadata value if found; otherwise, null.</returns>
    public object? GetMetadata(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _metadata.TryGetValue(key, out var value) ? value : null;
    }

    /// <inheritdoc/>
    public IAstNode? GetChild(string name)
    {
        return _children.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IEnumerable<T> GetChildren<T>() where T : class, IAstNode
    {
        return _children.OfType<T>();
    }

    /// <inheritdoc/>
    public T Accept<T>(IAstVisitor<T> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        return visitor.Visit(this);
    }

    /// <summary>
    /// Gets all descendant nodes of this node.
    /// </summary>
    /// <returns>An enumerable of all descendant nodes.</returns>
    public IEnumerable<IAstNode> GetDescendants()
    {
        foreach (var child in _children)
        {
            yield return child;
            
            if (child is TreeSitterAstNode tsChild)
            {
                foreach (var descendant in tsChild.GetDescendants())
                {
                    yield return descendant;
                }
            }
        }
    }

    /// <summary>
    /// Gets all descendant nodes of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of descendant nodes to retrieve.</typeparam>
    /// <returns>An enumerable of descendant nodes of the specified type.</returns>
    public IEnumerable<T> GetDescendants<T>() where T : class, IAstNode
    {
        return GetDescendants().OfType<T>();
    }

    /// <summary>
    /// Finds the first descendant node with the specified name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>The first descendant node with the specified name, or null if not found.</returns>
    public IAstNode? FindDescendant(string name)
    {
        return GetDescendants().FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds all descendant nodes with the specified name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>An enumerable of descendant nodes with the specified name.</returns>
    public IEnumerable<IAstNode> FindDescendants(string name)
    {
        return GetDescendants().Where(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the path from the root to this node.
    /// </summary>
    /// <returns>An array of nodes representing the path from root to this node.</returns>
    public IAstNode[] GetPath()
    {
        var path = new List<IAstNode>();
        var current = this as IAstNode;
        
        while (current != null)
        {
            path.Add(current);
            current = current.Parent;
        }
        
        path.Reverse();
        return path.ToArray();
    }

    /// <summary>
    /// Gets the depth of this node in the tree (0 for root).
    /// </summary>
    /// <returns>The depth of this node.</returns>
    public int GetDepth()
    {
        var depth = 0;
        var current = Parent;
        
        while (current != null)
        {
            depth++;
            current = current.Parent;
        }
        
        return depth;
    }

    /// <summary>
    /// Returns a string representation of this node.
    /// </summary>
    /// <returns>A string representation of this node.</returns>
    public override string ToString()
    {
        var text = !string.IsNullOrEmpty(Text) ? $" '{Text}'" : string.Empty;
        var name = !string.IsNullOrEmpty(Name) ? $" ({Name})" : string.Empty;
        return $"{NodeType}{name}{text}";
    }
}