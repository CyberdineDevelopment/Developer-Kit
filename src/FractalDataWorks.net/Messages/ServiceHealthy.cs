using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a service healthy debug message.
/// </summary>
[EnumOption]
public class ServiceHealthy() : ServiceMessage(9, "ServiceHealthy", "SERVICE_HEALTHY", "Service '{0}' is healthy", ServiceLevel.Debug, "Service");