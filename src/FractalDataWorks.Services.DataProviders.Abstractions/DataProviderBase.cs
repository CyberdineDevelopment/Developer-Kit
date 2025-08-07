using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks;

using FractalDataWorks.Services.ExternalConnections.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Base class for data providers that generates the DataProviders collection.
/// Integrates with the EnhancedEnums system for automatic service discovery and registration.
/// </summary>
/// <remarks>
/// This base class provides the foundation for all data providers in the framework.
/// It handles service registration, provider validation, and integration with the EnhancedEnums
/// system for automatic collection generation and lookup capabilities.
/// </remarks>
[EnumCollection(
    CollectionName = "DataProviders", 
    IncludeReferencedAssemblies = true,
    ReturnType = typeof(IDataProvider))]
public abstract class DataProviderBase : ServiceTypeBase<IDataProvider, DataConfiguration>, IDataProvider
{
    /// <summary>
    /// Gets the types of data commands this provider can execute.
    /// </summary>
    /// <value>A collection of command type names supported by this provider.</value>
    [EnumLookup("GetByCommandType", allowMultiple: true)]
    public IReadOnlyList<string> SupportedCommandTypes { get; }
    
    /// <summary>
    /// Gets the data sources this provider can work with.
    /// </summary>
    /// <value>A collection of data source identifiers supported by this provider.</value>
    [EnumLookup("GetByDataSource", allowMultiple: true)]
    public IReadOnlyList<string> SupportedDataSources { get; }
    
    /// <summary>
    /// Gets the external connection used by this provider.
    /// </summary>
    /// <value>The external connection instance, or null if not connection-based.</value>
    public IExternalConnection? Connection { get; protected set; }
    
    /// <summary>
    /// Gets the unique identifier for this service instance.
    /// </summary>
    /// <value>A unique identifier for the service instance.</value>
    public string ServiceId { get; }
    
    /// <summary>
    /// Gets the display name of the service.
    /// </summary>
    /// <value>A human-readable name for the service.</value>
    public string ServiceName => Name;
    
