using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a validation failure message.
/// </summary>
[EnumOption]
public class ValidationFailed() : ServiceMessage(1, "ValidationFailed", "VALIDATION_FAILED", "Validation failed: {0}", ServiceLevel.Warning, "Validation");