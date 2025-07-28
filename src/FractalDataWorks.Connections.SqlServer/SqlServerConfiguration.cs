using FractalDataWorks.Configuration;

namespace FractalDataWorks.Connections.SqlServer;

/// <summary>
/// Configuration for SQL Server connections
/// </summary>
public class SqlServerConfiguration : ConnectionConfiguration<SqlServerConfiguration>
{
    /// <summary>
    /// SQL Server connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    
    /// <summary>
    /// Whether to use bulk copy for bulk operations
    /// </summary>
    public bool UseBulkCopy { get; set; } = true;
    
    /// <summary>
    /// Batch size for bulk operations
    /// </summary>
    public int BulkCopyBatchSize { get; set; } = 1000;
    
    /// <summary>
    /// Whether to enable Multiple Active Result Sets
    /// </summary>
    public bool MultipleActiveResultSets { get; set; } = true;
    
    public override bool IsValid => !string.IsNullOrEmpty(ConnectionString) && 
                                   CommandTimeout > 0 && 
                                   BulkCopyBatchSize > 0;
}