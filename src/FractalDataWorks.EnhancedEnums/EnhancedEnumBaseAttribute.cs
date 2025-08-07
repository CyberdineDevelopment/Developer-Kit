using System;

namespace FractalDataWorks.EnhancedEnums;

/// <summary>
/// Marks a class as the base type for an enhanced enum.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EnhancedEnumBaseAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the generated collection class.
    /// </summary>
    public string? CollectionName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancedEnumBaseAttribute"/> class.
    /// </summary>
    /// <param name="collectionName">The name of the generated collection class.</param>
    public EnhancedEnumBaseAttribute(string? collectionName = null)
    {
        CollectionName = collectionName;
    }
}