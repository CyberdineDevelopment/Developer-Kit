using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Services.DataProvider.MsSql.Commands;
using FractalDataWorks.Services.DataProvider.MsSql.Configuration;
using FractalDataWorks.Services.DataProvider.MsSql.Services;

namespace FractalDataWorks.Services.DataProvider.MsSql.Samples;

/// <summary>
/// Comprehensive samples demonstrating how to use the MsSql data provider for various database operations.
/// This file contains practical examples of SELECT, INSERT, UPDATE, DELETE, UPSERT, and BULK operations
/// using realistic entity examples like Customer, Order, and Product.
/// </summary>
public static class MsSqlDataProviderSamples
{
    #region Sample Entities

    /// <summary>
    /// Sample Customer entity with common properties including versioning for optimistic concurrency.
    /// </summary>
    public sealed class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public byte[] Version { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// Sample Order entity demonstrating relationships and enum properties.
    /// </summary>
    public sealed class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Sample Product entity with pricing and inventory management.
    /// </summary>
    public sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public bool InStock { get; set; }
        public int StockQuantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Sample order status enumeration.
    /// </summary>
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }

    #endregion

    #region Configuration Setup Examples

    /// <summary>
    /// Example 1: Basic configuration setup with connection string.
    /// This is the most straightforward way to configure the SQL Server provider.
    /// </summary>
    public static MsSqlConfiguration CreateBasicConfiguration()
    {
        return new MsSqlConfiguration
        {
            ConnectionString = "Server=localhost;Database=SampleDB;Integrated Security=true;TrustServerCertificate=true;",
            CommandTimeoutSeconds = 30,
            ConnectionTimeoutSeconds = 30,
            EnableConnectionPooling = true,
            MaxPoolSize = 100
        };
    }

