# FractalDataWorks MsSql Data Provider

A comprehensive, type-safe Microsoft SQL Server data provider for the FractalDataWorks framework, featuring expression-based queries, advanced transaction management, and enterprise-grade security.

## Overview

The **FractalDataWorks MsSql Data Provider** is designed with a security-first approach, providing type-safe data access while maintaining high performance and developer productivity. It eliminates SQL injection vulnerabilities through expression-based query generation and offers comprehensive transaction support with proper isolation levels.

### Key Features

- **Expression-Based Queries**: Type-safe LINQ-style expressions that compile to efficient SQL
- **SQL Injection Prevention**: All queries are automatically parameterized
- **Comprehensive Transaction Support**: Full ACID compliance with configurable isolation levels
- **Connection Pooling**: Intelligent connection management with health monitoring
- **Schema Mapping**: Multi-tenant support with logical-to-physical schema mapping
- **Enhanced Enums Integration**: Strongly-typed service configurations
- **Retry Logic**: Automatic transient error recovery with exponential backoff
- **Performance Monitoring**: Built-in metrics and diagnostic capabilities

### Design Philosophy

The provider follows FractalDataWorks' core principles:
- **Type Safety First**: Catch errors at compile time, not runtime
- **Security by Design**: Prevent common vulnerabilities through architecture
- **Developer Experience**: IntelliSense support and refactoring safety
- **Enterprise Ready**: Production-grade features like connection pooling and monitoring

## Architecture

### Command Pattern Implementation

The provider uses the Command Pattern for all data operations, ensuring consistent behavior and enabling features like retry logic and transaction management.

```
IDataCommand Interface
├── MsSqlCommandBase (Base Implementation)
├── MsSqlQueryCommand<T> (SELECT operations)
├── MsSqlInsertCommand<T> (INSERT operations)
├── MsSqlUpdateCommand<T> (UPDATE operations)
├── MsSqlDeleteCommand<T> (DELETE operations)
└── MsSqlUpsertCommand<T> (MERGE operations)
```

### ServiceBase Integration

The `MsSqlDataProvider` extends `DataProvidersServiceBase`, providing:
- Unified logging and diagnostics
- Configuration validation
- Health monitoring
- Metrics collection

### Enhanced Enum Service Types

Pre-configured service types for different scenarios:
- **Default**: General-purpose configuration
- **ReadOnly**: Optimized for query operations
- **HighPerformance**: Maximum throughput settings
- **Reporting**: Long-running query support
- **Transactional**: ACID compliance with proper isolation

### Schema Mapping Capabilities

Support for multi-tenant architectures through logical schema mapping:

```csharp
public IDictionary<string, string> SchemaMapping { get; set; } = new Dictionary<string, string>
{
    ["Sales"] = "sales_tenant1",
    ["Inventory"] = "inventory_tenant1",
    ["Users"] = "users_tenant1"
};
```

### Transaction Management

Full transaction support with:
- Configurable isolation levels
- Automatic deadlock detection and retry
- Nested transaction support
- Timeout management

### Connection Pooling

Intelligent connection management featuring:
- Automatic pool sizing based on load
- Connection health monitoring
- Failover support for high availability
- Performance metrics collection

## Configuration

### Complete MsSqlConfiguration Properties

```csharp
public sealed class MsSqlConfiguration : ConfigurationBase<MsSqlConfiguration>
{
    // Connection Settings
    public string ConnectionString { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public int? Port { get; set; }
    public string? InstanceName { get; set; }
    
    // Authentication
    public bool UseWindowsAuthentication { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    // Security
    public bool EncryptConnection { get; set; } = true;
    public bool TrustServerCertificate { get; set; }
    
    // Performance
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    public int CommandTimeoutSeconds { get; set; } = 300;
    public bool EnableConnectionPooling { get; set; } = true;
    public int MaxPoolSize { get; set; } = 100;
    
    // Retry Policy
    public bool EnableAutoRetry { get; set; } = true;
    public IDataRetryPolicy? RetryPolicy { get; set; }
    
    // Schema Management
    public IDictionary<string, string> SchemaMapping { get; set; }
    public string DefaultSchema { get; set; } = "dbo";
    
    // Additional Settings
    public string ApplicationName { get; set; } = "FractalDataWorks";
    public bool EnableMars { get; set; }
    public string? WorkstationId { get; set; }
    public int? PacketSize { get; set; }
    public IDictionary<string, string> AdditionalParameters { get; set; }
}
```

### Configuration Examples

#### Basic Configuration (appsettings.json)

