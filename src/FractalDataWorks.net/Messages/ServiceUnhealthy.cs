using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a service unhealthy warning message.
/// </summary>
[EnumOption]
public class ServiceUnhealthy() : ServiceMessage(10, "ServiceUnhealthy", "SERVICE_UNHEALTHY", "Service '{0}' is unhealthy: {1}", ServiceLevel.Warning, "Service");