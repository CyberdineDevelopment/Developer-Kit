using System;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Represents transformation performance metrics.
/// </summary>
public interface ITransformationMetrics
{
    /// <summary>
    /// Gets the total number of transformations executed.
    /// </summary>
    long TotalTransformations { get; }
    
    /// <summary>
    /// Gets the number of successful transformations.
    /// </summary>
    long SuccessfulTransformations { get; }
    
    /// <summary>
    /// Gets the number of failed transformations.
    /// </summary>
    long FailedTransformations { get; }
    
    /// <summary>
    /// Gets the average transformation duration in milliseconds.
    /// </summary>
    double AverageTransformationDurationMs { get; }
    
    /// <summary>
    /// Gets the current number of active transformations.
    /// </summary>
    int ActiveTransformations { get; }
    
    /// <summary>
    /// Gets the timestamp when metrics collection started.
    /// </summary>
    DateTime MetricsStartTime { get; }
    
    /// <summary>
    /// Gets the last transformation timestamp.
    /// </summary>
    DateTime? LastTransformationTime { get; }
}