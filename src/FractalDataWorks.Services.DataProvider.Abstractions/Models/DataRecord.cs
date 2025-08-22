using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Models;

/// <summary>
/// Represents an individual data entity equivalent to a row in a database, a document in NoSQL, 
/// a record in a file, or a resource instance from an API.
/// </summary>
/// <remarks>
/// DataRecord provides a unified way to represent data entities across different storage systems.
/// It organizes data into semantic categories using Datum objects, making it provider-independent
/// while maintaining the ability to map back to provider-specific structures.
/// </remarks>
public sealed class DataRecord : IEquatable<DataRecord>
{
    private readonly Dictionary<string, Datum> _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataRecord"/> class.
    /// </summary>
    /// <param name="data">The collection of datum objects that make up this record.</param>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    public DataRecord(IEnumerable<Datum> data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        
        _data = data.ToDictionary(d => d.Name, d => d, StringComparer.OrdinalIgnoreCase);
        
        // Pre-compute categorized collections for performance
        Identifiers = _data.Values.Where(d => d.Category == DatumCategory.Identifier).ToList().AsReadOnly();
        Properties = _data.Values.Where(d => d.Category == DatumCategory.Property).ToList().AsReadOnly();
        Measures = _data.Values.Where(d => d.Category == DatumCategory.Measure).ToList().AsReadOnly();
        Metadata = _data.Values.Where(d => d.Category == DatumCategory.Metadata).ToList().AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataRecord"/> class with no data.
    /// </summary>
    public DataRecord() : this(Enumerable.Empty<Datum>())
    {
    }

    /// <summary>
    /// Gets all the data in this record.
    /// </summary>
    public IEnumerable<Datum> Data => _data.Values;

    /// <summary>
    /// Gets all identifier datum objects (primary keys, natural keys, unique identifiers).
    /// </summary>
    public IReadOnlyList<Datum> Identifiers { get; }

    /// <summary>
    /// Gets all property datum objects (descriptive attributes).
    /// </summary>
    public IReadOnlyList<Datum> Properties { get; }

    /// <summary>
    /// Gets all measure datum objects (numeric values that can be aggregated).
    /// </summary>
    public IReadOnlyList<Datum> Measures { get; }

    /// <summary>
    /// Gets all metadata datum objects (system fields, audit trails).
    /// </summary>
    public IReadOnlyList<Datum> Metadata { get; }

    /// <summary>
    /// Gets the number of datum objects in this record.
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Gets a value indicating whether this record is empty.
    /// </summary>
    public bool IsEmpty => _data.Count == 0;

    /// <summary>
    /// Gets a datum by name.
    /// </summary>
    /// <param name="name">The name of the datum.</param>
    /// <returns>The datum with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no datum with the specified name exists.</exception>
    public Datum this[string name]
    {
        get
        {
            if (!_data.TryGetValue(name, out var datum))
                throw new KeyNotFoundException($"Datum with name '{name}' not found in record.");
            return datum;
        }
    }

    /// <summary>
    /// Checks if a datum with the specified name exists in this record.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if a datum with the specified name exists; otherwise, false.</returns>
    public bool Contains(string name)
    {
        return _data.ContainsKey(name);
    }

    /// <summary>
    /// Tries to get a datum by name.
    /// </summary>
    /// <param name="name">The name of the datum.</param>
    /// <param name="datum">The datum if found.</param>
    /// <returns>True if the datum was found; otherwise, false.</returns>
    public bool TryGetDatum(string name, out Datum? datum)
    {
        return _data.TryGetValue(name, out datum);
    }

    /// <summary>
    /// Gets the value of a datum as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="name">The name of the datum.</param>
    /// <returns>The value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no datum with the specified name exists.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    public T? GetValue<T>(string name)
    {
        return this[name].GetValue<T>();
    }

    /// <summary>
    /// Tries to get the value of a datum as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="name">The name of the datum.</param>
    /// <param name="value">The converted value if successful.</param>
    /// <returns>True if the value was found and converted successfully; otherwise, false.</returns>
    public bool TryGetValue<T>(string name, out T? value)
    {
        if (_data.TryGetValue(name, out var datum))
        {
            return datum.TryGetValue<T>(out value);
        }
        
        value = default(T);
        return false;
    }

    /// <summary>
    /// Gets all datum objects of the specified category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>A collection of datum objects in the specified category.</returns>
    public IEnumerable<Datum> GetByCategory(DatumCategory category)
    {
        return category switch
        {
            DatumCategory.Identifier => Identifiers,
            DatumCategory.Property => Properties,
            DatumCategory.Measure => Measures,
            DatumCategory.Metadata => Metadata,
            _ => Enumerable.Empty<Datum>()
        };
    }

    /// <summary>
    /// Creates a new DataRecord with an additional datum.
    /// </summary>
    /// <param name="datum">The datum to add.</param>
    /// <returns>A new DataRecord instance with the added datum.</returns>
    /// <exception cref="ArgumentNullException">Thrown when datum is null.</exception>
    /// <exception cref="ArgumentException">Thrown when a datum with the same name already exists.</exception>
    public DataRecord Add(Datum datum)
    {
        if (datum == null)
            throw new ArgumentNullException(nameof(datum));
        
        if (_data.ContainsKey(datum.Name))
            throw new ArgumentException($"Datum with name '{datum.Name}' already exists in record.", nameof(datum));
        
        var newData = _data.Values.Concat(new[] { datum });
        return new DataRecord(newData);
    }

