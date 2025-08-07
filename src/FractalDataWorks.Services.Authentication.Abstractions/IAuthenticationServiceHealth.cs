using System.Collections.Generic;

namespace FractalDataWorks.Services.Authentication.Abstractions;

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