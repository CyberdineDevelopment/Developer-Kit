# FractalDataWorks.net

Core abstractions and base types for the FractalDataWorks framework. This package provides the foundational interfaces and types that all other FractalDataWorks packages depend on.

## Overview

FractalDataWorks.net is the Layer 0.5 foundation package that:
- Targets .NET Standard 2.0 for maximum compatibility
- Has zero external dependencies (except for enhanced enums and System.Collections.Immutable)
- Contains ALL core abstractions (interfaces) used throughout the framework
- Defines the messaging system using EnhancedEnums
- Serves as the single source of truth for all framework contracts

## Key Components

### Service Abstractions

#### IFdwService
Base interface for all services in the framework:
```csharp
public interface IFdwService
{
    string ServiceName { get; }
    bool IsHealthy { get; }
}

public interface IFdwService<TConfiguration> : IFdwService
    where TConfiguration : IFdwConfiguration
{
    TConfiguration Configuration { get; }
}

public interface IFdwService<TConfiguration, TCommand> : IFdwService<TConfiguration>
    where TConfiguration : IFdwConfiguration
    where TCommand : ICommand
{
    Task<FdwResult<T>> Execute<T>(TCommand command);
}
```

#### IServiceFactory
Factory abstraction for creating service instances:
```csharp
public interface IServiceFactory<TService>
{
    Task<TService> GetService(string configurationName);
    Task<TService> GetService(int configurationId);
}
```

### Configuration Abstractions

#### IFdwConfiguration
Base interface for all configuration types:
```csharp
public interface IFdwConfiguration
{
    int Id { get; set; }
    string Name { get; set; }
    string Version { get; set; }
    bool IsEnabled { get; set; }
    bool IsValid { get; }
    bool IsDefault { get; set; }
}
```

#### IConfigurationRegistry
Registry for managing multiple configurations:
```csharp
public interface IConfigurationRegistry<TConfiguration>
    where TConfiguration : IFdwConfiguration
{
    Task<TConfiguration> GetConfiguration(string name);
    Task<TConfiguration> GetConfiguration(int id);
    Task<IEnumerable<TConfiguration>> GetAllConfigurations();
}
```

### Result Pattern

#### IFdwResult & FdwResult<T>
Consistent result pattern for service operations:
```csharp
// Non-generic result
public interface IFdwResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    string? Error { get; }
}

// Generic result with value
public class FdwResult<T> : IFdwResult
{
    public T? Value { get; }
    
    public static FdwResult<T> Success(T value);
    public static FdwResult<T> Failure(string error);
}
```

### Validation

#### IFdwValidator<T>
Validation abstraction for consistent validation across the framework:
```csharp
public interface IFdwValidator<T>
{
    Task<IValidationResult> Validate(T instance);
    Task<FdwResult<T>> ValidateToResult(T instance);
}
```

### Messaging System

The messaging system uses EnhancedEnums for type-safe, discoverable messages:

```csharp
// Base message class
[EnhancedEnumOption("ServiceMessages")]
public abstract class ServiceMessage
{
    protected ServiceMessage(int id, string name, string code, 
        string message, ServiceLevel level, string category);
    
    [EnumLookup]
    public string Code { get; }
    public string Message { get; }
    [EnumLookup]
    public ServiceLevel Level { get; }
    [EnumLookup]
    public string Category { get; }
    
    public virtual string Format(params object[] parameters);
}

// Example message usage
[EnumOption]
public class ServiceStarted() : ServiceMessage(7, "ServiceStarted", 
    "SERVICE_STARTED", "Service '{0}' started successfully", 
    ServiceLevel.Information, "Service");
```

#### Available Message Categories
- **Validation**: ValidationFailed
- **Configuration**: InvalidConfiguration, ConfigurationNotFound
- **Connection**: ConnectionFailed, ConnectionTimeout, ConnectionSucceeded
- **Service**: ServiceStarted, ServiceStopped, ServiceHealthy, ServiceUnhealthy, ServiceDegraded
- **Command**: CommandExecuted, CommandFailed, InvalidCommand
- **Data**: RecordNotFound, InvalidId, DuplicateRecord, DataNotFound, DataCorrupted, DuplicateData
- **Operation**: OperationSucceeded, OperationFailed, RetryExhausted, OperationTimeout, OperationCancelled
- **Authorization**: UnauthorizedAccess, InsufficientPermissions

### Commands

