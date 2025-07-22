using FractalDataWorks;

namespace FractalDataWorks;

/// <summary>
/// Represents a result that can be either success or failure.
/// </summary>
public interface IFdwResult
{
    /// <summary>
    /// Gets a value indicating whether this result represents a success.
    /// </summary>
    bool IsSuccess { get; }
    
    /// <summary>
    /// Gets a value indicating whether this result represents a failure.
    /// </summary>
    bool IsFailure { get; }
    
    /// <summary>
    /// Gets the message associated with this result, if any.
    /// </summary>
    IFdwMessage? Message { get; }
}

/// <summary>
/// Represents a result that can be either success or failure with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public interface IFdwResult<T> : IFdwResult
{
    /// <summary>
    /// Gets the value of this result.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown when accessing value on a failed result.</exception>
    T Value { get; }
}
