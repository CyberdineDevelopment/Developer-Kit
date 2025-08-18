# MsSql Data Provider - Quick Reference Guide

## Cheat Sheet

### Basic Setup
```csharp
// Factory creation
var factory = new MsSqlDataProviderFactory(logger, configRegistry);
var dataProvider = factory.Create(config).Value;

// Direct instantiation
var dataProvider = new MsSqlDataProvider(logger, configuration);
```

### Query Patterns (One-liners)
```csharp
// Find by ID
var customer = await dataProvider.Execute<Customer?>(MsSqlQueryCommandFactory.FindById<Customer>(id), ct);

// Find with condition
var customers = await dataProvider.Execute<IEnumerable<Customer>>(MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.IsActive), ct);

// Count records
var count = await dataProvider.Execute<int>(MsSqlQueryCommandFactory.Count<Customer>(c => c.IsActive), ct);

// Check existence
var exists = await dataProvider.Execute<bool>(MsSqlQueryCommandFactory.Any<Customer>(c => c.Email == email), ct);

// Paged results
var page = await dataProvider.Execute<IEnumerable<Customer>>(MsSqlQueryCommandFactory.FindWithPaging<Customer>(c => c.IsActive, c => c.Name, 0, 10), ct);
```

### Expression Syntax
```csharp
// Simple conditions
c => c.IsActive
c => c.Age > 18
c => c.Name == "John"

// Complex conditions
c => c.IsActive && c.Age > 18
c => c.Status == CustomerStatus.Active || c.Status == CustomerStatus.Pending

// String operations
c => c.Name.Contains("smith")
c => c.Email.StartsWith("admin")
c => c.Phone.EndsWith("1234")

// Collections
c => countries.Contains(c.Country)
c => !excludedIds.Contains(c.Id)

// Null checks
c => c.LastLogin != null
c => c.Notes == null
```

### Parameter Passing
```csharp
// Automatic from expressions
var cmd = MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.Id == customerId);

// Manual parameters
var parameters = new Dictionary<string, object?>(StringComparer.Ordinal) { ["CustomerId"] = 123 };
var cmd = new MsSqlQueryCommand<Customer>("SELECT * FROM Customer WHERE Id = @CustomerId", typeof(Customer), null, parameters);
```

### Transaction Usage
```csharp
// Begin transaction
var txResult = await dataProvider.BeginTransactionAsync(FdwTransactionIsolationLevel.ReadCommitted, TimeSpan.FromMinutes(5), ct);
using var transaction = txResult.Value;

// Execute in transaction
await transaction.ExecuteAsync(insertCommand, ct);
await transaction.ExecuteAsync(updateCommand, ct);

// Commit
await transaction.CommitAsync(ct);
```

## Common Scenarios

| Scenario | Command Type | Example Code |
|----------|--------------|--------------|
| **Find by Primary Key** | Query | `MsSqlQueryCommandFactory.FindById<Customer>(123)` |
| **Get All Records** | Query | `MsSqlQueryCommandFactory.FindAll<Customer>()` |
| **Filter Records** | Query | `MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.IsActive)` |
| **Count Records** | Query | `MsSqlQueryCommandFactory.Count<Customer>()` |
| **Check Existence** | Query | `MsSqlQueryCommandFactory.Any<Customer>(c => c.Email == email)` |
| **Paginated Results** | Query | `MsSqlQueryCommandFactory.FindWithPaging<Customer>(null, c => c.Name, 0, 10)` |
| **Insert Single Record** | Insert | `new MsSqlInsertCommand<Customer>(entity)` |
| **Update Record** | Update | `new MsSqlUpdateCommand<Customer>(entity, c => c.Id == id)` |
| **Delete Records** | Delete | `new MsSqlDeleteCommand<Customer>(c => c.IsActive == false)` |
| **Upsert Record** | Upsert | `new MsSqlUpsertCommand<Customer>(entity, c => c.Email)` |
| **Complex Filter** | Query | `MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.Age > 18 && c.Country == "US")` |
| **Date Range Query** | Query | `MsSqlQueryCommandFactory.FindWhere<Order>(o => o.Date >= start && o.Date <= end)` |
| **String Search** | Query | `MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.Name.Contains("Smith"))` |
| **IN Clause** | Query | `MsSqlQueryCommandFactory.FindWhere<Customer>(c => ids.Contains(c.Id))` |
| **Order with Limit** | Query | `MsSqlQueryCommandFactory.FindWhere<Product>(p => p.InStock, p => p.Price)` |

## Expression Examples

### Simple Comparisons
```csharp
// Equality
c => c.Id == 123
c => c.Status == CustomerStatus.Active
c => c.IsActive == true

// Numeric comparisons
c => c.Age > 18
c => c.Price >= 100.50m
c => c.Quantity < 10

// String comparisons
c => c.Name == "John Doe"
c => c.Email != null
```

