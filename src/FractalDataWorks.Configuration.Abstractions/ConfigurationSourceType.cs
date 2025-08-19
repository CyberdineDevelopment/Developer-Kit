namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Defines the types of configuration sources.
/// </summary>
public enum ConfigurationSourceType
{
    /// <summary>
    /// Configuration from a file.
    /// </summary>
    File,

    /// <summary>
    /// Configuration from environment variables.
    /// </summary>
    Environment,

    /// <summary>
    /// Configuration from a database.
    /// </summary>
    Database,

    /// <summary>
    /// Configuration from a remote service.
    /// </summary>
    Remote,

    /// <summary>
    /// Configuration from memory/cache.
    /// </summary>
    Memory,

    /// <summary>
    /// Configuration from command line arguments.
    /// </summary>
    CommandLine,

    /// <summary>
    /// Custom configuration source.
    /// </summary>
    Custom
}