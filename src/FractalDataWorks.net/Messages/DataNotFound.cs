using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a data not found information message.
/// </summary>
[EnumOption]
public class DataNotFound() : ServiceMessage(25, "DataNotFound", "DATA_NOT_FOUND", "Data not found: {0}", ServiceLevel.Information, "Data");