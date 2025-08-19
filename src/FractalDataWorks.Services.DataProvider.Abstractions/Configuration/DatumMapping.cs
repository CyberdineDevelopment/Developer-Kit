using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Defines the mapping between a logical datum and its physical column representation.
/// </summary>
/// <remarks>
/// Maps individual data fields from the universal data model to physical storage columns.
/// Supports one-to-one, one-to-many (composite), and calculated field mappings. Includes
/// semantic categorization and optional data transformation expressions.
/// 
/// Mapping Types:
/// - Standard: LogicalName="CustomerId" -> PhysicalColumns=["customer_id"]
/// - Composite: LogicalName="Address" -> PhysicalColumns=["street", "city", "state", "zip"]
/// - Calculated: LogicalName="FullName" -> TransformExpression="CONCAT(first_name, ' ', last_name)"
/// - No Storage: LogicalName="IsActive" -> PhysicalColumns=[] (computed client-side)
/// </remarks>
public sealed class DatumMapping : IEquatable<DatumMapping>
{
    /// <summary>
    /// Gets or sets the logical name of the datum.
    /// </summary>
    /// <remarks>
    /// This is the universal field name used throughout the application to reference this data.
    /// Should follow consistent naming conventions regardless of the underlying storage system.
    /// Examples: "CustomerId", "FirstName", "OrderTotal", "CreatedAt", "FullAddress"
    /// </remarks>
    public string LogicalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of physical column names this datum maps to.
    /// </summary>
    /// <remarks>
    /// Physical storage column names in the target system:
    /// - Empty list: Calculated field with no physical storage
    /// - Single item: Standard one-to-one field mapping
    /// - Multiple items: Composite field spanning multiple columns
    /// 
    /// Examples:
    /// - ["customer_id"] - Simple mapping
    /// - ["first_name", "last_name"] - Composite name field
    /// - [] - Calculated field (computed at runtime)
    /// </remarks>
    public List<string> PhysicalColumns { get; set; } = new();

    /// <summary>
    /// Gets or sets the semantic category of this datum.
    /// </summary>
    /// <remarks>
    /// Categorizes the data field for appropriate handling by the data provider:
    /// - Identifier: Primary keys, foreign keys, unique identifiers
    /// - Property: Descriptive attributes like names, descriptions
    /// - Measure: Numeric values that can be aggregated (amounts, counts)
    /// - Metadata: System fields like timestamps, audit information
    /// </remarks>
    public DatumCategory DatumCategory { get; set; } = DatumCategory.Property;

    /// <summary>
    /// Gets or sets the .NET data type for this datum.
    /// </summary>
    /// <remarks>
    /// The expected .NET type when reading/writing this datum. Used for type conversion
    /// and validation. Common types: string, int, decimal, DateTime, bool, Guid.
    /// For composite fields, this represents the combined/aggregate type.
    /// </remarks>
    public Type? DataType { get; set; }

