using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.DataProvider.Abstractions;

/// <summary>
/// Interface for data provider performance metrics in the FractalDataWorks framework.
/// Provides insights into provider performance and health characteristics.
/// </summary>
/// <remarks>
/// Provider metrics enable monitoring, optimization, and troubleshooting of data access operations.
/// Metrics are collected during normal operation and provide both real-time and historical insights.
/// </remarks>
public interface IProviderMetrics
{
    /// <summary>
    /// Gets the provider identifier these metrics belong to.
    /// </summary>
    /// <value>The unique identifier of the data provider.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets the total number of commands executed by this provider.
    /// </summary>
    /// <value>The total command execution count since provider initialization.</value>
    long TotalCommandsExecuted { get; }
    
    /// <summary>
    /// Gets the number of successful command executions.
    /// </summary>
    /// <value>The count of commands that completed successfully.</value>
    long SuccessfulCommands { get; }
    
    /// <summary>
    /// Gets the number of failed command executions.
    /// </summary>
    /// <value>The count of commands that failed during execution.</value>
    long FailedCommands { get; }
    
    /// <summary>
    /// Gets the average command execution time.
    /// </summary>
    /// <value>The average time taken to execute commands, or null if no commands have been executed.</value>
    /// <remarks>
    /// This metric helps identify performance trends and potential bottlenecks in data access.
    /// It includes both successful and failed command execution times.
    /// </remarks>
    TimeSpan? AverageExecutionTime { get; }
    
    /// <summary>
    /// Gets the minimum command execution time observed.
    /// </summary>
    /// <value>The fastest command execution time, or null if no commands have been executed.</value>
    TimeSpan? MinExecutionTime { get; }
    
    /// <summary>
    /// Gets the maximum command execution time observed.
    /// </summary>
    /// <value>The slowest command execution time, or null if no commands have been executed.</value>
    TimeSpan? MaxExecutionTime { get; }
    
    /// <summary>
    /// Gets the current number of active connections.
    /// </summary>
    /// <value>The count of currently active connections, or null if not applicable.</value>
    /// <remarks>
    /// For connection-based providers, this indicates the current connection pool usage.
    /// Helps monitor connection resource utilization and potential connection leaks.
    /// </remarks>
    int? ActiveConnections { get; }
    
    /// <summary>
    /// Gets the maximum number of concurrent connections observed.
    /// </summary>
    /// <value>The peak concurrent connection count, or null if not applicable.</value>
    int? PeakConnections { get; }
    
    /// <summary>
    /// Gets the number of connection failures encountered.
    /// </summary>
    /// <value>The count of connection establishment failures.</value>
    /// <remarks>
    /// Connection failures may indicate network issues, authentication problems,
    /// or resource exhaustion scenarios that affect provider reliability.
    /// </remarks>
    long ConnectionFailures { get; }
    
    /// <summary>
    /// Gets the number of commands currently in the execution queue.
    /// </summary>
    /// <value>The count of commands waiting to be executed, or null if not applicable.</value>
    /// <remarks>
    /// Queue depth indicates provider load and potential performance bottlenecks.
    /// High queue depths may suggest the need for additional provider instances or optimization.
    /// </remarks>
    int? QueuedCommands { get; }
    
    /// <summary>
    /// Gets command execution statistics by command type.
    /// </summary>
    /// <value>A dictionary mapping command types to their execution statistics.</value>
    /// <remarks>
    /// Per-command-type metrics help identify which operations are most frequently used
    /// and which may be causing performance issues. Statistics include execution counts,
    /// average times, and error rates for each command type.
    /// </remarks>
    IReadOnlyDictionary<string, ICommandTypeMetrics> CommandTypeMetrics { get; }
    
    /// <summary>
    /// Gets the timestamp when these metrics were last updated.
    /// </summary>
    /// <value>The UTC timestamp of the last metrics update.</value>
    /// <remarks>
    /// The timestamp helps determine the freshness of metrics data and enables
    /// time-based analysis of provider performance trends.
    /// </remarks>
    DateTimeOffset LastUpdated { get; }
    
    /// <summary>
    /// Gets additional custom metrics specific to this provider type.
    /// </summary>
    /// <value>A dictionary of custom metric names and their values.</value>
    /// <remarks>
    /// Custom metrics allow providers to expose additional performance indicators
    /// that may be specific to their implementation or underlying technology.
    /// Examples include cache hit rates, batch sizes, or protocol-specific metrics.
    /// </remarks>
    IReadOnlyDictionary<string, object> CustomMetrics { get; }
    
    /// <summary>
    /// Gets the uptime of this provider instance.
    /// </summary>
    /// <value>The duration since the provider was initialized and became available.</value>
    /// <remarks>
    /// Uptime provides context for other metrics and helps assess provider stability.
    /// Combined with command counts, it can indicate provider usage patterns over time.
    /// </remarks>
    TimeSpan Uptime { get; }
}