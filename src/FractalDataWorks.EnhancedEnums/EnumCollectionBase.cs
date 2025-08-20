using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace FractalDataWorks.EnhancedEnums;

/// <summary>
/// Base class for enum collections that provides core functionality.
/// Classes decorated with [EnumCollection] or [GlobalEnumCollection] attributes 
/// can optionally inherit from this base class to get standard collection methods
/// like GetById(), GetByName(), and TryGetByName() without code generation.
/// The source generator will populate the static collection in the static constructor.
/// </summary>
/// <typeparam name="T">The enum option type that must derive from EnumOptionBase&lt;T&gt;</typeparam>
public abstract class EnumCollectionBase<T> where T : EnumOptionBase<T>
{

    /// <summary>
    /// Static collection of all enum options. Populated by the source generator.
    /// </summary>
    #pragma warning disable CA2211, MA0069,CA1707, MA0051
    protected static ImmutableArray<T> _all = [];

    /// <summary>
    /// Static empty instance. Populated by the source generator.
    /// </summary>
    protected static T _empty = default!;
    
#if NET8_0_OR_GREATER
    /// <summary>
    /// Lookup dictionary for enum options by name (case-insensitive). Populated when first accessed.
    /// </summary>
    private static FrozenDictionary<string, T>? _lookupByName;
    
    /// <summary>
    /// Lookup dictionary for enum options by ID. Populated when first accessed.
    /// </summary>
    private static FrozenDictionary<int, T>? _lookupById;
#else
    /// <summary>
    /// Lookup dictionary for enum options by name (case-insensitive). Populated when first accessed.
    /// </summary>
    private static ReadOnlyDictionary<string, T>? _lookupByName;
    
    /// <summary>
    /// Lookup dictionary for enum options by ID. Populated when first accessed.
    /// </summary>
    private static ReadOnlyDictionary<int, T>? _lookupById;
#endif
    #pragma warning restore CA2211, MA0069,CA1707, MA0051
    
    /// <summary>
    /// Ensures lookup dictionaries are initialized for fast O(1) lookups.
    /// </summary>
    private static void EnsureLookupsInitialized()
    {
        if (_lookupByName != null) return;
        
#if NET8_0_OR_GREATER
        _lookupByName = _all.ToFrozenDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _lookupById = _all.ToFrozenDictionary(x => x.Id);
#else
        _lookupByName = new ReadOnlyDictionary<string, T>(
            _all.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase));
        _lookupById = new ReadOnlyDictionary<int, T>(
            _all.ToDictionary(x => x.Id));
#endif
    }
    
    /// <summary>
    /// Gets all enum options in this collection.
    /// </summary>
    public static ImmutableArray<T> All() => _all;
    
    /// <summary>
    /// Gets an empty instance of the enum option type.
    /// </summary>
    public static T Empty() => _empty;
    
    /// <summary>
    /// Gets an enum option by name (case-insensitive).
    /// </summary>
    public static T? GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        
        EnsureLookupsInitialized();
        return _lookupByName!.TryGetValue(name, out var value) ? value : null;
    }
    
    /// <summary>
    /// Gets an enum option by ID.
    /// </summary>
    public static T? GetById(int id)
    {
        EnsureLookupsInitialized();
        return _lookupById!.TryGetValue(id, out var value) ? value : null;
    }
    
    /// <summary>
    /// Tries to get an enum option by name
    /// </summary>
    public static bool TryGetByName(string name, out T? value)
    {
        value = GetByName(name);
        return value != null;
    }
    
    /// <summary>
    /// Tries to get an enum option by ID
    /// </summary>
    public static bool TryGetById(int id, out T? value)
    {
        value = GetById(id);
        return value != null;
    }
    
    /// <summary>
    /// Gets all enum options as an enumerable
    /// </summary>
    public static IEnumerable<T> AsEnumerable() => _all;
    
    /// <summary>
    /// Gets the count of enum options
    /// </summary>
    public static int Count => _all.Length;
    
    /// <summary>
    /// Checks if the collection contains any items
    /// </summary>
    public static bool Any() => _all.Length > 0;
    
    /// <summary>
    /// Gets an enum option by index
    /// </summary>
    public static T GetByIndex(int index) => _all[index];
}

