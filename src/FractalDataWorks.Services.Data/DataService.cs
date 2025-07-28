using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Data;
using FractalDataWorks.Entities;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Data;

/// <summary>
/// Service for executing data commands through various providers
/// </summary>
public class DataService : ServiceBase<DataConfiguration>, IDataService
{
    private readonly IDataServiceProvider _provider;
    
    public DataService(
        DataConfiguration configuration,
        IDataServiceProvider provider,
        ILogger<DataService> logger) 
        : base(configuration, logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }
    
    /// <summary>
    /// Executes a data command using the specified connection
    /// </summary>
    /// <typeparam name="TResult">The expected result type</typeparam>
    /// <param name="connectionId">The connection identifier from configuration</param>
    /// <param name="command">The command to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The execution result</returns>
    public async Task<Result<TResult>> Execute<TResult>(
        string connectionId, 
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use default connection if not specified
            if (string.IsNullOrEmpty(connectionId))
            {
                connectionId = Configuration.DefaultConnectionId;
                if (string.IsNullOrEmpty(connectionId))
                {
                    return Result<TResult>.Fail("No connection ID specified and no default configured");
                }
            }
            
            _logger.LogDebug("Executing {CommandType} on connection {ConnectionId}", 
                command.GetType().Name, connectionId);
            
            // Get the connection
            var connectionResult = _provider.GetConnectionById(connectionId);
            if (!connectionResult.IsSuccess)
            {
                return Result<TResult>.Fail(connectionResult.Message);
            }
            
            // Execute the command
            var executionResult = await connectionResult.Value.Execute<TResult>(command, cancellationToken);
            
            if (executionResult.IsSuccess)
            {
                _logger.LogDebug("Command executed successfully on {ConnectionId}", connectionId);
                return Result<TResult>.Ok(executionResult.Value);
            }
            else
            {
                _logger.LogWarning("Command execution failed on {ConnectionId}: {Error}", 
                    connectionId, executionResult.Message);
                return Result<TResult>.Fail(executionResult.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command on {ConnectionId}", connectionId);
            return Result<TResult>.Fail($"Execution error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Executes a data command using the default connection
    /// </summary>
    public Task<Result<TResult>> Execute<TResult>(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        return Execute<TResult>(string.Empty, command, cancellationToken);
    }
    
    /// <summary>
    /// Tests a connection
    /// </summary>
    public Task<Result<bool>> TestConnection(
        string connectionId,
        CancellationToken cancellationToken = default)
    {
        return ConvertGenericResult(_provider.TestConnection(connectionId, cancellationToken));
    }
    
    /// <summary>
    /// Gets available connection IDs
    /// </summary>
    public Result<string[]> GetAvailableConnections()
    {
        var result = _provider.GetAvailableConnections();
        return result.IsSuccess 
            ? Result<string[]>.Ok(result.Value) 
            : Result<string[]>.Fail(result.Message);
    }
    
    private async Task<Result<T>> ConvertGenericResult<T>(Task<IGenericResult<T>> genericResultTask)
    {
        var result = await genericResultTask;
        return result.IsSuccess 
            ? Result<T>.Ok(result.Value) 
            : Result<T>.Fail(result.Message);
    }
}

/// <summary>
/// Interface for the data service
/// </summary>
public interface IDataService : IGenericService
{
    /// <summary>
    /// Executes a data command using the specified connection
    /// </summary>
    Task<Result<TResult>> Execute<TResult>(
        string connectionId, 
        IDataCommand command,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a data command using the default connection
    /// </summary>
    Task<Result<TResult>> Execute<TResult>(
        IDataCommand command,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tests a connection
    /// </summary>
    Task<Result<bool>> TestConnection(
        string connectionId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets available connection IDs
    /// </summary>
    Result<string[]> GetAvailableConnections();
}