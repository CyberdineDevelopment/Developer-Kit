using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Configuration.Sources;

/// <summary>
/// Configuration source that reads and writes JSON files.
/// </summary>
public class JsonConfigurationSource : ConfigurationSourceBase
{
    private readonly string _basePath;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConfigurationSource"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="basePath">The base path for JSON configuration files.</param>
    public JsonConfigurationSource(ILogger<JsonConfigurationSource> logger, string basePath) 
        : base(logger, "JSON")
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        
        // Ensure the directory exists
        Directory.CreateDirectory(_basePath);
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <inheritdoc/>
    public override bool IsWritable => true;

    /// <inheritdoc/>
    protected override async Task<IFdwResult<TConfiguration>> LoadCore<TConfiguration>(int id)
    {
        var fileName = GetFileName<TConfiguration>(id);
        var filePath = Path.Combine(_basePath, fileName);

        if (!File.Exists(filePath))
        {
            return FdwResult<TConfiguration>.Failure($"Configuration file not found: {fileName}");
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var config = JsonSerializer.Deserialize<TConfiguration>(json, _jsonOptions);
            
            if (config == null)
            {
                return FdwResult<TConfiguration>.Failure("Failed to deserialize configuration");
            }

            return FdwResult<TConfiguration>.Success(config);
        }
        catch (Exception ex)
        {
            return FdwResult<TConfiguration>.Failure($"Error loading configuration: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult<IEnumerable<TConfiguration>>> LoadAllCore<TConfiguration>()
    {
        var typeName = typeof(TConfiguration).Name;
        var pattern = $"{typeName}_*.json";
        var files = Directory.GetFiles(_basePath, pattern);

        var configurations = new List<TConfiguration>();

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var config = JsonSerializer.Deserialize<TConfiguration>(json, _jsonOptions);
                
                if (config != null)
                {
                    configurations.Add(config);
                }
            }
            catch (Exception ex)
            {
                // Log but continue loading other files
                ConfigurationSourceBaseLog.LoadFailed(Logger, Name, ex.Message);
            }
        }

        return FdwResult<IEnumerable<TConfiguration>>.Success(configurations);
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult<TConfiguration>> SaveCore<TConfiguration>(TConfiguration configuration)
    {
        var fileName = GetFileName(configuration);
        var filePath = Path.Combine(_basePath, fileName);

        try
        {
            var json = JsonSerializer.Serialize(configuration, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            
            ConfigurationSourceBaseLog.ConfigurationSaved(Logger, Name, configuration.Id);
            
            return FdwResult<TConfiguration>.Success(configuration);
        }
        catch (Exception ex)
        {
            return FdwResult<TConfiguration>.Failure($"Error saving configuration: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult<NonResult>> DeleteCore<TConfiguration>(int id)
    {
        var fileName = GetFileName<TConfiguration>(id);
        var filePath = Path.Combine(_basePath, fileName);

        if (!File.Exists(filePath))
        {
            return FdwResult<NonResult>.Failure($"Configuration file not found: {fileName}");
        }

        try
        {
            await Task.Run(() => File.Delete(filePath));
            
            ConfigurationSourceBaseLog.ConfigurationDeleted(Logger, Name, id);
            
            return FdwResult<NonResult>.Success(NonResult.Value);
        }
        catch (Exception ex)
        {
            return FdwResult<NonResult>.Failure($"Error deleting configuration: {ex.Message}");
        }
    }

    private string GetFileName<TConfiguration>(TConfiguration configuration) 
        where TConfiguration : IFdwConfiguration
    {
        return GetFileName<TConfiguration>(configuration.Id);
    }

    private string GetFileName<TConfiguration>(int id) 
        where TConfiguration : IFdwConfiguration
    {
        var typeName = typeof(TConfiguration).Name;
        return $"{typeName}_{id}.json";
    }
}