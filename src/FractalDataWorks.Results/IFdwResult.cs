using System;

namespace FractalDataWorks;

/// <summary>
/// Represents a result that can be either success or failure.
/// </summary>
public interface IFdwResult : IGenericResult
{
    /// <summary>
    /// Gets a value indicating whether this represents an empty result
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Gets a value indicating whether this result represents an error.
    /// </summary>
    bool Error { get; }
}

/// <summary>
/// Represents a result that can be either success or failure with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public interface IFdwResult<T> : IFdwResult, IGenericResult<T>
{
}
