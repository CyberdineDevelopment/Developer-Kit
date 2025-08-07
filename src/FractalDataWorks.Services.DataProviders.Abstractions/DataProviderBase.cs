using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Data;
using FractalDataWorks.Services;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Base class for data providers that executes data commands.
/// </summary>
/// <remarks>
/// This base class provides the foundation for all data providers in the framework.
/// It handles command execution, validation, and logging for data operations.
/// </remarks>
public abstract class DataProviderBase : ServiceBase<IDataCommand, IDataConfiguration, DataProviderBase>, IDataProvider
{
    /// <summary>
    /// Gets the types of data commands this provider can execute.
    /// </summary>
    /// <value>A collection of command type names supported by this provider.</value>
    public IReadOnlyList<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the data sources this provider can work with.
    /// </summary>
    /// <value>A collection of data source identifiers supported by this provider.</value>
    public IReadOnlyList<string> SupportedDataSources { get; }
    
    /// <summary>
    /// Gets the external connection used by this provider.
    /// </summary>
    /// <value>The external connection instance, or null if not connection-based.</value>
    public IExternalConnection? Connection { get; protected set; }
    
    /// <summary>
    /// Gets a value indicating whether the service is currently available for use.
    /// </summary>
    /// <value><c>true</c> if the service is available; otherwise, <c>false</c>.</value>
    public override bool IsAvailable => Connection?.State == FdwConnectionState.Open || Connection == null;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProviderBase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for this data provider.</param>
    /// <param name="configuration">The provider configuration.</param>
    /// <param name="supportedCommandTypes">The command types this provider can execute.</param>
    /// <param name="supportedDataSources">The data sources this provider can work with.</param>
    /// <param name="connection">The external connection used by this provider (optional).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when any string parameter is empty or whitespace, or when collections are empty.
    /// </exception>
    protected DataProviderBase(
        ILogger<DataProviderBase> logger,
        IDataConfiguration configuration,
        IReadOnlyList<string> supportedCommandTypes,
        IReadOnlyList<string> supportedDataSources,
        IExternalConnection? connection = null) 
        : base(logger, configuration)
    {
        if (supportedCommandTypes.Count == 0)
        {
            throw new ArgumentException("At least one supported command type must be specified.", nameof(supportedCommandTypes));
        }
        
        if (supportedDataSources.Count == 0)
        {
            throw new ArgumentException("At least one supported data source must be specified.", nameof(supportedDataSources));
        }
        
        // Validate that all command types are not null or empty
        for (int i = 0; i < supportedCommandTypes.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(supportedCommandTypes[i]))
            {
                throw new ArgumentException($"Command type at index {i} cannot be null, empty, or whitespace.", nameof(supportedCommandTypes));
            }
        }
        
        // Validate that all data sources are not null or empty
        for (int i = 0; i < supportedDataSources.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(supportedDataSources[i]))
            {
                throw new ArgumentException($"Data source at index {i} cannot be null, empty, or whitespace.", nameof(supportedDataSources));
            }
        }
        
        SupportedCommandTypes = supportedCommandTypes;
        SupportedDataSources = supportedDataSources;
        Connection = connection;
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<TOut>> Execute<TOut>(IDataCommand command, CancellationToken cancellationToken);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult> Execute(IDataCommand command, CancellationToken cancellationToken);
    
    /// <inheritdoc />
    protected abstract Task<IFdwResult<T>> ExecuteCore<T>(IDataCommand command);
    
    /// <inheritdoc />
    public virtual IFdwResult ValidateCommand(IDataCommand command)
    {
        
        // Check if this provider supports the command type
        bool supportsCommandType = false;
        for (int i = 0; i < SupportedCommandTypes.Count; i++)
        {
            if (string.Equals(SupportedCommandTypes[i], command.CommandType, StringComparison.OrdinalIgnoreCase))
            {
                supportsCommandType = true;
                break;
            }
        }
        
        if (!supportsCommandType)
        {
            DataProviderBaseLog.UnsupportedCommandType(Logger, Name, command.CommandType);
            return FdwResult.Failure($"Provider '{Name}' does not support command type '{command.CommandType}'.");
        }
        
        // Validate the command itself
        var commandValidation = command.Validate();
        if (!commandValidation.IsSuccess)
        {
            return commandValidation;
        }
        
        // Perform provider-specific validation
        return ValidateProviderSpecificCommand(command);
    }
    
    /// <summary>
    /// Performs provider-specific command validation.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>A result indicating whether the command is valid for this provider.</returns>
    /// <remarks>
    /// Override this method in derived classes to implement provider-specific
    /// validation logic beyond basic command type checking.
    /// </remarks>
    protected virtual IFdwResult ValidateProviderSpecificCommand(IDataCommand command)
    {
        return FdwResult.Success();
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<IProviderMetrics>> GetMetricsAsync();
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<IDataTransaction>> BeginTransactionAsync();
}