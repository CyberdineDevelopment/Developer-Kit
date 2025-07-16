using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents an operation cancelled information message.
/// </summary>
[EnumOption]
public class OperationCancelled() : ServiceMessage(22, "OperationCancelled", "OPERATION_CANCELLED", "Operation '{0}' was cancelled", ServiceLevel.Information, "Operation");