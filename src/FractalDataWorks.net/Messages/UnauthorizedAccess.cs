using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents an unauthorized access error message.
/// </summary>
[EnumOption]
public class UnauthorizedAccess() : ServiceMessage(23, "UnauthorizedAccess", "UNAUTHORIZED_ACCESS", "Unauthorized access to {0}", ServiceLevel.Error, "Authorization");