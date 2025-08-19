namespace FractalDataWorks.Services.DataProvider.Abstractions.Configuration;

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