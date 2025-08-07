namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Enumeration of authentication strength levels.
/// Indicates the level of confidence in the authentication result.
/// </summary>
public enum AuthenticationStrength
{
    /// <summary>
    /// Authentication strength is unknown or could not be determined.
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// Low-strength authentication using basic methods.
    /// </summary>
    /// <remarks>
    /// Examples include simple password authentication without additional security measures.
    /// </remarks>
    Low = 1,
    
    /// <summary>
    /// Medium-strength authentication using enhanced security measures.
    /// </summary>
    /// <remarks>
    /// Examples include password authentication with additional security checks
    /// like device verification or risk-based authentication.
    /// </remarks>
    Medium = 2,
    
    /// <summary>
    /// High-strength authentication using multiple factors.
    /// </summary>
    /// <remarks>
    /// Examples include multi-factor authentication with two or more verification methods.
    /// </remarks>
    High = 3,
    
    /// <summary>
    /// Very high-strength authentication using advanced security methods.
    /// </summary>
    /// <remarks>
    /// Examples include certificate-based authentication, hardware tokens,
    /// or biometric verification combined with other factors.
    /// </remarks>
    VeryHigh = 4
}