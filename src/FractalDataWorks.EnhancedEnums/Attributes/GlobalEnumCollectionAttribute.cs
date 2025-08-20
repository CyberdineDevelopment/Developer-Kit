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
        Generic = false;
        NameComparison = StringComparison.OrdinalIgnoreCase;
        UseSingletonInstances = true;
        ReturnType = null;
        Namespace = null;
        DefaultGenericReturnType = null;
        IncludeReferencedAssemblies = true; // Always true for global collections
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalEnumCollectionAttribute"/> class with all options.
    /// </summary>
    /// <param name="collectionName">The name of the generated collection class.</param>
    /// <param name="generateFactoryMethods">Whether to generate factory methods for each enum option.</param>
    /// <param name="generateStaticCollection">Whether to generate static collection properties.</param>
    /// <param name="generic">Whether to generate a generic collection class.</param>
    /// <param name="nameComparison">The string comparison method for name-based lookups.</param>
    /// <param name="useSingletonInstances">Whether to use singleton instances for enum options.</param>
    /// <param name="returnType">The return type for collection methods.</param>
    /// <param name="namespace">The namespace for the generated collection.</param>
    /// <param name="defaultGenericReturnType">The default return type for generic enum bases.</param>
    /// <param name="includeReferencedAssemblies">Whether to include enum options from referenced assemblies (always true for global).</param>
    public GlobalEnumCollectionAttribute(
        string? collectionName = null,
        bool generateFactoryMethods = true,
        bool generateStaticCollection = true,
        bool generic = false,
        StringComparison nameComparison = StringComparison.OrdinalIgnoreCase,
        bool useSingletonInstances = true,
        Type? returnType = null,
        string? @namespace = null,
        Type? defaultGenericReturnType = null,
        bool includeReferencedAssemblies = true)
    {
        CollectionName = collectionName;
        GenerateFactoryMethods = generateFactoryMethods;
        GenerateStaticCollection = generateStaticCollection;
        Generic = generic;
        NameComparison = nameComparison;
        UseSingletonInstances = useSingletonInstances;
        ReturnType = returnType;
        Namespace = @namespace;
        DefaultGenericReturnType = defaultGenericReturnType;
        IncludeReferencedAssemblies = includeReferencedAssemblies;
    }

    /// <summary>
    /// Gets or sets the name of the generated collection class.
    /// If not specified, defaults to the enum base class name with 's' suffix.
    /// </summary>
    public string? CollectionName { get; set; }

    /// <summary>
    /// Gets or sets whether to generate factory methods for each enum option.
    /// </summary>
    public bool GenerateFactoryMethods { get; set; }

    /// <summary>
    /// Gets or sets whether to generate static collection properties (All, Count, etc.).
    /// </summary>
    public bool GenerateStaticCollection { get; set; }

    /// <summary>
    /// Gets or sets the string comparison method for name-based lookups.
    /// </summary>
    public StringComparison NameComparison { get; set; }

    /// <summary>
    /// Gets or sets whether to use singleton instances for enum options.
    /// When true, each enum option is instantiated once and reused.
    /// When false, new instances are created for each access.
    /// </summary>
    public bool UseSingletonInstances { get; set; }

    /// <summary>
    /// Gets or sets the return type for collection methods.
    /// If not specified, the base enum type is used.
    /// </summary>
    public Type? ReturnType { get; set; }

    /// <summary>
    /// Gets or sets whether to generate a generic collection class.
    /// When true, generates Collections&lt;T&gt; where T : BaseType.
    /// When false, generates non-generic Collections class only.
    /// Defaults to false.
    /// </summary>
    public bool Generic { get; set; }

    /// <summary>
    /// Gets or sets the namespace for the generated collection.
    /// If not specified, uses the base class namespace.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the default return type for generic enum bases.
    /// </summary>
    public Type? DefaultGenericReturnType { get; set; }

    /// <summary>
    /// Gets or sets whether to include enum options from referenced assemblies.
    /// Note: For GlobalEnumCollection this is always true by design, but included for consistency.
    /// </summary>
    public bool IncludeReferencedAssemblies { get; set; }

}