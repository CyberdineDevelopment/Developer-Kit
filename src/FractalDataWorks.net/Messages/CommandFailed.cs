using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a command failed error message.
/// </summary>
[EnumOption]
public class CommandFailed() : ServiceMessage(12, "CommandFailed", "COMMAND_FAILED", "Command '{0}' failed: {1}", ServiceLevel.Error, "Command");