using System;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

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