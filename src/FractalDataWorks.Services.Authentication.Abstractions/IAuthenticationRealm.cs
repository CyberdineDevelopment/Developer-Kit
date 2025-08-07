using System.Collections.Generic;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface for authentication realm information.
/// </summary>
public interface IAuthenticationRealm
{
    /// <summary>
    /// Gets the realm identifier.
    /// </summary>
    /// <value>The realm identifier.</value>
    string RealmId { get; }
    
    /// <summary>
    /// Gets the realm name.
    /// </summary>
    /// <value>The realm display name.</value>
    string Name { get; }
    
    /// <summary>
    /// Gets the realm description.
    /// </summary>
    /// <value>The realm description, or null if not provided.</value>
    string? Description { get; }
    
    /// <summary>
    /// Gets a value indicating whether this realm is enabled.
    /// </summary>
    /// <value><c>true</c> if the realm is enabled; otherwise, <c>false</c>.</value>
    bool IsEnabled { get; }
    
    /// <summary>
    /// Gets the supported authentication flows for this realm.
    /// </summary>
    /// <value>A collection of supported authentication flow names.</value>
    IReadOnlyCollection<string> SupportedAuthenticationFlows { get; }
}