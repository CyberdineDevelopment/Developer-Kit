using System.Threading;
using System.Threading.Tasks;

namespace FractalDataWorks.Services;

/// <summary>
/// Defines the contract for all Fractal services with specific configuration and command types.
/// </summary>
/// <typeparam name="TCommand">The type of command this service processes.</typeparam>
public interface IFdwService<in TCommand> : IFdwService
    where TCommand : ICommand
{

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <typeparam name="TOut">The type of result returned by the command.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    Task<IFdwResult<TOut>> Execute<TOut>(TCommand command,CancellationToken cancellationToken);

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    Task<IFdwResult> Execute(TCommand command,CancellationToken cancellationToken);
}

/// <summary>
/// Defines the base contract for all Fractal services.
/// </summary>
public interface IFdwService 
{
    /// <summary>
    /// Gets the service name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    Task<IFdwResult> Execute(ICommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TOut">The type of result the command should return.</typeparam>
    /// <returns>A task containing the result of the command execution.</returns>
    Task<IFdwResult<TOut>> Execute<TOut>(ICommand command, CancellationToken cancellationToken);


}
