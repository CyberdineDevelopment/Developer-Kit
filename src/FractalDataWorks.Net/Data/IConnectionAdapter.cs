using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Connections;

namespace FractalDataWorks.Data;

/// <summary>
/// Interface for executing parsed operations against specific data providers.
/// Adapters handle the actual communication with the data store.
/// </summary>
public interface IConnectionAdapter
{
    /// <summary>
    /// Executes a parsed operation and returns the result
    /// </summary>
    /// <typeparam name="T">The expected result type</typeparam>
    /// <param name="operation">The parsed operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The execution result</returns>
    Task<IGenericResult<T>> Execute<T>(IParsedOperation operation, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tests the connection to the data store
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection is healthy</returns>
    Task<IGenericResult<bool>> TestConnection(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets information about the data store
    /// </summary>
    /// <returns>Data store information</returns>
    Task<IGenericResult<DataStoreInfo>> GetDataStoreInfo();
    
    /// <summary>
    /// Gets the provider capabilities
    /// </summary>
    ProviderCapabilities Capabilities { get; }
}

/// <summary>
/// Information about a data store
/// </summary>
public record DataStoreInfo
{
    /// <summary>
    /// The provider name (e.g., "SqlServer", "JsonFile", "RestApi")
    /// </summary>
    public string ProviderName { get; init; } = string.Empty;
    
    /// <summary>
    /// The provider version
    /// </summary>
    public string ProviderVersion { get; init; } = string.Empty;
    
    /// <summary>
    /// Available containers (tables, files, etc.)
    /// </summary>
    public List<string> AvailableContainers { get; init; } = new();
    
    /// <summary>
    /// Provider-specific properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}
