# FractalDataWorks.Services

✅ **STABLE** - Complete Enhanced Enum Type Factories implementation with service discovery and registration

Service patterns and base implementations for the FractalDataWorks framework. This package provides the consolidated service and message infrastructure, including service abstractions, base classes, and message types that simplify service development with built-in validation, logging, and error handling.

## Overview

FractalDataWorks.Services provides:
- Base service implementation with automatic validation
- Universal data service for all data operations
- Built-in command validation and execution pipeline
- Comprehensive logging and error handling
- Health check support
- **Consolidated service and message infrastructure** with Enhanced Enum support
- **ServiceTypeBase<T>** - Base class for service types with Enhanced Enum attributes
- **MessageBase<T>** - Base class for result messages (moved from separate Messages project)
- **Service and Message attributes** for discovery and cross-assembly generation

## Key Components

### ServiceTypeBase<T> and MessageBase<T>

The consolidated Services package now contains all service-related infrastructure:

```csharp
// Service type definition
[ServiceType]
public class EmailServiceType : ServiceTypeBase<INotificationService>
{
    public EmailServiceType() : base(1, "EmailService", "Email notification service")
    {
    }

    // Factory method for creating service instances
    public override INotificationService CreateService(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<EmailNotificationService>();
    }
}

// Message definition - for result values in FdwResult<T>, not logging
[Message]
public class InvalidEmailAddress : MessageBase<string>
{
    public InvalidEmailAddress() : base(2001, "InvalidEmailAddress", "The email address '{0}' is not valid", MessageSeverity.Error)
    {
    }
}
```

#### Key Points:
- **Messages are for result values** in `FdwResult<T>`, not for logging
- **ServiceTypeBase<T>** enables Enhanced Enum patterns for service discovery
- **MessageBase<T>** provides structured, type-safe error and success messages
- **Attributes** enable automatic discovery and cross-assembly generation

### ServiceBase<TCommand, TConfiguration, TService>

The primary base class for implementing services:

```csharp
public abstract class ServiceBase<TCommand, TConfiguration, TService> : IFdwService<TCommand>
    where TConfiguration : IFdwConfiguration
    where TCommand : ICommand
    where TService : class
{
    protected ServiceBase(ILogger<TService> logger, TConfiguration configuration);
    
    // Override this to implement your service logic
    protected abstract Task<IFdwResult<T>> ExecuteCore<T>(TCommand command);
}
```

#### Features
- **Automatic Configuration Validation**: Validates configurations on startup
- **Command Validation**: Validates commands before execution
- **Correlation ID Tracking**: Automatic correlation ID for request tracking
- **Performance Logging**: Logs execution duration
- **Error Handling**: Consistent error handling and logging
- **Health Checks**: Built-in health status based on configuration validity

### Universal Data Service Pattern

The framework uses a universal data service pattern through the DataProvider abstractions:

```csharp
// DataProvider service handles all data operations
public class DataProviderService : IDataProvider
{
    private readonly IExternalDataConnectionProvider _connectionProvider;
    private readonly ILogger<DataProviderService> _logger;
    
    public DataProviderService(
        IExternalDataConnectionProvider connectionProvider,
        ILogger<DataProviderService> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
    }
    
    public async Task<IFdwResult<TResult>> Execute<TResult>(DataCommandBase command)
    {
        // Provider selects appropriate connection based on command configuration
        var connection = await _connectionProvider.GetConnection(command.DataStoreConfiguration);
        return await connection.Execute<TResult>(command);
    }
}
```

#### Key Features
- **Provider Agnostic**: Works with any data source (SQL, NoSQL, APIs, Files)
- **Universal Commands**: Uses LINQ-like expressions that get transformed by providers
- **Command Transformation**: Each provider's CommandBuilder handles the transformation
- **Single Service**: One data service handles all data operations across all providers

## Usage Examples

### Creating a Service

