using System;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Commands;
using FractalDataWorks.Configuration;
using FractalDataWorks.Messages;
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
public abstract class ServiceBase<TConfiguration, TCommand, TService> : IFdwService<TConfiguration, TCommand>
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
    where TCommand : ICommand
    where TService : class
{
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

        _logger.LogWarning(
            ServiceMessages.InvalidConfiguration.Format(configuration?.GetType().Name ?? "null"));

        validConfiguration = GetInvalidConfiguration();
        return FdwResult<TConfiguration>.Failure(
            ServiceMessages.InvalidConfiguration.Format(ServiceName));
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
                ServiceMessages.InvalidId.Format(configurationId));
        }

        if (!_configurations.TryGet(configurationId, out var config))
        {
            return FdwResult<TConfiguration>.Failure(
                ServiceMessages.ConfigurationNotFound.Format(configurationId));
        }

        return ConfigurationIsValid(config!, out _);
    }

    /// <summary>
    /// Validates a command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>The validation result.</returns>
    protected async Task<FdwResult<TCommand>> ValidateCommand(ICommand command)
    {
        if (command is not TCommand cmd)
        {
            _logger.LogWarning(
                ServiceMessages.InvalidCommand.Format(command?.GetType().Name ?? "null"));

            return FdwResult<TCommand>.Failure(
                ServiceMessages.InvalidCommand.Format(ServiceName));
        }

        // Validate the command itself
        var validationResult = await cmd.Validate();
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return FdwResult<TCommand>.Failure(errors);
        }

        // Validate command configuration if present
        if (cmd.Configuration is TConfiguration config && !config.IsValid)
        {
            _logger.LogWarning(
                ServiceMessages.InvalidConfiguration.Format("Command configuration"));

            return FdwResult<TCommand>.Failure(
                ServiceMessages.InvalidConfiguration.Format(ServiceName));
        }

        return FdwResult<TCommand>.Success(cmd);
    }

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task containing the result of the command execution.</returns>
    public async Task<FdwResult<T>> Execute<T>(TCommand command)
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
                return FdwResult<T>.Failure(ServiceMessages.InvalidCommand.Format("null"));
            }

            var validationResult = await ValidateCommand(command);
            if (validationResult.IsFailure)
            {
                return FdwResult<T>.Failure(validationResult.Error!);
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

                return FdwResult<T>.Failure(
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
    protected abstract Task<FdwResult<T>> ExecuteCore<T>(TCommand command);

    /// <summary>
    /// Gets the invalid configuration instance for this service type.
    /// </summary>
    /// <returns>The invalid configuration instance.</returns>
    protected virtual TConfiguration GetInvalidConfiguration()
    {
        return new TConfiguration { IsEnabled = false };
    }
}