```json
{
  "DataProviders": {
    "MsSql": {
      "Configurations": {
        "Default": {
          "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=MyApp;Integrated Security=true;Trust Server Certificate=true;",
          "CommandTimeoutSeconds": 30,
          "EnableConnectionPooling": true,
          "MaxPoolSize": 100,
          "EnableAutoRetry": true
        }
      }
    }
  }
}
```

#### Advanced Multi-Schema Configuration

```json
{
  "DataProviders": {
    "MsSql": {
      "Configurations": {
        "Default": {
          "ServerName": "sql-server-01",
          "DatabaseName": "ProductionDB",
          "UseWindowsAuthentication": false,
          "Username": "app_user",
          "Password": "secure_password",
          "EncryptConnection": true,
          "TrustServerCertificate": false,
          "CommandTimeoutSeconds": 60,
          "EnableConnectionPooling": true,
          "MaxPoolSize": 200,
          "EnableAutoRetry": true,
          "SchemaMapping": {
            "Sales": "sales_prod",
            "Inventory": "inventory_prod",
            "Users": "users_prod"
          },
          "DefaultSchema": "dbo",
          "ApplicationName": "MyApplication",
          "EnableMars": true,
          "RetryPolicy": {
            "MaxRetries": 3,
            "RetryDelayMs": 1000,
            "MaxRetryDelayMs": 30000,
            "UseExponentialBackoff": true
          }
        }
      }
    }
  }
}
```

#### Performance Tuning Configuration

```json
{
  "DataProviders": {
    "MsSql": {
      "Configurations": {
        "HighPerformance": {
          "ConnectionString": "Server=sql-cluster;Database=HighThroughputDB;Integrated Security=true;",
          "CommandTimeoutSeconds": 120,
          "EnableConnectionPooling": true,
          "MaxPoolSize": 500,
          "PacketSize": 32768,
          "EnableMars": true,
          "AdditionalParameters": {
            "Min Pool Size": "50",
            "Connection Lifetime": "300",
            "Load Balance Timeout": "5"
          }
        }
      }
    }
  }
}
```

## Command Pattern Details

### Expression-Based Query System

The provider converts LINQ expressions to parameterized SQL, ensuring type safety and preventing SQL injection:

```csharp
// Type-safe expression
Expression<Func<Customer, bool>> predicate = c => c.IsActive && c.CreditLimit > 5000m;

// Generated SQL
// SELECT * FROM [Customer] WHERE ([IsActive] = @p0 AND [CreditLimit] > @p1)
// Parameters: @p0 = true, @p1 = 5000.0
```

### Supported Expression Types

- **Binary Operations**: `==`, `!=`, `>`, `>=`, `<`, `<=`, `&&`, `||`
- **String Operations**: `Contains()`, `StartsWith()`, `EndsWith()`
- **Collection Operations**: `Contains()` for IN queries
- **Null Checking**: `== null`, `!= null`
- **Negation**: `!` operator for NOT conditions

### Type Safety Benefits

1. **Compile-Time Validation**: Errors caught during compilation
2. **IntelliSense Support**: Full IDE auto-completion
3. **Refactoring Safety**: Property renames update all queries
4. **No Magic Strings**: Eliminate error-prone string concatenation

### SQL Injection Prevention

All expressions are automatically converted to parameterized queries:

```csharp
// Safe expression-based query
var customers = await dataProvider.Execute(
    MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.Name == userInput));

// Generated safe SQL
// SELECT * FROM [Customer] WHERE [Name] = @p0
// Parameter: @p0 = userInput (safely escaped)
```

## Complete Code Examples

### Service Registration

#### Basic Registration

```csharp
using FractalDataWorks.Services.DataProviders.MsSql.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register default MsSql data provider
builder.Services.AddMsSqlDataProvider(builder.Configuration);

var app = builder.Build();
```

#### Multiple Service Types Registration

```csharp
using FractalDataWorks.Services.DataProviders.MsSql.Extensions;
using FractalDataWorks.Services.DataProviders.MsSql.EnhancedEnums;

var builder = WebApplication.CreateBuilder(args);

// Register multiple specialized configurations
builder.Services.AddMsSqlDataProviders(
    builder.Configuration,
    MsSqlDataProviderServiceType.Default,
    MsSqlDataProviderServiceType.ReadOnly,
    MsSqlDataProviderServiceType.HighPerformance
);

var app = builder.Build();
```

#### Host Builder Registration

```csharp
using FractalDataWorks.Services.DataProviders.MsSql.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMsSqlDataProvider(context.Configuration);
    })
    .Build();
```

### Basic CRUD Operations