```csharp
public class CustomerService : ServiceBase<CustomerCommand, CustomerConfiguration, CustomerService>
{
    private readonly ICustomerRepository _repository;
    
    public CustomerService(
        ILogger<CustomerService> logger, 
        CustomerConfiguration configuration,
        ICustomerRepository repository)
        : base(logger, configuration)
    {
        _repository = repository;
    }
    
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(CustomerCommand command)
    {
        // Your service implementation here
        // Validation has already been performed
        // Configuration is available via this.Configuration
        
        return command switch
        {
            GetCustomerCommand getCmd => await GetCustomer<T>(getCmd),
            CreateCustomerCommand createCmd => await CreateCustomer<T>(createCmd),
            _ => FdwResult<T>.Failure(ServiceMessages.InvalidCommand.Format(command.GetType().Name))
        };
    }
    
    private async Task<IFdwResult<T>> GetCustomer<T>(GetCustomerCommand command)
    {
        var customer = await _repository.GetAsync(command.CustomerId);
        if (customer == null)
        {
            return FdwResult<T>.Failure(
                ServiceMessages.RecordNotFound.Format("Customer", command.CustomerId));
        }
        
        return FdwResult<T>.Success((T)(object)customer);
    }
}
```

### Using the Universal Data Service

```csharp
// Configure data service with multiple providers
services.AddScoped<IDataProvider, DataProviderService>();
services.AddScoped<IExternalDataConnectionProvider, ExternalDataConnectionProvider>();

// Query example - works with any data source
var queryCommand = new QueryCommand<Customer>
{
    DataStoreConfiguration = new DataStoreConfiguration
    {
        DataStoreType = "SqlServer",
        ConnectionString = "...",
        ContainerName = "Customers"
    },
    WhereClause = customer => customer.City == "Seattle"
};

var result = await dataProvider.Execute<IEnumerable<Customer>>(queryCommand);
```

### Configuration Management

The current ServiceBase implementation uses direct configuration injection. Each service receives a single configuration instance through its constructor:

```csharp
public class CustomerService : ServiceBase<CustomerCommand, CustomerConfiguration, CustomerService>
{
    public CustomerService(ILogger<CustomerService> logger, CustomerConfiguration configuration)
        : base(logger, configuration)
    {
        // Configuration is now available through the base class
    }
}
```

### Using the Service

```csharp
// In your DI container setup
services.AddSingleton<CustomerConfiguration>(sp =>
{
    return new CustomerConfiguration { Id = 1, Name = "Default", IsEnabled = true, ConnectionString = "..." };
});

services.AddScoped<CustomerService>();

// Using the service
var command = new GetCustomerCommand { CustomerId = 123 };
var result = await customerService.Execute<Customer>(command);

if (result.IsSuccess)
{
    return Ok(result.Value);
}
else
{
    return BadRequest(result.Error);
}
```

## Service Lifecycle

1. **Construction**: Service validates it has at least one valid configuration
2. **Command Receipt**: Service receives a command to execute
3. **Command Validation**: Automatic validation of command and its configuration
4. **Execution**: Your ExecuteCore implementation is called
5. **Result**: Consistent result pattern with success/failure
6. **Logging**: Automatic logging of execution time and results

## Built-in Logging with Serilog

The service automatically logs using structured logging with Serilog integration:

### Standard Log Events
- `ServiceStarted` - When service initializes successfully
- `InvalidConfiguration` - When configuration validation fails
- `InvalidCommand` - When command validation fails
- `CommandExecuted` - When command executes successfully
- `CommandFailed` - When command execution fails
- `OperationFailed` - When an exception occurs

### Structured Logging Examples

```csharp
// Use Serilog destructuring for complex objects
ServiceBaseLog.CommandExecutedWithContext(logger, command);
// Logs: "Command executed with detailed context {@Command}"

ServiceBaseLog.ConfigurationValidated(logger, configuration);
// Logs: "Service configuration validated {@Configuration}"

ServiceBaseLog.PerformanceMetrics(logger, new PerformanceMetrics(
    Duration: 150.5,
    ItemsProcessed: 1000,
    OperationType: "BatchProcess"));
// Logs: "Performance metrics available {@Metrics}"

ServiceBaseLog.ServiceOperationCompleted(logger, "DataProcessing", 250.0, result, context);
// Logs: "Service operation DataProcessing completed in 250ms {@Result} {@Context}"
```

