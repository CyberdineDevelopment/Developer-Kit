using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a command executed debug message.
/// </summary>
[EnumOption]
public class CommandExecuted() : ServiceMessage(11, "CommandExecuted", "COMMAND_EXECUTED", "Command '{0}' executed successfully in {1}ms", ServiceLevel.Debug, "Command");