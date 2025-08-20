using System;

namespace FractalDataWorks.EnhancedEnums.ExtendedEnums;

/// <summary>
/// Base class for extended enum types that wrap existing C# enums with additional functionality.
/// This base class allows extending standard enums with rich behavior while maintaining the original enum values.
/// </summary>
/// <typeparam name="T">The derived extended enum type.</typeparam>
/// <typeparam name="TEnum">The underlying enum type being extended.</typeparam>
public abstract class ExtendedEnumOptionBase<T, TEnum> : IEnumOption
    where T : ExtendedEnumOptionBase<T, TEnum>
    where TEnum : struct, Enum
{
    /// <summary>
    /// Gets the unique identifier for this enum value, derived from the underlying enum value.
    /// </summary>
    public int Id => (int)(object)EnumValue;

    /// <summary>
    /// Gets the name of this enum value, derived from the underlying enum value.
    /// </summary>
    public string Name => EnumValue.ToString();

    /// <summary>
    /// Gets the underlying enum value that this extended enum wraps.
    /// </summary>
    public TEnum EnumValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedEnumOptionBase{T, TEnum}"/> class.
    /// </summary>
    /// <param name="enumValue">The underlying enum value to wrap.</param>
    protected ExtendedEnumOptionBase(TEnum enumValue)
    {
        EnumValue = enumValue;
    }

    /// <summary>
    /// Implicitly converts the extended enum option to its underlying enum value.
    /// </summary>
    /// <param name="extendedEnum">The extended enum option to convert.</param>
    public static implicit operator TEnum(ExtendedEnumOptionBase<T, TEnum> extendedEnum)
    {
        return extendedEnum.EnumValue;
    }

    /// <summary>
    /// Returns a string representation of this extended enum option.
    /// </summary>
    /// <returns>The name of the enum value.</returns>
    public override string ToString() => Name;

    /// <summary>
    /// Determines whether the specified object is equal to the current extended enum option.
    /// </summary>
    /// <param name="obj">The object to compare with the current extended enum option.</param>
    /// <returns>True if the specified object is equal to the current extended enum option; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            ExtendedEnumOptionBase<T, TEnum> other => EnumValue.Equals(other.EnumValue),
            TEnum enumValue => EnumValue.Equals(enumValue),
            _ => false
        };
    }

    /// <summary>
    /// Returns the hash code for this extended enum option.
    /// </summary>
    /// <returns>A hash code for the current extended enum option.</returns>
    public override int GetHashCode() => EnumValue.GetHashCode();
}