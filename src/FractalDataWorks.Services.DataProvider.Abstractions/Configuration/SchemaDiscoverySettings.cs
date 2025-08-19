using System;
using System.Collections.Generic;
using FluentValidation;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Configuration for automatic schema discovery and metadata caching.
/// </summary>
/// <remarks>
/// Controls how the data provider automatically discovers physical schema structures
/// and generates container/datum mappings. Useful for rapid development, prototyping,
/// and working with dynamic or evolving schemas. Includes caching strategies to
/// minimize discovery overhead in production environments.
/// 
/// Discovery Process:
/// 1. Connect to data store and enumerate available containers
/// 2. For each container, discover column structure and metadata
/// 3. Apply categorization strategy to determine datum categories
/// 4. Generate DataContainerMapping and DatumMapping configurations
/// 5. Cache results according to cache strategy settings
/// </remarks>
public sealed class SchemaDiscoverySettings
{
    /// <summary>
    /// Gets or sets a value indicating whether schema discovery is enabled.
    /// </summary>
    /// <remarks>
    /// When true, the provider will attempt to discover schema information
    /// at startup or when explicitly requested. Discovery can be resource-intensive
    /// for large schemas, so consider disabling in production if not needed.
    /// </remarks>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the caching strategy for discovered schema information.
    /// </summary>
    /// <remarks>
    /// Determines how long and where to cache discovered schema metadata:
    /// - None: No caching, discover on every request
    /// - Memory: Cache in memory for the application lifetime
    /// - Persistent: Cache to disk/database with expiration
    /// - Hybrid: Memory cache with persistent backup
    /// </remarks>
    public CacheStrategy CacheStrategy { get; set; } = CacheStrategy.Memory;

    /// <summary>
    /// Gets or sets the cache duration in minutes for discovered schema information.
    /// </summary>
    /// <remarks>
    /// How long to keep cached schema information before refreshing.
    /// Applied when CacheStrategy is not None. Set to 0 for no expiration.
    /// Default is 60 minutes (1 hour).
    /// </remarks>
    public int CacheDurationMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether to refresh schema on application startup.
    /// </summary>
    /// <remarks>
    /// When true, forces a fresh schema discovery when the provider initializes,
    /// regardless of cache state. Useful for ensuring up-to-date schema information
    /// but may slow startup for large schemas.
    /// </remarks>
    public bool RefreshOnStartup { get; set; } = false;