#### ICommand
Base interface for command pattern implementation:
```csharp
public interface ICommand
{
    Guid CorrelationId { get; }
    DateTime Timestamp { get; }
    IFdwConfiguration? Configuration { get; }
    Task<IValidationResult> Validate();
}
```

### Connection Abstractions

#### IExternalConnection
Boundary interface for external system connections:
```csharp
public interface IExternalConnection
{
    string ConnectionId { get; }
    ConnectionState State { get; }
    Task<bool> OpenAsync();
    Task<bool> CloseAsync();
    Task<bool> TestConnectionAsync();
}
```

#### IFdwConnection
Framework wrapper for connections:
```csharp
public interface IFdwConnection<TConnection, TCommand>
    where TConnection : IExternalConnection
{
    TConnection ExternalConnection { get; }
    Task<FdwResult<T>> Execute<T>(TCommand command);
}
```

### Data Service Abstractions

#### IDataConnection
Universal data service interface:
```csharp
public interface IDataConnection : IFdwService<IFdwConfiguration, IFdwDataCommand>
{
    // Inherits Execute<T> method for data operations
}
```

#### IFdwDataCommand
Universal data command interface:
```csharp
public interface IFdwDataCommand : ICommand
{
    DataOperation Operation { get; } // Query, Insert, Update, Upsert, Delete
    string EntityType { get; }
    object QueryExpression { get; } // LINQ-like expression
    string ConnectionId { get; }
}
```

## Usage Examples

### Creating a Service
```csharp
public interface ICustomerService : IFdwService<CustomerConfiguration, CustomerCommand>
{
    // Service-specific methods
}
```

### Using Service Messages
```csharp
// Access messages through the generated ServiceMessages collection
_logger.LogInformation(ServiceMessages.ServiceStarted.Format("CustomerService"));
_logger.LogError(ServiceMessages.ConnectionFailed.Format(3, "Database unavailable"));

// Messages are strongly typed and discoverable
var allMessages = ServiceMessages.All;
var errorMessages = ServiceMessages.All.Where(m => m.Level == ServiceLevel.Error);
```

### Working with Results
```csharp
public async Task<FdwResult<Customer>> GetCustomerAsync(int id)
{
    if (id <= 0)
    {
        return FdwResult<Customer>.Failure(
            ServiceMessages.InvalidId.Format(id));
    }
    
    var customer = await _repository.GetAsync(id);
    if (customer == null)
    {
        return FdwResult<Customer>.Failure(
            ServiceMessages.RecordNotFound.Format("Customer", id));
    }
    
    return FdwResult<Customer>.Success(customer);
}
```

### Using the Universal Data Service
```csharp
// Create a data command
var query = new FdwDataCommand
{
    Operation = DataOperation.Query,
    EntityType = "Customer",
    QueryExpression = customers => customers.Where(c => c.City == "Seattle"),
    ConnectionId = "primary-db"
};

// Execute through data service
var result = await dataConnection.Execute<IEnumerable<Customer>>(query);
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.net" Version="*" />
```

## Dependencies

- FractalDataWorks.EnhancedEnums (for message system)
- System.Collections.Immutable (for immutable collections)
- .NET Standard 2.0

## Design Principles

1. **Zero External Dependencies**: Minimal dependencies to avoid version conflicts
2. **Interface-First Design**: All major components start with interfaces
3. **Immutability**: Prefer immutable types where appropriate
4. **Type Safety**: Use generics and strong typing throughout
5. **Discoverability**: EnhancedEnums provide compile-time discovery of messages

## Connection Abstractions and Integration Patterns

### Overview of Connection Architecture

The FractalDataWorks framework uses a layered connection architecture that separates concerns and enables flexibility:

1. **Framework Services** (`IFdwService`) - Business logic layer
2. **Universal Data Service** (`IDataConnection`) - Abstraction layer  
3. **External Connections** (`IExternalConnection`) - Provider boundary layer
4. **Native Providers** - Actual database/API/file systems

### Core Connection Interfaces

#### IExternalConnection - Provider Boundary
```csharp
public interface IExternalConnection
{
    string ConnectionId { get; }
    ConnectionState State { get; }
    Task<bool> OpenAsync();
    Task<bool> CloseAsync();
    Task<bool> TestConnectionAsync();
}
```

#### IFdwConnection - Framework Wrapper
```csharp
public interface IFdwConnection<TConnection, TCommand>
    where TConnection : IExternalConnection
{
    TConnection ExternalConnection { get; }
    Task<FdwResult<T>> Execute<T>(TCommand command);
}
```

