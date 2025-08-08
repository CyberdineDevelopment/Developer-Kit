using FractalDataWorks.Services;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Non-generic marker interface for secret services.
/// </summary>
public interface ISecretService : IFdwService
{
}

/// <summary>
/// Interface for secret service implementations that handle specific secret storage backends.
/// Defines the contract for services like AWS Secrets Manager, Azure Key Vault, HashiCorp Vault, etc.
/// </summary>
/// <typeparam name="TSecretCommand">The secret command type.</typeparam>
/// <remarks>
/// Secret services are responsible for implementing the actual communication with
/// secret storage systems. They handle service-specific authentication, API calls,
/// error handling, and result formatting.
/// </remarks>
public interface ISecretService<TSecretCommand> : ISecretService, IFdwService<TSecretCommand>
    where TSecretCommand : ISecretCommand
{
}