using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a connection failure error message.
/// </summary>
[EnumOption]
public class ConnectionFailed() : ServiceMessage(4, "ConnectionFailed", "CONNECTION_FAILED", "Connection failed after {0} retries: {1}", ServiceLevel.Error, "Connection");