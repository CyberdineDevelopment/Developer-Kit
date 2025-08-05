using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Represents a code session that maintains compilation context and allows incremental updates.
/// This provides context-efficient operations by caching parsed code and semantic information.
/// </summary>
public interface ICodeSession : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this session.
    /// </summary>
    Guid SessionId { get; }

    /// <summary>
    /// Gets the language for this session.
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Gets the creation timestamp for this session.
    /// </summary>
    DateTimeOffset Created { get; }

    /// <summary>
    /// Gets the last modified timestamp for this session.
    /// </summary>
    DateTimeOffset LastModified { get; }

    /// <summary>
    /// Gets whether this session is still valid.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Gets the primary source files in this session.
    /// </summary>
    IReadOnlyDictionary<string, string> SourceFiles { get; }

    /// <summary>
    /// Gets the assembly references for this session.
    /// </summary>
    IReadOnlyList<string> References { get; }

    /// <summary>
    /// Gets the current syntax trees in this session.
    /// </summary>
    IReadOnlyList<ISyntaxTree> SyntaxTrees { get; }

    /// <summary>
    /// Gets whether the session has any compilation errors.
    /// </summary>
    bool HasErrors { get; }

    /// <summary>
    /// Gets all compilation diagnostics for this session.
    /// </summary>
    IReadOnlyList<CompilationDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Adds or updates a source file in this session.
    /// </summary>
    /// <param name="filePath">The file path or identifier.</param>
    /// <param name="source">The source code content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<IFdwResult> UpdateSourceAsync(
        string filePath, 
        string source, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a source file from this session.
    /// </summary>
    /// <param name="filePath">The file path or identifier to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<IFdwResult> RemoveSourceAsync(
        string filePath, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an assembly reference to this session.
    /// </summary>
    /// <param name="assemblyPath">The path to the assembly.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<IFdwResult> AddReferenceAsync(
        string assemblyPath, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a code transformation to this session.
    /// </summary>
    /// <param name="transformation">The transformation to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the transformation results.</returns>
    Task<IFdwResult<TransformationResult>> ApplyTransformationAsync(
        ICodeTransformation transformation, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets semantic information at a specific position.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="position">The position in the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing semantic information.</returns>
    Task<IFdwResult<SemanticInfo>> GetSemanticInfoAsync(
        string filePath, 
        int position, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets code completions at a specific position.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="position">The position in the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing completion items.</returns>
    Task<IFdwResult<IReadOnlyList<CompletionItem>>> GetCompletionsAsync(
        string filePath, 
        int position, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compiles the current session and returns the result.
    /// </summary>
    /// <param name="outputPath">Optional output path for compilation artifacts.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing compilation information.</returns>
    Task<IFdwResult<CompilationResult>> CompileAsync(
        string? outputPath = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a snapshot of the current session state.
    /// </summary>
    /// <returns>An immutable snapshot of the session.</returns>
    ICodeSessionSnapshot CreateSnapshot();

    /// <summary>
    /// Restores the session from a snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to restore from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<IFdwResult> RestoreFromSnapshotAsync(
        ICodeSessionSnapshot snapshot, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an immutable snapshot of a code session.
/// </summary>
public interface ICodeSessionSnapshot
{
    /// <summary>
    /// Gets the session ID this snapshot was created from.
    /// </summary>
    Guid SessionId { get; }

    /// <summary>
    /// Gets the timestamp when this snapshot was created.
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the language for this snapshot.
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Gets the source files at the time of the snapshot.
    /// </summary>
    IReadOnlyDictionary<string, string> SourceFiles { get; }

    /// <summary>
    /// Gets the assembly references at the time of the snapshot.
    /// </summary>
    IReadOnlyList<string> References { get; }

    /// <summary>
    /// Gets the compilation diagnostics at the time of the snapshot.
    /// </summary>
    IReadOnlyList<CompilationDiagnostic> Diagnostics { get; }
}

/// <summary>
/// Represents a compilation diagnostic (error, warning, or info).
/// </summary>
public sealed record CompilationDiagnostic
{
    /// <summary>
    /// Gets the diagnostic message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the diagnostic code.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Gets the severity of the diagnostic.
    /// </summary>
    public ErrorSeverity Severity { get; init; } = ErrorSeverity.Error;

    /// <summary>
    /// Gets the location of the diagnostic.
    /// </summary>
    public SourceLocation? Location { get; init; }

    /// <summary>
    /// Gets the file path where the diagnostic occurred.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets the category of the diagnostic.
    /// </summary>
    public string Category { get; init; } = string.Empty;
}

/// <summary>
/// Represents semantic information at a specific code location.
/// </summary>
public sealed record SemanticInfo
{
    /// <summary>
    /// Gets the symbol at the location, if any.
    /// </summary>
    public string? Symbol { get; init; }

    /// <summary>
    /// Gets the type of the symbol, if available.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets the kind of symbol (class, method, property, etc.).
    /// </summary>
    public string? SymbolKind { get; init; }

    /// <summary>
    /// Gets the documentation for the symbol, if available.
    /// </summary>
    public string? Documentation { get; init; }

    /// <summary>
    /// Gets whether the symbol is deprecated.
    /// </summary>
    public bool IsDeprecated { get; init; }

    /// <summary>
    /// Gets additional metadata about the symbol.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = 
        new Dictionary<string, object>(StringComparer.Ordinal);
}

/// <summary>
/// Represents a code completion item.
/// </summary>
public sealed record CompletionItem
{
    /// <summary>
    /// Gets the label for this completion item.
    /// </summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// Gets the text to insert when this completion is selected.
    /// </summary>
    public string InsertText { get; init; } = string.Empty;

    /// <summary>
    /// Gets the kind of completion item.
    /// </summary>
    public CompletionKind Kind { get; init; } = CompletionKind.Text;

    /// <summary>
    /// Gets the detail text for this completion item.
    /// </summary>
    public string? Detail { get; init; }

    /// <summary>
    /// Gets the documentation for this completion item.
    /// </summary>
    public string? Documentation { get; init; }

    /// <summary>
    /// Gets the sort priority for this completion item.
    /// </summary>
    public int SortPriority { get; init; }

    /// <summary>
    /// Gets whether this completion item is deprecated.
    /// </summary>
    public bool IsDeprecated { get; init; }
}

/// <summary>
/// Represents the kind of completion item.
/// </summary>
public enum CompletionKind
{
    /// <summary>Plain text completion.</summary>
    Text,
    /// <summary>Method completion.</summary>
    Method,
    /// <summary>Property completion.</summary>
    Property,
    /// <summary>Field completion.</summary>
    Field,
    /// <summary>Variable completion.</summary>
    Variable,
    /// <summary>Class completion.</summary>
    Class,
    /// <summary>Interface completion.</summary>
    Interface,
    /// <summary>Namespace completion.</summary>
    Namespace,
    /// <summary>Keyword completion.</summary>
    Keyword,
    /// <summary>Snippet completion.</summary>
    Snippet,
    /// <summary>Enum completion.</summary>
    Enum,
    /// <summary>Enum member completion.</summary>
    EnumMember
}

/// <summary>
/// Represents the result of a compilation operation.
/// </summary>
public sealed record CompilationResult
{
    /// <summary>
    /// Gets whether the compilation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the compilation diagnostics.
    /// </summary>
    public IReadOnlyList<CompilationDiagnostic> Diagnostics { get; init; } = Array.Empty<CompilationDiagnostic>();

    /// <summary>
    /// Gets the output assembly path, if successful.
    /// </summary>
    public string? OutputAssemblyPath { get; init; }

    /// <summary>
    /// Gets the compilation duration.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets additional compilation metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = 
        new Dictionary<string, object>(StringComparer.Ordinal);
}

/// <summary>
/// Represents the result of applying a code transformation.
/// </summary>
public sealed record TransformationResult
{
    /// <summary>
    /// Gets whether the transformation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the files that were modified by the transformation.
    /// </summary>
    public IReadOnlyDictionary<string, string> ModifiedFiles { get; init; } = 
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets any diagnostics produced by the transformation.
    /// </summary>
    public IReadOnlyList<CompilationDiagnostic> Diagnostics { get; init; } = Array.Empty<CompilationDiagnostic>();

    /// <summary>
    /// Gets additional transformation metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = 
        new Dictionary<string, object>(StringComparer.Ordinal);
}