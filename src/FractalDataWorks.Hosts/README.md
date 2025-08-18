# FractalDataWorks.Hosts

Host abstractions and implementations for web applications and background services in the FractalDataWorks framework.

## Overview

FractalDataWorks.Hosts provides:
- Web host abstractions for ASP.NET Core integration
- Worker service patterns for background processing
- Health check endpoints and monitoring
- Graceful shutdown handling
- Service lifecycle management

## Planned Components

### IFdwHost

Base host abstraction:
```csharp
public interface IFdwHost
{
    string Name { get; }
    HostStatus Status { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
```

### IFdwWebHost

Web-specific host features:
```csharp
public interface IFdwWebHost : IFdwHost
{
    int Port { get; }
    string[] Urls { get; }
    void ConfigureEndpoints(IEndpointRouteBuilder endpoints);
    void ConfigureMiddleware(IApplicationBuilder app);
}
```

### IFdwWorkerHost

Background service host:
```csharp
public interface IFdwWorkerHost : IFdwHost
{
    TimeSpan ExecutionInterval { get; }
    bool RunContinuously { get; }
    Task ExecuteAsync(CancellationToken stoppingToken);
}
```

### Base Host Implementations

```csharp
public abstract class FdwHostBase : IFdwHost, IHostedService
{
    protected readonly ILogger Logger;
    protected readonly IHostApplicationLifetime Lifetime;
    
    public string Name { get; }
    public HostStatus Status { get; protected set; }
    
    protected FdwHostBase(
        string name,
        ILogger logger,
        IHostApplicationLifetime lifetime)
    {
        Name = name;
        Logger = logger;
        Lifetime = lifetime;
        Status = HostStatus.Stopped;
    }
    
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation(ServiceMessages.ServiceStarted.Format(Name));
        Status = HostStatus.Running;
        
        Lifetime.ApplicationStarted.Register(() =>
        {
            Logger.LogInformation($"{Name} fully started");
        });
        
        Lifetime.ApplicationStopping.Register(() =>
        {
            Logger.LogInformation($"{Name} is shutting down...");
            Status = HostStatus.Stopping;
        });
    }
    
    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation(ServiceMessages.ServiceStopped.Format(Name));
        Status = HostStatus.Stopped;
    }
}
```

## Planned Features

### Worker Service Base

```csharp
public abstract class FdwWorkerBase<TService, TConfiguration> : BackgroundService, IFdwWorkerHost
    where TService : IFdwService<TConfiguration>
    where TConfiguration : class, IFdwConfiguration
{
    private readonly TService _service;
    private readonly ILogger _logger;
    
    public string Name => GetType().Name;
    public HostStatus Status { get; private set; }
    public abstract TimeSpan ExecutionInterval { get; }
    public virtual bool RunContinuously => true;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && RunContinuously)
        {
            try
            {
                Status = HostStatus.Running;
                await ProcessAsync(stoppingToken);
                
                await Task.Delay(ExecutionInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ServiceMessages.OperationFailed.Format(Name, ex.Message));
                Status = HostStatus.Error;
                
                // Wait before retrying
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        
        Status = HostStatus.Stopped;
    }
    
    protected abstract Task ProcessAsync(CancellationToken cancellationToken);
}
```

### Web API Integration

```csharp
public static class WebHostExtensions
{
    public static IServiceCollection AddFdwWebHost<THost>(
        this IServiceCollection services,
        Action<FdwWebHostOptions>? configure = null)
        where THost : class, IFdwWebHost
    {
        var options = new FdwWebHostOptions();
        configure?.Invoke(options);
        
        services.Configure<FdwWebHostOptions>(opt =>
        {
            opt.EnableHealthChecks = options.EnableHealthChecks;
            opt.EnableMetrics = options.EnableMetrics;
            opt.EnableSwagger = options.EnableSwagger;
        });
        
        services.AddSingleton<THost>();
        services.AddHostedService<THost>();
        
        if (options.EnableHealthChecks)
        {
            services.AddHealthChecks()
                .AddCheck<FdwHealthCheck>("fdw_services");
        }
        
        return services;
    }
    
    public static IApplicationBuilder UseFdwWebHost(
        this IApplicationBuilder app,
        IFdwWebHost host)
    {
        host.ConfigureMiddleware(app);
        return app;
    }
    
    public static IEndpointRouteBuilder MapFdwEndpoints(
        this IEndpointRouteBuilder endpoints,
        IFdwWebHost host)
    {
        host.ConfigureEndpoints(endpoints);
        
        // Map health check endpoint
        endpoints.MapHealthChecks("/health");
        
        // Map service status endpoint
        endpoints.MapGet("/api/status", async context =>
        {
            var status = new
            {
                host = host.Name,
                status = host.Status.ToString(),
                timestamp = DateTime.UtcNow
            };
            
            await context.Response.WriteAsJsonAsync(status);
        });
        
        return endpoints;
    }
}
```

