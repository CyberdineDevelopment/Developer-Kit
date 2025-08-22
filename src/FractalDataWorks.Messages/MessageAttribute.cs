using System;

namespace FractalDataWorks;

/// <summary>
/// Attribute to mark message classes for Enhanced Enum generation.
/// Applied to concrete message classes that inherit from MessageBase to enable
/// automatic discovery and collection generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MessageAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the collection name for the generated Enhanced Enum collection.
    /// If not specified, defaults to the containing namespace or assembly name + "Messages".
    /// </summary>
    /// <value>The name of the generated static collection class.</value>
    public string? CollectionName { get; set; }

    /// <summary>
    /// Gets or sets the return type for the Enhanced Enum collection.
    /// Defaults to IFdwMessage if not specified.
    /// </summary>
    /// <value>The type returned by collection methods.</value>
    public Type? ReturnType { get; set; }

    /// <summary>
    /// Gets or sets the namespace for the return type.
    /// Used when ReturnType is specified as a string.
    /// </summary>
    /// <value>The namespace containing the return type.</value>
    public string? ReturnTypeNamespace { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this message should be included in cross-assembly collections.
    /// </summary>
    /// <value>true if the message should be discoverable across assemblies; otherwise, false.</value>
    public bool IncludeInGlobalCollection { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAttribute"/> class.
    /// </summary>
    public MessageAttribute()
    {
        ReturnType = typeof(IFdwMessage);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageAttribute"/> class with a specific collection name.
    /// </summary>
    /// <param name="collectionName">The name of the generated static collection class.</param>
    public MessageAttribute(string collectionName)
    {
        CollectionName = collectionName;
        ReturnType = typeof(IFdwMessage);
    }
}