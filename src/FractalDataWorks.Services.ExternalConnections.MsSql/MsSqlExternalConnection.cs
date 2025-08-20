using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
/// Stateless SQL Server implementation of IExternalDataConnection.
/// </summary>
/// <remarks>
/// This implementation provides a clean, stateless interface for SQL Server connectivity.
/// Each Execute call creates its own connection, executes the command, and automatically
/// cleans up resources. All connection lifecycle management is handled internally.
/// </remarks>
public sealed class MsSqlExternalConnection : IExternalDataConnection<MsSqlConfiguration>
{
    private readonly ILogger<MsSqlExternalConnection> _logger;
    private readonly MsSqlConfiguration _configuration;
    private readonly MsSqlCommandTranslator _commandTranslator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlExternalConnection"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The SQL Server configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or configuration is null.</exception>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    public MsSqlExternalConnection(ILogger<MsSqlExternalConnection> logger, MsSqlConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
            
        // Validate configuration upfront - fail fast
        var validationResult = configuration.Validate();
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage).ToArray());
            throw new ArgumentException($"Configuration validation failed: {errorMessage}", nameof(configuration));
        }

        _configuration = configuration;
        ConnectionId = Guid.NewGuid().ToString("N");
        
        // Initialize command translator once
        var translatorLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<MsSqlCommandTranslator>.Instance;
        _commandTranslator = new MsSqlCommandTranslator(_configuration, translatorLogger);
        
        _logger.LogDebug("SQL Server connection {ConnectionId} created with configuration", ConnectionId);
    }

    /// <inheritdoc/>
    public string ConnectionId { get; }

    /// <inheritdoc/>
    public string ProviderName => "MsSql";

    /// <inheritdoc/>
    public FdwConnectionState State => FdwConnectionState.Created; // Stateless design - always ready

    /// <inheritdoc/>
    public string ConnectionString => _configuration.GetSanitizedConnectionString();

    /// <inheritdoc/>
    public MsSqlConfiguration Configuration => _configuration;

    /// <summary>
    /// Executes a data command against the SQL Server database.
    /// </summary>
    /// <param name="command">The data command to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>The execution result.</returns>
    public async Task<IFdwResult> Execute(IDataCommand command, CancellationToken cancellationToken = default)
    {
        var result = await Execute<int>(command, cancellationToken);
        if (result.IsSuccess)
            return FdwResult.Success();
        
        return FdwResult.Failure(result.Message);
    }

    /// <summary>
    /// Executes a data command against the SQL Server database with typed result.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="command">The data command to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>The execution result.</returns>
    public async Task<IFdwResult<T>> Execute<T>(IDataCommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Ensure the command is a DataCommandBase for our translator
        if (command is not DataCommandBase dataCommand)
            throw new ArgumentException($"Expected DataCommandBase, got {command.GetType().Name}", nameof(command));

        try
        {
            _logger.LogDebug("Executing {CommandType} command on SQL Server connection {ConnectionId}", 
                command.CommandType, ConnectionId);

            // Create new connection for this execution - stateless approach
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // Handle transactions based on configuration
            if (_configuration.UseTransactions)
            {
                using var transaction = connection.BeginTransaction(_configuration.TransactionIsolationLevel);
                try
                {
                    var result = await ExecuteWithConnection<T>(connection, transaction, dataCommand, cancellationToken);
                    if (result.IsSuccess)
                    {
                        transaction.Commit();
                        _logger.LogDebug("Transaction committed for {CommandType} command on SQL Server connection {ConnectionId}",
                            command.CommandType, ConnectionId);
                    }
                    else
                    {
                        transaction.Rollback();
                        _logger.LogWarning("Transaction rolled back for {CommandType} command on SQL Server connection {ConnectionId}: {Message}",
                            command.CommandType, ConnectionId, result.Message);
                    }
                    return result;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            else
            {
                return await ExecuteWithConnection<T>(connection, null, dataCommand, cancellationToken);
            }
        }
        catch (Exception ex)
        {
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
        try
        {
            _logger.LogDebug("Discovering schema for SQL Server connection {ConnectionId}", ConnectionId);

            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            var containers = await DiscoverTablesAndViewsAsync(connection, cancellationToken);

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

    /// <inheritdoc/>
    public async Task<IFdwResult<bool>> TestConnection(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Testing SQL Server connection {ConnectionId}", ConnectionId);

            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            
            // Execute a simple test query
            using var command = new SqlCommand("SELECT 1", connection);
            command.CommandTimeout = _configuration.CommandTimeoutSeconds;
            
            var result = await command.ExecuteScalarAsync(cancellationToken);

            _logger.LogInformation("SQL Server connection {ConnectionId} test successful", ConnectionId);
            return FdwResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL Server connection {ConnectionId} test failed", ConnectionId);
            return FdwResult<bool>.Success(false);
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IDictionary<string, object>>> GetConnectionInfo(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving connection info for SQL Server connection {ConnectionId}", ConnectionId);

            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            var metadata = await CollectMetadataAsync(connection, cancellationToken);
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

    /// <summary>
    /// Executes a command with an established SQL connection and optional transaction.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="connection">The SQL connection.</param>
    /// <param name="transaction">Optional SQL transaction.</param>
    /// <param name="dataCommand">The data command to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    [ExcludeFromCodeCoverage(Justification = "Command execution with real SQL connection requires integration testing. Core logic tested via unit tests.")]
    private async Task<IFdwResult<T>> ExecuteWithConnection<T>(SqlConnection connection, SqlTransaction? transaction, DataCommandBase dataCommand, CancellationToken cancellationToken)
    {
        // Translate command to SQL
        var translation = _commandTranslator.Translate(dataCommand);

        using var command = new SqlCommand(translation.Sql, connection, transaction);
        command.CommandTimeout = dataCommand.Timeout?.TotalSeconds > 0 
            ? (int)dataCommand.Timeout.Value.TotalSeconds 
            : _configuration.CommandTimeoutSeconds;

        // Add parameters
        command.Parameters.AddRange(translation.Parameters.ToArray());

        try
        {
            // Execute based on expected result type
            if (typeof(T) == typeof(int))
            {
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                return FdwResult<T>.Success((T)(object)rowsAffected);
            }
            
            if (typeof(T) == typeof(bool))
            {
                var scalar = await command.ExecuteScalarAsync(cancellationToken);
                var boolResult = Convert.ToBoolean(scalar);
                return FdwResult<T>.Success((T)(object)boolResult);
            }

            // For other types, assume it's a collection query
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var results = await MapDataReaderToResults<T>(reader, cancellationToken);
            return FdwResult<T>.Success(results);
        }
        catch (SqlException ex)
        {
            return FdwResult<T>.Failure($"SQL execution failed: {ex.Message} (Error {ex.Number})");
        }
    }

    /// <summary>
    /// Collects metadata about the SQL Server connection and database.
    /// </summary>
    /// <param name="connection">The SQL connection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connection metadata.</returns>
    [ExcludeFromCodeCoverage(Justification = "Metadata collection requires real SQL Server connection for meaningful testing. Tested via integration tests.")]
    private async Task<MsSqlConnectionMetadata> CollectMetadataAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        var capabilities = new Dictionary<string, object>(StringComparer.Ordinal);
        var customProperties = new Dictionary<string, object>(StringComparer.Ordinal);

        // Get server version
        string? version = null;
        string? serverInfo = null;
        string? databaseName = null;

        try
        {
            using var command = new SqlCommand("SELECT @@VERSION, @@SERVERNAME, DB_NAME()", connection);
            command.CommandTimeout = _configuration.CommandTimeoutSeconds;
            
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                version = reader.GetString(0);
                serverInfo = await reader.IsDBNullAsync(1) ? null : reader.GetString(1);
                databaseName = await reader.IsDBNullAsync(2) ? null : reader.GetString(2);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve basic server information");
        }

        // Collect capabilities
        capabilities["SupportsTransactions"] = true;
        capabilities["SupportsMultipleActiveResultSets"] = _configuration.EnableMultipleActiveResultSets;
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
        customProperties["UseTransactions"] = _configuration.UseTransactions;
        customProperties["TransactionIsolationLevel"] = _configuration.TransactionIsolationLevel.ToString();

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

    /// <summary>
    /// Maps SqlDataReader results to the specified type T.
    /// </summary>
    /// <typeparam name="T">The target type to map to.</typeparam>
    /// <param name="reader">The SQL data reader.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mapped result of type T.</returns>
    [ExcludeFromCodeCoverage(Justification = "Complex mapping logic requires real SQL Server connection for meaningful testing. Tested via integration tests.")]
    private async Task<T> MapDataReaderToResults<T>(SqlDataReader reader, CancellationToken cancellationToken)
    {
        // This is a simplified mapping implementation
        // In production, you'd want more sophisticated object mapping
        
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var elementType = typeof(T).GetGenericArguments()[0];
            var results = new List<object>();

            while (await reader.ReadAsync(cancellationToken))
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

            return (T)(object)results;
        }

        // Single result
        if (await reader.ReadAsync(cancellationToken))
        {
            if (typeof(T) == typeof(DataRecord))
            {
                return (T)(object)MapReaderToDataRecord(reader);
            }
            
            var singleInstance = Activator.CreateInstance<T>();
            if (singleInstance != null)
            {
                MapReaderToObject(reader, singleInstance);
            }
            return singleInstance;
        }

        return default!;
    }

    /// <summary>
    /// Maps a SqlDataReader to a DataRecord.
    /// </summary>
    /// <param name="reader">The SQL data reader.</param>
    /// <returns>A DataRecord containing the row data.</returns>
    [ExcludeFromCodeCoverage(Justification = "Data mapping utility method requires real SqlDataReader for meaningful testing. Tested via integration tests.")]
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

    /// <summary>
    /// Maps SqlDataReader columns to object properties using reflection.
    /// </summary>
    /// <param name="reader">The SQL data reader.</param>
    /// <param name="instance">The target object instance.</param>
    [ExcludeFromCodeCoverage(Justification = "Reflection-based mapping utility requires real SqlDataReader for meaningful testing. Tested via integration tests.")]
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
                    value = Convert.ChangeType(value, property.PropertyType, System.Globalization.CultureInfo.InvariantCulture);
                }
                property.SetValue(instance, value);
            }
        }
    }

    private static DatumCategory DetermineColumnCategory(string columnName)
    {
        var lowerName = columnName.ToLowerInvariant();
        
        if (lowerName.EndsWith("id", StringComparison.Ordinal) || string.Equals(lowerName, "key", StringComparison.Ordinal) || lowerName.StartsWith("pk_", StringComparison.Ordinal))
            return DatumCategory.Identifier;
        
        if (lowerName.StartsWith("created", StringComparison.Ordinal) || lowerName.StartsWith("modified", StringComparison.Ordinal) || 
            lowerName.StartsWith("updated", StringComparison.Ordinal) || lowerName.EndsWith("_at", StringComparison.Ordinal) ||
            string.Equals(lowerName, "timestamp", StringComparison.Ordinal) || string.Equals(lowerName, "rowversion", StringComparison.Ordinal))
            return DatumCategory.Metadata;
        
        if (lowerName.Contains("amount") || lowerName.Contains("total") || 
            lowerName.Contains("count") || lowerName.Contains("sum") ||
            lowerName.Contains("price") || lowerName.Contains("cost"))
            return DatumCategory.Measure;
        
        return DatumCategory.Property;
    }

    /// <summary>
    /// Discovers tables and views in the SQL Server database.
    /// </summary>
    /// <param name="connection">The SQL connection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of discovered data containers.</returns>
    [ExcludeFromCodeCoverage(Justification = "Schema discovery requires real SQL Server connection for meaningful testing. Tested via integration tests.")]
    private async Task<IEnumerable<DataContainer>> DiscoverTablesAndViewsAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
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

        using var command = new SqlCommand(sql, connection);
        command.CommandTimeout = _configuration.CommandTimeoutSeconds;
        
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var schemaName = reader.GetString("SchemaName");
            var tableName = reader.GetString("TableName");
            var tableType = reader.GetString("TableType");
            
            var containerPath = new DataPath(new[] { schemaName, tableName });
            var containerType = string.Equals(tableType, "VIEW", StringComparison.Ordinal) ? ContainerType.View : ContainerType.Table;
            var container = new DataContainer(containerPath, tableName, containerType, null, null);
            
            containers.Add(container);
        }

        return containers;
    }

    #region Legacy Interface Methods - Stateless Implementations

    /// <inheritdoc/>
    /// <remarks>In stateless design, this is a no-op since connections are managed per Execute call.</remarks>
    public static Task<IFdwResult> OpenAsync()
    {
        return Task.FromResult(FdwResult.Success());
    }

    Task<IFdwResult> IExternalConnection.OpenAsync() => OpenAsync();

    /// <inheritdoc/>
    /// <remarks>In stateless design, this is a no-op since connections are auto-disposed after use.</remarks>
    public static Task<IFdwResult> CloseAsync()
    {
        return Task.FromResult(FdwResult.Success());
    }

    Task<IFdwResult> IExternalConnection.CloseAsync() => CloseAsync();

    /// <inheritdoc/>
    /// <remarks>Redirects to the stateless TestConnection method.</remarks>
    public async Task<IFdwResult> TestConnectionAsync()
    {
        var testResult = await TestConnection();
        return testResult.IsSuccess 
            ? FdwResult.Success() 
            : FdwResult.Failure(testResult.Message);
    }

    /// <inheritdoc/>
    /// <remarks>Provides connection metadata via stateless call.</remarks>
    public async Task<IFdwResult<IConnectionMetadata>> GetMetadataAsync()
    {
        try
        {
            using var connection = new SqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();
            var metadata = await CollectMetadataAsync(connection, CancellationToken.None);
            return FdwResult<IConnectionMetadata>.Success(metadata);
        }
        catch (Exception ex)
        {
            return FdwResult<IConnectionMetadata>.Failure($"Failed to get metadata: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    /// <remarks>Not needed in stateless design - configuration is validated in constructor.</remarks>
    public static Task<IFdwResult> InitializeAsync(MsSqlConfiguration configuration)
    {
        return Task.FromResult(FdwResult.Success());
    }

    Task<IFdwResult> IExternalConnection<MsSqlConfiguration>.InitializeAsync(MsSqlConfiguration configuration) => InitializeAsync(configuration);

    #endregion

    /// <inheritdoc/>
    public void Dispose()
    {
        // No resources to dispose in stateless design - connections are disposed per Execute call
        // Command translator doesn't need disposal
    }
}