    /// <summary>
    /// Gets or sets the auto-refresh interval in minutes.
    /// </summary>
    /// <remarks>
    /// Automatically refreshes cached schema information at this interval.
    /// Set to 0 to disable auto-refresh. Only applies when caching is enabled.
    /// Default is 0 (disabled) to avoid unexpected background operations.
    /// </remarks>
    public int AutoRefreshIntervalMinutes { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum number of containers to discover in a single operation.
    /// </summary>
    /// <remarks>
    /// Limits the scope of discovery operations to prevent overwhelming large schemas.
    /// Set to 0 for no limit. Can be used with inclusion/exclusion patterns to
    /// focus discovery on relevant containers.
    /// </remarks>
    public int MaxContainers { get; set; } = 0;

    /// <summary>
    /// Gets or sets the timeout in seconds for schema discovery operations.
    /// </summary>
    /// <remarks>
    /// Maximum time allowed for a single discovery operation before timing out.
    /// Large schemas or slow connections may require higher values.
    /// Default is 300 seconds (5 minutes).
    /// </remarks>
    public int DiscoveryTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the inclusion patterns for container discovery.
    /// </summary>
    /// <remarks>
    /// Wildcard patterns for containers to include in discovery.
    /// Only containers matching these patterns will be discovered.
    /// Empty list means include all containers.
    /// Examples: ["dbo.*", "sales.*"] for SQL Server schemas
    /// </remarks>
    public List<string> IncludePatterns { get; set; } = new();

    /// <summary>
    /// Gets or sets the exclusion patterns for container discovery.
    /// </summary>
    /// <remarks>
    /// Wildcard patterns for containers to exclude from discovery.
    /// Containers matching these patterns will be skipped.
    /// Applied after inclusion patterns.
    /// Examples: ["sys.*", "temp*", "*_backup"] for system tables
    /// </remarks>
    public List<string> ExcludePatterns { get; set; } = new()
    {
        "sys.*",
        "information_schema.*",
        "temp*",
        "*_temp",
        "*_backup",
        "*_log"
    };

    /// <summary>
    /// Gets or sets a value indicating whether to discover column metadata.
    /// </summary>
    /// <remarks>
    /// When true, discovers detailed column information including data types,
    /// constraints, default values, and descriptions. When false, only discovers
    /// container structure. Disabling can improve discovery performance.
    /// </remarks>
    public bool DiscoverColumnMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to discover relationships between containers.
    /// </summary>
    /// <remarks>
    /// When true, attempts to discover foreign key relationships, indexes,
    /// and other structural relationships. Useful for understanding data model
    /// but may significantly increase discovery time for complex schemas.
    /// </remarks>
    public bool DiscoverRelationships { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to merge discovered mappings with existing configuration.
    /// </summary>
    /// <remarks>
    /// When true, discovery results are merged with existing container mappings.
    /// Existing mappings take precedence over discovered ones. When false,
    /// discovery results replace existing mappings entirely.
    /// </remarks>
    public bool MergeWithExisting { get; set; } = true;

    /// <summary>
    /// Gets or sets additional discovery settings as key-value pairs.
    /// </summary>
    /// <remarks>
    /// Provider-specific discovery configuration:
    /// - SQL: "UseInformationSchema", "IncludeSystemObjects", "DiscoverIndexes"
    /// - File: "RecursiveSearch", "FileExtensions", "SampleFiles"
    /// - API: "DiscoverEndpoints", "FollowLinks", "AuthRequired"
    /// - NoSQL: "SampleDocuments", "InferSchema", "AnalyzeTypes"
    /// </remarks>
    public Dictionary<string, object> ExtendedSettings { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Checks if a container name matches the inclusion/exclusion patterns.
    /// </summary>
    /// <param name="containerName">The container name to check.</param>
    /// <returns>True if the container should be included in discovery.</returns>
    public bool ShouldIncludeContainer(string containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName))
            return false;

        // Check inclusion patterns - if specified, container must match at least one
        if (IncludePatterns.Count > 0)
        {
            var included = false;
            foreach (var pattern in IncludePatterns)
            {
                if (MatchesWildcardPattern(containerName, pattern))
                {
                    included = true;
                    break;
                }
            }
            if (!included)
                return false;
        }

        // Check exclusion patterns - if container matches any, exclude it
        foreach (var pattern in ExcludePatterns)
        {
            if (MatchesWildcardPattern(containerName, pattern))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets an extended setting value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    public T GetExtendedSetting<T>(string key)
    {
        if (!ExtendedSettings.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Extended setting '{key}' not found.");

        if (value is T directValue)
            return directValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Cannot convert extended setting '{key}' value from {value?.GetType().Name ?? "null"} to {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Tries to get an extended setting value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The setting value if found and converted successfully.</param>
    /// <returns>True if the setting was found and converted successfully; otherwise, false.</returns>
    public bool TryGetExtendedSetting<T>(string key, out T? value)
    {
        try
        {
            value = GetExtendedSetting<T>(key);
            return true;
        }
        catch
        {
            value = default(T);
            return false;
        }
    }

    /// <summary>
    /// Checks if a value matches a wildcard pattern (* only).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="pattern">The pattern with * wildcards.</param>
    /// <returns>True if the value matches the pattern.</returns>
    private static bool MatchesWildcardPattern(string value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        // Simple wildcard matching - * matches any sequence of characters
        if (pattern == "*")
            return true;

        if (!pattern.Contains('*'))
            return string.Equals(value, pattern, StringComparison.OrdinalIgnoreCase);

        var parts = pattern.Split('*');
        var currentIndex = 0;

        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            
            if (string.IsNullOrEmpty(part))
                continue;

            var foundIndex = value.IndexOf(part, currentIndex, StringComparison.OrdinalIgnoreCase);
            
            if (foundIndex == -1)
                return false;

            // For the first part, it must start at the beginning if pattern doesn't start with *
            if (i == 0 && !pattern.StartsWith("*") && foundIndex != 0)
                return false;

            // For the last part, it must end at the end if pattern doesn't end with *
            if (i == parts.Length - 1 && !pattern.EndsWith("*") && foundIndex + part.Length != value.Length)
                return false;

            currentIndex = foundIndex + part.Length;
        }

        return true;
    }
}

/// <summary>
/// Defines the caching strategies for discovered schema information.
/// </summary>
public enum CacheStrategy
{
    /// <summary>
    /// No caching - discover schema on every request.
    /// </summary>
    /// <remarks>
    /// Provides the most up-to-date information but has the highest performance cost.
    /// Suitable for development environments or schemas that change frequently.
    /// </remarks>
    None = 1,

    /// <summary>
    /// Cache schema information in memory for the application lifetime.
    /// </summary>
    /// <remarks>
    /// Fast access to cached information but lost on application restart.
    /// Good balance of performance and freshness for most scenarios.
    /// </remarks>
    Memory = 2,

    /// <summary>
    /// Cache schema information persistently with configurable expiration.
    /// </summary>
    /// <remarks>
    /// Preserves cache across application restarts. Requires additional storage
    /// but provides the best performance for static schemas.
    /// </remarks>
    Persistent = 3,

    /// <summary>
    /// Use memory cache with persistent backup for resilience.
    /// </summary>
    /// <remarks>
    /// Combines the speed of memory cache with the durability of persistent cache.
    /// Memory cache is populated from persistent cache on startup if available.
    /// </remarks>
    Hybrid = 4
}

/// <summary>
/// Validator for SchemaDiscoverySettings.
/// </summary>
internal sealed class SchemaDiscoverySettingsValidator : AbstractValidator<SchemaDiscoverySettings>
{
    public SchemaDiscoverySettingsValidator()
    {
        RuleFor(x => x.CacheStrategy)
            .IsInEnum()
            .WithMessage("Cache strategy must be a valid enum value.");

        RuleFor(x => x.CacheDurationMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Cache duration must be non-negative.");

        RuleFor(x => x.AutoRefreshIntervalMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Auto-refresh interval must be non-negative.");

        RuleFor(x => x.MaxContainers)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum containers must be non-negative.");

        RuleFor(x => x.DiscoveryTimeoutSeconds)
            .GreaterThan(0)
            .When(x => x.Enabled)
            .WithMessage("Discovery timeout must be positive when discovery is enabled.");

        RuleFor(x => x.IncludePatterns)
            .NotNull()
            .WithMessage("Include patterns cannot be null.");

        RuleFor(x => x.ExcludePatterns)
            .NotNull()
            .WithMessage("Exclude patterns cannot be null.");

        RuleFor(x => x.ExtendedSettings)
            .NotNull()
            .WithMessage("Extended settings cannot be null.");

        // Ensure patterns don't contain empty or null values
        RuleForEach(x => x.IncludePatterns)
            .NotEmpty()
            .WithMessage("Include patterns cannot contain empty values.");

        RuleForEach(x => x.ExcludePatterns)
            .NotEmpty()
            .WithMessage("Exclude patterns cannot contain empty values.");

        // Cache duration should be reasonable when auto-refresh is enabled
        RuleFor(x => x.CacheDurationMinutes)
            .GreaterThan(x => x.AutoRefreshIntervalMinutes)
            .When(x => x.AutoRefreshIntervalMinutes > 0 && x.CacheDurationMinutes > 0)
            .WithMessage("Cache duration should be greater than auto-refresh interval.");
    }
}