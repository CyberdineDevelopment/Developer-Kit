namespace FractalDataWorks.Services.Configuration;

/// <summary>
/// Defines retry policy configuration.
/// </summary>
public interface IRetryPolicyConfiguration
{
    /// <summary>
    /// Gets the maximum number of retry attempts.
    /// </summary>
    int MaxRetries { get; }

    /// <summary>
    /// Gets the initial retry delay in milliseconds.
    /// </summary>
    int InitialDelayMs { get; }

    /// <summary>
    /// Gets the backoff multiplier.
    /// </summary>
    double BackoffMultiplier { get; }

    /// <summary>
    /// Gets the maximum delay in milliseconds.
    /// </summary>
    int MaxDelayMs { get; }
}