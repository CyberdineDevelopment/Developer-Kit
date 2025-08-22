using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

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
    public IList<string> IdentifierPatterns { get; set; } = new List<string>()
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
    public IList<string> MeasurePatterns { get; set; } = new List<string>()
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
    public IList<string> MetadataPatterns { get; set; } = new List<string>()
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
    public IList<string> MeasureDataTypes { get; set; } = new List<string>()
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
    public IList<string> MetadataDataTypes { get; set; } = new List<string>()
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
    private bool MatchesAnyPattern(string value, IList<string> patterns)
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
        if (string.Equals(pattern, "*", StringComparison.Ordinal))
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
            if (i == 0 && !pattern.StartsWith("*", StringComparison.Ordinal) && foundIndex != 0)
                return false;

            // For the last part, it must end at the end if pattern doesn't end with *
            if (i == parts.Length - 1 && !pattern.EndsWith("*", StringComparison.Ordinal) && foundIndex + part.Length != value.Length)
                return false;

            currentIndex = foundIndex + part.Length;
        }

        return true;
    }
}