using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums.Attributes;
using FractalDataWorks.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base class for authentication provider types that generates the AuthenticationProviders collection.
/// Integrates with the EnhancedEnums system for automatic service discovery and registration.
/// </summary>
/// <remarks>
/// This base class provides the Enhanced Enum pattern for authentication providers in the framework.
/// It handles provider type registration, factory creation, and integration with the EnhancedEnums
/// system for automatic collection generation and lookup capabilities.
/// </remarks>
[EnumCollection(
    CollectionName = "AuthenticationProviders", 
    IncludeReferencedAssemblies = true,
    ReturnType = typeof(IAuthenticationProvider))]
public abstract class AuthenticationProviderType : ServiceTypeBase<IAuthenticationProvider, AuthenticationConfiguration>
{
    /// <summary>
    /// Gets the provider type identifier.
    /// </summary>
    /// <value>The provider type (e.g., "AzureEntra", "Okta", "Auth0", "LDAP").</value>
    [EnumLookup("GetByProviderType")]
    public string ProviderType { get; }
    
    /// <summary>
    /// Gets the version of this provider implementation.
    /// </summary>
    /// <value>The provider version string.</value>
    public string Version { get; }
    
    /// <summary>
    /// Gets the supported authentication flows for this provider type.
    /// </summary>
    /// <value>A collection of authentication flow names supported by this provider type.</value>
    [EnumLookup("GetByAuthenticationFlow", allowMultiple: true)]
    public IReadOnlyCollection<string> SupportedAuthenticationFlows { get; }
    
    /// <summary>
    /// Gets the supported authentication command types for this provider type.
    /// </summary>
    /// <value>A collection of command type names supported by this provider type.</value>
    [EnumLookup("GetByCommandType", allowMultiple: true)]
    public IReadOnlyCollection<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the supported realms or domains for this provider type.
    /// </summary>
    /// <value>A collection of realm identifiers supported by this provider type.</value>
    [EnumLookup("GetByRealm", allowMultiple: true)]
    public IReadOnlyCollection<string> SupportedRealms { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports multi-factor authentication.
    /// </summary>
    /// <value><c>true</c> if MFA is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetMfaSupported")]
    public bool SupportsMfa { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports token refresh.
    /// </summary>
    /// <value><c>true</c> if token refresh is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetTokenRefreshSupported")]
    public bool SupportsTokenRefresh { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports user information retrieval.
    /// </summary>
    /// <value><c>true</c> if user info retrieval is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetUserInfoSupported")]
    public bool SupportsUserInfo { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports password operations.
    /// </summary>
    /// <value><c>true</c> if password operations are supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetPasswordOperationsSupported")]
    public bool SupportsPasswordOperations { get; }
    
    /// <summary>
    /// Gets a value indicating whether this provider type supports session management.
    /// </summary>
    /// <value><c>true</c> if session management is supported; otherwise, <c>false</c>.</value>
    [EnumLookup("GetSessionManagementSupported")]
    public bool SupportsSessionManagement { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationProviderType"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this authentication provider type.</param>
    /// <param name="name">The display name of this authentication provider type.</param>
    /// <param name="description">The description of this authentication provider type.</param>
    /// <param name="providerType">The provider type identifier.</param>
    /// <param name="version">The provider version.</param>
    /// <param name="supportedAuthenticationFlows">The authentication flows this provider type supports.</param>
    /// <param name="supportedCommandTypes">The command types this provider type can execute.</param>
    /// <param name="supportedRealms">The realms this provider type can work with.</param>
    /// <param name="supportsMfa">Whether MFA is supported.</param>
    /// <param name="supportsTokenRefresh">Whether token refresh is supported.</param>
    /// <param name="supportsUserInfo">Whether user info retrieval is supported.</param>
    /// <param name="supportsPasswordOperations">Whether password operations are supported.</param>
    /// <param name="supportsSessionManagement">Whether session management is supported.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any string parameter is empty or whitespace, or when collections are empty.
    /// </exception>
    protected AuthenticationProviderType(
        int id, 
        string name, 
        string description,
        string providerType,
        string version,
        IReadOnlyCollection<string> supportedAuthenticationFlows,
        IReadOnlyCollection<string> supportedCommandTypes,
        IReadOnlyCollection<string> supportedRealms,
        bool supportsMfa = false,
        bool supportsTokenRefresh = true,
        bool supportsUserInfo = true,
        bool supportsPasswordOperations = false,
        bool supportsSessionManagement = true) 
        : base(id, name, description)
    {
        if (string.IsNullOrWhiteSpace(providerType))
        {
            throw new ArgumentException("Provider type cannot be empty or whitespace.", nameof(providerType));
        }
        
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Version cannot be empty or whitespace.", nameof(version));
        }
        
        if (supportedAuthenticationFlows.Count == 0)
        {
            throw new ArgumentException("At least one supported authentication flow must be specified.", nameof(supportedAuthenticationFlows));
        }
        
        if (supportedCommandTypes.Count == 0)
        {
            throw new ArgumentException("At least one supported command type must be specified.", nameof(supportedCommandTypes));
        }
        
        if (supportedRealms.Count == 0)
        {
            throw new ArgumentException("At least one supported realm must be specified.", nameof(supportedRealms));
        }
        
        // Validate that all flows are not null or empty
        foreach (var flow in supportedAuthenticationFlows)
        {
            if (string.IsNullOrWhiteSpace(flow))
            {
                throw new ArgumentException("Authentication flows cannot be null, empty, or whitespace.", nameof(supportedAuthenticationFlows));
            }
        }
        
        // Validate that all command types are not null or empty
        foreach (var commandType in supportedCommandTypes)
        {
            if (string.IsNullOrWhiteSpace(commandType))
            {
                throw new ArgumentException("Command types cannot be null, empty, or whitespace.", nameof(supportedCommandTypes));
            }
        }
        
        // Validate that all realms are not null or empty
        foreach (var realm in supportedRealms)
        {
            if (string.IsNullOrWhiteSpace(realm))
            {
                throw new ArgumentException("Realms cannot be null, empty, or whitespace.", nameof(supportedRealms));
            }
        }
        
        ProviderType = providerType;
        Version = version;
        SupportedAuthenticationFlows = supportedAuthenticationFlows;
        SupportedCommandTypes = supportedCommandTypes;
        SupportedRealms = supportedRealms;
        SupportsMfa = supportsMfa;
        SupportsTokenRefresh = supportsTokenRefresh;
        SupportsUserInfo = supportsUserInfo;
        SupportsPasswordOperations = supportsPasswordOperations;
        SupportsSessionManagement = supportsSessionManagement;
    }
    
    /// <summary>
    /// Creates a typed factory for this authentication provider type.
    /// </summary>
    /// <returns>The typed service factory.</returns>
    public abstract override IServiceFactory<IAuthenticationProvider, AuthenticationConfiguration> CreateTypedFactory();
}