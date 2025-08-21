# FractalDataWorks.Messages

This package provides the foundation for structured, type-safe messaging within the FractalDataWorks framework using the Enhanced Enums pattern.

## Overview

The Messages package enables consistent, discoverable, and type-safe messaging across all FractalDataWorks components. Messages are used for structured result information in `FdwResult<T>` operations, not for logging.

## Key Components

### IFdwMessage Interface

The core interface that all framework messages implement:

```csharp
public interface IFdwMessage
{
    MessageSeverity Severity { get; }    // Information, Warning, Error, Critical
    string Message { get; }              // Human-readable message text
    string? Code { get; }                // Unique identifier for programmatic handling
    string? Source { get; }              // Component that generated the message
}
```

### MessageSeverity Enum

Defines the severity levels for framework messages:

```csharp
public enum MessageSeverity
{
    Information = 0,    // Context or status updates
    Warning = 1,        // Potential issues that don't prevent operation
    Error = 2,          // Failures or critical problems
    Critical = 3        // System-level failures
}
```

### MessageBase Abstract Class

The Enhanced Enum base class for all framework messages with formatting and metadata capabilities:

```csharp
[EnumCollection(ReturnType = typeof(IFdwMessage))]
public abstract class MessageBase : EnumOptionBase<MessageBase>, IFdwMessage
{
    public MessageSeverity Severity { get; }
    public string Message { get; }
    public string? Code { get; }
    public string? Source { get; }
    public DateTime Timestamp { get; }              // UTC timestamp of creation
    public IDictionary<string, object?>? Details { get; }  // Additional metadata
    public object? Data { get; }                    // Associated data object

    protected MessageBase(int id, string name, MessageSeverity severity, string message, 
                         string? code = null, string? source = null)
        : base(id, name) { /* ... */ }

    protected MessageBase(int id, string name, MessageSeverity severity, string message, 
                         string? code = null, string? source = null, 
                         IDictionary<string, object?>? details = null, object? data = null)
        : base(id, name) { /* ... */ }

    // Format message with parameters
    public virtual string Format(params object[] args) { /* ... */ }
    
    // Create new instance with different severity
    public abstract MessageBase WithSeverity(MessageSeverity severity);
}
```

### MessageAttribute

Marks concrete message classes for Enhanced Enum generation:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MessageAttribute : Attribute
{
    public string? CollectionName { get; set; }
    public Type? ReturnType { get; set; }
    public string? ReturnTypeNamespace { get; set; }
    public bool IncludeInGlobalCollection { get; set; } = true;
}
```

## Usage Patterns

### 1. Component-Specific Message Base Classes

Create abstract message base classes for each major component:

```csharp
// For Services component
[EnumCollection("ServiceMessages", ReturnType = typeof(IFdwMessage))]
public abstract class ServiceMessageBase : MessageBase
{
    protected ServiceMessageBase(int id, string name, MessageSeverity severity, 
                                string message, string? code = null)
        : base(id, name, severity, message, code, "Services") { }
}

// For HTTP Connection component
[EnumCollection("HttpMessages", ReturnType = typeof(IFdwMessage))]
public abstract class HttpMessageBase : MessageBase
{
    protected HttpMessageBase(int id, string name, MessageSeverity severity, 
                             string message, string? code = null)
        : base(id, name, severity, message, code, "HttpConnection") { }
}
```

### 2. Concrete Message Implementations

Create specific message classes with built-in formatting capabilities:

```csharp
[Message]
public sealed class InvalidCommand : ServiceMessageBase
{
    public InvalidCommand() 
        : base(1001, nameof(InvalidCommand), MessageSeverity.Error, 
               "Invalid command type: {0}", "INVALID_COMMAND") { }

    public override ServiceMessageBase WithSeverity(MessageSeverity severity)
        => new InvalidCommand(); // Can create new instance with different severity if needed
}

