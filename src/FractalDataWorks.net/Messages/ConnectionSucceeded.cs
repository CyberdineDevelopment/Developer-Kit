using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a successful connection information message.
/// </summary>
[EnumOption]
public class ConnectionSucceeded() : ServiceMessage(6, "ConnectionSucceeded", "CONNECTION_SUCCESS", "Successfully connected to {0}", ServiceLevel.Information, "Connection");