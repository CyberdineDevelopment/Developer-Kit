namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Information about a stream.
/// </summary>
public class StreamInfo
{
    /// <summary>
    /// Gets or sets whether the stream supports reading.
    /// </summary>
    public bool CanRead { get; set; }

    /// <summary>
    /// Gets or sets whether the stream supports writing.
    /// </summary>
    public bool CanWrite { get; set; }

    /// <summary>
    /// Gets or sets whether the stream supports seeking.
    /// </summary>
    public bool CanSeek { get; set; }

    /// <summary>
    /// Gets or sets the length of the stream if seekable.
    /// </summary>
    public long? Length { get; set; }

    /// <summary>
    /// Gets or sets the current position in the stream if seekable.
    /// </summary>
    public long? Position { get; set; }

    /// <summary>
    /// Gets or sets the type of stream.
    /// </summary>
    public StreamType StreamType { get; set; }
}