#### Entity Definition

```csharp
public sealed class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public CustomerStatus Status { get; set; }
}

public enum CustomerStatus
{
    Pending = 0,
    Active = 1,
    Inactive = 2,
    Suspended = 3
}
```

#### CREATE Operations

```csharp
public class CustomerService
{
    private readonly MsSqlDataProvider _dataProvider;

    public CustomerService(MsSqlDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<IFdwResult<int>> CreateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create type-safe insert command
            var command = new MsSqlInsertCommand<Customer>(customer);
            
            // Execute and return the new ID
            var result = await _dataProvider.Execute<int>(command, cancellationToken);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"Created customer with ID: {result.Value}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            return FdwResult<int>.Failure($"Failed to create customer: {ex.Message}");
        }
    }

    public async Task<IFdwResult> CreateMultipleCustomersAsync(IEnumerable<Customer> customers, CancellationToken cancellationToken = default)
    {
        using var transaction = await _dataProvider.BeginTransactionAsync(cancellationToken: cancellationToken);
        
        try
        {
            foreach (var customer in customers)
            {
                var command = new MsSqlInsertCommand<Customer>(customer);
                var result = await _dataProvider.Execute<int>(command, cancellationToken);
                
                if (result.Error)
                {
                    await transaction.Value!.RollbackAsync(cancellationToken);
                    return FdwResult.Failure($"Failed to create customer {customer.Name}: {result.Message}");
                }
            }
            
            await transaction.Value!.CommitAsync(cancellationToken);
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            await transaction.Value!.RollbackAsync(cancellationToken);
            return FdwResult.Failure($"Transaction failed: {ex.Message}");
        }
    }
}
```

#### READ Operations

```csharp
public async Task<Customer?> GetCustomerByIdAsync(int customerId, CancellationToken cancellationToken = default)
{
    try
    {
        // Type-safe query by ID
        var command = MsSqlQueryCommandFactory.FindById<Customer>(customerId);
        var result = await _dataProvider.Execute<Customer?>(command, cancellationToken);
        
        return result.IsSuccess ? result.Value : null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving customer {customerId}: {ex.Message}");
        return null;
    }
}

public async Task<IEnumerable<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
{
    try
    {
        // Expression-based filtering
        var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
            c => c.IsActive && c.Status == CustomerStatus.Active);
        
        var result = await _dataProvider.Execute<IEnumerable<Customer>>(command, cancellationToken);
        
        return result.IsSuccess && result.Value != null 
            ? result.Value 
            : Enumerable.Empty<Customer>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving active customers: {ex.Message}");
        return Enumerable.Empty<Customer>();
    }
}

public async Task<IEnumerable<Customer>> GetCustomersPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
{
    try
    {
        var offset = (pageNumber - 1) * pageSize;
        
        // Paged query with ordering
        var command = MsSqlQueryCommandFactory.FindWithPaging<Customer>(
            predicate: c => c.IsActive,
            orderBy: c => c.Name,
            offset: offset,
            pageSize: pageSize);
        
        var result = await _dataProvider.Execute<IEnumerable<Customer>>(command, cancellationToken);
        
        return result.IsSuccess && result.Value != null 
            ? result.Value 
            : Enumerable.Empty<Customer>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving customers page {pageNumber}: {ex.Message}");
        return Enumerable.Empty<Customer>();
    }
}
```

#### UPDATE Operations

```csharp
public async Task<IFdwResult<int>> UpdateCustomerCreditLimitAsync(int customerId, decimal newCreditLimit, CancellationToken cancellationToken = default)
{
    try
    {
        // Create update command with WHERE clause
        var command = new MsSqlUpdateCommand<Customer>(
            setClause: "CreditLimit = @CreditLimit", 
            whereClause: "Id = @Id",
            parameters: new Dictionary<string, object?>
            {
                ["CreditLimit"] = newCreditLimit,
                ["Id"] = customerId
            });
        
        var result = await _dataProvider.Execute<int>(command, cancellationToken);
        
        if (result.IsSuccess && result.Value > 0)
        {
            Console.WriteLine($"Updated credit limit for customer {customerId} to ${newCreditLimit}");
        }
        
        return result;
    }
    catch (Exception ex)
    {
        return FdwResult<int>.Failure($"Failed to update customer credit limit: {ex.Message}");
    }
}

public async Task<IFdwResult<int>> UpdateCustomerStatusAsync(int customerId, CustomerStatus newStatus, CancellationToken cancellationToken = default)
{
    try
    {
        // Type-safe update with enum
        var command = new MsSqlUpdateCommand<Customer>(
            setClause: "Status = @Status", 
            whereClause: "Id = @Id",
            parameters: new Dictionary<string, object?>
            {
                ["Status"] = newStatus,
                ["Id"] = customerId
            });
        
        return await _dataProvider.Execute<int>(command, cancellationToken);
    }
    catch (Exception ex)
    {
        return FdwResult<int>.Failure($"Failed to update customer status: {ex.Message}");
    }
}
```

