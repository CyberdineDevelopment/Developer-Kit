using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Models;

/// <summary>
/// Represents a categorized data value that can map to zero, one, or many physical columns.
/// </summary>
/// <remarks>
/// Datum provides semantic categorization of data fields across different storage systems:
/// - Identifiers: Primary keys, natural keys, unique identifiers
/// - Properties: Descriptive attributes like names, descriptions, addresses
/// - Measures: Numeric values that can be aggregated like amounts, counts, percentages
/// - Metadata: System fields like timestamps, audit trails, version numbers
/// 
/// A Datum can represent:
/// - Zero columns: Calculated fields that don't exist in storage
/// - One column: Standard field mapping
/// - Many columns: Composite fields like Address (Street, City, State, Zip)
/// </remarks>
public sealed class Datum : IEquatable<Datum>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Datum"/> class.
    /// </summary>
    /// <param name="name">The logical name of the datum.</param>
    /// <param name="category">The semantic category of the datum.</param>
    /// <param name="dataType">The .NET type of the datum value.</param>
    /// <param name="value">The current value of the datum.</param>
    /// <param name="physicalColumns">The physical column names this datum maps to.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public Datum(
        string name,
        DatumCategory category,
        Type dataType,
        object? value = null,
        IEnumerable<string>? physicalColumns = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        
        Name = name;
        Category = category;
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        Value = value;
        PhysicalColumns = physicalColumns != null 
            ? new List<string>(physicalColumns).AsReadOnly()
            : new List<string>().AsReadOnly();
    }

    /// <summary>
    /// Gets the logical name of this datum.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the semantic category of this datum.
    /// </summary>
    public DatumCategory Category { get; }

    /// <summary>
    /// Gets the .NET type of the datum value.
    /// </summary>
    public Type DataType { get; }

    /// <summary>
    /// Gets the current value of the datum.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets the physical column names this datum maps to.
    /// </summary>
    /// <remarks>
    /// - Empty list: Calculated field with no physical storage
    /// - Single item: Standard one-to-one field mapping
    /// - Multiple items: Composite field spanning multiple columns
    /// </remarks>
    public IReadOnlyList<string> PhysicalColumns { get; }

    /// <summary>
    /// Gets a value indicating whether this datum maps to multiple physical columns.
    /// </summary>
    public bool IsComposite => PhysicalColumns.Count > 1;

    /// <summary>
    /// Gets a value indicating whether this datum is calculated and has no physical storage.
    /// </summary>
    public bool IsCalculated => PhysicalColumns.Count == 0;

    /// <summary>
    /// Gets a value indicating whether this datum has a null value.
    /// </summary>
    public bool IsNull => Value == null;

    /// <summary>
    /// Creates a new Datum for an identifier field.
    /// </summary>
    /// <param name="name">The logical name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="value">The value.</param>
    /// <param name="physicalColumns">The physical column names.</param>
    /// <returns>A new Datum instance.</returns>
    public static Datum Identifier(string name, Type dataType, object? value = null, params string[] physicalColumns)
    {
        return new Datum(name, DatumCategory.Identifier, dataType, value, physicalColumns);
    }

    /// <summary>
    /// Creates a new Datum for a property field.
    /// </summary>
    /// <param name="name">The logical name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="value">The value.</param>
    /// <param name="physicalColumns">The physical column names.</param>
    /// <returns>A new Datum instance.</returns>
    public static Datum Property(string name, Type dataType, object? value = null, params string[] physicalColumns)
    {
        return new Datum(name, DatumCategory.Property, dataType, value, physicalColumns);
    }

    /// <summary>
    /// Creates a new Datum for a measure field.
    /// </summary>
    /// <param name="name">The logical name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="value">The value.</param>
    /// <param name="physicalColumns">The physical column names.</param>
    /// <returns>A new Datum instance.</returns>
    public static Datum Measure(string name, Type dataType, object? value = null, params string[] physicalColumns)
    {
        return new Datum(name, DatumCategory.Measure, dataType, value, physicalColumns);
    }

    /// <summary>
    /// Creates a new Datum for a metadata field.
    /// </summary>
    /// <param name="name">The logical name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="value">The value.</param>
    /// <param name="physicalColumns">The physical column names.</param>
    /// <returns>A new Datum instance.</returns>
    public static Datum Metadata(string name, Type dataType, object? value = null, params string[] physicalColumns)
    {
        return new Datum(name, DatumCategory.Metadata, dataType, value, physicalColumns);
    }

    /// <summary>
    /// Creates a new Datum for a calculated field with no physical storage.
    /// </summary>
    /// <param name="name">The logical name.</param>
    /// <param name="category">The semantic category.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="value">The calculated value.</param>
    /// <returns>A new Datum instance.</returns>
    public static Datum Calculated(string name, DatumCategory category, Type dataType, object? value = null)
    {
        return new Datum(name, category, dataType, value, null);
    }

    /// <summary>
    /// Creates a new Datum with a different value.
    /// </summary>
    /// <param name="newValue">The new value.</param>
    /// <returns>A new Datum instance with the specified value.</returns>
    public Datum WithValue(object? newValue)
    {
        return new Datum(Name, Category, DataType, newValue, PhysicalColumns);
    }

    /// <summary>
    /// Gets the value as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <returns>The value converted to the specified type.</returns>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    public T? GetValue<T>()
    {
        if (Value == null)
            return default(T);
        
        if (Value is T directValue)
            return directValue;
        
        try
        {
            return (T)Convert.ChangeType(Value, typeof(T));
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Cannot convert datum '{Name}' value from {Value.GetType().Name} to {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Tries to get the value as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <param name="value">The converted value if successful.</param>
    /// <returns>True if the conversion was successful; otherwise, false.</returns>
    public bool TryGetValue<T>(out T? value)
    {
        try
        {
            value = GetValue<T>();
            return true;
        }
        catch
        {
            value = default(T);
            return false;
        }
    }

    /// <summary>
    /// Returns a string representation of the datum.
    /// </summary>
    /// <returns>A string describing the datum.</returns>
    public override string ToString()
    {
        var columnsInfo = IsCalculated ? "calculated" : 
                         IsComposite ? $"composite({PhysicalColumns.Count})" : 
                         $"[{PhysicalColumns[0]}]";
        
        return $"{Name}({Category}): {DataType.Name} = {Value ?? "null"} [{columnsInfo}]";
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Datum);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(Datum? other)
    {
        if (other == null)
            return false;
        
        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
               Category == other.Category &&
               DataType == other.DataType &&
               Equals(Value, other.Value);
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Name.ToLowerInvariant(),
            Category,
            DataType,
            Value);
    }

    /// <summary>
    /// Determines whether two Datum instances are equal.
    /// </summary>
    /// <param name="left">The first Datum to compare.</param>
    /// <param name="right">The second Datum to compare.</param>
    /// <returns>True if the Datum instances are equal; otherwise, false.</returns>
    public static bool operator ==(Datum? left, Datum? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        
        if (left is null || right is null)
            return false;
        
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Datum instances are not equal.
    /// </summary>
    /// <param name="left">The first Datum to compare.</param>
    /// <param name="right">The second Datum to compare.</param>
    /// <returns>True if the Datum instances are not equal; otherwise, false.</returns>
    public static bool operator !=(Datum? left, Datum? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Defines the semantic categories for data fields.
/// </summary>
public enum DatumCategory
{
    /// <summary>
    /// Identifier fields such as primary keys, natural keys, and unique identifiers.
    /// </summary>
    Identifier = 1,

    /// <summary>
    /// Descriptive attribute fields such as names, descriptions, and addresses.
    /// </summary>
    Property = 2,

    /// <summary>
    /// Numeric fields that can be aggregated such as amounts, counts, and percentages.
    /// </summary>
    Measure = 3,

    /// <summary>
    /// System fields such as timestamps, audit trails, and version numbers.
    /// </summary>
    Metadata = 4
}