using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Services.DataProvider.MsSql.Commands;
using FractalDataWorks.Services.DataProvider.MsSql.Configuration;

namespace FractalDataWorks.Services.DataProvider.MsSql.Services;

/// <summary>
/// Microsoft SQL Server data provider service implementation using Dapper.
/// </summary>
/// <remarks>
/// This service provides complete data access functionality for SQL Server databases,
/// including transaction support, connection management, retry logic, and comprehensive
/// error handling. It supports all standard data operations (Query, Insert, Update, Delete, Upsert).
/// </remarks>
public sealed class MsSqlDataProvider : DataProvidersServiceBase<FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand, MsSqlConfiguration, MsSqlDataProvider>, IDisposable
{
    private readonly MsSqlConnectionFactory _connectionFactory;
    private readonly Dictionary<string, object> _metadata;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlDataProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The SQL Server configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public MsSqlDataProvider(ILogger<MsSqlDataProvider> logger, MsSqlConfiguration configuration)
        : base(logger, configuration)
    {
        _connectionFactory = new MsSqlConnectionFactory(configuration, logger);
        _metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["ProviderType"] = "SqlServer",
            ["ServerName"] = configuration.ServerName ?? "Unknown",
            ["DatabaseName"] = configuration.DatabaseName ?? "Unknown",
            ["DefaultSchema"] = configuration.DefaultSchema
        };

