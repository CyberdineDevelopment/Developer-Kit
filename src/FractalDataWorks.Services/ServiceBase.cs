using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for all services with automatic validation and logging.
/// </summary>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TService">The concrete service type for logging category.</typeparam>
public abstract class ServiceBase<TCommand, TConfiguration, TService> : IFdwService<TCommand>
    where TConfiguration : IFdwConfiguration
    where TCommand : ICommand
    where TService : class
{
    private readonly ILogger<TService> _logger;
    private readonly TConfiguration _configuration;

    /// <summary>
    /// Gets the logger instance for derived classes.
    /// </summary>
    protected ILogger<TService> Logger => _logger;

    /// <inheritdoc/>
    public string Name => typeof(TService).Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBase{TCommand, TConfiguration, TService}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for the concrete service type. If null, uses Microsoft's NullLogger.</param>
    /// <param name="configuration">The configuration instance.</param>
    protected ServiceBase(
        ILogger<TService>? logger,
        TConfiguration configuration)
    {
        // Use Microsoft's NullLogger for consistency with ILogger<T> abstractions
        // This works seamlessly when Serilog is registered via services.AddSerilog()
        _logger = logger ?? NullLogger<TService>.Instance;
        _configuration = configuration;

        ServiceBaseLog.ServiceStarted(_logger, typeof(TService).Name);
    }

    /// <summary>
    /// Gets the unique identifier for this service instance.
    /// </summary>
    public virtual string Id => $"{ServiceType}_{Guid.NewGuid():N}";

    /// <summary>
    /// Gets the service name.
    /// </summary>
    public virtual string ServiceType => typeof(TService).Name;

    /// <summary>
    /// Gets a value indicating whether the service is currently available for use.
    /// </summary>
    public virtual bool IsAvailable => true;


    /// <summary>
    /// Validates a configuration.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <param name="validConfiguration">The valid configuration if successful.</param>
    /// <returns>The validation result.</returns>
    protected IFdwResult<TConfiguration> ConfigurationIsValid(
        IFdwConfiguration configuration,
        out TConfiguration validConfiguration)
    {
        if (configuration is TConfiguration config)
        {
            var validationResult = config.Validate();
            if (validationResult.IsValid)
            {
                validConfiguration = config;
                return FdwResult<TConfiguration>.Success(config);
            }
        }

        ServiceBaseLog.InvalidConfigurationWarning(_logger,
            $"Invalid configuration of type {configuration?.GetType().Name ?? "null"}. Not of expected type.");

        validConfiguration = default!;
        return FdwResult<TConfiguration>.Failure(
            "Invalid configuration.");
    }

    /// <summary>
    /// Validates a command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>The validation result.</returns>
    protected Task<IFdwResult<TCommand>> ValidateCommand(ICommand command)
    {
        return Task.FromResult(ValidateCommandCore(command));
    }

    private IFdwResult<TCommand> ValidateCommandCore(ICommand command)
    {
        if (command is not TCommand cmd)
        {
            ServiceBaseLog.InvalidConfigurationWarning(_logger,
                string.Format(CultureInfo.InvariantCulture, "Invalid command type. {0}", command?.GetType().Name ?? "null"));

            return FdwResult<TCommand>.Failure("Invalid command type.");
        }

        // Validate the command itself
        var validationResult = cmd.Validate();
        if (validationResult == null)
        {
            ServiceBaseLog.InvalidConfigurationWarning(_logger,
                string.Format(CultureInfo.InvariantCulture, "Command validation failed. {0}", "Validation returned null"));

            return FdwResult<TCommand>.Failure("Command validation failed.");
        }

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage).ToArray());
            return FdwResult<TCommand>.Failure("Command validation failed.");
        }

        // Validate command configuration if present
        if (cmd.Configuration is TConfiguration configToValidate)
        {
            var configValidationResult = configToValidate.Validate();
            if (!configValidationResult.IsValid)
            {
                ServiceBaseLog.InvalidConfigurationWarning(_logger,
                    string.Format(CultureInfo.InvariantCulture, "Invalid configuration: {0} of type {1}", "Command configuration", configToValidate.GetType().Name));

                return FdwResult<TCommand>.Failure("Invalid configuration.");
            }
        }

        return FdwResult<TCommand>.Success(cmd);
    }

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    public async Task<IFdwResult<T>> Execute<T>(TCommand command)
    {
        var startTime = DateTime.UtcNow;
        var correlationId = command?.CorrelationId ?? Guid.NewGuid();

        using (_logger.BeginScope(new Dictionary<string, object>(StringComparer.Ordinal) { ["CorrelationId"] = correlationId }))
        {
            ServiceBaseLog.ExecutingCommand(_logger, command?.GetType().Name ?? "null", ServiceType);

            // Validate the command
            if (command == null)
            {
                ServiceBaseLog.InvalidCommandType(_logger);
                return FdwResult<T>.Failure("Invalid command type");
            }

            var validationResult = await ValidateCommand(command).ConfigureAwait(false);
            if (validationResult.Error)
            {
                ServiceBaseLog.CommandValidationFailed(_logger, validationResult.Message!);
                return FdwResult<T>.Failure(validationResult.Message!);
            }

            try
            {
                // Execute the command
                var result = await ExecuteCore<T>(validationResult.Value!).ConfigureAwait(false);

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                if (result.IsSuccess)
                {
                    ServiceBaseLog.CommandExecuted(_logger, command.GetType().Name, duration);
                }
                else
                {
                    ServiceBaseLog.CommandFailed(_logger, command.GetType().Name, result.Message ?? "Unknown error");
                }

                return result;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                ServiceBaseLog.OperationFailed(_logger, command.GetType().Name, ex.Message, ex);

                return FdwResult<T>.Failure(
                    "Operation failed.");
            }
        }
    }

    /// <summary>
    /// Executes the core command logic.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The validated command to execute.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    protected abstract Task<IFdwResult<T>> ExecuteCore<T>(TCommand command);


    #region Implementation of IFdwService


    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    [SuppressMessage("CA1848", "Use the LoggerMessage delegates", Justification = "Dynamic message with variable content")]
    [SuppressMessage("CA1727", "Use PascalCase for named placeholders", Justification = "Existing placeholder format")]
    public async Task<IFdwResult> Execute(ICommand command, CancellationToken cancellationToken)
    {
        if (command is TCommand cmd) return await Execute(cmd, cancellationToken).ConfigureAwait(false);
#pragma warning disable CA1848 // Use the LoggerMessage delegates
        _logger.LogWarning("Invalid command for {Type}: {Command}", nameof(ServiceBase<TCommand, TConfiguration, TService>), command);
#pragma warning restore CA1848 // Use the LoggerMessage delegates
        ServiceBaseLog.InvalidCommandType(_logger);
        return FdwResult.Failure("Invalid command type");

    }

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TOut">The type of result the command should return.</typeparam>
    /// <returns>A task containing the result of the command execution.</returns>
    [SuppressMessage("CA1848", "Use the LoggerMessage delegates", Justification = "Dynamic message with variable content")]
    [SuppressMessage("CA1727", "Use PascalCase for named placeholders", Justification = "Existing placeholder format")]
    public async Task<IFdwResult<TOut>> Execute<TOut>(ICommand command, CancellationToken cancellationToken)
    {
        if (command is TCommand cmd) return await Execute<TOut>(cmd, cancellationToken).ConfigureAwait(false);
#pragma warning disable CA1848 // Use the LoggerMessage delegates
        _logger.LogWarning("Invalid command for {Type}: {Command}",
            nameof(ServiceBase<TCommand, TConfiguration, TService>), command);
#pragma warning restore CA1848 // Use the LoggerMessage delegates
        ServiceBaseLog.InvalidCommandType(_logger);
        return FdwResult<TOut>.Failure("Invalid command type");

    }

    #endregion

    #region Implementation of IFdwService<in TCommand>

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    public async Task<IFdwResult> Execute(TCommand command)
    {
        var result = await Execute<object>(command, CancellationToken.None).ConfigureAwait(false);
        return result.IsSuccess ? FdwResult.Success() : FdwResult.Failure(result.Message!);
    }

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <typeparam name="TOut">The type of result returned by the command.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    public abstract Task<IFdwResult<TOut>> Execute<TOut>(TCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    public abstract Task<IFdwResult> Execute(TCommand command, CancellationToken cancellationToken);

    #endregion
}