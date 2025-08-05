# FractalDataWorks.Framework.Abstractions

The **FractalDataWorks.Framework.Abstractions** package provides the core abstraction layer for the FractalDataWorks Framework, a service-oriented SDK that handles infrastructure complexity while allowing developers to focus on business logic. This package defines the fundamental interfaces, base classes, and result patterns used throughout the framework ecosystem.

## Overview

This abstraction layer provides:

- **Unified Service Pattern** - Common interfaces and base classes for all framework services
- **Result Pattern** - Consistent error handling with `IFdwResult<T>` and `IFdwResult`
- **Configuration Management** - Base classes for type-safe configuration objects
- **Service Discovery** - Integration with EnhancedEnums for automatic service registration
- **Lifecycle Management** - Health checking, initialization, and disposal patterns

## Quick Start

### Creating a Simple Service

```csharp
using FractalDataWorks.Framework.Abstractions;

// Define your service configuration
public sealed class MyServiceConfiguration : FdwConfigurationBase
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(ConnectionString))
            errors.Add("ConnectionString is required");
            
        if (Timeout <= 0)
            errors.Add("Timeout must be greater than zero");
            
        return errors;
    }
}

// Implement your service
public sealed class MyService : FdwServiceBase<MyServiceConfiguration>
{
    public MyService() : base("my-service", "My Business Service")
    {
    }
    
    protected override async Task<IFdwResult> PerformInitializationAsync(MyServiceConfiguration configuration)
    {
        // Initialize your service with the validated configuration
        // Configuration is guaranteed to be valid when this method is called
        
        try
        {
            // Setup connections, validate external dependencies, etc.
            await SetupConnectionAsync(configuration.ConnectionString);
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure($"Failed to initialize service: {ex.Message}", ex);
        }
    }
    
    protected override async Task<IFdwResult> PerformHealthCheckAsync()
    {
        try
        {
            // Check if your service is healthy
            var isHealthy = await CheckConnectionHealthAsync();
            return isHealthy 
                ? FdwResult.Success() 
                : FdwResult.Failure("Service is unhealthy");
        }
        catch (Exception ex)
        {
            return FdwResult.Failure($"Health check failed: {ex.Message}", ex);
        }
    }
    
    public async Task<IFdwResult<string>> ProcessDataAsync(string input)
    {
        if (!IsAvailable)
            return FdwResult<string>.Failure("Service is not available");
            
        try
        {
            var result = await DoBusinessLogicAsync(input);
            return FdwResult<string>.Success(result);
        }
        catch (ArgumentException ex)
        {
            return FdwResult<string>.Failure("Invalid input provided", ex);
        }
        catch (Exception ex)
        {
            return FdwResult<string>.Failure("Processing failed", ex);
        }
    }
    
    private async Task SetupConnectionAsync(string connectionString) { /* Implementation */ }
    private async Task<bool> CheckConnectionHealthAsync() { /* Implementation */ return true; }
    private async Task<string> DoBusinessLogicAsync(string input) { /* Implementation */ return input.ToUpper(); }
}
```

### Using the Service

```csharp
// Create and initialize the service
var service = new MyService();
var config = new MyServiceConfiguration 
{ 
    ConnectionString = "Server=localhost;Database=MyDb;",
    Timeout = 60 
};

var initResult = await service.InitializeAsync(config);
if (initResult.IsFailure)
{
    Console.WriteLine($"Initialization failed: {initResult.ErrorMessage}");
    return;
}

// Use the service
var result = await service.ProcessDataAsync("hello world");
if (result.IsSuccess)
{
    Console.WriteLine($"Result: {result.Value}");
}
else
{
    Console.WriteLine($"Processing failed: {result.ErrorMessage}");
}

// Health checking
var healthResult = await service.HealthCheckAsync();
Console.WriteLine($"Service health: {(healthResult.IsSuccess ? "Healthy" : "Unhealthy")}");
```

## Core Concepts

### IFdwResult Pattern

The framework uses a consistent result pattern to handle success/failure scenarios:

