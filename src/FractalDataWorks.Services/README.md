# FractalDataWorks.Services

Service patterns and base implementations for the FractalDataWorks framework. This package provides the foundational service abstractions and base classes that simplify service development with built-in validation, logging, and error handling.

## Overview

FractalDataWorks.Services provides:
- Base service implementation with automatic validation
- Universal data service for all data operations
- Built-in command validation and execution pipeline
- Comprehensive logging and error handling
- Health check support

## Key Components

### ServiceBase<TConfiguration, TCommand>

The primary base class for implementing services:

```csharp
public abstract class ServiceBase<TConfiguration, TCommand> : IFdwService<TConfiguration, TCommand>
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
    where TCommand : ICommand
{
    protected ServiceBase(ILogger logger, IConfigurationRegistry<TConfiguration> configurations);
    
    // Override this to implement your service logic
    protected abstract Task<FdwResult<T>> ExecuteCore<T>(TCommand command);
}
```

#### Features
- **Automatic Configuration Validation**: Validates configurations on startup
- **Command Validation**: Validates commands before execution
- **Correlation ID Tracking**: Automatic correlation ID for request tracking
- **Performance Logging**: Logs execution duration
- **Error Handling**: Consistent error handling and logging
- **Health Checks**: Built-in health status based on configuration validity

### DataConnection<TDataCommand, TConnection>

Universal data service implementation that works with any data source:

```csharp
public class DataConnection<TDataCommand, TConnection> : ServiceBase<DataConnectionConfiguration, TDataCommand>
    where TDataCommand : IFdwDataCommand
    where TConnection : IExternalConnection
{
    private readonly IExternalConnectionProvider _connectionProvider;
    
    protected override async Task<FdwResult<T>> ExecuteCore<T>(TDataCommand command)
    {
        // Get the appropriate external connection based on command
        var connection = await _connectionProvider.GetConnection<TConnection>(command.ConnectionId);
        
        // External connection's command builder transforms universal command to provider-specific
        return await connection.Execute<T>(command);
    }
}
```

#### Key Features
- **Provider Agnostic**: Works with any data source (SQL, NoSQL, APIs, Files)
- **Universal Commands**: Uses LINQ-like expressions that get transformed by providers
- **Command Transformation**: Each provider's CommandBuilder handles the transformation
- **Single Service**: One data service handles all data operations across all providers

## Usage Examples

### Creating a Service

```csharp
public class CustomerService : ServiceBase<CustomerConfiguration, CustomerCommand>
{
    private readonly ICustomerRepository _repository;
    
    public CustomerService(
        ILogger<CustomerService> logger, 
        IConfigurationRegistry<CustomerConfiguration> configurations,
        ICustomerRepository repository)
        : base(logger, configurations)
    {
        _repository = repository;
    }
    
    protected override async Task<FdwResult<T>> ExecuteCore<T>(CustomerCommand command)
    {
        // Your service implementation here
        // Validation has already been performed
        // Configuration is available via this.Configuration
        
        return command switch
        {
            GetCustomerCommand getCmd => await GetCustomer<T>(getCmd),
            CreateCustomerCommand createCmd => await CreateCustomer<T>(createCmd),
            _ => FdwResult<T>.Failure(ServiceMessages.InvalidCommand.Format(command.GetType().Name))
        };
    }
    
    private async Task<FdwResult<T>> GetCustomer<T>(GetCustomerCommand command)
    {
        var customer = await _repository.GetAsync(command.CustomerId);
        if (customer == null)
        {
            return FdwResult<T>.Failure(
                ServiceMessages.RecordNotFound.Format("Customer", command.CustomerId));
        }
        
        return FdwResult<T>.Success((T)(object)customer);
    }
}
```

### Using the Universal Data Service

```csharp
// Configure data service with multiple providers
services.AddSingleton<IExternalConnectionProvider, ExternalConnectionProvider>();
services.AddScoped<DataConnection<IFdwDataCommand, IExternalConnection>>();

// Query example - works with any data source
var queryCommand = new FdwDataCommand
{
    Operation = DataOperation.Query,
    EntityType = "Customer",
    QueryExpression = customers => customers.Where(c => c.City == "Seattle"),
    ConnectionId = "sql-primary" // Could be "mongo-primary", "api-customers", etc.
};

var result = await dataConnection.Execute<IEnumerable<Customer>>(queryCommand);
```

