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