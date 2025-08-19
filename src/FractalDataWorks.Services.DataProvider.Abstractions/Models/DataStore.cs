using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Models;

/// <summary>
/// Represents a physical data storage system such as a database server, file system, API endpoint base, or SFTP server.
/// </summary>
/// <remarks>
/// DataStore abstracts the physical system that holds data, providing a unified way to represent
/// different types of data sources:
/// - SQL Server: Database server with connection string and authentication
/// - File System: Local or network file system with root path
/// - REST API: Base URL with authentication and rate limiting
/// - SFTP: Remote file server with SSH authentication
/// - MongoDB: NoSQL database with cluster configuration
/// - Redis: In-memory cache with connection pooling
/// 
/// Each DataStore contains DataContainers that organize the actual data.
/// </remarks>
public sealed class DataStore : IEquatable<DataStore>
{
    private readonly Dictionary<string, DataContainer> _containers;
    private readonly Dictionary<string, object> _connectionProperties;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataStore"/> class.
    /// </summary>
    /// <param name="name">The logical name of the data store.</param>
    /// <param name="providerType">The type of provider (SqlServer, FileSystem, RestApi, etc.).</param>
    /// <param name="connectionProperties">Provider-specific connection properties.</param>
    /// <param name="containers">The containers within this data store.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public DataStore(
        string name,
        string providerType,
        IReadOnlyDictionary<string, object> connectionProperties,
        IEnumerable<DataContainer>? containers = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        
        if (string.IsNullOrWhiteSpace(providerType))
            throw new ArgumentException("Provider type cannot be null or empty.", nameof(providerType));
        
        Name = name;
        ProviderType = providerType;
        
        _connectionProperties = connectionProperties != null 
            ? new Dictionary<string, object>(connectionProperties, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        
        _containers = containers != null
            ? containers.ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, DataContainer>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the logical name of this data store.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of provider for this data store.
    /// </summary>
    /// <remarks>
    /// Common provider types include:
    /// - SqlServer: Microsoft SQL Server
    /// - PostgreSql: PostgreSQL database
    /// - MySQL: MySQL database
    /// - FileSystem: Local or network file system
    /// - RestApi: RESTful web API
    /// - Sftp: SFTP file server
    /// - MongoDB: MongoDB document database
    /// - Redis: Redis cache
    /// - Azure: Azure storage services
    /// - Amazon: AWS storage services
    /// </remarks>
    public string ProviderType { get; }

    /// <summary>
    /// Gets the provider-specific connection properties.
    /// </summary>
    public IReadOnlyDictionary<string, object> ConnectionProperties => _connectionProperties;

    /// <summary>
    /// Gets all containers in this data store.
    /// </summary>
    public IEnumerable<DataContainer> Containers => _containers.Values;

    /// <summary>
    /// Gets the number of containers in this data store.
    /// </summary>
    public int ContainerCount => _containers.Count;

    /// <summary>
    /// Gets a value indicating whether this data store has any containers.
    /// </summary>
    public bool HasContainers => _containers.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this data store supports hierarchical containers.
    /// </summary>
    public bool SupportsHierarchy => ProviderType switch
    {
        "SqlServer" => true,      // Supports Database.Schema.Table
        "PostgreSql" => true,     // Supports Schema.Table
        "MySQL" => true,          // Supports Database.Table
        "FileSystem" => true,     // Supports nested folders
        "Sftp" => true,          // Supports nested directories
        "RestApi" => true,       // Supports nested resource paths
        "MongoDB" => false,      // Flat collection structure
        "Redis" => false,        // Key-value store
        _ => false
    };

    /// <summary>
    /// Gets a value indicating whether this data store supports schema discovery.
    /// </summary>
    public bool SupportsSchemaDiscovery => ProviderType switch
    {
        "SqlServer" => true,
        "PostgreSql" => true,
        "MySQL" => true,
        "MongoDB" => true,
        "FileSystem" => true,
        "RestApi" => false,      // Usually requires manual configuration
        "Sftp" => true,
        "Redis" => false,
        _ => false
    };

    /// <summary>
    /// Creates a SQL Server data store.
    /// </summary>
    /// <param name="name">The logical name.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="additionalProperties">Additional connection properties.</param>
    /// <returns>A new DataStore instance.</returns>
    public static DataStore SqlServer(string name, string connectionString, IReadOnlyDictionary<string, object>? additionalProperties = null)
    {
        var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["ConnectionString"] = connectionString ?? throw new ArgumentNullException(nameof(connectionString))
        };
        
        if (additionalProperties != null)
        {
            foreach (var kvp in additionalProperties)
            {
                properties[kvp.Key] = kvp.Value;
            }
        }
        
        return new DataStore(name, "SqlServer", properties);
    }

