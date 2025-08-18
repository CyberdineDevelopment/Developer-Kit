using FractalDataWorks.Services.DataProvider.MsSql.Extensions;
using FractalDataWorks.Services.DataProvider.MsSql.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Samples.MsSqlProvider;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                // Add logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Register MsSql Data Provider using Enhanced Enum pattern
                services.AddMsSqlDataProvider(options =>
                {
                    options.ConnectionString = context.Configuration.GetConnectionString("SampleDb") 
                        ?? "Server=(localdb)\\MSSQLLocalDB;Database=SampleDb;Integrated Security=true;TrustServerCertificate=true;";
                    options.DefaultSchema = "dbo";
                    options.CommandTimeout = 30;
                    options.EnableDetailedErrors = context.HostingEnvironment.IsDevelopment();
                    options.EnableSensitiveDataLogging = context.HostingEnvironment.IsDevelopment();
                });

                // Register application services with different lifetimes
                services.AddScoped<IDataOperationsService, DataOperationsService>();
                services.AddScoped<IReportingService, ReportingService>();
                services.AddSingleton<IMetricsCollector, MetricsCollector>();
                services.AddTransient<IOrderProcessor, OrderProcessor>();

                // Add hosted services
                services.AddHostedService<DatabaseInitializationService>();
                services.AddHostedService<DataProcessingService>();
                services.AddHostedService<MetricsReportingService>();
                
                // Add the main application service
                services.AddHostedService<ApplicationService>();
            })
            .Build();

        // Run the host
        await host.RunAsync();
    }
}

// Main application service that demonstrates DI and service lifetimes
public class ApplicationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApplicationService> _logger;
    private readonly IMetricsCollector _metrics;
    private readonly IHostApplicationLifetime _lifetime;

    public ApplicationService(
        IServiceProvider serviceProvider,
        ILogger<ApplicationService> logger,
        IMetricsCollector metrics,
        IHostApplicationLifetime lifetime)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _metrics = metrics;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Application Service starting...");
        _logger.LogInformation("Press Ctrl+C to stop the application");

        var cycleCount = 0;
        
        while (!stoppingToken.IsCancellationRequested)
        {
            cycleCount++;
            
            try
            {
                // Create a scope for each iteration to demonstrate scoped services
                using var scope = _serviceProvider.CreateScope();
                
                _logger.LogInformation($"=== Starting operation cycle #{cycleCount} ===");
                
                // Get scoped services
                var dataOperations = scope.ServiceProvider.GetRequiredService<IDataOperationsService>();
                var reporting = scope.ServiceProvider.GetRequiredService<IReportingService>();
                
                // Get transient service (new instance each time)
                var processor1 = scope.ServiceProvider.GetRequiredService<IOrderProcessor>();
                var processor2 = scope.ServiceProvider.GetRequiredService<IOrderProcessor>();
                
                _logger.LogInformation($"Transient services are different instances: {!ReferenceEquals(processor1, processor2)}");
                
                // Perform operations
                var pendingCount = await dataOperations.GetPendingOrderCountAsync();
                _logger.LogInformation($"Found {pendingCount} pending orders");
                
                if (pendingCount > 0)
                {
                    await dataOperations.ProcessRecentOrdersAsync();
                    await processor1.ProcessNextBatchAsync();
                }
                
                await reporting.GenerateDailySummaryAsync();
                
                _metrics.RecordOperation("cycle_completed");
                
                _logger.LogInformation($"=== Operation cycle #{cycleCount} completed ===");

                // Wait before next cycle
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in application service cycle #{cycleCount}");
                _metrics.RecordError("cycle_error", ex);
                
                if (cycleCount > 10)
                {
                    _logger.LogCritical("Too many cycles with errors, shutting down");
                    _lifetime.StopApplication();
                    break;
                }
                
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("Application Service stopped");
    }
}

