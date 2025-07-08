namespace FractalDataWorks;

/// <summary>
/// Base interface for Enhanced Enums.
/// Enhanced Enums provide type-safe, extensible enumeration with automatic discovery.
/// </summary>
public interface IEnhancedEnum
{
    /// <summary>
    /// Gets the name of this enum option
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the display order for this enum option
    /// </summary>
    int Order { get; }
}

/// <summary>
/// Generic Enhanced Enum interface with type parameter
/// </summary>
/// <typeparam name="T">The concrete enum type</typeparam>
public interface IEnhancedEnum<T> : IEnhancedEnum where T : IEnhancedEnum<T>
{
    /// <summary>
    /// Creates an empty/invalid instance of this enum type
    /// </summary>
    /// <returns>An empty instance</returns>
    T Empty();
}