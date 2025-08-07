using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface representing information about multi-factor authentication.
/// Provides details about the MFA methods used and their verification status.
/// </summary>
/// <remarks>
/// MFA information helps track which authentication factors were used
/// and their individual verification outcomes for audit and security purposes.
/// </remarks>
public interface IMultiFactorAuthenticationInfo
{
    /// <summary>
    /// Gets a value indicating whether MFA was completed successfully.
    /// </summary>
    /// <value><c>true</c> if MFA was completed; otherwise, <c>false</c>.</value>
    bool IsCompleted { get; }
    
    /// <summary>
    /// Gets the number of authentication factors that were successfully verified.
    /// </summary>
    /// <value>The number of verified factors.</value>
    int VerifiedFactors { get; }
    
    /// <summary>
    /// Gets the total number of authentication factors that were attempted.
    /// </summary>
    /// <value>The total number of factors attempted.</value>
    int TotalFactors { get; }
    
    /// <summary>
    /// Gets the authentication methods that were used.
    /// </summary>
    /// <value>A collection of authentication method names.</value>
    /// <remarks>
    /// Common MFA methods include "SMS", "Email", "TOTP", "Push", "Biometric", "Hardware".
    /// </remarks>
    IReadOnlyCollection<string> AuthenticationMethods { get; }
    
    /// <summary>
    /// Gets detailed information about each authentication factor.
    /// </summary>
    /// <value>A collection of authentication factor details.</value>
    IReadOnlyCollection<IAuthenticationFactorInfo> Factors { get; }
    
    /// <summary>
    /// Gets when MFA verification started.
    /// </summary>
    /// <value>The MFA start timestamp.</value>
    DateTimeOffset StartedAt { get; }
    
    /// <summary>
    /// Gets when MFA verification completed.
    /// </summary>
    /// <value>The MFA completion timestamp, or null if not completed.</value>
    DateTimeOffset? CompletedAt { get; }
    
    /// <summary>
    /// Gets the total time taken for MFA verification.
    /// </summary>
    /// <value>The MFA verification duration.</value>
    TimeSpan VerificationTime { get; }
}