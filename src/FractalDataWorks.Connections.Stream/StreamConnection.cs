using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Connections;
using FractalDataWorks.Connections.Stream.Messages;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections.Stream;

/// <summary>
/// Represents a connection to a stream (file, memory, network, etc.).
/// </summary>
public class StreamConnection : ConnectionBase<StreamCommand, StreamConnectionConfiguration, StreamConnection>
{
    private readonly StreamConnectionConfiguration _configuration;
    private System.IO.Stream? _stream;
    private readonly object _streamLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnection"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurations">The configuration registry.</param>
    public StreamConnection(
        ILogger<StreamConnection>? logger,
        IConfigurationRegistry<StreamConnectionConfiguration> configurations)
        : base(logger, configurations)
    {
        _configuration = Configuration;
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult> OnConnectAsync(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            lock (_streamLock)
            {
                if (_stream != null && _stream.CanRead)
                {
                    return FdwResult.Success();
                }
            }

            // For streams, the connection string can override the configured path
            if (!string.IsNullOrWhiteSpace(connectionString) && _configuration.StreamType == StreamType.File)
            {
                _configuration.Path = connectionString;
            }

            var stream = await CreateStreamAsync(cancellationToken).ConfigureAwait(false);
            
            lock (_streamLock)
            {
                _stream?.Dispose();
                _stream = stream;
            }

            return FdwResult.Success();
        }
        catch (Exception)
        {
            return FdwResult.Failure(new StreamOperationFailed());
        }
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult> OnDisconnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            lock (_streamLock)
            {
                _stream?.Dispose();
                _stream = null;
            }

            return await Task.FromResult(FdwResult.Success());
        }
        catch (Exception)
        {
            return FdwResult.Failure(new StreamOperationFailed());
        }
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult> OnTestConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            System.IO.Stream? stream;
            lock (_streamLock)
            {
                stream = _stream;
            }

            if (stream == null)
            {
                return FdwResult.Failure(new StreamNotConnected());
            }

            // Test if we can still use the stream
            if (stream.CanRead || stream.CanWrite)
            {
                return await Task.FromResult(FdwResult.Success());
            }

