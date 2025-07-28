namespace FractalDataWorks.Connections;

/// <summary>
/// Base class for data connection configurations
/// </summary>
/// <typeparam name="T">The derived configuration type</typeparam>
public abstract class DataConnectionConfiguration<T> : ConnectionConfiguration<T>
    where T : DataConnectionConfiguration<T>
{
    /// <summary>
    /// Default container/database/collection name
    /// </summary>
    public string? DefaultContainer { get; set; }
    
    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    
    /// <summary>
    /// Whether to enable connection pooling
    /// </summary>
    public bool EnablePooling { get; set; } = true;
    
    /// <summary>
    /// Maximum pool size
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;
}