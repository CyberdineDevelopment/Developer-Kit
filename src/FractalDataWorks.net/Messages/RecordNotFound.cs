using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a record not found information message.
/// </summary>
[EnumOption]
public class RecordNotFound() : ServiceMessage(14, "RecordNotFound", "RECORD_NOT_FOUND", "{0} not found with ID: {1}", ServiceLevel.Information, "Data");