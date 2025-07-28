using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Connections;
using FractalDataWorks.Data;
using FractalDataWorks.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections.Data;

/// <summary>
/// Provides data connections based on configuration or provider type
/// </summary>
public class DataServiceProvider : IDataServiceProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DataProviderConfiguration _configuration;
    private readonly ILogger<DataServiceProvider> _logger;
    private readonly Dictionary<string, IDataProviderType> _providerTypes;
    
    public DataServiceProvider(
        IServiceProvider serviceProvider,
        DataProviderConfiguration configuration,
        ILogger<DataServiceProvider> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // TODO: Replace with enhanced enum discovery when available
        _providerTypes = new Dictionary<string, IDataProviderType>();
    }
    
    /// <inheritdoc/>
    public IGenericResult<IDataConnection> GetConnection(string dataStore)
    {
        try
        {
            if (!_providerTypes.TryGetValue(dataStore, out var providerType))
            {
                var error = $"Unknown data store type: {dataStore}";
                _logger.LogError(error);
                return GenericResult<IDataConnection>.Failure(error);
            }
            
            var connection = _serviceProvider.GetService(providerType.ConnectionType) as IDataConnection;
            if (connection == null)
            {
                var error = $"Connection type {providerType.ConnectionType.Name} not registered in DI container";
                _logger.LogError(error);
                return GenericResult<IDataConnection>.Failure(error);
            }
            
            _logger.LogDebug("Retrieved {DataStore} connection", dataStore);
            return GenericResult<IDataConnection>.Success(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection for {DataStore}", dataStore);
            return GenericResult<IDataConnection>.Failure($"Error getting connection: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public IGenericResult<IDataConnection> GetConnectionById(string connectionId)
    {
        try
        {
            if (!_configuration.Connections.TryGetValue(connectionId, out var config))
            {
                var error = $"Connection ID '{connectionId}' not found in configuration";
                _logger.LogError(error);
                return GenericResult<IDataConnection>.Failure(error);
            }
            
            if (!config.Enabled)
            {
                var error = $"Connection '{connectionId}' is disabled";
                _logger.LogWarning(error);
                return GenericResult<IDataConnection>.Failure(error);
            }
            
            var connectionResult = GetConnection(config.DataStore);
            if (connectionResult.IsFailure)
                return connectionResult;
            
            // Apply connection-specific settings if the connection supports it
            if (connectionResult.Value is IConfigurableConnection configurable)
            {
                configurable.ApplySettings(config.Settings);
            }
            
            _logger.LogDebug("Retrieved connection by ID: {ConnectionId}", connectionId);
            return connectionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection by ID: {ConnectionId}", connectionId);
            return GenericResult<IDataConnection>.Failure($"Error getting connection: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public async Task<IGenericResult<bool>> TestConnection(string connectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionResult = GetConnectionById(connectionId);
            if (connectionResult.IsFailure)
                return GenericResult<bool>.Failure(connectionResult.Message);
            
            var testResult = await connectionResult.Value.TestConnection(cancellationToken);
            
            if (testResult.IsSuccess && testResult.Value)
            {
                _logger.LogInformation("Connection test successful for: {ConnectionId}", connectionId);
            }
            else
            {
                _logger.LogWarning("Connection test failed for: {ConnectionId}", connectionId);
            }
            
            return testResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection: {ConnectionId}", connectionId);
            return GenericResult<bool>.Failure($"Error testing connection: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public IGenericResult<string[]> GetAvailableConnections()
    {
        try
        {
            var enabledConnections = _configuration.Connections
                .Where(kvp => kvp.Value.Enabled)
                .Select(kvp => kvp.Key)
                .OrderBy(id => id)
                .ToArray();
            
            _logger.LogDebug("Found {Count} available connections", enabledConnections.Length);
            return GenericResult<string[]>.Success(enabledConnections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available connections");
            return GenericResult<string[]>.Failure($"Error getting connections: {ex.Message}");
        }
    }
}

/// <summary>
/// Interface for connections that can be configured with settings
/// </summary>
public interface IConfigurableConnection
{
    /// <summary>
    /// Applies connection-specific settings
    /// </summary>
    void ApplySettings(Dictionary<string, object> settings);
}