
using System;
using System.Collections.Generic;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FluentValidation;

namespace FractalDataWorks.Services.ExternalConnections.MsSql;

/// <summary>
/// Configuration for SQL Server external connections.
/// </summary>
/// <remarks>
/// This configuration provides comprehensive settings for SQL Server connections including
/// connection string, timeouts, schema mappings, and connection pooling options.
/// </remarks>
public sealed class MsSqlConfiguration : ConfigurationBase<MsSqlConfiguration>, IExternalConnectionConfiguration
{
    /// <summary>
    /// Gets or sets the connection string for the SQL Server database.
    /// </summary>
    /// <remarks>
    /// This should be a complete connection string including server, database, 
    /// authentication information, and any additional connection parameters.
    /// </remarks>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    /// <remarks>
    /// This timeout applies to command execution. Default is 30 seconds.
    /// Set to 0 for infinite timeout (not recommended).
    /// </remarks>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    /// <remarks>
    /// This timeout applies to establishing the initial connection.
    /// Default is 15 seconds.
    /// </remarks>
    public int ConnectionTimeoutSeconds { get; set; } = 15;

    /// <summary>
    /// Gets or sets the default schema to use for operations.
    /// </summary>
    /// <remarks>
    /// When not specified in DataPath, this schema will be used.
    /// Defaults to "dbo" if not specified.
    /// </remarks>
    public string DefaultSchema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the schema mappings for data containers.
    /// </summary>
    /// <remarks>
    /// Maps DataContainer names to SQL Server schema.table combinations.
    /// Key is the container name, value is schema.table or just table name.
    /// If not found in mappings, will use DefaultSchema + container name.
    /// </remarks>
    public IDictionary<string, string> SchemaMappings { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets a value indicating whether to enable connection pooling.
    /// </summary>
    /// <remarks>
    /// Connection pooling is enabled by default for better performance.
    /// Disable only if you have specific requirements.
    /// </remarks>
    public bool EnableConnectionPooling { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum pool size.
    /// </summary>
    /// <remarks>
    /// The minimum number of connections to maintain in the pool.
    /// Only used when EnableConnectionPooling is true.
    /// </remarks>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum pool size.
    /// </summary>
    /// <remarks>
    /// The maximum number of connections allowed in the pool.
    /// Only used when EnableConnectionPooling is true.
    /// </remarks>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether to enable multiple active result sets (MARS).
    /// </summary>
    /// <remarks>
    /// MARS allows multiple batches to be pending on a single connection.
    /// Disabled by default for better compatibility.
    /// </remarks>
    public bool EnableMultipleActiveResultSets { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to enable retry logic for transient failures.
    /// </summary>
    /// <remarks>
    /// Automatically retry operations on transient SQL Server errors.
    /// Enabled by default for better reliability.
    /// </remarks>
    public bool EnableRetryLogic { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    /// <remarks>
    /// Only used when EnableRetryLogic is true.
    /// Default is 3 retry attempts.
    /// </remarks>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay between retry attempts in milliseconds.
    /// </summary>
    /// <remarks>
    /// Only used when EnableRetryLogic is true.
    /// Uses exponential backoff based on this value.
    /// </remarks>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether to enable detailed logging of SQL commands.
    /// </summary>
    /// <remarks>
    /// When enabled, logs generated SQL statements for debugging.
    /// Disable in production for security and performance.
    /// </remarks>
    public bool EnableSqlLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum length for logged SQL statements.
    /// </summary>
    /// <remarks>
    /// Only used when EnableSqlLogging is true.
    /// Prevents excessive log output from large SQL statements.
    /// </remarks>
    public int MaxSqlLogLength { get; set; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether to use transactions for command execution.
    /// </summary>
    /// <remarks>
    /// When enabled, each Execute call will be wrapped in a transaction.
    /// Transactions are automatically committed on success or rolled back on failure.
    /// Default is false for better performance.
    /// </remarks>
    public bool UseTransactions { get; set; } = false;

    /// <summary>
    /// Gets or sets the transaction isolation level to use when UseTransactions is enabled.
    /// </summary>
    /// <remarks>
    /// Only used when UseTransactions is true.
    /// Default is ReadCommitted for balanced consistency and performance.
    /// </remarks>
    public System.Data.IsolationLevel TransactionIsolationLevel { get; set; } = System.Data.IsolationLevel.ReadCommitted;

    /// <inheritdoc/>
    public override string SectionName => "ExternalConnections:MsSql";

    /// <summary>
    /// Gets the sanitized connection string for logging purposes.
    /// </summary>
    /// <returns>A connection string with sensitive information removed.</returns>
    public string GetSanitizedConnectionString()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            return "(empty)";

        // Simple sanitization - remove password and other sensitive keywords using manual string processing
        var sanitized = ConnectionString;
        var sensitiveKeywords = new[] { "password", "pwd", "user id", "uid" };

        foreach (var keyword in sensitiveKeywords)
        {
            sanitized = SanitizeConnectionStringKeyword(sanitized, keyword);
        }

        return sanitized;
    }

    private static string SanitizeConnectionStringKeyword(string connectionString, string keyword)
    {
        var comparison = StringComparison.OrdinalIgnoreCase;
        var keywordWithEquals = keyword + "=";
        var startIndex = 0;

        while (startIndex < connectionString.Length)
        {
            var keywordIndex = connectionString.IndexOf(keywordWithEquals, startIndex, comparison);
            if (keywordIndex == -1)
                break;

            // Find the end of the value (next semicolon or end of string)
            var valueStartIndex = keywordIndex + keywordWithEquals.Length;
            var valueEndIndex = connectionString.IndexOf(';', valueStartIndex);
            if (valueEndIndex == -1)
                valueEndIndex = connectionString.Length;

            // Replace the value with ***
            var beforeKeyword = connectionString[..keywordIndex];
            var keywordPart = connectionString.Substring(keywordIndex, keywordWithEquals.Length);
            var afterValue = connectionString[valueEndIndex..];
            
            connectionString = beforeKeyword + keywordPart + "***" + afterValue;
            startIndex = keywordIndex + keywordPart.Length + 3; // 3 is length of "***"
        }

        return connectionString;
    }

    /// <summary>
    /// Resolves the actual SQL schema and table name for a given container name.
    /// </summary>
    /// <param name="containerName">The data container name.</param>
    /// <returns>A tuple containing the schema name and table name.</returns>
    public (string Schema, string Table) ResolveSchemaAndTable(string containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Container name cannot be null or empty.", nameof(containerName));

        // Check if we have a specific mapping
        if (SchemaMappings.TryGetValue(containerName, out var mapping) && !string.IsNullOrWhiteSpace(mapping))
        {
            var parts = mapping.Split('.', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                1 => (DefaultSchema, parts[0]),
                2 => (parts[0], parts[1]),
                _ => (DefaultSchema, containerName)
            };
        }

        // Use default schema + container name
        return (DefaultSchema, containerName);
    }

    /// <inheritdoc/>
    protected override IValidator<MsSqlConfiguration>? GetValidator()
    {
        return new MsSqlConfigurationValidator();
    }

    /// <inheritdoc/>
    protected override void CopyTo(MsSqlConfiguration target)
    {
        base.CopyTo(target);
        target.ConnectionString = ConnectionString;
        target.CommandTimeoutSeconds = CommandTimeoutSeconds;
        target.ConnectionTimeoutSeconds = ConnectionTimeoutSeconds;
        target.DefaultSchema = DefaultSchema;
        target.SchemaMappings = new Dictionary<string, string>(SchemaMappings, StringComparer.Ordinal);
        target.EnableConnectionPooling = EnableConnectionPooling;
        target.MinPoolSize = MinPoolSize;
        target.MaxPoolSize = MaxPoolSize;
        target.EnableMultipleActiveResultSets = EnableMultipleActiveResultSets;
        target.EnableRetryLogic = EnableRetryLogic;
        target.MaxRetryAttempts = MaxRetryAttempts;
        target.RetryDelayMilliseconds = RetryDelayMilliseconds;
        target.EnableSqlLogging = EnableSqlLogging;
        target.MaxSqlLogLength = MaxSqlLogLength;
        target.UseTransactions = UseTransactions;
        target.TransactionIsolationLevel = TransactionIsolationLevel;
    }
}