using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents an operation timeout error message.
/// </summary>
[EnumOption]
public class OperationTimeout() : ServiceMessage(21, "OperationTimeout", "OPERATION_TIMEOUT", "Operation '{0}' timed out after {1}ms", ServiceLevel.Error, "Operation");