using System;
using System.IO;
using System.Threading.Tasks;
using FluentValidation;
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

/// <summary>
/// Validator for StreamCommand.
/// </summary>
public class StreamCommandValidator : AbstractValidator<StreamCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamCommandValidator"/> class.
    /// </summary>
    public StreamCommandValidator()
    {
        RuleFor(x => x.Operation)
            .IsInEnum()
            .WithMessage("Invalid stream operation");

        RuleFor(x => x.Data)
            .NotNull()
            .When(x => x.Operation == StreamOperation.Write)
            .WithMessage("Data is required for write operations");

        RuleFor(x => x.BufferSize)
            .GreaterThan(0)
            .When(x => x.BufferSize.HasValue)
            .WithMessage("Buffer size must be greater than 0");

        RuleFor(x => x.Position)
            .NotNull()
            .When(x => x.Operation == StreamOperation.Seek)
            .WithMessage("Position is required for seek operations");

        RuleFor(x => x.SeekOrigin)
            .NotNull()
            .IsInEnum()
            .When(x => x.Operation == StreamOperation.Seek)
            .WithMessage("SeekOrigin is required for seek operations");
    }
}

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