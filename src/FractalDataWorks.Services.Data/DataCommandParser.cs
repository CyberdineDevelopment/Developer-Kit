using System;
using System.Collections.Generic;
using FractalDataWorks.Connections.Data;
using FractalDataWorks.Connections.Data.Commands;
using FractalDataWorks.Data;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Data;

/// <summary>
/// Parses logical paths into data commands based on configuration
/// </summary>
public class DataCommandParser
{
    private readonly DataProviderConfiguration _configuration;
    private readonly ILogger<DataCommandParser> _logger;
    
    public DataCommandParser(
        DataProviderConfiguration configuration,
        ILogger<DataCommandParser> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Parses a logical path into a data command
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="logicalPath">The logical path (e.g., "customers/active")</param>
    /// <param name="connectionId">Optional connection ID to use</param>
    /// <returns>A query command configured for the logical path</returns>
    public IQueryCommand<T> Parse<T>(string logicalPath, string? connectionId = null) where T : class
    {
        try
        {
            // Parse the logical path
            var parts = logicalPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                throw new ArgumentException("Invalid logical path");
            
            var record = parts[0];
            
            // Get connection configuration
            connectionId ??= _configuration.DefaultConnectionId;
            if (string.IsNullOrEmpty(connectionId))
                throw new InvalidOperationException("No connection ID specified and no default configured");
            
            if (!_configuration.Connections.TryGetValue(connectionId, out var connectionConfig))
                throw new ArgumentException($"Unknown connection ID: {connectionId}");
            
            // Build the command
            var builder = QueryCommand<T>.Create()
                .From(connectionConfig.DataStore, 
                      connectionConfig.Settings.GetValueOrDefault("Container")?.ToString() ?? connectionId,
                      record);
            
            // Add any filters from the path
            if (parts.Length > 1)
            {
                // Example: "customers/active" might add a Where clause
                builder.WithParameter("PathFilter", parts[1]);
            }
            
            _logger.LogDebug("Parsed logical path '{Path}' to {DataStore}/{Record}", 
                logicalPath, connectionConfig.DataStore, record);
            
            return builder.Build();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing logical path: {Path}", logicalPath);
            throw;
        }
    }
    
    /// <summary>
    /// Creates a command from a template
    /// </summary>
    public IDataCommand CreateFromTemplate(string templateName, Dictionary<string, object> parameters)
    {
        // This could be extended to support command templates in configuration
        throw new NotImplementedException("Command templates not yet implemented");
    }
}