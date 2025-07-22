using System;
using System.Collections.Generic;
using System.Linq;
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
public abstract class ServiceBase<TConfiguration, TCommand, TService> : IFdwService<TConfiguration, TCommand>
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
    where TCommand : ICommand
    where TService : class
{
    private static readonly Action<ILogger, string, Exception?> _logServiceStarted =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(ServiceBase<TConfiguration, TCommand, TService>)),
            ServiceMessages.ServiceStarted.Message);

    private static readonly Action<ILogger, string, Exception?> _logInvalidConfiguration =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, "InvalidConfiguration"),
            "Invalid configuration: {Message}");

    private static readonly Action<ILogger, string, Exception?> _logInvalidConfigurationWarning =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(3, "InvalidConfigurationWarning"),
            "{Message}");

    private static readonly Action<ILogger, string, string, Exception?> _logExecutingCommand =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(4, "ExecutingCommand"),
            "Executing command {CommandType} in {Service}");

    private static readonly Action<ILogger, string, double, Exception?> _logCommandExecuted =
        LoggerMessage.Define<string, double>(
            LogLevel.Information,
            new EventId(5, "CommandExecuted"),
            "Command {CommandType} executed successfully in {Duration}ms");

    private static readonly Action<ILogger, string, string, Exception?> _logCommandFailed =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(6, "CommandFailed"),
            "Command {CommandType} failed: {Error}");

    private static readonly Action<ILogger, string, string, Exception?> _logOperationFailed =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(7, "OperationFailed"),
            "Operation {OperationType} failed: {Error}");
    private readonly ILogger<TService> _logger;
    private readonly IConfigurationRegistry<TConfiguration> _configurations;
    private readonly TConfiguration _primaryConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBase{TConfiguration, TCommand, TService}"/> class.
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
            _logInvalidConfiguration(_logger, "No configurations available", null);
            _primaryConfiguration = new TConfiguration { IsEnabled = false };
        }
        else
        {
            _primaryConfiguration = allConfigs.FirstOrDefault(c => c.IsEnabled && c.IsValid)
                                  ?? new TConfiguration { IsEnabled = false };
        }

        _logServiceStarted(_logger, typeof(TService).Name, null);
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

        _logInvalidConfigurationWarning(_logger, 
            ConfigurationMessages.InvalidConfiguration.Format(configuration?.GetType().Name ?? "null", "Not of expected type"), 
            null);

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
            _logInvalidConfigurationWarning(_logger,
                ServiceMessages.InvalidCommand.Format(command?.GetType().Name ?? "null"),
                null);

            return FdwResult<TCommand>.Failure(
                ServiceMessages.InvalidCommand);
        }

        // Validate the command itself
        var validationResult = await cmd.Validate().ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return FdwResult<TCommand>.Failure(
                ServiceMessages.ValidationFailed);
        }

        // Validate command configuration if present
        if (cmd.Configuration is TConfiguration config && !config.IsValid)
        {
            _logInvalidConfigurationWarning(_logger,
                ConfigurationMessages.InvalidConfiguration.Format("Command configuration", config.GetType().Name),
                null);

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
            _logExecutingCommand(_logger, command?.GetType().Name ?? "null", ServiceName, null);

            // Validate the command
            if (command == null)
            {
                return FdwResult<T>.Failure(ServiceMessages.InvalidCommand);
            }

            var validationResult = await ValidateCommand(command).ConfigureAwait(false);
            if (validationResult.IsFailure)
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
                    _logCommandExecuted(_logger, command.GetType().Name, duration, null);
                }
                else
                {
                    _logCommandFailed(_logger, command.GetType().Name, result.Message?.Message ?? "Unknown error", null);
                }

                return result;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                _logOperationFailed(_logger, command.GetType().Name, ex.Message, ex);

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
    Task<IFdwResult<T>> IFdwService<TConfiguration, TCommand, IFdwResult>.Execute<T>(TCommand command)
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
}