### Serilog Configuration

Configure Serilog in your application startup:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day, 
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.WithProperty("Application", "FractalDataWorks")
    .CreateLogger();

services.AddLogging(builder => builder.AddSerilog());
```

### Custom Structured Logging

Add custom structured logging in your services:

```csharp
protected override async Task<FdwResult<T>> ExecuteCore<T>(CustomerCommand command)
{
    logger.LogInformation("Processing customer command {@Command} for tenant {TenantId}", 
        command, Configuration.TenantId);
    
    var result = await ProcessCommand(command);
    
    logger.LogInformation("Customer operation completed with result {@Result}", 
        new { Success = result.IsSuccess, RecordsAffected = result.Value });
    
    return result;
}
```

## Health Checks

Services implement `IsHealthy` which returns true when:
- At least one configuration is available
- The primary configuration is valid
- The primary configuration is enabled

## Error Handling

The service base class provides comprehensive error handling:
- Validation errors return failure results with descriptive messages
- Exceptions are caught and logged (except OutOfMemoryException)
- All errors use the consistent FdwResult pattern
- Correlation IDs track requests through the system

## Cross-Assembly Discovery

The Services package supports optional cross-assembly discovery for service types and messages:

### Option 1: Automatic Discovery (Recommended)

```xml
<!-- Basic services -->
<PackageReference Include="FractalDataWorks.Services" />

<!-- With automatic cross-assembly discovery -->
<PackageReference Include="FractalDataWorks.ServiceTypes.CrossAssembly" />
```

This automatically generates static collections:

```csharp
// Automatically generated collections
var allServiceTypes = ServiceTypes.All;
var emailService = ServiceTypes.GetByName("EmailService");
var serviceById = ServiceTypes.GetById(1);

var allMessages = Messages.All;
var invalidEmail = Messages.GetByName("InvalidEmailAddress");
```

### Option 2: Manual Collection Creation

You can always create collections manually without source generators:

```csharp
public static class MyServiceTypes
{
    public static readonly List<ServiceTypeBase> All = new()
    {
        new EmailServiceType(),
        new SmsServiceType(),
        new PushNotificationServiceType(),
    };
    
    public static ServiceTypeBase GetByName(string name) 
        => All.FirstOrDefault(s => s.Name == name);
}

public static class MyMessages
{
    public static readonly List<MessageBase> All = new()
    {
        new InvalidEmailAddress(),
        new EmailSentSuccessfully(),
        new SmsSentSuccessfully(),
    };
}
```

## Installation

```xml
<!-- Basic services -->
<PackageReference Include="FractalDataWorks.Services" />

<!-- Optional: Add cross-assembly discovery -->
<PackageReference Include="FractalDataWorks.ServiceTypes.CrossAssembly" />
```

## Dependencies

- FractalDataWorks.Configuration (configuration abstractions)
- FractalDataWorks.Configuration
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options
- Microsoft.Extensions.DependencyInjection.Abstractions
- FluentValidation

## Best Practices

1. **Keep ExecuteCore Focused**: Implement only business logic, let the base class handle infrastructure
2. **Use Configuration Registry**: Manage multiple configurations for different environments/tenants
3. **Leverage Built-in Validation**: Don't duplicate validation that the base class provides
4. **Return Appropriate Results**: Use FdwResult for consistent error handling
5. **Log Sparingly**: The base class provides comprehensive logging already
6. **Use DataConnection for Data**: Don't create domain-specific data services, use the universal DataConnection
7. **Provider Agnostic Commands**: Write queries using universal expressions that work across providers

## Enhanced Enum Type Factories

✅ **IMPLEMENTED** - Enhanced Enum Type Factories provide complete service type registration with compile-time safety

### Overview

The Enhanced Enum Type Factories pattern uses source generators to create strongly-typed service registrations:

```csharp
[EnumOption(1, "EmailService", "Email notification service")]
public class EmailServiceType : ServiceTypeBase<INotificationService, EmailConfiguration>
{
    public EmailServiceType() : base(1, "EmailService", "Email notification service")
    {
    }

