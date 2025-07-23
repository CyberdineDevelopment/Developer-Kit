# FractalDataWorks.Connections

External connection implementations for the FractalDataWorks framework. This package provides the boundary implementations between the framework and external systems.

## Overview

FractalDataWorks.Connections provides:
- External connection base implementations
- Provider-specific connection implementations (SQL Server, PostgreSQL, MongoDB, etc.)
- Command builders that transform universal commands to provider-specific commands
- Connection parsers and factories
- Thread-safe connection lifecycle management

## Architecture

### External Connections as Boundaries

External connections represent the boundary between the FractalDataWorks framework and external systems. They are responsible for:
- **Command Transformation**: Converting universal commands (LINQ-like) to provider-specific commands
- **Protocol Handling**: Managing the specific protocol for each external system
- **Connection Management**: Handling connection lifecycle for the specific provider

### ExternalConnectionBase<TCommandBuilder, TCommand, TConnection, TFactory, TConfig>

Base class for provider-specific external connections:

```csharp
public abstract class ExternalConnectionBase<TCommandBuilder, TCommand, TConnection, TFactory, TConfig> 
    : IExternalConnection
    where TCommandBuilder : ICommandBuilder<IFdwDataCommand, TCommand>
    where TCommand : class
    where TConnection : class
    where TFactory : IConnectionFactory<TConnection, TConfig>
    where TConfig : IFdwConfiguration
{
    protected TCommandBuilder CommandBuilder { get; }
    protected TFactory ConnectionFactory { get; }
    protected TConfig Configuration { get; }
    
    // Transform universal command to provider-specific command
    protected virtual TCommand BuildCommand(IFdwDataCommand dataCommand)
    {
        return CommandBuilder.Build(dataCommand);
    }
    
    // Execute provider-specific command
    public abstract Task<FdwResult<T>> Execute<T>(IFdwDataCommand command);
}
```

### ConnectionBase

A simpler base class for basic connection lifecycle management:
```csharp
public abstract class ConnectionBase : IExternalConnection
{
    public string ConnectionId { get; }
    public ConnectionState State { get; protected set; }
    
    public abstract Task<bool> OpenAsync();
    public abstract Task<bool> CloseAsync();
    public abstract Task<bool> TestConnectionAsync();
}
```

### Key Components

#### ExternalConnectionProvider

Selects and manages appropriate external connections based on configuration:
```csharp
public class ExternalConnectionProvider : IExternalConnectionProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationRegistry<ConnectionConfiguration> _configurations;
    
    public async Task<IExternalConnection> GetConnection<TConnection>(string connectionId)
        where TConnection : IExternalConnection
    {
        var config = await _configurations.GetConfiguration(connectionId);
        return config.ProviderType switch
        {
            "SqlServer" => _serviceProvider.GetRequiredService<MsSqlConnection>(),
            "PostgreSQL" => _serviceProvider.GetRequiredService<PostgresConnection>(),
            "MongoDB" => _serviceProvider.GetRequiredService<MongoConnection>(),
            "API" => _serviceProvider.GetRequiredService<ApiConnection>(),
            "File" => _serviceProvider.GetRequiredService<FileConnection>(),
            _ => throw new NotSupportedException($"Provider {config.ProviderType} not supported")
        };
    }
}
```

#### Command Builders

Transform universal commands to provider-specific commands:
```csharp
public interface ICommandBuilder<TInput, TOutput>
{
    TOutput Build(TInput input);
}

public class SqlCommandBuilder : ICommandBuilder<IFdwDataCommand, SqlCommand>
{
    public SqlCommand Build(IFdwDataCommand dataCommand)
    {
        // Transform LINQ expression to SQL
        var sql = ExpressionToSql(dataCommand.QueryExpression);
        return new SqlCommand(sql);
    }
}
```

### Connection Messages

The package includes predefined messages for common connection scenarios:
- **ConnectionTimeout** - When connection attempts exceed the timeout period
- **ConnectionFailed** - General connection failure
- **NotAuthorized** - Authorization failures
- **InvalidCredentials** - Invalid connection credentials

## Provider Implementations

### SQL Server Connection

