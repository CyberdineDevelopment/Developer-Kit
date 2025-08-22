using System;
using System.Collections.Generic;
using FractalDataWorks.Services;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Base class for authentication service type definitions.
/// Used by Enhanced Enums to register different authentication providers.
/// </summary>
/// <typeparam name="TService">The authentication service type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the authentication service.</typeparam>
/// <remarks>
/// This base class enables the authentication service pattern where different authentication
/// providers (Azure Entra, OIDC, SAML, etc.) can be discovered and routed based on configuration.
/// Each authentication type inherits from this base to provide metadata about what authentication
/// modes it supports and how to create instances.
/// </remarks>
public abstract class AuthenticationServiceTypeBase<TService, TConfiguration>
    : ServiceTypeBase<TService, TConfiguration>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationServiceTypeBase{TService, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this authentication service type.</param>
    /// <param name="name">The name of this authentication service type.</param>
    /// <param name="description">The description of this authentication service type.</param>
    protected AuthenticationServiceTypeBase(int id, string name, string description)
        : base(id, name, description)
    {
    }

    /// <summary>
    /// Gets the authentication protocols supported by this provider.
    /// </summary>
    /// <value>An array of authentication protocol identifiers this provider can handle.</value>
    /// <remarks>
    /// Examples: ["OAuth2", "OpenIDConnect", "SAML2"] for Azure Entra,
    /// ["OAuth2", "OpenIDConnect"] for generic OIDC,
    /// ["SAML2", "WS-Federation"] for ADFS.
    /// Used by authentication services to route requests to appropriate providers
    /// based on configuration or client requirements.
    /// </remarks>
    public abstract string[] SupportedProtocols { get; }

    /// <summary>
    /// Gets the provider name for this authentication type.
    /// </summary>
    /// <value>The technical name of the underlying provider or framework.</value>
    /// <remarks>
    /// Examples: "Microsoft.Identity.Client", "IdentityServer", "Auth0".
    /// Used for diagnostics, logging, and provider-specific behavior.
    /// </remarks>
    public abstract string ProviderName { get; }

    /// <summary>
    /// Gets the authentication flows supported by this provider.
    /// </summary>
    /// <value>A read-only list of supported authentication flows.</value>
    /// <remarks>
    /// Common flows include:
    /// - "AuthorizationCode": Authorization code flow with PKCE
    /// - "ClientCredentials": Client credentials flow for service-to-service
    /// - "DeviceCode": Device code flow for devices without browsers
    /// - "Interactive": Interactive authentication with user involvement
    /// - "Silent": Silent token acquisition using cached tokens
    /// - "OnBehalfOf": On-behalf-of flow for middle-tier services
    /// </remarks>
    public abstract IReadOnlyList<string> SupportedFlows { get; }

    /// <summary>
    /// Gets the token types supported by this provider.
    /// </summary>
    /// <value>A read-only list of supported token types.</value>
    /// <remarks>
    /// Common token types include:
    /// - "AccessToken": OAuth2 access tokens
    /// - "IdToken": OpenID Connect ID tokens
    /// - "RefreshToken": OAuth2 refresh tokens
    /// - "SAMLAssertion": SAML assertion tokens
    /// </remarks>
    public abstract IReadOnlyList<string> SupportedTokenTypes { get; }

    /// <summary>
    /// Gets the priority of this authentication provider.
    /// </summary>
    /// <value>An integer representing selection priority (higher values = higher priority).</value>
    /// <remarks>
    /// When multiple authentication providers support the same protocol,
    /// the authentication service selects the one with the highest priority.
    /// Use this to prefer newer/better providers over legacy ones.
    /// Typical values: 0-100 (100 being highest priority).
    /// </remarks>
    public abstract int Priority { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports multi-tenant scenarios.
    /// </summary>
    /// <value>True if the provider supports multiple tenants; otherwise, false.</value>
    public abstract bool SupportsMultiTenant { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports token caching.
    /// </summary>
    /// <value>True if the provider supports token caching; otherwise, false.</value>
    public abstract bool SupportsTokenCaching { get; }
}