    public override object Create(EmailConfiguration configuration)
    {
        return new EmailService(configuration);
    }

    public override Task<INotificationService> GetService(string configurationName)
    {
        // Implementation to retrieve service by configuration name
    }

    public override Task<INotificationService> GetService(int configurationId)
    {
        // Implementation to retrieve service by configuration ID
    }
}
```

### ServiceTypeBase Pattern

The new pattern introduces two base classes:
- **ServiceTypeFactoryBase<TService, TConfiguration>**: Non-generic base with factory methods (no Enhanced Enum attributes)
- **ServiceTypeBase<TService, TConfiguration>**: Enhanced Enum base with `[EnhancedEnumBase]` attribute

### Benefits

1. **Compile-time Safety**: Service types are generated at compile time
2. **IntelliSense Support**: Full IDE support for ServiceTypes.* collections
3. **Automatic DI Registration**: Services are automatically registered with dependency injection
4. **Factory Pattern**: Each service type acts as a factory for creating service instances

### Usage with Dependency Injection

```csharp
// Register all service types in an assembly
services.AddServiceTypes(Assembly.GetExecutingAssembly());

// Service types are registered as both themselves and their factory interfaces
var emailFactory = provider.GetService<IServiceFactory<INotificationService, EmailConfiguration>>();
var service = emailFactory.Create(emailConfig);
```

### Generated Collections

Enhanced Enums generates static collections for easy access:

```csharp
// Access all service types
var allServices = ServiceTypes.All;

// Get by ID
var emailService = ServiceTypes.GetById(1);

// Get by name
var smsService = ServiceTypes.GetByName("SmsService");

// Iterate through all
foreach (var serviceType in ServiceTypes.All)
{
    Console.WriteLine($"{serviceType.Id}: {serviceType.Name}");
}
```

## ICommand Execution Flow and Patterns

### Command Interface Architecture

The FractalDataWorks framework uses the command pattern for all service operations. Understanding the command execution flow is critical for effective service implementation.

#### Core ICommand Interface

```csharp
public interface ICommand
{
    Guid CommandId { get; }           // Unique identifier for this command instance
    Guid CorrelationId { get; }       // Correlation identifier for tracking related operations
    DateTimeOffset Timestamp { get; }  // When this command was created
    IFdwConfiguration? Configuration { get; } // Configuration associated with this command
    Task<IValidationResult> Validate(); // Validates this command
}

