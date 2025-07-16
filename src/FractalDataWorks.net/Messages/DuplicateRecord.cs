using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a duplicate record warning message.
/// </summary>
[EnumOption]
public class DuplicateRecord() : ServiceMessage(16, "DuplicateRecord", "DUPLICATE_RECORD", "Duplicate {0} found: {1}", ServiceLevel.Warning, "Data");