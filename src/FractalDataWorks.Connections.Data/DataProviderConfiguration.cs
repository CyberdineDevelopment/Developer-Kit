using System.Collections.Generic;
using FractalDataWorks.Configuration;

namespace FractalDataWorks.Connections.Data;

/// <summary>
/// Configuration for data provider connections
/// </summary>
public class DataProviderConfiguration : ConfigurationBase<DataProviderConfiguration>
{
    /// <summary>
    /// Named connection configurations
    /// </summary>
    public Dictionary<string, ConnectionConfiguration> Connections { get; set; } = new();
    
    /// <summary>
    /// Default connection ID to use when none specified
    /// </summary>
    public string? DefaultConnectionId { get; set; }
    
    /// <summary>
    /// Whether to automatically register all discovered providers
    /// </summary>
    public bool AutoRegisterProviders { get; set; } = true;
    
    public override bool IsValid => Connections != null;
}

/// <summary>
/// Configuration for a single connection
/// </summary>
public class ConnectionConfiguration
{
    /// <summary>
    /// The data store type (SqlServer, FileSystem, RestApi, etc.)
    /// </summary>
    public string DataStore { get; set; } = string.Empty;
    
    /// <summary>
    /// Connection-specific settings (connection string, path, URL, etc.)
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();
    
    /// <summary>
    /// Optional description for this connection
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this connection is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
}