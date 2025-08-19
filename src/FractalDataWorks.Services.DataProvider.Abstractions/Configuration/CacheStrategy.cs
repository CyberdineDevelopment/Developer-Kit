namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Defines the caching strategies for discovered schema information.
/// </summary>
public enum CacheStrategy
{
    /// <summary>
    /// No caching - discover schema on every request.
    /// </summary>
    /// <remarks>
    /// Provides the most up-to-date information but has the highest performance cost.
    /// Suitable for development environments or schemas that change frequently.
    /// </remarks>
    None = 1,

    /// <summary>
    /// Cache schema information in memory for the application lifetime.
    /// </summary>
    /// <remarks>
    /// Fast access to cached information but lost on application restart.
    /// Good balance of performance and freshness for most scenarios.
    /// </remarks>
    Memory = 2,

    /// <summary>
    /// Cache schema information persistently with configurable expiration.
    /// </summary>
    /// <remarks>
    /// Preserves cache across application restarts. Requires additional storage
    /// but provides the best performance for static schemas.
    /// </remarks>
    Persistent = 3,

    /// <summary>
    /// Use memory cache with persistent backup for resilience.
    /// </summary>
    /// <remarks>
    /// Combines the speed of memory cache with the durability of persistent cache.
    /// Memory cache is populated from persistent cache on startup if available.
    /// </remarks>
    Hybrid = 4
}