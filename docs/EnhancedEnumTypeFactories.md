# Enhanced Enum Type Factories

## Overview

The Enhanced Enum Type Factories pattern combines the power of FractalDataWorks.EnhancedEnums source generators with factory patterns to create strongly-typed, discoverable, and automatically registered service, connection, and tool types. This pattern provides compile-time safety, IntelliSense support, and automatic dependency injection registration for all factory types.

## Architecture

### Two-Layer Base Class Pattern

Each type factory implementation follows a two-layer pattern:

1. **Non-Generic Factory Base** (e.g., `ServiceTypeFactoryBase`)
   - Contains factory methods and properties
   - No Enhanced Enum attributes
   - Provides the actual factory functionality

2. **Enhanced Enum Base** (e.g., `ServiceTypeBase`)
   - Inherits from the factory base
   - Has `[EnhancedEnumBase]` attribute
   - Enables source generation

This separation ensures that:
- Factory logic is independent of source generation
- Enhanced Enum constraints don't interfere with factory implementation
- Testing and mocking are simplified

## Service Type Factories

### Base Classes

```csharp
// Non-generic factory base (no Enhanced Enum attributes)
public abstract class ServiceTypeFactoryBase<TService, TConfiguration>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration
{
    protected ServiceTypeFactoryBase(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
    
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    
    public abstract object Create(TConfiguration configuration);
    public abstract Task<TService> GetService(string configurationName);
    public abstract Task<TService> GetService(int configurationId);
}

// Enhanced Enum base with source generator attributes
[EnhancedEnumBase("ServiceTypes", 
    ReturnType = "IServiceFactory<IFdwService, IFdwConfiguration>",
    ReturnTypeNamespace = "FractalDataWorks.Services")]
public abstract class ServiceTypeBase<TService, TFactory, TConfiguration>
    : EnumOptionBase<ServiceTypeBase<TService, TFactory, TConfiguration>>, IServiceType
    where TService : class
    where TFactory : class, new()
    where TConfiguration : class
{
    protected ServiceTypeBase(int id, string name, string description, Type serviceType, Type configurationType, string category)
        : base(id, name)
    {
    }
}
```

### Implementation Example

```csharp
[EnumOption(1, "EmailNotification", "Email notification service")]
public class EmailNotificationServiceType : ServiceTypeBase<INotificationService, EmailNotificationFactory, EmailConfiguration>
{
    public EmailNotificationServiceType() 
        : base(1, "EmailNotification", "Email notification service", typeof(INotificationService), typeof(EmailConfiguration), "Notification")
    {
    }

    public override INotificationService CreateService(IServiceProvider serviceProvider)
    {
        var emailProvider = serviceProvider.GetRequiredService<IEmailProvider>();
        var templateEngine = serviceProvider.GetRequiredService<ITemplateEngine>();
        var configuration = serviceProvider.GetRequiredService<EmailConfiguration>();
        return new EmailNotificationService(emailProvider, templateEngine, configuration);
    }

    public override void RegisterService(IServiceCollection services)
    {
        services.AddScoped<INotificationService, EmailNotificationService>();
        services.AddScoped<IEmailProvider, DefaultEmailProvider>();
        services.AddScoped<ITemplateEngine, DefaultTemplateEngine>();
    }

    public override EmailNotificationFactory Factory()
    {
        return new EmailNotificationFactory();
    }
}
```

### Generated Code

The Enhanced Enum source generator creates:

```csharp
public static class ServiceTypes
{
    public static IReadOnlyList<IServiceFactory<IFdwService, IFdwConfiguration>> All { get; }
    
    public static IServiceFactory<IFdwService, IFdwConfiguration>? GetById(int id);
    public static IServiceFactory<IFdwService, IFdwConfiguration>? GetByName(string name);
    
    public static class EmailNotification
    {
        public const int Id = 1;
        public const string Name = "EmailNotification";
        public const string Description = "Email notification service";
        
        public static EmailNotificationServiceType Instance { get; }
    }
}
```

## Connection Type Factories

### Base Classes

