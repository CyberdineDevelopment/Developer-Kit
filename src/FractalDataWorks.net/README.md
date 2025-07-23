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

## Contributing

This is a core package - changes here affect all dependent packages. Please ensure:
- All interfaces are well-documented with XML comments
- Breaking changes are avoided when possible
- New abstractions are discussed and approved before implementation
- Unit tests maintain high coverage