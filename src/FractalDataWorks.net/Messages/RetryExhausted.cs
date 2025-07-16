using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a retry exhausted error message.
/// </summary>
[EnumOption]
public class RetryExhausted() : ServiceMessage(19, "RetryExhausted", "RETRY_EXHAUSTED", "Retry attempts exhausted after {0} tries: {1}", ServiceLevel.Error, "Operation");