// Background service for continuous data processing
public class DataProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataProcessingService> _logger;
    private readonly IMetricsCollector _metrics;

    public DataProcessingService(
        IServiceProvider serviceProvider,
        ILogger<DataProcessingService> logger,
        IMetricsCollector metrics)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _metrics = metrics;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Processing Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessDataAsync();
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("Data Processing Service stopped");
    }

    private async Task ProcessDataAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dataProvider = scope.ServiceProvider.GetRequiredService<IMsSqlDataProvider>();
        
        try
        {
            _logger.LogDebug("Checking for data to process...");
            
            // Use transaction for consistency
            using var transaction = await dataProvider.BeginTransactionAsync();
            
            try
            {
                // Query for pending items
                var pendingOrders = await dataProvider.QueryAsync<dynamic>(
                    "SELECT TOP 5 OrderId, CustomerId, TotalAmount FROM sales.Orders WHERE Status = @Status",
                    new { Status = "Pending" },
                    transaction: transaction);

                var orderList = pendingOrders.ToList();
                if (orderList.Any())
                {
                    _logger.LogInformation($"Processing {orderList.Count} pending orders");
                    
                    foreach (var order in orderList)
                    {
                        // Update order status
                        await dataProvider.ExecuteAsync(
                            @"UPDATE sales.Orders 
                              SET Status = 'Processing', ProcessedDate = GETDATE() 
                              WHERE OrderId = @OrderId",
                            new { OrderId = order.OrderId },
                            transaction: transaction);
                        
                        _logger.LogDebug($"Order {order.OrderId} marked as processing");
                    }
                    
                    await transaction.CommitAsync();
                    _metrics.RecordOperation("orders_processed", orderList.Count);
                    _logger.LogInformation($"Successfully processed {orderList.Count} orders");
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing data");
            _metrics.RecordError("processing_error", ex);
        }
    }
}

// Metrics reporting service
public class MetricsReportingService : BackgroundService
{
    private readonly IMetricsCollector _metrics;
    private readonly ILogger<MetricsReportingService> _logger;

    public MetricsReportingService(
        IMetricsCollector metrics,
        ILogger<MetricsReportingService> logger)
    {
        _metrics = metrics;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics Reporting Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            
            var metrics = _metrics.GetMetrics();
            if (metrics.Any())
            {
                _logger.LogInformation("=== Metrics Report ===");
                foreach (var metric in metrics.OrderBy(m => m.Key))
                {
                    _logger.LogInformation($"  {metric.Key}: {metric.Value:N0}");
                }
                _logger.LogInformation("===================");
            }
        }

        _logger.LogInformation("Metrics Reporting Service stopped");
    }
}

// Service interfaces
public interface IDataOperationsService
{
    Task ProcessRecentOrdersAsync();
    Task<int> GetPendingOrderCountAsync();
}

public interface IReportingService
{
    Task GenerateDailySummaryAsync();
    Task<SummaryReport> GetSummaryAsync(DateTime date);
}

public interface IOrderProcessor
{
    Guid InstanceId { get; }
    Task ProcessNextBatchAsync();
}

public interface IMetricsCollector
{
    void RecordOperation(string operation, int count = 1);
    void RecordError(string operation, Exception ex);
    Dictionary<string, long> GetMetrics();
}

// Service implementations
public class DataOperationsService : IDataOperationsService
{
    private readonly IMsSqlDataProvider _dataProvider;
    private readonly ILogger<DataOperationsService> _logger;
    private readonly Guid _instanceId = Guid.NewGuid();

    public DataOperationsService(
        IMsSqlDataProvider dataProvider,
        ILogger<DataOperationsService> logger)
    {
        _dataProvider = dataProvider;
        _logger = logger;
        _logger.LogDebug($"DataOperationsService created with ID: {_instanceId}");
    }