    /// <summary>
    /// Creates a File System data store.
    /// </summary>
    /// <param name="name">The logical name.</param>
    /// <param name="rootPath">The root path to the file system.</param>
    /// <param name="additionalProperties">Additional properties.</param>
    /// <returns>A new DataStore instance.</returns>
    public static DataStore FileSystem(string name, string rootPath, IReadOnlyDictionary<string, object>? additionalProperties = null)
    {
        var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["RootPath"] = rootPath ?? throw new ArgumentNullException(nameof(rootPath))
        };
        
        if (additionalProperties != null)
        {
            foreach (var kvp in additionalProperties)
            {
                properties[kvp.Key] = kvp.Value;
            }
        }
        
        return new DataStore(name, "FileSystem", properties);
    }

    /// <summary>
    /// Creates a REST API data store.
    /// </summary>
    /// <param name="name">The logical name.</param>
    /// <param name="baseUrl">The base URL for the API.</param>
    /// <param name="additionalProperties">Additional properties like authentication.</param>
    /// <returns>A new DataStore instance.</returns>
    public static DataStore RestApi(string name, string baseUrl, IReadOnlyDictionary<string, object>? additionalProperties = null)
    {
        var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["BaseUrl"] = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl))
        };
        
        if (additionalProperties != null)
        {
            foreach (var kvp in additionalProperties)
            {
                properties[kvp.Key] = kvp.Value;
            }
        }
        
        return new DataStore(name, "RestApi", properties);
    }

    /// <summary>
    /// Creates a MongoDB data store.
    /// </summary>
    /// <param name="name">The logical name.</param>
    /// <param name="connectionString">The MongoDB connection string.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="additionalProperties">Additional properties.</param>
    /// <returns>A new DataStore instance.</returns>
    public static DataStore MongoDB(string name, string connectionString, string databaseName, IReadOnlyDictionary<string, object>? additionalProperties = null)
    {
        var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["ConnectionString"] = connectionString ?? throw new ArgumentNullException(nameof(connectionString)),
            ["DatabaseName"] = databaseName ?? throw new ArgumentNullException(nameof(databaseName))
        };
        
        if (additionalProperties != null)
        {
            foreach (var kvp in additionalProperties)
            {
                properties[kvp.Key] = kvp.Value;
            }
        }
        
        return new DataStore(name, "MongoDB", properties);
    }

    /// <summary>
    /// Gets a container by name.
    /// </summary>
    /// <param name="name">The name of the container.</param>
    /// <returns>The container with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no container with the specified name exists.</exception>
    public DataContainer this[string name]
    {
        get
        {
            if (!_containers.TryGetValue(name, out var container))
                throw new KeyNotFoundException($"Container with name '{name}' not found in data store '{Name}'.");
            return container;
        }
    }

    /// <summary>
    /// Checks if a container with the specified name exists.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if a container with the specified name exists; otherwise, false.</returns>
    public bool ContainsContainer(string name)
    {
        return _containers.ContainsKey(name);
    }

    /// <summary>
    /// Tries to get a container by name.
    /// </summary>
    /// <param name="name">The name of the container.</param>
    /// <param name="container">The container if found.</param>
    /// <returns>True if the container was found; otherwise, false.</returns>
    public bool TryGetContainer(string name, out DataContainer? container)
    {
        return _containers.TryGetValue(name, out container);
    }

    /// <summary>
    /// Finds a container by path (supports hierarchical lookup).
    /// </summary>
    /// <param name="path">The path to the container.</param>
    /// <returns>The container if found; otherwise, null.</returns>
    public DataContainer? FindContainer(DataPath path)
    {
        if (path.Depth == 0)
            return null;
        
        if (!_containers.TryGetValue(path.Root, out var rootContainer))
            return null;
        
        if (path.Depth == 1)
            return rootContainer;
        
        var remainingPath = new DataPath(path.Segments.Skip(1), path.Separator);
        return rootContainer.FindDescendant(remainingPath);
    }

    /// <summary>
    /// Gets all containers of the specified type.
    /// </summary>
    /// <param name="type">The container type to filter by.</param>
    /// <returns>All containers of the specified type.</returns>
    public IEnumerable<DataContainer> GetContainersByType(ContainerType type)
    {
        foreach (var container in _containers.Values)
        {
            if (container.Type == type)
                yield return container;
            
            foreach (var descendant in container.GetDescendantsByType(type))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Gets all containers that can contain data records.
    /// </summary>
    /// <returns>All data-containing containers.</returns>
    public IEnumerable<DataContainer> GetDataContainers()
    {
        return _containers.Values.SelectMany(c => c.GetDataContainers());
    }

    /// <summary>
    /// Gets a connection property value.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The property key.</param>
    /// <returns>The property value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    public T GetConnectionProperty<T>(string key)
    {
        if (!_connectionProperties.TryGetValue(key, out var value))
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
    /// Tries to get a connection property value.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value if found and converted successfully.</param>
    /// <returns>True if the value was found and converted successfully; otherwise, false.</returns>
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
    /// Creates a new DataStore with an added container.
    /// </summary>
    /// <param name="container">The container to add.</param>
    /// <returns>A new DataStore instance with the added container.</returns>
    /// <exception cref="ArgumentNullException">Thrown when container is null.</exception>
    /// <exception cref="ArgumentException">Thrown when a container with the same name already exists.</exception>
    public DataStore AddContainer(DataContainer container)
    {
        if (container == null)
            throw new ArgumentNullException(nameof(container));
        
        if (_containers.ContainsKey(container.Name))
            throw new ArgumentException($"Container with name '{container.Name}' already exists in data store '{Name}'.", nameof(container));
        
        var newContainers = _containers.Values.Concat(new[] { container });
        return new DataStore(Name, ProviderType, ConnectionProperties, newContainers);
    }

    /// <summary>
    /// Creates a new DataStore with a container removed.
    /// </summary>
    /// <param name="containerName">The name of the container to remove.</param>
    /// <returns>A new DataStore instance without the specified container.</returns>
    public DataStore RemoveContainer(string containerName)
    {
        var newContainers = _containers.Values.Where(c => !string.Equals(c.Name, containerName, StringComparison.OrdinalIgnoreCase));
        return new DataStore(Name, ProviderType, ConnectionProperties, newContainers);
    }

    /// <summary>
    /// Returns a string representation of the data store.
    /// </summary>
    /// <returns>A string describing the data store.</returns>
    public override string ToString()
    {
        return $"{ProviderType} DataStore '{Name}' ({ContainerCount} containers)";
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as DataStore);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(DataStore? other)
    {
        if (other == null)
            return false;
        
        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(ProviderType, other.ProviderType, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name.ToLowerInvariant(), ProviderType.ToLowerInvariant());
    }

    /// <summary>
    /// Determines whether two DataStore instances are equal.
    /// </summary>
    /// <param name="left">The first DataStore to compare.</param>
    /// <param name="right">The second DataStore to compare.</param>
    /// <returns>True if the DataStore instances are equal; otherwise, false.</returns>
    public static bool operator ==(DataStore? left, DataStore? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        
        if (left is null || right is null)
            return false;
        
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two DataStore instances are not equal.
    /// </summary>
    /// <param name="left">The first DataStore to compare.</param>
    /// <param name="right">The second DataStore to compare.</param>
    /// <returns>True if the DataStore instances are not equal; otherwise, false.</returns>
    public static bool operator !=(DataStore? left, DataStore? right)
    {
        return !(left == right);
    }
}