public interface ICommand<T> : ICommand
{
    T? Payload { get; init; }         // The command payload/data
}
```

#### Command Execution Pipeline

Every command executed through ServiceBase follows this pipeline:

1. **Command Receipt**: Service receives ICommand through Execute method
2. **Type Validation**: Command is cast to expected TCommand type
3. **Command Validation**: Command.Validate() method is called
4. **Configuration Validation**: Command.Configuration is validated if present
5. **ExecuteCore Invocation**: Your implementation receives the validated command
6. **Result Processing**: Results are wrapped and logged
7. **Exception Handling**: Exceptions are caught, logged, and converted to failure results

```csharp
// Simplified execution flow
public async Task<IFdwResult<T>> Execute<T>(ICommand command, CancellationToken cancellationToken)
{
    using (Logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = command.CorrelationId }))
    {
        // 1. Type validation
        if (command is not TCommand cmd)
            return FdwResult<T>.Failure("Invalid command type");
        
        // 2. Command validation
        var validationResult = await ValidateCommand(cmd);
        if (validationResult.Error)
            return FdwResult<T>.Failure(validationResult.Message);
        
        try
        {
            // 3. Execute your implementation
            var result = await ExecuteCore<T>(validationResult.Value);
            
            // 4. Log and return
            if (result.IsSuccess)
                ServiceBaseLog.CommandExecuted(Logger, cmd.GetType().Name, duration);
            else
                ServiceBaseLog.CommandFailed(Logger, cmd.GetType().Name, result.Message);
                
            return result;
        }
        catch (Exception ex)
        {
            ServiceBaseLog.OperationFailed(Logger, cmd.GetType().Name, ex.Message, ex);
            return FdwResult<T>.Failure("Operation failed.");
        }
    }
}
```

### Command Implementation Patterns

#### Basic Command Implementation

```csharp
public class CustomerCommand : ICommand<CustomerRequest>
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; set; }
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    public IFdwConfiguration? Configuration { get; set; }
    public CustomerRequest? Payload { get; init; }
    
    public async Task<IValidationResult> Validate()
    {
        var result = new ValidationResult();
        
        if (Payload == null)
        {
            result.AddError("Payload is required");
        }
        else
        {
            if (string.IsNullOrEmpty(Payload.Name))
                result.AddError("Customer name is required");
                
            if (Payload.Id <= 0)
                result.AddError("Customer ID must be positive");
        }
        
        return await Task.FromResult(result);
    }
}
```

#### Command with FluentValidation

```csharp
public class CreateCustomerCommand : ICommand<CreateCustomerRequest>
{
    private readonly IValidator<CreateCustomerRequest> _validator;
    
    public CreateCustomerCommand(IValidator<CreateCustomerRequest> validator)
    {
        _validator = validator;
        CommandId = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
    }
    
    public Guid CommandId { get; }
    public Guid CorrelationId { get; set; }
    public DateTimeOffset Timestamp { get; }
    public IFdwConfiguration? Configuration { get; set; }
    public CreateCustomerRequest? Payload { get; init; }
    
    public async Task<IValidationResult> Validate()
    {
        if (Payload == null)
            return ValidationResult.Failed("Payload is required");
            
        var fluentResult = await _validator.ValidateAsync(Payload);
        return fluentResult.ToFdwValidationResult();
    }
}
```

#### Command Factory Pattern

```csharp
public static class CustomerCommandFactory
{
    public static CustomerCommand GetCustomer(int customerId, Guid correlationId)
    {
        return new CustomerCommand
        {
            CorrelationId = correlationId,
            Payload = new CustomerRequest { Id = customerId, Operation = "Get" }
        };
    }
    
    public static CustomerCommand UpdateCustomer(Customer customer, Guid correlationId)
    {
        return new CustomerCommand
        {
            CorrelationId = correlationId,
            Payload = new CustomerRequest 
            { 
                Id = customer.Id, 
                Name = customer.Name,
                Operation = "Update" 
            }
        };
    }
}
```

### Command Correlation and Tracking

Commands support correlation tracking for distributed operations:

```csharp
// Start a new operation
var correlationId = Guid.NewGuid();

// Create related commands with same correlation ID
var getCustomerCmd = CustomerCommandFactory.GetCustomer(123, correlationId);
var updateAddressCmd = AddressCommandFactory.UpdateAddress(456, correlationId);
var sendEmailCmd = EmailCommandFactory.SendWelcomeEmail(customerEmail, correlationId);

// Execute related operations
var customer = await customerService.Execute<Customer>(getCustomerCmd);
var address = await addressService.Execute<Address>(updateAddressCmd);  
var email = await emailService.Execute<EmailResult>(sendEmailCmd);