#### DELETE Operations

```csharp
public async Task<IFdwResult<int>> DeleteCustomerAsync(int customerId, CancellationToken cancellationToken = default)
{
    try
    {
        // Type-safe delete command
        var command = new MsSqlDeleteCommand<Customer>(
            whereClause: "Id = @Id",
            parameters: new Dictionary<string, object?>
            {
                ["Id"] = customerId
            });
        
        var result = await _dataProvider.Execute<int>(command, cancellationToken);
        
        if (result.IsSuccess && result.Value > 0)
        {
            Console.WriteLine($"Deleted customer {customerId}");
        }
        
        return result;
    }
    catch (Exception ex)
    {
        return FdwResult<int>.Failure($"Failed to delete customer: {ex.Message}");
    }
}

public async Task<IFdwResult<int>> SoftDeleteCustomerAsync(int customerId, CancellationToken cancellationToken = default)
{
    try
    {
        // Soft delete by updating status
        return await UpdateCustomerStatusAsync(customerId, CustomerStatus.Inactive, cancellationToken);
    }
    catch (Exception ex)
    {
        return FdwResult<int>.Failure($"Failed to soft delete customer: {ex.Message}");
    }
}
```

### Complex Queries with Expressions

#### Advanced Filtering

```csharp
public async Task<IEnumerable<Customer>> FindHighValueCustomersAsync(CancellationToken cancellationToken = default)
{
    try
    {
        // Complex business logic in type-safe expressions
        var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
            c => c.IsActive && 
                 c.CreditLimit > 10000m && 
                 c.Status == CustomerStatus.Active &&
                 c.CreatedDate >= DateTime.UtcNow.AddMonths(-12));
        
        var result = await _dataProvider.Execute<IEnumerable<Customer>>(command, cancellationToken);
        
        return result.IsSuccess && result.Value != null 
            ? result.Value 
            : Enumerable.Empty<Customer>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error finding high-value customers: {ex.Message}");
        return Enumerable.Empty<Customer>();
    }
}

public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default)
{
    try
    {
        // String operations with type safety
        var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
            c => c.Name.Contains(searchTerm) || 
                 c.Email.Contains(searchTerm));
        
        var result = await _dataProvider.Execute<IEnumerable<Customer>>(command, cancellationToken);
        
        return result.IsSuccess && result.Value != null 
            ? result.Value 
            : Enumerable.Empty<Customer>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error searching customers: {ex.Message}");
        return Enumerable.Empty<Customer>();
    }
}

public async Task<IEnumerable<Customer>> FindCustomersInCountriesAsync(List<string> countries, CancellationToken cancellationToken = default)
{
    try
    {
        // Collection operations (IN clause)
        var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
            c => countries.Contains(c.Country) && c.IsActive);
        
        var result = await _dataProvider.Execute<IEnumerable<Customer>>(command, cancellationToken);
        
        return result.IsSuccess && result.Value != null 
            ? result.Value 
            : Enumerable.Empty<Customer>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error finding customers in countries: {ex.Message}");
        return Enumerable.Empty<Customer>();
    }
}
```

#### Aggregation Queries

```csharp
public async Task<int> CountActiveCustomersAsync(CancellationToken cancellationToken = default)
{
    try
    {
        var command = MsSqlQueryCommandFactory.Count<Customer>(c => c.IsActive);
        var result = await _dataProvider.Execute<int>(command, cancellationToken);
        
        return result.IsSuccess ? result.Value : 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error counting active customers: {ex.Message}");
        return 0;
    }
}

public async Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken = default)
{
    try
    {
        var command = MsSqlQueryCommandFactory.Any<Customer>(c => c.Email == email);
        var result = await _dataProvider.Execute<bool>(command, cancellationToken);
        
        return result.IsSuccess && result.Value;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error checking email existence: {ex.Message}");
        return false;
    }
}
```

### Transactions

#### Simple Transaction