```csharp
// Non-generic result for operations that don't return data
public async Task<IFdwResult> DeleteUserAsync(string userId)
{
    try
    {
        await _repository.DeleteAsync(userId);
        return FdwResult.Success();
    }
    catch (UserNotFoundException)
    {
        return FdwResult.Failure("User not found");
    }
    catch (Exception ex)
    {
        return FdwResult.Failure("Delete operation failed", ex);
    }
}

// Generic result for operations that return data
public async Task<IFdwResult<User>> GetUserAsync(string userId)
{
    try
    {
        var user = await _repository.GetAsync(userId);
        return user != null 
            ? FdwResult<User>.Success(user)
            : FdwResult<User>.Failure("User not found");
    }
    catch (Exception ex)
    {
        return FdwResult<User>.Failure("Failed to retrieve user", ex);
    }
}

// Safe result handling
var userResult = await GetUserAsync("user123");
if (userResult.TryGetValue(out var user))
{
    Console.WriteLine($"Found user: {user.Name}");
}
else
{
    Console.WriteLine($"Error: {userResult.ErrorMessage}");
    if (userResult.Exception != null)
    {
        _logger.LogError(userResult.Exception, "User retrieval failed");
    }
}
```

### Service Types and Auto-Discovery

The framework integrates with EnhancedEnums for automatic service discovery:

```csharp
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Framework.Abstractions;

// Define a service type for auto-discovery
[EnumCollection("BusinessServices")]
public abstract class BusinessServiceType : ServiceTypeBase<IBusinessService>
{
    protected BusinessServiceType(int id, string name, Type serviceType) 
        : base(id, name, serviceType, "Business")
    {
    }
}

// Concrete service types are auto-discovered
public sealed class UserServiceType : BusinessServiceType
{
    public static readonly UserServiceType Instance = new();
    
    private UserServiceType() : base(1, "User Service", typeof(UserService))
    {
    }
    
    public override IBusinessService CreateService(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<UserService>();
    }
    
    public override void RegisterService(IServiceCollection services)
    {
        services.AddScoped<UserService>();
        services.AddScoped<IBusinessService>(sp => sp.GetRequiredService<UserService>());
    }
}

// Framework automatically generates a collection of all service types
// BusinessServices.All will contain all discovered service types
```

## Implementation Examples

### E-commerce Order Service

```csharp
public sealed class OrderServiceConfiguration : FdwConfigurationBase
{
    public string DatabaseConnectionString { get; set; } = string.Empty;
    public string PaymentApiUrl { get; set; } = string.Empty;
    public string InventoryApiUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(DatabaseConnectionString))
            errors.Add("Database connection string is required");
            
        if (string.IsNullOrWhiteSpace(PaymentApiUrl))
            errors.Add("Payment API URL is required");
            
        if (string.IsNullOrWhiteSpace(InventoryApiUrl))
            errors.Add("Inventory API URL is required");
            
        if (TimeoutSeconds <= 0)
            errors.Add("Timeout must be positive");
            
        return errors;
    }
}

public sealed class OrderService : FdwServiceBase<OrderServiceConfiguration>
{
    private readonly IPaymentClient _paymentClient;
    private readonly IInventoryClient _inventoryClient;
    private readonly IOrderRepository _orderRepository;
    
    public OrderService(
        IPaymentClient paymentClient,
        IInventoryClient inventoryClient,
        IOrderRepository orderRepository) 
        : base("order-service", "E-commerce Order Service")
    {
        _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
        _inventoryClient = inventoryClient ?? throw new ArgumentNullException(nameof(inventoryClient));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }
    
    protected override async Task<IFdwResult> PerformInitializationAsync(OrderServiceConfiguration configuration)
    {
        try
        {
            // Initialize database connection
            await _orderRepository.InitializeAsync(configuration.DatabaseConnectionString);
            
            // Test external API connections
            var paymentHealth = await _paymentClient.HealthCheckAsync();
            if (!paymentHealth)
                return FdwResult.Failure("Payment API is unavailable");
                
            var inventoryHealth = await _inventoryClient.HealthCheckAsync();
            if (!inventoryHealth)
                return FdwResult.Failure("Inventory API is unavailable");
                
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("Service initialization failed", ex);
        }
    }
    
    protected override async Task<IFdwResult> PerformHealthCheckAsync()
    {
        var errors = new List<string>();
        
        try
        {
            // Check database connectivity
            if (!await _orderRepository.IsHealthyAsync())
                errors.Add("Database connection is unhealthy");
                
            // Check external services
            if (!await _paymentClient.HealthCheckAsync())
                errors.Add("Payment service is unavailable");
                
            if (!await _inventoryClient.HealthCheckAsync())
                errors.Add("Inventory service is unavailable");
                
            return errors.Count == 0 
                ? FdwResult.Success()
                : FdwResult.Failure("Health check failed", errors);
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("Health check error", ex);
        }
    }
    
    public async Task<IFdwResult<Order>> CreateOrderAsync(CreateOrderRequest request)
    {
        if (!IsAvailable)
            return FdwResult<Order>.Failure("Order service is not available");
            
        try
        {
            // Validate inventory
            var inventoryResult = await _inventoryClient.CheckAvailabilityAsync(request.Items);
            if (!inventoryResult.IsSuccess)
                return FdwResult<Order>.Failure("Inventory check failed", inventoryResult.ErrorDetails);
            
            // Process payment
            var paymentResult = await _paymentClient.ProcessPaymentAsync(request.PaymentInfo);
            if (!paymentResult.IsSuccess)
                return FdwResult<Order>.Failure("Payment processing failed", paymentResult.ErrorDetails);
            
            // Create order
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                CustomerId = request.CustomerId,
                Items = request.Items,
                PaymentId = paymentResult.Value.PaymentId,
                Status = OrderStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };
            
            await _orderRepository.SaveAsync(order);
            
            return FdwResult<Order>.Success(order);
        }
        catch (Exception ex)
        {
            return FdwResult<Order>.Failure("Order creation failed", ex);
        }
    }
}
```

