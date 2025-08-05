using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Interface representing performance metrics and operational statistics for a secret provider.
/// Provides detailed information about provider performance, usage patterns, and operational health.
/// </summary>
/// <remarks>
/// Provider metrics enable monitoring, performance optimization, and capacity planning
/// for secret management operations across different provider implementations.
/// </remarks>
public interface ISecretProviderMetrics
{
    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    /// <value>The unique identifier for the provider.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    /// <value>The display name of the provider.</value>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the provider type.
    /// </summary>
    /// <value>The provider type identifier.</value>
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
    /// Gets additional provider-specific metrics.
    /// </summary>
    /// <value>A dictionary of custom metric names to their values.</value>
    /// <remarks>
    /// Provider-specific metrics can include implementation details like
    /// authentication token refresh counts, rate limiting statistics,
    /// or storage utilization metrics.
    /// </remarks>
    IReadOnlyDictionary<string, object> CustomMetrics { get; }
    
    /// <summary>
    /// Gets throughput metrics for the provider.
    /// </summary>
    /// <value>Throughput metrics, or null if not available.</value>
    ISecretProviderThroughput? Throughput { get; }
}

/// <summary>
/// Interface representing metrics for a specific secret operation type.
/// Provides detailed performance information for individual operation categories.
/// </summary>
/// <remarks>
/// Operation-specific metrics enable fine-grained performance analysis
/// and help identify optimization opportunities for different operation types.
/// </remarks>
public interface ISecretOperationMetrics
{
    /// <summary>
    /// Gets the operation type name.
    /// </summary>
    /// <value>The operation type (e.g., "GetSecret", "SetSecret", "DeleteSecret").</value>
    string OperationType { get; }
    
    /// <summary>
    /// Gets the total number of operations of this type.
    /// </summary>
    /// <value>The total operation count.</value>
    long TotalOperations { get; }
    
    /// <summary>
    /// Gets the number of successful operations of this type.
    /// </summary>
    /// <value>The successful operation count.</value>
    long SuccessfulOperations { get; }
    
    /// <summary>
    /// Gets the number of failed operations of this type.
    /// </summary>
    /// <value>The failed operation count.</value>
    long FailedOperations { get; }
    
    /// <summary>
    /// Gets the success rate for this operation type.
    /// </summary>
    /// <value>The success rate (0.0 to 100.0).</value>
    double SuccessRate { get; }
    
    /// <summary>
    /// Gets the average response time for this operation type.
    /// </summary>
    /// <value>The average response time.</value>
    TimeSpan AverageResponseTime { get; }
    
    /// <summary>
    /// Gets the minimum response time for this operation type.
    /// </summary>
    /// <value>The minimum response time.</value>
    TimeSpan MinResponseTime { get; }
    
    /// <summary>
    /// Gets the maximum response time for this operation type.
    /// </summary>
    /// <value>The maximum response time.</value>
    TimeSpan MaxResponseTime { get; }
    
    /// <summary>
    /// Gets the 95th percentile response time for this operation type.
    /// </summary>
    /// <value>The 95th percentile response time.</value>
    TimeSpan P95ResponseTime { get; }
    
    /// <summary>
    /// Gets the total amount of data processed by this operation type.
    /// </summary>
    /// <value>The total data processed in bytes.</value>
    long TotalDataProcessed { get; }
    
    /// <summary>
    /// Gets the average data size per operation.
    /// </summary>
    /// <value>The average data size in bytes.</value>
    long AverageDataSize { get; }
}

/// <summary>
/// Interface representing throughput metrics for a secret provider.
/// Provides information about data transfer rates and operation frequencies.
/// </summary>
/// <remarks>
/// Throughput metrics help assess provider capacity and identify
/// performance bottlenecks in high-load scenarios.
/// </remarks>
public interface ISecretProviderThroughput
{
    /// <summary>
    /// Gets the operations per second rate.
    /// </summary>
    /// <value>The operations per second.</value>
    double OperationsPerSecond { get; }
    
    /// <summary>
    /// Gets the peak operations per second rate observed.
    /// </summary>
    /// <value>The peak operations per second.</value>
    double PeakOperationsPerSecond { get; }
    
    /// <summary>
    /// Gets the data throughput in bytes per second.
    /// </summary>
    /// <value>The data throughput in bytes per second.</value>
    long BytesPerSecond { get; }
    
    /// <summary>
    /// Gets the peak data throughput in bytes per second observed.
    /// </summary>
    /// <value>The peak data throughput in bytes per second.</value>
    long PeakBytesPerSecond { get; }
    
    /// <summary>
    /// Gets the request rate for read operations.
    /// </summary>
    /// <value>The read requests per second.</value>
    double ReadRequestsPerSecond { get; }
    
    /// <summary>
    /// Gets the request rate for write operations.
    /// </summary>
    /// <value>The write requests per second.</value>
    double WriteRequestsPerSecond { get; }
    
    /// <summary>
    /// Gets the request rate for delete operations.
    /// </summary>
    /// <value>The delete requests per second.</value>
    double DeleteRequestsPerSecond { get; }
}