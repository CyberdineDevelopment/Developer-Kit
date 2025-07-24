using FractalDataWorks.Messages;

namespace FractalDataWorks.Connections.Stream.Messages;

/// <summary>
/// Message indicating stream is not connected.
/// </summary>
public class StreamNotConnected : StreamMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamNotConnected"/> class.
    /// </summary>
    public StreamNotConnected() 
        : base(3, nameof(StreamNotConnected), "STREAM003", "Stream is not connected", MessageSeverity.Error) { }
}