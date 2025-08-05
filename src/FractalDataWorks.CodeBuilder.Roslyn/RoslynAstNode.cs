using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.CodeBuilder.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FractalDataWorks.CodeBuilder.Roslyn;

/// <summary>
/// Roslyn implementation of an AST node.
/// Wraps Roslyn's SyntaxNode and provides the common AST interface.
/// </summary>
public sealed class RoslynAstNode : IAstNode
{
    private readonly SyntaxNode _syntaxNode;
    private readonly Lazy<IReadOnlyList<IAstNode>> _children;
    private readonly Lazy<IReadOnlyDictionary<string, object>> _metadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoslynAstNode"/> class.
    /// </summary>
    /// <param name="syntaxNode">The underlying Roslyn syntax node.</param>
    public RoslynAstNode(SyntaxNode syntaxNode)
    {
        _syntaxNode = syntaxNode ?? throw new ArgumentNullException(nameof(syntaxNode));
        Id = Guid.NewGuid();
        _children = new Lazy<IReadOnlyList<IAstNode>>(() => 
            _syntaxNode.ChildNodes().Select(child => new RoslynAstNode(child)).ToArray());
        _metadata = new Lazy<IReadOnlyDictionary<string, object>>(CreateMetadata);
    }

    /// <inheritdoc/>
    public Guid Id { get; }

    /// <inheritdoc/>
    public string NodeType => _syntaxNode.Kind().ToString();

    /// <inheritdoc/>
    public string? Name => ExtractNodeName();

    /// <inheritdoc/>
    public IAstNode? Parent => _syntaxNode.Parent != null ? new RoslynAstNode(_syntaxNode.Parent) : null;

    /// <inheritdoc/>
    public IReadOnlyList<IAstNode> Children => _children.Value;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Metadata => _metadata.Value;

    /// <inheritdoc/>
    public SourceLocation? Location => CreateSourceLocation();

    /// <summary>
    /// Gets the underlying Roslyn syntax node.
    /// </summary>
    public SyntaxNode UnderlyingSyntaxNode => _syntaxNode;

    /// <summary>
    /// Gets the syntax kind of the underlying node.
    /// </summary>
    public SyntaxKind SyntaxKind => _syntaxNode.Kind();

    /// <summary>
    /// Gets whether this node has any compilation errors.
    /// </summary>
    public bool HasErrors => _syntaxNode.ContainsDiagnostics;

    /// <summary>
    /// Gets whether this node is missing from the source (inserted by error recovery).
    /// </summary>
    public bool IsMissing => _syntaxNode.IsMissing;

