using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace FractalDataWorks.Samples.MsSqlProvider.Services;

/// <summary>
/// Service responsible for initializing the database and running SQL scripts
/// </summary>
public sealed class DatabaseInitializationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly string _connectionString;
    private readonly string _scriptsPath;
    private readonly int _executionTimeout;

    public DatabaseInitializationService(
        IConfiguration configuration,
        ILogger<DatabaseInitializationService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _connectionString = _configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DefaultConnection connection string is required");
        
        _scriptsPath = _configuration["DatabaseInitialization:ScriptsPath"] ?? "Scripts";
        _executionTimeout = _configuration.GetValue<int>("DatabaseInitialization:ExecutionTimeout", 300);
    }

    /// <summary>
    /// Initializes the database by running all SQL scripts in order
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if initialization was successful</returns>
    public async Task<bool> InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting database initialization");
            var stopwatch = Stopwatch.StartNew();

            // Check LocalDB availability
            if (!await CheckLocalDbAvailabilityAsync(cancellationToken))
            {
                _logger.LogError("LocalDB is not available. Please ensure SQL Server LocalDB is installed");
                return false;
            }

            // Get script files in order
            var scriptFiles = GetScriptFilesInOrder();
            if (scriptFiles.Count == 0)
            {
                _logger.LogWarning("No SQL scripts found in {ScriptsPath}", _scriptsPath);
                return true;
            }

            _logger.LogInformation("Found {ScriptCount} SQL scripts to execute", scriptFiles.Count);

            // Execute scripts in order
            foreach (var scriptFile in scriptFiles)
            {
                if (!await ExecuteScriptFileAsync(scriptFile, cancellationToken))
                {
                    _logger.LogError("Failed to execute script: {ScriptFile}", scriptFile);
                    return false;
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("Database initialization completed successfully in {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database");
            return false;
        }
    }

    /// <summary>
    /// Checks if LocalDB is available and accessible
    /// </summary>
    private async Task<bool> CheckLocalDbAvailabilityAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Checking LocalDB availability");
            
            // Try to connect to master database first
            var masterConnectionString = GetMasterConnectionString();
            using var connection = new SqlConnection(masterConnectionString);
            
            await connection.OpenAsync(cancellationToken);
            _logger.LogDebug("Successfully connected to LocalDB master database");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LocalDB is not available");
            return false;
        }
    }

    /// <summary>
    /// Gets script files in the correct execution order
    /// </summary>
    private List<string> GetScriptFilesInOrder()
    {
        var scriptsDirectory = Path.Combine(AppContext.BaseDirectory, _scriptsPath);
        
        if (!Directory.Exists(scriptsDirectory))
        {
            _logger.LogWarning("Scripts directory not found: {ScriptsDirectory}", scriptsDirectory);
            return new List<string>();
        }

        var scriptFiles = Directory.GetFiles(scriptsDirectory, "*.sql")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        _logger.LogDebug("Found script files: {ScriptFiles}", 
            string.Join(", ", scriptFiles.Select(Path.GetFileName)));

        return scriptFiles;
    }

    /// <summary>
    /// Executes a single SQL script file
    /// </summary>
    private async Task<bool> ExecuteScriptFileAsync(string scriptFile, CancellationToken cancellationToken)
    {
        try
        {
            var fileName = Path.GetFileName(scriptFile);
            _logger.LogInformation("Executing script: {FileName}", fileName);
            
            var stopwatch = Stopwatch.StartNew();
            var scriptContent = await File.ReadAllTextAsync(scriptFile, cancellationToken);
            
            if (string.IsNullOrWhiteSpace(scriptContent))
            {
                _logger.LogWarning("Script file {FileName} is empty, skipping", fileName);
                return true;
            }

            // Use master connection for database creation, target connection for others
            var connectionString = fileName.StartsWith("01_") 
                ? GetMasterConnectionString() 
                : _connectionString;

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Split script by GO statements and execute each batch
            var batches = SplitScriptIntoBatches(scriptContent);
            
            foreach (var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch))
                    continue;

                using var command = new SqlCommand(batch, connection)
                {
                    CommandTimeout = _executionTimeout,
                    CommandType = System.Data.CommandType.Text
                };

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            stopwatch.Stop();
            _logger.LogInformation("Successfully executed {FileName} in {ElapsedMs}ms", 
                fileName, stopwatch.ElapsedMilliseconds);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute script: {ScriptFile}", Path.GetFileName(scriptFile));
            return false;
        }
    }

    /// <summary>
    /// Splits SQL script content into batches separated by GO statements
    /// </summary>
    private static List<string> SplitScriptIntoBatches(string scriptContent)
    {
        var batches = new List<string>();
        var lines = scriptContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var currentBatch = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip comments and empty lines
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("--"))
                continue;

            // Check for GO statement (case insensitive, standalone)
            if (string.Equals(trimmedLine, "GO", StringComparison.OrdinalIgnoreCase))
            {
                if (currentBatch.Count > 0)
                {
                    batches.Add(string.Join(Environment.NewLine, currentBatch));
                    currentBatch.Clear();
                }
            }
            else
            {
                currentBatch.Add(line);
            }
        }

        // Add the last batch if it exists
        if (currentBatch.Count > 0)
        {
            batches.Add(string.Join(Environment.NewLine, currentBatch));
        }

        return batches;
    }

    /// <summary>
    /// Gets connection string for the master database
    /// </summary>
    private string GetMasterConnectionString()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };
        return builder.ConnectionString;
    }

    /// <summary>
    /// Verifies that the database is properly initialized
    /// </summary>
    public async Task<bool> VerifyDatabaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Verifying database initialization");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Check if required schemas exist
            var schemaCheckSql = @"
                SELECT COUNT(*) 
                FROM sys.schemas 
                WHERE name IN ('sales', 'inventory', 'users')";

            using var schemaCommand = new SqlCommand(schemaCheckSql, connection);
            var schemaCount = (int)await schemaCommand.ExecuteScalarAsync(cancellationToken);

            if (schemaCount != 3)
            {
                _logger.LogError("Expected 3 schemas (sales, inventory, users), found {SchemaCount}", schemaCount);
                return false;
            }

            // Check if required tables exist
            var tableCheckSql = @"
                SELECT COUNT(*) 
                FROM sys.tables t
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name IN ('sales', 'inventory', 'users')";

            using var tableCommand = new SqlCommand(tableCheckSql, connection);
            var tableCount = (int)await tableCommand.ExecuteScalarAsync(cancellationToken);

            if (tableCount < 5) // Customers, Orders, Categories, Products, UserActivity
            {
                _logger.LogError("Expected at least 5 tables, found {TableCount}", tableCount);
                return false;
            }

            _logger.LogInformation("Database verification successful. Found {SchemaCount} schemas and {TableCount} tables", 
                schemaCount, tableCount);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database verification failed");
            return false;
        }
    }
}