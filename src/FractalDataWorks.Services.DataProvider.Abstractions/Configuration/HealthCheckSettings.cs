namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Health check configuration for monitoring data store availability.
/// </summary>
public sealed class HealthCheckSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether health checks are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval in seconds between health checks.
    /// </summary>
    public int IntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the timeout in seconds for each health check.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of consecutive failures before marking as unhealthy.
    /// </summary>
    public int MaxFailures { get; set; } = 3;

    /// <summary>
    /// Gets or sets the custom health check query or operation.
    /// </summary>
    /// <remarks>
    /// Provider-specific health check logic. For SQL: a simple query like "SELECT 1".
    /// For APIs: a health endpoint path. For files: a directory existence check.
    /// </remarks>
    public string? HealthCheckQuery { get; set; }
}