using FractalDataWorks.Messages;

namespace FractalDataWorks.Connections.Stream.Messages;

/// <summary>
/// Message indicating stream connection was closed.
/// </summary>
public class StreamDisconnected : StreamMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamDisconnected"/> class.
    /// </summary>
    public StreamDisconnected() 
        : base(2, nameof(StreamDisconnected), "STREAM002", "Stream connection closed", MessageSeverity.Information) { }
}