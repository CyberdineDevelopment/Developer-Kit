using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FractalDataWorks.CodeBuilder.Roslyn;

/// <summary>
/// Roslyn implementation of a syntax tree with full semantic analysis support.
/// Wraps Roslyn's SyntaxTree and provides semantic information.
/// </summary>
public sealed class RoslynSyntaxTree : ISyntaxTree
{
    private readonly SyntaxTree _syntaxTree;
    private readonly ImmutableArray<MetadataReference> _references;
    private readonly Lazy<RoslynAstNode> _root;
    private readonly Lazy<IReadOnlyList<SyntaxError>> _errors;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoslynSyntaxTree"/> class.
    /// </summary>
    /// <param name="syntaxTree">The underlying Roslyn syntax tree.</param>
    /// <param name="references">Assembly references for semantic analysis.</param>
    public RoslynSyntaxTree(SyntaxTree syntaxTree, ImmutableArray<MetadataReference> references)
    {
        _syntaxTree = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));
        _references = references;
        _root = new Lazy<RoslynAstNode>(() => new RoslynAstNode(_syntaxTree.GetRoot()));
        _errors = new Lazy<IReadOnlyList<SyntaxError>>(ExtractSyntaxErrors);
    }

    /// <inheritdoc/>
    public IAstNode Root => _root.Value;

    /// <inheritdoc/>
    public string SourceText => _syntaxTree.GetText().ToString();

    /// <inheritdoc/>
    public string? FilePath => _syntaxTree.FilePath;

    /// <inheritdoc/>
    public string Language => "csharp";

    /// <inheritdoc/>
    public IReadOnlyList<SyntaxError> Errors => _errors.Value;

    /// <inheritdoc/>
    public bool HasErrors => _errors.Value.Count > 0;

    /// <summary>
    /// Gets the underlying Roslyn syntax tree.
    /// </summary>
    public SyntaxTree UnderlyingSyntaxTree => _syntaxTree;

    /// <summary>
    /// Gets the assembly references used for semantic analysis.
    /// </summary>
    public ImmutableArray<MetadataReference> References => _references;

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

        var syntaxNode = _syntaxTree.GetRoot().FindNode(Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(position, position + 1));
        return syntaxNode != null ? new RoslynAstNode(syntaxNode) : null;
    }

    /// <inheritdoc/>
    public IAstNode? GetNodeAtLocation(int line, int column)
    {
        if (line < 1 || column < 1)
        {
            return null;
        }

        var text = _syntaxTree.GetText();
        var position = text.Lines[line - 1].Start + column - 1;
        
        if (position < 0 || position >= text.Length)
        {
            return null;
        }

        return GetNodeAtPosition(position);
    }

    /// <inheritdoc/>
    public ICompilationUnitDefinition ToDefinition()
    {
        var converter = new RoslynToDefinitionConverter();
        return converter.ConvertCompilationUnit(_syntaxTree);
    }

    /// <summary>
    /// Creates a compilation with semantic analysis capabilities.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the compilation.</returns>
    public async Task<Compilation> CreateCompilationAsync(
        string assemblyName,
        CancellationToken cancellationToken = default)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { _syntaxTree },
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        await Task.CompletedTask; // Keep async signature for consistency
        return compilation;
    }

    /// <summary>
    /// Gets semantic information at a specific position.
    /// </summary>
    /// <param name="position">The position in the source text.</param>
    /// <param name="compilation">The compilation for semantic analysis.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Semantic information at the position.</returns>
    public async Task<SemanticInfo?> GetSemanticInfoAsync(
        int position,
        Compilation? compilation = null,
        CancellationToken cancellationToken = default)
    {
        compilation ??= await CreateCompilationAsync("TempAssembly", cancellationToken).ConfigureAwait(false);
        var semanticModel = compilation.GetSemanticModel(_syntaxTree);

        var node = _syntaxTree.GetRoot().FindNode(Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(position, position + 1));
        if (node == null)
        {
            return null;
        }

        var symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);
        var typeInfo = semanticModel.GetTypeInfo(node, cancellationToken);

        return new SemanticInfo
        {
            Symbol = symbolInfo.Symbol?.Name,
            Type = typeInfo.Type?.ToDisplayString(),
            SymbolKind = symbolInfo.Symbol?.Kind.ToString(),
            Documentation = symbolInfo.Symbol?.GetDocumentationCommentXml(),
            IsDeprecated = symbolInfo.Symbol?.GetAttributes()
                .Any(a => a.AttributeClass?.Name == "ObsoleteAttribute") ?? false
        };
    }

    /// <summary>
    /// Gets code completions at a specific position.
    /// </summary>
    /// <param name="position">The position in the source text.</param>
    /// <param name="compilation">The compilation for semantic analysis.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Code completion items at the position.</returns>
    public async Task<IReadOnlyList<CompletionItem>> GetCompletionsAsync(
        int position,
        Compilation? compilation = null,
        CancellationToken cancellationToken = default)
    {
        compilation ??= await CreateCompilationAsync("TempAssembly", cancellationToken).ConfigureAwait(false);
        var semanticModel = compilation.GetSemanticModel(_syntaxTree);

        // This is a simplified implementation - a full implementation would use
        // Microsoft.CodeAnalysis.Completion APIs
        var completions = new List<CompletionItem>();

        // Get symbols in scope at the position
        var symbols = semanticModel.LookupSymbols(position);
        foreach (var symbol in symbols)
        {
            var completion = new CompletionItem
            {
                Label = symbol.Name,
                InsertText = symbol.Name,
                Kind = MapSymbolKindToCompletionKind(symbol.Kind),
                Detail = symbol.ToDisplayString(),
                Documentation = symbol.GetDocumentationCommentXml(),
                IsDeprecated = symbol.GetAttributes()
                    .Any(a => a.AttributeClass?.Name == "ObsoleteAttribute")
            };
            completions.Add(completion);
        }

        return completions;
    }

    /// <summary>
    /// Gets all compilation diagnostics.
    /// </summary>
    /// <param name="compilation">The compilation to get diagnostics from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All compilation diagnostics.</returns>
    public async Task<IReadOnlyList<CompilationDiagnostic>> GetDiagnosticsAsync(
        Compilation? compilation = null,
        CancellationToken cancellationToken = default)
    {
        compilation ??= await CreateCompilationAsync("TempAssembly", cancellationToken).ConfigureAwait(false);
        
        var diagnostics = compilation.GetDiagnostics(cancellationToken);
        return diagnostics.Select(d => new CompilationDiagnostic
        {
            Message = d.GetMessage(),
            Code = d.Id,
            Severity = MapDiagnosticSeverity(d.Severity),
            Location = d.Location.IsInSource ? CreateSourceLocation(d.Location) : null,
            FilePath = d.Location.IsInSource ? d.Location.SourceTree?.FilePath : null,
            Category = d.Category
        }).ToArray();
    }

    /// <summary>
    /// Gets all nodes in the syntax tree.
    /// </summary>
    /// <returns>An enumerable of all nodes in the tree.</returns>
    public IEnumerable<IAstNode> GetAllNodes()
    {
        return GetAllRoslynNodes(_syntaxTree.GetRoot()).Select(n => new RoslynAstNode(n));
    }

    /// <summary>
    /// Gets nodes by their syntax kind.
    /// </summary>
    /// <param name="syntaxKind">The syntax kind to filter by.</param>
    /// <returns>An enumerable of nodes with the specified syntax kind.</returns>
    public IEnumerable<IAstNode> GetNodesBySyntaxKind(SyntaxKind syntaxKind)
    {
        return _syntaxTree.GetRoot()
            .DescendantNodes()
            .Where(n => n.IsKind(syntaxKind))
            .Select(n => new RoslynAstNode(n));
    }

    /// <summary>
    /// Gets the text for a specific node.
    /// </summary>
    /// <param name="node">The node to get text for.</param>
    /// <returns>The text content of the node.</returns>
    public string GetNodeText(IAstNode node)
    {
        if (node is RoslynAstNode roslynNode)
        {
            return roslynNode.UnderlyingSyntaxNode.GetText().ToString();
        }

        return node.Location != null && node.Location.StartPosition >= 0 && node.Location.EndPosition <= SourceText.Length
            ? SourceText.Substring(node.Location.StartPosition, node.Location.EndPosition - node.Location.StartPosition)
            : string.Empty;
    }

    private IReadOnlyList<SyntaxError> ExtractSyntaxErrors()
    {
        var diagnostics = _syntaxTree.GetDiagnostics();
        return diagnostics.Select(d => new SyntaxError
        {
            Message = d.GetMessage(),
            Code = d.Id,
            Severity = MapDiagnosticSeverity(d.Severity),
            Location = d.Location.IsInSource ? CreateSourceLocation(d.Location) : null,
            FilePath = d.Location.IsInSource ? d.Location.SourceTree?.FilePath : null
        }).ToArray();
    }

    private static ErrorSeverity MapDiagnosticSeverity(DiagnosticSeverity severity)
    {
        return severity switch
        {
            DiagnosticSeverity.Hidden => ErrorSeverity.Hidden,
            DiagnosticSeverity.Info => ErrorSeverity.Info,
            DiagnosticSeverity.Warning => ErrorSeverity.Warning,
            DiagnosticSeverity.Error => ErrorSeverity.Error,
            _ => ErrorSeverity.Error
        };
    }

    private static CompletionKind MapSymbolKindToCompletionKind(SymbolKind symbolKind)
    {
        return symbolKind switch
        {
            SymbolKind.Method => CompletionKind.Method,
            SymbolKind.Property => CompletionKind.Property,
            SymbolKind.Field => CompletionKind.Field,
            SymbolKind.Local => CompletionKind.Variable,
            SymbolKind.NamedType => CompletionKind.Class,
            SymbolKind.Namespace => CompletionKind.Namespace,
            _ => CompletionKind.Text
        };
    }

    private static SourceLocation CreateSourceLocation(Location location)
    {
        var span = location.GetLineSpan();
        return new SourceLocation
        {
            FilePath = location.SourceTree?.FilePath,
            StartLine = span.StartLinePosition.Line + 1,
            StartColumn = span.StartLinePosition.Character + 1,
            EndLine = span.EndLinePosition.Line + 1,
            EndColumn = span.EndLinePosition.Character + 1,
            StartPosition = location.SourceSpan.Start,
            EndPosition = location.SourceSpan.End
        };
    }

    private static IEnumerable<SyntaxNode> GetAllRoslynNodes(SyntaxNode root)
    {
        yield return root;
        foreach (var child in root.ChildNodes())
        {
            foreach (var descendant in GetAllRoslynNodes(child))
            {
                yield return descendant;
            }
        }
    }
}