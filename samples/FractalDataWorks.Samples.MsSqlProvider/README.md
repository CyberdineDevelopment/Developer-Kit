# FractalDataWorks MsSql Data Provider Sample Application

This sample application demonstrates the comprehensive capabilities of the **FractalDataWorks MsSql Data Provider**, including type-safe CRUD operations, advanced querying, transactions, bulk operations, and performance optimization techniques.

## Overview

The sample application showcases a complete e-commerce-like database schema with the following features:

- **Type-safe query operations** using strongly-typed expressions
- **CRUD operations** (Create, Read, Update, Delete) with proper error handling
- **Advanced querying** with complex WHERE clauses, joins, and aggregations
- **Transaction management** with commit/rollback scenarios
- **Bulk operations** for performance optimization
- **Complex analytical queries** with grouping and hierarchy support
- **User activity logging** for analytics and audit trails
- **Performance monitoring** and connection pooling demonstrations

## Database Schema

The sample uses a multi-schema database structure:

### Sales Schema (`sales`)
- **Customers**: Customer master data with credit limits and versioning
- **Orders**: Order transactions linked to customers with status tracking

### Inventory Schema (`inventory`)
- **Categories**: Hierarchical product categorization (self-referencing)
- **Products**: Product catalog with pricing, stock status, and SKUs

### Users Schema (`users`)
- **UserActivity**: User activity logging for analytics and audit purposes

## Prerequisites

### Required Software

