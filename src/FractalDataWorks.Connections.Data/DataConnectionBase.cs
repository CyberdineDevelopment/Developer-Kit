using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Connections;
using FractalDataWorks.Data;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections.Data;

/// <summary>
/// Abstract base class for data connections
/// </summary>
public abstract class DataConnectionBase<TConfiguration> : IDataConnection, IConfigurableConnection
    where TConfiguration : ConnectionConfiguration, new()
{
    protected readonly ILogger _logger;
    protected TConfiguration _configuration;
    
    protected DataConnectionBase(ILogger logger, TConfiguration? configuration = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? new TConfiguration();
    }
    
    /// <inheritdoc/>
    public abstract string ProviderName { get; }
    
    /// <inheritdoc/>
    public abstract ProviderCapabilities Capabilities { get; }
    
    /// <inheritdoc/>
    public virtual async Task<IGenericResult<TResult>> Execute<TResult>(
        IDataCommand command, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Executing {OperationType} command on {Provider}", 
                command.OperationType, ProviderName);
            
            // Validate command
            var validationResult = ValidateCommand(command);
            if (validationResult.IsFailure)
                return GenericResult<TResult>.Failure(validationResult.Message);
            
            // Route to appropriate handler
            return command switch
            {
                IQueryCommand<object> queryCmd => await ExecuteQuery<TResult>(queryCmd, cancellationToken),
                IInsertCommand<object> insertCmd => await ExecuteInsert<TResult>(insertCmd, cancellationToken),
                IUpdateCommand<object> updateCmd => await ExecuteUpdate<TResult>(updateCmd, cancellationToken),
                IDeleteCommand<object> deleteCmd => await ExecuteDelete<TResult>(deleteCmd, cancellationToken),
                _ => GenericResult<TResult>.Failure($"Unknown command type: {command.GetType().Name}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command");
            return GenericResult<TResult>.Failure($"Execution error: {ex.Message}");
        }
    }
    
    /// <inheritdoc/>
    public abstract Task<IGenericResult<bool>> TestConnection(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates a command before execution
    /// </summary>
    protected virtual IGenericResult<bool> ValidateCommand(IDataCommand command)
    {
        if (string.IsNullOrEmpty(command.DataStore))
            return GenericResult<bool>.Failure("DataStore is required");
            
        if (string.IsNullOrEmpty(command.Record))
            return GenericResult<bool>.Failure("Record is required");
            
        if (command.DataStore != ProviderName && command.DataStore != GetType().Name)
            return GenericResult<bool>.Failure($"Invalid DataStore '{command.DataStore}' for provider '{ProviderName}'");
            
        return GenericResult<bool>.Success(true);
    }
    
    /// <summary>
    /// Executes a query command
    /// </summary>
    protected abstract Task<IGenericResult<TResult>> ExecuteQuery<TResult>(
        IQueryCommand<object> command, 
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Executes an insert command
    /// </summary>
    protected abstract Task<IGenericResult<TResult>> ExecuteInsert<TResult>(
        IInsertCommand<object> command, 
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Executes an update command
    /// </summary>
    protected abstract Task<IGenericResult<TResult>> ExecuteUpdate<TResult>(
        IUpdateCommand<object> command, 
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Executes a delete command
    /// </summary>
    protected abstract Task<IGenericResult<TResult>> ExecuteDelete<TResult>(
        IDeleteCommand<object> command, 
        CancellationToken cancellationToken);
    
    /// <inheritdoc/>
    public virtual void ApplySettings(Dictionary<string, object> settings)
    {
        // Apply settings to configuration
        foreach (var setting in settings)
        {
            var property = typeof(TConfiguration).GetProperty(setting.Key);
            if (property != null && property.CanWrite)
            {
                try
                {
                    var value = Convert.ChangeType(setting.Value, property.PropertyType);
                    property.SetValue(_configuration, value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to apply setting {Key}", setting.Key);
                }
            }
        }
    }
    
    #region IDataConnection Legacy Methods
    
    public virtual Task<IGenericResult<IEnumerable<T>>> Query<T>(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default) where T : class
    {
        var command = QueryCommand<T>.Create()
            .From(ProviderName, _configuration.Settings.GetValueOrDefault("Container")?.ToString() ?? "", typeof(T).Name)
            .Where(predicate)
            .Build();
            
        return Execute<IEnumerable<T>>(command, cancellationToken);
    }
    
    public virtual Task<IGenericResult<T>> Single<T>(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default) where T : class
    {
        var command = QueryCommand<T>.Create()
            .From(ProviderName, _configuration.Settings.GetValueOrDefault("Container")?.ToString() ?? "", typeof(T).Name)
            .Where(predicate)
            .Take(1)
            .Build();
            
        return Execute<T>(command, cancellationToken);
    }
    
    public virtual Task<IGenericResult<T>> Insert<T>(
        T entity, 
        CancellationToken cancellationToken = default) where T : class
    {
        var command = InsertCommand<T>.Create()
            .Into(ProviderName, _configuration.Settings.GetValueOrDefault("Container")?.ToString() ?? "", typeof(T).Name)
            .WithEntity(entity)
            .Build();
            
        return Execute<T>(command, cancellationToken);
    }
    
    public virtual Task<IGenericResult<int>> Update<T>(
        Expression<Func<T, bool>> where, 
        Expression<Func<T, T>> update, 
        CancellationToken cancellationToken = default) where T : class
    {
        // This is complex - would need expression visitor to convert update expression
        throw new NotImplementedException("Use UpdateCommand with UpdateAction or UpdateValues instead");
    }
    
    public virtual Task<IGenericResult<int>> Delete<T>(
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default) where T : class
    {
        var command = DeleteCommand<T>.Create()
            .From(ProviderName, _configuration.Settings.GetValueOrDefault("Container")?.ToString() ?? "", typeof(T).Name)
            .Where(predicate)
            .Build();
            
        return Execute<int>(command, cancellationToken);
    }
    
    public virtual bool SupportsDataLayout(DataContainerDefinition definition)
    {
        // Override in derived classes for specific validation
        return true;
    }
    
    #endregion
}