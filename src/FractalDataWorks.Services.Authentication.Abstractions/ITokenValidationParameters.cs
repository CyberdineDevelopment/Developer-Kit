using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface representing parameters for token validation.
/// Provides configuration for cryptographic and business rule validation of tokens.
/// </summary>
/// <remarks>
/// Validation parameters control how tokens are verified for authenticity,
/// integrity, and compliance with security policies.
/// </remarks>
public interface ITokenValidationParameters
{
    /// <summary>
    /// Gets a value indicating whether to validate the token's signature.
    /// </summary>
    /// <value><c>true</c> to validate the signature; otherwise, <c>false</c>.</value>
    bool ValidateSignature { get; }
    
    /// <summary>
    /// Gets a value indicating whether to validate the token's issuer.
    /// </summary>
    /// <value><c>true</c> to validate the issuer; otherwise, <c>false</c>.</value>
    bool ValidateIssuer { get; }
    
    /// <summary>
    /// Gets a value indicating whether to validate the token's audience.
    /// </summary>
    /// <value><c>true</c> to validate the audience; otherwise, <c>false</c>.</value>
    bool ValidateAudience { get; }
    
    /// <summary>
    /// Gets a value indicating whether to validate the token's expiration time.
    /// </summary>
    /// <value><c>true</c> to validate expiration; otherwise, <c>false</c>.</value>
    bool ValidateExpiration { get; }
    
    /// <summary>
    /// Gets a value indicating whether to validate the token's not-before time.
    /// </summary>
    /// <value><c>true</c> to validate not-before time; otherwise, <c>false</c>.</value>
    bool ValidateNotBefore { get; }
    
    /// <summary>
    /// Gets the valid issuers for token validation.
    /// </summary>
    /// <value>A collection of valid issuer identifiers.</value>
    IReadOnlyCollection<string> ValidIssuers { get; }
    
    /// <summary>
    /// Gets the valid audiences for token validation.
    /// </summary>
    /// <value>A collection of valid audience identifiers.</value>
    IReadOnlyCollection<string> ValidAudiences { get; }
    
    /// <summary>
    /// Gets the signing keys for signature validation.
    /// </summary>
    /// <value>A collection of signing keys or certificates.</value>
    IReadOnlyCollection<object> SigningKeys { get; }
    
    /// <summary>
    /// Gets the clock skew tolerance for time-based validations.
    /// </summary>
    /// <value>The allowed clock skew duration.</value>
    /// <remarks>
    /// Clock skew tolerance accounts for time differences between
    /// token issuer and validator systems.
    /// </remarks>
    TimeSpan ClockSkew { get; }
}