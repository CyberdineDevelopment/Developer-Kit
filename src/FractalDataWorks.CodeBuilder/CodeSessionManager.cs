using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Roslyn;
using FractalDataWorks.CodeBuilder.TreeSitter;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.CodeBuilder;

/// <summary>
/// Default implementation of the code session manager.
/// Manages session lifecycle and provides efficient resource utilization.
/// </summary>
public sealed class CodeSessionManager : ICodeSessionManager
{
    private readonly ICodeParserFactory _parserFactory;
    private readonly ILogger<CodeSessionManager> _logger;
    private readonly ConcurrentDictionary<Guid, ICodeSession> _sessions;
    private readonly ConcurrentDictionary<Guid, SessionInfo> _sessionInfo;
    private readonly Timer _cleanupTimer;
    private readonly SessionManagerConfiguration _configuration;
    private int _totalSessionsCreated;
    private int _totalSessionsDestroyed;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeSessionManager"/> class.
    /// </summary>
    /// <param name="parserFactory">The parser factory for creating language-specific sessions.</param>
    /// <param name="configuration">Optional configuration for the session manager.</param>
    /// <param name="logger">Optional logger instance.</param>
    public CodeSessionManager(
        ICodeParserFactory parserFactory,
        SessionManagerConfiguration? configuration = null,
        ILogger<CodeSessionManager>? logger = null)
    {
        _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CodeSessionManager>.Instance;
        _configuration = configuration ?? SessionManagerConfiguration.Default;
        _sessions = new ConcurrentDictionary<Guid, ICodeSession>();
        _sessionInfo = new ConcurrentDictionary<Guid, SessionInfo>();

        // Set up cleanup timer
        _cleanupTimer = new Timer(CleanupExpiredSessions, null, 
            _configuration.CleanupInterval, _configuration.CleanupInterval);

        _logger.LogDebug("Created code session manager with configuration: {Configuration}", _configuration);
    }

