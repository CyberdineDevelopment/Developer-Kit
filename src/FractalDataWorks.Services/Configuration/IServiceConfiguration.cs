using FractalDataWorks.Configuration;

namespace FractalDataWorks.Services.Configuration;

/// <summary>
/// Base interface for service configurations.
/// </summary>
public interface IServiceConfiguration : IFdwConfiguration
{
    /// <summary>
    /// Gets the service type this configuration is for.
    /// </summary>
    string ServiceType { get; }

    /// <summary>
    /// Gets the retry policy configuration.
    /// </summary>
    IRetryPolicyConfiguration? RetryPolicy { get; }

    /// <summary>
    /// Gets the timeout in milliseconds.
    /// </summary>
    int TimeoutMs { get; }
}

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