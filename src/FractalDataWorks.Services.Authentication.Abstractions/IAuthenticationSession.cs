using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface representing an active authentication session.
/// Provides information about user sessions and their current state.
/// </summary>
public interface IAuthenticationSession
{
    /// <summary>
    /// Gets the unique identifier for this session.
    /// </summary>
    /// <value>The session identifier.</value>
    string SessionId { get; }
    
    /// <summary>
    /// Gets the user identifier associated with this session.
    /// </summary>
    /// <value>The user identifier.</value>
    string UserId { get; }
    
    /// <summary>
    /// Gets the provider that manages this session.
    /// </summary>
    /// <value>The provider identifier.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets when the session was created.
    /// </summary>
    /// <value>The session creation timestamp.</value>
    DateTimeOffset CreatedAt { get; }
    
    /// <summary>
    /// Gets when the session was last accessed.
    /// </summary>
    /// <value>The last access timestamp.</value>
    DateTimeOffset LastAccessedAt { get; }
    
    /// <summary>
    /// Gets when the session expires.
    /// </summary>
    /// <value>The session expiration timestamp, or null if it doesn't expire.</value>
    DateTimeOffset? ExpiresAt { get; }
    
    /// <summary>
    /// Gets a value indicating whether this session is currently active.
    /// </summary>
    /// <value><c>true</c> if the session is active; otherwise, <c>false</c>.</value>
    bool IsActive { get; }
    
    /// <summary>
    /// Gets a value indicating whether this session has expired.
    /// </summary>
    /// <value><c>true</c> if the session has expired; otherwise, <c>false</c>.</value>
    bool IsExpired { get; }
    
    /// <summary>
    /// Gets the authentication method used for this session.
    /// </summary>
    /// <value>The authentication method.</value>
    string AuthenticationMethod { get; }
    
    /// <summary>
    /// Gets the realm or domain for this session.
    /// </summary>
    /// <value>The realm identifier, or null if not applicable.</value>
    string? Realm { get; }
    
    /// <summary>
    /// Gets additional session metadata.
    /// </summary>
    /// <value>A dictionary of session metadata properties.</value>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Interface for authentication realm information.
/// </summary>
public interface IAuthenticationRealm
{
    /// <summary>
    /// Gets the realm identifier.
    /// </summary>
    /// <value>The realm identifier.</value>
    string RealmId { get; }
    
    /// <summary>
    /// Gets the realm name.
    /// </summary>
    /// <value>The realm display name.</value>
    string Name { get; }
    
    /// <summary>
    /// Gets the realm description.
    /// </summary>
    /// <value>The realm description, or null if not provided.</value>
    string? Description { get; }
    
    /// <summary>
    /// Gets a value indicating whether this realm is enabled.
    /// </summary>
    /// <value><c>true</c> if the realm is enabled; otherwise, <c>false</c>.</value>
    bool IsEnabled { get; }
    
    /// <summary>
    /// Gets the supported authentication flows for this realm.
    /// </summary>
    /// <value>A collection of supported authentication flow names.</value>
    IReadOnlyCollection<string> SupportedAuthenticationFlows { get; }
}

/// <summary>
/// Interface for authentication provider health information.
/// </summary>
public interface IAuthenticationProviderHealth
{
    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    /// <value>The provider identifier.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets a value indicating whether the provider is healthy.
    /// </summary>
    /// <value><c>true</c> if the provider is healthy; otherwise, <c>false</c>.</value>
    bool IsHealthy { get; }
    
    /// <summary>
    /// Gets the last health check time.
    /// </summary>
    /// <value>The last health check timestamp.</value>
    DateTimeOffset LastCheckTime { get; }
    
    /// <summary>
    /// Gets the health check response time.
    /// </summary>
    /// <value>The response time duration.</value>
    TimeSpan ResponseTime { get; }
    
    /// <summary>
    /// Gets any health check error messages.
    /// </summary>
    /// <value>A collection of error messages.</value>
    IReadOnlyList<string> ErrorMessages { get; }
}

/// <summary>
/// Interface for authentication service health information.
/// </summary>
public interface IAuthenticationServiceHealth
{
    /// <summary>
    /// Gets a value indicating whether the service is healthy overall.
    /// </summary>
    /// <value><c>true</c> if the service is healthy; otherwise, <c>false</c>.</value>
    bool IsHealthy { get; }
    
    /// <summary>
    /// Gets the provider health statuses.
    /// </summary>
    /// <value>A collection of provider health information.</value>
    IReadOnlyList<IAuthenticationProviderHealth> ProviderHealthStatuses { get; }
}

/// <summary>
/// Interface for authentication provider metrics.
/// </summary>
public interface IAuthenticationProviderMetrics
{
    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    /// <value>The provider identifier.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets the total number of operations performed.
    /// </summary>
    /// <value>The total operation count.</value>
    long TotalOperations { get; }
    
    /// <summary>
    /// Gets the number of successful operations.
    /// </summary>
    /// <value>The successful operation count.</value>
    long SuccessfulOperations { get; }
    
    /// <summary>
    /// Gets the number of failed operations.
    /// </summary>
    /// <value>The failed operation count.</value>
    long FailedOperations { get; }
    
    /// <summary>
    /// Gets the average response time.
    /// </summary>
    /// <value>The average response time.</value>
    TimeSpan AverageResponseTime { get; }
    
    /// <summary>
    /// Gets when these metrics were collected.
    /// </summary>
    /// <value>The metrics collection timestamp.</value>
    DateTimeOffset CollectedAt { get; }
}