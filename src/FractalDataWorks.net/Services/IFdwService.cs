using System;
using System.Threading.Tasks;

namespace FractalDataWorks.Services;

/// <summary>
/// Base interface for all services in the FractalDataWorks framework.
/// Provides common service lifecycle management and identification capabilities.
/// </summary>
/// <remarks>
/// All framework services should implement this interface to ensure consistent
/// behavior and integration with the service management infrastructure.
/// The "Fdw" prefix avoids namespace collisions with common service interfaces.
/// </remarks>
public interface IFdwService
{
    /// <summary>
    /// Gets the unique identifier for this service instance.
    /// </summary>
    /// <value>A unique identifier for the service instance.</value>
    /// <remarks>
    /// This identifier is used for service tracking, logging, and debugging purposes.
    /// It should remain constant for the lifetime of the service instance.
    /// </remarks>
    string ServiceId { get; }
    
    /// <summary>
    /// Gets the display name of the service.
    /// </summary>
    /// <value>A human-readable name for the service.</value>
    /// <remarks>
    /// This name is used in user interfaces, logging, and diagnostic outputs.
    /// It should be descriptive and help identify the service's purpose.
    /// </remarks>
    string ServiceName { get; }
    
    /// <summary>
    /// Gets a value indicating whether the service is currently available for use.
    /// </summary>
    /// <value><c>true</c> if the service is available; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// Services may become unavailable due to configuration issues, network problems,
    /// or temporary failures. The framework can use this property to determine
    /// whether to route requests to this service instance.
    /// </remarks>
    bool IsAvailable { get; }

}

/// <summary>
/// Generic interface for services that provide specific functionality.
/// Extends the base service interface with typed configuration support.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this service requires.</typeparam>
/// <remarks>
/// Use this interface for services that require specific configuration objects
/// to function properly. The configuration should be provided via constructor.
/// </remarks>
public interface IFdwService<TCommand> : IFdwService
    where TCommand : ICommand
{
    /// <summary>
    /// Executes a command using the service.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    Task<IFdwResult> Execute(TCommand command);
}
