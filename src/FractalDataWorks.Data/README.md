# FractalDataWorks.Data

Data access abstractions and common data types for the FractalDataWorks framework.

## Overview

FractalDataWorks.Data provides:
- Base entity classes with audit fields and soft delete support
- Optimistic concurrency control via version fields
- Entity validation logic
- Data operation abstractions
- Interfaces for data connections, transactions, and commands

## Current Implementation

### EntityBase<TKey>

A fully implemented abstract base class that provides:
- **Audit fields** - CreatedAt, CreatedBy, ModifiedAt, ModifiedBy with automatic tracking
- **Soft delete functionality** - IsDeleted, DeletedAt, DeletedBy fields with restore capability
- **Optimistic concurrency** - Version field for conflict detection
- **Entity validation** - Built-in validation for temporal consistency
- **Equality comparison** - Proper equality based on entity ID and type
- **Transient detection** - Identifies unpersisted entities

```csharp
public abstract class EntityBase<TKey> where TKey : IEquatable<TKey>
{
    // Identity
    public virtual TKey Id { get; set; }
    
    // Audit fields
    public virtual DateTime CreatedAt { get; set; }
    public virtual string? CreatedBy { get; set; }
    public virtual DateTime? ModifiedAt { get; set; }
    public virtual string? ModifiedBy { get; set; }
    
    // Concurrency control
    public virtual byte[]? Version { get; set; }
    
    // Soft delete
    public virtual bool IsDeleted { get; set; }
    public virtual DateTime? DeletedAt { get; set; }
    public virtual string? DeletedBy { get; set; }
    
    // Helper methods
    public virtual void MarkAsCreated(string userId);
    public virtual void MarkAsModified(string userId);
    public virtual void MarkAsDeleted(string userId);
    public virtual void Restore(string userId);
    public virtual bool IsTransient();
    public virtual bool IsValid();
    public virtual IEnumerable<string> Validate();
}
```

### Specialized Entity Base Classes

**EntityBase** - For integer primary keys:
```csharp
public abstract class EntityBase : EntityBase<int>
{
}
```

**GuidEntityBase** - For GUID primary keys with auto-generation:
```csharp
public abstract class GuidEntityBase : EntityBase<Guid>
{
    protected GuidEntityBase()
    {
        Id = Guid.NewGuid();
    }
}
```

### Data Operation Interfaces

**IDataCommand** - Command pattern for data operations:
```csharp
public interface IDataCommand : ICommand
{
    DataOperation Operation { get; }
}
```

**IDataConnection** - Data connection abstraction:
```csharp
public interface IDataConnection : IFdwConnection
{
    Task<FdwResult<T>> ExecuteCommandAsync<T>(IDataCommand command, CancellationToken cancellationToken = default);
    Task<FdwResult<IDataTransaction>> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
```

**IDataTransaction** - Transaction management:
```csharp
public interface IDataTransaction : IDisposable, IAsyncDisposable
{
    Guid TransactionId { get; }
    bool IsActive { get; }
    Task<FdwResult> CommitAsync(CancellationToken cancellationToken = default);
    Task<FdwResult> RollbackAsync(CancellationToken cancellationToken = default);
}
```

### Data Messages

The package includes predefined messages for common data scenarios:
- **RecordNotFound** - When requested data cannot be found
- **DuplicateKey** - When a unique constraint is violated
- **DatabaseConnectionLost** - When database connection is lost
- **DataMessageBase** - Base class for data-related messages

## Usage Examples

### Creating an Entity

```csharp
public class Customer : GuidEntityBase
{
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    
    public override IEnumerable<string> Validate()
    {
        // Call base validation
        foreach (var error in base.Validate())
            yield return error;
        
        // Add custom validation
        if (string.IsNullOrWhiteSpace(Name))
            yield return "Customer name is required";
            
        if (string.IsNullOrWhiteSpace(Email))
            yield return "Customer email is required";
            
        if (!Email.Contains("@"))
            yield return "Invalid email format";
    }
}
```

### Using Entities

```csharp
// Create a new customer
var customer = new Customer
{
    Name = "John Doe",
    Email = "john@example.com",
    IsActive = true
};

// Mark as created by a user
customer.MarkAsCreated("user123");

// Validate before saving
if (!customer.IsValid())
{
    var errors = customer.Validate();
    // Handle validation errors
}

// Update the customer
customer.Name = "John Smith";
customer.MarkAsModified("user456");

// Soft delete
customer.MarkAsDeleted("admin");

// Restore deleted customer
customer.Restore("admin");
```

### Working with Different Key Types

```csharp
// Integer key entity
public class Order : EntityBase  // inherits EntityBase<int>
{
    public string OrderNumber { get; set; }
    public decimal Total { get; set; }
}

// GUID key entity with auto-generation
public class Product : GuidEntityBase
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Custom key type
public class Document : EntityBase<string>
{
    public string Title { get; set; }
    public byte[] Content { get; set; }
}
```

## Planned Features

### Repository Pattern
- Generic repository interface
- Specification pattern for complex queries
- Async operations throughout

### Unit of Work
- Transaction coordination across repositories
- Change tracking
- Batch operations

### Query Specifications
- Fluent API for building queries
- Include/eager loading support
- Paging and sorting

### Advanced Entity Features
- Domain events
- Value objects
- Aggregate root markers

## Installation

```xml
<PackageReference Include="FractalDataWorks.Data" Version="*" />
```

## Dependencies

- FractalDataWorks.net (core abstractions)
- FractalDataWorks.Connections (connection interfaces)

## Contributing

This package welcomes contributions for:
- Repository pattern implementations
- Query specification system
- Additional entity base classes
- Data access utilities
- Unit and integration tests