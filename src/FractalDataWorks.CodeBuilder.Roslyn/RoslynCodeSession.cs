using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.CodeBuilder.Roslyn;

/// <summary>
/// Roslyn implementation of a code session with full semantic analysis.
/// Provides context-efficient operations with cached compilation and incremental updates.
/// </summary>
public sealed class RoslynCodeSession : ICodeSession
{
    private readonly ILogger<RoslynCodeSession> _logger;
    private readonly Dictionary<string, string> _sourceFiles;
    private readonly List<string> _references;
    private readonly object _lock = new();
    private CSharpCompilation? _compilation;
    private bool _isDisposed;
    private DateTimeOffset _lastModified;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoslynCodeSession"/> class.
    /// </summary>
    /// <param name="sessionId">The unique session identifier.</param>
    /// <param name="assemblyName">The name of the assembly.</param>
    /// <param name="references">Initial assembly references.</param>
    /// <param name="logger">Optional logger instance.</param>
    public RoslynCodeSession(
        Guid sessionId,
        string assemblyName,
        IEnumerable<string>? references = null,
        ILogger<RoslynCodeSession>? logger = null)
    {
        SessionId = sessionId;
        AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<RoslynCodeSession>.Instance;
        _sourceFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _references = new List<string>();
        Created = DateTimeOffset.UtcNow;
        _lastModified = Created;

        if (references != null)
        {
            _references.AddRange(references);
        }

        _logger.LogDebug("Created Roslyn code session {SessionId} for assembly {AssemblyName}", SessionId, AssemblyName);
    }

    /// <inheritdoc/>
    public Guid SessionId { get; }

    /// <inheritdoc/>
    public string Language => "csharp";

    /// <inheritdoc/>
    public DateTimeOffset Created { get; }

    /// <inheritdoc/>
    public DateTimeOffset LastModified
    {
        get
        {
            lock (_lock)
            {
                return _lastModified;
            }
        }
    }

