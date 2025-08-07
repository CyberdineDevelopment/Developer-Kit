using FluentValidation;

namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Validator for StreamConnectionConfiguration.
/// </summary>
public class StreamConnectionConfigurationValidator : AbstractValidator<StreamConnectionConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnectionConfigurationValidator"/> class.
    /// </summary>
    public StreamConnectionConfigurationValidator()
    {
        RuleFor(x => x.StreamType)
            .IsInEnum()
            .WithMessage("Invalid stream type");

        RuleFor(x => x.Path)
            .NotEmpty()
            .When(x => x.StreamType == StreamType.File && x.IsEnabled)
            .WithMessage("Path is required for file streams");

        RuleFor(x => x.BufferSize)
            .GreaterThan(0)
            .WithMessage("Buffer size must be greater than 0");

        RuleFor(x => x.InitialCapacity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial capacity must be non-negative");
    }
}