```csharp
public async Task<IFdwResult> CreateCustomerWithOrderAsync(Customer customer, Order order, CancellationToken cancellationToken = default)
{
    using var transactionResult = await _dataProvider.BeginTransactionAsync(cancellationToken: cancellationToken);
    
    if (transactionResult.Error)
    {
        return FdwResult.Failure($"Failed to begin transaction: {transactionResult.Message}");
    }
    
    var transaction = transactionResult.Value!;
    
    try
    {
        // Create customer
        var createCustomerCommand = new MsSqlInsertCommand<Customer>(customer);
        var customerResult = await _dataProvider.Execute<int>(createCustomerCommand, cancellationToken);
        
        if (customerResult.Error)
        {
            await transaction.RollbackAsync(cancellationToken);
            return FdwResult.Failure($"Failed to create customer: {customerResult.Message}");
        }
        
        // Update order with customer ID
        order.CustomerId = customerResult.Value;
        
        // Create order
        var createOrderCommand = new MsSqlInsertCommand<Order>(order);
        var orderResult = await _dataProvider.Execute<int>(createOrderCommand, cancellationToken);
        
        if (orderResult.Error)
        {
            await transaction.RollbackAsync(cancellationToken);
            return FdwResult.Failure($"Failed to create order: {orderResult.Message}");
        }
        
        // Commit transaction
        await transaction.CommitAsync(cancellationToken);
        
        Console.WriteLine($"Successfully created customer {customerResult.Value} with order {orderResult.Value}");
        return FdwResult.Success();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        return FdwResult.Failure($"Transaction failed: {ex.Message}");
    }
}
```

#### Advanced Transaction with Isolation Level

```csharp
public async Task<IFdwResult> ProcessHighValueOrderAsync(Order order, CancellationToken cancellationToken = default)
{
    // Use Serializable isolation for high-value transactions
    using var transactionResult = await _dataProvider.BeginTransactionAsync(
        isolationLevel: FdwTransactionIsolationLevel.Serializable,
        timeout: TimeSpan.FromMinutes(5),
        cancellationToken: cancellationToken);
    
    if (transactionResult.Error)
    {
        return FdwResult.Failure($"Failed to begin transaction: {transactionResult.Message}");
    }
    
    var transaction = transactionResult.Value!;
    
    try
    {
        // Check customer credit limit
        var customer = await GetCustomerByIdAsync(order.CustomerId, cancellationToken);
        if (customer == null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return FdwResult.Failure("Customer not found");
        }
        
        if (customer.CreditLimit < order.TotalAmount)
        {
            await transaction.RollbackAsync(cancellationToken);
            return FdwResult.Failure("Insufficient credit limit");
        }
        
        // Create order
        var createOrderCommand = new MsSqlInsertCommand<Order>(order);
        var orderResult = await _dataProvider.Execute<int>(createOrderCommand, cancellationToken);
        
        if (orderResult.Error)
        {
            await transaction.RollbackAsync(cancellationToken);
            return FdwResult.Failure($"Failed to create order: {orderResult.Message}");
        }
        
        // Update customer credit limit
        var newCreditLimit = customer.CreditLimit - order.TotalAmount;
        var updateResult = await UpdateCustomerCreditLimitAsync(customer.Id, newCreditLimit, cancellationToken);
        
        if (updateResult.Error)
        {
            await transaction.RollbackAsync(cancellationToken);
            return FdwResult.Failure($"Failed to update credit limit: {updateResult.Message}");
        }
        
        // Commit transaction
        await transaction.CommitAsync(cancellationToken);
        
        Console.WriteLine($"Successfully processed high-value order {orderResult.Value}");
        return FdwResult.Success();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        return FdwResult.Failure($"High-value transaction failed: {ex.Message}");
    }
}
```

### Bulk Operations

#### Bulk Insert

```csharp
public async Task<IFdwResult> BulkCreateCustomersAsync(IEnumerable<Customer> customers, CancellationToken cancellationToken = default)
{
    const int batchSize = 1000;
    var customerList = customers.ToList();
    var totalCreated = 0;
    
    try
    {
        for (int i = 0; i < customerList.Count; i += batchSize)
        {
            var batch = customerList.Skip(i).Take(batchSize);
            
            using var transaction = await _dataProvider.BeginTransactionAsync(cancellationToken: cancellationToken);
            if (transaction.Error)
            {
                return FdwResult.Failure($"Failed to begin transaction for batch {i / batchSize + 1}");
            }
            
            try
            {
                foreach (var customer in batch)
                {
                    var command = new MsSqlInsertCommand<Customer>(customer);
                    var result = await _dataProvider.Execute<int>(command, cancellationToken);
                    
                    if (result.Error)
                    {
                        await transaction.Value!.RollbackAsync(cancellationToken);
                        return FdwResult.Failure($"Failed to create customer in batch: {result.Message}");
                    }
                    
                    totalCreated++;
                }
                
                await transaction.Value!.CommitAsync(cancellationToken);
                Console.WriteLine($"Successfully created batch {i / batchSize + 1} with {batch.Count()} customers");
            }
            catch (Exception ex)
            {
                await transaction.Value!.RollbackAsync(cancellationToken);
                return FdwResult.Failure($"Batch {i / batchSize + 1} failed: {ex.Message}");
            }
        }
        
        Console.WriteLine($"Bulk operation completed. Total customers created: {totalCreated}");
        return FdwResult.Success();
    }
    catch (Exception ex)
    {
        return FdwResult.Failure($"Bulk create operation failed: {ex.Message}");
    }
}
```

