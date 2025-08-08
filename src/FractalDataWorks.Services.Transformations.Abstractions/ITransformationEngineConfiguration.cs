using FractalDataWorks;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Configuration for a transformation engine.
/// </summary>
public interface ITransformationEngineConfiguration : IFdwConfiguration
{
    /// <summary>
    /// Gets the engine type to create.
    /// </summary>
    string EngineType { get; }
    
    /// <summary>
    /// Gets the maximum number of concurrent transformations.
    /// </summary>
    int MaxConcurrency { get; }
    
    /// <summary>
    /// Gets the timeout for transformation operations in seconds.
    /// </summary>
    int TimeoutSeconds { get; }
    
    /// <summary>
    /// Gets a value indicating whether to enable caching.
    /// </summary>
    bool EnableCaching { get; }
    
    /// <summary>
    /// Gets a value indicating whether to enable performance metrics collection.
    /// </summary>
    bool EnableMetrics { get; }
}