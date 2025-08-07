namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Interface representing throughput metrics for a secret provider.
/// Provides information about data transfer rates and operation frequencies.
/// </summary>
/// <remarks>
/// Throughput metrics help assess provider capacity and identify
/// performance bottlenecks in high-load scenarios.
/// </remarks>
public interface ISecretProviderThroughput
{
    /// <summary>
    /// Gets the operations per second rate.
    /// </summary>
    /// <value>The operations per second.</value>
    double OperationsPerSecond { get; }
    
    /// <summary>
    /// Gets the peak operations per second rate observed.
    /// </summary>
    /// <value>The peak operations per second.</value>
    double PeakOperationsPerSecond { get; }
    
    /// <summary>
    /// Gets the data throughput in bytes per second.
    /// </summary>
    /// <value>The data throughput in bytes per second.</value>
    long BytesPerSecond { get; }
    
    /// <summary>
    /// Gets the peak data throughput in bytes per second observed.
    /// </summary>
    /// <value>The peak data throughput in bytes per second.</value>
    long PeakBytesPerSecond { get; }
    
    /// <summary>
    /// Gets the request rate for read operations.
    /// </summary>
    /// <value>The read requests per second.</value>
    double ReadRequestsPerSecond { get; }
    
    /// <summary>
    /// Gets the request rate for write operations.
    /// </summary>
    /// <value>The write requests per second.</value>
    double WriteRequestsPerSecond { get; }
    
    /// <summary>
    /// Gets the request rate for delete operations.
    /// </summary>
    /// <value>The delete requests per second.</value>
    double DeleteRequestsPerSecond { get; }
}