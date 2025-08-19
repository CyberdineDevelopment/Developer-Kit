using System;
using System.Collections.Generic;
using FractalDataWorks.Configuration;
using FluentValidation;
using FluentValidation.Results;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Configuration for data store mapping between logical and physical data structures.
/// </summary>
/// <remarks>
/// This configuration enables the DataProvider to map universal data model entities
/// to physical storage implementations across various storage systems including
/// SQL databases, NoSQL databases, file systems, and APIs.
/// 
/// Key features:
/// - Logical to physical container mapping (e.g., Customer entity -> Customers table)
/// - Column-level datum mapping with categorization
/// - Schema discovery and caching settings
/// - Connection pooling and health check configuration
/// - Support for convention-based and explicit mapping strategies
/// </remarks>
public sealed class DataStoreConfiguration : ConfigurationBase<DataStoreConfiguration>
{
    /// <summary>
    /// Gets or sets the unique name of this data store.
    /// </summary>
    /// <remarks>
    /// This name is used to identify the data store in logging, metrics, and configuration references.
    /// Must be unique within a given application context.
    /// </remarks>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider type for this data store.
    /// </summary>
    /// <remarks>
    /// Examples: "MsSql", "PostgreSql", "MySql", "MongoDb", "FileSystem", "RestApi", "Sftp"
    /// This value determines which concrete data provider implementation to use.
    /// </remarks>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection properties specific to the provider type.
    /// </summary>
    /// <remarks>
    /// Provider-specific configuration such as:
    /// - SQL: ConnectionString, CommandTimeout, ConnectionTimeout
    /// - File: BasePath, FilePattern, Encoding
    /// - API: BaseUrl, ApiKey, Timeout
    /// - SFTP: Host, Port, Username, PrivateKeyPath
    /// </remarks>
    public Dictionary<string, object> ConnectionProperties { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the list of container mappings from logical to physical structures.
    /// </summary>
    /// <remarks>
    /// Each mapping defines how a logical data container (e.g., "Customer") maps to
    /// physical storage (e.g., "dbo.Customers" table, "/data/customers.json" file).
    /// Includes column-level datum mappings for each container.
    /// </remarks>
    public List<DataContainerMapping> ContainerMappings { get; set; } = new();

    /// <summary>
    /// Gets or sets the strategy for categorizing data columns into datum categories.
    /// </summary>
    /// <remarks>
    /// Controls how columns are automatically categorized as Identifier, Property, 
    /// Measure, or Metadata. Can use conventions (naming patterns), explicit 
    /// configuration, or a hybrid approach.
    /// </remarks>
    public DatumCategorizationStrategy CategorizationStrategy { get; set; } = new();

    /// <summary>
    /// Gets or sets the schema discovery settings for automatic mapping generation.
    /// </summary>
    /// <remarks>
    /// When enabled, the provider can automatically discover physical schema
    /// structure and generate container/datum mappings. Useful for rapid prototyping
    /// and when working with dynamic schemas.
    /// </remarks>
    public SchemaDiscoverySettings SchemaDiscovery { get; set; } = new();

    /// <summary>
    /// Gets or sets the connection pooling settings for providers that support it.
    /// </summary>
    /// <remarks>
    /// Only applies to providers that maintain persistent connections (SQL databases, APIs).
    /// File-based and stateless providers typically ignore these settings.
    /// </remarks>
    public ConnectionPoolingSettings ConnectionPooling { get; set; } = new();

    /// <summary>
    /// Gets or sets the health check settings for monitoring data store availability.
    /// </summary>
    /// <remarks>
    /// Defines how often to check data store health, what constitutes a healthy state,
    /// and timeout values for health checks. Used by monitoring and circuit breaker patterns.
    /// </remarks>
    public HealthCheckSettings HealthCheck { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to enable automatic container discovery.
    /// </summary>
    /// <remarks>
    /// When true, the provider will attempt to discover available containers
    /// (tables, files, endpoints) at startup and add them to ContainerMappings
    /// if they don't already exist.
    /// </remarks>
    public bool EnableAutoDiscovery { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of concurrent operations allowed.
    /// </summary>
    /// <remarks>
    /// Limits concurrent queries/operations to prevent overwhelming the data store.
    /// Set to 0 for no limit. Default is 50 for most providers.
    /// </remarks>
    public int MaxConcurrentOperations { get; set; } = 50;

    /// <summary>
    /// Gets or sets the default timeout in seconds for data operations.
    /// </summary>
    /// <remarks>
    /// Applied to queries, commands, and other data operations unless overridden
    /// at the operation level. Default is 30 seconds.
    /// </remarks>
    public int DefaultTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets additional provider-specific settings as key-value pairs.
    /// </summary>
    /// <remarks>
    /// Extensibility mechanism for provider-specific configuration that doesn't
    /// fit into the standard connection properties. Examples might include
    /// custom retry policies, caching strategies, or authentication methods.
    /// </remarks>
    public Dictionary<string, object> ExtendedProperties { get; set; } = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public override string SectionName => "DataProvider:DataStores";

    /// <summary>
    /// Finds a container mapping by logical name.
    /// </summary>
    /// <param name="logicalName">The logical container name to find.</param>
    /// <returns>The container mapping if found; otherwise, null.</returns>
    public DataContainerMapping? FindContainerMapping(string logicalName)
    {
        if (string.IsNullOrWhiteSpace(logicalName))
            return null;

        foreach (var mapping in ContainerMappings)
        {
            if (string.Equals(mapping.LogicalName, logicalName, StringComparison.OrdinalIgnoreCase))
                return mapping;
        }

        return null;
    }

    /// <summary>
    /// Gets a connection property value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The property key.</param>
    /// <returns>The property value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    public T GetConnectionProperty<T>(string key)
    {
        if (!ConnectionProperties.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Connection property '{key}' not found.");

        if (value is T directValue)
            return directValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Cannot convert connection property '{key}' value from {value?.GetType().Name ?? "null"} to {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Tries to get a connection property value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value if found and converted successfully.</param>
    /// <returns>True if the property was found and converted successfully; otherwise, false.</returns>
    public bool TryGetConnectionProperty<T>(string key, out T? value)
    {
        try
        {
            value = GetConnectionProperty<T>(key);
            return true;
        }
        catch
        {
            value = default(T);
            return false;
        }
    }

    /// <summary>
    /// Gets an extended property value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The property key.</param>
    /// <returns>The property value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    public T GetExtendedProperty<T>(string key)
    {
        if (!ExtendedProperties.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Extended property '{key}' not found.");

        if (value is T directValue)
            return directValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Cannot convert extended property '{key}' value from {value?.GetType().Name ?? "null"} to {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Tries to get an extended property value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value if found and converted successfully.</param>
    /// <returns>True if the property was found and converted successfully; otherwise, false.</returns>
    public bool TryGetExtendedProperty<T>(string key, out T? value)
    {
        try
        {
            value = GetExtendedProperty<T>(key);
            return true;
        }
        catch
        {
            value = default(T);
            return false;
        }
    }

    /// <inheritdoc/>
    protected override IValidator<DataStoreConfiguration>? GetValidator()
    {
        return new DataStoreConfigurationValidator();
    }

    /// <inheritdoc/>
    protected override void CopyTo(DataStoreConfiguration target)
    {
        base.CopyTo(target);
        target.StoreName = StoreName;
        target.ProviderType = ProviderType;
        target.ConnectionProperties = new Dictionary<string, object>(ConnectionProperties, StringComparer.Ordinal);
        target.ContainerMappings = new List<DataContainerMapping>(ContainerMappings);
        target.CategorizationStrategy = CategorizationStrategy;
        target.SchemaDiscovery = SchemaDiscovery;
        target.ConnectionPooling = ConnectionPooling;
        target.HealthCheck = HealthCheck;
        target.EnableAutoDiscovery = EnableAutoDiscovery;
        target.MaxConcurrentOperations = MaxConcurrentOperations;
        target.DefaultTimeoutSeconds = DefaultTimeoutSeconds;
        target.ExtendedProperties = new Dictionary<string, object>(ExtendedProperties, StringComparer.Ordinal);
    }


}