# FractalDataWorks.Data

Entity base classes and data modeling fundamentals for the FractalDataWorks ecosystem. Provides rich entity lifecycle management, change tracking, and domain modeling patterns.

## 📦 Package Information

- **Package ID**: `FractalDataWorks.Data`
- **Target Framework**: .NET Standard 2.0
- **Dependencies**: 
  - `FractalDataWorks` (core)
- **License**: Apache 2.0

## 🎯 Purpose

This package provides the foundation for data modeling in FractalDataWorks applications:

- **EntityBase Classes**: Rich base classes for domain entities
- **Entity Lifecycle**: Creation, modification, soft deletion tracking
- **Identity Management**: Flexible primary key support
- **Change Tracking**: Built-in change detection and auditing
- **Metadata Support**: Extensible entity metadata
- **Domain Modeling**: Value objects, aggregates, domain events

## 🚀 Usage

### Install Package

```bash
dotnet add package FractalDataWorks.Data
```

### Basic Entity

```csharp
using FractalDataWorks.Data;

public class Customer : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Business methods
    public void Deactivate()
    {
        IsActive = false;
        MarkAsModified();
    }
    
    public void UpdateContactInfo(string email, string region)
    {
        Email = email;
        Region = region;
        MarkAsModified();
    }
}
```

### Custom Key Types

```csharp
// Integer primary key
public class Product : EntityBase<int>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}

// GUID primary key
public class Order : EntityBase<Guid>
{
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    
    public Order()
    {
        Id = Guid.NewGuid(); // Auto-generate GUID
    }
}

// Composite key
public class OrderItem : EntityBase<(Guid OrderId, int ProductId)>
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}
```

## 🏗️ Key Features

### Entity Lifecycle Management

```csharp
var customer = new Customer
{
    Name = "John Doe",
    Email = "john@example.com"
};

// Automatic ID generation for string-based entities
customer.GenerateId(); // Creates GUID-based string ID

// Creation timestamp is automatically set
Console.WriteLine(customer.CreatedAt); // Current UTC time

// Modification tracking
customer.Email = "john.doe@example.com";
customer.MarkAsModified();
Console.WriteLine(customer.ModifiedAt); // Updated timestamp

// Soft deletion
customer.MarkAsDeleted();
Console.WriteLine(customer.IsDeleted); // true
Console.WriteLine(customer.DeletedAt); // Deletion timestamp

// Restoration
customer.Restore();
Console.WriteLine(customer.IsDeleted); // false
```

### Entity Equality and Comparison

```csharp
var customer1 = new Customer { Id = "123", Name = "John" };
var customer2 = new Customer { Id = "123", Name = "John Doe" };
var customer3 = new Customer { Id = "456", Name = "Jane" };

// Equality based on ID and type
Console.WriteLine(customer1.Equals(customer2)); // true (same ID)
Console.WriteLine(customer1.Equals(customer3)); // false (different ID)

// Hash code consistency
var set = new HashSet<Customer> { customer1, customer2, customer3 };
Console.WriteLine(set.Count); // 2 (customer1 and customer2 are considered equal)
```

### Metadata and Extensibility

```csharp
public class Customer : EntityBase
{
    public string Name { get; set; } = string.Empty;
    
    // Use metadata for flexible attributes
    public string? PreferredContactMethod 
    { 
        get => Metadata.TryGetValue("PreferredContactMethod", out var value) ? value?.ToString() : null;
        set => Metadata["PreferredContactMethod"] = value ?? string.Empty;
    }
    
    public int? LoyaltyPoints
    {
        get => Metadata.TryGetValue("LoyaltyPoints", out var value) && int.TryParse(value?.ToString(), out var points) ? points : null;
        set => Metadata["LoyaltyPoints"] = value ?? 0;
    }
}

// Usage
var customer = new Customer { Name = "John" };
customer.PreferredContactMethod = "Email";
customer.LoyaltyPoints = 1500;
```

### Value Objects

```csharp
public record Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    
    public bool IsValid => 
        !string.IsNullOrWhiteSpace(Street) &&
        !string.IsNullOrWhiteSpace(City) &&
        !string.IsNullOrWhiteSpace(ZipCode);
}

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    
    public static Money Zero(string currency = "USD") => new() { Amount = 0, Currency = currency };
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} to {other.Currency}");
        return new Money { Amount = Amount + other.Amount, Currency = Currency };
    }
}

// Entity with value objects
public class Customer : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public Address? BillingAddress { get; set; }
    public Address? ShippingAddress { get; set; }
    public Money AccountBalance { get; set; } = Money.Zero();
}
```

### Aggregate Roots

