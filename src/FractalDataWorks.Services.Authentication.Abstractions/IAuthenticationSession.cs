using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface representing an active authentication session.
/// Provides information about user sessions and their current state.
/// </summary>
public interface IAuthenticationSession
{
    /// <summary>
    /// Gets the unique identifier for this session.
    /// </summary>
    /// <value>The session identifier.</value>
    string SessionId { get; }
    
    /// <summary>
    /// Gets the user identifier associated with this session.
    /// </summary>
    /// <value>The user identifier.</value>
    string UserId { get; }
    
    /// <summary>
    /// Gets the provider that manages this session.
    /// </summary>
    /// <value>The provider identifier.</value>
    string ProviderId { get; }
    
    /// <summary>
    /// Gets when the session was created.
    /// </summary>
    /// <value>The session creation timestamp.</value>
    DateTimeOffset CreatedAt { get; }
    
    /// <summary>
    /// Gets when the session was last accessed.
    /// </summary>
    /// <value>The last access timestamp.</value>
    DateTimeOffset LastAccessedAt { get; }
    
    /// <summary>
    /// Gets when the session expires.
    /// </summary>
    /// <value>The session expiration timestamp, or null if it doesn't expire.</value>
    DateTimeOffset? ExpiresAt { get; }
    
    /// <summary>
    /// Gets a value indicating whether this session is currently active.
    /// </summary>
    /// <value><c>true</c> if the session is active; otherwise, <c>false</c>.</value>
    bool IsActive { get; }
    
    /// <summary>
    /// Gets a value indicating whether this session has expired.
    /// </summary>
    /// <value><c>true</c> if the session has expired; otherwise, <c>false</c>.</value>
    bool IsExpired { get; }
    
    /// <summary>
    /// Gets the authentication method used for this session.
    /// </summary>
    /// <value>The authentication method.</value>
    string AuthenticationMethod { get; }
    
    /// <summary>
    /// Gets the realm or domain for this session.
    /// </summary>
    /// <value>The realm identifier, or null if not applicable.</value>
    string? Realm { get; }
    
    /// <summary>
    /// Gets additional session metadata.
    /// </summary>
    /// <value>A dictionary of session metadata properties.</value>
    IReadOnlyDictionary<string, object> Metadata { get; }
}