using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using TreeSitter;

namespace FractalDataWorks.CodeBuilder.TreeSitter;

/// <summary>
/// TreeSitter implementation of ISyntaxNode.
/// </summary>
public sealed class TreeSitterSyntaxNode : ISyntaxNode
{
    private readonly Node _node;
    private readonly string _sourceText;
    private readonly TreeSitterSyntaxNode? _parent;
    private List<ISyntaxNode>? _children;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeSitterSyntaxNode"/> class.
    /// </summary>
    /// <param name="node">The TreeSitter node.</param>
    /// <param name="sourceText">The source text.</param>
    /// <param name="parent">The parent node.</param>
    public TreeSitterSyntaxNode(Node node, string sourceText, TreeSitterSyntaxNode? parent = null)
    {
        _node = node;
        _sourceText = sourceText;
        _parent = parent;
    }

    /// <inheritdoc/>
    public string NodeType => _node.Type;

    /// <inheritdoc/>
    public string Text => _sourceText.Substring((int)_node.StartByte, (int)(_node.EndByte - _node.StartByte));

    /// <inheritdoc/>
    public int StartPosition => (int)_node.StartByte;

    /// <inheritdoc/>
    public int EndPosition => (int)_node.EndByte;

    /// <inheritdoc/>
    public int StartLine => (int)_node.StartPoint.Row;

    /// <inheritdoc/>
    public int StartColumn => (int)_node.StartPoint.Column;

    /// <inheritdoc/>
    public IReadOnlyList<ISyntaxNode> Children
    {
        get
        {
            if (_children == null)
            {
                _children = new List<ISyntaxNode>();
                for (uint i = 0; i < _node.ChildCount; i++)
                {
                    var child = _node.Child(i);
                    if (child != null)
                    {
                        _children.Add(new TreeSitterSyntaxNode(child, _sourceText, this));
                    }
                }
            }
            return _children;
        }
    }

    /// <inheritdoc/>
    public ISyntaxNode? Parent => _parent;

    /// <inheritdoc/>
    public bool IsTerminal => _node.ChildCount == 0;

    /// <inheritdoc/>
    public bool IsError => _node.IsError || _node.IsMissing;

    /// <inheritdoc/>
    public ISyntaxNode? FindChild(string nodeType)
    {
        return Children.FirstOrDefault(c => c.NodeType == nodeType);
    }

    /// <inheritdoc/>
    public IEnumerable<ISyntaxNode> FindChildren(string nodeType)
    {
        return Children.Where(c => c.NodeType == nodeType);
    }

    /// <inheritdoc/>
    public IEnumerable<ISyntaxNode> DescendantNodes()
    {
        foreach (var child in Children)
        {
            yield return child;
            foreach (var descendant in child.DescendantNodes())
            {
                yield return descendant;
            }
        }
    }
}