### Health Checks

```csharp
public class FdwHealthCheck : IHealthCheck
{
    private readonly IEnumerable<IFdwService> _services;
    
    public FdwHealthCheck(IEnumerable<IFdwService> services)
    {
        _services = services;
    }
    
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var unhealthyServices = _services.Where(s => !s.IsHealthy).ToList();
        
        if (!unhealthyServices.Any())
        {
            return Task.FromResult(HealthCheckResult.Healthy("All services are healthy"));
        }
        
        var data = new Dictionary<string, object>
        {
            ["unhealthy_count"] = unhealthyServices.Count,
            ["unhealthy_services"] = unhealthyServices.Select(s => s.ServiceName)
        };
        
        return Task.FromResult(HealthCheckResult.Unhealthy(
            $"{unhealthyServices.Count} services are unhealthy",
            data: data));
    }
}
```

## Usage Examples (Planned)

### Creating a Worker Service
```csharp
public class OrderProcessingWorker : FdwWorkerBase<IOrderService, OrderConfiguration>
{
    public override TimeSpan ExecutionInterval => TimeSpan.FromMinutes(5);
    
    public OrderProcessingWorker(
        IOrderService orderService,
        ILogger<OrderProcessingWorker> logger)
        : base(orderService, logger)
    {
    }
    
    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var command = new ProcessPendingOrdersCommand();
        var result = await Service.Execute<ProcessingResult>(command);
        
        if (result.IsSuccess)
        {
            Logger.LogInformation($"Processed {result.Value.OrderCount} orders");
        }
    }
}

// Registration
services.AddFdwWorkerHost<OrderProcessingWorker>();
```

### Creating a Web API Host
```csharp
public class ApiHost : FdwWebHostBase
{
    private readonly IServiceProvider _serviceProvider;
    
    public override int Port => 5000;
    public override string[] Urls => new[] { "http://localhost:5000", "https://localhost:5001" };
    
    public ApiHost(IServiceProvider serviceProvider, ILogger<ApiHost> logger)
        : base("FdwDataWorks API", logger)
    {
        _serviceProvider = serviceProvider;
    }
    
    public override void ConfigureMiddleware(IApplicationBuilder app)
    {
        app.UseExceptionHandler("/error");
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
    }
    
    public override void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints.MapGrpcService<OrderGrpcService>();
    }
}

// In Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddFdwWebHost<ApiHost>(options =>
    {
        options.EnableHealthChecks = true;
        options.EnableMetrics = true;
        options.EnableSwagger = true;
    });
}

public void Configure(IApplicationBuilder app, IFdwWebHost host)
{
    app.UseFdwWebHost(host);
    
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapFdwEndpoints(host);
    });
}
```

### Graceful Shutdown
```csharp
public class GracefulShutdownWorker : FdwWorkerBase<ICleanupService, CleanupConfiguration>
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Register cleanup on shutdown
        var registration = stoppingToken.Register(async () =>
        {
            Logger.LogInformation("Performing graceful shutdown...");
            await PerformCleanupAsync();
        });
        
        await base.ExecuteAsync(stoppingToken);
        
        registration.Dispose();
    }
    
    private async Task PerformCleanupAsync()
    {
        // Close connections
        // Flush caches
        // Complete pending operations
        await Service.Execute<CleanupResult>(new CleanupCommand());
    }
}
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.Hosts" Version="*" />
```

## Dependencies

- FractalDataWorks.Services (core abstractions)
- FractalDataWorks.Services
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Hosting.Abstractions
- Microsoft.AspNetCore.Http.Abstractions (for web hosts)

## Status

This package is currently in planning phase. The interfaces and implementations described above represent the intended design and may change during development.

## Contributing

This package is accepting contributions for:
- Host abstraction definitions
- Worker service implementations
- Web host integrations
- Health check providers
- Monitoring and metrics integration
- Unit and integration tests