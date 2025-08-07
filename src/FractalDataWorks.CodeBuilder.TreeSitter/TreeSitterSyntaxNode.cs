using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using static TreeSitter.Bindings.Native;

namespace FractalDataWorks.CodeBuilder.TreeSitter;

/// <summary>
/// TreeSitter implementation of ISyntaxNode.
/// </summary>
public sealed class TreeSitterSyntaxNode : ISyntaxNode
{
    private readonly TsNode _node;
    private readonly string _sourceText;
    private readonly TreeSitterSyntaxNode? _parent;
    private List<ISyntaxNode>? _children;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeSitterSyntaxNode"/> class.
    /// </summary>
    /// <param name="node">The TreeSitter node.</param>
    /// <param name="sourceText">The source text.</param>
    /// <param name="parent">The parent node.</param>
    public TreeSitterSyntaxNode(TsNode node, string sourceText, TreeSitterSyntaxNode? parent = null)
    {
        _node = node;
        _sourceText = sourceText;
        _parent = parent;
    }

    /// <inheritdoc/>
    public string NodeType => TsNodeType(_node);

    /// <inheritdoc/>
    public string Text => _sourceText.Substring((int)TsNodeStartByte(_node), (int)(TsNodeEndByte(_node) - TsNodeStartByte(_node)));

    /// <inheritdoc/>
    public int StartPosition => (int)TsNodeStartByte(_node);

    /// <inheritdoc/>
    public int EndPosition => (int)TsNodeEndByte(_node);

    /// <inheritdoc/>
    public int StartLine => (int)TsNodeStartPoint(_node).Row;

    /// <inheritdoc/>
    public int StartColumn => (int)TsNodeStartPoint(_node).Column;

    /// <inheritdoc/>
    public IReadOnlyList<ISyntaxNode> Children
    {
        get
        {
            if (_children == null)
            {
                _children = new List<ISyntaxNode>();
                var childCount = TsNodeChildCount(_node);
                for (uint i = 0; i < childCount; i++)
                {
                    var child = TsNodeChild(_node, i);
                    _children.Add(new TreeSitterSyntaxNode(child, _sourceText, this));
                }
            }
            return _children;
        }
    }

    /// <inheritdoc/>
    public ISyntaxNode? Parent => _parent;

    /// <inheritdoc/>
    public bool IsTerminal => TsNodeChildCount(_node) == 0;

    /// <inheritdoc/>
    public bool IsError => TsNodeIsError(_node) || TsNodeIsMissing(_node);

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