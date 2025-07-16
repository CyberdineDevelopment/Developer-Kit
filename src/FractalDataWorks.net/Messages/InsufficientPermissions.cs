using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents an insufficient permissions error message.
/// </summary>
[EnumOption]
public class InsufficientPermissions() : ServiceMessage(24, "InsufficientPermissions", "INSUFFICIENT_PERMISSIONS", "Insufficient permissions to perform {0}", ServiceLevel.Error, "Authorization");