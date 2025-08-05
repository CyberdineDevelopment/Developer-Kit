using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Interface representing the health status of a secret provider.
/// Provides detailed information about the provider's operational state and connectivity.
/// </summary>
/// <remarks>
/// Health information helps monitor the operational status of secret providers
/// and enables automated failover or alerting based on provider availability.
/// </remarks>
public interface ISecretProviderHealth
{
    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    /// <value>The unique identifier for the provider.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    /// <value>The display name of the provider.</value>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the provider type.
    /// </summary>
    /// <value>The provider type identifier.</value>
    string ProviderType { get; }
    
    /// <summary>
    /// Gets a value indicating whether the provider is healthy and operational.
    /// </summary>
    /// <value><c>true</c> if the provider is healthy; otherwise, <c>false</c>.</value>
    bool IsHealthy { get; }
    
    /// <summary>
    /// Gets a value indicating whether the provider can connect to its backend service.
    /// </summary>
    /// <value><c>true</c> if connectivity is available; otherwise, <c>false</c>.</value>
    bool HasConnectivity { get; }
    
    /// <summary>
    /// Gets a value indicating whether the provider is properly authenticated.
    /// </summary>
    /// <value><c>true</c> if authentication is valid; otherwise, <c>false</c>.</value>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Gets the last time a health check was performed.
    /// </summary>
    /// <value>The timestamp of the last health check.</value>
    DateTimeOffset LastCheckTime { get; }
    
    /// <summary>
    /// Gets the response time for the last health check.
    /// </summary>
    /// <value>The duration of the last health check operation.</value>
    TimeSpan ResponseTime { get; }
    
    /// <summary>
    /// Gets any error messages from the health check.
    /// </summary>
    /// <value>A collection of error messages, or empty if healthy.</value>
    IReadOnlyList<string> ErrorMessages { get; }
    
    /// <summary>
    /// Gets any warning messages from the health check.
    /// </summary>
    /// <value>A collection of warning messages, or empty if no warnings.</value>
    IReadOnlyList<string> WarningMessages { get; }
    
    /// <summary>
    /// Gets additional health-related metadata.
    /// </summary>
    /// <value>A dictionary of health metadata properties.</value>
    /// <remarks>
    /// Metadata may include provider-specific health indicators, version information,
    /// capacity metrics, or other operational details.
    /// </remarks>
    IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Gets the health status level.
    /// </summary>
    /// <value>The health status level.</value>
    HealthStatus Status { get; }
}

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

/// <summary>
/// Enumeration of health status levels.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// Health status is unknown or could not be determined.
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// System is healthy and fully operational.
    /// </summary>
    Healthy = 1,
    
    /// <summary>
    /// System is operational but has warnings or minor issues.
    /// </summary>
    Warning = 2,
    
    /// <summary>
    /// System is degraded but still partially functional.
    /// </summary>
    Degraded = 3,
    
    /// <summary>
    /// System is unhealthy and not functioning properly.
    /// </summary>
    Unhealthy = 4,
    
    /// <summary>
    /// System is completely unavailable or failed.
    /// </summary>
    Critical = 5
}