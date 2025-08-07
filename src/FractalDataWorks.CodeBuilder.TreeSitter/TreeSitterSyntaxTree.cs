using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using static TreeSitter.Bindings.Native;

namespace FractalDataWorks.CodeBuilder.TreeSitter;

/// <summary>
/// TreeSitter implementation of ISyntaxTree.
/// </summary>
public sealed class TreeSitterSyntaxTree : ISyntaxTree
{
    private readonly IntPtr _tree;
    private readonly TreeSitterSyntaxNode _root;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeSitterSyntaxTree"/> class.
    /// </summary>
    /// <param name="tree">The TreeSitter tree pointer.</param>
    /// <param name="sourceText">The source text.</param>
    /// <param name="language">The language.</param>
    /// <param name="filePath">The file path.</param>
    public TreeSitterSyntaxTree(IntPtr tree, string sourceText, string language, string? filePath = null)
    {
        _tree = tree;
        SourceText = sourceText;
        Language = language;
        FilePath = filePath;
        var rootNode = TsTreeRootNode(_tree);
        _root = new TreeSitterSyntaxNode(rootNode, sourceText);
    }

    /// <inheritdoc/>
    public ISyntaxNode Root => _root;

    /// <inheritdoc/>
    public string SourceText { get; }

    /// <inheritdoc/>
    public string Language { get; }

    /// <inheritdoc/>
    public string? FilePath { get; }

    /// <inheritdoc/>
    public bool HasErrors => GetErrors().Any();

    /// <inheritdoc/>
    public IEnumerable<ISyntaxNode> GetErrors()
    {
        return Root.DescendantNodes().Where(n => n.IsError);
    }

    /// <inheritdoc/>
    public IEnumerable<ISyntaxNode> FindNodes(string nodeType)
    {
        return Root.DescendantNodes().Where(n => n.NodeType == nodeType);
    }

    /// <inheritdoc/>
    public ISyntaxNode? GetNodeAtPosition(int position)
    {
        return FindNodeAtPosition(Root, position);
    }

    /// <inheritdoc/>
    public ISyntaxNode? GetNodeAtLocation(int line, int column)
    {
        return FindNodeAtLocation(Root, line, column);
    }

    private static ISyntaxNode? FindNodeAtPosition(ISyntaxNode node, int position)
    {
        if (position < node.StartPosition || position >= node.EndPosition)
            return null;

        foreach (var child in node.Children)
        {
            var found = FindNodeAtPosition(child, position);
            if (found != null)
                return found;
        }

        return node;
    }

    private static ISyntaxNode? FindNodeAtLocation(ISyntaxNode node, int line, int column)
    {
        if (line != node.StartLine)
            return null;

        if (column < node.StartColumn)
            return null;

        foreach (var child in node.Children)
        {
            var found = FindNodeAtLocation(child, line, column);
            if (found != null)
                return found;
        }

        return node;
    }
}