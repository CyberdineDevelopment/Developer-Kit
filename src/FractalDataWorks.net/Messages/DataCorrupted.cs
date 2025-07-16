using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a data corrupted error message.
/// </summary>
[EnumOption]
public class DataCorrupted() : ServiceMessage(26, "DataCorrupted", "DATA_CORRUPTED", "Data corruption detected in {0}", ServiceLevel.Error, "Data");