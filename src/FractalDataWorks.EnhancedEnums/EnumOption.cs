using System;

namespace FractalDataWorks.EnhancedEnums;

/// <summary>
/// Base class for enhanced enum options.
/// </summary>
/// <typeparam name="TBase">The base type of the enum.</typeparam>
public abstract class EnumOption<TBase> where TBase : EnumOption<TBase>
{
    /// <summary>
    /// Gets the name of this enum option.
    /// </summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    /// Gets the display name of this enum option.
    /// </summary>
    public virtual string DisplayName => Name;

    /// <summary>
    /// Gets the ordinal value of this enum option.
    /// </summary>
    public virtual int Ordinal { get; protected set; }

    /// <summary>
    /// Returns the string representation of this enum option.
    /// </summary>
    /// <returns>The name of this enum option.</returns>
    public override string ToString() => Name;

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null) return false;
        return obj.GetType() == GetType();
    }

    /// <summary>
    /// Gets the hash code for this enum option.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => GetType().GetHashCode();
}