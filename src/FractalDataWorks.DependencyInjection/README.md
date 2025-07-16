# FractalDataWorks.DependencyInjection

Dependency injection abstractions and extensions for the FractalDataWorks framework.

## Overview

FractalDataWorks.DependencyInjection provides:
- Container-agnostic DI abstractions
- Service registration helpers
- Module/plugin system support
- Service discovery patterns
- Lifetime management utilities

## Planned Components

### IServiceModule

Module pattern for organizing service registrations:
```csharp
public interface IServiceModule
{
    string Name { get; }
    string Version { get; }
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
```

### Service Registration Extensions

Fluent registration helpers:
```csharp
public static class ServiceCollectionExtensions
{
    // Register all FractalDataWorks services
    public static IServiceCollection AddFractalDataWorks(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<FractalDataWorksOptions>? configure = null)
    {
        var options = new FractalDataWorksOptions();
        configure?.Invoke(options);
        
        // Register core services
        services.AddFractalCore();
        services.AddFractalConfiguration(configuration);
        services.AddFractalServices();
        
        // Register optional modules
        if (options.UseConnections)
            services.AddFractalConnections();
        if (options.UseData)
            services.AddFractalData();
        
        return services;
    }
    
    // Register services with configuration
    public static IServiceCollection AddFractalService<TService, TImplementation, TConfiguration>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class, IFractalService<TConfiguration>
        where TImplementation : class, TService
        where TConfiguration : class, IFractalConfiguration
    {
        services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
        services.AddConfigurationRegistry<TConfiguration>();
        return services;
    }
}
```

### Configuration Registry Registration

```csharp
public static class ConfigurationRegistrationExtensions
{
    public static IServiceCollection AddConfigurationRegistry<T>(
        this IServiceCollection services,
        Func<IServiceProvider, IConfigurationRegistry<T>>? factory = null)
        where T : class, IFractalConfiguration
    {
        if (factory != null)
        {
            services.AddSingleton(factory);
        }
        else
        {
            services.AddSingleton<IConfigurationRegistry<T>>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var configs = configuration.GetSection(typeof(T).Name).Get<List<T>>() ?? new List<T>();
                return new InMemoryConfigurationRegistry<T>(configs);
            });
        }
        
        return services;
    }
}
```

### Service Discovery

```csharp
public interface IServiceDiscovery
{
    IEnumerable<Type> DiscoverServices(Assembly assembly);
    IEnumerable<Type> DiscoverServices(string assemblyPattern);
    void RegisterDiscoveredServices(IServiceCollection services);
}

public class ConventionBasedServiceDiscovery : IServiceDiscovery
{
    public IEnumerable<Type> DiscoverServices(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IFractalService<,>)));
    }
}
```

## Planned Features

### Module System

```csharp
public abstract class ServiceModuleBase : IServiceModule
{
    public abstract string Name { get; }
    public virtual string Version => "1.0.0";
    
    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        RegisterServices(services);
        RegisterConfigurations(services, configuration);
        RegisterValidators(services);
    }
    
    protected abstract void RegisterServices(IServiceCollection services);
    protected virtual void RegisterConfigurations(IServiceCollection services, IConfiguration configuration) { }
    protected virtual void RegisterValidators(IServiceCollection services) { }
}

// Example module
public class CustomerServiceModule : ServiceModuleBase
{
    public override string Name => "Customer Services";
    
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
    }
    
    protected override void RegisterConfigurations(IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigurationRegistry<CustomerConfiguration>();
    }
}
```

### Service Factory Pattern

```csharp
public interface IServiceFactory<TService> where TService : IFractalService
{
    TService CreateService(string name);
    TService CreateService(int configurationId);
}

public class ServiceFactory<TService, TConfiguration> : IServiceFactory<TService>
    where TService : IFractalService<TConfiguration>
    where TConfiguration : class, IFractalConfiguration
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationRegistry<TConfiguration> _configurations;
    
    public TService CreateService(int configurationId)
    {
        var config = _configurations.Get(configurationId);
        if (config == null)
            throw new InvalidOperationException($"Configuration {configurationId} not found");
            
        return ActivatorUtilities.CreateInstance<TService>(_serviceProvider, config);
    }
}
```

### Decorator Pattern Support

```csharp
public static class DecoratorExtensions
{
    public static IServiceCollection Decorate<TInterface, TDecorator>(
        this IServiceCollection services)
        where TInterface : class
        where TDecorator : class, TInterface
    {
        var wrappedDescriptor = services.FirstOrDefault(
            s => s.ServiceType == typeof(TInterface));
            
        if (wrappedDescriptor == null)
            throw new InvalidOperationException($"Service {typeof(TInterface).Name} not registered");
            
        var objectFactory = ActivatorUtilities.CreateFactory(
            typeof(TDecorator),
            new[] { typeof(TInterface) });
            
        services.Replace(ServiceDescriptor.Describe(
            typeof(TInterface),
            s => (TInterface)objectFactory(s, new[] { s.CreateInstance(wrappedDescriptor) }),
            wrappedDescriptor.Lifetime));
            
        return services;
    }
}
```

## Usage Examples (Planned)

### Basic Registration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddFractalDataWorks(Configuration, options =>
    {
        options.UseConnections = true;
        options.UseData = true;
        options.ValidateOnStartup = true;
    });
    
    // Register custom services
    services.AddFractalService<IOrderService, OrderService, OrderConfiguration>();
}
```

### Module Registration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Discover and register modules
    var modules = DiscoverModules("MyApp.*.dll");
    foreach (var module in modules)
    {
        module.ConfigureServices(services, Configuration);
    }
}

private IEnumerable<IServiceModule> DiscoverModules(string pattern)
{
    var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, pattern)
        .Select(Assembly.LoadFrom);
        
    return assemblies
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(IServiceModule).IsAssignableFrom(t) && !t.IsAbstract)
        .Select(t => (IServiceModule)Activator.CreateInstance(t)!)
        .OrderBy(m => m.Name);
}
```

### Service Decoration
```csharp
services.AddScoped<ICustomerService, CustomerService>();
services.Decorate<ICustomerService, CachedCustomerService>();
services.Decorate<ICustomerService, LoggingCustomerService>();

// Results in: LoggingCustomerService -> CachedCustomerService -> CustomerService
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.DependencyInjection" Version="*" />
```

## Dependencies

- FractalDataWorks.net (core abstractions)
- FractalDataWorks.Configuration
- Microsoft.Extensions.DependencyInjection.Abstractions

## Status

This package is currently in planning phase. The interfaces and implementations described above represent the intended design and may change during development.

## Contributing

This package is accepting contributions for:
- Service registration helpers
- Module system implementation
- Service discovery mechanisms
- Container adapters for other DI frameworks
- Unit and integration tests