using FluentValidation;

namespace FractalDataWorks.Connections.Stream;

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