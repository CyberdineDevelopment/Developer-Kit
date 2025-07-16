# FractalDataWorks.net

Core abstractions and base types for the FractalDataWorks framework. This package provides the foundational interfaces and types that all other FractalDataWorks packages depend on.

## Overview

FractalDataWorks.net is the Layer 0.5 foundation package that:
- Targets .NET Standard 2.0 for maximum compatibility
- Has zero external dependencies (except for enhanced enums and System.Collections.Immutable)
- Provides core abstractions used throughout the framework
- Defines the messaging system using EnhancedEnums

## Key Components

### Service Abstractions

#### IFractalService
Base interface for all services in the framework:
```csharp
public interface IFractalService
{
    string ServiceName { get; }
    bool IsHealthy { get; }
}

public interface IFractalService<TConfiguration> : IFractalService
    where TConfiguration : IFractalConfiguration
{
    TConfiguration Configuration { get; }
}

public interface IFractalService<TConfiguration, TCommand> : IFractalService<TConfiguration>
    where TConfiguration : IFractalConfiguration
    where TCommand : ICommand
{
    Task<FractalResult<T>> Execute<T>(TCommand command);
}
```

### Configuration Abstractions

#### IFractalConfiguration
Base interface for all configuration types:
```csharp
public interface IFractalConfiguration
{
    int Id { get; set; }
    string Name { get; set; }
    string Version { get; set; }
    bool IsEnabled { get; set; }
    bool IsValid { get; }
    bool IsDefault { get; set; }
}
```

### Result Pattern

#### IServiceResult & FractalResult<T>
Consistent result pattern for service operations:
```csharp
// Non-generic result
public interface IServiceResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    string? Error { get; }
}

// Generic result with value
public class FractalResult<T> : IServiceResult
{
    public T? Value { get; }
    
    public static FractalResult<T> Success(T value);
    public static FractalResult<T> Failure(string error);
}
```

### Validation

#### IFractalValidator<T>
Validation abstraction for consistent validation across the framework:
```csharp
public interface IFractalValidator<T>
{
    Task<IValidationResult> Validate(T instance);
    Task<FractalResult<T>> ValidateToResult(T instance);
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
    IFractalConfiguration? Configuration { get; }
    Task<IValidationResult> Validate();
}
```

## Usage Examples

### Creating a Service
```csharp
public interface ICustomerService : IFractalService<CustomerConfiguration, CustomerCommand>
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
public async Task<FractalResult<Customer>> GetCustomerAsync(int id)
{
    if (id <= 0)
    {
        return FractalResult<Customer>.Failure(
            ServiceMessages.InvalidId.Format(id));
    }
    
    var customer = await _repository.GetAsync(id);
    if (customer == null)
    {
        return FractalResult<Customer>.Failure(
            ServiceMessages.RecordNotFound.Format("Customer", id));
    }
    
    return FractalResult<Customer>.Success(customer);
}
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