### Multi-tenant SaaS Service

```csharp
public sealed class TenantServiceConfiguration : FdwConfigurationBase
{
    public Dictionary<string, string> TenantConnectionStrings { get; set; } = new();
    public string DefaultTenant { get; set; } = string.Empty;
    public bool EnableTenantIsolation { get; set; } = true;
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (TenantConnectionStrings.Count == 0)
            errors.Add("At least one tenant connection string is required");
            
        if (string.IsNullOrWhiteSpace(DefaultTenant))
            errors.Add("Default tenant is required");
            
        if (!TenantConnectionStrings.ContainsKey(DefaultTenant))
            errors.Add("Default tenant must have a connection string");
            
        return errors;
    }
}

public sealed class TenantService : FdwServiceBase<TenantServiceConfiguration>
{
    private readonly Dictionary<string, IDataProvider> _tenantProviders = new();
    private readonly IServiceProvider _serviceProvider;
    
    public TenantService(IServiceProvider serviceProvider) 
        : base("tenant-service", "Multi-tenant SaaS Service")
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    
    protected override async Task<IFdwResult> PerformInitializationAsync(TenantServiceConfiguration configuration)
    {
        try
        {
            // Initialize data providers for each tenant
            foreach (var (tenantId, connectionString) in configuration.TenantConnectionStrings)
            {
                var provider = _serviceProvider.GetRequiredService<IDataProvider>();
                var providerConfig = new DataProviderConfiguration { ConnectionString = connectionString };
                
                var initResult = await provider.InitializeAsync(providerConfig);
                if (initResult.IsFailure)
                    return FdwResult.Failure($"Failed to initialize provider for tenant {tenantId}: {initResult.ErrorMessage}");
                    
                _tenantProviders.Add(tenantId, provider);
            }
            
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("Tenant service initialization failed", ex);
        }
    }
    
    public async Task<IFdwResult<TData>> GetTenantDataAsync<TData>(string tenantId, string query)
    {
        if (!IsAvailable)
            return FdwResult<TData>.Failure("Tenant service is not available");
            
        if (!_tenantProviders.TryGetValue(tenantId, out var provider))
            return FdwResult<TData>.Failure($"No provider configured for tenant {tenantId}");
            
        try
        {
            var command = new QueryCommand<TData>(query);
            var result = await provider.Execute(command);
            
            return result.IsSuccess 
                ? FdwResult<TData>.Success(result.Value)
                : FdwResult<TData>.Failure(result.ErrorMessage, result.ErrorDetails, result.Exception);
        }
        catch (Exception ex)
        {
            return FdwResult<TData>.Failure("Tenant data retrieval failed", ex);
        }
    }
}
```

## Configuration Examples

### JSON Configuration

```json
{
  "Services": {
    "OrderService": {
      "DatabaseConnectionString": "Server=localhost;Database=ECommerce;Trusted_Connection=true;",
      "PaymentApiUrl": "https://payments.api.company.com/v1",
      "InventoryApiUrl": "https://inventory.api.company.com/v1",
      "TimeoutSeconds": 45
    },
    "TenantService": {
      "TenantConnectionStrings": {
        "tenant1": "Server=tenant1-db;Database=App;Trusted_Connection=true;",
        "tenant2": "Server=tenant2-db;Database=App;Trusted_Connection=true;",
        "tenant3": "Server=tenant3-db;Database=App;Trusted_Connection=true;"
      },
      "DefaultTenant": "tenant1",
      "EnableTenantIsolation": true
    }
  }
}
```

