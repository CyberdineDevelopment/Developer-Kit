using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.ExternalConnections.Http;

/// <summary>
/// HTTP external connection service implementation.
/// </summary>
public sealed class HttpExternalConnectionService 
    : ExternalConnectionServiceBase<IExternalConnectionCommand, HttpConnectionConfiguration, HttpExternalConnectionService>, IFdwService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<string, IExternalConnection> _connections;
    private readonly string _serviceId;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpExternalConnectionService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="loggerFactory">The logger factory for creating connection loggers.</param>
    /// <param name="configuration">The HTTP service configuration.</param>
    public HttpExternalConnectionService(
        ILogger<HttpExternalConnectionService> logger,
        ILoggerFactory loggerFactory,
        HttpConnectionConfiguration configuration)
        : base(logger, configuration)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _connections = new Dictionary<string, IExternalConnection>(StringComparer.Ordinal);
        _serviceId = Guid.NewGuid().ToString("N");
    }

    /// <inheritdoc/>
    public override string Id => _serviceId;

    /// <inheritdoc/>
    public override string ServiceType => "HTTP External Connection Service";

    /// <inheritdoc/>
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(IExternalConnectionCommand command)
    {
        var result = await ExecuteInternal(command, CancellationToken.None).ConfigureAwait(false);
        
        if (result.IsSuccess && result.Value is T typedValue)
        {
            return FdwResult<T>.Success(typedValue);
        }
        
        if (result.IsSuccess)
        {
            // Try to convert the result to the expected type
            try
            {
                var convertedValue = (T)(object)result.Value!;
                return FdwResult<T>.Success(convertedValue);
            }
            catch (InvalidCastException)
            {
                return FdwResult<T>.Failure($"Unable to convert result to type {typeof(T).Name}");
            }
        }
        
        return FdwResult<T>.Failure(result.Message ?? "Command execution failed");
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult<TOut>> Execute<TOut>(IExternalConnectionCommand command, CancellationToken cancellationToken)
    {
        var result = await ExecuteInternal(command, cancellationToken).ConfigureAwait(false);
        
        if (result.IsSuccess && result.Value is TOut typedValue)
        {
            return FdwResult<TOut>.Success(typedValue);
        }
        
        if (result.IsSuccess)
        {
            // Try to convert the result to the expected type
            try
            {
                var convertedValue = (TOut)(object)result.Value!;
                return FdwResult<TOut>.Success(convertedValue);
            }
            catch (InvalidCastException)
            {
                return FdwResult<TOut>.Failure($"Unable to convert result to type {typeof(TOut).Name}");
            }
        }
        
        return FdwResult<TOut>.Failure(result.Message ?? "Command execution failed");
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult> Execute(IExternalConnectionCommand command, CancellationToken cancellationToken)
    {
        var result = await ExecuteInternal(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? FdwResult.Success() : FdwResult.Failure(result.Message!);
    }

    /// <summary>
    /// Internal method for executing external connection commands.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The command execution result.</returns>
    private async Task<IFdwResult<object>> ExecuteInternal(IExternalConnectionCommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        Logger.LogDebug("Executing {CommandType} command", command.GetType().Name);

        try
        {
            // HTTP command execution implementation pending
            Logger.LogInformation("HTTP command execution not yet implemented");
            await Task.Delay(50, cancellationToken).ConfigureAwait(false);
            return FdwResult<object>.Success(new { Success = true });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute HTTP command");
            return FdwResult<object>.Failure($"HTTP command execution failed: {ex.Message}");
        }
    }
}
