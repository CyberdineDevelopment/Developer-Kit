using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataProvider.MsSql.Configuration;

namespace FractalDataWorks.Services.DataProvider.MsSql.Services;

/// <summary>
/// Factory for creating and managing Microsoft SQL Server database connections.
/// </summary>
/// <remarks>
/// This factory handles connection creation, connection string building, and connection pooling
/// for SQL Server databases. It provides proper disposal patterns and connection management.
/// </remarks>
public sealed class MsSqlConnectionFactory : IDisposable
{
    private readonly MsSqlConfiguration _configuration;
    private readonly ILogger<MsSqlConnectionFactory> _logger;
    private readonly string _connectionString;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlConnectionFactory"/> class.
    /// </summary>
    /// <param name="configuration">The SQL Server configuration.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public MsSqlConnectionFactory(MsSqlConfiguration configuration, ILogger<MsSqlConnectionFactory> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Validate configuration and build connection string
        if (!_configuration.Validate())
        {
            throw new ArgumentException("Invalid SQL Server configuration provided.", nameof(configuration));
        }

        _connectionString = _configuration.BuildConnectionString();
        
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(configuration));
        }

        _logger.LogDebug("MsSqlConnectionFactory initialized with connection timeout: {ConnectionTimeout}s, command timeout: {CommandTimeout}s",
            _configuration.ConnectionTimeoutSeconds, _configuration.CommandTimeoutSeconds);
    }

    /// <summary>
    /// Creates a new SQL Server connection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation containing the connection result.</returns>
    public async Task<IFdwResult<SqlConnection>> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            _logger.LogDebug("Creating new SQL Server connection");
            
            var connection = new SqlConnection(_connectionString);
            
            // Open the connection
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            _logger.LogDebug("SQL Server connection opened successfully. Connection state: {State}", connection.State);
            
            return FdwResult<SqlConnection>.Success(connection);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to create SQL Server connection. Error number: {ErrorNumber}, severity: {Severity}, state: {State}",
                ex.Number, ex.Class, ex.State);
            
            return FdwResult<SqlConnection>.Failure($"SQL Server connection failed: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while creating SQL Server connection");
            return FdwResult<SqlConnection>.Failure($"Invalid connection operation: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating SQL Server connection");
            return FdwResult<SqlConnection>.Failure($"Connection creation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new SQL Server connection synchronously.
    /// </summary>
    /// <returns>The connection creation result.</returns>
    public IFdwResult<SqlConnection> CreateConnection()
    {
        ThrowIfDisposed();

        try
        {
            _logger.LogDebug("Creating new SQL Server connection (synchronous)");
            
            var connection = new SqlConnection(_connectionString);
            
            // Open the connection
            connection.Open();
            
            _logger.LogDebug("SQL Server connection opened successfully. Connection state: {State}", connection.State);
            
            return FdwResult<SqlConnection>.Success(connection);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to create SQL Server connection. Error number: {ErrorNumber}, severity: {Severity}, state: {State}",
                ex.Number, ex.Class, ex.State);
            
            return FdwResult<SqlConnection>.Failure($"SQL Server connection failed: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while creating SQL Server connection");
            return FdwResult<SqlConnection>.Failure($"Invalid connection operation: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating SQL Server connection");
            return FdwResult<SqlConnection>.Failure($"Connection creation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Tests the database connection without returning the connection instance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    public async Task<IFdwResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            _logger.LogDebug("Testing SQL Server connection");
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            _logger.LogDebug("SQL Server connection test successful");
            return FdwResult.Success();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL Server connection test failed. Error number: {ErrorNumber}, severity: {Severity}, state: {State}",
                ex.Number, ex.Class, ex.State);
            
            return FdwResult.Failure($"Connection test failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during SQL Server connection test");
            return FdwResult.Failure($"Connection test failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets connection information for diagnostic purposes.
    /// </summary>
    /// <returns>A dictionary containing connection information.</returns>
    public IFdwResult<ConnectionInfo> GetConnectionInfo()
    {
        ThrowIfDisposed();

        try
        {
            var connectionInfo = new ConnectionInfo
            {
                ServerName = _configuration.ServerName,
                DatabaseName = _configuration.DatabaseName,
                ProviderType = _configuration.ProviderType,
                ConnectionTimeoutSeconds = _configuration.ConnectionTimeoutSeconds,
                CommandTimeoutSeconds = _configuration.CommandTimeoutSeconds,
                EnableConnectionPooling = _configuration.EnableConnectionPooling,
                MaxPoolSize = _configuration.MaxPoolSize,
                EnableAutoRetry = _configuration.EnableAutoRetry,
                UseWindowsAuthentication = _configuration.UseWindowsAuthentication,
                EncryptConnection = _configuration.EncryptConnection,
                TrustServerCertificate = _configuration.TrustServerCertificate,
                ApplicationName = _configuration.ApplicationName,
                EnableMars = _configuration.EnableMars
            };

            return FdwResult<ConnectionInfo>.Success(connectionInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connection information");
            return FdwResult<ConnectionInfo>.Failure($"Failed to get connection info: {ex.Message}");
        }
    }

    /// <summary>
    /// Determines if a SQL exception represents a transient error that can be retried.
    /// </summary>
    /// <param name="sqlException">The SQL exception to check.</param>
    /// <returns>True if the error is transient; otherwise, false.</returns>
    public static bool IsTransientError(SqlException sqlException)
    {
        if (sqlException == null)
            return false;

        // Common transient error numbers
        return sqlException.Number switch
        {
            // Timeout expired
            -2 => true,
            // Connection timeout
            2 => true,
            // Database unavailable
            40197 => true,
            // Database busy
            40501 => true,
            // Service temporarily unavailable
            40613 => true,
            // Database temporarily unavailable
            49918 => true,
            // Database unavailable
            49919 => true,
            // Database unavailable
            49920 => true,
            // Cannot open database
            4060 when sqlException.Class == 11 => true,
            // Login failed due to resource limits
            18456 when sqlException.Message.Contains("resource", StringComparison.OrdinalIgnoreCase) => true,
            _ => false
        };
    }

    /// <summary>
    /// Disposes the connection factory and releases any resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogDebug("Disposing MsSqlConnectionFactory");
        
        // Clear connection pools
        try
        {
            SqlConnection.ClearAllPools();
            _logger.LogDebug("SQL Server connection pools cleared");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clearing SQL Server connection pools during disposal");
        }

        _disposed = true;
        
        _logger.LogDebug("MsSqlConnectionFactory disposed");
    }

    /// <summary>
    /// Throws an exception if the factory has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MsSqlConnectionFactory));
        }
    }
}

/// <summary>
/// Contains connection information for diagnostic purposes.
/// </summary>
public sealed class ConnectionInfo
{
    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public string? ProviderType { get; set; }

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether connection pooling is enabled.
    /// </summary>
    public bool EnableConnectionPooling { get; set; }

    /// <summary>
    /// Gets or sets the maximum pool size.
    /// </summary>
    public int MaxPoolSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether auto retry is enabled.
    /// </summary>
    public bool EnableAutoRetry { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Windows authentication is used.
    /// </summary>
    public bool UseWindowsAuthentication { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection is encrypted.
    /// </summary>
    public bool EncryptConnection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to trust the server certificate.
    /// </summary>
    public bool TrustServerCertificate { get; set; }

    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether MARS is enabled.
    /// </summary>
    public bool EnableMars { get; set; }
}