        Logger.LogInformation("MsSql Data Provider initialized for server: {ServerName}, database: {DatabaseName}",
            configuration.ServerName, configuration.DatabaseName);
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult<TOut>> Execute<TOut>(FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (command == null)
        {
            return FdwResult<TOut>.Failure("Command cannot be null");
        }

        // Validate command
        var validationResult = command.Validate();
        if (validationResult.Error)
        {
            Logger.LogWarning("Command validation failed: {ValidationError}", validationResult.Message);
            return FdwResult<TOut>.Failure(validationResult.Message!);
        }

        var startTime = DateTimeOffset.UtcNow;
        Logger.LogDebug("Executing {CommandType} command with target: {Target}", 
            command.CommandType, command.Target ?? "None");

        try
        {
            return await ExecuteWithRetry<TOut>(command, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            Logger.LogError(ex, "Command execution failed after {Duration}ms: {CommandType}", 
                duration.TotalMilliseconds, command.CommandType);
            
            return FdwResult<TOut>.Failure($"Command execution failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult> Execute(FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command, CancellationToken cancellationToken)
    {
        var result = await Execute<object>(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? FdwResult.Success() : FdwResult.Failure(result.Message!);
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command)
    {
        return await Execute<T>(command, CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new transaction for atomic operations.
    /// </summary>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <param name="timeout">The transaction timeout.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A new transaction instance.</returns>
    public async Task<IFdwResult<FractalDataWorks.Services.DataProvider.Abstractions.IDataTransaction>> BeginTransactionAsync(
        FdwTransactionIsolationLevel isolationLevel = FdwTransactionIsolationLevel.Default,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            Logger.LogDebug("Creating new transaction with isolation level: {IsolationLevel}", isolationLevel);

            var connectionResult = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            if (connectionResult.Error)
            {
                return FdwResult<IDataTransaction>.Failure(connectionResult.Message!);
            }

            var connection = connectionResult.Value!;
            var transactionLogger = Logger as ILogger<MsSqlTransactionManager> ?? 
                throw new InvalidOperationException("Unable to create transaction logger");

            var transaction = new MsSqlTransactionManager(connection, this, transactionLogger, isolationLevel, timeout);
            
            Logger.LogDebug("Transaction created successfully with ID: {TransactionId}", transaction.TransactionId);
            
            return FdwResult<FractalDataWorks.Services.DataProvider.Abstractions.IDataTransaction>.Success(transaction);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create transaction with isolation level: {IsolationLevel}", isolationLevel);
            return FdwResult<FractalDataWorks.Services.DataProvider.Abstractions.IDataTransaction>.Failure($"Transaction creation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Tests the database connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection test result.</returns>
    public async Task<IFdwResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return await _connectionFactory.TestConnectionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets connection information for diagnostic purposes.
    /// </summary>
    /// <returns>Connection information.</returns>
    public IFdwResult<ConnectionInfo> GetConnectionInfo()
    {
        ThrowIfDisposed();
        return _connectionFactory.GetConnectionInfo();
    }

    /// <summary>
    /// Executes a command with retry logic for transient failures.
    /// </summary>
    /// <typeparam name="TOut">The result type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The command execution result.</returns>
    private async Task<IFdwResult<TOut>> ExecuteWithRetry<TOut>(FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command, CancellationToken cancellationToken)
    {
        var maxRetries = Configuration.EnableAutoRetry ? (Configuration.RetryPolicy?.MaxRetries ?? 3) : 0;
        var currentAttempt = 0;

        while (currentAttempt <= maxRetries)
        {
            try
            {
                return await ExecuteCommand<TOut>(command, cancellationToken).ConfigureAwait(false);
            }
            catch (SqlException sqlEx) when (currentAttempt < maxRetries && MsSqlConnectionFactory.IsTransientError(sqlEx))
            {
                currentAttempt++;
                var delay = CalculateRetryDelay(currentAttempt);
                
                Logger.LogWarning("Transient error occurred (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}ms. Error: {Error}",
                    currentAttempt, maxRetries + 1, delay.TotalMilliseconds, sqlEx.Message);
                
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        // This shouldn't be reached, but included for completeness
        return FdwResult<TOut>.Failure("Maximum retry attempts exceeded");
    }

    /// <summary>
    /// Executes a command against the database.
    /// </summary>
    /// <typeparam name="TOut">The result type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The command execution result.</returns>
    private async Task<IFdwResult<TOut>> ExecuteCommand<TOut>(FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command, CancellationToken cancellationToken)
    {
        using var connectionResult = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        if (connectionResult.Error)
        {
            return FdwResult<TOut>.Failure(connectionResult.Message!);
        }

        var connection = connectionResult.Value!;

        try
        {
            return command.CommandType switch
            {
                "Query" => await ExecuteQuery<TOut>(connection, command, cancellationToken).ConfigureAwait(false),
                "Insert" => await ExecuteInsert<TOut>(connection, command, cancellationToken).ConfigureAwait(false),
                "Update" => await ExecuteUpdate<TOut>(connection, command, cancellationToken).ConfigureAwait(false),
                "Delete" => await ExecuteDelete<TOut>(connection, command, cancellationToken).ConfigureAwait(false),
                "Upsert" => await ExecuteUpsert<TOut>(connection, command, cancellationToken).ConfigureAwait(false),
                _ => FdwResult<TOut>.Failure($"Unsupported command type: {command.CommandType}")
            };
        }
        finally
        {
            connection?.Dispose();
        }
    }

    /// <summary>
    /// Executes a query command.
    /// </summary>
    private async Task<IFdwResult<TOut>> ExecuteQuery<TOut>(SqlConnection connection, FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command, CancellationToken cancellationToken)
    {
        if (command is not MsSqlCommandBase sqlCommand)
        {
            return FdwResult<TOut>.Failure("Command must be a SQL command");
        }

        try
        {
            var sql = ApplySchemaMapping(sqlCommand);
            var parameters = ConvertParameters(command.Parameters);
            var timeout = command.Timeout?.TotalSeconds ?? Configuration.CommandTimeoutSeconds;

            Logger.LogDebug("Executing query: {Sql}", sql);

            if (typeof(TOut) == typeof(int) || typeof(TOut) == typeof(long))
            {
                // Scalar query
                var result = await connection.QuerySingleOrDefaultAsync<TOut>(sql, parameters, commandTimeout: (int)timeout)
                    .ConfigureAwait(false);
                return FdwResult<TOut>.Success(result);
            }
            else if (typeof(TOut).IsGenericType && typeof(TOut).GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                // Multiple results
                var elementType = typeof(TOut).GetGenericArguments()[0];
                var results = await connection.QueryAsync(elementType, sql, parameters, commandTimeout: (int)timeout)
                    .ConfigureAwait(false);
                
                if (results is TOut typedResults)
                {
                    return FdwResult<TOut>.Success(typedResults);
                }
                
                return FdwResult<TOut>.Failure("Type conversion failed for query results");
            }
            else
            {
                // Single result
                var result = await connection.QuerySingleOrDefaultAsync<TOut>(sql, parameters, commandTimeout: (int)timeout)
                    .ConfigureAwait(false);
                return FdwResult<TOut>.Success(result);
            }
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "SQL query execution failed");
            return FdwResult<TOut>.Failure($"Query execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    private async Task<IFdwResult<TOut>> ExecuteInsert<TOut>(SqlConnection connection, FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command, CancellationToken cancellationToken)
    {
        if (command is not MsSqlCommandBase sqlCommand)
        {
            return FdwResult<TOut>.Failure("Command must be a SQL command");
        }

        try
        {
            var sql = ApplySchemaMapping(sqlCommand);
            var parameters = ConvertParameters(command.Parameters);
            var timeout = command.Timeout?.TotalSeconds ?? Configuration.CommandTimeoutSeconds;

            Logger.LogDebug("Executing insert: {Sql}", sql);

            var rowsAffected = await connection.ExecuteAsync(sql, parameters, commandTimeout: (int)timeout)
                .ConfigureAwait(false);

            if (typeof(TOut) == typeof(int))
            {
                return FdwResult<TOut>.Success((TOut)(object)rowsAffected);
            }

            return FdwResult<TOut>.Success(default(TOut)!);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "SQL insert execution failed");
            return FdwResult<TOut>.Failure($"Insert execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes an update command.
    /// </summary>
    private async Task<IFdwResult<TOut>> ExecuteUpdate<TOut>(SqlConnection connection, FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command, CancellationToken cancellationToken)
    {
        if (command is not MsSqlCommandBase sqlCommand)
        {
            return FdwResult<TOut>.Failure("Command must be a SQL command");
        }

        try
        {
            var sql = ApplySchemaMapping(sqlCommand);
            var parameters = ConvertParameters(command.Parameters);
            var timeout = command.Timeout?.TotalSeconds ?? Configuration.CommandTimeoutSeconds;

            Logger.LogDebug("Executing update: {Sql}", sql);

            var rowsAffected = await connection.ExecuteAsync(sql, parameters, commandTimeout: (int)timeout)
                .ConfigureAwait(false);

            if (typeof(TOut) == typeof(int))
            {
                return FdwResult<TOut>.Success((TOut)(object)rowsAffected);
            }

            return FdwResult<TOut>.Success(default(TOut)!);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "SQL update execution failed");
            return FdwResult<TOut>.Failure($"Update execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a delete command.
    /// </summary>
    private async Task<IFdwResult<TOut>> ExecuteDelete<TOut>(SqlConnection connection, FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command, CancellationToken cancellationToken)
    {
        if (command is not MsSqlCommandBase sqlCommand)
        {
            return FdwResult<TOut>.Failure("Command must be a SQL command");
        }

        try
        {
            var sql = ApplySchemaMapping(sqlCommand);
            var parameters = ConvertParameters(command.Parameters);
            var timeout = command.Timeout?.TotalSeconds ?? Configuration.CommandTimeoutSeconds;

            Logger.LogDebug("Executing delete: {Sql}", sql);

            var rowsAffected = await connection.ExecuteAsync(sql, parameters, commandTimeout: (int)timeout)
                .ConfigureAwait(false);

            if (typeof(TOut) == typeof(int))
            {
                return FdwResult<TOut>.Success((TOut)(object)rowsAffected);
            }

            return FdwResult<TOut>.Success(default(TOut)!);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "SQL delete execution failed");
            return FdwResult<TOut>.Failure($"Delete execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes an upsert command.
    /// </summary>
    private async Task<IFdwResult<TOut>> ExecuteUpsert<TOut>(SqlConnection connection, FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command, CancellationToken cancellationToken)
    {
        if (command is not MsSqlCommandBase sqlCommand)
        {
            return FdwResult<TOut>.Failure("Command must be a SQL command");
        }

        try
        {
            var sql = ApplySchemaMapping(sqlCommand);
            var parameters = ConvertParameters(command.Parameters);
            var timeout = command.Timeout?.TotalSeconds ?? Configuration.CommandTimeoutSeconds;

            Logger.LogDebug("Executing upsert: {Sql}", sql);

            var result = await connection.QuerySingleOrDefaultAsync<int>(sql, parameters, commandTimeout: (int)timeout)
                .ConfigureAwait(false);

            if (typeof(TOut) == typeof(int))
            {
                return FdwResult<TOut>.Success((TOut)(object)result);
            }

            return FdwResult<TOut>.Success(default(TOut)!);
        }
        catch (SqlException ex)
        {
            Logger.LogError(ex, "SQL upsert execution failed");
            return FdwResult<TOut>.Failure($"Upsert execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Applies schema mapping to the SQL command.
    /// </summary>
    /// <param name="command">The SQL command.</param>
    /// <returns>The SQL text with schema mapping applied.</returns>
    private string ApplySchemaMapping(MsSqlCommandBase command)
    {
        var sql = command.GetExecutableSql();
        
        // Apply global schema mapping from configuration
        foreach (var mapping in Configuration.SchemaMapping)
        {
            sql = sql.Replace($"{{{mapping.Key}}}", mapping.Value, StringComparison.OrdinalIgnoreCase);
        }

        return sql;
    }

    /// <summary>
    /// Converts command parameters to Dapper-compatible format.
    /// </summary>
    /// <param name="parameters">The command parameters.</param>
    /// <returns>Dapper parameters object.</returns>
    private static object? ConvertParameters(IReadOnlyDictionary<string, object?> parameters)
    {
        if (parameters?.Count > 0)
        {
            return new DynamicParameters(parameters);
        }
        
        return null;
    }

    /// <summary>
    /// Calculates the retry delay based on the attempt number.
    /// </summary>
    /// <param name="attempt">The current attempt number.</param>
    /// <returns>The delay before the next retry.</returns>
    private TimeSpan CalculateRetryDelay(int attempt)
    {
        var retryPolicy = Configuration.RetryPolicy;
        if (retryPolicy == null)
        {
            return TimeSpan.FromSeconds(1);
        }
        
        var baseDelayMs = retryPolicy.RetryDelayMs;
        var maxDelayMs = retryPolicy.MaxRetryDelayMs;
        
        var delayMs = retryPolicy.UseExponentialBackoff
            ? baseDelayMs * Math.Pow(2, attempt - 1)
            : baseDelayMs;
        
        if (delayMs > maxDelayMs)
        {
            delayMs = maxDelayMs;
        }

        // Add some randomness to prevent thundering herd
        var jitter = Random.Shared.Next(0, (int)(delayMs * 0.1));
        return TimeSpan.FromMilliseconds(delayMs + jitter);
    }

    /// <summary>
    /// Throws an exception if the service has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MsSqlDataProvider));
        }
    }

    /// <summary>
    /// Disposes the service and releases any resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _connectionFactory?.Dispose();
        Logger.LogDebug("MsSql Data Provider disposed");

        _disposed = true;
    }
}