### Implementing a Configuration Registry

```csharp
public class InMemoryConfigurationRegistry<T> : IConfigurationRegistry<T>
    where T : IFdwConfiguration
{
    private readonly Dictionary<int, T> _configurations = new();
    
    public InMemoryConfigurationRegistry(IEnumerable<T> configurations)
    {
        foreach (var config in configurations)
        {
            _configurations[config.Id] = config;
        }
    }
    
    public T? Get(int id) => _configurations.TryGetValue(id, out var config) ? config : null;
    
    public IEnumerable<T> GetAll() => _configurations.Values;
    
    public bool TryGet(int id, out T? configuration) => _configurations.TryGetValue(id, out configuration);
}
```

### Using the Service

```csharp
// In your DI container setup
services.AddSingleton<IConfigurationRegistry<CustomerConfiguration>>(sp =>
{
    var configs = new[]
    {
        new CustomerConfiguration { Id = 1, Name = "Default", IsEnabled = true, ConnectionString = "..." }
    };
    return new InMemoryConfigurationRegistry<CustomerConfiguration>(configs);
});

services.AddScoped<CustomerService>();

// Using the service
var command = new GetCustomerCommand { CustomerId = 123 };
var result = await customerService.Execute<Customer>(command);

if (result.IsSuccess)
{
    return Ok(result.Value);
}
else
{
    return BadRequest(result.Error);
}
```

## Service Lifecycle

1. **Construction**: Service validates it has at least one valid configuration
2. **Command Receipt**: Service receives a command to execute
3. **Command Validation**: Automatic validation of command and its configuration
4. **Execution**: Your ExecuteCore implementation is called
5. **Result**: Consistent result pattern with success/failure
6. **Logging**: Automatic logging of execution time and results

## Built-in Messages

The service automatically logs using the ServiceMessages system:
- `ServiceStarted` - When service initializes successfully
- `InvalidConfiguration` - When configuration validation fails
- `InvalidCommand` - When command validation fails
- `CommandExecuted` - When command executes successfully
- `CommandFailed` - When command execution fails
- `OperationFailed` - When an exception occurs

## Health Checks

Services implement `IsHealthy` which returns true when:
- At least one configuration is available
- The primary configuration is valid
- The primary configuration is enabled

## Error Handling

The service base class provides comprehensive error handling:
- Validation errors return failure results with descriptive messages
- Exceptions are caught and logged (except OutOfMemoryException)
- All errors use the consistent FdwResult pattern
- Correlation IDs track requests through the system

## Installation

```xml
<PackageReference Include="FractalDataWorks.Services" Version="*" />
```

## Dependencies

- FractalDataWorks.net (core abstractions)
- FractalDataWorks.Configuration
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options
- Microsoft.Extensions.DependencyInjection.Abstractions
- FluentValidation

## Best Practices

1. **Keep ExecuteCore Focused**: Implement only business logic, let the base class handle infrastructure
2. **Use Configuration Registry**: Manage multiple configurations for different environments/tenants
3. **Leverage Built-in Validation**: Don't duplicate validation that the base class provides
4. **Return Appropriate Results**: Use FdwResult for consistent error handling
5. **Log Sparingly**: The base class provides comprehensive logging already
6. **Use DataConnection for Data**: Don't create domain-specific data services, use the universal DataConnection
7. **Provider Agnostic Commands**: Write queries using universal expressions that work across providers

## Advanced Scenarios

### Custom Validation
Override `ValidateCommand` for additional validation:
```csharp
protected override async Task<FdwResult<TCommand>> ValidateCommand(ICommand command)
{
    var result = await base.ValidateCommand(command);
    if (result.IsFailure) return result;
    
    // Add custom validation
    if (customValidationFails)
    {
        return FdwResult<TCommand>.Failure("Custom validation error");
    }
    
    return result;
}
```

### Custom Health Checks
Override `IsHealthy` for custom health logic:
```csharp
public override bool IsHealthy => base.IsHealthy && _repository.IsConnected;
```