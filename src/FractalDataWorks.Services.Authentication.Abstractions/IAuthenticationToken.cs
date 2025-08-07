using System;
using System.Collections.Generic;
using System.Security;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface representing an authentication token.
/// Provides access to token information while maintaining security best practices.
/// </summary>
/// <remarks>
/// Authentication tokens represent credentials or proof of authentication
/// and should be handled securely throughout their lifecycle. This interface
/// provides safe access to token information without exposing sensitive details unnecessarily.
/// </remarks>
public interface IAuthenticationToken : IDisposable
{
    /// <summary>
    /// Gets the token type.
    /// </summary>
    /// <value>The token type (e.g., "Bearer", "JWT", "OAuth2", "SAML", "API_KEY").</value>
    /// <remarks>
    /// Token type indicates the format and usage pattern of the token,
    /// which determines how it should be transmitted and validated.
    /// </remarks>
    string TokenType { get; }
    
    /// <summary>
    /// Gets the token scheme or protocol.
    /// </summary>
    /// <value>The token scheme (e.g., "OAuth2", "JWT", "Basic", "Digest").</value>
    /// <remarks>
    /// The scheme indicates the authentication protocol or standard
    /// that governs how this token should be used and validated.
    /// </remarks>
    string Scheme { get; }
    
    /// <summary>
    /// Gets when the token was issued.
    /// </summary>
    /// <value>The token issuance timestamp.</value>
    DateTimeOffset IssuedAt { get; }
    
    /// <summary>
    /// Gets when the token expires.
    /// </summary>
    /// <value>The token expiration timestamp, or null if it doesn't expire.</value>
    DateTimeOffset? ExpiresAt { get; }
    
    /// <summary>
    /// Gets when the token becomes valid (not before time).
    /// </summary>
    /// <value>The token validity start timestamp, or null if valid immediately.</value>
    /// <remarks>
    /// Some tokens may not be valid until a specific time, which is useful
    /// for implementing token activation delays or scheduled access.
    /// </remarks>
    DateTimeOffset? NotBefore { get; }
    
    /// <summary>
    /// Gets a value indicating whether the token has expired.
    /// </summary>
    /// <value><c>true</c> if the token has expired; otherwise, <c>false</c>.</value>
    bool IsExpired { get; }
    
    /// <summary>
    /// Gets a value indicating whether the token is currently valid.
    /// </summary>
    /// <value><c>true</c> if the token is valid; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// A token is considered valid if it has not expired and is past
    /// its not-before time (if specified).
    /// </remarks>
    bool IsValid { get; }
    
    /// <summary>
    /// Gets the token issuer.
    /// </summary>
    /// <value>The issuer identifier, or null if not specified.</value>
    /// <remarks>
    /// The issuer identifies who created and signed the token,
    /// which is important for token validation and trust decisions.
    /// </remarks>
    string? Issuer { get; }
    
    /// <summary>
    /// Gets the intended audience for the token.
    /// </summary>
    /// <value>The audience identifier, or null if not specified.</value>
    /// <remarks>
    /// The audience identifies the intended recipient or service
    /// that should accept and validate this token.
    /// </remarks>
    string? Audience { get; }
    
    /// <summary>
    /// Gets the subject (user) that the token represents.
    /// </summary>
    /// <value>The subject identifier, or null if not specified.</value>
    /// <remarks>
    /// The subject typically identifies the user or entity
    /// that the token was issued for.
    /// </remarks>
    string? Subject { get; }
    
    /// <summary>
    /// Gets the scopes or permissions granted by this token.
    /// </summary>
    /// <value>A collection of scope names.</value>
    /// <remarks>
    /// Scopes define what resources or operations the token holder
    /// is authorized to access with this token.
    /// </remarks>
    IReadOnlyCollection<string> Scopes { get; }
    
    /// <summary>
    /// Gets the token's unique identifier.
    /// </summary>
    /// <value>The token identifier, or null if not specified.</value>
    /// <remarks>
    /// Token identifiers can be used for token revocation,
    /// audit logging, or duplicate detection.
    /// </remarks>
    string? TokenId { get; }
    
