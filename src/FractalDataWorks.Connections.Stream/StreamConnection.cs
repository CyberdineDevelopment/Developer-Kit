using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Connections;
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
    private readonly System.Threading.Lock _streamLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamConnection"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration instance.</param>
    public StreamConnection(
        ILogger<StreamConnection>? logger,
        StreamConnectionConfiguration configuration)
        : base(logger, configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult> OnConnectAsync(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            using (_streamLock.EnterScope())
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
            
            System.IO.Stream? oldStream;
            using (_streamLock.EnterScope())
            {
                oldStream = _stream;
                _stream = stream;
            }
            
            if (oldStream != null)
            {
                await oldStream.DisposeAsync().ConfigureAwait(false);
            }

            return FdwResult.Success();
        }
        catch (Exception)
        {
            StreamConnectionLog.StreamOperationFailed(Logger);
            return FdwResult.Failure("Stream operation failed");
        }
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult> OnDisconnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            System.IO.Stream? streamToDispose;
            using (_streamLock.EnterScope())
            {
                streamToDispose = _stream;
                _stream = null;
            }
            
            if (streamToDispose != null)
            {
                await streamToDispose.DisposeAsync().ConfigureAwait(false);
            }

            return FdwResult.Success();
        }
        catch (Exception)
        {
            return FdwResult.Failure("Stream operation failed");
        }
    }

    /// <inheritdoc/>
    protected override Task<IFdwResult> OnTestConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            System.IO.Stream? stream;
            using (_streamLock.EnterScope())
            {
                stream = _stream;
            }

            if (stream == null)
            {
                return FdwResult.Failure("Stream is not connected");
            }

            // Test if we can still use the stream
            if (stream.CanRead || stream.CanWrite)
            {
                return FdwResult.Success();
            }

            StreamConnectionLog.StreamOperationFailed(Logger);
            return FdwResult.Failure("Stream operation failed");
        }
        catch (Exception)
        {
            StreamConnectionLog.StreamOperationFailed(Logger);
            return FdwResult.Failure("Stream operation failed");
        }
    }

    /// <inheritdoc/>
    protected override async Task<IFdwResult<T>> OnExecuteCommandAsync<T>(StreamCommand command)
    {
        try
        {
            System.IO.Stream? stream;
            using (_streamLock.EnterScope())
            {
                stream = _stream;
            }

            if (stream == null)
            {
                return FdwResult<T>.Failure("Stream is not connected");
            }

            return command.Operation switch
            {
                StreamOperation.Read => await ExecuteReadAsync<T>(stream, command, CancellationToken.None).ConfigureAwait(false),
                StreamOperation.Write => await ExecuteWriteAsync<T>(stream, command, CancellationToken.None).ConfigureAwait(false),
                StreamOperation.Seek => await ExecuteSeekAsync<T>(stream, command, CancellationToken.None).ConfigureAwait(false),
                StreamOperation.GetInfo => await ExecuteGetInfoAsync<T>(stream, command, CancellationToken.None).ConfigureAwait(false),
                _ => FdwResult<T>.Failure("Stream operation failed")
            };
        }
        catch (Exception)
        {
            return FdwResult<T>.Failure("Stream operation failed");
        }
    }

    /// <inheritdoc/>
    protected override Task<IFdwResult<T>> ExecuteCore<T>(StreamCommand command)
    {
        // Delegate to OnExecuteCommandAsync which handles the actual stream operations
        return OnExecuteCommandAsync<T>(command);
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult<TOut>> Execute<TOut>(StreamCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateCommand(command).ConfigureAwait(false);
        if (!validationResult.IsSuccess)
        {
            StreamConnectionLog.StreamCommandValidationFailed(Logger, validationResult.Message!);
            return FdwResult<TOut>.Failure(validationResult.Message!);
        }

        return await ExecuteCore<TOut>(validationResult.Value).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult> Execute(StreamCommand command, CancellationToken cancellationToken)
    {
        var result = await Execute<object>(command, cancellationToken).ConfigureAwait(false);
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
            StreamConnectionLog.StreamOperationFailed(Logger);
            return FdwResult<TResult>.Failure("Stream operation failed");
        }

        var buffer = new byte[command.BufferSize ?? _configuration.BufferSize];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
        
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
        
        StreamConnectionLog.StreamOperationFailed(Logger);
        return FdwResult<TResult>.Failure("Stream operation failed");
    }

    private async Task<IFdwResult<TResult>> ExecuteWriteAsync<TResult>(
        System.IO.Stream stream,
        StreamCommand command,
        CancellationToken cancellationToken)
    {
        if (!stream.CanWrite)
        {
            StreamConnectionLog.StreamOperationFailed(Logger);
            return FdwResult<TResult>.Failure("Stream operation failed");
        }

        if (command.Data == null)
        {
            StreamConnectionLog.StreamOperationFailed(Logger);
            return FdwResult<TResult>.Failure("Stream operation failed");
        }

        await stream.WriteAsync(command.Data.AsMemory(), cancellationToken).ConfigureAwait(false);
        
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

    private Task<IFdwResult<TResult>> ExecuteSeekAsync<TResult>(
        System.IO.Stream stream,
        StreamCommand command,
        CancellationToken cancellationToken)
    {
        if (!stream.CanSeek)
        {
            StreamConnectionLog.StreamOperationFailed(Logger);
            return Task.FromResult(FdwResult<TResult>.Failure("Stream operation failed"));
        }

        var newPosition = stream.Seek(command.Position ?? 0, command.SeekOrigin ?? SeekOrigin.Begin);
        
        if (typeof(TResult) == typeof(long))
        {
            return Task.FromResult(FdwResult<TResult>.Success((TResult)(object)newPosition));
        }
        
        return Task.FromResult(FdwResult<TResult>.Success(default(TResult)!));
    }

    private Task<IFdwResult<TResult>> ExecuteGetInfoAsync<TResult>(
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
            return Task.FromResult(FdwResult<TResult>.Success((TResult)(object)info));
        }
        
        return Task.FromResult(FdwResult<TResult>.Failure("Stream operation failed"));
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            using (_streamLock.EnterScope())
            {
                _stream?.Dispose();
                _stream = null;
            }
        }
        
        base.Dispose(disposing);
    }
}