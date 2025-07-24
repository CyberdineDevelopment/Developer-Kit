using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Configuration.Messages;
using FractalDataWorks.Messages;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for all services with automatic validation and logging.
/// </summary>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TService">The concrete service type for logging category.</typeparam>
public abstract class ServiceBase<TCommand,TConfiguration, TService> : IFdwService<TCommand>
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
    where TCommand : ICommand
    where TService : class
{
    private readonly ILogger<TService> _logger;
    private readonly IConfigurationRegistry<TConfiguration> _configurations;
    private readonly TConfiguration _primaryConfiguration;

    /// <inheritdoc/>
    public string Name => typeof(TService).Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBase{TCommand, TConfiguration, TService}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for the concrete service type. If null, uses Microsoft's NullLogger.</param>
    /// <param name="configurations">The configuration registry.</param>
    protected ServiceBase(
        ILogger<TService>? logger,
        IConfigurationRegistry<TConfiguration> configurations)
    {
        // Use Microsoft's NullLogger for consistency with ILogger<T> abstractions
        // This works seamlessly when Serilog is registered via services.AddSerilog()
        _logger = logger ?? NullLogger<TService>.Instance;
        _configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));

        // Validate we have at least one configuration
        var allConfigs = _configurations.GetAll();
        if (!allConfigs.Any())
        {
            ServiceBaseLog.InvalidConfiguration(_logger, "No configurations available");
            _primaryConfiguration = new TConfiguration { IsEnabled = false };
        }
        else
        {
            _primaryConfiguration = allConfigs.FirstOrDefault(c => c.IsEnabled && c.IsValid)
                                  ?? new TConfiguration { IsEnabled = false };
        }

        ServiceBaseLog.ServiceStarted(_logger, typeof(TService).Name);
    }

    /// <summary>
    /// Gets the service name.
    /// </summary>
    public virtual string ServiceName => typeof(TService).Name;

    /// <summary>
    /// Gets whether the service is in a healthy state.
    /// </summary>
    public virtual bool IsHealthy => _primaryConfiguration.IsValid;

    /// <summary>
    /// Gets the service configuration.
    /// </summary>
    public TConfiguration Configuration => _primaryConfiguration;

    /// <summary>
    /// Gets the logger instance for this service.
    /// </summary>
    protected ILogger<TService> Logger => _logger;

    /// <summary>
    /// Validates a configuration.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <param name="validConfiguration">The valid configuration if successful.</param>
    /// <returns>The validation result.</returns>
    protected FdwResult<TConfiguration> ConfigurationIsValid(
        IFdwConfiguration configuration,
        out TConfiguration validConfiguration)
    {
        if (configuration is TConfiguration config && config.IsValid)
        {
            validConfiguration = config;
            return FdwResult<TConfiguration>.Success(config);
        }

        ServiceBaseLog.InvalidConfigurationWarning(_logger, 
            ConfigurationMessages.InvalidConfiguration.Format(configuration?.GetType().Name ?? "null", "Not of expected type"));

        validConfiguration = GetInvalidConfiguration();
        return FdwResult<TConfiguration>.Failure(
            ConfigurationMessages.InvalidConfiguration);
    }

    /// <summary>
    /// Validates a configuration by ID.
    /// </summary>
    /// <param name="configurationId">The configuration ID.</param>
    /// <returns>The validation result.</returns>
    protected FdwResult<TConfiguration> ConfigurationIsValid(int configurationId)
    {
        if (configurationId <= 0)
        {
            return FdwResult<TConfiguration>.Failure(
                ServiceMessages.InvalidId);
        }

        if (!_configurations.TryGet(configurationId, out var config))
        {
            return FdwResult<TConfiguration>.Failure(
                ConfigurationMessages.ConfigurationNotFound);
        }

        return ConfigurationIsValid(config!, out _);
    }

    /// <summary>
    /// Validates a command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>The validation result.</returns>
    protected async Task<IFdwResult<TCommand>> ValidateCommand(ICommand command)
    {
        if (command is not TCommand cmd)
        {
            ServiceBaseLog.InvalidConfigurationWarning(_logger,
                ServiceMessages.InvalidCommand.Format(command?.GetType().Name ?? "null"));

            return FdwResult<TCommand>.Failure(
                ServiceMessages.InvalidCommand);
        }

        // Validate the command itself
        var validationResult = await cmd.Validate().ConfigureAwait(false);
        if (validationResult == null)
        {
            ServiceBaseLog.InvalidConfigurationWarning(_logger,
                ServiceMessages.ValidationFailed.Format("Validation returned null"));
            
            return FdwResult<TCommand>.Failure(
                ServiceMessages.ValidationFailed);
        }
        
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return FdwResult<TCommand>.Failure(
                ServiceMessages.ValidationFailed);
        }

        // Validate command configuration if present
        if (cmd.Configuration is TConfiguration config && !config.IsValid)
        {
            ServiceBaseLog.InvalidConfigurationWarning(_logger,
                ConfigurationMessages.InvalidConfiguration.Format("Command configuration", config.GetType().Name));

            return FdwResult<TCommand>.Failure(
                ConfigurationMessages.InvalidConfiguration);
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
            ServiceBaseLog.ExecutingCommand(_logger, command?.GetType().Name ?? "null", ServiceName);

            // Validate the command
            if (command == null)
            {
                return FdwResult<T>.Failure(ServiceMessages.InvalidCommand);
            }

            var validationResult = await ValidateCommand(command).ConfigureAwait(false);
            if (validationResult.Error)
            {
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
                    ServiceBaseLog.CommandFailed(_logger, command.GetType().Name, result.Message?.Message ?? "Unknown error");
                }

                return result;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                ServiceBaseLog.OperationFailed(_logger, command.GetType().Name, ex.Message, ex);

                return FdwResult<T>.Failure(
                    ServiceMessages.OperationFailed);
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

    /// <summary>
    /// Explicit interface implementation for the base interface with constraint.
    /// </summary>
    Task<IFdwResult<T>> IFdwService<TCommand>.Execute<T>(TCommand command, CancellationToken cancellationToken)
    {
        return Execute<T>(command);
    }

    /// <summary>
    /// Gets the invalid configuration instance for this service type.
    /// </summary>
    /// <returns>The invalid configuration instance.</returns>
    protected virtual TConfiguration GetInvalidConfiguration()
    {
        return new TConfiguration { IsEnabled = false };
    }

    #region Implementation of IFdwService


    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    public async Task<IFdwResult> Execute(ICommand command, CancellationToken cancellationToken)
    {
        if (command is TCommand cmd) return await Execute(cmd, cancellationToken).ConfigureAwait(false);
        Logger.LogWarning("Invalid command for {type}: {command}",nameof(ServiceBase<TCommand,TConfiguration,TService>),command);
        return FdwResult.Failure(ServiceMessages.InvalidCommand);

    }

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TOut">The type of result the command should return.</typeparam>
    /// <returns>A task containing the result of the command execution.</returns>
    public async Task<IFdwResult<TOut>> Execute<TOut>(ICommand command, CancellationToken cancellationToken)
    {
        if (command is TCommand cmd) return await Execute<TOut>(cmd).ConfigureAwait(false);
        Logger.LogWarning("Invalid command for {type}: {command}",
            nameof(ServiceBase<TCommand, TConfiguration, TService>), command);
        return FdwResult<TOut>.Failure(ServiceMessages.InvalidCommand);

    }

    #endregion

    #region Implementation of IFdwService<in TCommand>

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