// All operations share the same correlation ID in logs
```

### Command Configuration Patterns

Commands can carry their own configuration for flexibility:

```csharp
public class DatabaseCommand : ICommand<QueryRequest>
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; set; }
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    
    // Command can specify which database configuration to use
    public IFdwConfiguration? Configuration { get; set; }
    public QueryRequest? Payload { get; init; }
    
    // Override database configuration for this specific command
    public string? ConnectionString { get; init; }
    public TimeSpan? CommandTimeout { get; init; }
    
    public async Task<IValidationResult> Validate()
    {
        var result = new ValidationResult();
        
        // Validate both payload and configuration
        if (Payload == null)
            result.AddError("Query payload is required");
            
        if (Configuration != null && !Configuration.Validate())
            result.AddError("Command configuration is invalid");
            
        return await Task.FromResult(result);
    }
}
```

## ServiceBase Usage Patterns

### Basic Service Implementation

```csharp
public class CustomerService : ServiceBase<CustomerCommand, CustomerConfiguration, CustomerService>
{
    private readonly ICustomerRepository _repository;
    private readonly IValidator<CustomerCommand> _commandValidator;
    
    public CustomerService(
        ILogger<CustomerService> logger, 
        CustomerConfiguration configuration,
        ICustomerRepository repository,
        IValidator<CustomerCommand> commandValidator)
        : base(logger, configuration)
    {
        _repository = repository;
        _commandValidator = commandValidator;
    }
    
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(CustomerCommand command)
    {
        return command.Payload?.Operation switch
        {
            "Get" => await GetCustomer<T>(command),
            "Create" => await CreateCustomer<T>(command),
            "Update" => await UpdateCustomer<T>(command),
            "Delete" => await DeleteCustomer<T>(command),
            _ => FdwResult<T>.Failure("Unknown operation")
        };
    }
    
    private async Task<IFdwResult<T>> GetCustomer<T>(CustomerCommand command)
    {
        var customer = await _repository.GetAsync(command.Payload!.Id);
        if (customer == null)
        {
            return FdwResult<T>.Failure($"Customer {command.Payload.Id} not found");
        }
        
        return FdwResult<T>.Success((T)(object)customer);
    }
    
    // Additional methods for Create, Update, Delete...
}
```

### Advanced Service with Custom Validation

```csharp
public class BankingService : ServiceBase<BankingCommand, BankingConfiguration, BankingService>
{
    private readonly IBankingRepository _repository;
    private readonly IFraudDetectionService _fraudDetection;
    
    public BankingService(
        ILogger<BankingService> logger,
        BankingConfiguration configuration,
        IBankingRepository repository,
        IFraudDetectionService fraudDetection)
        : base(logger, configuration)
    {
        _repository = repository;
        _fraudDetection = fraudDetection;
    }
    
