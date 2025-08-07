namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Specifies the type of stream to create.
/// </summary>
public enum StreamType
{
    /// <summary>
    /// File-based stream.
    /// </summary>
    File,

    /// <summary>
    /// Memory-based stream.
    /// </summary>
    Memory,

    /// <summary>
    /// Network stream (future implementation).
    /// </summary>
    Network
}