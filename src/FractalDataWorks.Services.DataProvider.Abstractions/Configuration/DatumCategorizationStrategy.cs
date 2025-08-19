using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using FluentValidation;

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
    public Dictionary<string, object> CustomRules { get; set; } = new(StringComparer.Ordinal);

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
            return (T)Convert.ChangeType(value, typeof(T));
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

/// <summary>
/// Defines the modes for datum categorization.
/// </summary>
public enum CategorizationMode
{
    /// <summary>
    /// Use only explicit configuration mappings for categorization.
    /// </summary>
    /// <remarks>
    /// Columns without explicit configuration will use the fallback category.
    /// Most predictable but requires complete mapping configuration.
    /// </remarks>
    Configuration = 1,

    /// <summary>
    /// Use naming conventions and patterns for automatic categorization.
    /// </summary>
    /// <remarks>
    /// Applies heuristics based on column names, data types, and context.
    /// Fastest to set up but may need tuning for specific domains.
    /// </remarks>
    Convention = 2,

    /// <summary>
    /// Use configuration first, then fall back to conventions for unmapped columns.
    /// </summary>
    /// <remarks>
    /// Combines the predictability of explicit configuration with the convenience
    /// of automatic categorization for remaining columns. Recommended approach.
    /// </remarks>
    Hybrid = 3
}

/// <summary>
/// Configuration for convention-based datum categorization.
/// </summary>
public sealed class ConventionSettings
{
    /// <summary>
    /// Gets or sets the naming patterns that indicate identifier columns.
    /// </summary>
    /// <remarks>
    /// Patterns for primary keys, foreign keys, and unique identifiers.
    /// Supports case-insensitive matching with wildcards.
    /// Default patterns: "*id", "*key", "*guid", "*uuid", "*identifier"
    /// </remarks>
    public List<string> IdentifierPatterns { get; set; } = new()
    {
        "*id",
        "*_id", 
        "*key",
        "*_key",
        "*guid",
        "*_guid",
        "*uuid",
        "*_uuid",
        "*identifier",
        "*_identifier"
    };

    /// <summary>
    /// Gets or sets the naming patterns that indicate measure columns.
    /// </summary>
    /// <remarks>
    /// Patterns for numeric fields that can be aggregated (sums, counts, amounts).
    /// Default patterns: "*amount", "*total", "*count", "*qty", "*quantity", "*price", "*cost", "*value"
    /// </remarks>
    public List<string> MeasurePatterns { get; set; } = new()
    {
        "*amount",
        "*_amount",
        "*total",
        "*_total",
        "*count",
        "*_count",
        "*qty",
        "*_qty",
        "*quantity",
        "*_quantity",
        "*price",
        "*_price",
        "*cost",
        "*_cost",
        "*value",
        "*_value",
        "*rate",
        "*_rate",
        "*percent*",
        "*ratio*"
    };

    /// <summary>
    /// Gets or sets the naming patterns that indicate metadata columns.
    /// </summary>
    /// <remarks>
    /// Patterns for system fields like timestamps, audit trails, version numbers.
    /// Default patterns: "*created*", "*updated*", "*modified*", "*deleted*", "*version*", "*status*"
    /// </remarks>
    public List<string> MetadataPatterns { get; set; } = new()
    {
        "*created*",
        "*_created*",
        "*updated*",
        "*_updated*",
        "*modified*",
        "*_modified*",
        "*deleted*",
        "*_deleted*",
        "*version*",
        "*_version*",
        "*status*",
        "*_status*",
        "*audit*",
        "*_audit*",
        "*timestamp*",
        "*_timestamp*",
        "*datetime*",
        "*_datetime*",
        "*rowversion*",
        "*_rowversion*"
    };

    /// <summary>
    /// Gets or sets the data types that typically indicate measure columns.
    /// </summary>
    /// <remarks>
    /// Data types that are commonly used for numeric measures.
    /// Case-insensitive matching. Provider-specific type names.
    /// SQL examples: "decimal", "money", "float", "real", "numeric"
    /// </remarks>
    public List<string> MeasureDataTypes { get; set; } = new()
    {
        "decimal",
        "money",
        "smallmoney",
        "float",
        "real",
        "numeric",
        "number",
        "currency"
    };

    /// <summary>
    /// Gets or sets the data types that typically indicate metadata columns.
    /// </summary>
    /// <remarks>
    /// Data types commonly used for system/audit fields.
    /// SQL examples: "timestamp", "rowversion", "datetime", "datetime2"
    /// </remarks>
    public List<string> MetadataDataTypes { get; set; } = new()
    {
        "timestamp",
        "rowversion",
        "datetime",
        "datetime2",
        "datetimeoffset",
        "smalldatetime"
    };