#### Bulk Update

```csharp
public async Task<IFdwResult<int>> BulkUpdateCustomerStatusAsync(CustomerStatus fromStatus, CustomerStatus toStatus, CancellationToken cancellationToken = default)
{
    try
    {
        // Bulk update using single command
        var command = new MsSqlUpdateCommand<Customer>(
            setClause: "Status = @ToStatus",
            whereClause: "Status = @FromStatus",
            parameters: new Dictionary<string, object?>
            {
                ["ToStatus"] = toStatus,
                ["FromStatus"] = fromStatus
            });
        
        var result = await _dataProvider.Execute<int>(command, cancellationToken);
        
        if (result.IsSuccess && result.Value > 0)
        {
            Console.WriteLine($"Updated {result.Value} customers from {fromStatus} to {toStatus}");
        }
        
        return result;
    }
    catch (Exception ex)
    {
        return FdwResult<int>.Failure($"Bulk update failed: {ex.Message}");
    }
}
```

### Schema-Aware Operations

#### Multi-Tenant Schema Mapping

```csharp
public class TenantAwareCustomerService
{
    private readonly MsSqlDataProvider _dataProvider;
    private readonly string _tenantSchema;

    public TenantAwareCustomerService(MsSqlDataProvider dataProvider, string tenantId)
    {
        _dataProvider = dataProvider;
        _tenantSchema = $"tenant_{tenantId}";
    }

    public async Task<Customer?> GetCustomerByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use schema-aware query
            var command = MsSqlQueryCommandFactory.FindById<Customer>(customerId, schema: _tenantSchema);
            var result = await _dataProvider.Execute<Customer?>(command, cancellationToken);
            
            return result.IsSuccess ? result.Value : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving customer {customerId} from schema {_tenantSchema}: {ex.Message}");
            return null;
        }
    }

    public async Task<IFdwResult<int>> CreateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create command with schema metadata
            var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["Schema"] = _tenantSchema
            };
            
            var command = new MsSqlInsertCommand<Customer>(customer, metadata: metadata);
            var result = await _dataProvider.Execute<int>(command, cancellationToken);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"Created customer {result.Value} in schema {_tenantSchema}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            return FdwResult<int>.Failure($"Failed to create customer in schema {_tenantSchema}: {ex.Message}");
        }
    }
}
```

## Best Practices

### Connection Lifecycle Management

```csharp
// ✅ Good: Let the provider manage connections
public async Task<Customer?> GetCustomerAsync(int id)
{
    var command = MsSqlQueryCommandFactory.FindById<Customer>(id);
    var result = await _dataProvider.Execute<Customer?>(command, CancellationToken.None);
    return result.IsSuccess ? result.Value : null;
}

// ❌ Avoid: Manual connection management
// The provider handles connection pooling automatically
```

### Transaction Patterns

```csharp
// ✅ Good: Use using statements for automatic cleanup
public async Task<IFdwResult> ProcessOrderAsync(Order order)
{
    using var transactionResult = await _dataProvider.BeginTransactionAsync();
    if (transactionResult.Error) return FdwResult.Failure(transactionResult.Message!);
    
    var transaction = transactionResult.Value!;
    
    try
    {
        // Business operations
        await CreateOrderAsync(order);
        await UpdateInventoryAsync(order.Items);
        
        await transaction.CommitAsync();
        return FdwResult.Success();
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return FdwResult.Failure(ex.Message);
    }
}
```

### Error Handling