    /// <summary>
    /// Creates a new DataRecord with a datum replaced or added.
    /// </summary>
    /// <param name="datum">The datum to set.</param>
    /// <returns>A new DataRecord instance with the set datum.</returns>
    /// <exception cref="ArgumentNullException">Thrown when datum is null.</exception>
    public DataRecord Set(Datum datum)
    {
        if (datum == null)
            throw new ArgumentNullException(nameof(datum));
        
        var newData = _data.Values.Where(d => !string.Equals(d.Name, datum.Name, StringComparison.OrdinalIgnoreCase))
                                  .Concat(new[] { datum });
        return new DataRecord(newData);
    }

    /// <summary>
    /// Creates a new DataRecord with a datum removed.
    /// </summary>
    /// <param name="name">The name of the datum to remove.</param>
    /// <returns>A new DataRecord instance without the specified datum.</returns>
    public DataRecord Remove(string name)
    {
        var newData = _data.Values.Where(d => !string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
        return new DataRecord(newData);
    }

    /// <summary>
    /// Creates a new DataRecord with updated value for the specified datum.
    /// </summary>
    /// <param name="name">The name of the datum to update.</param>
    /// <param name="newValue">The new value.</param>
    /// <returns>A new DataRecord instance with the updated value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no datum with the specified name exists.</exception>
    public DataRecord UpdateValue(string name, object? newValue)
    {
        if (!_data.TryGetValue(name, out var existingDatum))
            throw new KeyNotFoundException($"Datum with name '{name}' not found in record.");
        
        var updatedDatum = existingDatum.WithValue(newValue);
        return Set(updatedDatum);
    }

    /// <summary>
    /// Creates a new DataRecord containing only datum objects of the specified categories.
    /// </summary>
    /// <param name="categories">The categories to include.</param>
    /// <returns>A new DataRecord instance with only the specified categories.</returns>
    public DataRecord FilterByCategories(params DatumCategory[] categories)
    {
        var categorySet = new HashSet<DatumCategory>(categories);
        var filteredData = _data.Values.Where(d => categorySet.Contains(d.Category));
        return new DataRecord(filteredData);
    }

    /// <summary>
    /// Converts this DataRecord to a dictionary with datum names as keys and values as values.
    /// </summary>
    /// <returns>A dictionary representation of this record.</returns>
    public IDictionary<string, object?> ToDictionary()
    {
        return _data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a DataRecord from a dictionary.
    /// </summary>
    /// <param name="values">The dictionary of values.</param>
    /// <param name="category">The default category for all values.</param>
    /// <returns>A new DataRecord instance.</returns>
    public static DataRecord FromDictionary(IDictionary<string, object?> values, DatumCategory category = DatumCategory.Property)
    {
        var data = values.Select(kvp => new Datum(kvp.Key, category, kvp.Value?.GetType() ?? typeof(object), kvp.Value));
        return new DataRecord(data);
    }

    /// <summary>
    /// Creates a DataRecord from an object using reflection.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <param name="identifierProperties">Names of properties that should be treated as identifiers.</param>
    /// <param name="measureProperties">Names of properties that should be treated as measures.</param>
    /// <param name="metadataProperties">Names of properties that should be treated as metadata.</param>
    /// <returns>A new DataRecord instance.</returns>
    public static DataRecord FromObject(object obj, 
        IEnumerable<string>? identifierProperties = null,
        IEnumerable<string>? measureProperties = null,
        IEnumerable<string>? metadataProperties = null)
    {
        if (obj == null)
            return new DataRecord();
        
        var identifierSet = new HashSet<string>(identifierProperties ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var measureSet = new HashSet<string>(measureProperties ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var metadataSet = new HashSet<string>(metadataProperties ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        
        var properties = obj.GetType().GetProperties();
        var data = properties.Select(prop =>
        {
            var value = prop.GetValue(obj);
            var category = identifierSet.Contains(prop.Name) ? DatumCategory.Identifier :
                          measureSet.Contains(prop.Name) ? DatumCategory.Measure :
                          metadataSet.Contains(prop.Name) ? DatumCategory.Metadata :
                          DatumCategory.Property;
            
            return new Datum(prop.Name, category, prop.PropertyType, value, new[] { prop.Name });
        });
        
        return new DataRecord(data);
    }

    /// <summary>
    /// Returns a string representation of the record.
    /// </summary>
    /// <returns>A string describing the record.</returns>
    public override string ToString()
    {
        if (IsEmpty)
            return "DataRecord(empty)";
        
        var summary = $"DataRecord({Count} fields: {Identifiers.Count} id, {Properties.Count} props, {Measures.Count} measures, {Metadata.Count} meta)";
        return summary;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as DataRecord);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(DataRecord? other)
    {
        if (other == null)
            return false;
        
        if (_data.Count != other._data.Count)
            return false;
        
        foreach (var kvp in _data)
        {
            if (!other._data.TryGetValue(kvp.Key, out var otherDatum) || !kvp.Value.Equals(otherDatum))
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        
        foreach (var kvp in _data.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            hash.Add(kvp.Key.ToLowerInvariant());
            hash.Add(kvp.Value);
        }
        
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines whether two DataRecord instances are equal.
    /// </summary>
    /// <param name="left">The first DataRecord to compare.</param>
    /// <param name="right">The second DataRecord to compare.</param>
    /// <returns>True if the DataRecord instances are equal; otherwise, false.</returns>
    public static bool operator ==(DataRecord? left, DataRecord? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        
        if (left is null || right is null)
            return false;
        
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two DataRecord instances are not equal.
    /// </summary>
    /// <param name="left">The first DataRecord to compare.</param>
    /// <param name="right">The second DataRecord to compare.</param>
    /// <returns>True if the DataRecord instances are not equal; otherwise, false.</returns>
    public static bool operator !=(DataRecord? left, DataRecord? right)
    {
        return !(left == right);
    }
}