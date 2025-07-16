using System.Threading.Tasks;
using FractalDataWorks.Commands;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services;

/// <summary>
/// Defines the contract for all Fractal services with specific configuration and command types.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this service uses.</typeparam>
/// <typeparam name="TCommand">The type of command this service processes.</typeparam>
public interface IFractalService<TConfiguration, TCommand>
    where TConfiguration : IFractalConfiguration
    where TCommand : ICommand
{
    /// <summary>
    /// Gets the service name.
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Gets a value indicating whether the service is in a healthy state.
    /// </summary>
    bool IsHealthy { get; }

    /// <summary>
    /// Gets the service configuration.
    /// </summary>
    TConfiguration Configuration { get; }

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the command.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    Task<FractalResult<T>> Execute<T>(TCommand command);
}

/// <summary>
/// Defines the contract for all Fractal services with specific configuration type.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this service uses.</typeparam>
public interface IFractalService<TConfiguration> : IFractalService<TConfiguration, ICommand>
    where TConfiguration : IFractalConfiguration
{
}

/// <summary>
/// Defines the base contract for all Fractal services.
/// </summary>
public interface IFractalService : IFractalService<IFractalConfiguration, ICommand>
{
}
