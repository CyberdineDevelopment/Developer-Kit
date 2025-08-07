using System;
using System.Collections.Generic;


namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Interface for authentication commands in the FractalDataWorks framework.
/// Represents a command that can be executed against an authentication provider to perform authentication operations.
/// </summary>
/// <remarks>
/// Authentication commands encapsulate the details of authentication operations (login, logout, token validation, etc.)
/// and provide a consistent interface for authentication providers to execute operations
/// regardless of the underlying authentication system technology.
/// </remarks>
public interface IAuthenticationCommand
{
    /// <summary>
    /// Gets the unique identifier for this command.
    /// </summary>
    /// <value>A unique identifier for the command instance.</value>
    /// <remarks>
    /// This identifier is used for command tracking, logging, and debugging purposes.
    /// It should remain constant for the lifetime of the command instance.
    /// </remarks>
    string CommandId { get; }
    
    /// <summary>
    /// Gets the type of operation this command represents.
    /// </summary>
    /// <value>The command type (e.g., "Login", "Logout", "ValidateToken", "RefreshToken", "GetUserInfo").</value>
    /// <remarks>
    /// Command types help authentication providers determine how to execute the command
    /// and what type of result to expect. This enables provider-specific optimizations.
    /// </remarks>
    string CommandType { get; }
    
    /// <summary>
    /// Gets the authentication flow or method being used.
    /// </summary>
    /// <value>The authentication flow (e.g., "OAuth2", "SAML", "BasicAuth", "JWT", "MFA").</value>
    /// <remarks>
    /// The authentication flow indicates the type of authentication mechanism
    /// being used, which determines how the command should be processed.
    /// </remarks>
    string AuthenticationFlow { get; }
    
    /// <summary>
    /// Gets the target realm, domain, or application for this authentication.
    /// </summary>
    /// <value>The target realm or domain, or null if not applicable.</value>
    /// <remarks>
    /// The target helps authentication providers route commands to the appropriate
    /// authentication contexts and apply realm-specific configurations or policies.
    /// </remarks>
    string? Realm { get; }
    
    /// <summary>
    /// Gets the expected result type for this command.
    /// </summary>
    /// <value>The Type of object expected to be returned by command execution.</value>
    /// <remarks>
    /// This information enables authentication providers to prepare appropriate result handling
    /// and type conversion logic before executing the command.
    /// </remarks>
    Type ExpectedResultType { get; }
    
    /// <summary>
    /// Gets the timeout for command execution.
    /// </summary>
    /// <value>The maximum time to wait for command execution, or null for provider default.</value>
    /// <remarks>
    /// Command-specific timeouts allow fine-grained control over execution time limits.
    /// If null, the authentication provider should use its default timeout configuration.
    /// </remarks>
    TimeSpan? Timeout { get; }
    
    /// <summary>
    /// Gets the parameters for this command.
    /// </summary>
    /// <value>A dictionary of parameter names and values for command execution.</value>
    /// <remarks>
    /// Parameters provide input data for the command execution. Common parameters include
    /// "Username", "Password", "Token", "ClientId", "RedirectUri", "Scope", "Claims".
    /// Parameter names should use consistent naming conventions across commands.
    /// </remarks>
    IReadOnlyDictionary<string, object?> Parameters { get; }
    
    /// <summary>
    /// Gets additional metadata for this command.
    /// </summary>
    /// <value>A dictionary of metadata properties that may influence command execution.</value>
    /// <remarks>
    /// Metadata can include security hints, compliance requirements, audit trail data,
    /// or other provider-specific configuration options.
    /// Common metadata keys include "IpAddress", "UserAgent", "SessionId", "RequestId".
    /// </remarks>
    IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Gets a value indicating whether this command requires secure transport.
    /// </summary>
    /// <value><c>true</c> if secure transport is required; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// This property helps authentication providers determine appropriate security
    /// measures and transport protocols for the command execution.
    /// </remarks>
    bool RequiresSecureTransport { get; }
    
    /// <summary>
    /// Gets a value indicating whether this command should be audited.
    /// </summary>
    /// <value><c>true</c> if the command should be audited; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Audit requirements help authentication providers determine whether to
    /// log detailed information about the command execution for compliance purposes.
    /// </remarks>
    bool RequiresAudit { get; }
    
    /// <summary>
    /// Validates the command before execution.
    /// </summary>
    /// <returns>A result indicating whether the command is valid for execution.</returns>
    /// <remarks>
    /// This method allows commands to perform self-validation before being passed
    /// to authentication providers. It can check parameter completeness, value formats,
    /// security requirements, and other command-specific validation rules.
    /// </remarks>
    IFdwResult Validate();
    
    /// <summary>
    /// Creates a copy of this command with modified parameters.
    /// </summary>
    /// <param name="newParameters">The new parameters to use in the copied command.</param>
    /// <returns>A new command instance with the specified parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newParameters"/> is null.</exception>
    /// <remarks>
    /// This method enables command reuse with different parameter sets without
    /// modifying the original command instance. Useful for retry scenarios with
    /// different credentials or authentication flows.
    /// </remarks>
    IAuthenticationCommand WithParameters(IReadOnlyDictionary<string, object?> newParameters);
    
    /// <summary>
    /// Creates a copy of this command with modified metadata.
    /// </summary>
    /// <param name="newMetadata">The new metadata to use in the copied command.</param>
    /// <returns>A new command instance with the specified metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newMetadata"/> is null.</exception>
    /// <remarks>
    /// This method enables command customization with different execution hints
    /// or security contexts without modifying the original command instance.
    /// </remarks>
    IAuthenticationCommand WithMetadata(IReadOnlyDictionary<string, object> newMetadata);
}

/// <summary>
/// Generic interface for authentication commands with typed result expectations.
/// Extends the base command interface with compile-time type safety for results.
/// </summary>
/// <typeparam name="TResult">The type of result expected from command execution.</typeparam>
/// <remarks>
/// Use this interface when the expected result type is known at compile time.
/// It provides type safety and eliminates the need for runtime type checking and casting.
/// </remarks>
public interface IAuthenticationCommand<TResult> : IAuthenticationCommand
{
    /// <summary>
    /// Creates a copy of this command with modified parameters.
    /// </summary>
    /// <param name="newParameters">The new parameters to use in the copied command.</param>
    /// <returns>A new typed command instance with the specified parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newParameters"/> is null.</exception>
    /// <remarks>
    /// This method provides type-safe command copying for generic command instances.
    /// </remarks>
    new IAuthenticationCommand<TResult> WithParameters(IReadOnlyDictionary<string, object?> newParameters);
    
    /// <summary>
    /// Creates a copy of this command with modified metadata.
    /// </summary>
    /// <param name="newMetadata">The new metadata to use in the copied command.</param>
    /// <returns>A new typed command instance with the specified metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="newMetadata"/> is null.</exception>
    /// <remarks>
    /// This method provides type-safe command copying for generic command instances.
    /// </remarks>
    new IAuthenticationCommand<TResult> WithMetadata(IReadOnlyDictionary<string, object> newMetadata);
}