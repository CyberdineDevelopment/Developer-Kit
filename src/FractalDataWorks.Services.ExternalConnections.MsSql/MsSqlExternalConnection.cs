using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.ExternalConnections.MsSql;

/// <summary>
/// SQL Server implementation of IExternalConnection.
/// </summary>
/// <remarks>
/// This implementation provides connectivity to Microsoft SQL Server databases,
/// including command execution, schema discovery, and transaction support.
/// It translates universal DataCommandBase instances to SQL Server-specific SQL statements.
/// </remarks>
public sealed class MsSqlExternalConnection : IExternalDataConnection<MsSqlConfiguration>, IDisposable
{
    private readonly ILogger<MsSqlExternalConnection> _logger;
    private MsSqlCommandTranslator? _commandTranslator;
    private SqlConnection? _connection;
    private MsSqlConfiguration? _configuration;
    private FdwConnectionState _state = FdwConnectionState.Created;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlExternalConnection"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public MsSqlExternalConnection(ILogger<MsSqlExternalConnection> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ConnectionId = Guid.NewGuid().ToString("N");
    }

    /// <inheritdoc/>
    public string ConnectionId { get; }

    /// <inheritdoc/>
    public string ProviderName => "MsSql";

    /// <inheritdoc/>
    public FdwConnectionState State => _state;

    /// <inheritdoc/>
    public string ConnectionString => _configuration?.GetSanitizedConnectionString() ?? "(not initialized)";

    /// <inheritdoc/>
    public MsSqlConfiguration Configuration => 
        _configuration ?? throw new InvalidOperationException("Connection has not been initialized.");

    /// <inheritdoc/>
    public async Task<IFdwResult> InitializeAsync(MsSqlConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (_state != FdwConnectionState.Created)
            throw new InvalidOperationException($"Connection cannot be initialized in state {_state}.");

        try
        {
            _logger.LogDebug("Initializing SQL Server connection {ConnectionId}", ConnectionId);

            // Validate configuration
            var validationResult = await configuration.Validate().ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return FdwResult.Failure($"Configuration validation failed: {errorMessage}");
            }

            _configuration = configuration;
            _commandTranslator = new MsSqlCommandTranslator(_configuration, _logger);

            // Create connection but don't open it yet
            _connection = new SqlConnection(_configuration.ConnectionString);

            _logger.LogInformation("SQL Server connection {ConnectionId} initialized successfully", ConnectionId);
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SQL Server connection {ConnectionId}", ConnectionId);
            _state = FdwConnectionState.Broken;
            return FdwResult.Failure($"Initialization failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> OpenAsync()
    {
        ThrowIfDisposed();
        
        if (_connection == null || _configuration == null)
            throw new InvalidOperationException("Connection has not been initialized.");

        if (_state == FdwConnectionState.Open)
            return FdwResult.Success();

        if (_state != FdwConnectionState.Created && _state != FdwConnectionState.Closed)
            throw new InvalidOperationException($"Connection cannot be opened in state {_state}.");

        try
        {
            _state = FdwConnectionState.Opening;
            _logger.LogDebug("Opening SQL Server connection {ConnectionId}", ConnectionId);

            await _connection.OpenAsync().ConfigureAwait(false);

            _state = FdwConnectionState.Open;
            _logger.LogInformation("SQL Server connection {ConnectionId} opened successfully", ConnectionId);
            
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open SQL Server connection {ConnectionId}", ConnectionId);
            _state = FdwConnectionState.Broken;
            return FdwResult.Failure($"Failed to open connection: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> CloseAsync()
    {
        ThrowIfDisposed();

        if (_connection == null)
            return FdwResult.Success(); // Already effectively closed

        if (_state == FdwConnectionState.Closed || _state == FdwConnectionState.Disposed)
            return FdwResult.Success();

        try
        {
            _state = FdwConnectionState.Closing;
            _logger.LogDebug("Closing SQL Server connection {ConnectionId}", ConnectionId);

            await _connection.CloseAsync().ConfigureAwait(false);

            _state = FdwConnectionState.Closed;
            _logger.LogInformation("SQL Server connection {ConnectionId} closed successfully", ConnectionId);
            
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close SQL Server connection {ConnectionId}", ConnectionId);
            _state = FdwConnectionState.Broken;
            return FdwResult.Failure($"Failed to close connection: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> TestConnectionAsync()
    {
        ThrowIfDisposed();

        if (_connection == null || _configuration == null)
            throw new InvalidOperationException("Connection has not been initialized.");

        try
        {
            _logger.LogDebug("Testing SQL Server connection {ConnectionId}", ConnectionId);

            using var testConnection = new SqlConnection(_configuration.ConnectionString);
            testConnection.ConnectionTimeout = _configuration.ConnectionTimeoutSeconds;
            
            await testConnection.OpenAsync().ConfigureAwait(false);
            
            // Execute a simple test query
            using var command = new SqlCommand("SELECT 1", testConnection);
            command.CommandTimeout = _configuration.CommandTimeoutSeconds;
            
            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
            
            await testConnection.CloseAsync().ConfigureAwait(false);

            _logger.LogInformation("SQL Server connection {ConnectionId} test successful", ConnectionId);
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL Server connection {ConnectionId} test failed", ConnectionId);
            return FdwResult.Failure($"Connection test failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IConnectionMetadata>> GetMetadataAsync()
    {
        ThrowIfDisposed();

        if (_connection == null || _configuration == null)
            throw new InvalidOperationException("Connection has not been initialized.");

        try
        {
            _logger.LogDebug("Retrieving metadata for SQL Server connection {ConnectionId}", ConnectionId);

            // Ensure connection is open
            var openResult = await OpenAsync().ConfigureAwait(false);
            if (!openResult.IsSuccess)
                return FdwResult<IConnectionMetadata>.Failure(openResult.Message);

            var metadata = await CollectMetadataAsync().ConfigureAwait(false);
            
            _logger.LogInformation("Successfully retrieved metadata for SQL Server connection {ConnectionId}", ConnectionId);
            return FdwResult<IConnectionMetadata>.Success(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve metadata for SQL Server connection {ConnectionId}", ConnectionId);
            return FdwResult<IConnectionMetadata>.Failure($"Failed to retrieve metadata: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a DataCommandBase against the SQL Server database.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>The execution result.</returns>
    public async Task<IFdwResult<T>> Execute<T>(IDataCommand command, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (command == null)
            throw new ArgumentNullException(nameof(command));

        if (_connection == null || _configuration == null || _commandTranslator == null)
            throw new InvalidOperationException("Connection has not been initialized.");

        // Ensure the command is a DataCommandBase for our translator
        if (command is not DataCommandBase dataCommand)
            throw new ArgumentException($"Expected DataCommandBase, got {command.GetType().Name}", nameof(command));

        try
        {
            _logger.LogDebug("Executing {CommandType} command on SQL Server connection {ConnectionId}", 
                command.CommandType, ConnectionId);

            // Ensure connection is open
            var openResult = await OpenAsync().ConfigureAwait(false);
            if (!openResult.IsSuccess)
                return FdwResult<T>.Failure(openResult.Message);

            _state = FdwConnectionState.Executing;

            // Translate command to SQL
            var translation = _commandTranslator.Translate(dataCommand);

            // Execute the SQL
            var result = await ExecuteSql<T>(translation, dataCommand, cancellationToken).ConfigureAwait(false);

            _state = FdwConnectionState.Open;
            
            _logger.LogDebug("Successfully executed {CommandType} command on SQL Server connection {ConnectionId}",
                command.CommandType, ConnectionId);

            return result;
        }
        catch (Exception ex)
        {
            _state = FdwConnectionState.Open; // Reset state
            _logger.LogError(ex, "Failed to execute {CommandType} command on SQL Server connection {ConnectionId}",
                command.CommandType, ConnectionId);
            return FdwResult<T>.Failure($"Command execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Discovers the schema structure starting from an optional path.
    /// </summary>
    /// <param name="startPath">Optional starting path for schema discovery.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>A collection of DataContainer objects representing the database schema.</returns>
    public async Task<IFdwResult<IEnumerable<DataContainer>>> DiscoverSchema(DataPath? startPath = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_connection == null || _configuration == null)
            throw new InvalidOperationException("Connection has not been initialized.");

        try
        {
            _logger.LogDebug("Discovering schema for SQL Server connection {ConnectionId}", ConnectionId);

            // Ensure connection is open
            var openResult = await OpenAsync().ConfigureAwait(false);
            if (!openResult.IsSuccess)
                return FdwResult<IEnumerable<DataContainer>>.Failure(openResult.Message);

            var containers = await DiscoverTablesAndViewsAsync().ConfigureAwait(false);

            _logger.LogInformation("Successfully discovered {ContainerCount} containers for SQL Server connection {ConnectionId}",
                containers.Count(), ConnectionId);

            return FdwResult<IEnumerable<DataContainer>>.Success(containers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover schema for SQL Server connection {ConnectionId}", ConnectionId);
            return FdwResult<IEnumerable<DataContainer>>.Failure($"Schema discovery failed: {ex.Message}");
        }
    }

    private async Task<MsSqlConnectionMetadata> CollectMetadataAsync()
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection is null.");

        var capabilities = new Dictionary<string, object>(StringComparer.Ordinal);
        var customProperties = new Dictionary<string, object>(StringComparer.Ordinal);

        // Get server version
        string? version = null;
        string? serverInfo = null;
        string? databaseName = null;

        try
        {
            using var command = new SqlCommand("SELECT @@VERSION, @@SERVERNAME, DB_NAME()", _connection);
            command.CommandTimeout = _configuration!.CommandTimeoutSeconds;
            
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                version = reader.GetString(0);
                serverInfo = reader.IsDBNull(1) ? null : reader.GetString(1);
                databaseName = reader.IsDBNull(2) ? null : reader.GetString(2);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve basic server information");
        }

        // Collect capabilities
        capabilities["SupportsTransactions"] = true;
        capabilities["SupportsMultipleActiveResultSets"] = _configuration!.EnableMultipleActiveResultSets;
        capabilities["MaxParameterCount"] = 2100; // SQL Server limit
        capabilities["MaxBatchSize"] = 1000; // Recommended batch size
        capabilities["SupportsJsonData"] = true;
        capabilities["SupportsXmlData"] = true;
        capabilities["SupportsFullTextSearch"] = true;

        // Add custom properties
        customProperties["ConnectionPooling"] = _configuration.EnableConnectionPooling;
        customProperties["RetryLogic"] = _configuration.EnableRetryLogic;
        customProperties["CommandTimeout"] = _configuration.CommandTimeoutSeconds;
        customProperties["ConnectionTimeout"] = _configuration.ConnectionTimeoutSeconds;

        return new MsSqlConnectionMetadata
        {
            SystemName = "Microsoft SQL Server",
            Version = version,
            ServerInfo = serverInfo,
            DatabaseName = databaseName,
            Capabilities = capabilities,
            CustomProperties = customProperties,
            CollectedAt = DateTimeOffset.UtcNow
        };
    }

    private async Task<IFdwResult<TResult>> ExecuteSql<TResult>(SqlTranslationResult translation, DataCommandBase originalCommand, CancellationToken cancellationToken)
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection is null.");

        using var command = new SqlCommand(translation.Sql, _connection);
        command.CommandTimeout = originalCommand.Timeout?.TotalSeconds > 0 
            ? (int)originalCommand.Timeout.Value.TotalSeconds 
            : _configuration!.CommandTimeoutSeconds;

        // Add parameters
        command.Parameters.AddRange(translation.Parameters.ToArray());

        try
        {
            // Execute based on expected result type
            if (typeof(TResult) == typeof(int))
            {
                var rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                return FdwResult<TResult>.Success((TResult)(object)rowsAffected);
            }
            
            if (typeof(TResult) == typeof(bool))
            {
                var scalar = await command.ExecuteScalarAsync().ConfigureAwait(false);
                var boolResult = Convert.ToBoolean(scalar);
                return FdwResult<TResult>.Success((TResult)(object)boolResult);
            }

            // For other types, assume it's a collection query
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            var results = await MapDataReaderToResults<TResult>(reader).ConfigureAwait(false);
            return FdwResult<TResult>.Success(results);
        }
        catch (SqlException ex)
        {
            return FdwResult<TResult>.Failure($"SQL execution failed: {ex.Message} (Error {ex.Number})");
        }
    }

    private async Task<TResult> MapDataReaderToResults<TResult>(SqlDataReader reader)
    {
        // This is a simplified mapping implementation
        // In production, you'd want more sophisticated object mapping
        
        if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var elementType = typeof(TResult).GetGenericArguments()[0];
            var results = new List<object>();

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                if (elementType == typeof(DataRecord))
                {
                    results.Add(MapReaderToDataRecord(reader));
                }
                else
                {
                    // Simple object mapping
                    var instance = Activator.CreateInstance(elementType);
                    if (instance != null)
                    {
                        MapReaderToObject(reader, instance);
                        results.Add(instance);
                    }
                }
            }

            return (TResult)results;
        }

        // Single result
        if (await reader.ReadAsync().ConfigureAwait(false))
        {
            if (typeof(TResult) == typeof(DataRecord))
            {
                return (TResult)(object)MapReaderToDataRecord(reader);
            }
            
            var singleInstance = Activator.CreateInstance<TResult>();
            if (singleInstance != null)
            {
                MapReaderToObject(reader, singleInstance);
            }
            return singleInstance;
        }

        return default(TResult)!;
    }

    private static DataRecord MapReaderToDataRecord(SqlDataReader reader)
    {
        var data = new List<Datum>();
        
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
            var type = reader.GetFieldType(i);
            
            // Determine category based on naming conventions
            var category = DetermineColumnCategory(name);
            
            data.Add(new Datum(name, category, type, value));
        }

        return new DataRecord(data);
    }

    private static void MapReaderToObject(SqlDataReader reader, object instance)
    {
        var type = instance.GetType();
        var properties = type.GetProperties();

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            var property = properties.FirstOrDefault(p => 
                string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));
            
            if (property != null && property.CanWrite && !reader.IsDBNull(i))
            {
                var value = reader.GetValue(i);
                if (value != null && property.PropertyType != value.GetType())
                {
                    value = Convert.ChangeType(value, property.PropertyType);
                }
                property.SetValue(instance, value);
            }
        }
    }

    private static DatumCategory DetermineColumnCategory(string columnName)
    {
        var lowerName = columnName.ToLowerInvariant();
        
        if (lowerName.EndsWith("id") || lowerName == "key" || lowerName.StartsWith("pk_"))
            return DatumCategory.Identifier;
        
        if (lowerName.StartsWith("created") || lowerName.StartsWith("modified") || 
            lowerName.StartsWith("updated") || lowerName.EndsWith("_at") ||
            lowerName == "timestamp" || lowerName == "rowversion")
            return DatumCategory.Metadata;
        
        if (lowerName.Contains("amount") || lowerName.Contains("total") || 
            lowerName.Contains("count") || lowerName.Contains("sum") ||
            lowerName.Contains("price") || lowerName.Contains("cost"))
            return DatumCategory.Measure;
        
        return DatumCategory.Property;
    }

    private async Task<IEnumerable<DataContainer>> DiscoverTablesAndViewsAsync()
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection is null.");

        var containers = new List<DataContainer>();

        const string sql = @"
            SELECT 
                s.name AS SchemaName,
                t.name AS TableName,
                t.type_desc AS TableType
            FROM sys.tables t
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            UNION ALL
            SELECT 
                s.name AS SchemaName,
                v.name AS TableName,
                'VIEW' AS TableType
            FROM sys.views v
            INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
            ORDER BY SchemaName, TableName";

        using var command = new SqlCommand(sql, _connection);
        command.CommandTimeout = _configuration!.CommandTimeoutSeconds;
        
        using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var schemaName = reader.GetString("SchemaName");
            var tableName = reader.GetString("TableName");
            var tableType = reader.GetString("TableType");
            
            var containerPath = new DataPath(new[] { schemaName, tableName });
            var container = new DataContainer(containerPath, tableType == "VIEW" ? "View" : "Table");
            
            containers.Add(container);
        }

        return containers;
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<bool>> TestConnection(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await TestConnectionAsync().ConfigureAwait(false);
            return FdwResult<bool>.Success(result.IsSuccess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test connection failed for SQL Server connection {ConnectionId}", ConnectionId);
            return FdwResult<bool>.Failure($"Test connection failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IDictionary<string, object>>> GetConnectionInfo(CancellationToken cancellationToken = default)
    {
        try
        {
            var metadataResult = await GetMetadataAsync().ConfigureAwait(false);
            if (!metadataResult.IsSuccess || metadataResult.Value == null)
                return FdwResult<IDictionary<string, object>>.Failure(metadataResult.Message);

            var metadata = metadataResult.Value;
            var connectionInfo = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["SystemName"] = metadata.SystemName,
                ["Version"] = metadata.Version ?? "Unknown",
                ["ServerInfo"] = metadata.ServerInfo ?? "Unknown",
                ["DatabaseName"] = metadata.DatabaseName ?? "Unknown",
                ["CollectedAt"] = metadata.CollectedAt
            };

            // Add capabilities
            foreach (var capability in metadata.Capabilities)
            {
                connectionInfo[$"Capability_{capability.Key}"] = capability.Value;
            }

            // Add custom properties
            foreach (var property in metadata.CustomProperties)
            {
                connectionInfo[$"Property_{property.Key}"] = property.Value;
            }

            return FdwResult<IDictionary<string, object>>.Success(connectionInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get connection info failed for SQL Server connection {ConnectionId}", ConnectionId);
            return FdwResult<IDictionary<string, object>>.Failure($"Get connection info failed: {ex.Message}");
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MsSqlExternalConnection));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            _state = FdwConnectionState.Disposed;
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception occurred while disposing SQL Server connection {ConnectionId}", ConnectionId);
        }
        finally
        {
            _disposed = true;
        }
    }
}