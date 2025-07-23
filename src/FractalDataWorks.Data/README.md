# FractalDataWorks.Data

Entity base classes and data patterns for the FractalDataWorks framework. This package provides domain entity foundations, NOT data access implementations.

## Overview

FractalDataWorks.Data provides:
- Base entity classes with audit fields and soft delete support
- Optimistic concurrency control via version fields
- Entity validation logic
- Domain-driven design patterns
- NO data access implementations (those belong in Services/Connections)

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

### Domain Patterns

This package focuses on domain entity patterns following Domain-Driven Design principles. Data access is handled by the universal DataConnection service in FractalDataWorks.Services.

**Key Concepts:**
- Entities are domain objects with identity
- Base classes provide common functionality
- Validation is part of the domain model
- Data access is delegated to services

### Integration with Data Services

Entities defined using these base classes work seamlessly with the universal data service:

```csharp
// Define your entity
public class Customer : GuidEntityBase
{
    public string Name { get; set; }
    public string Email { get; set; }
}

// Use with DataConnection service
var query = new FdwDataCommand
{
    Operation = DataOperation.Query,
    EntityType = nameof(Customer),
    QueryExpression = customers => customers.Where(c => !c.IsDeleted && c.Name.StartsWith("John")),
    ConnectionId = "primary"
};

var result = await dataConnection.Execute<IEnumerable<Customer>>(query);
```

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

## Design Philosophy

This package intentionally does NOT include:
- Repository interfaces (use DataConnection service)
- Data access abstractions (in FractalDataWorks.net)
- Query builders (handled by external connections)
- Database-specific code (in provider implementations)

Instead, it focuses on:
- Clean domain entity design
- Consistent audit and versioning patterns
- Entity validation as part of the domain
- Base classes that work with any data provider

## Advanced Entity Patterns

### Value Objects
```csharp
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
    }
}
```

### Aggregate Roots
```csharp
public abstract class AggregateRoot<TKey> : EntityBase<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.Data" Version="*" />
```

## Dependencies

- FractalDataWorks.net (core abstractions only)

## Contributing

This package welcomes contributions for:
- Additional entity base classes
- Value object patterns
- Domain event implementations
- Aggregate root patterns
- Entity validation strategies
- Unit tests for domain logic

Remember: This package is for domain entities only. Data access belongs in the Services/Connections layer.