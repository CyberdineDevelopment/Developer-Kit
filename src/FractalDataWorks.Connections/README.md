# FractalDataWorks.Connections

Connection abstractions and implementations for data sources and messaging systems in the FractalDataWorks framework.

## Overview

FractalDataWorks.Connections provides:
- Abstractions for various connection types (database, messaging, APIs)
- Connection management patterns
- Retry and resilience policies
- Connection pooling abstractions
- Health check integration

## Planned Components

### IFractalConnection

Base interface for all connection types:
```csharp
public interface IFractalConnection : IDisposable
{
    string Name { get; }
    ConnectionState State { get; }
    bool IsConnected { get; }
    Task<FractalResult> ConnectAsync(CancellationToken cancellationToken = default);
    Task<FractalResult> DisconnectAsync(CancellationToken cancellationToken = default);
    Task<FractalResult> ValidateConnectionAsync(CancellationToken cancellationToken = default);
}
```

### IDatabaseConnection

Database-specific connection abstraction:
```csharp
public interface IDatabaseConnection : IFractalConnection
{
    string ConnectionString { get; }
    int CommandTimeout { get; }
    Task<FractalResult<T>> ExecuteAsync<T>(string query, object? parameters = null);
    Task<FractalResult<IEnumerable<T>>> QueryAsync<T>(string query, object? parameters = null);
}
```

### IMessageConnection

Messaging system connection abstraction:
```csharp
public interface IMessageConnection : IFractalConnection
{
    Task<FractalResult> PublishAsync<T>(T message, string topic = "") where T : class;
    Task<FractalResult> SubscribeAsync<T>(Func<T, Task> handler, string topic = "") where T : class;
    Task<FractalResult> UnsubscribeAsync(string subscriptionId);
}
```

### IApiConnection

HTTP API connection abstraction:
```csharp
public interface IApiConnection : IFractalConnection
{
    Uri BaseUri { get; }
    TimeSpan Timeout { get; }
    Task<FractalResult<T>> GetAsync<T>(string endpoint);
    Task<FractalResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
}
```

## Planned Features

### Connection Factory
```csharp
public interface IConnectionFactory
{
    T CreateConnection<T>(IFractalConfiguration configuration) where T : IFractalConnection;
}
```

### Connection Pool
```csharp
public interface IConnectionPool<T> where T : IFractalConnection
{
    Task<T> GetConnectionAsync();
    Task ReturnConnectionAsync(T connection);
    int ActiveConnections { get; }
    int AvailableConnections { get; }
}
```

### Retry Policies
Integration with Polly for resilient connections:
```csharp
public class RetryableConnection<T> : IFractalConnection where T : IFractalConnection
{
    private readonly T _innerConnection;
    private readonly IAsyncPolicy _retryPolicy;
    
    public async Task<FractalResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(
            async () => await _innerConnection.ConnectAsync(cancellationToken));
    }
}
```

## Usage Examples (Planned)

### Database Connection
```csharp
public class SqlServerConnection : DatabaseConnectionBase
{
    public SqlServerConnection(DatabaseConfiguration config) : base(config)
    {
    }
    
    protected override async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}
```

### Message Bus Connection
```csharp
public class ServiceBusConnection : MessageConnectionBase
{
    private readonly ServiceBusClient _client;
    
    public ServiceBusConnection(ServiceBusConfiguration config) : base(config)
    {
        _client = new ServiceBusClient(config.ConnectionString);
    }
    
    public override async Task<FractalResult> PublishAsync<T>(T message, string topic = "")
    {
        var sender = _client.CreateSender(topic);
        await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(message)));
        return FractalResult.Success();
    }
}
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.Connections" Version="*" />
```

## Dependencies

- FractalDataWorks.net (core abstractions)
- Polly (for retry policies)
- Microsoft.Extensions.Logging.Abstractions

## Status

This package is currently in planning phase. The interfaces and implementations described above represent the intended design and may change during development.

## Contributing

This package is accepting contributions for:
- Connection interface definitions
- Base implementation classes
- Connection pool implementations
- Integration with specific data sources
- Unit and integration tests