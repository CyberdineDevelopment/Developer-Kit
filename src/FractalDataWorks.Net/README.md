# FractalDataWorks Core

The foundational package of the FractalDataWorks SDK, providing core interfaces, abstractions, and the Result<T> pattern. This package has zero dependencies and targets .NET Standard 2.0 for maximum compatibility.

## 📦 Package Information

- **Package ID**: `FractalDataWorks`
- **Target Framework**: .NET Standard 2.0
- **Dependencies**: None
- **License**: Apache 2.0

## 🎯 Purpose

This package defines the core contracts and patterns that all other FractalDataWorks packages implement. It provides:

- **Result<T> Pattern**: Type-safe error handling without exceptions
- **Service Interfaces**: Base contracts for the service hierarchy
- **Enhanced Enum Interface**: Foundation for automatic service discovery
- **Smart Delegate Interface**: Base for pipeline processing
- **Data Operation Interfaces**: Universal data access contracts

## 🏗️ Key Components

### Result<T> Pattern

```csharp
// Success case
var result = Result<Customer>.Ok(customer, "Customer created successfully");

// Failure case  
var result = Result<Customer>.Fail("Customer not found");

// Pattern matching
var response = result.Match(
    success: (customer, message) => Ok(customer),
    failure: error => BadRequest(error.Message)
);
```

### Service Hierarchy

```csharp
// Most generic - any service can serve any request
public interface IGenericService
{
    Result<T> Serve<T>(ConfigurationBase configuration);
}

// More specific - services process things
public interface IService : IGenericService  
{
    Result<T> Process<T>(ServiceConfiguration configuration);
}

// Connections execute operations
public interface IConnection : IService
{
    Task<Result<T>> Execute<T>(IDataOperation operation, CancellationToken cancellationToken);
}
```

### Universal Data Operations

```csharp
// Works with any data source
public interface IDataConnection : IConnection
{
    Task<Result<IEnumerable<T>>> Query<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class;
    Task<Result<T>> Insert<T>(T entity, CancellationToken cancellationToken) where T : class;
    Task<Result<int>> Update<T>(Expression<Func<T, bool>> where, Expression<Func<T, T>> update, CancellationToken cancellationToken) where T : class;
    Task<Result<int>> Delete<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class;
}
```

### Enhanced Enum Foundation

```csharp
public interface IEnhancedEnum
{
    string Name { get; }
    int Order { get; }
}

public interface IEnhancedEnum<T> : IEnhancedEnum where T : IEnhancedEnum<T>
{
    T Empty();
}
```

## 🚀 Usage

### Install Package

```bash
dotnet add package FractalDataWorks
```

### Basic Usage

```csharp
using FractalDataWorks;

// Use Result<T> for robust error handling
public async Task<Result<User>> GetUser(int id)
{
    if (id <= 0)
        return Result<User>.Fail("Invalid user ID");
        
    var user = await _repository.GetByIdAsync(id);
    
    return user != null 
        ? Result<User>.Ok(user, "User found")
        : Result<User>.Fail("User not found");
}

// Pattern matching for control flow
var result = await GetUser(123);
var response = result.Match(
    success: (user, msg) => new { success = true, data = user, message = msg },
    failure: error => new { success = false, error = error.Message }
);
```

### Configuration Base

```csharp
public class MyServiceConfig : ConfigurationBase<MyServiceConfig>
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
    
    public override bool IsValid => 
        !string.IsNullOrWhiteSpace(ConnectionString) && 
        Timeout > 0;
        
    public override List<string> ValidationErrors
    {
        get
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(ConnectionString))
                errors.Add("ConnectionString is required");
            if (Timeout <= 0)
                errors.Add("Timeout must be greater than zero");
            return errors;
        }
    }
    
    public override MyServiceConfig CreateDefault() => new();
}
```

## 🎨 Design Principles

1. **Zero Dependencies**: This package references no external libraries
2. **Maximum Compatibility**: Targets .NET Standard 2.0
3. **Immutable by Default**: Use record types and readonly properties where possible
4. **Async-First**: All operations return Task<Result<T>>
5. **Type Safety**: Strong typing prevents runtime errors
6. **Fail-Safe**: Invalid configurations use Null Object pattern

## 🔗 Integration

This package is designed to be consumed by:

- **Application Code**: Use Result<T>, service interfaces
- **Implementation Packages**: Implement the service interfaces
- **Provider Packages**: Implement data operation interfaces
- **Framework Code**: Build on the core abstractions

## 📋 Interface Reference

### Core Interfaces

- `IGenericService<TConfiguration>` - Base service contract
- `IService<TConfiguration>` - Service processing contract  
- `IConnection<TConfiguration>` - Connection execution contract
- `IDataConnection<TConfiguration>` - Data operation contract

### Data Interfaces

- `IDataOperation` - Base data operation
- `IDataQuery<T>` - Query operations
- `IDataInsert<T>` - Insert operations
- `IDataUpdate<T>` - Update operations
- `IDataDelete<T>` - Delete operations
- `IOperationParser` - Parse operations for providers
- `IConnectionAdapter` - Execute parsed operations

### Enhanced Enum Interfaces

- `IEnhancedEnum` - Base enhanced enum
- `IEnhancedEnum<T>` - Typed enhanced enum
- `ISmartDelegate<TContext>` - Pipeline processing

## 🧪 Testing

```csharp
[Test]
public void Result_Success_ShouldReturnValue()
{
    var result = Result<string>.Ok("test value");
    
    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldBe("test value");
}

[Test]
public void Result_Failure_ShouldReturnError()
{
    var result = Result<string>.Fail("Something went wrong");
    
    result.IsFailure.ShouldBeTrue();
    result.Error.Message.ShouldBe("Something went wrong");
}
```

## 🔄 Version History

- **0.1.0-preview**: Initial release with core interfaces
- **Future**: Enhanced Enum source generators, Smart Delegate pipeline processing

## 🤝 Contributing

See the main [repository README](../../README.md) for contribution guidelines.

## 📄 License

Licensed under the Apache License 2.0. See [LICENSE](../../LICENSE) for details.