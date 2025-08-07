using System;

namespace FractalDataWorks.Services.Authentication.Abstractions;

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