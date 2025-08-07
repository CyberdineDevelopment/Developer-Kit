using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface representing information about an individual authentication factor.
/// Provides details about a specific factor used in multi-factor authentication.
/// </summary>
/// <remarks>
/// Factor information enables detailed tracking and auditing of each
/// authentication method used in a multi-factor authentication flow.
/// </remarks>
public interface IAuthenticationFactorInfo
{
    /// <summary>
    /// Gets the type of authentication factor.
    /// </summary>
    /// <value>The factor type (e.g., "SMS", "Email", "TOTP", "Push", "Biometric").</value>
    string FactorType { get; }
    
    /// <summary>
    /// Gets a value indicating whether this factor was successfully verified.
    /// </summary>
    /// <value><c>true</c> if the factor was verified; otherwise, <c>false</c>.</value>
    bool IsVerified { get; }
    
    /// <summary>
    /// Gets when verification of this factor was attempted.
    /// </summary>
    /// <value>The verification attempt timestamp.</value>
    DateTimeOffset AttemptedAt { get; }
    
    /// <summary>
    /// Gets when verification of this factor was completed.
    /// </summary>
    /// <value>The verification completion timestamp, or null if not completed.</value>
    DateTimeOffset? VerifiedAt { get; }
    
    /// <summary>
    /// Gets the number of verification attempts made for this factor.
    /// </summary>
    /// <value>The number of attempts.</value>
    int AttemptCount { get; }
    
    /// <summary>
    /// Gets any error message associated with this factor's verification.
    /// </summary>
    /// <value>The error message, or null if no error occurred.</value>
    string? ErrorMessage { get; }
    
    /// <summary>
    /// Gets additional metadata about this authentication factor.
    /// </summary>
    /// <value>A dictionary of factor-specific metadata.</value>
    /// <remarks>
    /// Metadata might include information like device identifiers for push notifications,
    /// partial phone numbers for SMS, or issuer information for TOTP tokens.
    /// </remarks>
    IReadOnlyDictionary<string, object> Metadata { get; }
}