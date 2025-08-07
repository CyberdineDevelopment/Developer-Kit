using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.Authentication.Abstractions;

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