### Complex Conditions
```csharp
// AND operations
c => c.IsActive && c.Age > 18 && c.Country == "US"

// OR operations
c => c.Status == CustomerStatus.Active || c.Status == CustomerStatus.Pending

// Mixed logical operations
c => (c.IsActive || c.IsPremium) && c.Age > 18

// Negation
c => !c.IsDeleted
c => !(c.Status == CustomerStatus.Suspended)
```

### String Operations
```csharp
// Contains (generates LIKE '%value%')
c => c.Name.Contains("smith")
c => c.Description.Contains(searchTerm)

// StartsWith (generates LIKE 'value%')
c => c.Email.StartsWith("admin")
c => c.Phone.StartsWith("+1")

// EndsWith (generates LIKE '%value')
c => c.Email.EndsWith("@company.com")
c => c.Filename.EndsWith(".pdf")

// Case sensitivity handled by SQL Server collation
```

### Date Operations
```csharp
// Date comparisons
c => c.CreatedDate >= DateTime.Today
c => c.LastLogin > DateTime.UtcNow.AddDays(-30)

// Date ranges
c => c.OrderDate >= startDate && c.OrderDate <= endDate

// Null date handling
c => c.LastLogin != null
c => c.DeletedDate == null
```

### Null Handling
```csharp
// Null checks
c => c.MiddleName != null
c => c.Notes == null

// Nullable value comparisons
c => c.LastLogin.HasValue
c => c.Score.GetValueOrDefault() > 80

// Combining with other conditions
c => c.IsActive && c.LastLogin != null
```

## Configuration Quick Setup

### Minimal Configuration
```csharp
var config = new MsSqlConfiguration
{
    ServerName = "localhost",
    DatabaseName = "MyDatabase",
    UseWindowsAuthentication = true
};
```

### With SQL Authentication
```csharp
var config = new MsSqlConfiguration
{
    ServerName = "server.domain.com",
    DatabaseName = "MyDatabase",
    UseWindowsAuthentication = false,
    Username = "sqluser",
    Password = "password123",
    EncryptConnection = true
};
```

### Connection String Formats
```csharp
// Windows Authentication
"Server=localhost;Database=MyDB;Integrated Security=true;Encrypt=true;"

// SQL Authentication
"Server=server,1433;Database=MyDB;User ID=user;Password=pass;Encrypt=true;"

// Named Instance
"Server=server\\SQLEXPRESS;Database=MyDB;Integrated Security=true;"

// With custom settings
"Server=localhost;Database=MyDB;Integrated Security=true;Connect Timeout=30;Command Timeout=300;Max Pool Size=100;"
```

### Most Common Settings
```csharp
var config = new MsSqlConfiguration
{
    ServerName = "localhost",
    DatabaseName = "MyDatabase",
    UseWindowsAuthentication = true,
    EncryptConnection = true,
    ConnectionTimeoutSeconds = 30,
    CommandTimeoutSeconds = 300,
    EnableConnectionPooling = true,
    MaxPoolSize = 100,
    EnableAutoRetry = true,
    DefaultSchema = "dbo"
};
```

## Troubleshooting Guide

### Common Errors and Solutions

**Error**: `Login failed for user`
- **Solution**: Check authentication settings, verify username/password, ensure SQL Server allows the authentication method

**Error**: `A network-related or instance-specific error occurred`
- **Solution**: Verify server name, check if SQL Server is running, confirm firewall settings, test network connectivity

**Error**: `Cannot open database requested by the login`
- **Solution**: Verify database name exists, check user permissions, ensure database is online

**Error**: `Timeout expired`
- **Solution**: Increase `CommandTimeoutSeconds`, optimize query performance, check for blocking

**Error**: `The connection pool is exhausted`
- **Solution**: Increase `MaxPoolSize`, ensure connections are properly disposed, check for connection leaks

### Performance Issues

**Slow Queries**:
- Add appropriate indexes for WHERE clause columns
- Use paging for large result sets with `FindWithPaging`
- Consider custom SQL for complex scenarios
- Monitor query execution plans

**High Memory Usage**:
- Use paging instead of loading all records
- Process large datasets in batches
- Dispose of data provider instances properly

**Connection Problems**:
- Enable connection pooling
- Adjust pool size based on concurrent users
- Implement retry logic for transient failures

### Transaction Deadlocks

**Prevention**:
```csharp
// Use appropriate isolation levels
var tx = await dataProvider.BeginTransactionAsync(FdwTransactionIsolationLevel.ReadCommitted);

// Keep transactions short
// Access resources in consistent order
// Use timeouts
```

**Handling**:
```csharp
try
{
    await transaction.ExecuteAsync(command, ct);
}
catch (SqlException ex) when (ex.Number == 1205) // Deadlock
{
    // Implement exponential backoff retry
    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(100, 1000)));
    // Retry operation
}
```

