using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents an operation failed error message.
/// </summary>
[EnumOption]
public class OperationFailed() : ServiceMessage(18, "OperationFailed", "OPERATION_FAILED", "Operation '{0}' failed: {1}", ServiceLevel.Error, "Operation");