### Integration Flow Patterns

#### Universal Data Service Flow

The framework enables writing provider-agnostic code through universal commands:

```csharp
// 1. Business Service creates universal command
var command = new FdwDataCommand
{
    Operation = DataOperation.Query,
    EntityType = "Customer", 
    QueryExpression = customers => customers.Where(c => c.City == "Seattle"),
    ConnectionId = "primary-db" // Could be SQL, MongoDB, API, etc.
};

// 2. DataConnection service receives command
public class DataConnection<TDataCommand, TConnection> : ServiceBase<DataConnectionConfiguration, TDataCommand>
{
    protected override async Task<FdwResult<T>> ExecuteCore<T>(TDataCommand command)
    {
        // 3. Get appropriate external connection from provider
        var connection = await _connectionProvider.GetConnection<TConnection>(command.ConnectionId);
        
        // 4. External connection transforms and executes
        return await connection.Execute<T>(command);
    }
}

// 5. External connection transforms universal command to provider-specific
public class SqlServerConnection : ExternalConnectionBase<SqlCommandBuilder, SqlCommand, SqlConnection>
{
    public override async Task<FdwResult<T>> Execute<T>(IFdwDataCommand dataCommand)
    {
        // Transform LINQ expression to SQL
        var sqlCommand = CommandBuilder.Build(dataCommand);
        
        // Execute with native provider
        using var connection = await ConnectionFactory.GetConnection(Configuration);
        return await ExecuteNativeSql<T>(connection, sqlCommand);
    }
}
```

#### Connection Provider Selection

The ExternalConnectionProvider handles dynamic provider selection:

```csharp
public class ExternalConnectionProvider : IExternalConnectionProvider
{
    public async Task<IExternalConnection> GetConnection<TConnection>(string connectionId)
    {
        var config = await _configurations.GetConfiguration(connectionId);
        
        // Route to appropriate provider based on configuration
        return config.ProviderType switch
        {
            "SqlServer" => _serviceProvider.GetRequiredService<SqlServerConnection>(),
            "PostgreSQL" => _serviceProvider.GetRequiredService<PostgresConnection>(), 
            "MongoDB" => _serviceProvider.GetRequiredService<MongoConnection>(),
            "RestApi" => _serviceProvider.GetRequiredService<RestApiConnection>(),
            "GraphQL" => _serviceProvider.GetRequiredService<GraphQLConnection>(),
            "CosmosDB" => _serviceProvider.GetRequiredService<CosmosConnection>(),
            "Redis" => _serviceProvider.GetRequiredService<RedisConnection>(),
            "File" => _serviceProvider.GetRequiredService<FileConnection>(),
            _ => throw new NotSupportedException($"Provider {config.ProviderType} not supported")
        };
    }
}
```

### Command Transformation Patterns

Each provider implements command builders that transform universal commands:

#### SQL Command Builder
```csharp
public class SqlCommandBuilder : ICommandBuilder<IFdwDataCommand, SqlCommand>
{
    public SqlCommand Build(IFdwDataCommand dataCommand)
    {
        var sql = dataCommand.Operation switch
        {
            DataOperation.Query => BuildSelectQuery(dataCommand),
            DataOperation.Insert => BuildInsertCommand(dataCommand),
            DataOperation.Update => BuildUpdateCommand(dataCommand), 
            DataOperation.Delete => BuildDeleteCommand(dataCommand),
            _ => throw new NotSupportedException($"Operation {dataCommand.Operation} not supported")
        };
        
        return new SqlCommand(sql);
    }
    
    private string BuildSelectQuery(IFdwDataCommand command)
    {
        // Transform LINQ expression to SQL SELECT
        var whereClause = ExpressionToSqlTranslator.Translate(command.QueryExpression);
        return $"SELECT * FROM {command.EntityType} WHERE {whereClause}";
    }
}
```

#### MongoDB Command Builder  
```csharp
public class MongoCommandBuilder : ICommandBuilder<IFdwDataCommand, BsonDocument>
{
    public BsonDocument Build(IFdwDataCommand dataCommand)
    {
        return dataCommand.Operation switch
        {
            DataOperation.Query => BuildFindQuery(dataCommand),
            DataOperation.Insert => BuildInsertDocument(dataCommand),
            DataOperation.Update => BuildUpdateDocument(dataCommand),
            DataOperation.Delete => BuildDeleteQuery(dataCommand),
            _ => throw new NotSupportedException($"Operation {dataCommand.Operation} not supported")
        };
    }
    
    private BsonDocument BuildFindQuery(IFdwDataCommand command)
    {
        // Transform LINQ expression to MongoDB query document
        var filter = ExpressionToMongoTranslator.Translate(command.QueryExpression);
        return new BsonDocument("find", command.EntityType).Add("filter", filter);
    }
}
```

