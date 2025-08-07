using System;

namespace FractalDataWorks.EnhancedEnums;

/// <summary>
/// Marks a class as an option in an enhanced enum.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EnumOptionAttribute : Attribute
{
    /// <summary>
    /// Gets the display name for this enum option.
    /// </summary>
    public string? DisplayName { get; }

    /// <summary>
    /// Gets the value for this enum option.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumOptionAttribute"/> class.
    /// </summary>
    /// <param name="displayName">The display name for this enum option.</param>
    /// <param name="value">The value for this enum option.</param>
    public EnumOptionAttribute(string? displayName = null, object? value = null)
    {
        DisplayName = displayName;
        Value = value;
    }
}