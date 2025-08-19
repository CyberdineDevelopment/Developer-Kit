using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using FluentValidation;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Defines the mapping between a logical data container and its physical storage representation.
/// </summary>
/// <remarks>
/// Maps universal data model containers (e.g., "Customer", "Order") to physical storage structures
/// such as database tables, file paths, API endpoints, or NoSQL collections. Includes column-level
/// mappings for individual data fields within the container.
/// 
/// Examples:
/// - SQL: LogicalName="Customer" -> PhysicalPath="dbo.Customers"
/// - File: LogicalName="Customer" -> PhysicalPath="/data/customers.json"
/// - API: LogicalName="Customer" -> PhysicalPath="/api/v1/customers"
/// - NoSQL: LogicalName="Customer" -> PhysicalPath="customers" (collection name)
/// </remarks>
public sealed class DataContainerMapping : IEquatable<DataContainerMapping>
{
    /// <summary>
    /// Gets or sets the logical name of the data container.
    /// </summary>
    /// <remarks>
    /// This is the universal name used throughout the application to reference this container.
    /// Should follow consistent naming conventions regardless of the underlying storage system.
    /// Examples: "Customer", "Order", "Product", "OrderLineItem"
    /// </remarks>
    public string LogicalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the physical path or identifier in the storage system.
    /// </summary>
    /// <remarks>
    /// Storage-specific path to the actual data container:
    /// - SQL Database: "schema.table" (e.g., "dbo.Customers", "sales.Orders")
    /// - File System: "/path/to/file.ext" (e.g., "/data/customers.json", "\\files\\customers.csv")
    /// - REST API: "/resource/path" (e.g., "/api/v1/customers", "/customers")
    /// - NoSQL: "collection_name" (e.g., "customers", "orders")
    /// - SFTP: "/remote/path/file.ext" (e.g., "/exports/customers.xml")
    /// </remarks>
    public string PhysicalPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of container in the physical storage system.
    /// </summary>
    /// <remarks>
    /// Indicates what kind of structure this mapping represents in the target system.
    /// Used by providers to determine appropriate access patterns and capabilities.
    /// </remarks>
    public ContainerType ContainerType { get; set; } = ContainerType.Table;

