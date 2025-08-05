using System;

namespace FractalDataWorks.CodeBuilder.Types;

/// <summary>
/// Flags enumeration representing code modifiers that can be combined.
/// </summary>
[Flags]
public enum Modifiers
{
    /// <summary>No modifiers.</summary>
    None = 0,

    /// <summary>Static modifier.</summary>
    Static = 1 << 0,

    /// <summary>Abstract modifier.</summary>
    Abstract = 1 << 1,

    /// <summary>Virtual modifier.</summary>
    Virtual = 1 << 2,

    /// <summary>Override modifier.</summary>
    Override = 1 << 3,

    /// <summary>Sealed modifier.</summary>
    Sealed = 1 << 4,

    /// <summary>Partial modifier.</summary>
    Partial = 1 << 5,

    /// <summary>Readonly modifier.</summary>
    ReadOnly = 1 << 6,

    /// <summary>Const modifier.</summary>
    Const = 1 << 7,

    /// <summary>Extern modifier.</summary>
    Extern = 1 << 8,

    /// <summary>New modifier (hides inherited member).</summary>
    New = 1 << 9,

    /// <summary>Volatile modifier.</summary>
    Volatile = 1 << 10,

    /// <summary>Unsafe modifier.</summary>
    Unsafe = 1 << 11,

    /// <summary>Async modifier.</summary>
    Async = 1 << 12,

    /// <summary>Ref modifier.</summary>
    Ref = 1 << 13,

    /// <summary>Out modifier.</summary>
    Out = 1 << 14,

    /// <summary>In modifier.</summary>
    In = 1 << 15,

    /// <summary>Params modifier.</summary>
    Params = 1 << 16
}

/// <summary>
/// Extension methods for working with modifiers.
/// </summary>
public static class ModifiersExtensions
{
    /// <summary>
    /// Checks if the specified modifier is set.
    /// </summary>
    /// <param name="modifiers">The modifiers to check.</param>
    /// <param name="modifier">The modifier to check for.</param>
    /// <returns>True if the modifier is set; otherwise, false.</returns>
    public static bool HasModifier(this Modifiers modifiers, Modifiers modifier)
    {
        return (modifiers & modifier) == modifier;
    }

    /// <summary>
    /// Adds a modifier to the existing modifiers.
    /// </summary>
    /// <param name="modifiers">The existing modifiers.</param>
    /// <param name="modifier">The modifier to add.</param>
    /// <returns>The combined modifiers.</returns>
    public static Modifiers AddModifier(this Modifiers modifiers, Modifiers modifier)
    {
        return modifiers | modifier;
    }

    /// <summary>
    /// Removes a modifier from the existing modifiers.
    /// </summary>
    /// <param name="modifiers">The existing modifiers.</param>
    /// <param name="modifier">The modifier to remove.</param>
    /// <returns>The modifiers with the specified modifier removed.</returns>
    public static Modifiers RemoveModifier(this Modifiers modifiers, Modifiers modifier)
    {
        return modifiers & ~modifier;
    }

    /// <summary>
    /// Validates that the modifiers are compatible with each other.
    /// </summary>
    /// <param name="modifiers">The modifiers to validate.</param>
    /// <returns>True if the modifiers are valid; otherwise, false.</returns>
    public static bool IsValid(this Modifiers modifiers)
    {
        // Abstract and sealed cannot be combined
        if (modifiers.HasModifier(Modifiers.Abstract) && modifiers.HasModifier(Modifiers.Sealed))
            return false;

        // Abstract and static cannot be combined
        if (modifiers.HasModifier(Modifiers.Abstract) && modifiers.HasModifier(Modifiers.Static))
            return false;

        // Override and new cannot be combined
        if (modifiers.HasModifier(Modifiers.Override) && modifiers.HasModifier(Modifiers.New))
            return false;

        // Virtual and static cannot be combined
        if (modifiers.HasModifier(Modifiers.Virtual) && modifiers.HasModifier(Modifiers.Static))
            return false;

        // Override and static cannot be combined
        if (modifiers.HasModifier(Modifiers.Override) && modifiers.HasModifier(Modifiers.Static))
            return false;

        // Const and readonly cannot be combined
        if (modifiers.HasModifier(Modifiers.Const) && modifiers.HasModifier(Modifiers.ReadOnly))
            return false;

        // Ref, out, and in are mutually exclusive
        var parameterModifiers = new[] { Modifiers.Ref, Modifiers.Out, Modifiers.In };
        var parameterModifierCount = 0;
        foreach (var paramMod in parameterModifiers)
        {
            if (modifiers.HasModifier(paramMod))
                parameterModifierCount++;
        }
        if (parameterModifierCount > 1)
            return false;

        return true;
    }
}