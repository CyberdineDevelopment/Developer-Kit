using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Models;

/// <summary>
/// Represents a logical collection of data records that can map to tables, views, folders, API resources, etc.
/// </summary>
/// <remarks>
/// DataContainer provides a unified way to represent data collections across different storage systems:
/// - SQL Server: Tables, Views, Stored Procedures
/// - File System: Directories containing data files
/// - REST API: Resource endpoints (e.g., /api/customers)
/// - SFTP: Remote directories
/// - NoSQL: Collections, Document stores
/// 
/// Containers can be hierarchical, allowing for nested structures like schema.table or folder/subfolder.
/// </remarks>
public sealed class DataContainer : IEquatable<DataContainer>
{
    private readonly List<DataContainer> _children;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataContainer"/> class.
    /// </summary>
    /// <param name="path">The hierarchical path to this container.</param>
    /// <param name="name">The logical name of the container.</param>
    /// <param name="type">The type of container.</param>
    /// <param name="children">Child containers for hierarchical structures.</param>
    /// <param name="metadata">Additional metadata about the container.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public DataContainer(
        DataPath path,
        string name,
        ContainerType type,
        IEnumerable<DataContainer>? children = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        
        Name = name;
        Type = type;
        Metadata = metadata ?? new Dictionary<string, object>();
        