```csharp
// Non-generic factory base
public abstract class ConnectionTypeFactoryBase<TConnection, TConfiguration>
    where TConnection : class, IExternalConnection
    where TConfiguration : class, IFdwConfiguration
{
    // Similar structure to ServiceTypeFactoryBase
}

// Enhanced Enum base
[EnhancedEnumBase("ConnectionTypes", 
    ReturnType = "IConnectionFactory<IExternalConnection, IFdwConfiguration>",
    ReturnTypeNamespace = "FractalDataWorks.Connections")]
public abstract class ConnectionTypeBase<TConnection, TConfiguration> 
    : ConnectionTypeFactoryBase<TConnection, TConfiguration>
    where TConnection : class, IExternalConnection
    where TConfiguration : class, IFdwConfiguration
{
    // Similar structure to ServiceTypeBase
}
```

### Implementation Example

```csharp
[EnumOption(1, "SqlServer", "Microsoft SQL Server connection")]
public class SqlServerConnectionType : ConnectionTypeBase<SqlConnection, SqlServerConfiguration>
{
    private readonly ISqlCommandBuilder _commandBuilder;
    private readonly IConnectionStringParser _parser;
    
    public SqlServerConnectionType(
        ISqlCommandBuilder commandBuilder,
        IConnectionStringParser parser) 
        : base(1, "SqlServer", "Microsoft SQL Server connection")
    {
        _commandBuilder = commandBuilder;
        _parser = parser;
    }

    public override object Create(SqlServerConfiguration configuration)
    {
        var connectionString = _parser.Parse(configuration);
        return new SqlConnection(connectionString, _commandBuilder);
    }

    public override async Task<SqlConnection> GetConnection(string configurationName)
    {
        var config = await _configurationRegistry.GetByNameAsync(configurationName);
        var connectionString = _parser.Parse(config);
        var connection = new SqlConnection(connectionString, _commandBuilder);
        await connection.OpenAsync();
        return connection;
    }

    public override async Task<SqlConnection> GetConnection(int configurationId)
    {
        var config = await _configurationRegistry.GetByIdAsync(configurationId);
        var connectionString = _parser.Parse(config);
        var connection = new SqlConnection(connectionString, _commandBuilder);
        await connection.OpenAsync();
        return connection;
    }
}
```

## Tool Type Factories

### Base Classes

```csharp
// Non-generic factory base
public abstract class ToolTypeFactoryBase<TTool, TConfiguration>
    where TTool : class, IFdwTool
    where TConfiguration : class, IFdwConfiguration
{
    // Similar structure to other factory bases
}

// Enhanced Enum base
[EnhancedEnumBase("ToolTypes", 
    ReturnType = "IToolFactory<IFdwTool, IFdwConfiguration>",
    ReturnTypeNamespace = "FractalDataWorks.Services")]
public abstract class ToolTypeBase<TTool, TConfiguration> 
    : ToolTypeFactoryBase<TTool, TConfiguration>
    where TTool : class, IFdwTool
    where TConfiguration : class, IFdwConfiguration
{
    // Similar structure to other type bases
}
```

### Implementation Example

```csharp
[EnumOption(1, "CodeGenerator", "Source code generation tool")]
public class CodeGeneratorToolType : ToolTypeBase<ICodeGeneratorTool, CodeGeneratorConfiguration>
{
    private readonly ITemplateEngine _templateEngine;
    private readonly IFileSystem _fileSystem;
    
    public CodeGeneratorToolType(
        ITemplateEngine templateEngine,
        IFileSystem fileSystem) 
        : base(1, "CodeGenerator", "Source code generation tool")
    {
        _templateEngine = templateEngine;
        _fileSystem = fileSystem;
    }

    public override object Create(CodeGeneratorConfiguration configuration)
    {
        return new CodeGeneratorTool(_templateEngine, _fileSystem, configuration);
    }

    public override async Task<ICodeGeneratorTool> GetTool(string configurationName)
    {
        var config = await _configurationRegistry.GetByNameAsync(configurationName);
        return new CodeGeneratorTool(_templateEngine, _fileSystem, config);
    }

    public override async Task<ICodeGeneratorTool> GetTool(int configurationId)
    {
        var config = await _configurationRegistry.GetByIdAsync(configurationId);
        return new CodeGeneratorTool(_templateEngine, _fileSystem, config);
    }
}
```

## Dependency Injection

### Registration