    /// <summary>
    /// Example 2: Advanced configuration with individual properties and retry policies.
    /// This approach gives you more control over connection settings.
    /// </summary>
    public static MsSqlConfiguration CreateAdvancedConfiguration()
    {
        return new MsSqlConfiguration
        {
            ServerName = "localhost",
            DatabaseName = "SampleDB",
            UseWindowsAuthentication = true,
            EncryptConnection = true,
            TrustServerCertificate = true,
            CommandTimeoutSeconds = 60,
            ConnectionTimeoutSeconds = 30,
            EnableConnectionPooling = true,
            MaxPoolSize = 50,
            EnableAutoRetry = true,
            ApplicationName = "MsSqlDataProviderSample",
            DefaultSchema = "dbo",
            SchemaMapping = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["CustomerSchema"] = "Sales",
                ["ProductSchema"] = "Inventory"
            }
        };
    }

    /// <summary>
    /// Example 3: Configuration for SQL Server Authentication (non-Windows).
    /// </summary>
    public static MsSqlConfiguration CreateSqlAuthConfiguration(string username, string password)
    {
        return new MsSqlConfiguration
        {
            ServerName = "sql-server.example.com",
            DatabaseName = "ProductionDB",
            UseWindowsAuthentication = false,
            Username = username,
            Password = password,
            EncryptConnection = true,
            TrustServerCertificate = false,
            Port = 1433,
            CommandTimeoutSeconds = 120,
            EnableConnectionPooling = true,
            MaxPoolSize = 200
        };
    }

    #endregion

    #region Service Registration Examples

    /// <summary>
    /// Example: Register MsSql data provider with dependency injection.
    /// </summary>
    public static void RegisterServices(IServiceCollection services)
    {
        // Register the configuration
        var configuration = CreateBasicConfiguration();
        services.AddSingleton(configuration);

        // Register logging
        services.AddLogging(builder => builder.AddConsole());

        // Register the MsSql data provider
        services.AddSingleton<MsSqlDataProvider>();

        // Alternative: Use the extension method (if available)
        // services.AddMsSqlDataProvider(configuration);
    }

    #endregion

    #region SELECT Operations Examples

    /// <summary>
    /// Example 1: Find a single customer by ID.
    /// Demonstrates basic entity retrieval with parameters.
    /// </summary>
    public static async Task<Customer?> FindCustomerByIdAsync(MsSqlDataProvider dataProvider, int customerId)
    {
        try
        {
            // Create a query command using the factory method
            var command = MsSqlQueryCommandFactory.FindById<Customer>("Customers", customerId);
            
            // Execute the command
            var result = await dataProvider.Execute<Customer>(command, CancellationToken.None);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"Successfully found customer: {result.Value?.Name}");
                return result.Value;
            }
            
            Console.WriteLine($"Failed to find customer: {result.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding customer: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2: Find multiple customers with filtering and ordering.
    /// Demonstrates parameterized queries with WHERE clauses.
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindActiveCustomersAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            // Create a custom WHERE query
            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["MinDate"] = DateTime.UtcNow.AddYears(-1)
            };

            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
                "Customers",
                "CreatedDate >= @MinDate AND Email IS NOT NULL",
                parameters,
                orderBy: "Name ASC"
            );
            
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);
            
            if (result.IsSuccess && result.Value != null)
            {
                var customers = result.Value.ToList();
                Console.WriteLine($"Found {customers.Count} active customers");
                return customers;
            }
            
            Console.WriteLine($"Failed to find customers: {result.Message}");
            return Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding customers: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 3: Custom SQL query with complex joins and projections.
    /// Demonstrates advanced querying with custom SQL.
    /// </summary>
    public static async Task<IEnumerable<object>> GetCustomerOrderSummaryAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var sql = """
                SELECT 
                    c.Id,
                    c.Name,
                    c.Email,
                    COUNT(o.Id) as TotalOrders,
                    ISNULL(SUM(o.TotalAmount), 0) as TotalSpent,
                    MAX(o.OrderDate) as LastOrderDate
                FROM Customers c
                LEFT JOIN Orders o ON c.Id = o.CustomerId
                WHERE c.CreatedDate >= @StartDate
                GROUP BY c.Id, c.Name, c.Email
                ORDER BY TotalSpent DESC
                """;

            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["StartDate"] = DateTime.UtcNow.AddMonths(-6)
            };

            var command = new MsSqlQueryCommand<IEnumerable<object>>(sql, "CustomerOrderSummary", parameters);
            var result = await dataProvider.Execute<IEnumerable<object>>(command, CancellationToken.None);
            
            if (result.IsSuccess && result.Value != null)
            {
                Console.WriteLine("Successfully retrieved customer order summary");
                return result.Value;
            }
            
            Console.WriteLine($"Failed to get summary: {result.Message}");
            return Enumerable.Empty<object>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting customer summary: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region INSERT Operations Examples

    /// <summary>
    /// Example 1: Insert a single customer and return the complete entity.
    /// Demonstrates basic entity insertion with OUTPUT clause.
    /// </summary>
    public static async Task<Customer?> CreateCustomerAsync(MsSqlDataProvider dataProvider, Customer customer)
    {
        try
        {
            // Set the creation date
            customer.CreatedDate = DateTime.UtcNow;
            
            // Create an insert command that returns the inserted entity
            var command = MsSqlInsertCommandFactory.Insert<Customer>("Customers", customer);
            
            var result = await dataProvider.Execute<Customer>(command, CancellationToken.None);
            
            if (result.IsSuccess && result.Value != null)
            {
                Console.WriteLine($"Successfully created customer with ID: {result.Value.Id}");
                return result.Value;
            }
            
            Console.WriteLine($"Failed to create customer: {result.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating customer: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2: Insert a customer and return only the generated ID.
    /// More efficient when you only need the ID after insertion.
    /// </summary>
    public static async Task<int?> CreateCustomerReturnIdAsync(MsSqlDataProvider dataProvider, Customer customer)
    {
        try
        {
            customer.CreatedDate = DateTime.UtcNow;
            
            // Create an insert command that returns only the ID
            var command = MsSqlInsertCommandFactory.InsertReturnId<Customer, int>("Customers", customer);
            
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"Successfully created customer with ID: {result.Value}");
                return result.Value;
            }
            
            Console.WriteLine($"Failed to create customer: {result.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating customer: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 3: Insert with explicit column values.
    /// Useful when you don't have a full entity object.
    /// </summary>
    public static async Task<int?> CreateQuickCustomerAsync(MsSqlDataProvider dataProvider, string name, string email)
    {
        try
        {
            var columnValues = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Name"] = name,
                ["Email"] = email,
                ["CreatedDate"] = DateTime.UtcNow
            };

            var command = MsSqlInsertCommandFactory.InsertValues<int>(
                "Customers", 
                columnValues,
                returnClause: "OUTPUT INSERTED.Id"
            );
            
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"Successfully created customer with ID: {result.Value}");
                return result.Value;
            }
            
            Console.WriteLine($"Failed to create customer: {result.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating customer: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region UPDATE Operations Examples

    /// <summary>
    /// Example 1: Update a customer by ID.
    /// Demonstrates basic entity updates with WHERE clause.
    /// </summary>
    public static async Task<bool> UpdateCustomerEmailAsync(MsSqlDataProvider dataProvider, int customerId, string newEmail)
    {
        try
        {
            var sql = "UPDATE Customers SET Email = @Email, LastUpdated = @LastUpdated WHERE Id = @Id";
            
            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Id"] = customerId,
                ["Email"] = newEmail,
                ["LastUpdated"] = DateTime.UtcNow
            };

            var command = new MsSqlUpdateCommand<int>(sql, "Customers", parameters);
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);
            
            if (result.IsSuccess && result.Value > 0)
            {
                Console.WriteLine($"Successfully updated customer {customerId}. Rows affected: {result.Value}");
                return true;
            }
            
            Console.WriteLine($"Failed to update customer or no rows affected: {result.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating customer: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2: Update with optimistic concurrency using version column.
    /// Demonstrates concurrency control to prevent lost updates.
    /// </summary>
    public static async Task<bool> UpdateCustomerWithConcurrencyAsync(MsSqlDataProvider dataProvider, Customer customer)
    {
        try
        {
            var sql = """
                UPDATE Customers 
                SET Name = @Name, 
                    Email = @Email,
                    Version = Version + 1
                WHERE Id = @Id AND Version = @Version
                """;
            
            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Id"] = customer.Id,
                ["Name"] = customer.Name,
                ["Email"] = customer.Email,
                ["Version"] = customer.Version
            };

            var command = new MsSqlUpdateCommand<int>(sql, "Customers", parameters);
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);
            
            if (result.IsSuccess && result.Value > 0)
            {
                Console.WriteLine($"Successfully updated customer {customer.Id} with concurrency check");
                return true;
            }
            else if (result.IsSuccess && result.Value == 0)
            {
                Console.WriteLine($"Concurrency conflict: Customer {customer.Id} was modified by another user");
                return false;
            }
            
            Console.WriteLine($"Failed to update customer: {result.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating customer with concurrency: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 3: Bulk update with WHERE clause.
    /// Updates multiple records that match certain criteria.
    /// </summary>
    public static async Task<int> ActivateRecentCustomersAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var sql = """
                UPDATE Customers 
                SET Status = 'Active', 
                    LastUpdated = @LastUpdated
                WHERE CreatedDate >= @StartDate 
                  AND (Status IS NULL OR Status = 'Pending')
                """;
            
            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["StartDate"] = DateTime.UtcNow.AddDays(-30),
                ["LastUpdated"] = DateTime.UtcNow
            };

            var command = new MsSqlUpdateCommand<int>(sql, "Customers", parameters);
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"Successfully activated {result.Value} customers");
                return result.Value;
            }
            
            Console.WriteLine($"Failed to activate customers: {result.Message}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error activating customers: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region DELETE Operations Examples

    /// <summary>
    /// Example 1: Delete a customer by ID.
    /// Demonstrates basic entity deletion.
    /// </summary>
    public static async Task<bool> DeleteCustomerAsync(MsSqlDataProvider dataProvider, int customerId)
    {
        try
        {
            var sql = "DELETE FROM Customers WHERE Id = @Id";
            
            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Id"] = customerId
            };

            var command = new MsSqlDeleteCommand<int>(sql, "Customers", parameters);
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);
            
            if (result.IsSuccess && result.Value > 0)
            {
                Console.WriteLine($"Successfully deleted customer {customerId}");
                return true;
            }
            else if (result.IsSuccess && result.Value == 0)
            {
                Console.WriteLine($"No customer found with ID {customerId}");
                return false;
            }
            
            Console.WriteLine($"Failed to delete customer: {result.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting customer: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2: Soft delete with WHERE clause and date criteria.
    /// Marks records as deleted instead of physically removing them.
    /// </summary>
    public static async Task<int> SoftDeleteInactiveCustomersAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var sql = """
                UPDATE Customers 
                SET IsDeleted = 1, 
                    DeletedDate = @DeletedDate
                WHERE CreatedDate < @CutoffDate 
                  AND IsDeleted = 0
                  AND NOT EXISTS (SELECT 1 FROM Orders WHERE CustomerId = Customers.Id)
                """;
            
            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["CutoffDate"] = DateTime.UtcNow.AddYears(-2),
                ["DeletedDate"] = DateTime.UtcNow
            };

            var command = new MsSqlUpdateCommand<int>(sql, "Customers", parameters);
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"Successfully soft-deleted {result.Value} inactive customers");
                return result.Value;
            }
            
            Console.WriteLine($"Failed to soft delete customers: {result.Message}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error soft deleting customers: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 3: Cascading delete with related records.
    /// Deletes related records in proper order to maintain referential integrity.
    /// </summary>
    public static async Task<bool> DeleteCustomerWithOrdersAsync(MsSqlDataProvider dataProvider, int customerId)
    {
        // This should be done within a transaction (see transaction examples)
        try
        {
            // First delete related orders
            var deleteOrdersSql = "DELETE FROM Orders WHERE CustomerId = @CustomerId";
            var deleteOrdersParams = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["CustomerId"] = customerId
            };

            var deleteOrdersCommand = new MsSqlDeleteCommand<int>(deleteOrdersSql, "Orders", deleteOrdersParams);
            var ordersResult = await dataProvider.Execute<int>(deleteOrdersCommand, CancellationToken.None);
            
            if (!ordersResult.IsSuccess)
            {
                Console.WriteLine($"Failed to delete orders: {ordersResult.Message}");
                return false;
            }

            // Then delete the customer
            var deleteCustomerResult = await DeleteCustomerAsync(dataProvider, customerId);
            
            if (deleteCustomerResult)
            {
                Console.WriteLine($"Successfully deleted customer {customerId} and {ordersResult.Value} related orders");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting customer with orders: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region BULK INSERT Operations Examples

    /// <summary>
    /// Example 1: Bulk insert multiple customers.
    /// Efficient way to insert large numbers of records.
    /// </summary>
    public static async Task<int> BulkInsertCustomersAsync(MsSqlDataProvider dataProvider, IEnumerable<Customer> customers)
    {
        try
        {
            var customerList = customers.ToList();
            if (customerList.Count == 0)
            {
                Console.WriteLine("No customers to insert");
                return 0;
            }

            // Set creation dates
            foreach (var customer in customerList)
            {
                customer.CreatedDate = DateTime.UtcNow;
            }

            var command = MsSqlInsertCommandFactory.BulkInsert("Customers", customerList);
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"Successfully inserted {customerList.Count} customers");
                return result.Value;
            }
            
            Console.WriteLine($"Failed to bulk insert customers: {result.Message}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error bulk inserting customers: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2: Bulk insert with chunking for large datasets.
    /// Splits large datasets into smaller chunks to avoid timeout issues.
    /// </summary>
    public static async Task<int> BulkInsertCustomersChunkedAsync(MsSqlDataProvider dataProvider, IEnumerable<Customer> customers, int chunkSize = 1000)
    {
        try
        {
            var customerList = customers.ToList();
            var totalInserted = 0;

            for (int i = 0; i < customerList.Count; i += chunkSize)
            {
                var chunk = customerList.Skip(i).Take(chunkSize);
                var inserted = await BulkInsertCustomersAsync(dataProvider, chunk);
                totalInserted += inserted;
                
                Console.WriteLine($"Processed chunk {(i / chunkSize) + 1}: {inserted} records inserted");
            }

            Console.WriteLine($"Total bulk insert completed: {totalInserted} customers inserted");
            return totalInserted;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in chunked bulk insert: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region UPSERT Operations Examples

    /// <summary>
    /// Example 1: Upsert customer using MERGE statement.
    /// Updates existing records or inserts new ones based on key matching.
    /// </summary>
    public static async Task<int> UpsertCustomerAsync(MsSqlDataProvider dataProvider, Customer customer)
    {
        try
        {
            var sql = """
                MERGE Customers AS target
                USING (SELECT @Id as Id, @Name as Name, @Email as Email, @CreatedDate as CreatedDate) AS source
                ON target.Id = source.Id
                WHEN MATCHED THEN
                    UPDATE SET Name = source.Name, Email = source.Email
                WHEN NOT MATCHED THEN
                    INSERT (Name, Email, CreatedDate)
                    VALUES (source.Name, source.Email, source.CreatedDate)
                OUTPUT $action;
                """;

            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Id"] = customer.Id,
                ["Name"] = customer.Name,
                ["Email"] = customer.Email,
                ["CreatedDate"] = customer.CreatedDate == default ? DateTime.UtcNow : customer.CreatedDate
            };

            var command = new MsSqlUpsertCommand<int>(sql, "Customers", parameters);
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"Successfully upserted customer. Operation result: {result.Value}");
                return result.Value;
            }
            
            Console.WriteLine($"Failed to upsert customer: {result.Message}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error upserting customer: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2: Upsert customer by email (natural key).
    /// Uses email as the matching key instead of ID.
    /// </summary>
    public static async Task<Customer?> UpsertCustomerByEmailAsync(MsSqlDataProvider dataProvider, Customer customer)
    {
        try
        {
            var sql = """
                MERGE Customers AS target
                USING (SELECT @Name as Name, @Email as Email) AS source
                ON target.Email = source.Email
                WHEN MATCHED THEN
                    UPDATE SET Name = source.Name
                WHEN NOT MATCHED THEN
                    INSERT (Name, Email, CreatedDate)
                    VALUES (source.Name, source.Email, @CreatedDate)
                OUTPUT INSERTED.*;
                """;

            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Name"] = customer.Name,
                ["Email"] = customer.Email,
                ["CreatedDate"] = DateTime.UtcNow
            };

            var command = new MsSqlUpsertCommand<Customer>(sql, "Customers", parameters);
            var result = await dataProvider.Execute<Customer>(command, CancellationToken.None);
            
            if (result.IsSuccess && result.Value != null)
            {
                Console.WriteLine($"Successfully upserted customer: {result.Value.Name} (ID: {result.Value.Id})");
                return result.Value;
            }
            
            Console.WriteLine($"Failed to upsert customer: {result.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error upserting customer by email: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Transaction Usage Examples

    /// <summary>
    /// Example 1: Simple transaction with commit/rollback.
    /// Demonstrates basic transaction usage for atomic operations.
    /// </summary>
    public static async Task<bool> CreateCustomerAndOrderTransactionAsync(MsSqlDataProvider dataProvider, Customer customer, Order order)
    {
        // Begin transaction
        var transactionResult = await dataProvider.BeginTransactionAsync(FdwTransactionIsolationLevel.ReadCommitted);
        if (transactionResult.Error || transactionResult.Value == null)
        {
            Console.WriteLine($"Failed to begin transaction: {transactionResult.Message}");
            return false;
        }

        var transaction = transactionResult.Value;
        
        try
        {
            // Insert customer first
            customer.CreatedDate = DateTime.UtcNow;
            var customerCommand = MsSqlInsertCommandFactory.InsertReturnId<Customer, int>("Customers", customer);
            var customerResult = await dataProvider.Execute<int>(customerCommand, CancellationToken.None);
            
            if (customerResult.Error)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Failed to insert customer, rolling back: {customerResult.Message}");
                return false;
            }

            // Set the customer ID in the order and insert
            order.CustomerId = customerResult.Value;
            order.OrderDate = DateTime.UtcNow;
            var orderCommand = MsSqlInsertCommandFactory.Insert<Order>("Orders", order);
            var orderResult = await dataProvider.Execute<Order>(orderCommand, CancellationToken.None);
            
            if (orderResult.Error)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Failed to insert order, rolling back: {orderResult.Message}");
                return false;
            }

            // Commit transaction
            var commitResult = await transaction.CommitAsync();
            if (commitResult.IsSuccess)
            {
                Console.WriteLine($"Successfully created customer {customerResult.Value} and order {orderResult.Value?.Id} in transaction");
                return true;
            }
            
            Console.WriteLine($"Failed to commit transaction: {commitResult.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error in transaction, rolled back: {ex.Message}");
            throw;
        }
        finally
        {
            transaction.Dispose();
        }
    }

    /// <summary>
    /// Example 2: Complex transaction with multiple operations and validation.
    /// Shows more sophisticated transaction handling with business logic.
    /// </summary>
    public static async Task<bool> ProcessBulkOrderTransactionAsync(MsSqlDataProvider dataProvider, int customerId, IEnumerable<Order> orders)
    {
        var transactionResult = await dataProvider.BeginTransactionAsync(FdwTransactionIsolationLevel.Serializable, TimeSpan.FromMinutes(5));
        if (transactionResult.Error || transactionResult.Value == null)
        {
            Console.WriteLine($"Failed to begin transaction: {transactionResult.Message}");
            return false;
        }

        var transaction = transactionResult.Value;
        
        try
        {
            // Validate customer exists
            var customerCheck = await FindCustomerByIdAsync(dataProvider, customerId);
            if (customerCheck == null)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("Customer not found, transaction rolled back");
                return false;
            }

            var orderList = orders.ToList();
            var totalAmount = orderList.Sum(o => o.TotalAmount);
            
            // Business rule: Check if total amount exceeds customer credit limit
            if (totalAmount > 10000) // Arbitrary business rule
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Total amount ${totalAmount} exceeds credit limit, transaction rolled back");
                return false;
            }

            // Insert all orders
            var insertedOrders = 0;
            foreach (var order in orderList)
            {
                order.CustomerId = customerId;
                order.OrderDate = DateTime.UtcNow;
                order.Status = OrderStatus.Pending;

                var orderCommand = MsSqlInsertCommandFactory.InsertReturnId<Order, int>("Orders", order);
                var orderResult = await dataProvider.Execute<int>(orderCommand, CancellationToken.None);
                
                if (orderResult.Error)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Failed to insert order {insertedOrders + 1}, rolling back transaction: {orderResult.Message}");
                    return false;
                }
                
                insertedOrders++;
            }

            // Update customer last activity
            var updateCustomerSql = "UPDATE Customers SET LastOrderDate = @LastOrderDate WHERE Id = @Id";
            var updateParams = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Id"] = customerId,
                ["LastOrderDate"] = DateTime.UtcNow
            };
            var updateCommand = new MsSqlUpdateCommand<int>(updateCustomerSql, "Customers", updateParams);
            var updateResult = await dataProvider.Execute<int>(updateCommand, CancellationToken.None);
            
            if (updateResult.Error)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Failed to update customer last activity, rolling back: {updateResult.Message}");
                return false;
            }

            // Commit transaction
            var commitResult = await transaction.CommitAsync();
            if (commitResult.IsSuccess)
            {
                Console.WriteLine($"Successfully processed {insertedOrders} orders for customer {customerId} in transaction");
                return true;
            }
            
            Console.WriteLine($"Failed to commit transaction: {commitResult.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error in bulk order transaction, rolled back: {ex.Message}");
            throw;
        }
        finally
        {
            transaction.Dispose();
        }
    }

    #endregion

    #region Error Handling Patterns

    /// <summary>
    /// Example 1: Comprehensive error handling with retry logic.
    /// Shows how to handle different types of database errors appropriately.
    /// </summary>
    public static async Task<Customer?> GetCustomerWithRetryAsync(MsSqlDataProvider dataProvider, int customerId, int maxRetries = 3)
    {
        var attempt = 0;
        
        while (attempt <= maxRetries)
        {
            try
            {
                var command = MsSqlQueryCommandFactory.FindById<Customer>("Customers", customerId);
                var result = await dataProvider.Execute<Customer>(command, CancellationToken.None);
                
                if (result.IsSuccess)
                {
                    return result.Value;
                }
                
                // Check if this is a transient error that should be retried
                if (IsTransientError(result.Message) && attempt < maxRetries)
                {
                    attempt++;
                    var delayMs = CalculateRetryDelay(attempt);
                    Console.WriteLine($"Transient error detected, retrying in {delayMs}ms (attempt {attempt}/{maxRetries})");
                    await Task.Delay(delayMs);
                    continue;
                }
                
                Console.WriteLine($"Non-transient error or max retries reached: {result.Message}");
                return null;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                if (IsTransientException(ex) && attempt < maxRetries)
                {
                    attempt++;
                    var delayMs = CalculateRetryDelay(attempt);
                    Console.WriteLine($"Transient exception detected, retrying in {delayMs}ms (attempt {attempt}/{maxRetries}): {ex.Message}");
                    await Task.Delay(delayMs);
                    continue;
                }
                
                Console.WriteLine($"Non-transient exception or max retries reached: {ex.Message}");
                throw;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Example 2: Error handling with detailed logging and metrics.
    /// Demonstrates comprehensive error handling for production scenarios.
    /// </summary>
    public static async Task<bool> CreateCustomerWithDetailedErrorHandlingAsync(MsSqlDataProvider dataProvider, Customer customer)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Validate input
            if (customer == null)
            {
                Console.WriteLine("Error: Customer object is null");
                return false;
            }

            if (string.IsNullOrWhiteSpace(customer.Name))
            {
                Console.WriteLine("Error: Customer name is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(customer.Email))
            {
                Console.WriteLine("Error: Customer email is required");
                return false;
            }

            // Test connection first
            var connectionTest = await dataProvider.TestConnectionAsync();
            if (connectionTest.Error)
            {
                Console.WriteLine($"Database connection failed: {connectionTest.Message}");
                return false;
            }

            // Attempt the operation
            customer.CreatedDate = DateTime.UtcNow;
            var command = MsSqlInsertCommandFactory.Insert<Customer>("Customers", customer);
            var result = await dataProvider.Execute<Customer>(command, CancellationToken.None);
            
            stopwatch.Stop();
            
            if (result.IsSuccess && result.Value != null)
            {
                Console.WriteLine($"Successfully created customer {result.Value.Id} in {stopwatch.ElapsedMilliseconds}ms");
                LogMetrics("customer_create_success", stopwatch.ElapsedMilliseconds);
                return true;
            }
            
            Console.WriteLine($"Failed to create customer: {result.Message} (Duration: {stopwatch.ElapsedMilliseconds}ms)");
            LogMetrics("customer_create_failure", stopwatch.ElapsedMilliseconds);
            return false;
        }
        catch (TimeoutException ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"Operation timed out after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            LogMetrics("customer_create_timeout", stopwatch.ElapsedMilliseconds);
            return false;
        }
        catch (InvalidOperationException ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"Invalid operation: {ex.Message}");
            LogMetrics("customer_create_invalid", stopwatch.ElapsedMilliseconds);
            return false;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"Unexpected error creating customer: {ex.Message} (Duration: {stopwatch.ElapsedMilliseconds}ms)");
            LogMetrics("customer_create_error", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Determines if an error message indicates a transient failure.
    /// </summary>
    private static bool IsTransientError(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return false;

        var transientKeywords = new[]
        {
            "timeout",
            "deadlock",
            "connection",
            "network",
            "transport",
            "throttling"
        };

        return transientKeywords.Any(keyword => 
            errorMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines if an exception represents a transient failure.
    /// </summary>
    private static bool IsTransientException(Exception ex)
    {
        return ex is TimeoutException ||
               ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Calculates exponential backoff delay for retry attempts.
    /// </summary>
    private static int CalculateRetryDelay(int attempt)
    {
        var baseDelay = 1000; // 1 second
        var maxDelay = 30000; // 30 seconds
        var delay = Math.Min(baseDelay * Math.Pow(2, attempt - 1), maxDelay);
        
        // Add some jitter to prevent thundering herd
        var random = new Random();
        var jitter = random.Next(0, (int)(delay * 0.1));
        
        return (int)delay + jitter;
    }

    /// <summary>
    /// Logs performance metrics (placeholder for actual metrics implementation).
    /// </summary>
    private static void LogMetrics(string metricName, long durationMs)
    {
        Console.WriteLine($"[METRIC] {metricName}: {durationMs}ms");
        // In a real implementation, you would send this to your metrics system
        // e.g., Prometheus, Application Insights, CloudWatch, etc.
    }

    #endregion

    #region Complete Usage Example

    /// <summary>
    /// Complete example showing typical application usage patterns.
    /// Demonstrates a realistic scenario with proper error handling and resource management.
    /// </summary>
    public static async Task<bool> RunCompleteExampleAsync()
    {
        // Setup configuration
        var configuration = CreateAdvancedConfiguration();
        
        // Create service provider and register services
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddSingleton<MsSqlDataProvider>();
        
        var serviceProvider = services.BuildServiceProvider();
        var dataProvider = serviceProvider.GetRequiredService<MsSqlDataProvider>();
        
        try
        {
            Console.WriteLine("=== MsSql Data Provider Complete Example ===");
            
            // Test connection
            Console.WriteLine("\n1. Testing database connection...");
            var connectionTest = await dataProvider.TestConnectionAsync();
            if (connectionTest.Error)
            {
                Console.WriteLine($"Connection test failed: {connectionTest.Message}");
                return false;
            }
            Console.WriteLine("Connection test successful!");

            // Create sample customers
            Console.WriteLine("\n2. Creating sample customers...");
            var customers = new List<Customer>
            {
                new Customer { Name = "John Doe", Email = "john.doe@example.com" },
                new Customer { Name = "Jane Smith", Email = "jane.smith@example.com" },
                new Customer { Name = "Bob Johnson", Email = "bob.johnson@example.com" }
            };

            var createdCustomers = new List<Customer>();
            foreach (var customer in customers)
            {
                var created = await CreateCustomerAsync(dataProvider, customer);
                if (created != null)
                {
                    createdCustomers.Add(created);
                }
            }

            Console.WriteLine($"Created {createdCustomers.Count} customers");

            // Query customers
            Console.WriteLine("\n3. Querying active customers...");
            var activeCustomers = await FindActiveCustomersAsync(dataProvider);
            Console.WriteLine($"Found {activeCustomers.Count()} active customers");

            // Create orders for customers
            Console.WriteLine("\n4. Creating orders...");
            var orders = createdCustomers.Select(c => new Order
            {
                CustomerId = c.Id,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 100.50m * (c.Id % 5 + 1), // Vary amounts
                Status = OrderStatus.Pending,
                Notes = $"Sample order for customer {c.Name}"
            }).ToList();

            var orderCreationTasks = orders.Select(o => 
                MsSqlInsertCommandFactory.Insert<Order>("Orders", o))
                .Select(cmd => dataProvider.Execute<Order>(cmd, CancellationToken.None));
            
            var orderResults = await Task.WhenAll(orderCreationTasks);
            var successfulOrders = orderResults.Count(r => r.IsSuccess);
            Console.WriteLine($"Created {successfulOrders} orders");

            // Demonstrate transaction usage
            Console.WriteLine("\n5. Demonstrating transaction usage...");
            var newCustomer = new Customer { Name = "Alice Brown", Email = "alice.brown@example.com" };
            var newOrder = new Order { TotalAmount = 250.75m, Status = OrderStatus.Pending };
            
            var transactionResult = await CreateCustomerAndOrderTransactionAsync(dataProvider, newCustomer, newOrder);
            Console.WriteLine($"Transaction example completed: {(transactionResult ? "Success" : "Failed")}");

            // Update example
            Console.WriteLine("\n6. Updating customer email...");
            if (createdCustomers.Count > 0)
            {
                var customerToUpdate = createdCustomers[0];
                var updateResult = await UpdateCustomerEmailAsync(dataProvider, customerToUpdate.Id, "updated.email@example.com");
                Console.WriteLine($"Customer update: {(updateResult ? "Success" : "Failed")}");
            }

            // Query with joins
            Console.WriteLine("\n7. Getting customer order summary...");
            var summary = await GetCustomerOrderSummaryAsync(dataProvider);
            Console.WriteLine($"Retrieved summary for {summary.Count()} customers");

            Console.WriteLine("\n=== Complete example finished successfully! ===");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Complete example failed with error: {ex.Message}");
            return false;
        }
        finally
        {
            // Dispose resources
            dataProvider?.Dispose();
            serviceProvider?.Dispose();
        }
    }

    #endregion
}