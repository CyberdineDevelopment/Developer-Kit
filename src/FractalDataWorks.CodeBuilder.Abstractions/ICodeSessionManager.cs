using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.CodeBuilder.Abstractions;

/// <summary>
/// Interface for managing code sessions.
/// Provides session lifecycle management and efficient resource utilization.
/// </summary>
public interface ICodeSessionManager : IDisposable
{
    /// <summary>
    /// Gets all active session IDs.
    /// </summary>
    IReadOnlyList<Guid> ActiveSessions { get; }

    /// <summary>
    /// Creates a new code session.
    /// </summary>
    /// <param name="language">The programming language for the session.</param>
    /// <param name="assemblyName">The name of the assembly.</param>
    /// <param name="references">Optional assembly references.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the created session.</returns>
    Task<IFdwResult<ICodeSession>> CreateSessionAsync(
        string language,
        string assemblyName,
        IEnumerable<string>? references = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing code session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session if found; otherwise, null.</returns>
    Task<ICodeSession?> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Destroys a code session and releases its resources.
    /// </summary>
    /// <param name="sessionId">The session ID to destroy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<IFdwResult> DestroySessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session information without loading the full session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Session information if found; otherwise, null.</returns>
    Task<SessionInfo?> GetSessionInfoAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all sessions with optional filtering.
    /// </summary>
    /// <param name="language">Optional language filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of session information.</returns>
    Task<IReadOnlyList<SessionInfo>> ListSessionsAsync(
        string? language = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired or invalid sessions.
    /// </summary>
    /// <param name="maxAge">Maximum age for sessions before cleanup.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of sessions cleaned up.</returns>
    Task<int> CleanupSessionsAsync(
        TimeSpan? maxAge = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session usage statistics.
    /// </summary>
    /// <returns>Session usage statistics.</returns>
    SessionManagerStatistics GetStatistics();
}

/// <summary>
/// Represents basic information about a code session.
/// </summary>
public sealed record SessionInfo
{
    /// <summary>Gets the session ID.</summary>
    public Guid SessionId { get; init; }
    
    /// <summary>Gets the programming language.</summary>
    public string Language { get; init; } = string.Empty;
    
    /// <summary>Gets the assembly name.</summary>
    public string AssemblyName { get; init; } = string.Empty;
    
    /// <summary>Gets the creation timestamp.</summary>
    public DateTimeOffset Created { get; init; }
    
    /// <summary>Gets the last modified timestamp.</summary>
    public DateTimeOffset LastModified { get; init; }
    
    /// <summary>Gets whether the session is valid.</summary>
    public bool IsValid { get; init; }
    
    /// <summary>Gets whether the session has errors.</summary>
    public bool HasErrors { get; init; }
    
    /// <summary>Gets the number of source files.</summary>
    public int SourceFileCount { get; init; }
    
    /// <summary>Gets the number of references.</summary>
    public int ReferenceCount { get; init; }
    
    /// <summary>Gets the number of diagnostics.</summary>
    public int DiagnosticCount { get; init; }
}

/// <summary>
/// Represents session manager usage statistics.
/// </summary>
public sealed record SessionManagerStatistics
{
    /// <summary>Gets the total number of active sessions.</summary>
    public int ActiveSessionCount { get; init; }
    
    /// <summary>Gets the total number of sessions created.</summary>
    public int TotalSessionsCreated { get; init; }
    
    /// <summary>Gets the total number of sessions destroyed.</summary>
    public int TotalSessionsDestroyed { get; init; }
    
    /// <summary>Gets memory usage in bytes.</summary>
    public long MemoryUsageBytes { get; init; }
    
    /// <summary>Gets the average session lifetime.</summary>
    public TimeSpan AverageSessionLifetime { get; init; }
    
    /// <summary>Gets sessions grouped by language.</summary>
    public IReadOnlyDictionary<string, int> SessionsByLanguage { get; init; } = 
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>Gets the timestamp when statistics were collected.</summary>
    public DateTimeOffset CollectedAt { get; init; } = DateTimeOffset.UtcNow;
}