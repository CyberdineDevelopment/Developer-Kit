using System;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks;

/// <summary>
/// Base class for all messages in the FractalDataWorks system using Enhanced Enums pattern.
/// </summary>
[EnumCollection(CollectionName = "Messages", ReturnType = typeof(IFdwMessage))]
public abstract class MessageBase : EnumOptionBase<MessageBase>, IFdwMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBase"/> class.
    /// </summary>
    /// <param name="id">The message id.</param>
    /// <param name="name">The name for enhanced enum lookups.</param>
    /// <param name="code">The message code.</param>
    /// <param name="message">The message template.</param>
    /// <param name="severity">The message severity.</param>
    protected MessageBase(int id, string name, string code, string message, MessageSeverity severity = MessageSeverity.Information)
        : base(id, name)
    {
        Code = code;
        Message = message;
        Severity = severity;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <inheritdoc/>
    public string Code { get; init; }

    /// <inheritdoc/>
    public string Message { get; init; }

    /// <inheritdoc/>
    public MessageSeverity Severity { get; private set; }

    /// <inheritdoc/>
    public string? Source { get; set; }

    /// <summary>
    /// Gets the timestamp when this message was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets or sets additional details about this message.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets additional data associated with this message.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Formats the message with the specified arguments.
    /// </summary>
    /// <param name="args">The arguments to format the message with.</param>
    /// <returns>The formatted message string.</returns>
    public virtual string Format(params object[] args)
        => args?.Length > 0 ? string.Format(System.Globalization.CultureInfo.InvariantCulture, Message, args) : Message;

    /// <summary>
    /// Creates a copy of this message with a different severity level.
    /// </summary>
    /// <param name="severity">The new severity level.</param>
    /// <returns>A new message instance with the specified severity.</returns>
    public virtual IFdwMessage WithSeverity(MessageSeverity severity)
    {
        var clone = (MessageBase)MemberwiseClone();
        clone.Severity = severity;
        return clone;
    }

    /// <summary>
    /// Returns a string representation of this message.
    /// </summary>
    /// <returns>A string containing the code and message.</returns>
    public override string ToString() => $"[{Code}] {Message}";

    /// <summary>
    /// Determines whether the specified object is equal to the current message.
    /// Messages are considered equal if they have the same code.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is MessageBase other && string.Equals(Code, other.Code, StringComparison.Ordinal);

    /// <summary>
    /// Returns a hash code for this message based on its code.
    /// </summary>
    public override int GetHashCode() => Code?.GetHashCode() ?? 0;
}