[Message]
public sealed class RecordNotFound : ServiceMessageBase
{
    public RecordNotFound() 
        : base(1002, nameof(RecordNotFound), MessageSeverity.Warning, 
               "{0} with ID {1} was not found", "RECORD_NOT_FOUND") { }

    public override ServiceMessageBase WithSeverity(MessageSeverity severity)
        => new RecordNotFound(); // Can create new instance with different severity if needed
}

[Message]
public sealed class ConnectionTimeout : HttpMessageBase
{
    public ConnectionTimeout() 
        : base(2001, nameof(ConnectionTimeout), MessageSeverity.Error, 
               "Connection to {0} timed out after {1}ms", "HTTP_TIMEOUT") { }

    public override HttpMessageBase WithSeverity(MessageSeverity severity)
        => new ConnectionTimeout(); // Can create new instance with different severity if needed
}
```

### 3. Using Messages in FdwResult Operations

The new Format() method provides built-in string formatting:

```csharp
public async Task<IFdwResult<Customer>> GetCustomer(GetCustomerCommand command)
{
    if (command == null)
    {
        // Use the built-in Format method with params
        var errorMessage = ServiceMessages.InvalidCommand.Format(nameof(GetCustomerCommand));
        return FdwResult<Customer>.Failure(errorMessage);
    }

    var customer = await _repository.GetAsync(command.CustomerId);
    if (customer == null)
    {
        // Format with multiple parameters
        var notFoundMessage = ServiceMessages.RecordNotFound.Format("Customer", command.CustomerId);
        return FdwResult<Customer>.Failure(notFoundMessage);
    }

    return FdwResult<Customer>.Success(customer);
}

// Additional examples of enhanced formatting:
public string HandleTimeout(string endpoint, int timeoutMs)
{
    // Simple parameter formatting
    return HttpMessages.ConnectionTimeout.Format(endpoint, timeoutMs);
    
    // With severity adjustment
    var criticalTimeout = HttpMessages.ConnectionTimeout.WithSeverity(MessageSeverity.Critical);
    return criticalTimeout.Format(endpoint, timeoutMs);
}
```

### 4. Enhanced Enum Generated Collections

The Enhanced Enum system automatically generates static collections:

```csharp
// Generated ServiceMessages collection
public static class ServiceMessages
{
    public static readonly InvalidCommand InvalidCommand = new();
    public static readonly RecordNotFound RecordNotFound = new();
    
    public static readonly List<ServiceMessageBase> All = new()
    {
        InvalidCommand,
        RecordNotFound
    };

    public static ServiceMessageBase? GetByName(string name) 
        => All.FirstOrDefault(m => m.Name == name);
        
    public static ServiceMessageBase? GetById(int id) 
        => All.FirstOrDefault(m => m.Id == id);
}

