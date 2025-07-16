using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a service degraded warning message.
/// </summary>
[EnumOption]
public class ServiceDegraded() : ServiceMessage(20, "ServiceDegraded", "SERVICE_DEGRADED", "Service '{0}' is running in degraded mode: {1}", ServiceLevel.Warning, "Service");