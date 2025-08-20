using System;

namespace FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;

/// <summary>
/// Marks a class for global extended enum collection generation with cross-assembly scanning.
/// This attribute enables discovery of extended enum options across all referenced assemblies,
/// allowing for distributed extended enum definitions and cross-assembly composition patterns.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class GlobalExtendedEnumCollectionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExtendedEnumCollectionAttribute"/> class.
    /// </summary>
    /// <param name="enumType">The enum type to extend globally.</param>
    /// <exception cref="ArgumentNullException">Thrown when enumType is null.</exception>
    /// <exception cref="ArgumentException">Thrown when enumType is not an enum type.</exception>
    public GlobalExtendedEnumCollectionAttribute(Type enumType)
    {
        EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
        
        if (!enumType.IsEnum)
        {
            throw new ArgumentException($"Type {enumType.Name} must be an enum type.", nameof(enumType));
        }

        CollectionName = $"Global{enumType.Name}Collection";
        NameComparison = StringComparison.Ordinal;
        GenerateFactoryMethods = true;
        GenerateStaticCollection = true;
        UseSingletonInstances = true;
        Generic = false;
    }

    /// <summary>
    /// Gets the enum type being extended globally.
    /// </summary>
    public Type EnumType { get; }

    /// <summary>
    /// Gets or sets the name of the generated global collection class.
    /// If not specified, defaults to "Global{EnumTypeName}Collection".
    /// </summary>
    public string CollectionName { get; set; }

    /// <summary>
    /// Gets or sets the string comparison type for name-based lookups.
    /// </summary>
    public StringComparison NameComparison { get; set; }

    /// <summary>
    /// Gets or sets whether to generate factory methods for creating extended enum instances.
    /// </summary>
    public bool GenerateFactoryMethods { get; set; }

    /// <summary>
    /// Gets or sets whether to generate a static collection of all extended enum values.
    /// </summary>
    public bool GenerateStaticCollection { get; set; }

    /// <summary>
    /// Gets or sets whether to use singleton instances for extended enum values.
    /// When true, the same instance is returned for each enum value.
    /// When false, new instances are created each time.
    /// </summary>
    public bool UseSingletonInstances { get; set; }

    /// <summary>
    /// Gets or sets whether to generate generic collection methods.
    /// </summary>
    public bool Generic { get; set; }

    /// <summary>
    /// Gets or sets the default return type for collection methods.
    /// If not specified, uses the base extended enum type.
    /// </summary>
    public Type? DefaultReturnType { get; set; }
}