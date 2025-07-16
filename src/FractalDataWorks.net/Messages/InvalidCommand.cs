using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents an invalid command error message.
/// </summary>
[EnumOption]
public class InvalidCommand() : ServiceMessage(13, "InvalidCommand", "INVALID_COMMAND", "Invalid command type: {0}", ServiceLevel.Error, "Command");