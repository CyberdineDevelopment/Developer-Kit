using FractalDataWorks.Messages;

namespace FractalDataWorks.Connections.Stream.Messages;

/// <summary>
/// Message indicating stream connection was established.
/// </summary>
public class StreamConnected : StreamMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnected"/> class.
    /// </summary>
    public StreamConnected() 
        : base(1, nameof(StreamConnected), "STREAM001", "Stream connection established", MessageSeverity.Information) { }
}