    /// <summary>
    /// Gets a value indicating whether the service is currently available for use.
    /// </summary>
    /// <value><c>true</c> if the service is available; otherwise, <c>false</c>.</value>
    public virtual bool IsAvailable => Connection?.State == FdwConnectionState.Open || Connection == null;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DataProviderBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this data provider.</param>
    /// <param name="name">The display name of this data provider.</param>
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
        int id, 
        string name, 
        IReadOnlyList<string> supportedCommandTypes,
        IReadOnlyList<string> supportedDataSources,
        IExternalConnection? connection = null) 
        : base(id, name, typeof(IDataProvider), typeof(DataConfiguration), "DataProvider")
    {
        ArgumentNullException.ThrowIfNull(supportedCommandTypes, nameof(supportedCommandTypes));
        ArgumentNullException.ThrowIfNull(supportedDataSources, nameof(supportedDataSources));
        
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
        ServiceId = $"DataProvider_{id}_{name}";
    }
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<object?>> Execute(IDataCommand command);
    
    /// <inheritdoc />
    public abstract Task<IFdwResult<TResult>> Execute<TResult>(IDataCommand<TResult> command);
    
    /// <inheritdoc />
    public virtual IFdwResult ValidateCommand(IDataCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
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
            return Logger.FailureWithLog($"Provider '{Name}' does not support command type '{command.CommandType}'.");
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
    
    /// <inheritdoc />
    public virtual async Task<IFdwResult<IBatchResult>> ExecuteBatch(IReadOnlyList<IDataCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(commands, nameof(commands));
        
        if (commands.Count == 0)
        {
            return Logger.FailureWithLog<IBatchResult>("Command list cannot be empty.");
        }
        
        // Default implementation executes commands sequentially
        // Override in derived classes for optimized batch processing
        var batchId = Guid.NewGuid().ToString();
        var startTime = DateTimeOffset.UtcNow;
        var commandResults = new List<ICommandResult>();
        
        int successCount = 0;
        int failCount = 0;
        
        try
        {
            for (int i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                var commandStartTime = DateTimeOffset.UtcNow;
                
                try
                {
                    var result = await Execute(command);
                    var commandEndTime = DateTimeOffset.UtcNow;
                    
                    if (result.IsSuccess)
                    {
                        successCount++;
                        commandResults.Add(CreateCommandResult(command, i, true, result.Value, null, null, null, 
                            commandEndTime - commandStartTime, commandStartTime, commandEndTime));
                    }
                    else
                    {
                        failCount++;
                        commandResults.Add(CreateCommandResult(command, i, false, null, result.ErrorMessage, 
                            result.ErrorDetails, result.Exception, commandEndTime - commandStartTime, 
                            commandStartTime, commandEndTime));
                    }
                }
                catch (Exception ex)
                {
                    var commandEndTime = DateTimeOffset.UtcNow;
                    failCount++;
                    commandResults.Add(CreateCommandResult(command, i, false, null, ex.Message, 
                        new[] { ex.ToString() }, ex, commandEndTime - commandStartTime, 
                        commandStartTime, commandEndTime));
                }
            }
            
            var endTime = DateTimeOffset.UtcNow;
            var batchResult = CreateBatchResult(batchId, commands.Count, successCount, failCount, 0,
                endTime - startTime, startTime, endTime, commandResults, Array.Empty<string>());
            
            return FdwResult<IBatchResult>.Success(batchResult);
        }
        catch (Exception ex)
        {
            var endTime = DateTimeOffset.UtcNow;
            return Logger.FailureWithLog<IBatchResult>($"Batch execution failed: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Creates a command result instance.
    /// </summary>
    /// <param name="command">The original command.</param>
    /// <param name="batchPosition">The position in the batch.</param>
    /// <param name="isSuccessful">Whether the command succeeded.</param>
    /// <param name="resultData">The result data.</param>
    /// <param name="errorMessage">The error message if failed.</param>
    /// <param name="errorDetails">Additional error details.</param>
    /// <param name="exception">The exception if one occurred.</param>
    /// <param name="executionTime">The command execution time.</param>
    /// <param name="startedAt">When the command started.</param>
    /// <param name="completedAt">When the command completed.</param>
    /// <returns>A command result instance.</returns>
    protected abstract ICommandResult CreateCommandResult(
        IDataCommand command, int batchPosition, bool isSuccessful, object? resultData,
        string? errorMessage, IReadOnlyList<string>? errorDetails, Exception? exception,
        TimeSpan executionTime, DateTimeOffset startedAt, DateTimeOffset completedAt);
    
    /// <summary>
    /// Creates a batch result instance.
    /// </summary>
    /// <param name="batchId">The batch identifier.</param>
    /// <param name="totalCommands">The total number of commands.</param>
    /// <param name="successfulCommands">The number of successful commands.</param>
    /// <param name="failedCommands">The number of failed commands.</param>
    /// <param name="skippedCommands">The number of skipped commands.</param>
    /// <param name="executionTime">The total execution time.</param>
    /// <param name="startedAt">When the batch started.</param>
    /// <param name="completedAt">When the batch completed.</param>
    /// <param name="commandResults">The individual command results.</param>
    /// <param name="batchErrors">Any batch-level errors.</param>
    /// <returns>A batch result instance.</returns>
    protected abstract IBatchResult CreateBatchResult(
        string batchId, int totalCommands, int successfulCommands, int failedCommands, int skippedCommands,
        TimeSpan executionTime, DateTimeOffset startedAt, DateTimeOffset completedAt,
        IReadOnlyList<ICommandResult> commandResults, IReadOnlyList<string> batchErrors);
    
    /// <inheritdoc />
    public virtual async Task<IFdwResult> HealthCheckAsync()
    {
        if (Connection != null)
        {
            var connectionResult = await Connection.TestConnectionAsync();
            return connectionResult.IsSuccess 
                ? FdwResult.Success() 
                : FdwResult.Failure(connectionResult.ErrorMessage ?? "Connection test failed");
        }
        
        return FdwResult.Success();
    }
    
    /// <inheritdoc />
    public override IDataProvider CreateService(IServiceProvider serviceProvider)
    {
        // Return this instance as it's already a data provider
        return this;
    }
    
    /// <summary>
    /// Creates a service instance (bridge to generic base).
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>A data provider instance.</returns>
    object IServiceType.CreateService(IServiceProvider serviceProvider) => CreateService(serviceProvider).GetAwaiter().GetResult();
}