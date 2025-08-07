using System;
using System.Diagnostics.CodeAnalysis;

namespace FractalDataWorks.EnhancedEnums.Attributes;

/// <summary>
/// Marks an enhanced enum collection for global cross-assembly discovery.
/// When this attribute is present, the source generator will scan all referenced assemblies
/// for enum options, not just the current project.
/// </summary>
/// <remarks>
/// Excluded from code coverage: Attribute class with only constructor and properties for configuration.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[ExcludeFromCodeCoverage]
public sealed class GlobalEnumCollectionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalEnumCollectionAttribute"/> class.
    /// </summary>
    public GlobalEnumCollectionAttribute()
    {
        CollectionName = null;
        GenerateFactoryMethods = true;
        GenerateStaticCollection = true;
        NameComparison = StringComparison.OrdinalIgnoreCase;
        UseSingletonInstances = true;
        ReturnType = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalEnumCollectionAttribute"/> class with all options.
    /// </summary>
    /// <param name="collectionName">The name of the generated collection class.</param>
    /// <param name="generateFactoryMethods">Whether to generate factory methods for each enum option.</param>
    /// <param name="generateStaticCollection">Whether to generate static collection properties.</param>
    /// <param name="nameComparison">The string comparison method for name-based lookups.</param>
    /// <param name="useSingletonInstances">Whether to use singleton instances for enum options.</param>
    /// <param name="returnType">The return type for collection methods.</param>
    public GlobalEnumCollectionAttribute(
        string? collectionName = null,
        bool generateFactoryMethods = true,
        bool generateStaticCollection = true,
        StringComparison nameComparison = StringComparison.OrdinalIgnoreCase,
        bool useSingletonInstances = true,
        Type? returnType = null)
    {
        CollectionName = collectionName;
        GenerateFactoryMethods = generateFactoryMethods;
        GenerateStaticCollection = generateStaticCollection;
        NameComparison = nameComparison;
        UseSingletonInstances = useSingletonInstances;
        ReturnType = returnType;
    }

    /// <summary>
    /// Gets the name of the generated collection class.
    /// If not specified, defaults to the enum base class name with 's' suffix.
    /// </summary>
    public string? CollectionName { get; }

    /// <summary>
    /// Gets whether to generate factory methods for each enum option.
    /// </summary>
    public bool GenerateFactoryMethods { get; }

    /// <summary>
    /// Gets whether to generate static collection properties (All, Count, etc.).
    /// </summary>
    public bool GenerateStaticCollection { get; }

    /// <summary>
    /// Gets the string comparison method for name-based lookups.
    /// </summary>
    public StringComparison NameComparison { get; }

    /// <summary>
    /// Gets whether to use singleton instances for enum options.
    /// When true, each enum option is instantiated once and reused.
    /// When false, new instances are created for each access.
    /// </summary>
    public bool UseSingletonInstances { get; }

    /// <summary>
    /// Gets the return type for collection methods.
    /// If not specified, the base enum type is used.
    /// </summary>
    public Type? ReturnType { get; }
}