### Dependency Injection Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register configuration
    services.Configure<OrderServiceConfiguration>(
        Configuration.GetSection("Services:OrderService"));
    services.Configure<TenantServiceConfiguration>(
        Configuration.GetSection("Services:TenantService"));
    
    // Register dependencies
    services.AddScoped<IPaymentClient, PaymentClient>();
    services.AddScoped<IInventoryClient, InventoryClient>();
    services.AddScoped<IOrderRepository, SqlOrderRepository>();
    services.AddScoped<IDataProvider, SqlDataProvider>();
    
    // Register services
    services.AddScoped<OrderService>();
    services.AddScoped<TenantService>();
    
    // Framework services can be auto-registered via service discovery
    services.AddFractalDataWorksServices(); // Extension method from framework
}
```

## Advanced Usage

### Custom Result Types

```csharp
// Create domain-specific result extensions
public static class OrderResultExtensions
{
    public static IFdwResult<Order> OrderNotFound(string orderId)
    {
        return FdwResult<Order>.Failure($"Order {orderId} was not found");
    }
    
    public static IFdwResult<Order> InsufficientInventory(IReadOnlyList<string> unavailableItems)
    {
        return FdwResult<Order>.Failure(
            "Insufficient inventory for order", 
            unavailableItems.Select(item => $"Item '{item}' is out of stock").ToList());
    }
    
    public static IFdwResult<Order> PaymentDeclined(string reason)
    {
        return FdwResult<Order>.Failure($"Payment was declined: {reason}");
    }
}

// Usage in service methods
public async Task<IFdwResult<Order>> GetOrderAsync(string orderId)
{
    var order = await _repository.GetByIdAsync(orderId);
    return order != null 
        ? FdwResult<Order>.Success(order)
        : OrderResultExtensions.OrderNotFound(orderId);
}
```

### Service Composition

```csharp
public sealed class CompositeBusinessService : FdwServiceBase
{
    private readonly IReadOnlyList<IFdwService> _childServices;
    
    public CompositeBusinessService(IEnumerable<IFdwService> childServices)
        : base("composite-service", "Composite Business Service")
    {
        _childServices = childServices?.ToList() ?? throw new ArgumentNullException(nameof(childServices));
    }
    
    protected override async Task<IFdwResult> PerformHealthCheckAsync()
    {
        var errors = new List<string>();
        
        foreach (var service in _childServices)
        {
            var healthResult = await service.HealthCheckAsync();
            if (healthResult.IsFailure)
            {
                errors.Add($"Service {service.ServiceName}: {healthResult.ErrorMessage}");
            }
        }
        
        return errors.Count == 0 
            ? FdwResult.Success()
            : FdwResult.Failure("One or more child services are unhealthy", errors);
    }
    
    public override bool IsAvailable => 
        base.IsAvailable && _childServices.All(s => s.IsAvailable);
}
```

## Integration with Other Framework Components

This abstraction layer works seamlessly with other FractalDataWorks packages:

- **ExternalConnections**: Use `IFdwResult<IExternalConnection>` for connection operations
- **DataProviders**: Commands return `IFdwResult<T>` for consistent error handling
- **Authentication**: Authentication operations use `IFdwResult<IAuthenticationToken>`
- **SecretManagement**: Secret retrieval returns `IFdwResult<SecretValue>`
- **Transformations**: Transform results use `IFdwResult<TransformationResult>`
- **Scheduling**: Task execution results use `IFdwResult<TaskResult>`

## Best Practices

1. **Always use IFdwResult** for operations that can fail
2. **Validate configurations** thoroughly in the `Validate()` method
3. **Implement proper health checks** that verify external dependencies
4. **Use meaningful error messages** and include relevant details
5. **Handle exceptions gracefully** and wrap them in FdwResult
6. **Check IsAvailable** before performing operations
7. **Dispose services properly** to clean up resources
8. **Use typed configurations** to ensure compile-time safety
9. **Leverage service discovery** for automatic registration
10. **Compose services** to build complex business logic from simple components

## License

This package is part of the FractalDataWorks Framework and is licensed under the Apache 2.0 License.