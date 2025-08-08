using System.Collections.Generic;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Represents the result of a transformation operation.
/// </summary>
public interface ITransformationResult
{
    /// <summary>
    /// Gets the transformed data.
    /// </summary>
    object? Data { get; }
    
    /// <summary>
    /// Gets the output data type.
    /// </summary>
    string OutputType { get; }
    
    /// <summary>
    /// Gets transformation metadata.
    /// </summary>
    IReadOnlyDictionary<string, object?> Metadata { get; }
    
    /// <summary>
    /// Gets the transformation duration in milliseconds.
    /// </summary>
    long DurationMs { get; }
}