    public async Task ProcessRecentOrdersAsync()
    {
        _logger.LogInformation("Processing recent orders...");

        var recentOrders = await _dataProvider.QueryAsync<dynamic>(
            @"SELECT TOP 5 o.OrderId, o.OrderDate, o.TotalAmount, c.CustomerName 
              FROM sales.Orders o
              JOIN sales.Customers c ON o.CustomerId = c.CustomerId
              WHERE o.OrderDate >= DATEADD(day, -7, GETDATE())
              ORDER BY o.OrderDate DESC");

        var orderList = recentOrders.ToList();
        _logger.LogInformation($"Found {orderList.Count} recent orders");

        foreach (var order in orderList)
        {
            _logger.LogInformation($"  Order {order.OrderId}: {order.CustomerName} - ${order.TotalAmount:F2}");
        }
    }

    public async Task<int> GetPendingOrderCountAsync()
    {
        return await _dataProvider.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sales.Orders WHERE Status = 'Pending'");
    }
}

public class ReportingService : IReportingService
{
    private readonly IMsSqlDataProvider _dataProvider;
    private readonly ILogger<ReportingService> _logger;
    private readonly Guid _instanceId = Guid.NewGuid();

    public ReportingService(
        IMsSqlDataProvider dataProvider,
        ILogger<ReportingService> logger)
    {
        _dataProvider = dataProvider;
        _logger = logger;
        _logger.LogDebug($"ReportingService created with ID: {_instanceId}");
    }

    public async Task GenerateDailySummaryAsync()
    {
        _logger.LogInformation("Generating daily summary report...");

        var summary = await GetSummaryAsync(DateTime.Today);

        _logger.LogInformation("╔══════════════════════════════════════╗");
        _logger.LogInformation("║         DAILY SUMMARY REPORT         ║");
        _logger.LogInformation("╠══════════════════════════════════════╣");
        _logger.LogInformation($"║ Date: {summary.ReportDate:yyyy-MM-dd}                   ║");
        _logger.LogInformation($"║ Total Orders: {summary.TotalOrders,23} ║");
        _logger.LogInformation($"║ Total Revenue: {summary.TotalRevenue,22:C} ║");
        _logger.LogInformation($"║ Avg Order Value: {summary.AverageOrderValue,20:C} ║");
        _logger.LogInformation($"║ Unique Customers: {summary.UniqueCustomers,19} ║");
        _logger.LogInformation("╚══════════════════════════════════════╝");
    }

    public async Task<SummaryReport> GetSummaryAsync(DateTime date)
    {
        var data = await _dataProvider.QuerySingleOrDefaultAsync<SummaryReport>(
            @"SELECT 
                CAST(@Date as DATE) as ReportDate,
                COUNT(*) as TotalOrders,
                ISNULL(SUM(TotalAmount), 0) as TotalRevenue,
                ISNULL(AVG(TotalAmount), 0) as AverageOrderValue,
                COUNT(DISTINCT CustomerId) as UniqueCustomers
              FROM sales.Orders
              WHERE CAST(OrderDate as DATE) = CAST(@Date as DATE)",
            new { Date = date });

        return data ?? new SummaryReport { ReportDate = date };
    }
}

// Transient service - new instance each time
public class OrderProcessor : IOrderProcessor
{
    private readonly IMsSqlDataProvider _dataProvider;
    private readonly ILogger<OrderProcessor> _logger;
    
    public Guid InstanceId { get; }

    public OrderProcessor(
        IMsSqlDataProvider dataProvider,
        ILogger<OrderProcessor> logger)
    {
        InstanceId = Guid.NewGuid();
        _dataProvider = dataProvider;
        _logger = logger;
        _logger.LogDebug($"OrderProcessor created with ID: {InstanceId}");
    }

    public async Task ProcessNextBatchAsync()
    {
        _logger.LogInformation($"Processing batch with processor {InstanceId:N}");
        
        var processed = await _dataProvider.ExecuteAsync(
            @"UPDATE TOP (3) sales.Orders 
              SET Status = 'Completed', 
                  ProcessedDate = GETDATE() 
              WHERE Status = 'Processing' 
                AND ProcessedDate < DATEADD(second, -30, GETDATE())",
            null);

        if (processed > 0)
        {
            _logger.LogInformation($"Processor {InstanceId:N} completed {processed} orders");
        }
    }
}

public class SummaryReport
{
    public DateTime ReportDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int UniqueCustomers { get; set; }
}

// Singleton metrics collector
public class MetricsCollector : IMetricsCollector
{
    private readonly Dictionary<string, long> _metrics = new();
    private readonly ILogger<MetricsCollector> _logger;
    private readonly object _lock = new();