    // Override validation to add business rules
    protected override async Task<IFdwResult<BankingCommand>> ValidateCommand(ICommand command)
    {
        // First, run base validation
        var baseResult = await base.ValidateCommand(command);
        if (baseResult.Error) return baseResult;
        
        var bankingCommand = baseResult.Value!;
        
        // Add fraud detection validation
        if (bankingCommand.Payload?.Operation == "Transfer" && bankingCommand.Payload.Amount > 10000)
        {
            var fraudResult = await _fraudDetection.CheckTransaction(bankingCommand.Payload);
            if (fraudResult.IsHighRisk)
            {
                Logger.LogWarning("High-risk transaction blocked: {CorrelationId}", 
                    bankingCommand.CorrelationId);
                return FdwResult<BankingCommand>.Failure("Transaction blocked for security review");
            }
        }
        
        return baseResult;
    }
    
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(BankingCommand command)
    {
        // Business logic here - validation has already passed
        return command.Payload?.Operation switch
        {
            "Transfer" => await ProcessTransfer<T>(command),
            "Withdraw" => await ProcessWithdrawal<T>(command),
            "Deposit" => await ProcessDeposit<T>(command),
            _ => FdwResult<T>.Failure("Unknown banking operation")
        };
    }
}
```

### Service with Transaction Support

```csharp
public class OrderService : ServiceBase<OrderCommand, OrderConfiguration, OrderService>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPaymentService _paymentService;
    private readonly IDbContext _dbContext;
    
    public OrderService(/* dependencies */) : base(logger, configuration)
    {
        // Initialize dependencies...
    }
    
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(OrderCommand command)
    {
        if (command.Payload?.Operation == "PlaceOrder")
        {
            return await PlaceOrderWithTransaction<T>(command);
        }
        
        // Handle other operations...
    }
    
    private async Task<IFdwResult<T>> PlaceOrderWithTransaction<T>(OrderCommand command)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        
        try
        {
            // Step 1: Reserve inventory
            var inventoryResult = await _inventoryRepository.ReserveItems(command.Payload!.Items);
            if (inventoryResult.IsFailure)
            {
                return FdwResult<T>.Failure($"Inventory reservation failed: {inventoryResult.Message}");
            }
            
            // Step 2: Process payment
            var paymentResult = await _paymentService.ProcessPayment(command.Payload.Payment);
            if (paymentResult.IsFailure)
            {
                return FdwResult<T>.Failure($"Payment failed: {paymentResult.Message}");
            }
            
            // Step 3: Create order record
            var order = new Order
            {
                CustomerId = command.Payload.CustomerId,
                Items = command.Payload.Items,
                PaymentId = paymentResult.Value!.PaymentId,
                Status = OrderStatus.Confirmed
            };
            
            await _orderRepository.AddAsync(order);
            
            // Step 4: Commit transaction
            await transaction.CommitAsync();
            
            Logger.LogInformation("Order {OrderId} placed successfully for customer {CustomerId}", 
                order.Id, order.CustomerId);
                
            return FdwResult<T>.Success((T)(object)order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Logger.LogError(ex, "Failed to place order for customer {CustomerId}", 
                command.Payload?.CustomerId);
            throw; // Let base class handle the exception
        }
    }
}
```

### Service with Performance Monitoring

```csharp
public class ReportingService : ServiceBase<ReportCommand, ReportConfiguration, ReportingService>
{
    private readonly IReportingRepository _repository;
    private readonly IMemoryCache _cache;
    
