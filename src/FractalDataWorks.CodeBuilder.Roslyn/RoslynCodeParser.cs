using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.CodeBuilder.Roslyn;

/// <summary>
/// Roslyn-based code parser for C# with full semantic analysis.
/// Provides advanced parsing capabilities using Microsoft's Roslyn compiler.
/// </summary>
public sealed class RoslynCodeParser : ICodeParser
{
    private readonly ILogger<RoslynCodeParser> _logger;
    private readonly RoslynParseOptions _parseOptions;
    private readonly ImmutableArray<MetadataReference> _references;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoslynCodeParser"/> class.
    /// </summary>
    /// <param name="references">Assembly references for compilation.</param>
    /// <param name="parseOptions">Parse options for the parser.</param>
    /// <param name="logger">Optional logger instance.</param>
    public RoslynCodeParser(
        IEnumerable<MetadataReference>? references = null,
        RoslynParseOptions? parseOptions = null,
        ILogger<RoslynCodeParser>? logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RoslynCodeParser>.Instance;
        _parseOptions = parseOptions ?? RoslynParseOptions.Default;
        _references = references?.ToImmutableArray() ?? ImmutableArray<MetadataReference>.Empty;
    }

    /// <inheritdoc/>
    public string Language => "csharp";

    /// <inheritdoc/>
    public IReadOnlyList<string> SupportedExtensions => new[] { ".cs" };

    /// <inheritdoc/>
    public async Task<IFdwResult<ISyntaxTree>> ParseAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(source))
        {
            return FdwResult<ISyntaxTree>.Failure(new RoslynParsingError("Source code cannot be null or empty"));
        }

        try
        {
            _logger.LogDebug("Parsing C# source code using Roslyn");

            var parseOptions = CSharpParseOptions.Default
                .WithLanguageVersion(_parseOptions.LanguageVersion)
                .WithKind(_parseOptions.SourceCodeKind);

            var syntaxTree = CSharpSyntaxTree.ParseText(
                source, 
                parseOptions, 
                filePath ?? "");

            await Task.CompletedTask; // Keep async signature for consistency

            var roslynSyntaxTree = new RoslynSyntaxTree(syntaxTree, _references);

            _logger.LogDebug("Successfully parsed C# source code using Roslyn");
            return FdwResult<ISyntaxTree>.Success(roslynSyntaxTree);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error parsing C# source code using Roslyn");
            return FdwResult<ISyntaxTree>.Failure(new RoslynParsingError($"Parsing failed: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<ICompilationUnitDefinition>> ParseToDefinitionAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseAsync(source, filePath, cancellationToken).ConfigureAwait(false);
        if (parseResult.IsFailure)
        {
            return FdwResult<ICompilationUnitDefinition>.Failure(parseResult.Message!);
        }

        try
        {
            var definition = parseResult.Value!.ToDefinition();
            return FdwResult<ICompilationUnitDefinition>.Success(definition);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error converting Roslyn syntax tree to definition");
            return FdwResult<ICompilationUnitDefinition>.Failure(new RoslynParsingError($"Definition conversion failed: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<bool>> ValidateSyntaxAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseAsync(source, filePath, cancellationToken).ConfigureAwait(false);
        if (parseResult.IsFailure)
        {
            return FdwResult<bool>.Success(false);
        }

        var hasErrors = parseResult.Value!.HasErrors;
        return FdwResult<bool>.Success(!hasErrors);
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IReadOnlyList<SyntaxError>>> GetSyntaxErrorsAsync(
        string source, 
        string? filePath = null, 
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseAsync(source, filePath, cancellationToken).ConfigureAwait(false);
        if (parseResult.IsFailure)
        {
            return FdwResult<IReadOnlyList<SyntaxError>>.Success(Array.Empty<SyntaxError>());
        }

        return FdwResult<IReadOnlyList<SyntaxError>>.Success(parseResult.Value!.Errors);
    }

    /// <summary>
    /// Parses source code and creates a compilation with semantic analysis.
    /// </summary>
    /// <param name="source">The source code to parse.</param>
    /// <param name="filePath">Optional file path for error reporting.</param>
    /// <param name="assemblyName">Optional assembly name for the compilation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the Roslyn compilation.</returns>
    public async Task<IFdwResult<Compilation>> ParseWithSemanticAnalysisAsync(
        string source,
        string? filePath = null,
        string? assemblyName = null,
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseAsync(source, filePath, cancellationToken).ConfigureAwait(false);
        if (parseResult.IsFailure)
        {
            return FdwResult<Compilation>.Failure(parseResult.Message!);
        }

        try
        {
            var roslynSyntaxTree = (RoslynSyntaxTree)parseResult.Value!;
            var compilation = await roslynSyntaxTree.CreateCompilationAsync(
                assemblyName ?? "GeneratedAssembly",
                cancellationToken).ConfigureAwait(false);

            return FdwResult<Compilation>.Success(compilation);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error creating compilation with semantic analysis");
            return FdwResult<Compilation>.Failure(new RoslynParsingError($"Semantic analysis failed: {ex.Message}"));
        }
    }
}

/// <summary>
/// Parse options for Roslyn parsing.
/// </summary>
public sealed record RoslynParseOptions
{
    /// <summary>
    /// Gets the default parse options.
    /// </summary>
    public static RoslynParseOptions Default { get; } = new()
    {
        LanguageVersion = LanguageVersion.Latest,
        SourceCodeKind = SourceCodeKind.Regular
    };

    /// <summary>
    /// Gets or sets the C# language version.
    /// </summary>
    public LanguageVersion LanguageVersion { get; init; } = LanguageVersion.Latest;

    /// <summary>
    /// Gets or sets the source code kind.
    /// </summary>
    public SourceCodeKind SourceCodeKind { get; init; } = SourceCodeKind.Regular;

    /// <summary>
    /// Gets or sets additional parse options.
    /// </summary>
    public IReadOnlyDictionary<string, object> AdditionalOptions { get; init; } = 
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Error message type for Roslyn parsing errors.
/// </summary>
public sealed record RoslynParsingError : IFdwMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoslynParsingError"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public RoslynParsingError(string message)
    {
        Message = message;
    }

    /// <inheritdoc/>
    public string Message { get; }

    /// <inheritdoc/>
    public string Format(params object[] args) => string.Format(Message, args);
}