    /// <summary>
    /// Gets or sets the optional transformation expression for this datum.
    /// </summary>
    /// <remarks>
    /// Provider-specific expression for computing this datum value:
    /// - SQL: "CONCAT(first_name, ' ', last_name)", "CASE WHEN amount > 0 THEN 'Active' ELSE 'Inactive' END"
    /// - File: JSONPath expression like "$.customer.address.street"
    /// - API: Field selector like "data.user.profile.fullName"
    /// - NoSQL: Aggregation pipeline expression
    /// 
    /// When specified, this expression takes precedence over PhysicalColumns for reads.
    /// For writes, PhysicalColumns are still used unless the field is read-only.
    /// </remarks>
    public string? TransformExpression { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this datum is read-only.
    /// </summary>
    /// <remarks>
    /// When true, this datum cannot be modified through write operations.
    /// Typically used for calculated fields, system-generated values, or
    /// fields that are managed by the storage system itself.
    /// </remarks>
    public bool IsReadOnly { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether this datum is required.
    /// </summary>
    /// <remarks>
    /// When true, this datum must have a non-null value for insert operations.
    /// Used for validation before sending data to the storage system.
    /// </remarks>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets the default value for this datum when none is provided.
    /// </summary>
    /// <remarks>
    /// Used during insert operations when no value is specified for this datum.
    /// Can be a literal value, a provider-specific function, or an expression.
    /// Examples: "0", "GETDATE()", "NEW_UUID()", "current_timestamp"
    /// </remarks>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about this datum mapping.
    /// </summary>
    /// <remarks>
    /// Storage for provider-specific configuration or custom properties:
    /// - SQL: "MaxLength", "Precision", "Scale", "Collation"
    /// - File: "Format", "Pattern", "Encoding"
    /// - API: "FieldPath", "Format", "Validation"
    /// - NoSQL: "IndexHint", "Shard", "TTL"
    /// </remarks>
    public Dictionary<string, object> Metadata { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets a value indicating whether this datum maps to multiple physical columns.
    /// </summary>
    public bool IsComposite => PhysicalColumns.Count > 1;

    /// <summary>
    /// Gets a value indicating whether this datum is calculated and has no physical storage.
    /// </summary>
    public bool IsCalculated => PhysicalColumns.Count == 0 || !string.IsNullOrWhiteSpace(TransformExpression);

    /// <summary>
    /// Gets a value indicating whether this datum has a simple one-to-one column mapping.
    /// </summary>
    public bool IsSimple => PhysicalColumns.Count == 1 && string.IsNullOrWhiteSpace(TransformExpression);

    /// <summary>
    /// Creates a new DatumMapping for an identifier field.
    /// </summary>
    /// <param name="logicalName">The logical name.</param>
    /// <param name="physicalColumn">The physical column name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="isRequired">Whether the field is required.</param>
    /// <returns>A new DatumMapping instance.</returns>
    public static DatumMapping Identifier(string logicalName, string physicalColumn, Type? dataType = null, bool isRequired = true)
    {
        return new DatumMapping
        {
            LogicalName = logicalName,
            PhysicalColumns = new List<string> { physicalColumn },
            DatumCategory = DatumCategory.Identifier,
            DataType = dataType,
            IsRequired = isRequired
        };
    }

    /// <summary>
    /// Creates a new DatumMapping for a property field.
    /// </summary>
    /// <param name="logicalName">The logical name.</param>
    /// <param name="physicalColumn">The physical column name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="isRequired">Whether the field is required.</param>
    /// <returns>A new DatumMapping instance.</returns>
    public static DatumMapping Property(string logicalName, string physicalColumn, Type? dataType = null, bool isRequired = false)
    {
        return new DatumMapping
        {
            LogicalName = logicalName,
            PhysicalColumns = new List<string> { physicalColumn },
            DatumCategory = DatumCategory.Property,
            DataType = dataType,
            IsRequired = isRequired
        };
    }

    /// <summary>
    /// Creates a new DatumMapping for a measure field.
    /// </summary>
    /// <param name="logicalName">The logical name.</param>
    /// <param name="physicalColumn">The physical column name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="isRequired">Whether the field is required.</param>
    /// <returns>A new DatumMapping instance.</returns>
    public static DatumMapping Measure(string logicalName, string physicalColumn, Type? dataType = null, bool isRequired = false)
    {
        return new DatumMapping
        {
            LogicalName = logicalName,
            PhysicalColumns = new List<string> { physicalColumn },
            DatumCategory = DatumCategory.Measure,
            DataType = dataType,
            IsRequired = isRequired
        };
    }

    /// <summary>
    /// Creates a new DatumMapping for a metadata field.
    /// </summary>
    /// <param name="logicalName">The logical name.</param>
    /// <param name="physicalColumn">The physical column name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="isRequired">Whether the field is required.</param>
    /// <returns>A new DatumMapping instance.</returns>
    public static DatumMapping MetadataField(string logicalName, string physicalColumn, Type? dataType = null, bool isRequired = false)
    {
        return new DatumMapping
        {
            LogicalName = logicalName,
            PhysicalColumns = new List<string> { physicalColumn },
            DatumCategory = DatumCategory.Metadata,
            DataType = dataType,
            IsRequired = isRequired
        };
    }

    /// <summary>
    /// Creates a new DatumMapping for a composite field spanning multiple columns.
    /// </summary>
    /// <param name="logicalName">The logical name.</param>
    /// <param name="category">The datum category.</param>
    /// <param name="physicalColumns">The physical column names.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="isRequired">Whether the field is required.</param>
    /// <returns>A new DatumMapping instance.</returns>
    public static DatumMapping Composite(string logicalName, DatumCategory category, IEnumerable<string> physicalColumns, Type? dataType = null, bool isRequired = false)
    {
        return new DatumMapping
        {
            LogicalName = logicalName,
            PhysicalColumns = new List<string>(physicalColumns),
            DatumCategory = category,
            DataType = dataType,
            IsRequired = isRequired
        };
    }

    /// <summary>
    /// Creates a new DatumMapping for a calculated field.
    /// </summary>
    /// <param name="logicalName">The logical name.</param>
    /// <param name="category">The datum category.</param>
    /// <param name="transformExpression">The transformation expression.</param>
    /// <param name="dataType">The data type.</param>
    /// <returns>A new DatumMapping instance.</returns>
    public static DatumMapping Calculated(string logicalName, DatumCategory category, string transformExpression, Type? dataType = null)
    {
        return new DatumMapping
        {
            LogicalName = logicalName,
            PhysicalColumns = new List<string>(),
            DatumCategory = category,
            TransformExpression = transformExpression,
            DataType = dataType,
            IsReadOnly = true,
            IsRequired = false
        };
    }

    /// <summary>
    /// Gets a metadata value by key.
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
    /// Tries to get a metadata value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value if found and converted successfully.</param>
    /// <returns>True if the metadata was found and converted successfully; otherwise, false.</returns>
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
    /// Returns a string representation of the datum mapping.
    /// </summary>
    /// <returns>A string describing the datum mapping.</returns>
    public override string ToString()
    {
        var physicalInfo = IsCalculated && !string.IsNullOrWhiteSpace(TransformExpression) 
            ? $"calculated({TransformExpression})"
            : IsComposite 
                ? $"composite({string.Join(", ", PhysicalColumns)})"
                : PhysicalColumns.Count > 0 
                    ? PhysicalColumns[0]
                    : "no-storage";

        var typeInfo = DataType != null ? $" : {DataType.Name}" : "";
        return $"{LogicalName}({DatumCategory}) -> {physicalInfo}{typeInfo}";
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as DatumMapping);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(DatumMapping? other)
    {
        if (other == null)
            return false;

        return string.Equals(LogicalName, other.LogicalName, StringComparison.OrdinalIgnoreCase) &&
               DatumCategory == other.DatumCategory &&
               PhysicalColumns.SequenceEqual(other.PhysicalColumns, StringComparer.OrdinalIgnoreCase) &&
               string.Equals(TransformExpression, other.TransformExpression, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        var columnsHash = PhysicalColumns.Count > 0 
            ? PhysicalColumns.Select(c => c.ToLowerInvariant()).OrderBy(c => c).Aggregate(0, (acc, c) => acc ^ c.GetHashCode())
            : 0;

        return HashCode.Combine(
            LogicalName.ToLowerInvariant(),
            DatumCategory,
            columnsHash,
            TransformExpression?.ToLowerInvariant() ?? string.Empty);
    }

    /// <summary>
    /// Determines whether two DatumMapping instances are equal.
    /// </summary>
    /// <param name="left">The first DatumMapping to compare.</param>
    /// <param name="right">The second DatumMapping to compare.</param>
    /// <returns>True if the DatumMapping instances are equal; otherwise, false.</returns>
    public static bool operator ==(DatumMapping? left, DatumMapping? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two DatumMapping instances are not equal.
    /// </summary>
    /// <param name="left">The first DatumMapping to compare.</param>
    /// <param name="right">The second DatumMapping to compare.</param>
    /// <returns>True if the DatumMapping instances are not equal; otherwise, false.</returns>
    public static bool operator !=(DatumMapping? left, DatumMapping? right)
    {
        return !(left == right);
    }
}