```csharp
// ✅ Good: Comprehensive error handling
public async Task<IFdwResult<IEnumerable<Customer>>> GetCustomersAsync()
{
    try
    {
        var command = MsSqlQueryCommandFactory.FindAll<Customer>();
        var result = await _dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);
        
        if (result.Error)
        {
            Console.WriteLine($"Query failed: {result.Message}");
            return FdwResult<IEnumerable<Customer>>.Failure(result.Message!);
        }
        
        return FdwResult<IEnumerable<Customer>>.Success(result.Value ?? Enumerable.Empty<Customer>());
    }
    catch (TimeoutException ex)
    {
        return FdwResult<IEnumerable<Customer>>.Failure($"Query timeout: {ex.Message}");
    }
    catch (SqlException ex) when (ex.Number == 2) // Timeout
    {
        return FdwResult<IEnumerable<Customer>>.Failure("Database timeout occurred");
    }
    catch (Exception ex)
    {
        return FdwResult<IEnumerable<Customer>>.Failure($"Unexpected error: {ex.Message}");
    }
}
```

### Performance Optimization

```csharp
// ✅ Good: Use paging for large datasets
public async Task<IEnumerable<Customer>> GetCustomersPagedAsync(int page, int size)
{
    var command = MsSqlQueryCommandFactory.FindWithPaging<Customer>(
        predicate: c => c.IsActive,
        orderBy: c => c.Name,
        offset: (page - 1) * size,
        pageSize: size);
    
    var result = await _dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);
    return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
}

// ✅ Good: Use specific queries when you need only certain fields
public async Task<IEnumerable<CustomerSummary>> GetCustomerSummariesAsync()
{
    var sql = "SELECT Id, Name, Email FROM [Customer] WHERE [IsActive] = @IsActive";
    var parameters = new Dictionary<string, object?> { ["IsActive"] = true };
    
    var command = new MsSqlQueryCommand<IEnumerable<CustomerSummary>>(sql, "Customer", parameters);
    var result = await _dataProvider.Execute<IEnumerable<CustomerSummary>>(command, CancellationToken.None);
    
    return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<CustomerSummary>();
}
```

### Schema Design Recommendations

```csharp
// ✅ Good: Use explicit schema mapping
var configuration = new MsSqlConfiguration
{
    SchemaMapping = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Sales"] = "sales_prod",
        ["Inventory"] = "inventory_prod",
        ["Users"] = "users_prod"
    },
    DefaultSchema = "dbo"
};

// ✅ Good: Use consistent naming conventions
public sealed class Customer  // PascalCase for entities
{
    public int Id { get; set; }              // PascalCase for properties
    public string FirstName { get; set; }    // Descriptive names
    public string LastName { get; set; }
    public CustomerStatus Status { get; set; } // Enum for fixed values
}
```

## Portability and Consistency

### Command Portability Across Providers

Commands created with the MsSql provider follow FractalDataWorks standards, making them portable across different database providers:

```csharp
// This pattern works across all FractalDataWorks providers
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetActiveAsync();
    Task<IFdwResult<int>> CreateAsync(Customer customer);
}

// Implementation can switch between providers transparently
public class CustomerRepository : ICustomerRepository
{
    private readonly IDataService _dataService; // Provider-agnostic interface
    
    public async Task<Customer?> GetByIdAsync(int id)
    {
        // Works with MsSql, PostgreSql, MySQL, etc.
        var command = QueryCommandFactory.FindById<Customer>(id);
        var result = await _dataService.ExecuteAsync<Customer?>(command);
        return result.IsSuccess ? result.Value : null;
    }
}
```

### Naming Conventions Following FractalDataWorks Patterns

- **Entities**: PascalCase, singular nouns (`Customer`, `Order`, `Product`)
- **Properties**: PascalCase (`FirstName`, `LastName`, `CreatedDate`)
- **Enums**: PascalCase with descriptive values (`CustomerStatus.Active`)
- **Services**: PascalCase with "Service" suffix (`CustomerService`, `OrderService`)
- **Commands**: Descriptive action names (`CreateCustomerCommand`, `UpdateOrderStatusCommand`)

### Integration with Other FractalDataWorks Services

```csharp
// Seamless integration with other framework services
public class OrderProcessingService
{
    private readonly MsSqlDataProvider _dataProvider;
    private readonly ISecretManager _secretManager;          // FractalDataWorks Secret Management
    private readonly IScheduler _scheduler;                  // FractalDataWorks Scheduling
    private readonly IExternalConnectionService _connections; // FractalDataWorks External Connections

    public async Task ProcessOrderAsync(Order order)
    {
        // Database operations
        await _dataProvider.Execute(new MsSqlInsertCommand<Order>(order), CancellationToken.None);
        
        // Secret management
        var apiKey = await _secretManager.GetSecretAsync("PaymentGatewayKey");
        
        // Schedule follow-up
        await _scheduler.ScheduleTaskAsync("SendOrderConfirmation", TimeSpan.FromMinutes(5));
        
        // External service integration
        await _connections.SendAsync("PaymentService", order);
    }
}
```

