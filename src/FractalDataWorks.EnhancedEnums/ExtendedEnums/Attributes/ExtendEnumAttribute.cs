using System;

namespace FractalDataWorks.EnhancedEnums.ExtendedEnums.Attributes;

/// <summary>
/// Marks a class as extending an existing C# enum with additional functionality.
/// When applied to a class, the source generator will automatically create wrapper classes
/// for all values of the specified enum type and generate a collection for them.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ExtendEnumAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendEnumAttribute"/> class.
    /// </summary>
    /// <param name="enumType">The enum type to extend.</param>
    /// <exception cref="ArgumentNullException">Thrown when enumType is null.</exception>
    /// <exception cref="ArgumentException">Thrown when enumType is not an enum type.</exception>
    public ExtendEnumAttribute(Type enumType)
    {
        EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
        
        if (!enumType.IsEnum)
        {
            throw new ArgumentException($"Type {enumType.Name} must be an enum type.", nameof(enumType));
        }

        CollectionName = $"{enumType.Name}Collection";
        NameComparison = StringComparison.Ordinal;
        GenerateFactoryMethods = true;
        GenerateStaticCollection = true;
        UseSingletonInstances = true;
        IncludeReferencedAssemblies = false;
        Generic = false;
    }

    /// <summary>
    /// Gets the enum type being extended.
    /// </summary>
    public Type EnumType { get; }

    /// <summary>
    /// Gets or sets the name of the generated collection class.
    /// If not specified, defaults to "{EnumTypeName}Collection".
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
    /// Gets or sets whether to scan referenced assemblies for additional extended enum options.
    /// When true, enables cross-assembly composition of extended enum values.
    /// </summary>
    public bool IncludeReferencedAssemblies { get; set; }

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