```csharp
public class MsSqlConnection : ExternalConnectionBase<SqlCommandBuilder, SqlCommand, SqlConnection, SqlConnectionFactory, SqlServerConfiguration>
{
    private SqlConnection? _connection;
    
    public MsSqlConnection(
        SqlCommandBuilder commandBuilder,
        SqlConnectionFactory connectionFactory,
        SqlServerConfiguration configuration)
        : base(commandBuilder, connectionFactory, configuration)
    {
    }
    
    public override async Task<FdwResult<T>> Execute<T>(IFdwDataCommand dataCommand)
    {
        // Transform universal command to SQL
        var sqlCommand = BuildCommand(dataCommand);
        
        // Get connection from factory
        using var connection = await ConnectionFactory.GetConnection(Configuration);
        
        // Execute SQL command
        return dataCommand.Operation switch
        {
            DataOperation.Query => await ExecuteQuery<T>(connection, sqlCommand),
            DataOperation.Insert => await ExecuteInsert<T>(connection, sqlCommand),
            DataOperation.Update => await ExecuteUpdate<T>(connection, sqlCommand),
            DataOperation.Delete => await ExecuteDelete<T>(connection, sqlCommand),
            _ => FdwResult<T>.Failure("Unsupported operation")
        };
    }
    
    private async Task<FdwResult<T>> ExecuteQuery<T>(SqlConnection connection, SqlCommand command)
    {
        // Execute query and map results
        command.Connection = connection;
        using var reader = await command.ExecuteReaderAsync();
        var results = MapResults<T>(reader);
        return FdwResult<T>.Success(results);
    }
}
}
```

### Other Provider Examples

#### MongoDB Connection
```csharp
public class MongoConnection : ExternalConnectionBase<MongoCommandBuilder, BsonDocument, IMongoDatabase, MongoConnectionFactory, MongoConfiguration>
{
    public override async Task<FdwResult<T>> Execute<T>(IFdwDataCommand dataCommand)
    {
        // Transform LINQ to MongoDB query
        var bsonQuery = BuildCommand(dataCommand);
        var database = await ConnectionFactory.GetConnection(Configuration);
        var collection = database.GetCollection<T>(dataCommand.EntityType);
        
        // Execute MongoDB operation
        return await ExecuteMongoOperation<T>(collection, bsonQuery, dataCommand.Operation);
    }
}
```

#### API Connection
```csharp
public class ApiConnection : ExternalConnectionBase<ApiCommandBuilder, HttpRequestMessage, HttpClient, HttpClientFactory, ApiConfiguration>
{
    public override async Task<FdwResult<T>> Execute<T>(IFdwDataCommand dataCommand)
    {
        // Transform to HTTP request
        var httpRequest = BuildCommand(dataCommand);
        var httpClient = await ConnectionFactory.GetConnection(Configuration);
        
        // Execute HTTP request
        var response = await httpClient.SendAsync(httpRequest);
        return await MapApiResponse<T>(response);
    }
}
```

#### File Connection
```csharp
public class FileConnection : ExternalConnectionBase<FileCommandBuilder, FileOperation, FileStream, FileConnectionFactory, FileConfiguration>
{
    public override async Task<FdwResult<T>> Execute<T>(IFdwDataCommand dataCommand)
    {
        // Transform to file operation
        var fileOp = BuildCommand(dataCommand);
        
        // Execute file operation (CSV, JSON, XML, etc.)
        return await ExecuteFileOperation<T>(fileOp);
    }
}
```

## Key Concepts

### Universal to Provider-Specific Transformation

The architecture enables writing provider-agnostic code:
```csharp
// This same command works with SQL, MongoDB, API, or Files
var command = new FdwDataCommand
{
    Operation = DataOperation.Query,
    EntityType = "Customer",
    QueryExpression = customers => customers.Where(c => c.City == "Seattle" && c.Active),
    ConnectionId = "primary" // Could point to any provider
};

// The DataConnection service handles routing to the correct provider
var result = await dataConnection.Execute<IEnumerable<Customer>>(command);
```

### Provider Registration

```csharp
// Register providers
services.AddScoped<MsSqlConnection>();
services.AddScoped<PostgresConnection>();
services.AddScoped<MongoConnection>();
services.AddScoped<ApiConnection>();
services.AddScoped<FileConnection>();

// Register command builders
services.AddSingleton<SqlCommandBuilder>();
services.AddSingleton<MongoCommandBuilder>();
services.AddSingleton<ApiCommandBuilder>();
services.AddSingleton<FileCommandBuilder>();

// Register connection provider
services.AddSingleton<IExternalConnectionProvider, ExternalConnectionProvider>();
```

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