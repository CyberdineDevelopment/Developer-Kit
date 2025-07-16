
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Base class for service messages using the enhanced enum pattern.
/// </summary>
[EnhancedEnumOption("ServiceMessages")]
public abstract class ServiceMessage 
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceMessage"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this message.</param>
    /// <param name="name">The name of this message type.</param>
    /// <param name="code">The unique code for this message type.</param>
    /// <param name="message">The default message text.</param>
    /// <param name="level">The severity level of this message.</param>
    /// <param name="category">The category of this message.</param>
    protected ServiceMessage(int id, string name, string code, string message, ServiceLevel level, string category)
    {
        Id = id;
        Name = name;
        Code = code;
        Message = message;
        Level = level;
        Category = category;
    }

    /// <summary>
    /// Gets the unique code for this message type.
    /// </summary>
    [EnumLookup]
    public string Code { get; }

    /// <summary>
    /// Gets the default message text.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the severity level of this message.
    /// </summary>
    public ServiceLevel Level { get; }

    /// <summary>
    /// Gets the category of this message.
    /// </summary>
    [EnumLookup]
    public string Category { get; }

    /// <summary>
    /// Formats the message with the provided parameters.
    /// </summary>
    /// <param name="parameters">The parameters to format the message with.</param>
    /// <returns>The formatted message.</returns>
    public virtual string Format(params object[] parameters)
    {
        return parameters?.Length > 0 
            ? string.Format(Message, parameters) 
            : Message;
    }

    #region Implementation of IEnhancedEnumOption
    
    /// <summary>Gets the unique identifier for this enum value.</summary>
    public int Id { get; }

    /// <summary>
    /// Gets the display name or string representation of this enum value.
    /// </summary>
    public string Name { get; }

    #endregion
}