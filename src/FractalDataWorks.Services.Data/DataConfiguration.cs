using FractalDataWorks.Configuration;

namespace FractalDataWorks.Services.Data;

/// <summary>
/// Configuration for the data service
/// </summary>
public class DataConfiguration : ConfigurationBase<DataConfiguration>
{
    /// <summary>
    /// Default connection ID to use when none specified
    /// </summary>
    public string? DefaultConnectionId { get; set; }
    
    /// <summary>
    /// Whether to log command details (be careful with sensitive data)
    /// </summary>
    public bool LogCommandDetails { get; set; } = false;
    
    /// <summary>
    /// Default timeout for commands in seconds
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Whether to automatically retry failed commands
    /// </summary>
    public bool EnableAutoRetry { get; set; } = false;
    
    /// <summary>
    /// Number of retry attempts if auto-retry is enabled
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Delay between retry attempts in milliseconds
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;
    
    public override bool IsValid => DefaultTimeoutSeconds > 0 && 
                                   MaxRetryAttempts >= 0 && 
                                   RetryDelayMilliseconds >= 0;
}