1. **SQL Server LocalDB** - Free, lightweight version of SQL Server
   - Download from: [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
   - Alternative: Full SQL Server Express or SQL Server Developer Edition

2. **.NET 10.0 SDK** - Required for building and running the application
   - Download from: [.NET Downloads](https://dotnet.microsoft.com/download/dotnet/10.0)

### Verification

Verify your setup by running these commands:

```bash
# Check .NET version
dotnet --version

# Check SQL Server LocalDB installation
sqllocaldb info

# List LocalDB instances
sqllocaldb info mssqllocaldb
```

## Quick Start

### 1. Clone and Navigate

```bash
git clone <repository-url>
cd Developer-Kit/samples/FractalDataWorks.Samples.MsSqlProvider
```

### 2. Run the Sample

```bash
# Build and run
dotnet run

# Or build first, then run
dotnet build
dotnet run --no-build
```

The application will:

1. **Automatically detect LocalDB** and establish connection
2. **Create the database** (`SampleDb`) if it doesn't exist
3. **Create schemas and tables** using the SQL scripts
4. **Insert sample data** to demonstrate operations
5. **Run comprehensive demonstrations** of all features
6. **Display detailed logging** of operations and performance metrics

### 3. Expected Output

```
================================================================================
FractalDataWorks MsSql Data Provider Sample Application
================================================================================

[12:34:56.789] Starting FractalDataWorks MsSql Provider Sample Application
[12:34:56.890] Checking LocalDB availability
[12:34:56.950] Successfully connected to LocalDB master database
[12:34:57.100] Executing script: 01_CreateDatabase.sql
[12:34:57.250] Successfully executed 01_CreateDatabase.sql in 150ms
[12:34:57.300] Executing script: 02_CreateSchemas.sql
[12:34:57.400] Successfully executed 02_CreateSchemas.sql in 100ms
[12:34:57.450] Executing script: 03_CreateTables.sql
[12:34:57.650] Successfully executed 03_CreateTables.sql in 200ms
[12:34:57.700] Executing script: 04_InsertSampleData.sql
[12:34:57.900] Successfully executed 04_InsertSampleData.sql in 200ms
[12:34:57.950] Database initialization completed successfully in 1060ms

=== Basic CRUD Operations ===
--- CREATE Operations ---
[12:34:58.100] Successfully created categories
[12:34:58.200] Created customer: John Doe (ID: 1)
[12:34:58.250] Created customer: Jane Smith (ID: 2)
[12:34:58.300] Created product: iPhone 15 Pro (ID: 1)
[12:34:58.350] Created product: Samsung Galaxy S24 (ID: 2)
[12:34:58.400] Created order: 1 for customer 1

--- READ Operations ---
[12:34:58.450] Found 2 active customers
[12:34:58.500] Found 1 customers with credit limit > $7,500
[12:34:58.550] Found 2 smartphone products
[12:34:58.600] Found 1 orders in the last 30 days
[12:34:58.650] First active customer: John Doe
[12:34:58.700] Total customers: 2, Active: 2

--- UPDATE Operations ---
[12:34:58.750] Updated credit limit for customer John Doe
[12:34:58.800] Applied 5% discount to 2 expensive products
[12:34:58.850] Updated 1 orders to Shipped status

--- DELETE Operations ---
[12:34:58.900] Successfully deleted test customer
[12:34:58.950] Found 0 inactive customers (soft deleted)
[12:34:59.000] Basic CRUD operations completed in 900ms

=== Advanced Query Operations ===
=== Transaction Operations ===
=== Bulk Operations ===
=== Complex Query Scenarios ===
=== Performance Samples ===

[12:35:05.500] Sample application completed successfully in 6500ms

================================================================================
Sample application completed successfully!
Total execution time: 6500ms
================================================================================
```

## Project Structure

```
FractalDataWorks.Samples.MsSqlProvider/
├── Models/                           # Entity models matching database schema
│   ├── Customer.cs                   # Customer entity with annotations
│   ├── Order.cs                      # Order entity with status management
│   ├── Product.cs                    # Product entity with category relationship
│   ├── Category.cs                   # Hierarchical category entity
│   ├── UserActivity.cs               # User activity logging entity
│   └── OrderStatus.cs                # Order status enumeration
├── Services/                         # Application services
│   ├── DatabaseInitializationService.cs  # Database setup and script execution
│   └── SampleOperationsService.cs    # Comprehensive operation demonstrations
├── Scripts/                          # SQL database scripts
│   ├── 01_CreateDatabase.sql         # Database creation
│   ├── 02_CreateSchemas.sql          # Schema creation (sales, inventory, users)
│   ├── 03_CreateTables.sql           # Table creation with constraints and indexes
│   ├── 04_InsertSampleData.sql       # Sample data insertion
│   └── DatabaseHelper.sql            # Utility procedures and functions
├── Program.cs                        # Main application entry point
├── appsettings.json                  # Configuration including connection strings
├── FractalDataWorks.Samples.MsSqlProvider.csproj  # Project file
└── README.md                         # This file
```

## Configuration

The application uses `appsettings.json` for configuration:

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=SampleDb;Integrated Security=true;Trust Server Certificate=true;Connection Timeout=30;"
  }
}
```

### Data Provider Settings

```json
{
  "DataProvider": {
    "Provider": "MsSql",
    "CommandTimeout": 30,
    "BulkCopyTimeout": 300,
    "RetryAttempts": 3,
    "RetryDelayMs": 1000,
    "SchemaValidation": true,
    "Schemas": {
      "Sales": "sales",
      "Inventory": "inventory", 
      "Users": "users"
    }
  }
}
```

### Sample Operation Configuration

```json
{
  "SampleOperations": {
    "EnableTransactionDemos": true,
    "EnableBulkOperations": true,
    "EnableComplexQueries": true,
    "CustomerTestCount": 100,
    "OrderTestCount": 500,
    "ProductTestCount": 50,
    "CategoryTestCount": 10,
    "UserActivityTestCount": 1000
  }
}
```

## Demonstrated Features

### 1. Basic CRUD Operations

- **Create**: Insert single entities with proper relationship handling
- **Read**: Query with filters, sorting, paging, and navigation properties
- **Update**: Single entity updates with optimistic concurrency control
- **Delete**: Safe deletion with foreign key constraint handling

### 2. Advanced Querying

- **Complex WHERE clauses** with multiple conditions and operators
- **Joins and navigation** using Entity Framework-style includes
- **Aggregation operations** (Count, Sum, Average, Min, Max)
- **Paging and sorting** with Skip/Take and OrderBy operations
- **String operations** (StartsWith, Contains, EndsWith)
- **Date range queries** with DateTime comparisons
- **Null checks** and conditional logic

### 3. Transaction Management

- **Successful transactions** with multiple related operations
- **Rollback scenarios** demonstrating error handling
- **Isolation levels** and concurrency control
- **Nested transactions** and savepoint management

### 4. Bulk Operations

- **Bulk insert** operations for performance optimization
- **Bulk update** scenarios with batch processing
- **Bulk delete** with proper constraint handling
- **Performance comparison** between single and bulk operations

### 5. Complex Query Scenarios

- **Subquery operations** using LINQ-style expressions
- **Analytical queries** with grouping and aggregation
- **Hierarchical queries** for category tree navigation
- **Window functions** and advanced SQL features

### 6. Performance Monitoring

- **Query performance** measurement and optimization
- **Connection pooling** behavior demonstration
- **User activity logging** for analytics
- **Memory usage** and resource management

## Sample Code Examples

### Type-Safe Entity Creation

```csharp
// Create a new customer with type safety
var customer = new Customer
{
    Name = "John Doe",
    Email = "john.doe@example.com", 
    CreditLimit = 5000.00m,
    IsActive = true
};