Each package provides extension methods for DI registration:

```csharp
// In Startup.cs or Program.cs
services.AddServiceTypes(Assembly.GetExecutingAssembly());
services.AddConnectionTypes(Assembly.GetExecutingAssembly());
services.AddToolTypes(Assembly.GetExecutingAssembly());
```

### How Registration Works

The extension methods:
1. Scan the assembly for types inheriting from the respective base classes
2. Register each type as a singleton
3. Also register the type as its factory interface

```csharp
public static IServiceCollection AddServiceTypes(this IServiceCollection services, Assembly? assembly = null)
{
    assembly ??= Assembly.GetCallingAssembly();
    
    var serviceTypes = assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract)
        .Where(t => IsServiceType(t))
        .ToList();

    foreach (var serviceType in serviceTypes)
    {
        // Register the type itself
        services.TryAddSingleton(serviceType);
        
        // Also register as the factory interface
        RegisterAsServiceFactory(services, serviceType);
    }

    return services;
}
```

### Resolution

```csharp
// Resolve by concrete type
var emailServiceType = provider.GetService<EmailNotificationServiceType>();

// Resolve by factory interface
var factory = provider.GetService<IServiceFactory<INotificationService, EmailConfiguration>>();

// Both resolve to the same instance
```

## Usage Patterns

### Direct Access via Generated Collections

```csharp
// Get all service types
foreach (var serviceType in ServiceTypes.All)
{
    Console.WriteLine($"{serviceType.Id}: {serviceType.Name}");
}

// Get by ID
var emailService = ServiceTypes.GetById(1);

// Get by name
var smsService = ServiceTypes.GetByName("SmsNotification");

// Access specific type directly
var email = ServiceTypes.EmailNotification.Instance;
```

### Factory Pattern Usage

```csharp
public class NotificationManager
{
    private readonly IServiceProvider _provider;
    
    public async Task<INotificationService> GetNotificationService(string type, string configName)
    {
        var factory = ServiceTypes.GetByName(type);
        if (factory == null)
            throw new NotSupportedException($"Notification type '{type}' not supported");
            
        return await factory.GetService(configName);
    }
}
```

### Dynamic Service Creation

```csharp
public class DynamicServiceFactory
{
    private readonly IServiceProvider _provider;
    
    public IFdwService CreateService(int typeId, IFdwConfiguration config)
    {
        var factory = ServiceTypes.GetById(typeId);
        if (factory == null)
            throw new InvalidOperationException($"Service type {typeId} not found");
            
        // Resolve the factory from DI to get dependency injection
        var resolvedFactory = _provider.GetService(factory.GetType());
        return (IFdwService)resolvedFactory.Create(config);
    }
}
```

## Best Practices

### 1. Constructor Dependency Injection

Always inject dependencies through the constructor:

```csharp
[EnumOption(1, "MyService", "My service implementation")]
public class MyServiceType : ServiceTypeBase<IMyService, MyServiceFactory, MyConfiguration>
{
    private readonly IDependency1 _dep1;
    private readonly IDependency2 _dep2;
    
    public MyServiceType() 
        : base(1, "MyService", "My service implementation", typeof(IMyService), typeof(MyConfiguration), "Custom")
    {
    }
}
```

### 2. Configuration Validation

Validate configurations in factory methods:

```csharp
public override IMyService CreateService(IServiceProvider serviceProvider)
{
    var configuration = serviceProvider.GetRequiredService<MyConfiguration>();
    
    if (configuration == null)
        throw new ArgumentNullException(nameof(configuration));
        
    var validationResult = configuration.Validate();
    if (!validationResult.IsValid)
        throw new InvalidOperationException($"Invalid configuration: {validationResult.Error}");
    
    var dep1 = serviceProvider.GetRequiredService<IDependency1>();
    var dep2 = serviceProvider.GetRequiredService<IDependency2>();
        
    return new MyService(dep1, dep2, configuration);
}
```

### 3. Service Registration

Properly register all dependencies in the RegisterService method:

```csharp
public override void RegisterService(IServiceCollection services)
{
    // Register the main service
    services.AddScoped<IMyService, MyService>();
    
    // Register all dependencies
    services.AddScoped<IDependency1, ConcreteDependency1>();
    services.AddScoped<IDependency2, ConcreteDependency2>();
    
    // Register configuration if needed
    services.AddSingleton<MyConfiguration>(sp => /* configuration setup */);
}
```

