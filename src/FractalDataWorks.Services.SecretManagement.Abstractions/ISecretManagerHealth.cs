using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Interface representing the health status of a secret manager.
/// Provides aggregate health information across all registered providers.
/// </summary>
/// <remarks>
/// Manager health provides a system-wide view of secret management capabilities
/// and can be used for overall system health monitoring.
/// </remarks>
public interface ISecretManagerHealth
{
    /// <summary>
    /// Gets a value indicating whether the secret manager is healthy overall.
    /// </summary>
    /// <value><c>true</c> if the manager is healthy; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// The manager is considered healthy if at least one provider is healthy
    /// and critical providers are operational.
    /// </remarks>
    bool IsHealthy { get; }

    /// <summary>
    /// Gets the total number of registered providers.
    /// </summary>
    /// <value>The total provider count.</value>
    int TotalProviders { get; }

    /// <summary>
    /// Gets the number of healthy providers.
    /// </summary>
    /// <value>The healthy provider count.</value>
    int HealthyProviders { get; }

    /// <summary>
    /// Gets the number of unhealthy providers.
    /// </summary>
    /// <value>The unhealthy provider count.</value>
    int UnhealthyProviders { get; }

    /// <summary>
    /// Gets the number of providers with unknown health status.
    /// </summary>
    /// <value>The unknown status provider count.</value>
    int UnknownStatusProviders { get; }

    /// <summary>
    /// Gets the last time a health check was performed.
    /// </summary>
    /// <value>The timestamp of the last health check.</value>
    DateTimeOffset LastCheckTime { get; }

    /// <summary>
    /// Gets the total time taken for the last health check.
    /// </summary>
    /// <value>The duration of the last health check operation.</value>
    TimeSpan TotalCheckTime { get; }

    /// <summary>
    /// Gets the health status of individual providers.
    /// </summary>
    /// <value>A collection of provider health status information.</value>
    IReadOnlyList<ISecretProviderHealth> ProviderHealthStatuses { get; }

    /// <summary>
    /// Gets any system-level error messages.
    /// </summary>
    /// <value>A collection of system-level error messages, or empty if healthy.</value>
    IReadOnlyList<string> SystemErrors { get; }

    /// <summary>
    /// Gets any system-level warning messages.
    /// </summary>
    /// <value>A collection of system-level warning messages, or empty if no warnings.</value>
    IReadOnlyList<string> SystemWarnings { get; }

    /// <summary>
    /// Gets additional system health metadata.
    /// </summary>
    /// <value>A dictionary of system health metadata properties.</value>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the overall health status level.
    /// </summary>
    /// <value>The health status level.</value>
    HealthStatus Status { get; }
}