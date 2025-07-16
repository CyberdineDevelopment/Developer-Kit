using FractalDataWorks.Configuration;

namespace FractalDataWorks.Data;

/// <summary>
/// Defines the contract for data connection configurations.
/// </summary>
public interface IDataConfiguration : IFractalConfiguration
{
    /// <summary>
    /// Gets the connection string or endpoint.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Gets the provider type.
    /// </summary>
    string ProviderType { get; }

    /// <summary>
    /// Gets the connection timeout in seconds.
    /// </summary>
    int ConnectionTimeoutSeconds { get; }

    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    int CommandTimeoutSeconds { get; }

    /// <summary>
    /// Gets a value indicating whether to enable connection pooling.
    /// </summary>
    bool EnableConnectionPooling { get; }

    /// <summary>
    /// Gets the maximum pool size.
    /// </summary>
    int MaxPoolSize { get; }

    /// <summary>
    /// Gets a value indicating whether to enable automatic retry.
    /// </summary>
    bool EnableAutoRetry { get; }

    /// <summary>
    /// Gets the retry policy configuration.
    /// </summary>
    IDataRetryPolicy? RetryPolicy { get; }
}

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