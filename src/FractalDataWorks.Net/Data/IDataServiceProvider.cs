using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Connections;

namespace FractalDataWorks.Data;

/// <summary>
/// Provides data connections based on configuration or provider type
/// </summary>
public interface IDataServiceProvider
{
    /// <summary>
    /// Gets a data connection by provider type
    /// </summary>
    /// <param name="dataStore">The data store type (SqlServer, FileSystem, etc.)</param>
    /// <returns>Result containing the connection or an error</returns>
    IGenericResult<IDataConnection> GetConnection(string dataStore);
    
    /// <summary>
    /// Gets a data connection by configured connection ID
    /// </summary>
    /// <param name="connectionId">The connection identifier from configuration</param>
    /// <returns>Result containing the connection or an error</returns>
    IGenericResult<IDataConnection> GetConnectionById(string connectionId);
    
    /// <summary>
    /// Tests a connection by ID
    /// </summary>
    /// <param name="connectionId">The connection identifier to test</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating if the connection is healthy</returns>
    Task<IGenericResult<bool>> TestConnection(string connectionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all configured connection IDs
    /// </summary>
    /// <returns>List of available connection identifiers</returns>
    IGenericResult<string[]> GetAvailableConnections();
}