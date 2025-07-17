namespace FractalDataWorks.Configuration;

/// <summary>
/// Defines the types of configuration changes.
/// </summary>
public enum ConfigurationChangeType
{
    /// <summary>
    /// A configuration was added.
    /// </summary>
    Added,

    /// <summary>
    /// A configuration was updated.
    /// </summary>
    Updated,

    /// <summary>
    /// A configuration was deleted.
    /// </summary>
    Deleted,

    /// <summary>
    /// The configuration source was reloaded.
    /// </summary>
    Reloaded
}