```csharp
public class Order : EntityBase<Guid>, IAggregateRoot
{
    private readonly List<OrderItem> _items = new();
    
    public DateTime OrderDate { get; private set; } = DateTime.UtcNow;
    public string CustomerId { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    
    // Read-only access to items
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    
    public Money Total => _items.Aggregate(Money.Zero(), (sum, item) => sum.Add(item.LineTotal));
    
    public Order(string customerId)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
    }
    
    // Business methods that maintain invariants
    public void AddItem(string productId, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed orders");
            
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            _items.Add(new OrderItem(Id, productId, quantity, unitPrice));
        }
        
        MarkAsModified();
    }
    
    public void RemoveItem(string productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify confirmed orders");
            
        _items.RemoveAll(i => i.ProductId == productId);
        MarkAsModified();
    }
    
    public void Confirm()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm empty order");
            
        Status = OrderStatus.Confirmed;
        MarkAsModified();
    }
}

public interface IAggregateRoot
{
    // Marker interface for aggregate roots
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
```

### Domain Events

```csharp
public abstract class DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}

public class CustomerCreatedEvent : DomainEvent
{
    public string CustomerId { get; }
    public string CustomerName { get; }
    
    public CustomerCreatedEvent(string customerId, string customerName)
    {
        CustomerId = customerId;
        CustomerName = customerName;
    }
}

// Entity with domain events
public class Customer : EntityBase, IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = new();
    
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public Customer(string name, string email)
    {
        GenerateId();
        Name = name;
        Email = email;
        
        // Raise domain event
        _domainEvents.Add(new CustomerCreatedEvent(Id, Name));
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

## 🧪 Testing

```csharp
[Test]
public void EntityBase_WhenCreated_ShouldHaveCreationTimestamp()
{
    var customer = new Customer { Name = "Test" };
    customer.GenerateId();
    
    customer.HasValidId.ShouldBeTrue();
    customer.CreatedAt.ShouldBeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    customer.ModifiedAt.ShouldBeNull();
    customer.IsDeleted.ShouldBeFalse();
}

[Test]
public void EntityBase_WhenModified_ShouldUpdateTimestamp()
{
    var customer = new Customer { Name = "Test" };
    var originalCreated = customer.CreatedAt;
    
    Thread.Sleep(10); // Ensure time difference
    customer.MarkAsModified();
    
    customer.ModifiedAt.ShouldNotBeNull();
    customer.ModifiedAt.ShouldBeGreaterThan(originalCreated);
}

[Test]
public void EntityBase_WhenSoftDeleted_ShouldSetFlags()
{
    var customer = new Customer { Name = "Test" };
    
    customer.MarkAsDeleted();
    
    customer.IsDeleted.ShouldBeTrue();
    customer.DeletedAt.ShouldNotBeNull();
    customer.ModifiedAt.ShouldNotBeNull();
}

[Test]
public void EntityBase_Equality_ShouldBeBasedOnIdAndType()
{
    var customer1 = new Customer { Id = "123", Name = "John" };
    var customer2 = new Customer { Id = "123", Name = "Jane" };
    var product = new Product { Id = 123, Name = "Widget" };
    
    customer1.Equals(customer2).ShouldBeTrue(); // Same ID, same type
    customer1.Equals(product).ShouldBeFalse();  // Different type
}
```

## 📚 Integration Examples

### With Entity Framework Core

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity base properties
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Version).IsRowVersion();
            
            // Global query filter for soft deletes
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // Configure value objects
        modelBuilder.Entity<Customer>().OwnsOne(c => c.BillingAddress);
        modelBuilder.Entity<Customer>().OwnsOne(c => c.ShippingAddress);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set modification timestamps
        var entities = ChangeTracker.Entries<EntityBase>()
            .Where(e => e.State == EntityState.Modified);
            
        foreach (var entity in entities)
        {
            entity.Entity.MarkAsModified();
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

### With Dapper

```csharp
public class CustomerRepository
{
    private readonly IDbConnection _connection;
    
    public async Task<Customer?> GetByIdAsync(string id)
    {
        const string sql = @"
            SELECT Id, Name, Email, Region, TotalValue, IsActive, 
                   CreatedAt, ModifiedAt, IsDeleted, DeletedAt, Version
            FROM Customers 
            WHERE Id = @Id AND IsDeleted = 0";
            
        return await _connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
    }
    
    public async Task<int> InsertAsync(Customer customer)
    {
        customer.GenerateId();
        
        const string sql = @"
            INSERT INTO Customers (Id, Name, Email, Region, TotalValue, IsActive, CreatedAt)
            VALUES (@Id, @Name, @Email, @Region, @TotalValue, @IsActive, @CreatedAt)";
            
        return await _connection.ExecuteAsync(sql, customer);
    }
}
```

## 🔄 Version History

- **0.1.0-preview**: Initial release with EntityBase and value objects
- **Future**: Domain events, aggregate root patterns, specification pattern

## 📄 License

Licensed under the Apache License 2.0. See [LICENSE](../../LICENSE) for details.