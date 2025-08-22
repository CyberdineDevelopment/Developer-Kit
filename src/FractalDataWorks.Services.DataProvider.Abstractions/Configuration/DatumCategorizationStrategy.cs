using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

/// <summary>
/// Defines the strategy for automatically categorizing data columns into datum categories.
/// </summary>
/// <remarks>
/// Provides flexible approaches for categorizing columns as Identifier, Property, Measure, or Metadata
/// based on naming conventions, explicit configuration, or hybrid approaches. This enables automatic
/// mapping generation and reduces configuration overhead while maintaining control over categorization.
/// 
/// Categorization Modes:
/// - Configuration: Use only explicit mapping configuration
/// - Convention: Use naming patterns and data type analysis
/// - Hybrid: Try configuration first, fall back to convention
/// </remarks>
public sealed class DatumCategorizationStrategy
{
    /// <summary>
    /// Gets or sets the categorization mode to use.
    /// </summary>
    /// <remarks>
    /// Determines the primary method for categorizing columns:
    /// - Configuration: Only use explicitly configured mappings
    /// - Convention: Use naming patterns and conventions
    /// - Hybrid: Use configuration first, then conventions for unmapped columns
    /// </remarks>
    public CategorizationMode Mode { get; set; } = CategorizationMode.Hybrid;

    /// <summary>
    /// Gets or sets the convention settings for pattern-based categorization.
    /// </summary>
    /// <remarks>
    /// Applied when Mode is Convention or Hybrid. Defines naming patterns,
    /// data type rules, and other heuristics for automatic categorization.
    /// </remarks>
    public ConventionSettings Conventions { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to use attribute-based categorization.
    /// </summary>
    /// <remarks>
    /// When true, looks for categorization hints in column metadata, annotations,
    /// or provider-specific attributes. For example, SQL Server column descriptions
    /// containing category hints, or NoSQL field annotations.
    /// </remarks>
    public bool UseAttributes { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use explicit configuration mappings.
    /// </summary>
    /// <remarks>
    /// When true, explicit datum mappings in DataContainerMapping.DatumMappings
    /// are used for categorization. This setting is typically always true unless
    /// you want to rely entirely on conventions.
    /// </remarks>
    public bool UseConfiguration { get; set; } = true;

    /// <summary>
    /// Gets or sets the fallback category for columns that cannot be categorized.
    /// </summary>
    /// <remarks>
    /// When no patterns match and no explicit configuration exists, columns
    /// are assigned this default category. Property is typically the safest default.
    /// </remarks>
    public DatumCategory FallbackCategory { get; set; } = DatumCategory.Property;

    /// <summary>
    /// Gets or sets custom categorization rules as key-value pairs.
    /// </summary>
    /// <remarks>
    /// Provider-specific or domain-specific categorization rules:
    /// - "TableSpecificRules": JSON mapping of table-specific patterns
    /// - "DataTypeOverrides": Custom type-to-category mappings
    /// - "BusinessRules": Domain-specific categorization logic
    /// </remarks>
    public IDictionary<string, object> CustomRules { get; set; } = new Dictionary<string, object>(StringComparer.Ordinal);

    /// <summary>
    /// Categorizes a column based on the configured strategy.
    /// </summary>
    /// <param name="columnName">The physical column name.</param>
    /// <param name="dataType">The column data type (provider-specific).</param>
    /// <param name="tableContext">Optional table/container context for categorization.</param>
    /// <param name="explicitMapping">Explicit mapping if configured.</param>
    /// <returns>The determined datum category.</returns>
    public DatumCategory Categorize(string columnName, string? dataType = null, string? tableContext = null, DatumMapping? explicitMapping = null)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return FallbackCategory;

        // Use explicit configuration if available and enabled
        if (UseConfiguration && explicitMapping != null)
            return explicitMapping.DatumCategory;

        // Apply convention-based categorization if enabled
        if (Mode == CategorizationMode.Convention || Mode == CategorizationMode.Hybrid)
        {
            var conventionCategory = CategorizeByConvention(columnName, dataType, tableContext);
            if (conventionCategory.HasValue)
                return conventionCategory.Value;
        }

        // Use attribute-based categorization if enabled and available
        if (UseAttributes)
        {
            var attributeCategory = CategorizeByAttributes(columnName, dataType, tableContext);
            if (attributeCategory.HasValue)
                return attributeCategory.Value;
        }

        // Fall back to default category
        return FallbackCategory;
    }

    /// <summary>
    /// Categorizes a column using convention-based patterns.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="dataType">The column data type.</param>
    /// <param name="tableContext">The table context.</param>
    /// <returns>The category if determined by convention; otherwise, null.</returns>
    private DatumCategory? CategorizeByConvention(string columnName, string? dataType, string? tableContext)
    {
        // Check identifier patterns
        if (Conventions.IsIdentifierPattern(columnName, dataType, tableContext))
            return DatumCategory.Identifier;

        // Check measure patterns
        if (Conventions.IsMeasurePattern(columnName, dataType, tableContext))
            return DatumCategory.Measure;

        // Check metadata patterns
        if (Conventions.IsMetadataPattern(columnName, dataType, tableContext))
            return DatumCategory.Metadata;

        // Check property patterns (should be last as it's often the default)
        if (Conventions.IsPropertyPattern(columnName, dataType, tableContext))
            return DatumCategory.Property;

        return null;
    }

    /// <summary>
    /// Categorizes a column using attribute-based hints.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="dataType">The column data type.</param>
    /// <param name="tableContext">The table context.</param>
    /// <returns>The category if determined by attributes; otherwise, null.</returns>
    private DatumCategory? CategorizeByAttributes(string columnName, string? dataType, string? tableContext)
    {
        // This would typically inspect provider-specific metadata
        // For now, return null to indicate no attribute-based categorization
        // Concrete providers can override this behavior
        return null;
    }

    /// <summary>
    /// Gets a custom rule value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The rule key.</param>
    /// <returns>The rule value converted to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type.</exception>
    public T GetCustomRule<T>(string key)
    {
        if (!CustomRules.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Custom rule '{key}' not found.");

        if (value is T directValue)
            return directValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException($"Cannot convert custom rule '{key}' value from {value?.GetType().Name ?? "null"} to {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Tries to get a custom rule value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The rule key.</param>
    /// <param name="value">The rule value if found and converted successfully.</param>
    /// <returns>True if the rule was found and converted successfully; otherwise, false.</returns>
    public bool TryGetCustomRule<T>(string key, out T? value)
    {
        try
        {
            value = GetCustomRule<T>(key);
            return true;
        }
        catch
        {
            value = default(T);
            return false;
        }
    }
}