## Do's and Don'ts

### ✅ Do's

- **Use type-safe expressions** for compile-time safety
- **Implement proper error handling** with try-catch blocks
- **Use paging** for large result sets
- **Dispose resources properly** with using statements
- **Use async/await** consistently throughout your application
- **Implement retry logic** for transient failures
- **Use appropriate timeouts** for long-running operations
- **Validate input parameters** before creating queries
- **Use connection pooling** for better performance
- **Test with realistic data volumes** during development

### ❌ Don'ts

- **Don't use string concatenation** for SQL queries (SQL injection risk)
- **Don't load entire tables** without filtering or paging
- **Don't ignore error results** from Execute methods
- **Don't block async operations** with .Result or .Wait()
- **Don't create data provider instances** in loops
- **Don't forget to handle null results** from nullable queries
- **Don't use overly complex expressions** that can't be translated
- **Don't mix transaction contexts** across different providers
- **Don't use long-running transactions** that can cause blocking
- **Don't hardcode connection strings** in application code

### Security Reminders

- **Always use parameterized queries** (expressions do this automatically)
- **Validate and sanitize user input** before query construction
- **Use principle of least privilege** for database accounts
- **Enable connection encryption** in production environments
- **Don't log sensitive data** like passwords or personal information
- **Use schema mapping** for multi-tenant scenarios
- **Implement proper authentication** and authorization

## API Method Reference

### Factory Methods
| Method | Return Type | Required Parameters | Description |
|--------|-------------|---------------------|-------------|
| `Create(config)` | `IFdwResult<MsSqlDataProvider>` | `MsSqlConfiguration` | Creates provider instance |
| `GetService(name)` | `Task<MsSqlDataProvider>` | `string` | Gets provider by config name |
| `GetService(id)` | `Task<MsSqlDataProvider>` | `int` | Gets provider by config ID |

### Data Provider Methods
| Method | Return Type | Required Parameters | Description |
|--------|-------------|---------------------|-------------|
| `Execute<T>(command, ct)` | `Task<IFdwResult<T>>` | `IDataCommand, CancellationToken` | Executes command with result |
| `Execute(command, ct)` | `Task<IFdwResult>` | `IDataCommand, CancellationToken` | Executes command without result |
| `BeginTransactionAsync()` | `Task<IFdwResult<IDataTransaction>>` | Optional isolation, timeout, ct | Creates new transaction |
| `TestConnectionAsync(ct)` | `Task<IFdwResult>` | `CancellationToken` | Tests database connectivity |
| `GetConnectionInfo()` | `IFdwResult<ConnectionInfo>` | None | Gets connection information |

### Query Factory Methods
| Method | Return Type | Required Parameters | Description |
|--------|-------------|---------------------|-------------|
| `FindById<T>(id)` | `MsSqlQueryCommand<T?>` | `object` | Find entity by ID |
| `FindWhere<T>(predicate)` | `MsSqlQueryCommand<IEnumerable<T>>` | `Expression<Func<T, bool>>` | Filter entities |
| `FindAll<T>()` | `MsSqlQueryCommand<IEnumerable<T>>` | None | Get all entities |
| `FindWithPaging<T>()` | `MsSqlQueryCommand<IEnumerable<T>>` | predicate, orderBy, offset, size | Paginated query |
| `Count<T>(predicate)` | `MsSqlQueryCommand<int>` | Optional predicate | Count matching records |
| `Any<T>(predicate)` | `MsSqlQueryCommand<bool>` | `Expression<Func<T, bool>>` | Check if any match |

### Command Constructors
| Command Type | Constructor Parameters | Description |
|--------------|------------------------|-------------|
| `MsSqlQueryCommand<T>` | sql, target, parameters, metadata, timeout | Custom SELECT query |
| `MsSqlInsertCommand<T>` | entity, schema, metadata, timeout | INSERT operation |
| `MsSqlUpdateCommand<T>` | entity, predicate, schema, metadata, timeout | UPDATE operation |
| `MsSqlDeleteCommand<T>` | predicate, schema, metadata, timeout | DELETE operation |
| `MsSqlUpsertCommand<T>` | entity, keySelector, schema, metadata, timeout | MERGE/UPSERT operation |

### Transaction Methods
| Method | Return Type | Parameters | Description |
|--------|-------------|------------|-------------|
| `ExecuteAsync<T>(command, ct)` | `Task<IFdwResult<T>>` | command, cancellation token | Execute in transaction |
| `CommitAsync(ct)` | `Task<IFdwResult>` | cancellation token | Commit transaction |
| `RollbackAsync(ct)` | `Task<IFdwResult>` | cancellation token | Rollback transaction |

---

**Keep this reference handy while coding with the MsSql Data Provider!**