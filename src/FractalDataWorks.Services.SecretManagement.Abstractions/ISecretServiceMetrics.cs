using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Interface representing performance metrics and operational statistics for a secret service.
/// Provides detailed information about service performance, usage patterns, and operational health.
/// </summary>
/// <remarks>
/// Provider metrics enable monitoring, performance optimization, and capacity planning
/// for secret management operations across different service implementations.
/// </remarks>
public interface ISecretServiceMetrics
{
    /// <summary>
    /// Gets the service identifier.
    /// </summary>
    /// <value>The unique identifier for the service.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets the service name.
    /// </summary>
    /// <value>The display name of the service.</value>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the service type.
    /// </summary>
    /// <value>The service type identifier.</value>
    string ProviderType { get; }
    
    /// <summary>
    /// Gets when these metrics were collected.
    /// </summary>
    /// <value>The metrics collection timestamp.</value>
    DateTimeOffset CollectedAt { get; }
    
    /// <summary>
    /// Gets the time period covered by these metrics.
    /// </summary>
    /// <value>The metrics period.</value>
    /// <remarks>
    /// This indicates the time window over which the metrics were collected,
    /// helping interpret the performance data appropriately.
    /// </remarks>
    TimeSpan MetricsPeriod { get; }
    
    /// <summary>
    /// Gets the total number of operations performed.
    /// </summary>
    /// <value>The total operation count.</value>
    long TotalOperations { get; }
    
    /// <summary>
    /// Gets the number of successful operations.
    /// </summary>
    /// <value>The successful operation count.</value>
    long SuccessfulOperations { get; }
    
    /// <summary>
    /// Gets the number of failed operations.
    /// </summary>
    /// <value>The failed operation count.</value>
    long FailedOperations { get; }
    
    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    /// <value>The success rate (0.0 to 100.0).</value>
    double SuccessRate { get; }
    
    /// <summary>
    /// Gets the average response time for all operations.
    /// </summary>
    /// <value>The average response time.</value>
    TimeSpan AverageResponseTime { get; }
    
    /// <summary>
    /// Gets the minimum response time observed.
    /// </summary>
    /// <value>The minimum response time.</value>
    TimeSpan MinResponseTime { get; }
    
    /// <summary>
    /// Gets the maximum response time observed.
    /// </summary>
    /// <value>The maximum response time.</value>
    TimeSpan MaxResponseTime { get; }
    
    /// <summary>
    /// Gets the 50th percentile (median) response time.
    /// </summary>
    /// <value>The 50th percentile response time.</value>
    TimeSpan P50ResponseTime { get; }
    
    /// <summary>
    /// Gets the 95th percentile response time.
    /// </summary>
    /// <value>The 95th percentile response time.</value>
    TimeSpan P95ResponseTime { get; }
    
    /// <summary>
    /// Gets the 99th percentile response time.
    /// </summary>
    /// <value>The 99th percentile response time.</value>
    TimeSpan P99ResponseTime { get; }
    
    /// <summary>
    /// Gets the current number of active connections.
    /// </summary>
    /// <value>The active connection count.</value>
    int ActiveConnections { get; }
    
    /// <summary>
    /// Gets the maximum number of concurrent connections observed.
    /// </summary>
    /// <value>The peak concurrent connection count.</value>
    int PeakConcurrentConnections { get; }
    
    /// <summary>
    /// Gets the number of connection timeouts.
    /// </summary>
    /// <value>The connection timeout count.</value>
    long ConnectionTimeouts { get; }
    
    /// <summary>
    /// Gets the number of retry attempts made.
    /// </summary>
    /// <value>The retry attempt count.</value>
    long RetryAttempts { get; }
    
    /// <summary>
    /// Gets the number of cache hits (if caching is enabled).
    /// </summary>
    /// <value>The cache hit count.</value>
    long CacheHits { get; }
    
    /// <summary>
    /// Gets the number of cache misses (if caching is enabled).
    /// </summary>
    /// <value>The cache miss count.</value>
    long CacheMisses { get; }
    
    /// <summary>
    /// Gets the cache hit rate as a percentage (if caching is enabled).
    /// </summary>
    /// <value>The cache hit rate (0.0 to 100.0).</value>
    double CacheHitRate { get; }
    
    /// <summary>
    /// Gets operation-specific metrics.
    /// </summary>
    /// <value>A dictionary of operation types to their specific metrics.</value>
    /// <remarks>
    /// This provides detailed metrics for each type of operation (Get, Set, Delete, etc.)
    /// allowing fine-grained performance analysis and optimization.
    /// </remarks>
    IReadOnlyDictionary<string, ISecretOperationMetrics> OperationMetrics { get; }
    
    /// <summary>
    /// Gets error metrics categorized by error type.
    /// </summary>
    /// <value>A dictionary of error types to their occurrence counts.</value>
    /// <remarks>
    /// Error metrics help identify common failure patterns and guide
    /// troubleshooting and reliability improvements.
    /// </remarks>
    IReadOnlyDictionary<string, long> ErrorMetrics { get; }
    
    /// <summary>
    /// Gets additional service-specific metrics.
    /// </summary>
    /// <value>A dictionary of custom metric names to their values.</value>
    /// <remarks>
    /// Provider-specific metrics can include implementation details like
    /// authentication token refresh counts, rate limiting statistics,
    /// or storage utilization metrics.
    /// </remarks>
    IReadOnlyDictionary<string, object> CustomMetrics { get; }

}