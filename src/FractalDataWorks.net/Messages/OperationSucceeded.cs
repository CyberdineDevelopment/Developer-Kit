using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a successful operation information message.
/// </summary>
[EnumOption]
public class OperationSucceeded() : ServiceMessage(17, "OperationSucceeded", "OPERATION_SUCCESS", "Operation '{0}' completed successfully", ServiceLevel.Information, "Operation");