### 4. Consistent ID Management

Keep IDs consistent and well-documented:

```csharp
// Consider using a constants class
public static class ServiceTypeIds
{
    public const int EmailNotification = 1;
    public const int SmsNotification = 2;
    public const int PushNotification = 3;
}

[EnumOption(ServiceTypeIds.EmailNotification, "EmailNotification", "Email notification service")]
public class EmailNotificationServiceType : ServiceTypeBase<INotificationService, EmailNotificationFactory, EmailConfiguration>
{
    // ...
}
```

## Migration Guide

### From Old Pattern to Enhanced Enum Type Factories

Before:
```csharp
public enum ServiceType
{
    EmailNotification = 1,
    SmsNotification = 2
}

public class ServiceFactory
{
    public IService Create(ServiceType type, IConfiguration config)
    {
        return type switch
        {
            ServiceType.EmailNotification => new EmailService(config),
            ServiceType.SmsNotification => new SmsService(config),
            _ => throw new NotSupportedException()
        };
    }
}
```

After:
```csharp
[EnumOption(1, "EmailNotification", "Email notification service")]
public class EmailNotificationServiceType : ServiceTypeBase<INotificationService, EmailNotificationFactory, EmailConfiguration>
{
    public EmailNotificationServiceType() 
        : base(1, "EmailNotification", "Email notification service", typeof(INotificationService), typeof(EmailConfiguration), "Notification")
    {
    }

    public override INotificationService CreateService(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<EmailConfiguration>();
        return new EmailNotificationService(configuration);
    }

    public override void RegisterService(IServiceCollection services)
    {
        services.AddScoped<INotificationService, EmailNotificationService>();
    }

    public override EmailNotificationFactory Factory()
    {
        return new EmailNotificationFactory();
    }
}

// Usage
var service = ServiceTypes.EmailNotification.Instance.Create(config);
```

## Testing

### Unit Testing Factory Types

```csharp
public class EmailNotificationServiceTypeTests
{
    [Fact]
    public void CreateReturnsEmailNotificationService()
    {
        // Arrange
        var mockEmailProvider = new Mock<IEmailProvider>();
        var mockTemplateEngine = new Mock<ITemplateEngine>();
        var factory = new EmailNotificationServiceType(mockEmailProvider.Object, mockTemplateEngine.Object);
        var config = new EmailConfiguration { /* ... */ };
        
        // Act
        var service = factory.Create(config);
        
        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<EmailNotificationService>();
    }
}
```

### Integration Testing with DI

```csharp
public class ServiceTypeIntegrationTests
{
    [Fact]
    public void ServiceTypesAreRegisteredCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddServiceTypes(typeof(EmailNotificationServiceType).Assembly);
        var provider = services.BuildServiceProvider();
        
        // Act
        var factory = provider.GetService<IServiceFactory<INotificationService, EmailConfiguration>>();
        
        // Assert
        factory.ShouldNotBeNull();
        factory.ShouldBeOfType<EmailNotificationServiceType>();
    }
}
```

## Troubleshooting

### Common Issues

1. **ENH001: Enhanced enum base types cannot be generic**
   - Solution: Ensure your base types follow the two-layer pattern with non-generic factory base

2. **Source generator not running**
   - Solution: Ensure FractalDataWorks.EnhancedEnums package is referenced with `PrivateAssets="all"`

3. **Types not being registered with DI**
   - Solution: Verify the assembly being scanned contains the types and they inherit from the correct base class

4. **Factory methods returning null**
   - Solution: Check configuration registry is properly configured and contains the requested configurations

## Summary

The Enhanced Enum Type Factories pattern provides:
- **Type Safety**: Compile-time checked factory types
- **Discoverability**: IntelliSense support for all types
- **Automatic Registration**: DI registration handled automatically
- **Flexibility**: Full factory pattern with dependency injection
- **Maintainability**: Clear separation of concerns and testability

This pattern is ideal for scenarios where you need to manage multiple implementations of a service, connection, or tool type with full type safety and dependency injection support.