    /// <summary>
    /// Gets or sets a value indicating whether pattern matching is case-sensitive.
    /// </summary>
    public bool CaseSensitivePatterns { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to consider data types in categorization.
    /// </summary>
    /// <remarks>
    /// When true, data type information is used alongside naming patterns
    /// to improve categorization accuracy.
    /// </remarks>
    public bool UseDataTypeHints { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to consider table context in categorization.
    /// </summary>
    /// <remarks>
    /// When true, table/container name and structure can influence categorization.
    /// For example, "amount" in an "OrderTotals" table vs. "Configurations" table.
    /// </remarks>
    public bool UseTableContext { get; set; } = true;

    /// <summary>
    /// Checks if a column name matches identifier patterns.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="tableContext">The table context.</param>
    /// <returns>True if the column matches identifier patterns.</returns>
    public bool IsIdentifierPattern(string columnName, string? dataType = null, string? tableContext = null)
    {
        return MatchesAnyPattern(columnName, IdentifierPatterns);
    }

    /// <summary>
    /// Checks if a column name matches measure patterns.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="tableContext">The table context.</param>
    /// <returns>True if the column matches measure patterns.</returns>
    public bool IsMeasurePattern(string columnName, string? dataType = null, string? tableContext = null)
    {
        var nameMatches = MatchesAnyPattern(columnName, MeasurePatterns);
        var typeMatches = UseDataTypeHints && !string.IsNullOrWhiteSpace(dataType) && 
                         MatchesAnyPattern(dataType, MeasureDataTypes);
        
        return nameMatches || typeMatches;
    }

    /// <summary>
    /// Checks if a column name matches metadata patterns.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="tableContext">The table context.</param>
    /// <returns>True if the column matches metadata patterns.</returns>
    public bool IsMetadataPattern(string columnName, string? dataType = null, string? tableContext = null)
    {
        var nameMatches = MatchesAnyPattern(columnName, MetadataPatterns);
        var typeMatches = UseDataTypeHints && !string.IsNullOrWhiteSpace(dataType) && 
                         MatchesAnyPattern(dataType, MetadataDataTypes);
        
        return nameMatches || typeMatches;
    }

    /// <summary>
    /// Checks if a column name matches property patterns (default category).
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="tableContext">The table context.</param>
    /// <returns>True if the column should be categorized as a property.</returns>
    public bool IsPropertyPattern(string columnName, string? dataType = null, string? tableContext = null)
    {
        // Property is the default category for columns that don't match other patterns
        // This method could implement specific property patterns if needed
        return true;
    }

    /// <summary>
    /// Checks if a value matches any pattern in the list.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="patterns">The patterns to match against.</param>
    /// <returns>True if the value matches any pattern.</returns>
    private bool MatchesAnyPattern(string value, List<string> patterns)
    {
        if (string.IsNullOrWhiteSpace(value) || patterns.Count == 0)
            return false;

        var comparison = CaseSensitivePatterns ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        foreach (var pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                continue;

            if (MatchesWildcardPattern(value, pattern, comparison))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a value matches a wildcard pattern (* only).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="pattern">The pattern with * wildcards.</param>
    /// <param name="comparison">The string comparison type.</param>
    /// <returns>True if the value matches the pattern.</returns>
    private static bool MatchesWildcardPattern(string value, string pattern, StringComparison comparison)
    {
        // Simple wildcard matching - * matches any sequence of characters
        if (pattern == "*")
            return true;

        if (!pattern.Contains('*'))
            return string.Equals(value, pattern, comparison);

        var parts = pattern.Split('*');
        var currentIndex = 0;

        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            
            if (string.IsNullOrEmpty(part))
                continue;

            var foundIndex = value.IndexOf(part, currentIndex, comparison);
            
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
/// Validator for DatumCategorizationStrategy.
/// </summary>
internal sealed class DatumCategorizationStrategyValidator : AbstractValidator<DatumCategorizationStrategy>
{
    public DatumCategorizationStrategyValidator()
    {
        RuleFor(x => x.Mode)
            .IsInEnum()
            .WithMessage("Categorization mode must be a valid enum value.");

        RuleFor(x => x.FallbackCategory)
            .IsInEnum()
            .WithMessage("Fallback category must be a valid enum value.");

        RuleFor(x => x.Conventions)
            .NotNull()
            .WithMessage("Convention settings cannot be null.")
            .SetValidator(new ConventionSettingsValidator());
    }
}

/// <summary>
/// Validator for ConventionSettings.
/// </summary>
internal sealed class ConventionSettingsValidator : AbstractValidator<ConventionSettings>
{
    public ConventionSettingsValidator()
    {
        RuleFor(x => x.IdentifierPatterns)
            .NotNull()
            .WithMessage("Identifier patterns cannot be null.");

        RuleFor(x => x.MeasurePatterns)
            .NotNull()
            .WithMessage("Measure patterns cannot be null.");

        RuleFor(x => x.MetadataPatterns)
            .NotNull()
            .WithMessage("Metadata patterns cannot be null.");

        RuleFor(x => x.MeasureDataTypes)
            .NotNull()
            .WithMessage("Measure data types cannot be null.");

        RuleFor(x => x.MetadataDataTypes)
            .NotNull()
            .WithMessage("Metadata data types cannot be null.");

        // Ensure patterns don't contain empty or null values
        RuleForEach(x => x.IdentifierPatterns)
            .NotEmpty()
            .WithMessage("Identifier patterns cannot contain empty values.");

        RuleForEach(x => x.MeasurePatterns)
            .NotEmpty()
            .WithMessage("Measure patterns cannot contain empty values.");

        RuleForEach(x => x.MetadataPatterns)
            .NotEmpty()
            .WithMessage("Metadata patterns cannot contain empty values.");

        RuleForEach(x => x.MeasureDataTypes)
            .NotEmpty()
            .WithMessage("Measure data types cannot contain empty values.");

        RuleForEach(x => x.MetadataDataTypes)
            .NotEmpty()
            .WithMessage("Metadata data types cannot contain empty values.");
    }
}