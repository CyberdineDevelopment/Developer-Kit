using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a service stopped information message.
/// </summary>
[EnumOption]
public class ServiceStopped() : ServiceMessage(8, "ServiceStopped", "SERVICE_STOPPED", "Service '{0}' stopped", ServiceLevel.Information, "Service");