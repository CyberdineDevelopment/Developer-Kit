using System.Collections.Generic;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Interface for transformation context information.
/// Provides additional metadata and environment information for transformations.
/// </summary>
/// <remarks>
/// Transformation context enables providers to access additional information
/// that may influence transformation behavior, such as security context,
/// pipeline information, or environment-specific configuration.
/// </remarks>
public interface ITransformationContext
{
    /// <summary>
    /// Gets the user or system identity associated with this transformation.
    /// </summary>
    /// <value>The identity information, or null if not applicable.</value>
    string? Identity { get; }
    
    /// <summary>
    /// Gets the correlation identifier for tracking this transformation across systems.
    /// </summary>
    /// <value>The correlation identifier, or null if not applicable.</value>
    string? CorrelationId { get; }
    
    /// <summary>
    /// Gets the pipeline stage or step information.
    /// </summary>
    /// <value>The pipeline stage identifier, or null if not part of a pipeline.</value>
    string? PipelineStage { get; }
    
    /// <summary>
    /// Gets additional context properties.
    /// </summary>
    /// <value>A dictionary of context-specific properties.</value>
    /// <remarks>
    /// Context properties provide extensible metadata that may be relevant
    /// for specific transformation scenarios or provider implementations.
    /// </remarks>
    IReadOnlyDictionary<string, object> Properties { get; }
}