    /// <inheritdoc/>
    public IReadOnlyList<Guid> ActiveSessions
    {
        get
        {
            ThrowIfDisposed();
            return _sessions.Keys.ToArray();
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<ICodeSession>> CreateSessionAsync(
        string language,
        string assemblyName,
        IEnumerable<string>? references = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(assemblyName);

        try
        {
            _logger.LogDebug("Creating session for language {Language} with assembly {AssemblyName}", language, assemblyName);

            // Check if we've exceeded the maximum number of sessions
            if (_sessions.Count >= _configuration.MaxActiveSessions)
            {
                return FdwResult<ICodeSession>.Failure(new CodeSessionError("Maximum number of active sessions exceeded"));
            }

            // Create session based on language
            var sessionId = Guid.NewGuid();
            ICodeSession session = language.ToLowerInvariant() switch
            {
                "csharp" or "cs" => new RoslynCodeSession(sessionId, assemblyName, references, _logger),
                _ => await CreateTreeSitterSessionAsync(sessionId, language, assemblyName, references, cancellationToken).ConfigureAwait(false)
            };

            // Add to collections
            _sessions[sessionId] = session;
            _sessionInfo[sessionId] = CreateSessionInfo(session);

            Interlocked.Increment(ref _totalSessionsCreated);

            _logger.LogDebug("Successfully created session {SessionId} for language {Language}", sessionId, language);
            return FdwResult<ICodeSession>.Success(session);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error creating session for language {Language}", language);
            return FdwResult<ICodeSession>.Failure(new CodeSessionError($"Failed to create session: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<ICodeSession?> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await Task.CompletedTask; // Keep async signature
        return _sessions.TryGetValue(sessionId, out var session) && session.IsValid ? session : null;
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> DestroySessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            await Task.CompletedTask; // Keep async signature

            if (_sessions.TryRemove(sessionId, out var session))
            {
                _sessionInfo.TryRemove(sessionId, out _);
                session.Dispose();
                Interlocked.Increment(ref _totalSessionsDestroyed);

                _logger.LogDebug("Destroyed session {SessionId}", sessionId);
                return FdwResult.Success();
            }

            return FdwResult.Failure(new CodeSessionError($"Session {sessionId} not found"));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Error destroying session {SessionId}", sessionId);
            return FdwResult.Failure(new CodeSessionError($"Failed to destroy session: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<SessionInfo?> GetSessionInfoAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await Task.CompletedTask; // Keep async signature

        if (_sessionInfo.TryGetValue(sessionId, out var info))
        {
            // Update info if session still exists
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                info = CreateSessionInfo(session);
                _sessionInfo[sessionId] = info;
            }
            return info;
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SessionInfo>> ListSessionsAsync(
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await Task.CompletedTask; // Keep async signature

        var allSessions = _sessionInfo.Values.ToArray();

        if (string.IsNullOrEmpty(language))
        {
            return allSessions;
        }

        return allSessions.Where(s => string.Equals(s.Language, language, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    /// <inheritdoc/>
    public async Task<int> CleanupSessionsAsync(
        TimeSpan? maxAge = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var cutoffTime = DateTimeOffset.UtcNow - (maxAge ?? _configuration.DefaultSessionTimeout);
        var sessionsToRemove = new List<Guid>();

        await Task.CompletedTask; // Keep async signature

        foreach (var kvp in _sessions)
        {
            var session = kvp.Value;
            if (!session.IsValid || session.LastModified < cutoffTime)
            {
                sessionsToRemove.Add(kvp.Key);
            }
        }

        var cleanedUp = 0;
        foreach (var sessionId in sessionsToRemove)
        {
            var result = await DestroySessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                cleanedUp++;
            }
        }

        if (cleanedUp > 0)
        {
            _logger.LogInformation("Cleaned up {CleanedUpCount} expired sessions", cleanedUp);
        }

        return cleanedUp;
    }

    /// <inheritdoc/>
    public SessionManagerStatistics GetStatistics()
    {
        ThrowIfDisposed();

        var sessionsByLanguage = _sessionInfo.Values
            .GroupBy(s => s.Language, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var memoryUsage = GC.GetTotalMemory(false);

        var lifetimes = _sessionInfo.Values
            .Select(s => s.LastModified - s.Created)
            .Where(t => t.TotalMilliseconds > 0)
            .ToArray();

        var averageLifetime = lifetimes.Length > 0 
            ? TimeSpan.FromTicks((long)lifetimes.Average(t => t.Ticks))
            : TimeSpan.Zero;

        return new SessionManagerStatistics
        {
            ActiveSessionCount = _sessions.Count,
            TotalSessionsCreated = _totalSessionsCreated,
            TotalSessionsDestroyed = _totalSessionsDestroyed,
            MemoryUsageBytes = memoryUsage,
            AverageSessionLifetime = averageLifetime,
            SessionsByLanguage = sessionsByLanguage
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _logger.LogDebug("Disposing code session manager");

            _cleanupTimer?.Dispose();

            // Dispose all sessions
            foreach (var session in _sessions.Values)
            {
                try
                {
                    session.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing session during cleanup");
                }
            }

            _sessions.Clear();
            _sessionInfo.Clear();
            _isDisposed = true;

            _logger.LogDebug("Disposed code session manager");
        }
    }

    private async Task<ICodeSession> CreateTreeSitterSessionAsync(
        Guid sessionId,
        string language,
        string assemblyName,
        IEnumerable<string>? references,
        CancellationToken cancellationToken)
    {
        // For now, create a basic session wrapper
        // In a full implementation, this would create a TreeSitter-based session
        return new BasicCodeSession(sessionId, language, assemblyName, references);
    }

    private SessionInfo CreateSessionInfo(ICodeSession session)
    {
        return new SessionInfo
        {
            SessionId = session.SessionId,
            Language = session.Language,
            AssemblyName = session is RoslynCodeSession roslynSession ? roslynSession.AssemblyName : "Unknown",
            Created = session.Created,
            LastModified = session.LastModified,
            IsValid = session.IsValid,
            HasErrors = session.HasErrors,
            SourceFileCount = session.SourceFiles.Count,
            ReferenceCount = session.References.Count,
            DiagnosticCount = session.Diagnostics.Count
        };
    }

    private void CleanupExpiredSessions(object? state)
    {
        if (_isDisposed) return;

        try
        {
            _ = Task.Run(async () =>
            {
                await CleanupSessionsAsync().ConfigureAwait(false);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during scheduled session cleanup");
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(CodeSessionManager));
        }
    }
}

/// <summary>
/// Configuration for the session manager.
/// </summary>
public sealed record SessionManagerConfiguration
{
    /// <summary>Gets the default configuration.</summary>
    public static SessionManagerConfiguration Default { get; } = new();

    /// <summary>Gets or sets the maximum number of active sessions.</summary>
    public int MaxActiveSessions { get; init; } = 100;

    /// <summary>Gets or sets the default session timeout.</summary>
    public TimeSpan DefaultSessionTimeout { get; init; } = TimeSpan.FromHours(2);

    /// <summary>Gets or sets the cleanup interval.</summary>
    public TimeSpan CleanupInterval { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>Gets or sets whether to enable session persistence.</summary>
    public bool EnableSessionPersistence { get; init; } = false;

    /// <summary>Gets or sets the session persistence directory.</summary>
    public string? SessionPersistenceDirectory { get; init; }
}

/// <summary>
/// Basic implementation of ICodeSession for non-Roslyn languages.
/// </summary>
internal sealed class BasicCodeSession : ICodeSession
{
    private readonly Dictionary<string, string> _sourceFiles;
    private readonly List<string> _references;
    private bool _isDisposed;

    public BasicCodeSession(Guid sessionId, string language, string assemblyName, IEnumerable<string>? references)
    {
        SessionId = sessionId;
        Language = language;
        AssemblyName = assemblyName;
        Created = DateTimeOffset.UtcNow;
        LastModified = Created;
        _sourceFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _references = new List<string>(references ?? Array.Empty<string>());
    }

    public string AssemblyName { get; }
    public Guid SessionId { get; }
    public string Language { get; }
    public DateTimeOffset Created { get; }
    public DateTimeOffset LastModified { get; private set; }
    public bool IsValid => !_isDisposed;
    public IReadOnlyDictionary<string, string> SourceFiles => _sourceFiles;
    public IReadOnlyList<string> References => _references;
    public IReadOnlyList<ISyntaxTree> SyntaxTrees => Array.Empty<ISyntaxTree>();
    public bool HasErrors => false;
    public IReadOnlyList<CompilationDiagnostic> Diagnostics => Array.Empty<CompilationDiagnostic>();

    public async Task<IFdwResult> UpdateSourceAsync(string filePath, string source, CancellationToken cancellationToken = default)
    {
        _sourceFiles[filePath] = source;
        LastModified = DateTimeOffset.UtcNow;
        await Task.CompletedTask;
        return FdwResult.Success();
    }

    public async Task<IFdwResult> RemoveSourceAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _sourceFiles.Remove(filePath);
        LastModified = DateTimeOffset.UtcNow;
        await Task.CompletedTask;
        return FdwResult.Success();
    }

    public async Task<IFdwResult> AddReferenceAsync(string assemblyPath, CancellationToken cancellationToken = default)
    {
        if (!_references.Contains(assemblyPath, StringComparer.OrdinalIgnoreCase))
        {
            _references.Add(assemblyPath);
            LastModified = DateTimeOffset.UtcNow;
        }
        await Task.CompletedTask;
        return FdwResult.Success();
    }

    public Task<IFdwResult<TransformationResult>> ApplyTransformationAsync(ICodeTransformation transformation, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FdwResult<TransformationResult>.Failure(new CodeSessionError("Transformations not supported in basic session")));
    }

    public Task<IFdwResult<SemanticInfo>> GetSemanticInfoAsync(string filePath, int position, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FdwResult<SemanticInfo>.Failure(new CodeSessionError("Semantic analysis not supported in basic session")));
    }

    public Task<IFdwResult<IReadOnlyList<CompletionItem>>> GetCompletionsAsync(string filePath, int position, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FdwResult<IReadOnlyList<CompletionItem>>.Success(Array.Empty<CompletionItem>()));
    }

    public Task<IFdwResult<CompilationResult>> CompileAsync(string? outputPath = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FdwResult<CompilationResult>.Failure(new CodeSessionError("Compilation not supported in basic session")));
    }

    public ICodeSessionSnapshot CreateSnapshot()
    {
        return new BasicCodeSessionSnapshot(SessionId, DateTimeOffset.UtcNow, Language, SourceFiles, References, Diagnostics);
    }

    public Task<IFdwResult> RestoreFromSnapshotAsync(ICodeSessionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FdwResult.Failure(new CodeSessionError("Snapshot restore not supported in basic session")));
    }

    public void Dispose()
    {
        _isDisposed = true;
    }
}

/// <summary>
/// Basic implementation of ICodeSessionSnapshot.
/// </summary>
internal sealed record BasicCodeSessionSnapshot : ICodeSessionSnapshot
{
    public BasicCodeSessionSnapshot(
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

    public Guid SessionId { get; }
    public DateTimeOffset Timestamp { get; }
    public string Language { get; }
    public IReadOnlyDictionary<string, string> SourceFiles { get; }
    public IReadOnlyList<string> References { get; }
    public IReadOnlyList<CompilationDiagnostic> Diagnostics { get; }
}

/// <summary>
/// Error message type for code session errors.
/// </summary>
public sealed record CodeSessionError : IFdwMessage
{
    public CodeSessionError(string message) => Message = message;
    public string Message { get; }
    public string Format(params object[] args) => string.Format(Message, args);
}