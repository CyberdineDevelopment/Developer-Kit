using FractalDataWorks.Messages;

namespace FractalDataWorks.Connections.Stream.Messages;

/// <summary>
/// Message indicating a stream operation failed.
/// </summary>
public class StreamOperationFailed : StreamMessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamOperationFailed"/> class.
    /// </summary>
    public StreamOperationFailed() 
        : base(4, nameof(StreamOperationFailed), "STREAM004", "Stream operation failed: {0}", MessageSeverity.Error) { }
}