    public MetricsCollector(ILogger<MetricsCollector> logger)
    {
        _logger = logger;
        _logger.LogInformation("MetricsCollector singleton created");
    }

    public void RecordOperation(string operation, int count = 1)
    {
        lock (_lock)
        {
            if (!_metrics.ContainsKey(operation))
                _metrics[operation] = 0;
            
            _metrics[operation] += count;
        }
        
        _logger.LogDebug($"Metric recorded: {operation} +{count}");
    }

    public void RecordError(string operation, Exception ex)
    {
        var errorKey = $"{operation}_errors";
        RecordOperation(errorKey);
        _logger.LogWarning($"Error recorded for {operation}: {ex.GetType().Name}");
    }

    public Dictionary<string, long> GetMetrics()
    {
        lock (_lock)
        {
            return new Dictionary<string, long>(_metrics);
        }
    }
}

// Database initialization service
public class DatabaseInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("═══════════════════════════════════════════════════");
        _logger.LogInformation("    DATABASE INITIALIZATION SERVICE STARTING");
        _logger.LogInformation("═══════════════════════════════════════════════════");

        using var scope = _serviceProvider.CreateScope();
        var dataProvider = scope.ServiceProvider.GetRequiredService<IMsSqlDataProvider>();

        try
        {
            // Test connection
            var dbName = await dataProvider.ExecuteScalarAsync<string>("SELECT DB_NAME()");
            _logger.LogInformation($"✓ Connected to database: {dbName}");

            // Verify schemas
            var schemas = await dataProvider.QueryAsync<string>(
                "SELECT DISTINCT TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA IN ('sales', 'inventory', 'users')");
            
            var schemaList = schemas.ToList();
            _logger.LogInformation($"✓ Found {schemaList.Count} application schemas: {string.Join(", ", schemaList)}");

            // Get table counts
            var tables = await dataProvider.QueryAsync<dynamic>(
                @"SELECT TABLE_SCHEMA, COUNT(*) as TableCount 
                  FROM INFORMATION_SCHEMA.TABLES 
                  WHERE TABLE_SCHEMA IN ('sales', 'inventory', 'users')
                  GROUP BY TABLE_SCHEMA");

            foreach (var schema in tables)
            {
                _logger.LogInformation($"  • {schema.TABLE_SCHEMA}: {schema.TableCount} tables");
            }

            // Get data metrics
            var metrics = await dataProvider.QuerySingleAsync<dynamic>(
                @"SELECT 
                    (SELECT COUNT(*) FROM sales.Orders) as OrderCount,
                    (SELECT COUNT(*) FROM sales.Customers) as CustomerCount,
                    (SELECT COUNT(*) FROM inventory.Products) as ProductCount");

            _logger.LogInformation($"✓ Database contains:");
            _logger.LogInformation($"  • {metrics.OrderCount} orders");
            _logger.LogInformation($"  • {metrics.CustomerCount} customers");
            _logger.LogInformation($"  • {metrics.ProductCount} products");

            // Reset some orders to pending for demo
            var reset = await dataProvider.ExecuteAsync(
                "UPDATE sales.Orders SET Status = 'Pending', ProcessedDate = NULL WHERE Status IN ('Processing', 'Completed')");
            
            if (reset > 0)
            {
                _logger.LogInformation($"✓ Reset {reset} orders to 'Pending' status for demo");
            }

            _logger.LogInformation("═══════════════════════════════════════════════════");
            _logger.LogInformation("    DATABASE INITIALIZATION COMPLETED");
            _logger.LogInformation("═══════════════════════════════════════════════════");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database initialization service stopped");
        return Task.CompletedTask;
    }
}