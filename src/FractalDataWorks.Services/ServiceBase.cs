using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Commands;
using FractalDataWorks.Configuration;
using FractalDataWorks.Messages;
using FractalDataWorks.Results;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for all services with automatic validation and logging.
/// </summary>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
/// <typeparam name="TCommand">The command type.</typeparam>
public abstract class ServiceBase<TConfiguration, TCommand> : IFractalService<TConfiguration, TCommand>
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
    where TCommand : ICommand
{
    private readonly ILogger _logger;
    private readonly IConfigurationRegistry<TConfiguration> _configurations;
    private readonly TConfiguration _primaryConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBase{TConfiguration, TCommand}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurations">The configuration registry.</param>
    protected ServiceBase(
        ILogger logger,
        IConfigurationRegistry<TConfiguration> configurations)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));

        // Validate we have at least one configuration
        var allConfigs = _configurations.GetAll();
        if (!allConfigs.Any())
        {
            _logger.LogError(ServiceMessages.InvalidConfiguration.Message, "No configurations available");
            _primaryConfiguration = GetInvalidConfiguration();
        }
        else
        {
            _primaryConfiguration = allConfigs.FirstOrDefault(c => c.IsEnabled && c.IsValid) 
                                  ?? GetInvalidConfiguration();
        }

        _logger.LogInformation(
            ServiceMessages.ServiceStarted.Message, 
            ServiceName);
    }

    /// <summary>
    /// Gets the service name.
    /// </summary>
    public virtual string ServiceName => GetType().Name;

    /// <summary>
    /// Gets whether the service is in a healthy state.
    /// </summary>
    public virtual bool IsHealthy => _primaryConfiguration.IsValid;

    /// <summary>
    /// Gets the service configuration.
    /// </summary>
    public TConfiguration Configuration => _primaryConfiguration;

    /// <summary>
    /// Validates a configuration.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <param name="validConfiguration">The valid configuration if successful.</param>
    /// <returns>The validation result.</returns>
    protected FractalResult<TConfiguration> ConfigurationIsValid(
        IFractalConfiguration configuration, 
        out TConfiguration validConfiguration)
    {
        if (configuration is TConfiguration config && config.IsValid)
        {
            validConfiguration = config;
            return FractalResult<TConfiguration>.Success(config);
        }

        _logger.LogWarning(
            ServiceMessages.InvalidConfiguration.Format(configuration?.GetType().Name ?? "null"));

        validConfiguration = GetInvalidConfiguration();
        return FractalResult<TConfiguration>.Failure(
            ServiceMessages.InvalidConfiguration.Format(ServiceName));
    }

    /// <summary>
    /// Validates a configuration by ID.
    /// </summary>
    /// <param name="configurationId">The configuration ID.</param>
    /// <returns>The validation result.</returns>
    protected FractalResult<TConfiguration> ConfigurationIsValid(int configurationId)
    {
        if (configurationId <= 0)
        {
            return FractalResult<TConfiguration>.Failure(
                ServiceMessages.InvalidId.Format(configurationId));
        }

        if (!_configurations.TryGet(configurationId, out var config))
        {
            return FractalResult<TConfiguration>.Failure(
                ServiceMessages.ConfigurationNotFound.Format(configurationId));
        }

        return ConfigurationIsValid(config!, out _);
    }

    /// <summary>
    /// Validates a command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>The validation result.</returns>
    protected async Task<FractalResult<TCommand>> ValidateCommand(ICommand command)
    {
        if (command is not TCommand cmd)
        {
            _logger.LogWarning(
                ServiceMessages.InvalidCommand.Format(command?.GetType().Name ?? "null"));

            return FractalResult<TCommand>.Failure(
                ServiceMessages.InvalidCommand.Format(ServiceName));
        }

        // Validate the command itself
        var validationResult = await cmd.Validate();
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return FractalResult<TCommand>.Failure(errors);
        }

        // Validate command configuration if present
        if (cmd.Configuration is TConfiguration config && !config.IsValid)
        {
            _logger.LogWarning(
                ServiceMessages.InvalidConfiguration.Format("Command configuration"));

            return FractalResult<TCommand>.Failure(
                ServiceMessages.InvalidConfiguration.Format(ServiceName));
        }

        return FractalResult<TCommand>.Success(cmd);
    }

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    public async Task<FractalResult<T>> Execute<T>(TCommand command)
    {
        var startTime = DateTime.UtcNow;
        var correlationId = command?.CorrelationId ?? Guid.NewGuid();

        using (_logger.BeginScope("CorrelationId", correlationId))
        {
            _logger.LogDebug("Executing command {CommandType} in {Service}", 
                command?.GetType().Name ?? "null", 
                ServiceName);

            // Validate the command
            if (command == null)
            {
                return FractalResult<T>.Failure(ServiceMessages.InvalidCommand.Format("null"));
            }
            
            var validationResult = await ValidateCommand(command);
            if (validationResult.IsFailure)
            {
                return FractalResult<T>.Failure(validationResult.Error!);
            }

            try
            {
                // Execute the command
                var result = await ExecuteCore<T>(validationResult.Value!);

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        ServiceMessages.CommandExecuted.Format(command.GetType().Name, duration));
                }
                else
                {
                    _logger.LogWarning(
                        ServiceMessages.CommandFailed.Format(command.GetType().Name, result.Error ?? "Unknown error"));
                }

                return result;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                _logger.LogError(ex, 
                    ServiceMessages.OperationFailed.Format(command.GetType().Name, ex.Message));

                return FractalResult<T>.Failure(
                    ServiceMessages.OperationFailed.Format(command.GetType().Name, ex.Message));
            }
        }
    }

    /// <summary>
    /// Executes the core command logic.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The validated command to execute.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    protected abstract Task<FractalResult<T>> ExecuteCore<T>(TCommand command);

    /// <summary>
    /// Gets the invalid configuration instance for this service type.
    /// </summary>
    /// <returns>The invalid configuration instance.</returns>
    protected virtual TConfiguration GetInvalidConfiguration()
    {
        return new TConfiguration { IsEnabled = false };
    }
}

/// <summary>
/// Defines the contract for configuration registries.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration managed by this registry.</typeparam>
public interface IConfigurationRegistry<TConfiguration>
    where TConfiguration : IFractalConfiguration
{
    /// <summary>
    /// Gets a configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <returns>The configuration if found; otherwise, null.</returns>
    TConfiguration? Get(int id);

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    /// <returns>All available configurations.</returns>
    IEnumerable<TConfiguration> GetAll();

    /// <summary>
    /// Tries to get a configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <param name="configuration">The configuration if found; otherwise, null.</param>
    /// <returns>True if the configuration was found; otherwise, false.</returns>
    bool TryGet(int id, out TConfiguration? configuration);
}