    /// <inheritdoc/>
    public bool IsValid
    {
        get
        {
            lock (_lock)
            {
                return !_isDisposed;
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> SourceFiles
    {
        get
        {
            lock (_lock)
            {
                return new Dictionary<string, string>(_sourceFiles, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> References
    {
        get
        {
            lock (_lock)
            {
                return _references.ToArray();
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<ISyntaxTree> SyntaxTrees
    {
        get
        {
            lock (_lock)
            {
                if (_compilation == null)
                {
                    return Array.Empty<ISyntaxTree>();
                }

                return _compilation.SyntaxTrees
                    .Select(st => new RoslynSyntaxTree(st, GetMetadataReferences()))
                    .ToArray();
            }
        }
    }

    /// <inheritdoc/>
    public bool HasErrors
    {
        get
        {
            lock (_lock)
            {
                if (_compilation == null) return false;
                return _compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error);
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<CompilationDiagnostic> Diagnostics
    {
        get
        {
            lock (_lock)
            {
                if (_compilation == null) return Array.Empty<CompilationDiagnostic>();

                return _compilation.GetDiagnostics()
                    .Select(d => new CompilationDiagnostic
                    {
                        Message = d.GetMessage(),
                        Code = d.Id,
                        Severity = MapDiagnosticSeverity(d.Severity),
                        Location = d.Location.IsInSource ? CreateSourceLocation(d.Location) : null,
                        FilePath = d.Location.IsInSource ? d.Location.SourceTree?.FilePath : null,
                        Category = d.Category
                    })
                    .ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the assembly name for this session.
    /// </summary>
    public string AssemblyName { get; }

    /// <summary>
    /// Gets the current Roslyn compilation.
    /// </summary>
    public CSharpCompilation? Compilation
    {
        get
        {
            lock (_lock)
            {
                return _compilation;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> UpdateSourceAsync(
        string filePath, 
        string source, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(source);

        if (_isDisposed)
        {
            return FdwResult.Failure(new RoslynSessionError("Session has been disposed"));
        }

        try
        {
            lock (_lock)
            {
                _logger.LogDebug("Updating source file {FilePath} in session {SessionId}", filePath, SessionId);

                _sourceFiles[filePath] = source;
                _lastModified = DateTimeOffset.UtcNow;

                // Invalidate compilation
                _compilation = null;
            }

            // Rebuild compilation
            await RebuildCompilationAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Successfully updated source file {FilePath} in session {SessionId}", filePath, SessionId);
            return FdwResult.Success();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error updating source file {FilePath} in session {SessionId}", filePath, SessionId);
            return FdwResult.Failure(new RoslynSessionError($"Failed to update source: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> RemoveSourceAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (_isDisposed)
        {
            return FdwResult.Failure(new RoslynSessionError("Session has been disposed"));
        }

        try
        {
            lock (_lock)
            {
                _logger.LogDebug("Removing source file {FilePath} from session {SessionId}", filePath, SessionId);

                if (!_sourceFiles.Remove(filePath))
                {
                    return FdwResult.Failure(new RoslynSessionError($"Source file '{filePath}' not found"));
                }

                _lastModified = DateTimeOffset.UtcNow;

                // Invalidate compilation
                _compilation = null;
            }

            // Rebuild compilation
            await RebuildCompilationAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Successfully removed source file {FilePath} from session {SessionId}", filePath, SessionId);
            return FdwResult.Success();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error removing source file {FilePath} from session {SessionId}", filePath, SessionId);
            return FdwResult.Failure(new RoslynSessionError($"Failed to remove source: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> AddReferenceAsync(
        string assemblyPath, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assemblyPath);

        if (_isDisposed)
        {
            return FdwResult.Failure(new RoslynSessionError("Session has been disposed"));
        }

        try
        {
            if (!File.Exists(assemblyPath))
            {
                return FdwResult.Failure(new RoslynSessionError($"Assembly file '{assemblyPath}' not found"));
            }

            lock (_lock)
            {
                _logger.LogDebug("Adding reference {AssemblyPath} to session {SessionId}", assemblyPath, SessionId);

                if (_references.Contains(assemblyPath, StringComparer.OrdinalIgnoreCase))
                {
                    return FdwResult.Success(); // Already added
                }

                _references.Add(assemblyPath);
                _lastModified = DateTimeOffset.UtcNow;

                // Invalidate compilation
                _compilation = null;
            }

            // Rebuild compilation
            await RebuildCompilationAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Successfully added reference {AssemblyPath} to session {SessionId}", assemblyPath, SessionId);
            return FdwResult.Success();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error adding reference {AssemblyPath} to session {SessionId}", assemblyPath, SessionId);
            return FdwResult.Failure(new RoslynSessionError($"Failed to add reference: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<TransformationResult>> ApplyTransformationAsync(
        ICodeTransformation transformation, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transformation);

        if (_isDisposed)
        {
            return FdwResult<TransformationResult>.Failure(new RoslynSessionError("Session has been disposed"));
        }

        try
        {
            _logger.LogDebug("Applying transformation {TransformationName} to session {SessionId}", 
                transformation.Name, SessionId);

            var syntaxTrees = SyntaxTrees;
            var context = new RoslynTransformationContext(this, _logger);

            var result = await transformation.ApplyAsync(syntaxTrees, context, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                return FdwResult<TransformationResult>.Failure(result.Message!);
            }

            // Update source files with transformed content
            foreach (var transformedTree in result.Value!)
            {
                if (!string.IsNullOrEmpty(transformedTree.FilePath))
                {
                    var updateResult = await UpdateSourceAsync(
                        transformedTree.FilePath, 
                        transformedTree.SourceText, 
                        cancellationToken).ConfigureAwait(false);

                    if (updateResult.IsFailure)
                    {
                        return FdwResult<TransformationResult>.Failure(updateResult.Message!);
                    }
                }
            }

            var transformationResult = new TransformationResult
            {
                Success = true,
                ModifiedFiles = result.Value!.ToDictionary(
                    st => st.FilePath ?? "unknown", 
                    st => st.SourceText,
                    StringComparer.OrdinalIgnoreCase),
                Diagnostics = Diagnostics
            };

            _logger.LogDebug("Successfully applied transformation {TransformationName} to session {SessionId}", 
                transformation.Name, SessionId);

            return FdwResult<TransformationResult>.Success(transformationResult);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error applying transformation {TransformationName} to session {SessionId}", 
                transformation.Name, SessionId);
            return FdwResult<TransformationResult>.Failure(new RoslynSessionError($"Transformation failed: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<SemanticInfo>> GetSemanticInfoAsync(
        string filePath, 
        int position, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (_isDisposed)
        {
            return FdwResult<SemanticInfo>.Failure(new RoslynSessionError("Session has been disposed"));
        }

        try
        {
            await EnsureCompilationAsync(cancellationToken).ConfigureAwait(false);

            var syntaxTree = _compilation?.SyntaxTrees.FirstOrDefault(st => 
                string.Equals(st.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

            if (syntaxTree == null)
            {
                return FdwResult<SemanticInfo>.Failure(new RoslynSessionError($"File '{filePath}' not found in session"));
            }

            var roslynSyntaxTree = new RoslynSyntaxTree(syntaxTree, GetMetadataReferences());
            var semanticInfo = await roslynSyntaxTree.GetSemanticInfoAsync(position, _compilation, cancellationToken).ConfigureAwait(false);

            return semanticInfo != null 
                ? FdwResult<SemanticInfo>.Success(semanticInfo)
                : FdwResult<SemanticInfo>.Failure(new RoslynSessionError("No semantic information available at position"));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error getting semantic info at {FilePath}:{Position} in session {SessionId}", 
                filePath, position, SessionId);
            return FdwResult<SemanticInfo>.Failure(new RoslynSessionError($"Failed to get semantic info: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IReadOnlyList<CompletionItem>>> GetCompletionsAsync(
        string filePath, 
        int position, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (_isDisposed)
        {
            return FdwResult<IReadOnlyList<CompletionItem>>.Failure(new RoslynSessionError("Session has been disposed"));
        }

        try
        {
            await EnsureCompilationAsync(cancellationToken).ConfigureAwait(false);

            var syntaxTree = _compilation?.SyntaxTrees.FirstOrDefault(st => 
                string.Equals(st.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

            if (syntaxTree == null)
            {
                return FdwResult<IReadOnlyList<CompletionItem>>.Failure(new RoslynSessionError($"File '{filePath}' not found in session"));
            }

            var roslynSyntaxTree = new RoslynSyntaxTree(syntaxTree, GetMetadataReferences());
            var completions = await roslynSyntaxTree.GetCompletionsAsync(position, _compilation, cancellationToken).ConfigureAwait(false);

            return FdwResult<IReadOnlyList<CompletionItem>>.Success(completions);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error getting completions at {FilePath}:{Position} in session {SessionId}", 
                filePath, position, SessionId);
            return FdwResult<IReadOnlyList<CompletionItem>>.Failure(new RoslynSessionError($"Failed to get completions: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<CompilationResult>> CompileAsync(
        string? outputPath = null, 
        CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
        {
            return FdwResult<CompilationResult>.Failure(new RoslynSessionError("Session has been disposed"));
        }

        try
        {
            var startTime = DateTimeOffset.UtcNow;
            
            await EnsureCompilationAsync(cancellationToken).ConfigureAwait(false);

            if (_compilation == null)
            {
                return FdwResult<CompilationResult>.Failure(new RoslynSessionError("No compilation available"));
            }

            var diagnostics = Diagnostics;
            var hasErrors = diagnostics.Any(d => d.Severity == ErrorSeverity.Error);

            string? assemblyPath = null;
            if (!hasErrors && !string.IsNullOrEmpty(outputPath))
            {
                var emitResult = _compilation.Emit(outputPath, cancellationToken: cancellationToken);
                if (emitResult.Success)
                {
                    assemblyPath = outputPath;
                }
            }

            var duration = DateTimeOffset.UtcNow - startTime;
            var result = new CompilationResult
            {
                Success = !hasErrors,
                Diagnostics = diagnostics,
                OutputAssemblyPath = assemblyPath,
                Duration = duration.TimeOfDay
            };

            return FdwResult<CompilationResult>.Success(result);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error compiling session {SessionId}", SessionId);
            return FdwResult<CompilationResult>.Failure(new RoslynSessionError($"Compilation failed: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public ICodeSessionSnapshot CreateSnapshot()
    {
        lock (_lock)
        {
            return new RoslynCodeSessionSnapshot(
                SessionId,
                DateTimeOffset.UtcNow,
                Language,
                new Dictionary<string, string>(_sourceFiles, StringComparer.OrdinalIgnoreCase),
                _references.ToArray(),
                Diagnostics);
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> RestoreFromSnapshotAsync(
        ICodeSessionSnapshot snapshot, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (_isDisposed)
        {
            return FdwResult.Failure(new RoslynSessionError("Session has been disposed"));
        }

        try
        {
            lock (_lock)
            {
                _logger.LogDebug("Restoring session {SessionId} from snapshot", SessionId);

                _sourceFiles.Clear();
                foreach (var kvp in snapshot.SourceFiles)
                {
                    _sourceFiles[kvp.Key] = kvp.Value;
                }

                _references.Clear();
                _references.AddRange(snapshot.References);

                _lastModified = DateTimeOffset.UtcNow;
                _compilation = null;
            }

            await RebuildCompilationAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Successfully restored session {SessionId} from snapshot", SessionId);
            return FdwResult.Success();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error restoring session {SessionId} from snapshot", SessionId);
            return FdwResult.Failure(new RoslynSessionError($"Failed to restore from snapshot: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_lock)
        {
            if (!_isDisposed)
            {
                _logger.LogDebug("Disposing Roslyn code session {SessionId}", SessionId);
                _sourceFiles.Clear();
                _references.Clear();
                _compilation = null;
                _isDisposed = true;
            }
        }
    }

    private async Task RebuildCompilationAsync(CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            var syntaxTrees = new List<SyntaxTree>();

            foreach (var kvp in _sourceFiles)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(kvp.Value, path: kvp.Key);
                syntaxTrees.Add(syntaxTree);
            }

            var metadataReferences = GetMetadataReferences();

            _compilation = CSharpCompilation.Create(
                AssemblyName,
                syntaxTrees,
                metadataReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        }, cancellationToken);
    }

    private async Task EnsureCompilationAsync(CancellationToken cancellationToken)
    {
        if (_compilation == null)
        {
            await RebuildCompilationAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private ImmutableArray<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>();

        foreach (var referencePath in _references)
        {
            if (File.Exists(referencePath))
            {
                references.Add(MetadataReference.CreateFromFile(referencePath));
            }
        }

        return references.ToImmutableArray();
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
}

/// <summary>
/// Roslyn implementation of a code session snapshot.
/// </summary>
public sealed record RoslynCodeSessionSnapshot : ICodeSessionSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoslynCodeSessionSnapshot"/> class.
    /// </summary>
    public RoslynCodeSessionSnapshot(
        Guid sessionId,
        DateTimeOffset timestamp,
        string language,
        IReadOnlyDictionary<string, string> sourceFiles,
        IReadOnlyList<string> references,
        IReadOnlyList<CompilationDiagnostic> diagnostics)
    {
        SessionId = sessionId;
        Timestamp = timestamp;
        Language = language;
        SourceFiles = sourceFiles;
        References = references;
        Diagnostics = diagnostics;
    }

    /// <inheritdoc/>
    public Guid SessionId { get; }

    /// <inheritdoc/>
    public DateTimeOffset Timestamp { get; }

    /// <inheritdoc/>
    public string Language { get; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> SourceFiles { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> References { get; }

    /// <inheritdoc/>
    public IReadOnlyList<CompilationDiagnostic> Diagnostics { get; }
}

/// <summary>
/// Roslyn implementation of transformation context.
/// </summary>
internal sealed class RoslynTransformationContext : ITransformationContext
{
    private readonly RoslynCodeSession _session;
    private readonly List<CompilationDiagnostic> _diagnostics;

    public RoslynTransformationContext(RoslynCodeSession session, Microsoft.Extensions.Logging.ILogger logger)
    {
        _session = session;
        Logger = logger;
        _diagnostics = new List<CompilationDiagnostic>();
    }

    public Guid SessionId => _session.SessionId;
    public string Language => _session.Language;
    public IReadOnlyDictionary<string, string> SourceFiles => _session.SourceFiles;
    public IReadOnlyList<string> References => _session.References;
    public IReadOnlyDictionary<string, object> Options => new Dictionary<string, object>(StringComparer.Ordinal);
    public Microsoft.Extensions.Logging.ILogger Logger { get; }

    public async Task<SemanticInfo?> ResolveSymbolAsync(string filePath, int position)
    {
        var result = await _session.GetSemanticInfoAsync(filePath, position).ConfigureAwait(false);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task<IReadOnlyList<SymbolReference>> FindReferencesAsync(string symbol)
    {
        // Simplified implementation - a full implementation would use Roslyn's find references APIs
        await Task.CompletedTask;
        return Array.Empty<SymbolReference>();
    }

    public void AddDiagnostic(CompilationDiagnostic diagnostic)
    {
        _diagnostics.Add(diagnostic);
    }
}

/// <summary>
/// Error message type for Roslyn session errors.
/// </summary>
public sealed record RoslynSessionError : IFdwMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoslynSessionError"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public RoslynSessionError(string message)
    {
        Message = message;
    }

    /// <inheritdoc/>
    public string Message { get; }

    /// <inheritdoc/>
    public string Format(params object[] args) => string.Format(Message, args);
}