// Generated HttpMessages collection
public static class HttpMessages
{
    public static readonly ConnectionTimeout ConnectionTimeout = new();
    // ... other HTTP messages
}
```

## Message ID Ranges

To avoid conflicts, use these ID ranges for different components:

- **1000-1999**: Core Services messages
- **2000-2999**: HTTP Connection messages  
- **3000-3999**: Secret Management messages
- **4000-4999**: Authentication Service messages
- **5000-5999**: Data Provider messages
- **6000-6999**: Configuration messages
- **7000-7999**: External Connections messages
- **8000-8999**: Tools messages
- **9000-9999**: Host/Application messages

## Best Practices

### Message Design
- **Clear, actionable messages**: Write messages that help users understand what happened and what to do
- **Consistent formatting**: Use standard .NET format strings (`{0}`, `{1}`, etc.) for the built-in Format() method
- **Appropriate severity**: Choose severity levels that match the actual impact
- **Unique codes**: Use meaningful, unique codes for programmatic handling
- **Use Format() method**: Leverage the built-in Format(params object[] args) instead of custom formatting logic
- **Immutable message templates**: Keep base message text as templates, use Format() for runtime values

### Enhanced Enum Integration
- **Component-specific base classes**: Create abstract base classes for each major component
- **Meaningful collection names**: Use descriptive names like `ServiceMessages`, `HttpMessages`
- **Consistent naming**: Follow `[Component]Messages` pattern for collection names
- **Proper ID ranges**: Use designated ID ranges to avoid conflicts

### Usage Guidelines
- **For results, not logging**: Messages are for `FdwResult<T>` operations, not logging
- **Use built-in Format()**: Leverage the inherited Format(params object[] args) method for all parameterization
- **Immutable design**: Messages should be immutable once created (WithSeverity creates new instances)
- **Thread-safe**: All message operations should be thread-safe
- **Metadata support**: Use Details dictionary and Data property for structured additional information
- **Timestamp awareness**: All messages include automatic UTC timestamp creation for audit trails

## Dependencies

- **FractalDataWorks.EnhancedEnums**: Enhanced Enum pattern and attributes
- **System.ComponentModel.Annotations**: Attribute support

## Integration

Messages integrate seamlessly with other FractalDataWorks components:

- **Results**: Used in `FdwResult<T>` for structured error/success information
- **Services**: Service operations return messages for validation failures, errors
- **Validation**: Configuration and command validation returns formatted messages
- **Enhanced Enums**: Automatic collection generation and discovery
- **Dependency Injection**: Messages can be injected and used throughout the application

## Examples

### Basic Service Integration

```csharp
// 1. Define service-specific messages
[Message]
public sealed class CustomerNotFound : ServiceMessageBase
{
    public CustomerNotFound() : base(1003, nameof(CustomerNotFound), 
        MessageSeverity.Warning, "Customer with ID {0} was not found", "CUSTOMER_NOT_FOUND") { }
    
    public override ServiceMessageBase WithSeverity(MessageSeverity severity)
        => new CustomerNotFound(); // Could enhance to preserve severity if needed
}

// 2. Use in service operations with built-in formatting
public async Task<IFdwResult<Customer>> GetCustomer(int customerId)
{
    var customer = await _repository.GetByIdAsync(customerId);
    
    return customer != null 
        ? FdwResult<Customer>.Success(customer)
        : FdwResult<Customer>.Failure(ServiceMessages.CustomerNotFound.Format(customerId));
}

// 3. Handle results with structured message information
var result = await customerService.GetCustomer(123);
if (!result.IsSuccess)
{
    var message = result.Messages.First();
    _logger.LogWarning("Operation failed at {Timestamp}: {Code} - {Message}", 
        message.Timestamp, message.Code, message.Message);
}
```

### Advanced Usage with Metadata

```csharp
// Message with additional metadata support
[Message]
public sealed class ValidationFailure : ServiceMessageBase
{
    public ValidationFailure() : base(1004, nameof(ValidationFailure), 
        MessageSeverity.Error, "Validation failed for {0}: {1}", "VALIDATION_ERROR") { }
    
    public ValidationFailure(IDictionary<string, object?> validationDetails, object failedData)
        : base(1004, nameof(ValidationFailure), MessageSeverity.Error, 
               "Validation failed for {0}: {1}", "VALIDATION_ERROR", "Services", 
               validationDetails, failedData) { }
               
    public override ServiceMessageBase WithSeverity(MessageSeverity severity)
        => new ValidationFailure(Details ?? new Dictionary<string, object?>(), Data);
}

// Usage with metadata
public IFdwResult<Customer> ValidateCustomer(Customer customer)
{
    var validationErrors = new Dictionary<string, object?>
    {
        { "Field", "Email" },
        { "Value", customer.Email },
        { "Rule", "ValidEmailFormat" }
    };
    
    if (string.IsNullOrEmpty(customer.Email) || !IsValidEmail(customer.Email))
    {
        var failure = new ValidationFailure(validationErrors, customer);
        return FdwResult<Customer>.Failure(failure.Format("Customer", "Invalid email format"));
    }
    
    return FdwResult<Customer>.Success(customer);
}
```

This approach provides consistent, discoverable, and maintainable messaging across the entire FractalDataWorks ecosystem.