            return FdwResult.Failure(new StreamOperationFailed());
        }
        catch (Exception)
        {
            return FdwResult.Failure(new StreamOperationFailed());
        }
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult<T>> OnExecuteCommandAsync<T>(StreamCommand command)
    {
        try
        {
            System.IO.Stream? stream;
            lock (_streamLock)
            {
                stream = _stream;
            }

            if (stream == null)
            {
                return FdwResult<T>.Failure(new StreamNotConnected());
            }

            return command.Operation switch
            {
                StreamOperation.Read => await ExecuteReadAsync<T>(stream, command, CancellationToken.None),
                StreamOperation.Write => await ExecuteWriteAsync<T>(stream, command, CancellationToken.None),
                StreamOperation.Seek => await ExecuteSeekAsync<T>(stream, command, CancellationToken.None),
                StreamOperation.GetInfo => await ExecuteGetInfoAsync<T>(stream, command, CancellationToken.None),
                _ => FdwResult<T>.Failure(new StreamOperationFailed())
            };
        }
        catch (Exception)
        {
            return FdwResult<T>.Failure(new StreamOperationFailed());
        }
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(StreamCommand command)
    {
        // Delegate to OnExecuteCommandAsync which handles the actual stream operations
        return await OnExecuteCommandAsync<T>(command);
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult<TOut>> Execute<TOut>(StreamCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateCommand(command);
        if (!validationResult.IsSuccess)
        {
            return FdwResult<TOut>.Failure(validationResult.Message!);
        }

        return await ExecuteCore<TOut>(validationResult.Value);
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult> Execute(StreamCommand command, CancellationToken cancellationToken)
    {
        var result = await Execute<object>(command, cancellationToken);
        return result.IsSuccess ? FdwResult.Success() : FdwResult.Failure(result.Message!);
    }

    private Task<System.IO.Stream> CreateStreamAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<System.IO.Stream>(_configuration.StreamType switch
        {
            StreamType.File => new FileStream(
                _configuration.Path ?? throw new InvalidOperationException("Path is required for file streams"),
                _configuration.FileMode,
                _configuration.FileAccess,
                _configuration.FileShare,
                _configuration.BufferSize,
                _configuration.UseAsync),
            
            StreamType.Memory => new MemoryStream(_configuration.InitialCapacity),
            
            _ => throw new NotSupportedException($"Stream type {_configuration.StreamType} is not supported")
        });
    }

    private async Task<IFdwResult<TResult>> ExecuteReadAsync<TResult>(
        System.IO.Stream stream,
        StreamCommand command,
        CancellationToken cancellationToken)
    {
        if (!stream.CanRead)
        {
            return FdwResult<TResult>.Failure(new StreamOperationFailed());
        }

        var buffer = new byte[command.BufferSize ?? _configuration.BufferSize];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
        
        if (bytesRead == 0)
        {
            return FdwResult<TResult>.Success(default(TResult)!);
        }

        var data = new byte[bytesRead];
        Array.Copy(buffer, data, bytesRead);
        
        if (typeof(TResult) == typeof(byte[]))
        {
            return FdwResult<TResult>.Success((TResult)(object)data);
        }
        
        return FdwResult<TResult>.Failure(new StreamOperationFailed());
    }

    private async Task<IFdwResult<TResult>> ExecuteWriteAsync<TResult>(
        System.IO.Stream stream,
        StreamCommand command,
        CancellationToken cancellationToken)
    {
        if (!stream.CanWrite)
        {
            return FdwResult<TResult>.Failure(new StreamOperationFailed());
        }

        if (command.Data == null)
        {
            return FdwResult<TResult>.Failure(new StreamOperationFailed());
        }

        await stream.WriteAsync(command.Data, 0, command.Data.Length, cancellationToken).ConfigureAwait(false);
        
        if (_configuration.AutoFlush)
        {
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        if (typeof(TResult) == typeof(int))
        {
            return FdwResult<TResult>.Success((TResult)(object)command.Data.Length);
        }
        
        return FdwResult<TResult>.Success(default(TResult)!);
    }

    private async Task<IFdwResult<TResult>> ExecuteSeekAsync<TResult>(
        System.IO.Stream stream,
        StreamCommand command,
        CancellationToken cancellationToken)
    {
        if (!stream.CanSeek)
        {
            return FdwResult<TResult>.Failure(new StreamOperationFailed());
        }

        var newPosition = stream.Seek(command.Position ?? 0, command.SeekOrigin ?? SeekOrigin.Begin);
        
        if (typeof(TResult) == typeof(long))
        {
            return await Task.FromResult(FdwResult<TResult>.Success((TResult)(object)newPosition));
        }
        
        return await Task.FromResult(FdwResult<TResult>.Success(default(TResult)!));
    }

    private async Task<IFdwResult<TResult>> ExecuteGetInfoAsync<TResult>(
        System.IO.Stream stream,
        StreamCommand command,
        CancellationToken cancellationToken)
    {
        var info = new StreamInfo
        {
            CanRead = stream.CanRead,
            CanWrite = stream.CanWrite,
            CanSeek = stream.CanSeek,
            Length = stream.CanSeek ? stream.Length : null,
            Position = stream.CanSeek ? stream.Position : null,
            StreamType = _configuration.StreamType
        };
        
        if (typeof(TResult) == typeof(StreamInfo))
        {
            return await Task.FromResult(FdwResult<TResult>.Success((TResult)(object)info));
        }
        
        return await Task.FromResult(FdwResult<TResult>.Failure(new StreamOperationFailed()));
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_streamLock)
            {
                _stream?.Dispose();
                _stream = null;
            }
        }
        
        base.Dispose(disposing);
    }
}