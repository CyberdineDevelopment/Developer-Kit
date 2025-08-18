using FractalDataWorks.Samples.MsSqlProvider.Models;
using FractalDataWorks.Services.DataProvider.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq.Expressions;

namespace FractalDataWorks.Samples.MsSqlProvider.Services;

/// <summary>
/// Service that demonstrates comprehensive CRUD operations and advanced features
/// of the FractalDataWorks MsSql data provider
/// </summary>
public sealed class SampleOperationsService
{
    private readonly IDataService _dataService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SampleOperationsService> _logger;

    public SampleOperationsService(
        IDataService dataService,
        IConfiguration configuration,
        ILogger<SampleOperationsService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs all sample operations demonstrating the data provider capabilities
    /// </summary>
    public async Task RunAllSamplesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting comprehensive data provider samples");
        var totalStopwatch = Stopwatch.StartNew();

        try
        {
            // Basic CRUD Operations
            await RunBasicCrudSamplesAsync(cancellationToken);

            // Advanced Query Operations
            await RunAdvancedQuerySamplesAsync(cancellationToken);

            // Transaction Demonstrations
            if (_configuration.GetValue<bool>("SampleOperations:EnableTransactionDemos", true))
            {
                await RunTransactionSamplesAsync(cancellationToken);
            }

            // Bulk Operations
            if (_configuration.GetValue<bool>("SampleOperations:EnableBulkOperations", true))
            {
                await RunBulkOperationSamplesAsync(cancellationToken);
            }

            // Complex Query Scenarios
            if (_configuration.GetValue<bool>("SampleOperations:EnableComplexQueries", true))
            {
                await RunComplexQuerySamplesAsync(cancellationToken);
            }

            // Performance and Analytics Demonstrations
            await RunPerformanceSamplesAsync(cancellationToken);

            totalStopwatch.Stop();
            _logger.LogInformation("All samples completed successfully in {ElapsedMs}ms", 
                totalStopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sample operations failed");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates basic CRUD (Create, Read, Update, Delete) operations
    /// </summary>
    private async Task RunBasicCrudSamplesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Basic CRUD Operations ===");
        var stopwatch = Stopwatch.StartNew();

        // CREATE operations
        await DemonstrateCreateOperationsAsync(cancellationToken);

        // READ operations  
        await DemonstrateReadOperationsAsync(cancellationToken);

        // UPDATE operations
        await DemonstrateUpdateOperationsAsync(cancellationToken);

        // DELETE operations
        await DemonstrateDeleteOperationsAsync(cancellationToken);

        stopwatch.Stop();
        _logger.LogInformation("Basic CRUD operations completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Demonstrates CREATE operations with different scenarios
    /// </summary>
    private async Task DemonstrateCreateOperationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- CREATE Operations ---");

        // Create categories first (for foreign key relationships)
        var electronics = new Category
        {
            Name = "Electronics",
            Description = "Electronic devices and accessories",
            IsActive = true
        };

        var smartphones = new Category
        {
            Name = "Smartphones",
            Description = "Mobile phones and accessories",
            IsActive = true
        };

        // Insert categories
        var insertCategoryCommand = _dataService.CreateInsertCommand<Category>();
        var electronicsResult = await insertCategoryCommand.ExecuteAsync(electronics, cancellationToken);
        var smartphonesResult = await insertCategoryCommand.ExecuteAsync(smartphones, cancellationToken);

        if (electronicsResult.IsSuccess && smartphonesResult.IsSuccess)
        {
            _logger.LogInformation("Successfully created categories");
            
            // Set up hierarchy (smartphones under electronics)
            smartphones.ParentId = electronics.Id;
            var updateCategoryCommand = _dataService.CreateUpdateCommand<Category>();
            await updateCategoryCommand.ExecuteAsync(smartphones, cancellationToken);
        }

        // Create customers
        var customers = new[]
        {
            new Customer
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                CreditLimit = 5000.00m,
                IsActive = true
            },
            new Customer
            {
                Name = "Jane Smith", 
                Email = "jane.smith@example.com",
                CreditLimit = 10000.00m,
                IsActive = true
            }
        };

        var insertCustomerCommand = _dataService.CreateInsertCommand<Customer>();
        foreach (var customer in customers)
        {
            var result = await insertCustomerCommand.ExecuteAsync(customer, cancellationToken);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Created customer: {CustomerName} (ID: {CustomerId})", 
                    customer.Name, customer.Id);
            }
        }

        // Create products
        var products = new[]
        {
            new Product
            {
                Name = "iPhone 15 Pro",
                SKU = "APPLE-IP15PRO-128",
                Price = 999.99m,
                CategoryId = smartphones.Id,
                InStock = true,
                Description = "Latest iPhone with advanced features"
            },
            new Product
            {
                Name = "Samsung Galaxy S24",
                SKU = "SAMSUNG-GS24-256",
                Price = 899.99m,
                CategoryId = smartphones.Id,
                InStock = true,
                Description = "Premium Android smartphone"
            }
        };

        var insertProductCommand = _dataService.CreateInsertCommand<Product>();
        foreach (var product in products)
        {
            var result = await insertProductCommand.ExecuteAsync(product, cancellationToken);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Created product: {ProductName} (ID: {ProductId})", 
                    product.Name, product.Id);
            }
        }

        // Create orders
        var order = new Order
        {
            CustomerId = customers[0].Id,
            StatusEnum = OrderStatus.Processing,
            TotalAmount = 999.99m,
            OrderDate = DateTime.UtcNow
        };

        var insertOrderCommand = _dataService.CreateInsertCommand<Order>();
        var orderResult = await insertOrderCommand.ExecuteAsync(order, cancellationToken);
        if (orderResult.IsSuccess)
        {
            _logger.LogInformation("Created order: {OrderId} for customer {CustomerId}", 
                order.Id, order.CustomerId);
        }
    }

    /// <summary>
    /// Demonstrates READ operations with various query patterns
    /// </summary>
    private async Task DemonstrateReadOperationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- READ Operations ---");

        // Query all active customers
        var queryCustomerCommand = _dataService.CreateQueryCommand<Customer>();
        var activeCustomers = await queryCustomerCommand
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Found {CustomerCount} active customers", activeCustomers.Count());

        // Query customers with credit limit above threshold
        var highCreditCustomers = await queryCustomerCommand
            .Where(c => c.CreditLimit > 7500)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Found {CustomerCount} customers with credit limit > $7,500", 
            highCreditCustomers.Count());

        // Query products by category
        var queryProductCommand = _dataService.CreateQueryCommand<Product>();
        var smartphoneProducts = await queryProductCommand
            .Where(p => p.Category.Name == "Smartphones")
            .OrderBy(p => p.Price)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Found {ProductCount} smartphone products", smartphoneProducts.Count());

        // Query orders with customer information
        var queryOrderCommand = _dataService.CreateQueryCommand<Order>();
        var recentOrders = await queryOrderCommand
            .Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Found {OrderCount} orders in the last 30 days", recentOrders.Count());

        // Single entity queries
        var firstCustomer = await queryCustomerCommand
            .Where(c => c.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (firstCustomer != null)
        {
            _logger.LogInformation("First active customer: {CustomerName}", firstCustomer.Name);
        }

        // Count operations
        var totalCustomers = await queryCustomerCommand.CountAsync(cancellationToken);
        var activeCustomerCount = await queryCustomerCommand
            .Where(c => c.IsActive)
            .CountAsync(cancellationToken);

        _logger.LogInformation("Total customers: {TotalCustomers}, Active: {ActiveCustomers}", 
            totalCustomers, activeCustomerCount);
    }

    /// <summary>
    /// Demonstrates UPDATE operations with different scenarios
    /// </summary>
    private async Task DemonstrateUpdateOperationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- UPDATE Operations ---");

        // Update single customer
        var queryCustomerCommand = _dataService.CreateQueryCommand<Customer>();
        var customer = await queryCustomerCommand
            .Where(c => c.Email == "john.doe@example.com")
            .FirstOrDefaultAsync(cancellationToken);

        if (customer != null)
        {
            customer.CreditLimit = 7500.00m;
            
            var updateCustomerCommand = _dataService.CreateUpdateCommand<Customer>();
            var result = await updateCustomerCommand.ExecuteAsync(customer, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Updated credit limit for customer {CustomerName}", customer.Name);
            }
        }

        // Bulk update operations
        var queryProductCommand = _dataService.CreateQueryCommand<Product>();
        var expensiveProducts = await queryProductCommand
            .Where(p => p.Price > 800)
            .ExecuteAsync(cancellationToken);

        var updateProductCommand = _dataService.CreateUpdateCommand<Product>();
        foreach (var product in expensiveProducts)
        {
            product.Price *= 0.95m; // 5% discount
            await updateProductCommand.ExecuteAsync(product, cancellationToken);
        }

        _logger.LogInformation("Applied 5% discount to {ProductCount} expensive products", 
            expensiveProducts.Count());

        // Update order status
        var queryOrderCommand = _dataService.CreateQueryCommand<Order>();
        var pendingOrders = await queryOrderCommand
            .Where(o => o.Status == nameof(OrderStatus.Processing))
            .ExecuteAsync(cancellationToken);

        var updateOrderCommand = _dataService.CreateUpdateCommand<Order>();
        foreach (var order in pendingOrders)
        {
            order.StatusEnum = OrderStatus.Shipped;
            await updateOrderCommand.ExecuteAsync(order, cancellationToken);
        }

        _logger.LogInformation("Updated {OrderCount} orders to Shipped status", pendingOrders.Count());
    }

    /// <summary>
    /// Demonstrates DELETE operations with proper handling
    /// </summary>
    private async Task DemonstrateDeleteOperationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- DELETE Operations ---");

        // Create a test customer to delete
        var testCustomer = new Customer
        {
            Name = "Test Customer",
            Email = "test@example.com",
            CreditLimit = 1000.00m,
            IsActive = false
        };

        var insertCustomerCommand = _dataService.CreateInsertCommand<Customer>();
        await insertCustomerCommand.ExecuteAsync(testCustomer, cancellationToken);

        // Delete the test customer
        var deleteCustomerCommand = _dataService.CreateDeleteCommand<Customer>();
        var deleteResult = await deleteCustomerCommand.ExecuteAsync(testCustomer, cancellationToken);

        if (deleteResult.IsSuccess)
        {
            _logger.LogInformation("Successfully deleted test customer");
        }

        // Soft delete example (marking as inactive instead of deleting)
        var queryCustomerCommand = _dataService.CreateQueryCommand<Customer>();
        var inactiveCustomers = await queryCustomerCommand
            .Where(c => !c.IsActive)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Found {CustomerCount} inactive customers (soft deleted)", 
            inactiveCustomers.Count());
    }

    /// <summary>
    /// Demonstrates advanced query operations and complex expressions
    /// </summary>
    private async Task RunAdvancedQuerySamplesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Advanced Query Operations ===");
        var stopwatch = Stopwatch.StartNew();

        // Complex WHERE clauses
        await DemonstrateComplexWhereClauses(cancellationToken);

        // Joins and navigation properties
        await DemonstrateJoinsAndNavigationAsync(cancellationToken);

        // Aggregation operations
        await DemonstrateAggregationOperationsAsync(cancellationToken);

        // Paging and sorting
        await DemonstratePagingAndSortingAsync(cancellationToken);

        stopwatch.Stop();
        _logger.LogInformation("Advanced query operations completed in {ElapsedMs}ms", 
            stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Demonstrates complex WHERE clause expressions
    /// </summary>
    private async Task DemonstrateComplexWhereClauses(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Complex WHERE Clauses ---");

        var queryCustomerCommand = _dataService.CreateQueryCommand<Customer>();

        // Multiple conditions with AND/OR
        var complexCustomers = await queryCustomerCommand
            .Where(c => (c.CreditLimit > 5000 && c.IsActive) || c.Email.Contains("john"))
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Complex WHERE: Found {CustomerCount} customers", complexCustomers.Count());

        // Date range queries
        var queryOrderCommand = _dataService.CreateQueryCommand<Order>();
        var dateRangeOrders = await queryOrderCommand
            .Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-7) && 
                       o.OrderDate <= DateTime.UtcNow.AddDays(1))
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Date range: Found {OrderCount} orders in last week", 
            dateRangeOrders.Count());

        // String operations
        var queryProductCommand = _dataService.CreateQueryCommand<Product>();
        var nameSearchProducts = await queryProductCommand
            .Where(p => p.Name.StartsWith("iPhone") || p.Name.Contains("Samsung"))
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("String search: Found {ProductCount} matching products", 
            nameSearchProducts.Count());

        // Null checks
        var categoriesWithoutParent = await _dataService.CreateQueryCommand<Category>()
            .Where(c => c.ParentId == null)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Null check: Found {CategoryCount} root categories", 
            categoriesWithoutParent.Count());
    }

    /// <summary>
    /// Demonstrates joins and navigation property usage
    /// </summary>
    private async Task DemonstrateJoinsAndNavigationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Joins and Navigation Properties ---");

        // Query orders with customer information
        var queryOrderCommand = _dataService.CreateQueryCommand<Order>();
        var ordersWithCustomers = await queryOrderCommand
            .Where(o => o.Customer.IsActive && o.TotalAmount > 500)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Orders with customer navigation: Found {OrderCount} orders", 
            ordersWithCustomers.Count());

        // Query products with category information
        var queryProductCommand = _dataService.CreateQueryCommand<Product>();
        var productsWithCategories = await queryProductCommand
            .Where(p => p.Category.IsActive && p.InStock)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Products with category navigation: Found {ProductCount} products", 
            productsWithCategories.Count());

        // Hierarchical queries (category with parent)
        var queryCategoryCommand = _dataService.CreateQueryCommand<Category>();
        var subCategories = await queryCategoryCommand
            .Where(c => c.Parent != null && c.Parent.Name == "Electronics")
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Hierarchical query: Found {CategoryCount} subcategories", 
            subCategories.Count());
    }

    /// <summary>
    /// Demonstrates aggregation operations (Count, Sum, Average, etc.)
    /// </summary>
    private async Task DemonstrateAggregationOperationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Aggregation Operations ---");

        // Count operations
        var totalCustomers = await _dataService.CreateQueryCommand<Customer>()
            .CountAsync(cancellationToken);

        var activeCustomers = await _dataService.CreateQueryCommand<Customer>()
            .Where(c => c.IsActive)
            .CountAsync(cancellationToken);

        _logger.LogInformation("Count aggregation: Total={TotalCustomers}, Active={ActiveCustomers}", 
            totalCustomers, activeCustomers);

        // Sum operations
        var queryOrderCommand = _dataService.CreateQueryCommand<Order>();
        var orders = await queryOrderCommand.ExecuteAsync(cancellationToken);
        var totalOrderValue = orders.Sum(o => o.TotalAmount);

        _logger.LogInformation("Sum aggregation: Total order value=${TotalValue:F2}", totalOrderValue);

        // Average operations
        var averageOrderValue = orders.Count() > 0 ? orders.Average(o => o.TotalAmount) : 0;
        var averageCreditLimit = await _dataService.CreateQueryCommand<Customer>()
            .Where(c => c.IsActive)
            .ExecuteAsync(cancellationToken)
            .ContinueWith(t => t.Result.Count() > 0 ? t.Result.Average(c => c.CreditLimit) : 0, 
                cancellationToken);

        _logger.LogInformation("Average aggregation: Order=${AvgOrder:F2}, Credit=${AvgCredit:F2}", 
            averageOrderValue, averageCreditLimit);

        // Min/Max operations
        var products = await _dataService.CreateQueryCommand<Product>()
            .ExecuteAsync(cancellationToken);

        if (products.Count() > 0)
        {
            var minPrice = products.Min(p => p.Price);
            var maxPrice = products.Max(p => p.Price);

            _logger.LogInformation("Min/Max aggregation: Price range ${MinPrice:F2} - ${MaxPrice:F2}", 
                minPrice, maxPrice);
        }
    }

    /// <summary>
    /// Demonstrates paging and sorting operations
    /// </summary>
    private async Task DemonstratePagingAndSortingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Paging and Sorting ---");

        // Simple sorting
        var queryCustomerCommand = _dataService.CreateQueryCommand<Customer>();
        var sortedCustomers = await queryCustomerCommand
            .OrderBy(c => c.Name)
            .ThenByDescending(c => c.CreditLimit)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Sorting: Found {CustomerCount} customers sorted by name", 
            sortedCustomers.Count());

        // Paging with Skip/Take
        var pageSize = 5;
        var page1Customers = await queryCustomerCommand
            .OrderBy(c => c.Id)
            .Take(pageSize)
            .ExecuteAsync(cancellationToken);

        var page2Customers = await queryCustomerCommand
            .OrderBy(c => c.Id)
            .Skip(pageSize)
            .Take(pageSize)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Paging: Page 1 has {Page1Count} customers, Page 2 has {Page2Count} customers", 
            page1Customers.Count(), page2Customers.Count());

        // Complex sorting with navigation properties
        var queryOrderCommand = _dataService.CreateQueryCommand<Order>();
        var sortedOrders = await queryOrderCommand
            .OrderBy(o => o.Customer.Name)
            .ThenByDescending(o => o.OrderDate)
            .Take(10)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Complex sorting: Found {OrderCount} orders sorted by customer name", 
            sortedOrders.Count());
    }

    /// <summary>
    /// Demonstrates transaction operations
    /// </summary>
    private async Task RunTransactionSamplesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Transaction Operations ===");
        var stopwatch = Stopwatch.StartNew();

        await DemonstrateSuccessfulTransactionAsync(cancellationToken);
        await DemonstrateTransactionRollbackAsync(cancellationToken);

        stopwatch.Stop();
        _logger.LogInformation("Transaction operations completed in {ElapsedMs}ms", 
            stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Demonstrates a successful transaction with multiple operations
    /// </summary>
    private async Task DemonstrateSuccessfulTransactionAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Successful Transaction ---");

        using var transaction = await _dataService.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Create customer
            var customer = new Customer
            {
                Name = "Transaction Customer",
                Email = "transaction@example.com",
                CreditLimit = 2000.00m,
                IsActive = true
            };

            var insertCustomerCommand = _dataService.CreateInsertCommand<Customer>();
            await insertCustomerCommand.ExecuteAsync(customer, cancellationToken);

            // Create order for the customer
            var order = new Order
            {
                CustomerId = customer.Id,
                StatusEnum = OrderStatus.Pending,
                TotalAmount = 150.00m
            };

            var insertOrderCommand = _dataService.CreateInsertCommand<Order>();
            await insertOrderCommand.ExecuteAsync(order, cancellationToken);

            // Log user activity
            var activity = new UserActivity
            {
                UserId = customer.Id.ToString(),
                ActivityType = "ORDER_CREATED",
                IsSuccessful = true,
                Details = $"Order {order.Id} created for ${order.TotalAmount:F2}"
            };

            var insertActivityCommand = _dataService.CreateInsertCommand<UserActivity>();
            await insertActivityCommand.ExecuteAsync(activity, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Successfully completed transaction: Customer={CustomerId}, Order={OrderId}", 
                customer.Id, order.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Transaction failed and was rolled back");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates transaction rollback on error
    /// </summary>
    private async Task DemonstrateTransactionRollbackAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Transaction Rollback ---");

        using var transaction = await _dataService.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Create a customer
            var customer = new Customer
            {
                Name = "Rollback Customer",
                Email = "rollback@example.com",
                CreditLimit = 1000.00m,
                IsActive = true
            };

            var insertCustomerCommand = _dataService.CreateInsertCommand<Customer>();
            await insertCustomerCommand.ExecuteAsync(customer, cancellationToken);

            // Intentionally create a duplicate email to cause constraint violation
            var duplicateCustomer = new Customer
            {
                Name = "Duplicate Customer",
                Email = "rollback@example.com", // Same email - will cause error
                CreditLimit = 500.00m,
                IsActive = true
            };

            try
            {
                await insertCustomerCommand.ExecuteAsync(duplicateCustomer, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogInformation("Transaction correctly rolled back due to constraint violation");
                
                // Verify the first customer was not saved
                var queryCommand = _dataService.CreateQueryCommand<Customer>();
                var savedCustomer = await queryCommand
                    .Where(c => c.Email == "rollback@example.com")
                    .FirstOrDefaultAsync(cancellationToken);

                if (savedCustomer == null)
                {
                    _logger.LogInformation("Rollback verification: Customer was not saved (correct)");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in rollback demonstration");
        }
    }

    /// <summary>
    /// Demonstrates bulk operations for performance
    /// </summary>
    private async Task RunBulkOperationSamplesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Bulk Operations ===");
        var stopwatch = Stopwatch.StartNew();

        await DemonstrateBulkInsertAsync(cancellationToken);
        await DemonstrateBulkUpdateAsync(cancellationToken);
        await DemonstrateBulkDeleteAsync(cancellationToken);

        stopwatch.Stop();
        _logger.LogInformation("Bulk operations completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Demonstrates bulk insert operations
    /// </summary>
    private async Task DemonstrateBulkInsertAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Bulk Insert ---");

        var customerCount = _configuration.GetValue<int>("SampleOperations:CustomerTestCount", 100);
        var customers = new List<Customer>();

        // Generate test customers
        for (int i = 1; i <= customerCount; i++)
        {
            customers.Add(new Customer
            {
                Name = $"Bulk Customer {i:D3}",
                Email = $"bulk{i:D3}@example.com",
                CreditLimit = (decimal)(Random.Shared.NextDouble() * 10000),
                IsActive = Random.Shared.Next(0, 10) < 9 // 90% active
            });
        }

        var insertCommand = _dataService.CreateInsertCommand<Customer>();
        var bulkStopwatch = Stopwatch.StartNew();

        // Bulk insert using batch operations
        const int batchSize = 50;
        for (int i = 0; i < customers.Count; i += batchSize)
        {
            var batch = customers.Skip(i).Take(batchSize);
            foreach (var customer in batch)
            {
                await insertCommand.ExecuteAsync(customer, cancellationToken);
            }
        }

        bulkStopwatch.Stop();
        _logger.LogInformation("Bulk inserted {CustomerCount} customers in {ElapsedMs}ms", 
            customerCount, bulkStopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Demonstrates bulk update operations
    /// </summary>
    private async Task DemonstrateBulkUpdateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Bulk Update ---");

        // Get bulk customers for updating
        var queryCommand = _dataService.CreateQueryCommand<Customer>();
        var bulkCustomers = await queryCommand
            .Where(c => c.Email.StartsWith("bulk"))
            .ExecuteAsync(cancellationToken);

        var updateCommand = _dataService.CreateUpdateCommand<Customer>();
        var updateStopwatch = Stopwatch.StartNew();

        // Apply bulk credit limit increase
        foreach (var customer in bulkCustomers)
        {
            customer.CreditLimit *= 1.1m; // 10% increase
            await updateCommand.ExecuteAsync(customer, cancellationToken);
        }

        updateStopwatch.Stop();
        _logger.LogInformation("Bulk updated {CustomerCount} customers in {ElapsedMs}ms", 
            bulkCustomers.Count(), updateStopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Demonstrates bulk delete operations
    /// </summary>
    private async Task DemonstrateBulkDeleteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Bulk Delete ---");

        // Get inactive bulk customers for deletion
        var queryCommand = _dataService.CreateQueryCommand<Customer>();
        var inactiveCustomers = await queryCommand
            .Where(c => c.Email.StartsWith("bulk") && !c.IsActive)
            .ExecuteAsync(cancellationToken);

        var deleteCommand = _dataService.CreateDeleteCommand<Customer>();
        var deleteStopwatch = Stopwatch.StartNew();

        foreach (var customer in inactiveCustomers)
        {
            await deleteCommand.ExecuteAsync(customer, cancellationToken);
        }

        deleteStopwatch.Stop();
        _logger.LogInformation("Bulk deleted {CustomerCount} inactive customers in {ElapsedMs}ms", 
            inactiveCustomers.Count(), deleteStopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Demonstrates complex query scenarios and advanced patterns
    /// </summary>
    private async Task RunComplexQuerySamplesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Complex Query Scenarios ===");
        var stopwatch = Stopwatch.StartNew();

        await DemonstrateSubqueriesAsync(cancellationToken);
        await DemonstrateAnalyticalQueriesAsync(cancellationToken);
        await DemonstrateHierarchicalQueriesAsync(cancellationToken);

        stopwatch.Stop();
        _logger.LogInformation("Complex query scenarios completed in {ElapsedMs}ms", 
            stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Demonstrates subquery-like operations using LINQ
    /// </summary>
    private async Task DemonstrateSubqueriesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Subquery Operations ---");

        // Find customers who have placed orders
        var queryCustomerCommand = _dataService.CreateQueryCommand<Customer>();
        var queryOrderCommand = _dataService.CreateQueryCommand<Order>();

        var allOrders = await queryOrderCommand.ExecuteAsync(cancellationToken);
        var customerIdsWithOrders = allOrders.Select(o => o.CustomerId).Distinct();

        var customersWithOrders = await queryCustomerCommand
            .Where(c => customerIdsWithOrders.Contains(c.Id))
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation("Subquery: Found {CustomerCount} customers who have placed orders", 
            customersWithOrders.Count());

        // Find products in categories with more than 1 product
        var queryProductCommand = _dataService.CreateQueryCommand<Product>();
        var allProducts = await queryProductCommand.ExecuteAsync(cancellationToken);
        var categoriesWithMultipleProducts = allProducts
            .GroupBy(p => p.CategoryId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        var productsInPopularCategories = allProducts
            .Where(p => categoriesWithMultipleProducts.Contains(p.CategoryId));

        _logger.LogInformation("Subquery: Found {ProductCount} products in categories with multiple products", 
            productsInPopularCategories.Count());
    }

    /// <summary>
    /// Demonstrates analytical queries with grouping and aggregation
    /// </summary>
    private async Task DemonstrateAnalyticalQueriesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Analytical Queries ---");

        // Customer analysis
        var customers = await _dataService.CreateQueryCommand<Customer>()
            .ExecuteAsync(cancellationToken);

        var customerAnalytics = customers
            .GroupBy(c => c.IsActive)
            .Select(g => new
            {
                IsActive = g.Key,
                Count = g.Count(),
                AverageCreditLimit = g.Average(c => c.CreditLimit),
                TotalCreditLimit = g.Sum(c => c.CreditLimit)
            });

        foreach (var analytics in customerAnalytics)
        {
            _logger.LogInformation("Customer Analytics - Active={IsActive}: Count={Count}, " +
                "Avg Credit=${AvgCredit:F2}, Total Credit=${TotalCredit:F2}",
                analytics.IsActive, analytics.Count, analytics.AverageCreditLimit, 
                analytics.TotalCreditLimit);
        }

        // Order analysis by status
        var orders = await _dataService.CreateQueryCommand<Order>()
            .ExecuteAsync(cancellationToken);

        var orderAnalytics = orders
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                TotalValue = g.Sum(o => o.TotalAmount),
                AverageValue = g.Average(o => o.TotalAmount)
            })
            .OrderByDescending(x => x.TotalValue);

        foreach (var analytics in orderAnalytics)
        {
            _logger.LogInformation("Order Analytics - Status={Status}: Count={Count}, " +
                "Total=${TotalValue:F2}, Avg=${AvgValue:F2}",
                analytics.Status, analytics.Count, analytics.TotalValue, analytics.AverageValue);
        }

        // Product analysis by category
        var products = await _dataService.CreateQueryCommand<Product>()
            .ExecuteAsync(cancellationToken);

        var productAnalytics = products
            .GroupBy(p => p.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Count = g.Count(),
                AveragePrice = g.Average(p => p.Price),
                InStockCount = g.Count(p => p.InStock)
            });

        foreach (var analytics in productAnalytics)
        {
            _logger.LogInformation("Product Analytics - CategoryId={CategoryId}: Count={Count}, " +
                "Avg Price=${AvgPrice:F2}, In Stock={InStock}",
                analytics.CategoryId, analytics.Count, analytics.AveragePrice, analytics.InStockCount);
        }
    }

    /// <summary>
    /// Demonstrates hierarchical queries for category trees
    /// </summary>
    private async Task DemonstrateHierarchicalQueriesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Hierarchical Queries ---");

        var categories = await _dataService.CreateQueryCommand<Category>()
            .ExecuteAsync(cancellationToken);

        // Find root categories
        var rootCategories = categories.Where(c => c.ParentId == null);
        _logger.LogInformation("Found {RootCount} root categories", rootCategories.Count());

        // Build category hierarchy
        foreach (var rootCategory in rootCategories)
        {
            await LogCategoryHierarchy(rootCategory, categories, 0);
        }

        // Find leaf categories (categories with no children)
        var leafCategories = categories.Where(c => !categories.Any(child => child.ParentId == c.Id));
        _logger.LogInformation("Found {LeafCount} leaf categories", leafCategories.Count());
    }

    /// <summary>
    /// Recursively logs category hierarchy
    /// </summary>
    private async Task LogCategoryHierarchy(Category category, IEnumerable<Category> allCategories, int level)
    {
        var indent = new string(' ', level * 2);
        var productCount = await _dataService.CreateQueryCommand<Product>()
            .Where(p => p.CategoryId == category.Id)
            .CountAsync();

        _logger.LogInformation("{Indent}- {CategoryName} (ID: {CategoryId}, Products: {ProductCount})",
            indent, category.Name, category.Id, productCount);

        var children = allCategories.Where(c => c.ParentId == category.Id);
        foreach (var child in children)
        {
            await LogCategoryHierarchy(child, allCategories, level + 1);
        }
    }

    /// <summary>
    /// Demonstrates performance monitoring and optimization techniques
    /// </summary>
    private async Task RunPerformanceSamplesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Performance Samples ===");
        var stopwatch = Stopwatch.StartNew();

        await DemonstrateQueryPerformanceAsync(cancellationToken);
        await DemonstrateConnectionPoolingAsync(cancellationToken);
        await DemonstrateUserActivityLoggingAsync(cancellationToken);

        stopwatch.Stop();
        _logger.LogInformation("Performance samples completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Demonstrates query performance monitoring
    /// </summary>
    private async Task DemonstrateQueryPerformanceAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Query Performance ---");

        var iterations = 10;
        var stopwatch = new Stopwatch();

        // Test simple query performance
        stopwatch.Start();
        for (int i = 0; i < iterations; i++)
        {
            var customers = await _dataService.CreateQueryCommand<Customer>()
                .Where(c => c.IsActive)
                .ExecuteAsync(cancellationToken);
        }
        stopwatch.Stop();

        var avgTimePerQuery = stopwatch.ElapsedMilliseconds / iterations;
        _logger.LogInformation("Simple query average time: {AvgTime}ms per query ({Iterations} iterations)", 
            avgTimePerQuery, iterations);

        // Test complex query performance
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var complexResult = await _dataService.CreateQueryCommand<Order>()
                .Where(o => o.Customer.IsActive && o.TotalAmount > 100)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ExecuteAsync(cancellationToken);
        }
        stopwatch.Stop();

        var avgComplexTime = stopwatch.ElapsedMilliseconds / iterations;
        _logger.LogInformation("Complex query average time: {AvgTime}ms per query ({Iterations} iterations)", 
            avgComplexTime, iterations);
    }

    /// <summary>
    /// Demonstrates connection pooling behavior
    /// </summary>
    private async Task DemonstrateConnectionPoolingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- Connection Pooling ---");

        // Simulate concurrent operations
        var tasks = new List<Task>();
        var concurrentOperations = 5;

        for (int i = 0; i < concurrentOperations; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(async () =>
            {
                var stopwatch = Stopwatch.StartNew();
                
                var customers = await _dataService.CreateQueryCommand<Customer>()
                    .Take(10)
                    .ExecuteAsync(cancellationToken);

                stopwatch.Stop();
                _logger.LogInformation("Concurrent operation {TaskId} completed in {ElapsedMs}ms", 
                    taskId, stopwatch.ElapsedMilliseconds);
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
        _logger.LogInformation("All {OperationCount} concurrent operations completed", concurrentOperations);
    }

    /// <summary>
    /// Demonstrates user activity logging for analytics
    /// </summary>
    private async Task DemonstrateUserActivityLoggingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("--- User Activity Logging ---");

        var activityCount = _configuration.GetValue<int>("SampleOperations:UserActivityTestCount", 1000);
        var activities = new List<UserActivity>();

        var activityTypes = new[] { "LOGIN", "LOGOUT", "VIEW_PRODUCT", "ADD_TO_CART", "CHECKOUT", "SEARCH" };
        var userIds = new[] { "user001", "user002", "user003", "user004", "user005" };

        // Generate test activity data
        for (int i = 0; i < activityCount; i++)
        {
            activities.Add(new UserActivity
            {
                UserId = userIds[Random.Shared.Next(userIds.Length)],
                ActivityType = activityTypes[Random.Shared.Next(activityTypes.Length)],
                Timestamp = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(1440)), // Last 24 hours
                IsSuccessful = Random.Shared.Next(0, 10) < 9, // 90% success rate
                SessionId = $"session_{Random.Shared.Next(1000, 9999)}",
                IPAddress = $"192.168.1.{Random.Shared.Next(1, 255)}",
                Details = $"Activity details for operation {i}"
            });
        }

        // Bulk insert activity logs
        var insertCommand = _dataService.CreateInsertCommand<UserActivity>();
        var activityStopwatch = Stopwatch.StartNew();

        foreach (var activity in activities)
        {
            await insertCommand.ExecuteAsync(activity, cancellationToken);
        }

        activityStopwatch.Stop();
        _logger.LogInformation("Inserted {ActivityCount} user activities in {ElapsedMs}ms", 
            activityCount, activityStopwatch.ElapsedMilliseconds);

        // Analyze the activity data
        var allActivities = await _dataService.CreateQueryCommand<UserActivity>()
            .ExecuteAsync(cancellationToken);

        var activityAnalytics = allActivities
            .GroupBy(a => a.ActivityType)
            .Select(g => new
            {
                ActivityType = g.Key,
                Count = g.Count(),
                SuccessRate = g.Count(a => a.IsSuccessful) * 100.0 / g.Count()
            })
            .OrderByDescending(x => x.Count);

        foreach (var analytics in activityAnalytics)
        {
            _logger.LogInformation("Activity Analytics - {ActivityType}: Count={Count}, " +
                "Success Rate={SuccessRate:F1}%",
                analytics.ActivityType, analytics.Count, analytics.SuccessRate);
        }

        // User activity by hour
        var hourlyActivity = allActivities
            .GroupBy(a => a.Timestamp.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour);

        _logger.LogInformation("Hourly activity distribution:");
        foreach (var hourData in hourlyActivity)
        {
            _logger.LogInformation("  Hour {Hour:D2}: {Count} activities", hourData.Hour, hourData.Count);
        }
    }
}