    /// <inheritdoc/>
    public IAstNode? GetChild(string name)
    {
        return _children.Value.FirstOrDefault(c => 
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IEnumerable<T> GetChildren<T>() where T : class, IAstNode
    {
        return _children.Value.OfType<T>();
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
    public IEnumerable<RoslynAstNode> GetDescendants()
    {
        return _syntaxNode.DescendantNodes().Select(n => new RoslynAstNode(n));
    }

    /// <summary>
    /// Gets all descendant nodes of a specific syntax kind.
    /// </summary>
    /// <param name="syntaxKind">The syntax kind to filter by.</param>
    /// <returns>An enumerable of descendant nodes with the specified syntax kind.</returns>
    public IEnumerable<RoslynAstNode> GetDescendants(SyntaxKind syntaxKind)
    {
        return _syntaxNode.DescendantNodes()
            .Where(n => n.IsKind(syntaxKind))
            .Select(n => new RoslynAstNode(n));
    }

    /// <summary>
    /// Gets the first descendant node of a specific syntax kind.
    /// </summary>
    /// <param name="syntaxKind">The syntax kind to search for.</param>
    /// <returns>The first descendant node with the specified syntax kind, or null if not found.</returns>
    public RoslynAstNode? GetFirstDescendant(SyntaxKind syntaxKind)
    {
        var node = _syntaxNode.DescendantNodes().FirstOrDefault(n => n.IsKind(syntaxKind));
        return node != null ? new RoslynAstNode(node) : null;
    }

    /// <summary>
    /// Gets the text content of this node.
    /// </summary>
    /// <returns>The text content of this node.</returns>
    public string GetText()
    {
        return _syntaxNode.GetText().ToString();
    }

    /// <summary>
    /// Gets the full text content of this node including trivia.
    /// </summary>
    /// <returns>The full text content including trivia.</returns>
    public string GetFullText()
    {
        return _syntaxNode.ToFullString();
    }

    /// <summary>
    /// Gets the leading trivia (whitespace, comments, etc.) for this node.
    /// </summary>
    /// <returns>The leading trivia.</returns>
    public string GetLeadingTrivia()
    {
        return _syntaxNode.GetLeadingTrivia().ToFullString();
    }

    /// <summary>
    /// Gets the trailing trivia (whitespace, comments, etc.) for this node.
    /// </summary>
    /// <returns>The trailing trivia.</returns>
    public string GetTrailingTrivia()
    {
        return _syntaxNode.GetTrailingTrivia().ToFullString();
    }

    /// <summary>
    /// Checks if this node is of a specific syntax kind.
    /// </summary>
    /// <param name="syntaxKind">The syntax kind to check.</param>
    /// <returns>True if this node is of the specified syntax kind; otherwise, false.</returns>
    public bool IsKind(SyntaxKind syntaxKind)
    {
        return _syntaxNode.IsKind(syntaxKind);
    }

    /// <summary>
    /// Checks if this node is of any of the specified syntax kinds.
    /// </summary>
    /// <param name="syntaxKinds">The syntax kinds to check.</param>
    /// <returns>True if this node is of any of the specified syntax kinds; otherwise, false.</returns>
    public bool IsKind(params SyntaxKind[] syntaxKinds)
    {
        return _syntaxNode.IsKind(syntaxKinds);
    }

    /// <summary>
    /// Gets ancestors of this node.
    /// </summary>
    /// <returns>An enumerable of ancestor nodes.</returns>
    public IEnumerable<RoslynAstNode> GetAncestors()
    {
        return _syntaxNode.Ancestors().Select(n => new RoslynAstNode(n));
    }

    /// <summary>
    /// Gets the first ancestor of a specific syntax kind.
    /// </summary>
    /// <param name="syntaxKind">The syntax kind to search for.</param>
    /// <returns>The first ancestor with the specified syntax kind, or null if not found.</returns>
    public RoslynAstNode? GetFirstAncestor(SyntaxKind syntaxKind)
    {
        var ancestor = _syntaxNode.Ancestors().FirstOrDefault(n => n.IsKind(syntaxKind));
        return ancestor != null ? new RoslynAstNode(ancestor) : null;
    }

    /// <summary>
    /// Returns a string representation of this node.
    /// </summary>
    /// <returns>A string representation of this node.</returns>
    public override string ToString()
    {
        var text = GetText();
        var truncatedText = text.Length > 50 ? text.Substring(0, 47) + "..." : text;
        var name = !string.IsNullOrEmpty(Name) ? $" ({Name})" : string.Empty;
        return $"{NodeType}{name}: {truncatedText.Replace('\n', ' ').Replace('\r', ' ')}";
    }

    private string? ExtractNodeName()
    {
        // Extract names based on the syntax kind
        return _syntaxNode switch
        {
            Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classDecl => classDecl.Identifier.ValueText,
            Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax structDecl => structDecl.Identifier.ValueText,
            Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax interfaceDecl => interfaceDecl.Identifier.ValueText,
            Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax enumDecl => enumDecl.Identifier.ValueText,
            Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodDecl => methodDecl.Identifier.ValueText,
            Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax propertyDecl => propertyDecl.Identifier.ValueText,
            Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax fieldDecl => ExtractFieldName(fieldDecl),
            Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax variableDecl => variableDecl.Identifier.ValueText,
            Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax paramDecl => paramDecl.Identifier.ValueText,
            Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax namespaceDecl => namespaceDecl.Name.ToString(),
            Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDecl => fileScopedNamespaceDecl.Name.ToString(),
            Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax usingDecl => usingDecl.Name?.ToString(),
            Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            _ => null
        };
    }

    private static string? ExtractFieldName(Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax fieldDecl)
    {
        var declarator = fieldDecl.Declaration.Variables.FirstOrDefault();
        return declarator?.Identifier.ValueText;
    }

    private SourceLocation? CreateSourceLocation()
    {
        if (_syntaxNode.SyntaxTree == null)
        {
            return null;
        }

        var span = _syntaxNode.GetLocation().GetLineSpan();
        return new SourceLocation
        {
            FilePath = _syntaxNode.SyntaxTree.FilePath,
            StartLine = span.StartLinePosition.Line + 1,
            StartColumn = span.StartLinePosition.Character + 1,
            EndLine = span.EndLinePosition.Line + 1,
            EndColumn = span.EndLinePosition.Character + 1,
            StartPosition = _syntaxNode.SpanStart,
            EndPosition = _syntaxNode.Span.End
        };
    }

    private IReadOnlyDictionary<string, object> CreateMetadata()
    {
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["SyntaxKind"] = _syntaxNode.Kind(),
            ["HasErrors"] = _syntaxNode.ContainsDiagnostics,
            ["IsMissing"] = _syntaxNode.IsMissing,
            ["SpanStart"] = _syntaxNode.SpanStart,
            ["SpanEnd"] = _syntaxNode.Span.End,
            ["FullSpanStart"] = _syntaxNode.FullSpan.Start,
            ["FullSpanEnd"] = _syntaxNode.FullSpan.End
        };

        // Add language-specific metadata
        switch (_syntaxNode)
        {
            case Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax method:
                metadata["ReturnType"] = method.ReturnType.ToString();
                metadata["Modifiers"] = method.Modifiers.ToString();
                metadata["ParameterCount"] = method.ParameterList.Parameters.Count;
                break;

            case Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classDecl:
                metadata["Modifiers"] = classDecl.Modifiers.ToString();
                metadata["BaseTypes"] = classDecl.BaseList?.Types.ToString() ?? string.Empty;
                metadata["TypeParameterCount"] = classDecl.TypeParameterList?.Parameters.Count ?? 0;
                break;

            case Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax property:
                metadata["Type"] = property.Type.ToString();
                metadata["Modifiers"] = property.Modifiers.ToString();
                metadata["HasGetter"] = property.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false;
                metadata["HasSetter"] = property.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false;
                break;
        }

        return metadata;
    }
}