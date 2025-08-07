namespace FractalDataWorks.Services.SecretManagement.Abstractions;

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