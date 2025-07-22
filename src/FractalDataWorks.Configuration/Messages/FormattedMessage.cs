using System;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Configuration.Messages;

/// <summary>
/// A message that has been formatted with specific data.
/// </summary>
internal sealed class FormattedMessage : IFdwMessage
{
    private readonly IFdwMessage _baseMessage;
    private readonly object[] _args;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormattedMessage"/> class.
    /// </summary>
    /// <param name="baseMessage">The base message to format.</param>
    /// <param name="args">The format arguments.</param>
    public FormattedMessage(IFdwMessage baseMessage, params object[] args)
    {
        _baseMessage = baseMessage ?? throw new ArgumentNullException(nameof(baseMessage));
        _args = args ?? Array.Empty<object>();
    }

    /// <inheritdoc/>
    public string Code => _baseMessage.Code;

    /// <inheritdoc/>
    public string Message => _baseMessage.Format(_args);

    /// <inheritdoc/>
    public MessageSeverity Severity => _baseMessage.Severity;

    /// <inheritdoc/>
    public string Format(params object[] args) => _baseMessage.Format(args);

    /// <inheritdoc/>
    public IFdwMessage WithSeverity(MessageSeverity severity)
    {
        return new FormattedMessage(_baseMessage.WithSeverity(severity), _args);
    }
}