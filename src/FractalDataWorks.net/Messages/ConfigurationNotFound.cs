using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Messages;

/// <summary>
/// Represents a configuration not found warning message.
/// </summary>
[EnumOption]
public class ConfigurationNotFound() : ServiceMessage(3, "ConfigurationNotFound", "CONFIG_NOT_FOUND", "Configuration not found with ID: {0}", ServiceLevel.Warning, "Configuration");