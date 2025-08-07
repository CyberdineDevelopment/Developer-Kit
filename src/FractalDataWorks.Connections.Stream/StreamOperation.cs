namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Specifies the stream operation to perform.
/// </summary>
public enum StreamOperation
{
    /// <summary>
    /// Read data from the stream.
    /// </summary>
    Read,

    /// <summary>
    /// Write data to the stream.
    /// </summary>
    Write,

    /// <summary>
    /// Seek to a position in the stream.
    /// </summary>
    Seek,

    /// <summary>
    /// Get information about the stream.
    /// </summary>
    GetInfo
}