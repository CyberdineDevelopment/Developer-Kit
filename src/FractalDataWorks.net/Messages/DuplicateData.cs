using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a duplicate data warning message.
/// </summary>
[EnumOption]
public class DuplicateData() : ServiceMessage(27, "DuplicateData", "DUPLICATE_DATA", "Duplicate data detected: {0}", ServiceLevel.Warning, "Data");