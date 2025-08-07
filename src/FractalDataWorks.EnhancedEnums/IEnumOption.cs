namespace FractalDataWorks.EnhancedEnums;

/// <summary>
/// Represents an enhanced enumeration type that provides additional functionality beyond standard enums.
/// This interface enables strongly-typed enumerations with identifiers, names, and the ability to represent an empty state.
/// </summary>
/// <typeparam name="T">The implementing type, used for self-referencing generics pattern.</typeparam>
/// <remarks>
/// Excluded from code coverage: Interface definition with no implementation.
/// </remarks>
public interface IEnumOption<T> : IEnumOption
	where T : IEnumOption<T>
{

}

/// <summary>
/// Represents an enhanced enumeration type that provides additional functionality beyond standard enums.
/// This interface enables strongly-typed enumerations with identifiers, names, and the ability to represent an empty state.
/// </summary>
/// <remarks>
/// Excluded from code coverage: Interface definition with no implementation.
/// </remarks>
public interface IEnumOption
{
	/// <summary>
	/// Gets the unique identifier for this enum value.
	/// </summary>
	int Id { get; }

	/// <summary>
	/// Gets the display name or string representation of this enum value.
	/// </summary>
	string Name { get; }
}