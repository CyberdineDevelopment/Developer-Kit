using System;
using System.IO;
using System.Threading.Tasks;
using FractalDataWorks.Validation;

namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Command for stream operations.
/// </summary>
public class StreamCommand : ICommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamCommand"/> class.
    /// </summary>
    public StreamCommand()
    {
        CommandId = Guid.NewGuid();
        CorrelationId = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <inheritdoc/>
    public Guid CommandId { get; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset Timestamp { get; }

    /// <inheritdoc/>
    public IFdwConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets or sets the operation to perform.
    /// </summary>
    public StreamOperation Operation { get; set; }

    /// <summary>
    /// Gets or sets the data for write operations.
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    /// Gets or sets the buffer size for read operations.
    /// </summary>
    public int? BufferSize { get; set; }

    /// <summary>
    /// Gets or sets the position for seek operations.
    /// </summary>
    public long? Position { get; set; }

    /// <summary>
    /// Gets or sets the seek origin for seek operations.
    /// </summary>
    public SeekOrigin? SeekOrigin { get; set; }

    /// <inheritdoc/>
    public async Task<IValidationResult> Validate()
    {
        var validator = new StreamCommandValidator();
        var result = await validator.ValidateAsync(this);
        return new ValidationResultAdapter(result);
    }
}