    /// <summary>
    /// Gets or sets the datum mappings for individual fields within this container.
    /// </summary>
    /// <remarks>
    /// Maps logical field names to physical column names or storage locations.
    /// Key is the logical datum name, value is the datum mapping configuration.
    /// If a logical field is not found in this dictionary, convention-based mapping
    /// may be applied depending on the categorization strategy.
    /// </remarks>
    public Dictionary<string, DatumMapping> DatumMappings { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets a value indicating whether to enable automatic discovery of unmapped fields.
    /// </summary>
    /// <remarks>
    /// When true, the provider will automatically discover physical columns/fields that don't
    /// have explicit datum mappings and attempt to map them using the categorization strategy.
    /// Useful for rapid development and when working with evolving schemas.
    /// </remarks>
    public bool EnableAutoDiscovery { get; set; } = true;

    /// <summary>
    /// Gets or sets additional metadata about this container mapping.
    /// </summary>
    /// <remarks>
    /// Storage for provider-specific configuration or custom properties:
    /// - SQL: "PrimaryKey", "Indexes", "Partitioning"
    /// - File: "Encoding", "Delimiter", "HeaderRow"
    /// - API: "AuthRequired", "RateLimit", "CachePolicy"
    /// - NoSQL: "ShardKey", "IndexHints", "ConsistencyLevel"
    /// </remarks>
    public Dictionary<string, object> Metadata { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the read access configuration for this container.
    /// </summary>
    /// <remarks>
    /// Defines how data can be read from this container including filtering,
    /// sorting, paging, and projection capabilities. Null means use provider defaults.
    /// </remarks>
    public ContainerAccessConfiguration? ReadAccess { get; set; }

    /// <summary>
    /// Gets or sets the write access configuration for this container.
    /// </summary>
    /// <remarks>
    /// Defines how data can be written to this container including insert, update,
    /// delete capabilities and any constraints. Null means use provider defaults.
    /// </remarks>
    public ContainerAccessConfiguration? WriteAccess { get; set; }

    /// <summary>
    /// Finds a datum mapping by logical name.
    /// </summary>
    /// <param name="logicalName">The logical datum name to find.</param>
    /// <returns>The datum mapping if found; otherwise, null.</returns>
    public DatumMapping? FindDatumMapping(string logicalName)
    {
        if (string.IsNullOrWhiteSpace(logicalName))
            return null;

        return DatumMappings.TryGetValue(logicalName, out var mapping) ? mapping : null;
    }

    /// <summary>
    /// Gets all datum mappings of a specific category.
    /// </summary>
    /// <param name="category">The datum category to filter by.</param>
    /// <returns>All datum mappings of the specified category.</returns>
    public IEnumerable<DatumMapping> GetDatumMappingsByCategory(DatumCategory category)
    {
        return DatumMappings.Values.Where(mapping => mapping.DatumCategory == category);
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
    /// Returns a string representation of the container mapping.
    /// </summary>
    /// <returns>A string describing the container mapping.</returns>
    public override string ToString()
    {
        var mappingCount = DatumMappings.Count;
        var mappingInfo = mappingCount > 0 ? $" ({mappingCount} datum mappings)" : "";
        return $"{LogicalName} -> {PhysicalPath} ({ContainerType}){mappingInfo}";
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as DataContainerMapping);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(DataContainerMapping? other)
    {
        if (other == null)
            return false;

        return string.Equals(LogicalName, other.LogicalName, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(PhysicalPath, other.PhysicalPath, StringComparison.OrdinalIgnoreCase) &&
               ContainerType == other.ContainerType;
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            LogicalName.ToLowerInvariant(),
            PhysicalPath.ToLowerInvariant(),
            ContainerType);
    }

    /// <summary>
    /// Determines whether two DataContainerMapping instances are equal.
    /// </summary>
    /// <param name="left">The first DataContainerMapping to compare.</param>
    /// <param name="right">The second DataContainerMapping to compare.</param>
    /// <returns>True if the DataContainerMapping instances are equal; otherwise, false.</returns>
    public static bool operator ==(DataContainerMapping? left, DataContainerMapping? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two DataContainerMapping instances are not equal.
    /// </summary>
    /// <param name="left">The first DataContainerMapping to compare.</param>
    /// <param name="right">The second DataContainerMapping to compare.</param>
    /// <returns>True if the DataContainerMapping instances are not equal; otherwise, false.</returns>
    public static bool operator !=(DataContainerMapping? left, DataContainerMapping? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Configuration for container access capabilities and constraints.
/// </summary>
public sealed class ContainerAccessConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether this access type is supported.
    /// </summary>
    public bool Supported { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of records that can be processed in a single operation.
    /// </summary>
    /// <remarks>
    /// For reads: maximum result set size before paging is required.
    /// For writes: maximum batch size for bulk operations.
    /// Set to 0 for no limit.
    /// </remarks>
    public int MaxRecords { get; set; } = 0;

    /// <summary>
    /// Gets or sets the timeout in seconds for operations of this type.
    /// </summary>
    /// <remarks>
    /// Override for the default timeout specified in DataStoreConfiguration.
    /// Set to 0 to use the default timeout.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 0;

    /// <summary>
    /// Gets or sets additional constraints or capabilities for this access type.
    /// </summary>
    /// <remarks>
    /// Provider-specific access configuration:
    /// - SQL: "AllowDirtyReads", "RequireTransaction", "SupportsCTE"
    /// - File: "AppendOnly", "RequiresLocking", "SupportsStreaming"
    /// - API: "RequiresAuth", "SupportsETag", "RateLimit"
    /// </remarks>
    public Dictionary<string, object> Properties { get; set; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Validator for DataContainerMapping.
/// </summary>
internal sealed class DataContainerMappingValidator : AbstractValidator<DataContainerMapping>
{
    public DataContainerMappingValidator()
    {
        RuleFor(x => x.LogicalName)
            .NotEmpty()
            .WithMessage("Logical name is required.")
            .MaximumLength(100)
            .WithMessage("Logical name cannot exceed 100 characters.");

        RuleFor(x => x.PhysicalPath)
            .NotEmpty()
            .WithMessage("Physical path is required.")
            .MaximumLength(500)
            .WithMessage("Physical path cannot exceed 500 characters.");

        // Validate datum mappings have unique logical names
        RuleFor(x => x.DatumMappings)
            .Must(HaveUniqueLogicalNames)
            .WithMessage("Datum mappings must have unique logical names.");

        // Validate each datum mapping
        RuleForEach(x => x.DatumMappings.Values)
            .SetValidator(new DatumMappingValidator());

        // Validate access configurations
        RuleFor(x => x.ReadAccess)
            .SetValidator(new ContainerAccessConfigurationValidator())
            .When(x => x.ReadAccess != null);

        RuleFor(x => x.WriteAccess)
            .SetValidator(new ContainerAccessConfigurationValidator())
            .When(x => x.WriteAccess != null);
    }

    private static bool HaveUniqueLogicalNames(Dictionary<string, DatumMapping> mappings)
    {
        if (mappings.Count == 0)
            return true;

        // Keys should already be unique in a dictionary, but check for case-insensitive duplicates
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in mappings.Keys)
        {
            if (!names.Add(key))
                return false;
        }
        return true;
    }
}

/// <summary>
/// Validator for ContainerAccessConfiguration.
/// </summary>
internal sealed class ContainerAccessConfigurationValidator : AbstractValidator<ContainerAccessConfiguration>
{
    public ContainerAccessConfigurationValidator()
    {
        RuleFor(x => x.MaxRecords)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum records must be non-negative.");

        RuleFor(x => x.TimeoutSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Timeout seconds must be non-negative.");
    }
}