#### REST API Command Builder
```csharp  
public class RestApiCommandBuilder : ICommandBuilder<IFdwDataCommand, HttpRequestMessage>
{
    public HttpRequestMessage Build(IFdwDataCommand dataCommand)
    {
        return dataCommand.Operation switch
        {
            DataOperation.Query => BuildGetRequest(dataCommand),
            DataOperation.Insert => BuildPostRequest(dataCommand),
            DataOperation.Update => BuildPutRequest(dataCommand),
            DataOperation.Delete => BuildDeleteRequest(dataCommand),
            _ => throw new NotSupportedException($"Operation {dataCommand.Operation} not supported")
        };
    }
    
    private HttpRequestMessage BuildGetRequest(IFdwDataCommand command)
    {
        // Transform LINQ expression to query parameters
        var queryString = ExpressionToQueryStringTranslator.Translate(command.QueryExpression);
        var url = $"/api/{command.EntityType}?{queryString}";
        
        return new HttpRequestMessage(HttpMethod.Get, url);
    }
}
```

### Provider Implementation Patterns

#### Database Connection Pattern
```csharp
public class SqlServerConnection : ExternalConnectionBase<SqlCommandBuilder, SqlCommand, SqlConnection, SqlConnectionFactory, SqlServerConfiguration>
{
    public override async Task<FdwResult<T>> Execute<T>(IFdwDataCommand dataCommand)
    {
        try
        {
            // 1. Transform command
            var sqlCommand = BuildCommand(dataCommand);
            
            // 2. Get native connection
            using var connection = await ConnectionFactory.GetConnection(Configuration);
            await connection.OpenAsync();
            
            // 3. Execute with appropriate method
            return dataCommand.Operation switch
            {
                DataOperation.Query => await ExecuteQuery<T>(connection, sqlCommand),
                DataOperation.Insert => await ExecuteInsert<T>(connection, sqlCommand),
                DataOperation.Update => await ExecuteUpdate<T>(connection, sqlCommand), 
                DataOperation.Delete => await ExecuteDelete<T>(connection, sqlCommand),
                _ => FdwResult<T>.Failure("Unsupported operation")
            };
        }
        catch (SqlException ex)
        {
            return FdwResult<T>.Failure($"SQL Error: {ex.Message}");
        }
    }
}
```

#### API Connection Pattern
```csharp
public class RestApiConnection : ExternalConnectionBase<RestApiCommandBuilder, HttpRequestMessage, HttpClient, HttpClientFactory, ApiConfiguration>
{
    public override async Task<FdwResult<T>> Execute<T>(IFdwDataCommand dataCommand)
    {
        try
        {
            // 1. Transform to HTTP request
            var httpRequest = BuildCommand(dataCommand);
            
            // 2. Get HTTP client
            var httpClient = await ConnectionFactory.GetConnection(Configuration);
            
            // 3. Execute request
            var response = await httpClient.SendAsync(httpRequest);
            
            // 4. Handle response
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content);
                return FdwResult<T>.Success(result!);
            }
            else
            {
                return FdwResult<T>.Failure($"API Error: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            return FdwResult<T>.Failure($"HTTP Error: {ex.Message}");
        }
    }
}
```

#### File System Connection Pattern
```csharp
public class FileConnection : ExternalConnectionBase<FileCommandBuilder, FileOperation, FileStream, FileConnectionFactory, FileConfiguration>
{
    public override async Task<FdwResult<T>> Execute<T>(IFdwDataCommand dataCommand)
    {
        try
        {
            // 1. Transform to file operation
            var fileOperation = BuildCommand(dataCommand);
            
            // 2. Execute based on file type and operation
            return Configuration.FileType switch
            {
                "CSV" => await ExecuteCsvOperation<T>(fileOperation),
                "JSON" => await ExecuteJsonOperation<T>(fileOperation),
                "XML" => await ExecuteXmlOperation<T>(fileOperation),
                "Parquet" => await ExecuteParquetOperation<T>(fileOperation),
                _ => FdwResult<T>.Failure("Unsupported file type")
            };
        }
        catch (IOException ex)
        {
            return FdwResult<T>.Failure($"File Error: {ex.Message}");
        }
    }
}
```

