# FractalDataWorks.Connections

Connection abstractions and implementations for data sources and messaging systems in the FractalDataWorks framework.

## Overview

FractalDataWorks.Connections provides:
- Base connection abstractions with lifecycle management
- Thread-safe connection state management
- Connection timeout handling
- Integration with the service framework
- Extensible connection patterns for various data sources

## Current Implementation

### ConnectionBase<TConfiguration, TCommand, TConnection>

A fully implemented abstract base class that provides:
- **Connection lifecycle management** - Connect, Disconnect, and Test operations
- **Thread-safe state management** - Using SemaphoreSlim for concurrent access control
- **Timeout handling** - Configurable connection timeouts with automatic cancellation
- **Service integration** - Inherits from ServiceBase for command execution
- **Disposable pattern** - Both synchronous and asynchronous disposal support

```csharp
public abstract class ConnectionBase<TConfiguration, TCommand, TConnection> 
    : ServiceBase<TConfiguration, TCommand, TConnection>, IFdwConnection
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
    where TCommand : ICommand
    where TConnection : class
{
    // Properties
    public Guid ConnectionId { get; }
    public bool IsConnected { get; protected set; }
    public DateTimeOffset? ConnectedAt { get; protected set; }
    public DateTimeOffset? DisconnectedAt { get; protected set; }
    public string ConnectionString { get; protected set; }
    
    // Core connection methods
    public async Task<FdwResult> ConnectAsync(string connectionString, CancellationToken cancellationToken = default);
    public async Task<FdwResult> DisconnectAsync(CancellationToken cancellationToken = default);
    public async Task<FdwResult> TestConnectionAsync(CancellationToken cancellationToken = default);
    
    // Abstract methods for implementation
    protected abstract Task<FdwResult> OnConnectAsync(string connectionString, CancellationToken cancellationToken);
    protected abstract Task<FdwResult> OnDisconnectAsync(CancellationToken cancellationToken);
    protected abstract Task<FdwResult> OnTestConnectionAsync(CancellationToken cancellationToken);
    protected abstract Task<FdwResult<T>> OnExecuteCommandAsync<T>(TCommand command);
}
```

### Connection Interfaces

**IConnection** - Represents an active connection:
```csharp
public interface IConnection : IDisposable, IAsyncDisposable
{
    Guid ConnectionId { get; }
    bool IsOpen { get; }
    DateTimeOffset EstablishedAt { get; }
    Task CloseAsync(CancellationToken cancellationToken = default);
}
```

**IConnectionProvider** - Factory for creating connections:
```csharp
public interface IConnectionProvider
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
    void ReturnConnection(IConnection connection);
}
```

**IFdwConnection** - Framework-specific connection interface:
```csharp
public interface IFdwConnection : IDisposable, IAsyncDisposable
{
    Guid ConnectionId { get; }
    bool IsConnected { get; }
    DateTimeOffset? ConnectedAt { get; }
    DateTimeOffset? DisconnectedAt { get; }
    string ConnectionString { get; }
    
    Task<FdwResult> ConnectAsync(string connectionString, CancellationToken cancellationToken = default);
    Task<FdwResult> DisconnectAsync(CancellationToken cancellationToken = default);
    Task<FdwResult> TestConnectionAsync(CancellationToken cancellationToken = default);
}
```

### Connection Messages

The package includes predefined messages for common connection scenarios:
- **ConnectionTimeout** - When connection attempts exceed the timeout period
- **ConnectionFailed** - General connection failure
- **NotAuthorized** - Authorization failures
- **InvalidCredentials** - Invalid connection credentials

## Usage Examples

### Creating a Database Connection

```csharp
public class SqlServerConnection : ConnectionBase<SqlServerConfiguration, SqlCommand, SqlServerConnection>
{
    private SqlConnection? _connection;
    
    public SqlServerConnection(
        ILogger<SqlServerConnection> logger,
        IConfigurationRegistry<SqlServerConfiguration> configurations)
        : base(logger, configurations)
    {
    }
    
    protected override async Task<FdwResult> OnConnectAsync(
        string connectionString, 
        CancellationToken cancellationToken)
    {
        try
        {
            _connection = new SqlConnection(connectionString);
            await _connection.OpenAsync(cancellationToken);
            return FdwResult.Success();
        }
        catch (SqlException ex)
        {
            return FdwResult.Failure(ConnectionMessages.ConnectionFailed.Format(ex.Message));
        }
    }
    
    protected override async Task<FdwResult> OnDisconnectAsync(CancellationToken cancellationToken)
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
            _connection = null;
        }
        return FdwResult.Success();
    }
    
    protected override async Task<FdwResult> OnTestConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection?.State == ConnectionState.Open)
        {
            // Execute a simple query to test the connection
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);
            return FdwResult.Success();
        }
        return FdwResult.Failure(ConnectionMessages.ConnectionFailed);
    }
    
    protected override async Task<FdwResult<T>> OnExecuteCommandAsync<T>(SqlCommand command)
    {
        // Implementation for executing SQL commands
        // ...
    }
}
```

### Using the Connection

```csharp
// Setup
var logger = serviceProvider.GetRequiredService<ILogger<SqlServerConnection>>();
var configurations = serviceProvider.GetRequiredService<IConfigurationRegistry<SqlServerConfiguration>>();
var connection = new SqlServerConnection(logger, configurations);

// Connect
var connectResult = await connection.ConnectAsync(connectionString);
if (!connectResult.IsSuccess)
{
    logger.LogError("Failed to connect: {Error}", connectResult.Message);
    return;
}

// Test connection
var testResult = await connection.TestConnectionAsync();
logger.LogInformation("Connection test: {Result}", testResult.IsSuccess ? "Success" : "Failed");

// Execute commands through the service interface
var command = new QueryCustomersCommand { Active = true };
var result = await connection.Execute<List<Customer>>(command);

// Disconnect when done
await connection.DisconnectAsync();

// Or use disposal
await connection.DisposeAsync();
```

## Planned Features

### Connection Pooling
- Efficient connection reuse
- Configurable pool sizes
- Health check integration

### Retry Policies
- Integration with Polly for resilient connections
- Configurable retry strategies
- Circuit breaker patterns

### Additional Connection Types
- Message bus connections (Service Bus, RabbitMQ)
- HTTP API connections
- NoSQL database connections
- Cache connections (Redis, etc.)

## Installation

```xml
<PackageReference Include="FractalDataWorks.Connections" Version="*" />
```

## Dependencies

- FractalDataWorks.net (core abstractions)
- FractalDataWorks.Services (service base class)
- FractalDataWorks.Configuration (configuration management)
- Microsoft.Extensions.Logging.Abstractions

## Contributing

This package welcomes contributions for:
- Additional connection type implementations
- Connection pool implementations
- Integration with specific data sources
- Unit and integration tests
- Performance optimizations