using System;
using FluentValidation.Results;
using FractalDataWorks.Services.Authentication.Abstractions;

namespace FractalDataWorks.Services.Authentication.AzureEntra.Configuration;

/// <summary>
/// Configuration for Azure Entra (Azure Active Directory) authentication.
/// </summary>
public sealed record AzureEntraConfiguration : IAuthenticationConfiguration
{
    /// <inheritdoc/>
    public string SectionName => "AzureEntra";

    /// <inheritdoc/>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the client secret for confidential client applications.
    /// </summary>
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>
    /// Gets the tenant ID for the Azure Entra tenant.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string Authority { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string RedirectUri { get; init; } = string.Empty;

    /// <inheritdoc/>
    public string[] Scopes { get; init; } = Array.Empty<string>();

    /// <inheritdoc/>
    public bool EnableTokenCaching { get; init; } = true;

    /// <inheritdoc/>
    public int TokenCacheLifetimeMinutes { get; init; } = 60;

    /// <summary>
    /// Gets the Azure cloud instance.
    /// </summary>
    /// <remarks>
    /// Common values: "https://login.microsoftonline.com", "https://login.microsoftonline.us", "https://login.microsoftonline.de"
    /// </remarks>
    public string Instance { get; init; } = "https://login.microsoftonline.com";

    /// <summary>
    /// Gets the application type for the client.
    /// </summary>
    /// <remarks>
    /// Values: "Public", "Confidential"
    /// </remarks>
    public string ClientType { get; init; } = "Confidential";

    /// <summary>
    /// Gets a value indicating whether to validate the issuer.
    /// </summary>
    public bool ValidateIssuer { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to validate the audience.
    /// </summary>
    public bool ValidateAudience { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to validate token lifetime.
    /// </summary>
    public bool ValidateLifetime { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to validate the token signature.
    /// </summary>
    public bool ValidateIssuerSigningKey { get; init; } = true;

    /// <summary>
    /// Gets the clock skew tolerance in minutes for token validation.
    /// </summary>
    public int ClockSkewToleranceMinutes { get; init; } = 5;

    /// <summary>
    /// Gets the cache file path for token storage.
    /// </summary>
    public string? CacheFilePath { get; init; }

    /// <summary>
    /// Gets a value indicating whether to enable logging of personally identifiable information.
    /// </summary>
    public bool EnablePiiLogging { get; init; } = false;

    /// <summary>
    /// Gets the HTTP timeout in seconds for authentication requests.
    /// </summary>
    public int HttpTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets the maximum number of retry attempts for failed requests.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Gets additional audiences that are valid for token validation.
    /// </summary>
    public string[] AdditionalValidAudiences { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets additional issuers that are valid for token validation.
    /// </summary>
    public string[] AdditionalValidIssuers { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Validates this configuration using FluentValidation.
    /// </summary>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate()
    {
        return new AzureEntraConfigurationValidator().Validate(this);
    }
}