using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks;

/// <summary>
/// Base interface for Smart Delegates.
/// Smart Delegates provide queryable, composable, multi-stage processing pipelines.
/// </summary>
public interface ISmartDelegate : IEnhancedEnum
{
    /// <summary>
    /// Gets the priority of this delegate (lower numbers execute first)
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Gets whether this delegate can be skipped
    /// </summary>
    bool CanBeSkipped { get; }
    
    /// <summary>
    /// Gets whether this delegate can run in parallel with others
    /// </summary>
    bool CanRunInParallel { get; }
    
    /// <summary>
    /// Gets the category this delegate belongs to
    /// </summary>
    string Category { get; }
}

/// <summary>
/// Smart Delegate interface for pipeline processing
/// </summary>
/// <typeparam name="TContext">The context type that flows through the pipeline</typeparam>
public interface ISmartDelegate<TContext> : ISmartDelegate
    where TContext : IPipelineContext
{
    /// <summary>
    /// Executes this delegate in the pipeline
    /// </summary>
    /// <param name="context">The pipeline context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated context or an error</returns>
    Task<IGenericResult<TContext>> Execute(TContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Determines if this delegate should execute based on the context
    /// </summary>
    /// <param name="context">The pipeline context</param>
    /// <returns>True if this delegate should execute</returns>
    bool ShouldExecute(TContext context);
    
    /// <summary>
    /// Called before execution (middleware hook)
    /// </summary>
    /// <param name="context">The pipeline context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The potentially modified context</returns>
    Task<IGenericResult<TContext>> BeforeExecute(TContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Called after execution (middleware hook)
    /// </summary>
    /// <param name="context">The pipeline context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The potentially modified context</returns>
    Task<IGenericResult<TContext>> AfterExecute(TContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for pipeline context
/// </summary>
public interface IPipelineContext
{
    /// <summary>
    /// Unique identifier for this pipeline execution
    /// </summary>
    Guid CorrelationId { get; }
    
    /// <summary>
    /// Context items that flow through the pipeline
    /// </summary>
    Dictionary<string, object> Items { get; }
    
    /// <summary>
    /// Whether processing should continue
    /// </summary>
    bool ContinueProcessing { get; set; }
    
    /// <summary>
    /// Pipeline execution metadata
    /// </summary>
    IPipelineMetadata Metadata { get; }
}

/// <summary>
/// Interface for pipeline execution metadata
/// </summary>
public interface IPipelineMetadata
{
    /// <summary>
    /// When the pipeline started
    /// </summary>
    DateTime StartTime { get; }
    
    /// <summary>
    /// How long the pipeline has been running
    /// </summary>
    TimeSpan Elapsed { get; }
    
    /// <summary>
    /// List of executed stages
    /// </summary>
    List<StageExecution> ExecutedStages { get; }
    
    /// <summary>
    /// Pipeline metrics
    /// </summary>
    Dictionary<string, object> Metrics { get; }
}

/// <summary>
/// Information about a stage execution
/// </summary>
public class StageExecution
{
    /// <summary>
    /// The stage name
    /// </summary>
    public string StageName { get; set; } = string.Empty;
    
    /// <summary>
    /// When the stage started
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// How long the stage took
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Whether the stage succeeded
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if the stage failed
    /// </summary>
    public string Error { get; set; } = string.Empty;
    
    /// <summary>
    /// Stage-specific metrics
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
}