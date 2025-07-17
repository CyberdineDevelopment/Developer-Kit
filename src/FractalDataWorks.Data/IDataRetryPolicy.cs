namespace FractalDataWorks.Data;

/// <summary>
/// Defines retry policy for data operations.
/// </summary>
public interface IDataRetryPolicy
{
    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    int MaxRetries { get; }

    /// <summary>
    /// Gets the delay between retries in milliseconds.
    /// </summary>
    int RetryDelayMs { get; }

    /// <summary>
    /// Gets a value indicating whether to use exponential backoff.
    /// </summary>
    bool UseExponentialBackoff { get; }

    /// <summary>
    /// Gets the maximum delay in milliseconds.
    /// </summary>
    int MaxRetryDelayMs { get; }
}