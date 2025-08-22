namespace FractalDataWorks.Services.Authentication.Abstractions.Commands;

/// <summary>
/// Command interface for token validation operations.
/// </summary>
public interface ITokenValidationCommand : IAuthenticationCommand
{
    /// <summary>
    /// Gets the access token to validate.
    /// </summary>
    string AccessToken { get; }

    /// <summary>
    /// Gets the token type to validate.
    /// </summary>
    /// <remarks>
    /// Common values: "Bearer", "Pop", "JWT"
    /// </remarks>
    string TokenType { get; }

    /// <summary>
    /// Gets the expected audience for the token.
    /// </summary>
    string? ExpectedAudience { get; }

    /// <summary>
    /// Gets the expected issuer for the token.
    /// </summary>
    string? ExpectedIssuer { get; }

    /// <summary>
    /// Gets the required scopes that must be present in the token.
    /// </summary>
    string[]? RequiredScopes { get; }

    /// <summary>
    /// Gets a value indicating whether to validate token expiration.
    /// </summary>
    bool ValidateLifetime { get; }

    /// <summary>
    /// Gets a value indicating whether to validate the token signature.
    /// </summary>
    bool ValidateSignature { get; }
}