        _children = children != null ? new List<DataContainer>(children) : new List<DataContainer>();
    }

    /// <summary>
    /// Gets the hierarchical path to this container.
    /// </summary>
    public DataPath Path { get; }

    /// <summary>
    /// Gets the logical name of this container.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the type of this container.
    /// </summary>
    public ContainerType Type { get; }

    /// <summary>
    /// Gets the child containers for hierarchical structures.
    /// </summary>
    public IReadOnlyList<DataContainer> Children => _children.AsReadOnly();

    /// <summary>
    /// Gets additional metadata about this container.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets a value indicating whether this container has child containers.
    /// </summary>
    public bool HasChildren => _children.Count > 0;

    /// <summary>
    /// Gets the number of child containers.
    /// </summary>
    public int ChildCount => _children.Count;

    /// <summary>
    /// Gets a value indicating whether this container can contain data records directly.
    /// </summary>
    public bool CanContainData => Type switch
    {
        ContainerType.Table => true,
        ContainerType.View => true,
        ContainerType.File => true,
        ContainerType.Resource => true,
        ContainerType.Collection => true,
        ContainerType.Stream => true,
        _ => false
    };

    /// <summary>
    /// Gets a value indicating whether this container is primarily structural (contains other containers).
    /// </summary>
    public bool IsStructural => Type switch
    {
        ContainerType.Schema => true,
        ContainerType.Database => true,
        ContainerType.Folder => true,
        ContainerType.Namespace => true,
        _ => false
    };

    /// <summary>
    /// Creates a Table container.
    /// </summary>
    /// <param name="path">The path to the table.</param>
    /// <param name="name">The table name.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A new DataContainer instance.</returns>
    public static DataContainer Table(DataPath path, string name, IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new DataContainer(path, name, ContainerType.Table, null, metadata);
    }

    /// <summary>
    /// Creates a View container.
    /// </summary>
    /// <param name="path">The path to the view.</param>
    /// <param name="name">The view name.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A new DataContainer instance.</returns>
    public static DataContainer View(DataPath path, string name, IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new DataContainer(path, name, ContainerType.View, null, metadata);
    }

    /// <summary>
    /// Creates a Folder container.
    /// </summary>
    /// <param name="path">The path to the folder.</param>
    /// <param name="name">The folder name.</param>
    /// <param name="children">Child containers in the folder.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A new DataContainer instance.</returns>
    public static DataContainer Folder(DataPath path, string name, IEnumerable<DataContainer>? children = null, IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new DataContainer(path, name, ContainerType.Folder, children, metadata);
    }

    /// <summary>
    /// Creates a File container.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="name">The file name.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A new DataContainer instance.</returns>
    public static DataContainer File(DataPath path, string name, IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new DataContainer(path, name, ContainerType.File, null, metadata);
    }

    /// <summary>
    /// Creates a Resource container (for API endpoints).
    /// </summary>
    /// <param name="path">The path to the resource.</param>
    /// <param name="name">The resource name.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A new DataContainer instance.</returns>
    public static DataContainer Resource(DataPath path, string name, IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new DataContainer(path, name, ContainerType.Resource, null, metadata);
    }

    /// <summary>
    /// Creates a Collection container (for NoSQL).
    /// </summary>
    /// <param name="path">The path to the collection.</param>
    /// <param name="name">The collection name.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A new DataContainer instance.</returns>
    public static DataContainer Collection(DataPath path, string name, IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new DataContainer(path, name, ContainerType.Collection, null, metadata);
    }

    /// <summary>
    /// Creates a Schema container.
    /// </summary>
    /// <param name="path">The path to the schema.</param>
    /// <param name="name">The schema name.</param>
    /// <param name="children">Child containers in the schema.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A new DataContainer instance.</returns>
    public static DataContainer Schema(DataPath path, string name, IEnumerable<DataContainer>? children = null, IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new DataContainer(path, name, ContainerType.Schema, children, metadata);
    }

    /// <summary>
    /// Finds a child container by name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>The child container if found; otherwise, null.</returns>
    public DataContainer? FindChild(string name)
    {
        return _children.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds a descendant container by path.
    /// </summary>
    /// <param name="relativePath">The relative path from this container.</param>
    /// <returns>The descendant container if found; otherwise, null.</returns>
    public DataContainer? FindDescendant(DataPath relativePath)
    {
        if (relativePath.Depth == 0)
            return this;
        
        var child = FindChild(relativePath.Root);
        if (child == null || relativePath.Depth == 1)
            return child;
        
        var remainingPath = new DataPath(relativePath.Segments.Skip(1), relativePath.Separator);
        return child.FindDescendant(remainingPath);
    }

    /// <summary>
    /// Gets all descendant containers recursively.
    /// </summary>
    /// <returns>All descendant containers.</returns>
    public IEnumerable<DataContainer> GetAllDescendants()
    {
        foreach (var child in _children)
        {
            yield return child;
            
            foreach (var descendant in child.GetAllDescendants())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Gets all descendant containers of the specified type.
    /// </summary>
    /// <param name="type">The container type to filter by.</param>
    /// <returns>All descendant containers of the specified type.</returns>
    public IEnumerable<DataContainer> GetDescendantsByType(ContainerType type)
    {
        return GetAllDescendants().Where(c => c.Type == type);
    }

    /// <summary>
    /// Gets all containers that can contain data records.
    /// </summary>
    /// <returns>All data-containing containers.</returns>
    public IEnumerable<DataContainer> GetDataContainers()
    {
        if (CanContainData)
            yield return this;
        
        foreach (var dataContainer in GetAllDescendants().Where(c => c.CanContainData))
        {
            yield return dataContainer;
        }
    }

    /// <summary>
    /// Creates a new DataContainer with an added child.
    /// </summary>
    /// <param name="child">The child container to add.</param>
    /// <returns>A new DataContainer instance with the added child.</returns>
    /// <exception cref="ArgumentNullException">Thrown when child is null.</exception>
    /// <exception cref="ArgumentException">Thrown when a child with the same name already exists.</exception>
    public DataContainer AddChild(DataContainer child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));
        
        if (_children.Any(c => string.Equals(c.Name, child.Name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Child container with name '{child.Name}' already exists.", nameof(child));
        
        var newChildren = _children.Concat(new[] { child });
        return new DataContainer(Path, Name, Type, newChildren, Metadata);
    }

    /// <summary>
    /// Creates a new DataContainer with a child removed.
    /// </summary>
    /// <param name="childName">The name of the child to remove.</param>
    /// <returns>A new DataContainer instance without the specified child.</returns>
    public DataContainer RemoveChild(string childName)
    {
        var newChildren = _children.Where(c => !string.Equals(c.Name, childName, StringComparison.OrdinalIgnoreCase));
        return new DataContainer(Path, Name, Type, newChildren, Metadata);
    }

    /// <summary>
    /// Gets metadata value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The metadata key.</param>
    /// <returns>The metadata value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    public T GetMetadata<T>(string key)
    {
        if (!Metadata.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Metadata key '{key}' not found.");
        
        if (value is T directValue)
            return directValue;
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Cannot convert metadata '{key}' value from {value?.GetType().Name ?? "null"} to {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Tries to get metadata value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value if found and converted successfully.</param>
    /// <returns>True if the value was found and converted successfully; otherwise, false.</returns>
    public bool TryGetMetadata<T>(string key, out T? value)
    {
        try
        {
            value = GetMetadata<T>(key);
            return true;
        }
        catch
        {
            value = default(T);
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of the container.
    /// </summary>
    /// <returns>A string describing the container.</returns>
    public override string ToString()
    {
        var childInfo = HasChildren ? $" ({ChildCount} children)" : "";
        return $"{Type}: {Path}{childInfo}";
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as DataContainer);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(DataContainer? other)
    {
        if (other == null)
            return false;
        
        return Path.Equals(other.Path) &&
               string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
               Type == other.Type;
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Path, Name.ToLowerInvariant(), Type);
    }

    /// <summary>
    /// Determines whether two DataContainer instances are equal.
    /// </summary>
    /// <param name="left">The first DataContainer to compare.</param>
    /// <param name="right">The second DataContainer to compare.</param>
    /// <returns>True if the DataContainer instances are equal; otherwise, false.</returns>
    public static bool operator ==(DataContainer? left, DataContainer? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        
        if (left is null || right is null)
            return false;
        
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two DataContainer instances are not equal.
    /// </summary>
    /// <param name="left">The first DataContainer to compare.</param>
    /// <param name="right">The second DataContainer to compare.</param>
    /// <returns>True if the DataContainer instances are not equal; otherwise, false.</returns>
    public static bool operator !=(DataContainer? left, DataContainer? right)
    {
        return !(left == right);
    }
}