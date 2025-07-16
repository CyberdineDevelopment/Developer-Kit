using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents an invalid configuration error message.
/// </summary>
[EnumOption]
public class InvalidConfiguration() : ServiceMessage(2, "InvalidConfiguration", "INVALID_CONFIG", "Invalid configuration: {0}", ServiceLevel.Error, "Configuration");