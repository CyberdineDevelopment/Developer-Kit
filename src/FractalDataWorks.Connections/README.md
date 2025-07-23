# FractalDataWorks.Connections

🚧 **IN PROGRESS** - Enhanced Enum Type Factories implementation in progress

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

## Enhanced Enum Type Factories

🚧 **IN PROGRESS** - New pattern for connection type registration using Enhanced Enums

### Overview

The Enhanced Enum Type Factories pattern uses source generators to create strongly-typed connection registrations:

```csharp
[EnumOption(1, "SqlServer", "Microsoft SQL Server connection")]
public class SqlServerConnectionType : ConnectionTypeBase<MsSqlConnection, SqlServerConfiguration>
{
    public SqlServerConnectionType() : base(1, "SqlServer", "Microsoft SQL Server connection")
    {
    }

    public override object Create(SqlServerConfiguration configuration)
    {
        return new MsSqlConnection(_commandBuilder, _connectionFactory, configuration);
    }

    public override Task<MsSqlConnection> GetConnection(string configurationName)
    {
        // Implementation to retrieve connection by configuration name
    }

    public override Task<MsSqlConnection> GetConnection(int configurationId)
    {
        // Implementation to retrieve connection by configuration ID
    }
}
```

### ConnectionTypeBase Pattern

The new pattern introduces two base classes:
- **ConnectionTypeFactoryBase<TConnection, TConfiguration>**: Non-generic base with factory methods (no Enhanced Enum attributes)
- **ConnectionTypeBase<TConnection, TConfiguration>**: Enhanced Enum base with `[EnhancedEnumBase]` attribute

### Benefits

1. **Compile-time Safety**: Connection types are generated at compile time
2. **IntelliSense Support**: Full IDE support for ConnectionTypes.* collections
3. **Automatic DI Registration**: Connections are automatically registered with dependency injection
4. **Factory Pattern**: Each connection type acts as a factory for creating connection instances

### Usage with Dependency Injection

```csharp
// Register all connection types in an assembly
services.AddConnectionTypes(Assembly.GetExecutingAssembly());

// Connection types are registered as both themselves and their factory interfaces
var sqlFactory = provider.GetService<IConnectionFactory<MsSqlConnection, SqlServerConfiguration>>();
var connection = sqlFactory.Create(sqlConfig);
```

### Generated Collections

Enhanced Enums generates static collections for easy access:

```csharp
// Access all connection types
var allConnections = ConnectionTypes.All;

// Get by ID
var sqlConnection = ConnectionTypes.GetById(1);

// Get by name
var mongoConnection = ConnectionTypes.GetByName("MongoDB");

// Iterate through all
foreach (var connectionType in ConnectionTypes.All)
{
    Console.WriteLine($"{connectionType.Id}: {connectionType.Name}");
}
```

### Example Implementation

```csharp
[EnumOption(2, "PostgreSQL", "PostgreSQL database connection")]
public class PostgresConnectionType : ConnectionTypeBase<PostgresConnection, PostgresConfiguration>
{
    private readonly ICommandBuilder<IFdwDataCommand, NpgsqlCommand> _commandBuilder;
    private readonly IConnectionFactory<NpgsqlConnection, PostgresConfiguration> _connectionFactory;
    
    public PostgresConnectionType(
        ICommandBuilder<IFdwDataCommand, NpgsqlCommand> commandBuilder,
        IConnectionFactory<NpgsqlConnection, PostgresConfiguration> connectionFactory) 
        : base(2, "PostgreSQL", "PostgreSQL database connection")
    {
        _commandBuilder = commandBuilder;
        _connectionFactory = connectionFactory;
    }

    public override object Create(PostgresConfiguration configuration)
    {
        return new PostgresConnection(_commandBuilder, _connectionFactory, configuration);
    }

    public override async Task<PostgresConnection> GetConnection(string configurationName)
    {
        var config = await _configurationRegistry.GetByName(configurationName);
        return new PostgresConnection(_commandBuilder, _connectionFactory, config);
    }

    public override async Task<PostgresConnection> GetConnection(int configurationId)
    {
        var config = await _configurationRegistry.GetById(configurationId);
        return new PostgresConnection(_commandBuilder, _connectionFactory, config);
    }
}
```

## Contributing

This package welcomes contributions for:
- Additional connection type implementations
- Connection pool implementations
- Integration with specific data sources
- Unit and integration tests
- Performance optimizations