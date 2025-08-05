using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;

namespace FractalDataWorks.CodeBuilder.TreeSitter;

/// <summary>
/// Tree-sitter implementation of a syntax tree.
/// Provides access to the parsed AST and source information.
/// </summary>
public sealed class TreeSitterSyntaxTree : ISyntaxTree
{
    private readonly List<SyntaxError> _errors;

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeSitterSyntaxTree"/> class.
    /// </summary>
    /// <param name="root">The root node of the syntax tree.</param>
    /// <param name="sourceText">The original source text.</param>
    /// <param name="filePath">The file path, if available.</param>
    /// <param name="language">The language of the syntax tree.</param>
    public TreeSitterSyntaxTree(
        TreeSitterAstNode root,
        string sourceText,
        string? filePath,
        string language)
    {
        Root = root ?? throw new ArgumentNullException(nameof(root));
        SourceText = sourceText ?? throw new ArgumentNullException(nameof(sourceText));
        FilePath = filePath;
        Language = language ?? throw new ArgumentNullException(nameof(language));
        _errors = new List<SyntaxError>();

        // Extract errors from the tree
        ExtractErrors();
    }

    /// <inheritdoc/>
    public IAstNode Root { get; }

    /// <inheritdoc/>
    public string SourceText { get; }

    /// <inheritdoc/>
    public string? FilePath { get; }

    /// <inheritdoc/>
    public string Language { get; }

    /// <inheritdoc/>
    public IReadOnlyList<SyntaxError> Errors => _errors;

    /// <inheritdoc/>
    public bool HasErrors => _errors.Count > 0;

    /// <inheritdoc/>
    public IEnumerable<T> GetNodes<T>() where T : class, IAstNode
    {
        return GetAllNodes().OfType<T>();
    }

    /// <inheritdoc/>
    public IAstNode? GetNodeAtPosition(int position)
    {
        if (position < 0 || position >= SourceText.Length)
        {
            return null;
        }

        return FindNodeAtPosition(Root, position);
    }

    /// <inheritdoc/>
    public IAstNode? GetNodeAtLocation(int line, int column)
    {
        if (line < 1 || column < 1)
        {
            return null;
        }

        var position = GetPositionFromLineColumn(line, column);
        return position >= 0 ? GetNodeAtPosition(position) : null;
    }

    /// <inheritdoc/>
    public ICompilationUnitDefinition ToDefinition()
    {
        var converter = new TreeSitterToDefinitionConverter(Language);
        return converter.ConvertCompilationUnit(this);
    }

    /// <summary>
    /// Gets all nodes in the syntax tree.
    /// </summary>
    /// <returns>An enumerable of all nodes in the tree.</returns>
    public IEnumerable<IAstNode> GetAllNodes()
    {
        yield return Root;
        
        foreach (var descendant in GetDescendants(Root))
        {
            yield return descendant;
        }
    }

    /// <summary>
    /// Gets nodes by their type.
    /// </summary>
    /// <param name="nodeType">The type of nodes to retrieve.</param>
    /// <returns>An enumerable of nodes with the specified type.</returns>
    public IEnumerable<IAstNode> GetNodesByType(string nodeType)
    {
        return GetAllNodes().Where(n => string.Equals(n.NodeType, nodeType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the text for a specific node.
    /// </summary>
    /// <param name="node">The node to get text for.</param>
    /// <returns>The text content of the node.</returns>
    public string GetNodeText(IAstNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.Location == null)
        {
            return string.Empty;
        }

        var start = node.Location.StartPosition;
        var end = node.Location.EndPosition;

        if (start >= 0 && end > start && end <= SourceText.Length)
        {
            return SourceText.Substring(start, end - start);
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the line and column from a position in the source text.
    /// </summary>
    /// <param name="position">The position in the source text.</param>
    /// <returns>A tuple containing the line and column (1-based).</returns>
    public (int Line, int Column) GetLineColumnFromPosition(int position)
    {
        if (position < 0 || position > SourceText.Length)
        {
            return (1, 1);
        }

        var line = 1;
        var column = 1;

        for (var i = 0; i < position && i < SourceText.Length; i++)
        {
            if (SourceText[i] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        return (line, column);
    }

    /// <summary>
    /// Gets metrics about the syntax tree.
    /// </summary>
    /// <returns>Syntax tree metrics.</returns>
    public SyntaxTreeMetrics GetMetrics()
    {
        var allNodes = GetAllNodes().ToList();
        var nodeTypeCounts = allNodes
            .GroupBy(n => n.NodeType)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        return new SyntaxTreeMetrics
        {
            TotalNodes = allNodes.Count,
            MaxDepth = CalculateMaxDepth(Root),
            ErrorCount = _errors.Count,
            NodeTypeCounts = nodeTypeCounts,
            SourceLength = SourceText.Length,
            LineCount = SourceText.Count(c => c == '\n') + 1
        };
    }

    private void ExtractErrors()
    {
        foreach (var node in GetAllNodes().OfType<TreeSitterAstNode>())
        {
            if (node.IsError)
            {
                var error = new SyntaxError
                {
                    Message = "Syntax error",
                    Severity = ErrorSeverity.Error,
                    Location = node.Location,
                    FilePath = FilePath
                };
                _errors.Add(error);
            }
        }
    }

    private IEnumerable<IAstNode> GetDescendants(IAstNode node)
    {
        foreach (var child in node.Children)
        {
            yield return child;
            foreach (var descendant in GetDescendants(child))
            {
                yield return descendant;
            }
        }
    }

    private IAstNode? FindNodeAtPosition(IAstNode node, int position)
    {
        if (node.Location != null &&
            position >= node.Location.StartPosition &&
            position < node.Location.EndPosition)
        {
            // Check children first (most specific match)
            foreach (var child in node.Children)
            {
                var childResult = FindNodeAtPosition(child, position);
                if (childResult != null)
                {
                    return childResult;
                }
            }

            // Return this node if no child contains the position
            return node;
        }

        return null;
    }

    private int GetPositionFromLineColumn(int line, int column)
    {
        var currentLine = 1;
        var currentColumn = 1;

        for (var i = 0; i < SourceText.Length; i++)
        {
            if (currentLine == line && currentColumn == column)
            {
                return i;
            }

            if (SourceText[i] == '\n')
            {
                currentLine++;
                currentColumn = 1;
            }
            else
            {
                currentColumn++;
            }
        }

        return -1;
    }

    private int CalculateMaxDepth(IAstNode node, int currentDepth = 0)
    {
        var maxDepth = currentDepth;

        foreach (var child in node.Children)
        {
            var childDepth = CalculateMaxDepth(child, currentDepth + 1);
            maxDepth = Math.Max(maxDepth, childDepth);
        }

        return maxDepth;
    }
}

/// <summary>
/// Represents metrics about a syntax tree.
/// </summary>
public sealed record SyntaxTreeMetrics
{
    /// <summary>
    /// Gets the total number of nodes in the tree.
    /// </summary>
    public int TotalNodes { get; init; }

    /// <summary>
    /// Gets the maximum depth of the tree.
    /// </summary>
    public int MaxDepth { get; init; }

    /// <summary>
    /// Gets the number of syntax errors in the tree.
    /// </summary>
    public int ErrorCount { get; init; }

    /// <summary>
    /// Gets the count of nodes by type.
    /// </summary>
    public IReadOnlyDictionary<string, int> NodeTypeCounts { get; init; } = 
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the length of the source text.
    /// </summary>
    public int SourceLength { get; init; }

    /// <summary>
    /// Gets the number of lines in the source text.
    /// </summary>
    public int LineCount { get; init; }
}