var insertCommand = _dataService.CreateInsertCommand<Customer>();
var result = await insertCommand.ExecuteAsync(customer, cancellationToken);
```

### Complex Query Operations

```csharp
// Complex query with joins and filtering
var complexQuery = _dataService.CreateQueryCommand<Order>()
    .Where(o => o.Customer.IsActive && 
                o.TotalAmount > 100 && 
                o.OrderDate >= DateTime.UtcNow.AddDays(-30))
    .OrderByDescending(o => o.OrderDate)
    .Take(10);

var recentOrders = await complexQuery.ExecuteAsync(cancellationToken);
```

### Transaction Management

```csharp
// Transaction with multiple operations
using var transaction = await _dataService.BeginTransactionAsync(cancellationToken);

try
{
    // Create customer
    await insertCustomerCommand.ExecuteAsync(customer, cancellationToken);
    
    // Create order for customer
    await insertOrderCommand.ExecuteAsync(order, cancellationToken);
    
    // Log activity
    await insertActivityCommand.ExecuteAsync(activity, cancellationToken);
    
    await transaction.CommitAsync(cancellationToken);
}
catch (Exception)
{
    await transaction.RollbackAsync(cancellationToken);
    throw;
}
```

### Analytical Queries

```csharp
// Customer analytics with grouping
var customerAnalytics = customers
    .GroupBy(c => c.IsActive)
    .Select(g => new
    {
        IsActive = g.Key,
        Count = g.Count(),
        AverageCreditLimit = g.Average(c => c.CreditLimit),
        TotalCreditLimit = g.Sum(c => c.CreditLimit)
    });
```

## Troubleshooting

### Common Issues

#### 1. LocalDB Not Found

**Error**: `Cannot connect to LocalDB`

**Solutions**:
- Install SQL Server Express LocalDB
- Verify installation: `sqllocaldb info`
- Start LocalDB instance: `sqllocaldb start mssqllocaldb`

#### 2. Permission Issues

**Error**: `Access denied` or `Login failed`

**Solutions**:
- Run command prompt as Administrator
- Check Windows Authentication configuration
- Verify user account has LocalDB access

#### 3. Database Creation Fails

**Error**: `Cannot create database`

**Solutions**:
- Check disk space availability
- Verify LocalDB service is running
- Check connection string format
- Ensure database doesn't already exist

#### 4. Package Reference Errors

**Error**: `Could not load assembly` or `Package not found`

**Solutions**:
```bash
# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

### Logging Configuration

For detailed troubleshooting, increase logging verbosity:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "FractalDataWorks": "Trace",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Database Inspection

Connect to the database using SQL Server Management Studio (SSMS) or Azure Data Studio:

**Connection String**: `(localdb)\MSSQLLocalDB`
**Database**: `SampleDb`

## Learning Resources

### FractalDataWorks Documentation
- [Data Provider Architecture](../../docs/DataProviders.md)
- [Query Expression Guide](../../docs/QueryExpressions.md)
- [Transaction Management](../../docs/Transactions.md)
- [Performance Optimization](../../docs/Performance.md)

### Related Technologies
- [SQL Server LocalDB Documentation](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [.NET Dependency Injection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)

## Next Steps

After exploring this sample:

1. **Customize the schema** to match your domain requirements
2. **Implement additional entities** with complex relationships
3. **Add custom query operations** for specific business logic
4. **Integrate with your application** using the same patterns
5. **Explore other data providers** (PostgreSQL, MySQL, etc.)
6. **Implement caching strategies** for performance optimization
7. **Add monitoring and metrics** for production usage

## Support

For questions or issues:

1. **Check the documentation** in the `docs/` directory
2. **Review the source code** for implementation details
3. **Create an issue** in the project repository
4. **Contact the development team** for enterprise support

---

**Happy coding with FractalDataWorks!** 🚀