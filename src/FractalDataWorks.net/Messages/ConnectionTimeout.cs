using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a connection timeout error message.
/// </summary>
[EnumOption]
public class ConnectionTimeout() : ServiceMessage(5, "ConnectionTimeout", "CONNECTION_TIMEOUT", "Connection timed out after {0}ms", ServiceLevel.Error, "Connection");