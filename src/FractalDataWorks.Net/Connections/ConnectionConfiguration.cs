using FractalDataWorks.Configuration;

namespace FractalDataWorks.Connections;

/// <summary>
/// Base class for connection configurations
/// </summary>
/// <typeparam name="T">The derived configuration type</typeparam>
public abstract class ConnectionConfiguration<T> : ConfigurationBase
    where T : ConnectionConfiguration<T>
{
    /// <summary>
    /// Connection string or identifier
    /// </summary>
    public string? ConnectionString { get; set; }
    
    /// <summary>
    /// Connection name for identification
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Provider type (SqlServer, FileSystem, etc.)
    /// </summary>
    public string? Provider { get; set; }
}