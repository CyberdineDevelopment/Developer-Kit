namespace FractalDataWorks.Services.DataProvider.Abstractions.Models;

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