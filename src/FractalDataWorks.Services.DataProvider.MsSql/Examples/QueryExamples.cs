using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.DataProvider.MsSql.Commands;
using FractalDataWorks.Services.DataProvider.MsSql.Services;

namespace FractalDataWorks.Services.DataProvider.MsSql.Examples;

/// <summary>
/// Comprehensive examples demonstrating type-safe expression-based queries with the MsSql data provider.
/// Shows practical usage patterns for realistic scenarios including customer management, order processing,
/// product catalogs, and user activity tracking.
/// </summary>
public static class QueryExamples
{
    #region Sample Entities for Examples

    /// <summary>
    /// Customer entity for CRM scenarios.
    /// </summary>
    public sealed class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? Phone { get; set; }
        public CustomerStatus Status { get; set; }
        public decimal CreditLimit { get; set; }
        public string Country { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalOrders { get; set; }
    }

    /// <summary>
    /// Order entity for e-commerce scenarios.
    /// </summary>
    public sealed class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public bool IsRushOrder { get; set; }
        public decimal TaxAmount { get; set; }
    }

    /// <summary>
    /// Product entity for inventory management.
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
        public string? Description { get; set; }
        public string Sku { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public bool IsDiscontinued { get; set; }
    }

    /// <summary>
    /// User activity entity for tracking user behavior.
    /// </summary>
    public sealed class UserActivity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public int? Duration { get; set; }
        public bool IsSuccessful { get; set; }
    }

    /// <summary>
    /// Category entity for product categorization.
    /// </summary>
    public sealed class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Customer status enumeration.
    /// </summary>
    public enum CustomerStatus
    {
        Pending = 0,
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Deleted = 4
    }

    /// <summary>
    /// Order status enumeration.
    /// </summary>
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4,
        Returned = 5
    }

    #endregion

    #region 1. Basic Queries

    /// <summary>
    /// Example 1.1: Find customer by ID using type-safe expressions.
    /// Generated SQL: SELECT * FROM [Customer] WHERE [Id] = @p0
    /// </summary>
    public static async Task<Customer?> FindCustomerByIdAsync(MsSqlDataProvider dataProvider, int customerId)
    {
        try
        {
            // Type-safe query - no magic strings, compile-time checking
            var command = MsSqlQueryCommandFactory.FindById<Customer>(customerId);
            var result = await dataProvider.Execute<Customer?>(command, CancellationToken.None);

            return result.IsSuccess ? result.Value : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding customer by ID {customerId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 1.2: Find all active customers.
    /// Generated SQL: SELECT * FROM [Customer] WHERE [IsActive] = @p0
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindActiveCustomersAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.IsActive);
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding active customers: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 1.3: Simple WHERE condition with string comparison.
    /// Generated SQL: SELECT * FROM [Customer] WHERE [Country] = @p0
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindCustomersByCountryAsync(MsSqlDataProvider dataProvider, string country)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.Country == country);
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding customers by country {country}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 1.4: Multiple conditions with AND operator.
    /// Generated SQL: SELECT * FROM [Customer] WHERE ([IsActive] = @p0 AND [CreditLimit] > @p1)
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindActiveCustomersWithHighCreditLimitAsync(
        MsSqlDataProvider dataProvider, decimal minCreditLimit)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
                c => c.IsActive && c.CreditLimit > minCreditLimit);
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding active customers with high credit limit: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 1.5: Multiple conditions with OR operator.
    /// Generated SQL: SELECT * FROM [Customer] WHERE ([Status] = @p0 OR [Status] = @p1)
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindCustomersWithActiveOrPendingStatusAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
                c => c.Status == CustomerStatus.Active || c.Status == CustomerStatus.Pending);
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding customers with active or pending status: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region 2. Complex Queries

    /// <summary>
    /// Example 2.1: Date range queries for recent customers.
    /// Generated SQL: SELECT * FROM [Customer] WHERE ([CreatedDate] >= @p0 AND [CreatedDate] <= @p1)
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindRecentCustomersAsync(
        MsSqlDataProvider dataProvider, DateTime startDate, DateTime endDate)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
                c => c.CreatedDate >= startDate && c.CreatedDate <= endDate);
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding recent customers: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2.2: String operations - Contains, StartsWith, EndsWith.
    /// Generated SQL: SELECT * FROM [Customer] WHERE [Name] LIKE '%' + @p0 + '%'
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindCustomersByNameContainsAsync(
        MsSqlDataProvider dataProvider, string searchTerm)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.Name.Contains(searchTerm));
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding customers by name contains '{searchTerm}': {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2.3: String StartsWith operation.
    /// Generated SQL: SELECT * FROM [Customer] WHERE [Email] LIKE @p0 + '%'
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindCustomersByEmailDomainAsync(
        MsSqlDataProvider dataProvider, string emailPrefix)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.Email.StartsWith(emailPrefix));
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding customers by email prefix '{emailPrefix}': {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2.4: Numeric comparisons with decimal precision.
    /// Generated SQL: SELECT * FROM [Product] WHERE ([Price] >= @p0 AND [Price] <= @p1)
    /// </summary>
    public static async Task<IEnumerable<Product>> FindProductsInPriceRangeAsync(
        MsSqlDataProvider dataProvider, decimal minPrice, decimal maxPrice)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Product>(
                p => p.Price >= minPrice && p.Price <= maxPrice);
            var result = await dataProvider.Execute<IEnumerable<Product>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding products in price range ${minPrice}-${maxPrice}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2.5: Null checking for optional fields.
    /// Generated SQL: SELECT * FROM [Customer] WHERE [LastLoginDate] IS NOT NULL
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindCustomersWithLoginHistoryAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(c => c.LastLoginDate != null);
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding customers with login history: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 2.6: Enum comparisons with type safety.
    /// Generated SQL: SELECT * FROM [Order] WHERE [Status] = @p0
    /// </summary>
    public static async Task<IEnumerable<Order>> FindOrdersByStatusAsync(
        MsSqlDataProvider dataProvider, OrderStatus status)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Order>(o => o.Status == status);
            var result = await dataProvider.Execute<IEnumerable<Order>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Order>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding orders by status {status}: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region 3. Collection Operations

    /// <summary>
    /// Example 3.1: IN queries - checking if value is in a list.
    /// Generated SQL: SELECT * FROM [Customer] WHERE [Country] IN (@p0, @p1, @p2)
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindCustomersInCountriesAsync(
        MsSqlDataProvider dataProvider, List<string> countries)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(c => countries.Contains(c.Country));
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding customers in countries: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 3.2: NOT IN queries using negation.
    /// Generated SQL: SELECT * FROM [Customer] WHERE NOT ([Status] IN (@p0, @p1))
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindCustomersNotInStatusListAsync(
        MsSqlDataProvider dataProvider, List<CustomerStatus> excludedStatuses)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
                c => !excludedStatuses.Contains(c.Status));
            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding customers not in status list: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 3.3: Complex collection filtering with multiple conditions.
    /// Generated SQL: SELECT * FROM [Order] WHERE ([CustomerId] IN (@p0, @p1, @p2) AND [TotalAmount] > @p3)
    /// </summary>
    public static async Task<IEnumerable<Order>> FindLargeOrdersForSpecificCustomersAsync(
        MsSqlDataProvider dataProvider, List<int> customerIds, decimal minAmount)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Order>(
                o => customerIds.Contains(o.CustomerId) && o.TotalAmount > minAmount);
            var result = await dataProvider.Execute<IEnumerable<Order>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Order>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding large orders for specific customers: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region 4. Aggregation Queries

    /// <summary>
    /// Example 4.1: Count records with type-safe filtering.
    /// Generated SQL: SELECT COUNT(*) FROM [Customer] WHERE [IsActive] = @p0
    /// </summary>
    public static async Task<int> CountActiveCustomersAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.Count<Customer>(c => c.IsActive);
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);

            return result.IsSuccess ? result.Value : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error counting active customers: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 4.2: Count all records without filtering.
    /// Generated SQL: SELECT COUNT(*) FROM [Product]
    /// </summary>
    public static async Task<int> CountAllProductsAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.Count<Product>();
            var result = await dataProvider.Execute<int>(command, CancellationToken.None);

            return result.IsSuccess ? result.Value : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error counting all products: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 4.3: Exists checks using Any method.
    /// Generated SQL: SELECT CASE WHEN EXISTS (SELECT 1 FROM [Customer] WHERE [Email] = @p0) THEN 1 ELSE 0 END
    /// </summary>
    public static async Task<bool> CheckIfEmailExistsAsync(MsSqlDataProvider dataProvider, string email)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.Any<Customer>(c => c.Email == email);
            var result = await dataProvider.Execute<bool>(command, CancellationToken.None);

            return result.IsSuccess && result.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if email exists: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 4.4: Complex exists check with multiple conditions.
    /// Generated SQL: SELECT CASE WHEN EXISTS (SELECT 1 FROM [Order] WHERE ([CustomerId] = @p0 AND [Status] = @p1)) THEN 1 ELSE 0 END
    /// </summary>
    public static async Task<bool> CheckIfCustomerHasPendingOrdersAsync(
        MsSqlDataProvider dataProvider, int customerId)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.Any<Order>(
                o => o.CustomerId == customerId && o.Status == OrderStatus.Pending);
            var result = await dataProvider.Execute<bool>(command, CancellationToken.None);

            return result.IsSuccess && result.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if customer has pending orders: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region 5. Paging and Ordering

    /// <summary>
    /// Example 5.1: Simple paging with ordering.
    /// Generated SQL: SELECT * FROM [Customer] ORDER BY [Name] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
    /// </summary>
    public static async Task<IEnumerable<Customer>> GetCustomersPagedAsync(
        MsSqlDataProvider dataProvider, int pageNumber, int pageSize)
    {
        try
        {
            var offset = (pageNumber - 1) * pageSize;
            var command = MsSqlQueryCommandFactory.FindWithPaging<Customer>(
                predicate: null,
                orderBy: c => c.Name,
                offset: offset,
                pageSize: pageSize);

            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);
            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting customers page {pageNumber}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 5.2: Filtered paging with complex conditions.
    /// Generated SQL: SELECT * FROM [Customer] WHERE ([IsActive] = @p0 AND [CreatedDate] >= @p1) ORDER BY [Name] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
    /// </summary>
    public static async Task<IEnumerable<Customer>> GetActiveCustomersFromDatePagedAsync(
        MsSqlDataProvider dataProvider, DateTime fromDate, int pageNumber, int pageSize)
    {
        try
        {
            var offset = (pageNumber - 1) * pageSize;
            var command = MsSqlQueryCommandFactory.FindWithPaging<Customer>(
                predicate: c => c.IsActive && c.CreatedDate >= fromDate,
                orderBy: c => c.Name,
                offset: offset,
                pageSize: pageSize);

            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);
            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting active customers from date paged: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 5.3: Ordered results without paging.
    /// Generated SQL: SELECT * FROM [Product] WHERE [InStock] = @p0 ORDER BY [Price]
    /// </summary>
    public static async Task<IEnumerable<Product>> GetInStockProductsOrderedByPriceAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Product>(
                predicate: p => p.InStock,
                orderBy: p => p.Price);

            var result = await dataProvider.Execute<IEnumerable<Product>>(command, CancellationToken.None);
            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting in-stock products ordered by price: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 5.4: Skip/Take pattern for infinite scrolling.
    /// Generated SQL: SELECT * FROM [UserActivity] WHERE [UserId] = @p0 ORDER BY [Timestamp] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
    /// </summary>
    public static async Task<IEnumerable<UserActivity>> GetUserActivityBatchAsync(
        MsSqlDataProvider dataProvider, int userId, int skip, int take)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWithPaging<UserActivity>(
                predicate: ua => ua.UserId == userId,
                orderBy: ua => ua.Timestamp,
                offset: skip,
                pageSize: take);

            var result = await dataProvider.Execute<IEnumerable<UserActivity>>(command, CancellationToken.None);
            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<UserActivity>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user activity batch: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region 6. Complex Scenarios

    /// <summary>
    /// Example 6.1: Complex business logic query - High-value customers.
    /// Generated SQL: SELECT * FROM [Customer] WHERE (([TotalOrders] > @p0 AND [CreditLimit] > @p1) AND [Status] = @p2)
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindHighValueCustomersAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
                c => c.TotalOrders > 10 && 
                     c.CreditLimit > 5000m && 
                     c.Status == CustomerStatus.Active);

            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);
            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding high-value customers: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 6.2: Time-based filtering for recent activity.
    /// Generated SQL: SELECT * FROM [UserActivity] WHERE ([Timestamp] >= @p0 AND [IsSuccessful] = @p1)
    /// </summary>
    public static async Task<IEnumerable<UserActivity>> FindRecentSuccessfulActivityAsync(
        MsSqlDataProvider dataProvider, TimeSpan lookbackPeriod)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.Subtract(lookbackPeriod);
            var command = MsSqlQueryCommandFactory.FindWhere<UserActivity>(
                ua => ua.Timestamp >= cutoffTime && ua.IsSuccessful);

            var result = await dataProvider.Execute<IEnumerable<UserActivity>>(command, CancellationToken.None);
            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<UserActivity>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding recent successful activity: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 6.3: Inventory management - Low stock products.
    /// Generated SQL: SELECT * FROM [Product] WHERE (([InStock] = @p0 AND [StockQuantity] < @p1) AND [IsDiscontinued] = @p2)
    /// </summary>
    public static async Task<IEnumerable<Product>> FindLowStockProductsAsync(
        MsSqlDataProvider dataProvider, int lowStockThreshold)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Product>(
                p => p.InStock && 
                     p.StockQuantity < lowStockThreshold && 
                     !p.IsDiscontinued);

            var result = await dataProvider.Execute<IEnumerable<Product>>(command, CancellationToken.None);
            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding low stock products: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region 7. Error Handling and Performance Considerations

    /// <summary>
    /// Example 7.1: Query with timeout and comprehensive error handling.
    /// Demonstrates proper error handling patterns for production use.
    /// </summary>
    public static async Task<IEnumerable<Customer>> FindCustomersWithTimeoutAsync(
        MsSqlDataProvider dataProvider, string searchTerm, TimeSpan timeout)
    {
        try
        {
            var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
                predicate: c => c.Name.Contains(searchTerm) || c.Email.Contains(searchTerm),
                timeout: timeout);

            var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);
            
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Query failed: {result.Message}");
                return Enumerable.Empty<Customer>();
            }

            var customers = result.Value?.ToList() ?? new List<Customer>();
            Console.WriteLine($"Found {customers.Count} customers matching '{searchTerm}'");
            
            return customers;
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"Query timed out after {timeout.TotalSeconds} seconds: {ex.Message}");
            return Enumerable.Empty<Customer>();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Invalid operation: {ex.Message}");
            return Enumerable.Empty<Customer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error during customer search: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 7.2: Performance-optimized query with selective field projection.
    /// Note: This would require custom SQL for field selection, showing when to switch from expressions to custom SQL.
    /// </summary>
    public static async Task<IEnumerable<object>> FindCustomerSummaryOptimizedAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            // For performance-critical scenarios with specific field requirements,
            // you might need to use custom SQL instead of full entity queries
            var sql = """
                SELECT Id, Name, Email, Status 
                FROM [Customer] 
                WHERE [IsActive] = @IsActive 
                ORDER BY [Name]
                """;

            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["IsActive"] = true
            };

            var command = new MsSqlQueryCommand<IEnumerable<object>>(sql, "Customer", parameters);
            var result = await dataProvider.Execute<IEnumerable<object>>(command, CancellationToken.None);

            return result.IsSuccess && result.Value != null ? result.Value : Enumerable.Empty<object>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in optimized customer summary query: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Example 7.3: Batch processing with error recovery.
    /// Demonstrates handling large datasets with proper error recovery.
    /// </summary>
    public static async Task<(int SuccessCount, int ErrorCount)> ProcessCustomersBatchAsync(
        MsSqlDataProvider dataProvider, List<int> customerIds, int batchSize = 100)
    {
        var successCount = 0;
        var errorCount = 0;

        try
        {
            for (int i = 0; i < customerIds.Count; i += batchSize)
            {
                var batchIds = customerIds.Skip(i).Take(batchSize).ToList();
                
                try
                {
                    var command = MsSqlQueryCommandFactory.FindWhere<Customer>(
                        c => batchIds.Contains(c.Id));
                    
                    var result = await dataProvider.Execute<IEnumerable<Customer>>(command, CancellationToken.None);
                    
                    if (result.IsSuccess && result.Value != null)
                    {
                        var processedCount = result.Value.Count();
                        successCount += processedCount;
                        Console.WriteLine($"Processed batch {(i / batchSize) + 1}: {processedCount} customers");
                    }
                    else
                    {
                        errorCount += batchIds.Count;
                        Console.WriteLine($"Batch {(i / batchSize) + 1} failed: {result.Message}");
                    }
                }
                catch (Exception batchEx)
                {
                    errorCount += batchIds.Count;
                    Console.WriteLine($"Error processing batch {(i / batchSize) + 1}: {batchEx.Message}");
                    // Continue processing other batches instead of failing completely
                }
                
                // Small delay to prevent overwhelming the database
                await Task.Delay(50);
            }

            Console.WriteLine($"Batch processing completed. Success: {successCount}, Errors: {errorCount}");
            return (successCount, errorCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error in batch processing: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region 8. Complete Usage Scenarios

    /// <summary>
    /// Example 8.1: E-commerce order analysis - Complete business scenario.
    /// Demonstrates combining multiple query patterns for a real business case.
    /// </summary>
    public static async Task<OrderAnalysisResult> AnalyzeCustomerOrdersAsync(
        MsSqlDataProvider dataProvider, int customerId, DateTime fromDate)
    {
        try
        {
            var result = new OrderAnalysisResult { CustomerId = customerId };

            // 1. Check if customer exists and is active
            var customer = await FindCustomerByIdAsync(dataProvider, customerId);
            if (customer == null || !customer.IsActive)
            {
                result.ErrorMessage = "Customer not found or inactive";
                return result;
            }

            result.CustomerName = customer.Name;

            // 2. Count total orders for the period
            var totalOrdersCommand = MsSqlQueryCommandFactory.Count<Order>(
                o => o.CustomerId == customerId && o.OrderDate >= fromDate);
            var totalOrdersResult = await dataProvider.Execute<int>(totalOrdersCommand, CancellationToken.None);
            result.TotalOrders = totalOrdersResult.IsSuccess ? totalOrdersResult.Value : 0;

            // 3. Check for pending orders
            var hasPendingOrders = await CheckIfCustomerHasPendingOrdersAsync(dataProvider, customerId);
            result.HasPendingOrders = hasPendingOrders;

            // 4. Get recent large orders (over $500)
            var largeOrdersCommand = MsSqlQueryCommandFactory.FindWhere<Order>(
                o => o.CustomerId == customerId && 
                     o.OrderDate >= fromDate && 
                     o.TotalAmount > 500m,
                orderBy: o => o.OrderDate);
            
            var largeOrdersResult = await dataProvider.Execute<IEnumerable<Order>>(largeOrdersCommand, CancellationToken.None);
            result.LargeOrders = largeOrdersResult.IsSuccess && largeOrdersResult.Value != null 
                ? largeOrdersResult.Value.ToList() 
                : new List<Order>();

            // 5. Calculate summary statistics
            if (result.LargeOrders.Count > 0)
            {
                result.TotalLargeOrderValue = result.LargeOrders.Sum(o => o.TotalAmount);
                result.AverageLargeOrderValue = result.LargeOrders.Average(o => o.TotalAmount);
            }

            result.IsSuccess = true;
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing customer orders: {ex.Message}");
            return new OrderAnalysisResult 
            { 
                CustomerId = customerId, 
                ErrorMessage = ex.Message,
                IsSuccess = false
            };
        }
    }

    /// <summary>
    /// Example 8.2: Product inventory dashboard - Multiple related queries.
    /// Shows how to efficiently gather dashboard data with type-safe queries.
    /// </summary>
    public static async Task<InventoryDashboard> GetInventoryDashboardAsync(MsSqlDataProvider dataProvider)
    {
        try
        {
            var dashboard = new InventoryDashboard();

            // Execute multiple queries concurrently for better performance
            var tasks = new[]
            {
                // Total products count
                Task.Run(async () => 
                {
                    var cmd = MsSqlQueryCommandFactory.Count<Product>();
                    var result = await dataProvider.Execute<int>(cmd, CancellationToken.None);
                    dashboard.TotalProducts = result.IsSuccess ? result.Value : 0;
                }),

                // In-stock products count
                Task.Run(async () => 
                {
                    var cmd = MsSqlQueryCommandFactory.Count<Product>(p => p.InStock);
                    var result = await dataProvider.Execute<int>(cmd, CancellationToken.None);
                    dashboard.InStockProducts = result.IsSuccess ? result.Value : 0;
                }),

                // Low stock products (< 10 units)
                Task.Run(async () => 
                {
                    var cmd = MsSqlQueryCommandFactory.FindWhere<Product>(
                        p => p.InStock && p.StockQuantity < 10 && !p.IsDiscontinued);
                    var result = await dataProvider.Execute<IEnumerable<Product>>(cmd, CancellationToken.None);
                    dashboard.LowStockProducts = result.IsSuccess && result.Value != null 
                        ? result.Value.ToList() 
                        : new List<Product>();
                }),

                // Discontinued products count
                Task.Run(async () => 
                {
                    var cmd = MsSqlQueryCommandFactory.Count<Product>(p => p.IsDiscontinued);
                    var result = await dataProvider.Execute<int>(cmd, CancellationToken.None);
                    dashboard.DiscontinuedProducts = result.IsSuccess ? result.Value : 0;
                })
            };

            await Task.WhenAll(tasks);

            dashboard.LowStockCount = dashboard.LowStockProducts.Count;
            dashboard.OutOfStockProducts = dashboard.TotalProducts - dashboard.InStockProducts;
            dashboard.GeneratedAt = DateTime.UtcNow;

            Console.WriteLine($"Dashboard generated: {dashboard.TotalProducts} total products, {dashboard.LowStockCount} low stock");
            return dashboard;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating inventory dashboard: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Result Classes for Complex Scenarios

    /// <summary>
    /// Result class for order analysis scenario.
    /// </summary>
    public sealed class OrderAnalysisResult
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public bool HasPendingOrders { get; set; }
        public List<Order> LargeOrders { get; set; } = new();
        public decimal TotalLargeOrderValue { get; set; }
        public decimal AverageLargeOrderValue { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Dashboard data for inventory management.
    /// </summary>
    public sealed class InventoryDashboard
    {
        public int TotalProducts { get; set; }
        public int InStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int LowStockCount { get; set; }
        public List<Product> LowStockProducts { get; set; } = new();
        public int DiscontinuedProducts { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    #endregion

    #region Performance Tips and Best Practices

    /*
     * PERFORMANCE CONSIDERATIONS:
     * 
     * 1. **Use Specific Queries**: Avoid SELECT * when you only need specific columns.
     *    For performance-critical scenarios, write custom SQL with selected fields.
     * 
     * 2. **Index Usage**: Ensure your WHERE clause conditions match your database indexes.
     *    The expression translator generates parameterized queries that work well with indexes.
     * 
     * 3. **Paging**: Always use paging for large result sets. The FindWithPaging method
     *    generates efficient OFFSET/FETCH queries.
     * 
     * 4. **Batch Processing**: For large datasets, process in batches to avoid memory issues
     *    and improve user experience.
     * 
     * 5. **Async Patterns**: Use async/await properly and consider concurrent execution
     *    for independent queries.
     * 
     * 6. **Error Handling**: Implement proper error handling with retries for transient failures.
     * 
     * 7. **Connection Management**: The data provider handles connection pooling,
     *    but be mindful of long-running operations.
     * 
     * SECURITY CONSIDERATIONS:
     * 
     * 1. **SQL Injection Prevention**: Expression-based queries automatically generate
     *    parameterized SQL, preventing injection attacks.
     * 
     * 2. **Input Validation**: Always validate input parameters before constructing queries.
     * 
     * 3. **Timeout Settings**: Use appropriate timeouts to prevent hanging operations.
     * 
     * 4. **Schema Security**: Use schema mapping for multi-tenant scenarios.
     * 
     * TYPE SAFETY BENEFITS:
     * 
     * 1. **Compile-time Checking**: Catch errors at compile time instead of runtime.
     * 
     * 2. **IntelliSense Support**: Full IDE support with auto-completion and refactoring.
     * 
     * 3. **Refactoring Safety**: Property renames automatically update all queries.
     * 
     * 4. **No Magic Strings**: Eliminate string-based column references that can break silently.
     */

    #endregion
}