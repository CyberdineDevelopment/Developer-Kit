using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base configuration class for authentication providers.
/// Provides common configuration properties that all authentication providers can use.
/// </summary>
/// <remarks>
/// This class serves as a foundation for provider-specific configuration classes.
/// It includes common settings like timeouts, retry policies, and security parameters
/// that are applicable across different authentication implementations.
/// </remarks>
public abstract class AuthenticationConfiguration
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    /// <value>A unique identifier for the provider instance.</value>
    /// <remarks>
    /// This identifier is used to distinguish between multiple instances of the same
    /// provider type, enabling scenarios like multi-tenant or multi-environment deployments.
    /// </remarks>
    public string ProviderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the display name for the provider instance.
    /// </summary>
    /// <value>A human-readable name for the provider.</value>
    /// <remarks>
    /// This name is used in logs, error messages, and management interfaces
    /// to help identify the specific provider instance.
    /// </remarks>
    public string ProviderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the default timeout for authentication operations.
    /// </summary>
    /// <value>The default timeout duration, or null to use provider defaults.</value>
    /// <remarks>
    /// This timeout applies to individual authentication operations when no specific timeout
    /// is specified in the command. Different providers may have different default values.
    /// </remarks>
    public TimeSpan? DefaultTimeout { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed operations.
    /// </summary>
    /// <value>The maximum retry count.</value>
    /// <remarks>
    /// Retry attempts help handle transient failures in network communication
    /// or temporary unavailability of the authentication service.
    /// </remarks>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Gets or sets the base delay between retry attempts.
    /// </summary>
    /// <value>The base delay duration.</value>
    /// <remarks>
    /// The actual delay may use exponential backoff or other strategies
    /// based on the provider implementation. This value serves as the baseline.
    /// </remarks>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Gets or sets a value indicating whether to use exponential backoff for retries.
    /// </summary>
    /// <value><c>true</c> to use exponential backoff; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Exponential backoff increases the delay between retry attempts to reduce
    /// load on the authentication service during outages or high-load conditions.
    /// </remarks>
    public bool UseExponentialBackoff { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether to enable connection pooling.
    /// </summary>
    /// <value><c>true</c> to enable connection pooling; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Connection pooling can improve performance by reusing connections
    /// across multiple operations, where supported by the provider.
    /// </remarks>
    public bool EnableConnectionPooling { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the maximum number of concurrent connections.
    /// </summary>
    /// <value>The maximum concurrent connection count.</value>
    /// <remarks>
    /// This setting helps control resource usage and may be subject to
    /// limits imposed by the authentication service.
    /// </remarks>
    public int MaxConcurrentConnections { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets a value indicating whether to validate SSL certificates.
    /// </summary>
    /// <value><c>true</c> to validate SSL certificates; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// SSL certificate validation should typically be enabled in production environments.
    /// Disabling validation may be useful for development or testing scenarios.
    /// </remarks>
    public bool ValidateSslCertificates { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the supported authentication flows.
    /// </summary>
    /// <value>A collection of supported authentication flow names.</value>
    /// <remarks>
    /// Different providers support different authentication flows (OAuth2, SAML, etc.).
    /// This property allows configuration of which flows are available.
    /// </remarks>
    public ICollection<string> SupportedAuthenticationFlows { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets the supported realms or domains.
    /// </summary>
    /// <value>A collection of supported realm identifiers.</value>
    /// <remarks>
    /// Realms represent different authentication domains or tenants.
    /// This property configures which realms this provider instance can handle.
    /// </remarks>
    public ICollection<string> SupportedRealms { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets the default token lifetime.
    /// </summary>
    /// <value>The default token lifetime, or null for provider defaults.</value>
    /// <remarks>
    /// This setting controls how long authentication tokens remain valid
    /// when no specific lifetime is requested during authentication.
    /// </remarks>
    public TimeSpan? DefaultTokenLifetime { get; set; }
    
    /// <summary>
    /// Gets or sets the default refresh token lifetime.
    /// </summary>
    /// <value>The default refresh token lifetime, or null for provider defaults.</value>
    /// <remarks>
    /// This setting controls how long refresh tokens remain valid for
    /// obtaining new authentication tokens without re-authentication.
    /// </remarks>
    public TimeSpan? DefaultRefreshTokenLifetime { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to enable session management.
    /// </summary>
    /// <value><c>true</c> to enable session management; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Session management enables tracking and control of user authentication sessions,
    /// including features like session timeout and concurrent session limits.
    /// </remarks>
    public bool EnableSessionManagement { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the maximum session lifetime.
    /// </summary>
    /// <value>The maximum session lifetime, or null for no limit.</value>
    /// <remarks>
    /// This setting enforces a maximum duration for authentication sessions,
    /// after which users must re-authenticate regardless of activity.
    /// </remarks>
    public TimeSpan? MaxSessionLifetime { get; set; }
    
    /// <summary>
    /// Gets or sets the session inactivity timeout.
    /// </summary>
    /// <value>The session inactivity timeout, or null for no timeout.</value>
    /// <remarks>
    /// This setting automatically expires sessions after a period of inactivity,
    /// helping to reduce security risks from abandoned sessions.
    /// </remarks>
    public TimeSpan? SessionInactivityTimeout { get; set; }
    
    /// <summary>
    /// Gets or sets additional configuration properties.
    /// </summary>
    /// <value>A dictionary of additional configuration key-value pairs.</value>
    /// <remarks>
    /// This property allows provider-specific configuration options that don't
    /// fit into the standard configuration properties. Examples include
    /// endpoint URLs, client identifiers, or feature flags.
    /// </remarks>
    public IDictionary<string, object> AdditionalProperties { get; set; } = new Dictionary<string, object>(StringComparer.Ordinal);
    
    /// <summary>
    /// Gets or sets a value indicating whether to enable performance metrics collection.
    /// </summary>
    /// <value><c>true</c> to enable metrics collection; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Metrics collection provides operational insights but may have a small
    /// performance impact. Enable based on monitoring and observability requirements.
    /// </remarks>
    public bool EnableMetrics { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether to enable detailed logging.
    /// </summary>
    /// <value><c>true</c> to enable detailed logging; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Detailed logging can help with troubleshooting but may impact performance
    /// and could potentially log sensitive information. Use with caution.
    /// </remarks>
    public bool EnableDetailedLogging { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether to enable audit logging.
    /// </summary>
    /// <value><c>true</c> to enable audit logging; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Audit logging records security-relevant events for compliance and
    /// security monitoring purposes. This may be required in regulated environments.
    /// </remarks>
    public bool EnableAuditLogging { get; set; } = true;
    
    /// <summary>
    /// Gets or sets security-related configuration options.
    /// </summary>
    /// <value>A dictionary of security configuration key-value pairs.</value>
    /// <remarks>
    /// Security options may include password policies, MFA requirements,
    /// rate limiting settings, or other security-related configurations.
    /// </remarks>
    public IDictionary<string, object> SecurityOptions { get; set; } = new Dictionary<string, object>(StringComparer.Ordinal);
    
    /// <summary>
    /// Validates the configuration settings.
    /// </summary>
    /// <returns>A collection of validation error messages, or empty if valid.</returns>
    /// <remarks>
    /// This method performs basic validation of the configuration properties.
    /// Derived classes should override this method to add provider-specific validation.
    /// </remarks>
    public virtual IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(ProviderId))
        {
            errors.Add("ProviderId cannot be null, empty, or whitespace.");
        }
        
        if (string.IsNullOrWhiteSpace(ProviderName))
        {
            errors.Add("ProviderName cannot be null, empty, or whitespace.");
        }
        
        if (DefaultTimeout.HasValue && DefaultTimeout.Value <= TimeSpan.Zero)
        {
            errors.Add("DefaultTimeout must be positive when specified.");
        }
        
        if (MaxRetryAttempts < 0)
        {
            errors.Add("MaxRetryAttempts cannot be negative.");
        }
        
        if (RetryDelay <= TimeSpan.Zero)
        {
            errors.Add("RetryDelay must be positive.");
        }
        
        if (MaxConcurrentConnections <= 0)
        {
            errors.Add("MaxConcurrentConnections must be positive.");
        }
        
        if (DefaultTokenLifetime.HasValue && DefaultTokenLifetime.Value <= TimeSpan.Zero)
        {
            errors.Add("DefaultTokenLifetime must be positive when specified.");
        }
        
        if (DefaultRefreshTokenLifetime.HasValue && DefaultRefreshTokenLifetime.Value <= TimeSpan.Zero)
        {
            errors.Add("DefaultRefreshTokenLifetime must be positive when specified.");
        }
        
        if (MaxSessionLifetime.HasValue && MaxSessionLifetime.Value <= TimeSpan.Zero)
        {
            errors.Add("MaxSessionLifetime must be positive when specified.");
        }
        
        if (SessionInactivityTimeout.HasValue && SessionInactivityTimeout.Value <= TimeSpan.Zero)
        {
            errors.Add("SessionInactivityTimeout must be positive when specified.");
        }
        
        return errors;
    }
    
    /// <summary>
    /// Creates a copy of this configuration.
    /// </summary>
    /// <returns>A new configuration instance with the same settings.</returns>
    /// <remarks>
    /// This method creates a shallow copy of the configuration. Derived classes
    /// should override this method to ensure proper copying of provider-specific properties.
    /// </remarks>
    public virtual AuthenticationConfiguration Clone()
    {
        // This is abstract, so derived classes must implement
        throw new NotImplementedException("Derived classes must implement Clone method.");
    }
}