using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions;

/// <summary>
/// Interface for provider metadata in the FractalDataWorks framework.
/// Provides information about connection provider capabilities and characteristics.
/// </summary>
/// <remarks>
/// Provider metadata helps the framework make informed decisions about provider
/// selection, feature compatibility, and performance optimization.
/// </remarks>
public interface IProviderMetadata
{
    /// <summary>
    /// Gets the name of the connection provider.
    /// </summary>
    /// <value>The provider name (e.g., "SQL Server Provider", "PostgreSQL Provider").</value>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the version of the provider implementation.
    /// </summary>
    /// <value>The provider version string.</value>
    string Version { get; }
    
    /// <summary>
    /// Gets the underlying driver or library version.
    /// </summary>
    /// <value>The driver version string, or null if not applicable.</value>
    string? DriverVersion { get; }
    
    /// <summary>
    /// Gets the supported protocol versions.
    /// </summary>
    /// <value>A collection of protocol version strings supported by this provider.</value>
    /// <remarks>
    /// Protocol versions may include database protocol versions (e.g., "TDS 7.4"),
    /// API versions (e.g., "REST v2"), or other relevant protocol information.
    /// </remarks>
    IReadOnlyList<string> SupportedProtocolVersions { get; }
    
    /// <summary>
    /// Gets the performance characteristics of this provider.
    /// </summary>
    /// <value>A dictionary of performance metric names and their values.</value>
    /// <remarks>
    /// Performance characteristics may include connection pool sizes, timeout values,
    /// batch size limits, or other metrics relevant for performance tuning.
    /// Common keys include "MaxConnections", "DefaultTimeout", "MaxBatchSize".
    /// </remarks>
    IReadOnlyDictionary<string, object> PerformanceCharacteristics { get; }
    
    /// <summary>
    /// Gets the feature flags indicating which capabilities are supported.
    /// </summary>
    /// <value>A dictionary of feature names and their support status.</value>
    /// <remarks>
    /// Feature flags help the framework determine what operations are available
    /// with this provider. Common features include "Transactions", "BulkOperations",
    /// "Streaming", "Encryption", "Compression".
    /// Values are typically boolean but may include version strings or capability levels.
    /// </remarks>
    IReadOnlyDictionary<string, object> FeatureFlags { get; }
    
    /// <summary>
    /// Gets the security features supported by this provider.
    /// </summary>
    /// <value>A collection of security feature names supported by this provider.</value>
    /// <remarks>
    /// Security features may include authentication methods, encryption protocols,
    /// certificate validation, and other security-related capabilities.
    /// Examples: "IntegratedAuth", "SSL", "CertificateAuth", "TokenAuth".
    /// </remarks>
    IReadOnlyList<string> SecurityFeatures { get; }
    
    /// <summary>
    /// Gets the limitations or restrictions of this provider.
    /// </summary>
    /// <value>A dictionary of limitation names and their values.</value>
    /// <remarks>
    /// Limitations help the framework understand provider constraints and work around them.
    /// Common limitations include "MaxQueryLength", "MaxParameterCount", "MaxConcurrentOperations".
    /// Values may be numeric limits, boolean restrictions, or descriptive strings.
    /// </remarks>
    IReadOnlyDictionary<string, object> Limitations { get; }
    
    /// <summary>
    /// Gets additional custom metadata specific to this provider.
    /// </summary>
    /// <value>A dictionary of custom metadata properties.</value>
    /// <remarks>
    /// Custom metadata allows providers to expose additional information that may be
    /// relevant for specific use cases or advanced configuration scenarios.
    /// </remarks>
    IReadOnlyDictionary<string, object> CustomMetadata { get; }
    
    /// <summary>
    /// Gets the timestamp when this metadata was collected or last updated.
    /// </summary>
    /// <value>The UTC timestamp when metadata was collected.</value>
    /// <remarks>
    /// This timestamp helps determine if metadata needs to be refreshed and provides
    /// context for troubleshooting issues that may be related to provider changes.
    /// </remarks>
    DateTimeOffset CollectedAt { get; }
}