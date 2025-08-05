using System;
using FractalDataWorks.Framework.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Interface for transformation providers in the FractalDataWorks framework.
/// Provides data transformation capabilities with support for various input and output formats.
/// </summary>
/// <remarks>
/// Transformation providers handle the conversion, mapping, and processing of data between
/// different formats, structures, and representations. They enable data pipeline operations
/// and ETL (Extract, Transform, Load) processes within the framework.
/// </remarks>
public interface ITransformationProvider : IFdwService
{
    /// <summary>
    /// Gets the supported input data types for this transformation provider.
    /// </summary>
    /// <value>A collection of input data type identifiers this provider can process.</value>
    /// <remarks>
    /// Input types may include format specifiers (JSON, XML, CSV), data structures
    /// (objects, dictionaries, streams), or domain-specific types. This enables
    /// the framework to route data to appropriate transformation providers.
    /// </remarks>
    IReadOnlyList<string> SupportedInputTypes { get; }
    
    /// <summary>
    /// Gets the supported output data types for this transformation provider.
    /// </summary>
    /// <value>A collection of output data type identifiers this provider can produce.</value>
    /// <remarks>
    /// Output types specify the formats and structures this provider can generate.
    /// The framework uses this information for transformation chain planning and validation.
    /// </remarks>
    IReadOnlyList<string> SupportedOutputTypes { get; }
    
    /// <summary>
    /// Gets the transformation categories this provider supports.
    /// </summary>
    /// <value>A collection of transformation category names.</value>
    /// <remarks>
    /// Categories help classify transformation types such as "Mapping", "Filtering",
    /// "Aggregation", "Validation", "Formatting". This enables fine-grained
    /// transformation selection and pipeline composition.
    /// </remarks>
    IReadOnlyList<string> TransformationCategories { get; }
    
    /// <summary>
    /// Gets the priority of this provider when multiple providers support the same transformation.
    /// </summary>
    /// <value>A numeric priority value where higher numbers indicate higher priority.</value>
    /// <remarks>
    /// Priority enables provider selection when multiple providers can handle
    /// the same transformation requirements. Higher priority providers are preferred.
    /// </remarks>
    int Priority { get; }
    
    /// <summary>
    /// Validates whether this provider can perform the specified transformation.
    /// </summary>
    /// <param name="inputType">The input data type.</param>
    /// <param name="outputType">The desired output data type.</param>
    /// <param name="transformationCategory">The transformation category (optional).</param>
    /// <returns>A result indicating whether the transformation is supported.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="inputType"/> or <paramref name="outputType"/> is null.
    /// </exception>
    /// <remarks>
    /// This method enables dynamic transformation validation without executing
    /// the actual transformation. Useful for pipeline planning and validation.
    /// </remarks>
    IFdwResult ValidateTransformation(string inputType, string outputType, string? transformationCategory = null);
    
    /// <summary>
    /// Transforms data from the input format to the output format.
    /// </summary>
    /// <typeparam name="TOutput">The expected output data type.</typeparam>
    /// <param name="transformationRequest">The transformation request containing input data and configuration.</param>
    /// <returns>
    /// A task representing the asynchronous transformation operation.
    /// The result contains the transformed data if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformationRequest"/> is null.</exception>
    /// <remarks>
    /// This method performs the actual data transformation using the configuration
    /// and parameters specified in the transformation request.
    /// </remarks>
    Task<IFdwResult<TOutput>> Transform<TOutput>(ITransformationRequest transformationRequest);
    
    /// <summary>
    /// Transforms data from the input format to the output format with non-generic result.
    /// </summary>
    /// <param name="transformationRequest">The transformation request containing input data and configuration.</param>
    /// <returns>
    /// A task representing the asynchronous transformation operation.
    /// The result contains the transformed data if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformationRequest"/> is null.</exception>
    /// <remarks>
    /// This method provides non-generic transformation for scenarios where the output
    /// type is not known at compile time or varies based on runtime configuration.
    /// </remarks>
    Task<IFdwResult<object?>> Transform(ITransformationRequest transformationRequest);
    
    /// <summary>
    /// Creates a transformation engine for complex, multi-step transformations.
    /// </summary>
    /// <param name="configuration">The transformation engine configuration.</param>
    /// <returns>
    /// A task representing the asynchronous engine creation operation.
    /// The result contains the transformation engine if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <remarks>
    /// Transformation engines enable complex, stateful transformations that may
    /// involve multiple steps, caching, or batch processing capabilities.
    /// </remarks>
    Task<IFdwResult<ITransformationEngine>> CreateEngineAsync(ITransformationEngineConfiguration configuration);
    
    /// <summary>
    /// Gets transformation performance metrics for this provider.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous metrics collection operation.
    /// The result contains transformation performance metrics if available.
    /// </returns>
    /// <remarks>
    /// Metrics help monitor transformation performance, identify bottlenecks,
    /// and optimize data processing pipelines.
    /// </remarks>
    Task<IFdwResult<ITransformationMetrics>> GetTransformationMetricsAsync();
}