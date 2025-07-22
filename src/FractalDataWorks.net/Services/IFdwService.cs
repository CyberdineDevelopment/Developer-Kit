using System.Threading.Tasks;

namespace FractalDataWorks.Services;

/// <summary>
/// Defines the contract for all Fractal services with specific configuration and command types.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this service uses.</typeparam>
/// <typeparam name="TCommand">The type of command this service processes.</typeparam>
/// <typeparam name="TResult">The base type of results returned by this service.</typeparam>
public interface IFdwService<TConfiguration, TCommand,TResult>
    where TConfiguration : IFdwConfiguration
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
    Task<IFdwResult<T>> Execute<T>(TCommand command) where T : TResult;
}
/// <summary>
/// Defines the contract for all Fractal services with specific configuration and command types.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this service uses.</typeparam>
/// <typeparam name="TCommand">The type of command this service processes.</typeparam>
public interface IFdwService<TConfiguration, TCommand> : IFdwService<TConfiguration, TCommand, IFdwResult>
    where TConfiguration : IFdwConfiguration
    where TCommand : ICommand
{
    /// <summary>
    /// Gets the service name.
    /// </summary>
    new string ServiceName { get; }

    /// <summary>
    /// Gets a value indicating whether the service is in a healthy state.
    /// </summary>
    new bool IsHealthy { get; }

    /// <summary>
    /// Gets the service configuration.
    /// </summary>
    new TConfiguration Configuration { get; }

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the command.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    new Task<IFdwResult<T>> Execute<T>(TCommand command);
}
/// <summary>
/// Defines the contract for all Fractal services with specific configuration type.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this service uses.</typeparam>
public interface IFdwService<TConfiguration> : IFdwService<TConfiguration, ICommand>
    where TConfiguration : IFdwConfiguration
{
}

/// <summary>
/// Defines the base contract for all Fractal services.
/// </summary>
public interface IFdwService : IFdwService<IFdwConfiguration, ICommand>
{
}