    /// <summary>
    /// Gets additional claims or properties associated with the token.
    /// </summary>
    /// <value>A dictionary of claim key-value pairs.</value>
    /// <remarks>
    /// Claims provide additional context or authorization information
    /// embedded within the token structure.
    /// </remarks>
    IReadOnlyDictionary<string, object> Claims { get; }
    
    /// <summary>
    /// Gets the size of the token in bytes.
    /// </summary>
    /// <value>The token size in bytes.</value>
    /// <remarks>
    /// Token size information helps with performance planning
    /// and transport considerations.
    /// </remarks>
    int SizeInBytes { get; }
    
    /// <summary>
    /// Gets a value indicating whether this token contains binary data.
    /// </summary>
    /// <value><c>true</c> if the token contains binary data; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Some token formats (like certificates or encrypted tokens)
    /// may contain binary data that requires special handling.
    /// </remarks>
    bool IsBinary { get; }
    
    /// <summary>
    /// Gets the token value as a string.
    /// </summary>
    /// <returns>The token string value.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the token has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the token contains binary data.</exception>
    /// <remarks>
    /// This method should be used sparingly and the returned string should be
    /// cleared from memory as soon as possible after use. Consider using
    /// <see cref="AccessTokenValue{TResult}"/> for safer access patterns.
    /// </remarks>
    string GetTokenValue();
    
    /// <summary>
    /// Gets the token value as a byte array.
    /// </summary>
    /// <returns>A copy of the token binary value.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the token has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the token contains string data.</exception>
    /// <remarks>
    /// The returned byte array is a copy. Callers should clear the array
    /// from memory when finished using it. Consider using
    /// <see cref="AccessTokenValue{TResult}"/> for safer access patterns.
    /// </remarks>
    byte[] GetTokenBytes();
    
    /// <summary>
    /// Performs a secure access to the token value using a callback function.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <param name="accessor">The callback function that receives the token value.</param>
    /// <returns>The result of the callback function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="accessor"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the token has been disposed.</exception>
    /// <remarks>
    /// This method provides a more secure way to access token values by ensuring
    /// the value is only available within the scope of the callback function.
    /// The token value is automatically cleared from memory after the callback completes.
    /// </remarks>
    TResult AccessTokenValue<TResult>(Func<string, TResult> accessor);
    
    /// <summary>
    /// Performs a secure access to the binary token value using a callback function.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <param name="accessor">The callback function that receives the token bytes.</param>
    /// <returns>The result of the callback function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="accessor"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the token has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the token contains string data.</exception>
    /// <remarks>
    /// This method provides a more secure way to access binary token values by ensuring
    /// the value is only available within the scope of the callback function.
    /// The token bytes are automatically cleared from memory after the callback completes.
    /// </remarks>
    TResult AccessTokenBytes<TResult>(Func<byte[], TResult> accessor);
    
    /// <summary>
    /// Validates the token's signature and integrity.
    /// </summary>
    /// <param name="validationParameters">The validation parameters to use.</param>
    /// <returns>A result indicating whether the token is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationParameters"/> is null.</exception>
    /// <remarks>
    /// This method performs cryptographic validation of the token to ensure
    /// it has not been tampered with and was issued by a trusted authority.
    /// </remarks>
    TokenValidationResult Validate(ITokenValidationParameters validationParameters);
    
    /// <summary>
    /// Creates a copy of this token with a new expiration time.
    /// </summary>
    /// <param name="newExpiration">The new expiration time.</param>
    /// <returns>A new token instance with the updated expiration.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="newExpiration"/> is in the past.</exception>
    /// <exception cref="NotSupportedException">Thrown when the token type doesn't support expiration updates.</exception>
    /// <remarks>
    /// Not all token types support expiration updates. This method is primarily
    /// useful for session tokens or internally-managed authentication tokens.
    /// </remarks>
    IAuthenticationToken WithExpiration(DateTimeOffset newExpiration);
}