## Migration Guide

### Coming from Entity Framework

#### Entity Framework Pattern

```csharp
// Entity Framework approach
public async Task<Customer?> GetCustomerAsync(int id)
{
    using var context = new AppDbContext();
    return await context.Customers
        .Where(c => c.Id == id && c.IsActive)
        .FirstOrDefaultAsync();
}
```

#### FractalDataWorks MsSql Provider Equivalent

```csharp
// FractalDataWorks approach
public async Task<Customer?> GetCustomerAsync(int id)
{
    var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
        c => c.Id == id && c.IsActive);
    var result = await _dataProvider.Execute<Customer?>(command, CancellationToken.None);
    return result.IsSuccess ? result.Value : null;
}
```

#### Key Differences

1. **No DbContext**: Direct service injection instead of context management
2. **Explicit Commands**: Commands are created explicitly for better control
3. **Result Wrapping**: All operations return `IFdwResult<T>` for consistent error handling
4. **Type Safety**: Expression-based queries prevent runtime errors

### Coming from Raw ADO.NET

#### ADO.NET Pattern

```csharp
// ADO.NET approach
public async Task<Customer?> GetCustomerAsync(int id)
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    
    using var command = new SqlCommand("SELECT * FROM Customer WHERE Id = @Id", connection);
    command.Parameters.AddWithValue("@Id", id);
    
    using var reader = await command.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return new Customer
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            // ... map all fields
        };
    }
    return null;
}
```

#### FractalDataWorks MsSql Provider Equivalent

```csharp
// FractalDataWorks approach
public async Task<Customer?> GetCustomerAsync(int id)
{
    var command = MsSqlQueryCommandFactory.FindById<Customer>(id);
    var result = await _dataProvider.Execute<Customer?>(command, CancellationToken.None);
    return result.IsSuccess ? result.Value : null;
}
```

#### Key Benefits

1. **Automatic Mapping**: No manual field mapping required
2. **Connection Management**: Automatic connection pooling and cleanup
3. **Parameter Safety**: Automatic parameterization prevents SQL injection
4. **Error Handling**: Consistent error handling with retry logic

### Coming from Dapper

#### Dapper Pattern

```csharp
// Dapper approach
public async Task<Customer?> GetCustomerAsync(int id)
{
    using var connection = new SqlConnection(connectionString);
    var sql = "SELECT * FROM Customer WHERE Id = @Id";
    return await connection.QuerySingleOrDefaultAsync<Customer>(sql, new { Id = id });
}
```

#### FractalDataWorks MsSql Provider Equivalent

```csharp
// FractalDataWorks approach (maintains Dapper's simplicity)
public async Task<Customer?> GetCustomerAsync(int id)
{
    var command = MsSqlQueryCommandFactory.FindById<Customer>(id);
    var result = await _dataProvider.Execute<Customer?>(command, CancellationToken.None);
    return result.IsSuccess ? result.Value : null;
}
```

#### Key Advantages

1. **Type Safety**: Compile-time checking instead of runtime string errors
2. **Expression Queries**: LINQ-style expressions for complex filtering
3. **Built-in Retry**: Automatic transient error handling
4. **Transaction Support**: Easy transaction management with proper cleanup

### Migration Steps

1. **Update Dependencies**: Replace EF/ADO.NET/Dapper packages with FractalDataWorks packages
2. **Register Services**: Configure the MsSql provider in your DI container
3. **Convert Queries**: Replace string-based queries with expression-based commands
4. **Update Error Handling**: Use `IFdwResult<T>` pattern for consistent error handling
5. **Test Thoroughly**: Verify all operations work correctly with the new provider

## Summary

The FractalDataWorks MsSql Data Provider offers a comprehensive, type-safe solution for Microsoft SQL Server data access with enterprise-grade features. Its expression-based query system eliminates SQL injection vulnerabilities while providing excellent developer experience through IntelliSense support and compile-time error checking.

Key advantages include:
- **Security**: Automatic SQL injection prevention through parameterized queries
- **Performance**: Intelligent connection pooling and retry logic
- **Maintainability**: Type-safe expressions that survive refactoring
- **Scalability**: Support for transactions, bulk operations, and schema mapping
- **Integration**: Seamless integration with other FractalDataWorks services

Whether migrating from Entity Framework, ADO.NET, or Dapper, the provider offers a smooth transition path while providing enhanced security, performance, and developer productivity.