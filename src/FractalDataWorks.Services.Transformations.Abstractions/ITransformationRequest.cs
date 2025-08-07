using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Interface for transformation requests in the FractalDataWorks framework.
/// Encapsulates input data, transformation configuration, and execution parameters.
/// </summary>
/// <remarks>
/// Transformation requests provide a standardized way to specify transformation operations
/// including input data, desired output format, transformation rules, and execution options.
/// </remarks>
public interface ITransformationRequest
{
    /// <summary>
    /// Gets the unique identifier for this transformation request.
    /// </summary>
    /// <value>A unique identifier for the transformation request.</value>
    /// <remarks>
    /// Request identifiers are used for tracking, logging, and debugging purposes.
    /// They help correlate transformation operations across distributed systems.
    /// </remarks>
    string RequestId { get; }
    
    /// <summary>
    /// Gets the input data to be transformed.
    /// </summary>
    /// <value>The source data for transformation.</value>
    /// <remarks>
    /// Input data can be of various types including objects, streams, collections,
    /// or formatted data (JSON, XML, etc.). The transformation provider determines
    /// how to interpret and process the input data.
    /// </remarks>
    object? InputData { get; }
    
    /// <summary>
    /// Gets the type of the input data.
    /// </summary>
    /// <value>The input data type identifier.</value>
    /// <remarks>
    /// Input type helps transformation providers understand how to interpret
    /// and process the input data. Common types include "JSON", "XML", "CSV",
    /// "Object", "Stream", or custom domain-specific identifiers.
    /// </remarks>
    string InputType { get; }
    
    /// <summary>
    /// Gets the desired output data type.
    /// </summary>
    /// <value>The target output data type identifier.</value>
    /// <remarks>
    /// Output type specifies the desired format or structure for the transformation result.
    /// The transformation provider uses this to determine the appropriate processing logic.
    /// </remarks>
    string OutputType { get; }
    
    /// <summary>
    /// Gets the category of transformation to perform.
    /// </summary>
    /// <value>The transformation category, or null if not specified.</value>
    /// <remarks>
    /// Transformation categories help specify the type of processing required
    /// such as "Mapping", "Filtering", "Aggregation", "Validation", or "Formatting".
    /// This enables more precise transformation provider selection.
    /// </remarks>
    string? TransformationCategory { get; }
    
    /// <summary>
    /// Gets the transformation configuration parameters.
    /// </summary>
    /// <value>A dictionary of configuration parameters for the transformation.</value>
    /// <remarks>
    /// Configuration parameters provide transformation-specific settings such as
    /// mapping rules, filter criteria, aggregation functions, or formatting options.
    /// The structure and content depend on the specific transformation being performed.
    /// </remarks>
    IReadOnlyDictionary<string, object> Configuration { get; }
    
    /// <summary>
    /// Gets additional options for transformation execution.
    /// </summary>
    /// <value>A dictionary of execution options.</value>
    /// <remarks>
    /// Execution options may include performance hints, caching directives,
    /// error handling preferences, or other settings that influence how
    /// the transformation is executed rather than what is transformed.
    /// </remarks>
    IReadOnlyDictionary<string, object> Options { get; }
    
    /// <summary>
    /// Gets the timeout for transformation execution.
    /// </summary>
    /// <value>The maximum time to wait for transformation completion, or null for no timeout.</value>
    /// <remarks>
    /// Transformation timeouts prevent long-running operations from blocking
    /// system resources indefinitely. When exceeded, the transformation is cancelled.
    /// </remarks>
    TimeSpan? Timeout { get; }
    
    /// <summary>
    /// Gets the context information for this transformation.
    /// </summary>
    /// <value>The transformation context, or null if not applicable.</value>
    /// <remarks>
    /// Context provides additional information about the transformation environment
    /// such as user identity, security context, or pipeline stage information.
    /// </remarks>
    ITransformationContext? Context { get; }
    
    /// <summary>
    /// Gets the expected result type for this transformation.
    /// </summary>
    /// <value>The Type of object expected to be returned by the transformation.</value>
    /// <remarks>
    /// Expected result type enables transformation providers to prepare appropriate
    /// result handling and type conversion logic before executing the transformation.
    /// </remarks>
    Type ExpectedResultType { get; }
    
    /// <summary>
    /// Creates a copy of this request with modified input data.
    /// </summary>
    /// <param name="newInputData">The new input data.</param>
    /// <param name="newInputType">The new input type (optional, defaults to current type).</param>
    /// <returns>A new transformation request with the specified input data.</returns>
    /// <remarks>
    /// This method enables request reuse with different input data without
    /// modifying the original request instance. Useful for batch processing scenarios.
    /// </remarks>
    ITransformationRequest WithInputData(object? newInputData, string? newInputType = null);
    
    /// <summary>
    /// Creates a copy of this request with modified output type.
    /// </summary>
    /// <param name="newOutputType">The new output type.</param>
    /// <param name="newExpectedResultType">The new expected result type (optional).</param>
    /// <returns>A new transformation request with the specified output type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newOutputType"/> is null.</exception>
    /// <remarks>
    /// This method enables request adaptation for different output requirements
    /// while preserving other request configuration.
    /// </remarks>
    ITransformationRequest WithOutputType(string newOutputType, Type? newExpectedResultType = null);
    
    /// <summary>
    /// Creates a copy of this request with modified configuration.
    /// </summary>
    /// <param name="newConfiguration">The new configuration parameters.</param>
    /// <returns>A new transformation request with the specified configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newConfiguration"/> is null.</exception>
    /// <remarks>
    /// This method enables request customization with different transformation
    /// parameters while preserving input/output specifications.
    /// </remarks>
    ITransformationRequest WithConfiguration(IReadOnlyDictionary<string, object> newConfiguration);
    
    /// <summary>
    /// Creates a copy of this request with modified execution options.
    /// </summary>
    /// <param name="newOptions">The new execution options.</param>
    /// <returns>A new transformation request with the specified options.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newOptions"/> is null.</exception>
    /// <remarks>
    /// This method enables request customization with different execution
    /// preferences while preserving transformation specifications.
    /// </remarks>
    ITransformationRequest WithOptions(IReadOnlyDictionary<string, object> newOptions);
}