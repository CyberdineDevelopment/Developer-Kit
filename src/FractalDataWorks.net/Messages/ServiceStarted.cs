using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a service started information message.
/// </summary>
[EnumOption]
public class ServiceStarted() : ServiceMessage(7, "ServiceStarted", "SERVICE_STARTED", "Service '{0}' started successfully", ServiceLevel.Information, "Service");