### Configuration Integration

Connections integrate with the configuration system for flexible deployment:

```csharp
// appsettings.json
{
    "ConnectionConfigurations": [
        {
            "Id": 1,
            "Name": "primary-db",
            "ProviderType": "SqlServer", 
            "ConnectionString": "Server=localhost;Database=MyApp;",
            "IsEnabled": true,
            "IsDefault": true
        },
        {
            "Id": 2,
            "Name": "analytics-db",
            "ProviderType": "MongoDB",
            "ConnectionString": "mongodb://localhost:27017/analytics",
            "IsEnabled": true
        },
        {
            "Id": 3,
            "Name": "customer-api",
            "ProviderType": "RestApi",
            "BaseUrl": "https://api.customers.com",
            "AuthenticationType": "Bearer",
            "IsEnabled": true
        }
    ]
}

// Registration
services.Configure<ConnectionConfiguration[]>(
    configuration.GetSection("ConnectionConfigurations"));

services.AddSingleton<IConfigurationRegistry<ConnectionConfiguration>>(provider =>
{
    var configs = provider.GetRequiredService<IOptions<ConnectionConfiguration[]>>();
    return new InMemoryConfigurationRegistry<ConnectionConfiguration>(configs.Value);
});
```

### Dependency Injection Integration

The framework provides extension methods for seamless DI registration:

```csharp
// Startup.cs
services.AddFractalDataWorksServices(configuration);

// Or manually register components
services.AddScoped<IDataConnection, DataConnection<IFdwDataCommand, IExternalConnection>>();
services.AddSingleton<IExternalConnectionProvider, ExternalConnectionProvider>();

// Register connection types
services.AddScoped<SqlServerConnection>();
services.AddScoped<PostgresConnection>();
services.AddScoped<MongoConnection>();
services.AddScoped<RestApiConnection>();
services.AddScoped<FileConnection>();

// Register command builders  
services.AddSingleton<SqlCommandBuilder>();
services.AddSingleton<MongoCommandBuilder>();
services.AddSingleton<RestApiCommandBuilder>();
services.AddSingleton<FileCommandBuilder>();

// Register connection factories
services.AddSingleton<SqlConnectionFactory>();
services.AddSingleton<HttpClientFactory>();
services.AddSingleton<FileConnectionFactory>();
```

### Testing Integration Patterns

The abstraction layers enable comprehensive testing:

#### Unit Testing Services
```csharp
[Test]
public async Task CustomerService_GetCustomer_ReturnsCustomer()
{
    // Arrange
    var mockDataConnection = new Mock<IDataConnection>();
    var expectedCustomer = new Customer { Id = 123, Name = "John Doe" };
    
    mockDataConnection
        .Setup(x => x.Execute<Customer>(It.IsAny<IFdwDataCommand>()))
        .ReturnsAsync(FdwResult<Customer>.Success(expectedCustomer));
    
    var service = new CustomerService(mockDataConnection.Object);
    var command = new GetCustomerCommand { CustomerId = 123 };
    
    // Act
    var result = await service.Execute<Customer>(command);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(expectedCustomer);
}
```

#### Integration Testing Connections
```csharp
[Test]
public async Task SqlServerConnection_ExecuteQuery_ReturnsData()
{
    // Arrange
    using var connection = new SqlConnection(TestConnectionString);
    var commandBuilder = new SqlCommandBuilder();
    var sqlConnection = new SqlServerConnection(commandBuilder, /* other deps */);
    
    var command = new FdwDataCommand
    {
        Operation = DataOperation.Query,
        EntityType = "Customers", 
        QueryExpression = c => c.Where(customer => customer.City == "Seattle")
    };
    
    // Act
    var result = await sqlConnection.Execute<IEnumerable<Customer>>(command);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
}
```

### Benefits of the Connection Architecture

1. **Provider Agnostic**: Write business logic once, run on any data store
2. **Testable**: Mock at appropriate abstraction levels
3. **Configurable**: Change providers without code changes
4. **Extensible**: Add new providers without touching existing code
5. **Type Safe**: Compile-time safety with runtime flexibility
6. **Performance**: Command transformation optimized per provider
7. **Consistent**: Same patterns across all connection types

## Contributing

This is a core package - changes here affect all dependent packages. Please ensure:
- All interfaces are well-documented with XML comments
- Breaking changes are avoided when possible
- New abstractions are discussed and approved before implementation
- Unit tests maintain high coverage