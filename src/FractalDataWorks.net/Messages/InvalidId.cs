using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents an invalid ID warning message.
/// </summary>
[EnumOption]
public class InvalidId() : ServiceMessage(15, "InvalidId", "INVALID_ID", "Invalid ID: {0}", ServiceLevel.Warning, "Data");