    public ReportingService(/* dependencies */) : base(logger, configuration)
    {
        // Initialize...
    }
    
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(ReportCommand command)
    {
        var stopwatch = Stopwatch.StartNew();
        var itemsProcessed = 0;
        
        try
        {
            var result = command.Payload?.ReportType switch
            {
                "Sales" => await GenerateSalesReport<T>(command),
                "Customer" => await GenerateCustomerReport<T>(command),
                "Inventory" => await GenerateInventoryReport<T>(command),
                _ => FdwResult<T>.Failure("Unknown report type")
            };
            
            if (result.IsSuccess && result.Value is ICollection collection)
            {
                itemsProcessed = collection.Count;
            }
            
            return result;
        }
        finally
        {
            // Log performance metrics
            stopwatch.Stop();
            var metrics = new PerformanceMetrics(
                Duration: stopwatch.Elapsed.TotalMilliseconds,
                ItemsProcessed: itemsProcessed,
                OperationType: $"Report_{command.Payload?.ReportType}"
            );
            
            ServiceBaseLog.PerformanceMetrics(Logger, metrics);
            
            // Log warning for slow operations
            if (stopwatch.Elapsed.TotalSeconds > 30)
            {
                Logger.LogWarning("Slow report generation: {ReportType} took {Duration}ms", 
                    command.Payload?.ReportType, stopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}
```

### Service Health and Availability Patterns

```csharp
public class ExternalApiService : ServiceBase<ApiCommand, ApiConfiguration, ExternalApiService>
{
    private readonly HttpClient _httpClient;
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly IRetryPolicy _retryPolicy;
    private bool _lastHealthCheckPassed = true;
    private DateTime _lastHealthCheck = DateTime.MinValue;
    
    public ExternalApiService(/* dependencies */) : base(logger, configuration)
    {
        // Initialize...
    }
    
    // Override health check with circuit breaker pattern
    public override bool IsAvailable
    {
        get
        {
            // Check every 60 seconds
            if (DateTime.UtcNow - _lastHealthCheck > TimeSpan.FromSeconds(60))
            {
                _ = Task.Run(async () => await CheckHealthAsync());
            }
            
            return base.IsAvailable && _lastHealthCheckPassed && !_circuitBreaker.IsOpen;
        }
    }
    
    private async Task CheckHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_configuration.HealthCheckUrl);
            _lastHealthCheckPassed = response.IsSuccessStatusCode;
            _lastHealthCheck = DateTime.UtcNow;
            
            if (_lastHealthCheckPassed)
            {
                _circuitBreaker.RecordSuccess();
            }
            else
            {
                _circuitBreaker.RecordFailure();
                Logger.LogWarning("Health check failed for external API: {StatusCode}", 
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _lastHealthCheckPassed = false;
            _lastHealthCheck = DateTime.UtcNow;
            _circuitBreaker.RecordFailure();
            Logger.LogError(ex, "Health check exception for external API");
        }
    }
    
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(ApiCommand command)
    {
        if (!IsAvailable)
        {
            return FdwResult<T>.Failure("External API service is currently unavailable");
        }
        
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await ExecuteApiCall<T>(command);
        });
    }
}
```

## Advanced Scenarios

### Custom Validation
Override `ValidateCommand` for additional validation:
```csharp
protected override async Task<FdwResult<TCommand>> ValidateCommand(ICommand command)
{
    var result = await base.ValidateCommand(command);
    if (result.IsFailure) return result;
    
    // Add custom validation
    if (customValidationFails)
    {
        return FdwResult<TCommand>.Failure("Custom validation error");
    }
    
    return result;
}
```

### Custom Health Checks
Override `IsHealthy` for custom health logic:
```csharp
public override bool IsHealthy => base.IsHealthy && _repository.IsConnected;
```

## ServiceTypeProviderBase: Factory Pattern, Not Service Locator

The `ServiceTypeProviderBase<TService, TServiceType, TConfiguration>` class is often mistaken for a service locator anti-pattern, but it actually implements a legitimate **Factory Pattern** with **Strategy Pattern** elements. Here's why this is good design:

### Why It's NOT a Service Locator Anti-Pattern

1. **Explicit Dependencies**: The provider is injected as a dependency, not hidden or accessed globally
2. **Single Responsibility**: Each provider manages only one type of service (e.g., notification services, data providers)
3. **Type Safety**: Uses strongly-typed service types with compile-time verification
4. **Configuration-Driven**: Service selection is based on explicit configuration, not arbitrary requests
5. **Factory Semantics**: Creates instances rather than acting as a global registry

### Legitimate Factory/Strategy Pattern Usage

```csharp
// This is a Factory Pattern, not Service Locator
public class NotificationManager
{
    private readonly ServiceTypeProviderBase<INotificationService, NotificationServiceType, NotificationConfiguration> _notificationProvider;
    
    // Explicit dependency injection - no hidden dependencies
    public NotificationManager(ServiceTypeProviderBase<INotificationService, NotificationServiceType, NotificationConfiguration> notificationProvider)
    {
        _notificationProvider = notificationProvider;
    }
    
    // Factory method - creates instances based on business logic
    public async Task<IFdwResult<INotificationService>> GetNotificationService(string notificationType)
    {
        // Uses configuration-driven selection, not arbitrary service location
        return await _notificationProvider.GetServiceAsync(notificationType);
    }
}
```

### When This Pattern is Appropriate

- **Runtime Service Selection**: When the specific service implementation must be chosen at runtime based on configuration
- **Plugin Architecture**: When supporting multiple implementations of the same interface
- **Multi-Tenant Systems**: When different tenants require different service implementations
- **A/B Testing**: When switching between service implementations for testing

### Design Benefits

1. **Testability**: Easy to mock and test the provider interface
2. **Flexibility**: New service types can be added without changing consumers
3. **Encapsulation**: Service creation logic is centralized and consistent
4. **Type Safety**: Enhanced Enum pattern provides compile-time verification

The key difference from the anti-pattern is that ServiceTypeProviderBase doesn't hide dependencies or create global access points. It's an explicitly injected factory that follows dependency inversion principles.