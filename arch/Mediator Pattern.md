# EnhancedMediator Design

## Overview

EnhancedMediator implements the Mediator pattern using Enhanced Enums as the discovery mechanism, providing automatic handler registration, pipeline behaviors, and compile-time safety with runtime extensibility.

## Core Architecture

### 1. Request/Response Pattern with Enhanced Enums

```csharp
// Base request interface
public interface IRequest<TResponse>
{
    Guid RequestId { get; }
    DateTime Timestamp { get; }
}

public interface IRequest : IRequest<Unit> { }

// Enhanced Enum for request handlers
[EnhancedEnum("RequestHandlers", IncludeReferencedAssemblies = true)]
public abstract record RequestHandlerEnum(string Name) : IEnhancedEnum<RequestHandlerEnum>
{
    public abstract Type RequestType { get; }
    public abstract Type ResponseType { get; }
    public abstract Type HandlerType { get; }
    public abstract int Priority { get; } // For multiple handlers
    
    public abstract Task<object> HandleAsync(
        IRequest request, 
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

// Concrete handler implementation
[EnumOption(Name = "CreateUserHandler")]
public record CreateUserHandlerEnum() : RequestHandlerEnum("CreateUserHandler")
{
    public override Type RequestType => typeof(CreateUserCommand);
    public override Type ResponseType => typeof(User);
    public override Type HandlerType => typeof(CreateUserHandler);
    public override int Priority => 100;
    
    public override async Task<object> HandleAsync(
        IRequest request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = (CreateUserHandler)serviceProvider.GetRequiredService(HandlerType);
        return await handler.Handle((CreateUserCommand)request, cancellationToken);
    }
}
```

### 2. Notification Pattern (Multiple Handlers)

```csharp
// Notification interface
public interface INotification
{
    Guid NotificationId { get; }
    DateTime Timestamp { get; }
}

// Enhanced Enum for notification handlers
[EnhancedEnum("NotificationHandlers", IncludeReferencedAssemblies = true)]
public abstract record NotificationHandlerEnum(string Name) : IEnhancedEnum<NotificationHandlerEnum>
{
    public abstract Type NotificationType { get; }
    public abstract Type HandlerType { get; }
    public abstract int Order { get; } // Execution order
    
    public abstract Task HandleAsync(
        INotification notification,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

// Multiple handlers for same notification
[EnumOption(Name = "UserCreatedEmailHandler", Order = 10)]
public record UserCreatedEmailHandler() : NotificationHandlerEnum("UserCreatedEmailHandler")
{
    public override Type NotificationType => typeof(UserCreatedNotification);
    public override Type HandlerType => typeof(SendEmailHandler);
    public override int Order => 10;
    
    public override async Task HandleAsync(
        INotification notification,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<SendEmailHandler>();
        await handler.HandleUserCreated((UserCreatedNotification)notification);
    }
}

[EnumOption(Name = "UserCreatedAuditHandler", Order = 20)]
public record UserCreatedAuditHandler() : NotificationHandlerEnum("UserCreatedAuditHandler")
{
    public override Type NotificationType => typeof(UserCreatedNotification);
    public override Type HandlerType => typeof(AuditLogHandler);
    public override int Order => 20;
    
    // Implementation...
}
```

### 3. Pipeline Behaviors with Smart Delegates

```csharp
// Pipeline behavior using Smart Delegates
[SmartDelegate("MediatorPipeline", ContextType = typeof(MediatorContext))]
public abstract record PipelineBehaviorEnum(string Name, int Order) 
    : SmartDelegateBase<MediatorContext>(Name, Order)
{
    public abstract bool AppliesTo(Type requestType);
}

// Validation behavior
[Stage(Order = 10, Category = "Validation")]
public record ValidationBehavior() : PipelineBehaviorEnum("Validation", 10)
{
    public override bool AppliesTo(Type requestType) => true; // Apply to all
    
    public override async Task<Result<MediatorContext>> ExecuteAsync(
        MediatorContext context,
        CancellationToken cancellationToken)
    {
        if (context.Request is IValidatable validatable)
        {
            var validationResult = await validatable.ValidateAsync();
            if (validationResult.IsFailure)
            {
                context.Response = Result<object>.Fail(validationResult.Error);
                context.ContinueProcessing = false;
            }
        }
        
        return Result<MediatorContext>.Ok(context);
    }
}

// Caching behavior
[Stage(Order = 5, Category = "Performance")]
public record CachingBehavior() : PipelineBehaviorEnum("Caching", 5)
{
    public override bool AppliesTo(Type requestType) => 
        requestType.GetCustomAttribute<CacheableAttribute>() != null;
        
    public override async Task<Result<MediatorContext>> ExecuteAsync(
        MediatorContext context,
        CancellationToken cancellationToken)
    {
        var cache = context.ServiceProvider.GetRequiredService<ICache>();
        var cacheKey = GenerateCacheKey(context.Request);
        
        var cachedResult = await cache.GetAsync<object>(cacheKey);
        if (cachedResult != null)
        {
            context.Response = Result<object>.Ok(cachedResult);
            context.ContinueProcessing = false; // Skip handler
        }
        
        return Result<MediatorContext>.Ok(context);
    }
}
```

### 4. The Mediator Implementation

```csharp
public interface IEnhancedMediator
{
    Task<Result<TResponse>> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
        
    Task<Result<Unit>> PublishAsync(
        INotification notification,
        CancellationToken cancellationToken = default);
}

public class EnhancedMediator : IEnhancedMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MediatorPipelineProvider _pipelineProvider;
    private readonly ILogger<EnhancedMediator> _logger;
    
    // Static handler cache for performance
    private static readonly Dictionary<Type, RequestHandlerEnum> _handlerCache;
    private static readonly Dictionary<Type, List<NotificationHandlerEnum>> _notificationHandlerCache;
    
    static EnhancedMediator()
    {
        // Build caches from Enhanced Enum collections
        _handlerCache = RequestHandlers.All
            .GroupBy(h => h.RequestType)
            .ToDictionary(g => g.Key, g => g.OrderBy(h => h.Priority).First());
            
        _notificationHandlerCache = NotificationHandlers.All
            .GroupBy(h => h.NotificationType)
            .ToDictionary(g => g.Key, g => g.OrderBy(h => h.Order).ToList());
    }
    
    public EnhancedMediator(
        IServiceProvider serviceProvider,
        MediatorPipelineProvider pipelineProvider,
        ILogger<EnhancedMediator> logger)
    {
        _serviceProvider = serviceProvider;
        _pipelineProvider = pipelineProvider;
        _logger = logger;
    }
    
    public async Task<Result<TResponse>> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        
        if (!_handlerCache.TryGetValue(requestType, out var handlerEnum))
        {
            return Result<TResponse>.Fail($"No handler registered for {requestType.Name}");
        }
        
        // Create mediator context
        var context = new MediatorContext
        {
            Request = request,
            RequestType = requestType,
            ServiceProvider = _serviceProvider,
            Items = new Dictionary<string, object>()
        };
        
        // Execute pipeline
        var pipelineResult = await _pipelineProvider.ExecuteAsync(context, cancellationToken);
        
        if (pipelineResult.IsFailure)
        {
            return Result<TResponse>.Fail(pipelineResult.Error);
        }
        
        // If pipeline set response (e.g., from cache), return it
        if (context.Response != null && !context.ContinueProcessing)
        {
            return context.Response.Map(r => (TResponse)r);
        }
        
        // Execute handler
        try
        {
            var response = await handlerEnum.HandleAsync(request, _serviceProvider, cancellationToken);
            
            // Post-processing behaviors
            context.Response = Result<object>.Ok(response);
            var postResult = await _pipelineProvider.ExecutePostAsync(context, cancellationToken);
            
            return Result<TResponse>.Ok((TResponse)response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request {RequestType}", requestType.Name);
            return Result<TResponse>.Fail($"Handler error: {ex.Message}");
        }
    }
    
    public async Task<Result<Unit>> PublishAsync(
        INotification notification,
        CancellationToken cancellationToken = default)
    {
        var notificationType = notification.GetType();
        
        if (!_notificationHandlerCache.TryGetValue(notificationType, out var handlers))
        {
            _logger.LogWarning("No handlers for notification {NotificationType}", notificationType.Name);
            return Result<Unit>.Ok(Unit.Value);
        }
        
        var errors = new List<string>();
        
        foreach (var handlerEnum in handlers)
        {
            try
            {
                await handlerEnum.HandleAsync(notification, _serviceProvider, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification handler {Handler}", handlerEnum.Name);
                errors.Add($"{handlerEnum.Name}: {ex.Message}");
            }
        }
        
        return errors.Any() 
            ? Result<Unit>.Fail($"Some handlers failed: {string.Join("; ", errors)}")
            : Result<Unit>.Ok(Unit.Value);
    }
}
```

### 5. Handler Base Classes

```csharp
// Traditional handler interface for compatibility
public interface IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

// Base class with common functionality
public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected readonly ILogger Logger;
    
    protected RequestHandlerBase(ILogger logger)
    {
        Logger = logger;
    }
    
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    
    protected Result<T> ExecuteWithLogging<T>(Func<Result<T>> operation, [CallerMemberName] string operationName = "")
    {
        try
        {
            Logger.LogDebug("Executing {Operation} for {Request}", operationName, typeof(TRequest).Name);
            return operation();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in {Operation}", operationName);
            return Result<T>.Fail($"Operation failed: {ex.Message}");
        }
    }
}
```

### 6. Query Separation

```csharp
// Separate enum for queries vs commands
[EnhancedEnum("QueryHandlers", IncludeReferencedAssemblies = true)]
public abstract record QueryHandlerEnum(string Name) : RequestHandlerEnum(Name)
{
    public abstract bool SupportsProjection { get; }
    public abstract bool SupportsPaging { get; }
    public abstract bool SupportsFiltering { get; }
}

[EnumOption(Name = "GetUsersQuery")]
public record GetUsersQueryHandler() : QueryHandlerEnum("GetUsersQuery")
{
    public override Type RequestType => typeof(GetUsersQuery);
    public override Type ResponseType => typeof(PagedResult<User>);
    public override bool SupportsProjection => true;
    public override bool SupportsPaging => true;
    public override bool SupportsFiltering => true;
    
    // Implementation...
}
```

### 7. Source Generator Components

The source generator would:

1. **Discover all handlers** marked with Enhanced Enum attributes
2. **Generate handler registration** code
3. **Create strongly-typed mediator extensions**
4. **Generate pipeline configuration** from attributes
5. **Create diagnostic analyzers** to ensure handlers exist for requests

Generated extension methods:
```csharp
// Generated: MediatorExtensions.g.cs
public static class MediatorExtensions
{
    public static Task<Result<User>> CreateUserAsync(
        this IEnhancedMediator mediator,
        string name,
        string email,
        CancellationToken cancellationToken = default)
    {
        return mediator.SendAsync(new CreateUserCommand(name, email), cancellationToken);
    }
    
    public static Task<Result<PagedResult<User>>> GetUsersAsync(
        this IEnhancedMediator mediator,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return mediator.SendAsync(new GetUsersQuery(page, pageSize), cancellationToken);
    }
}
```

### 8. Advanced Features

#### Dynamic Handler Registration
```csharp
public interface IHandlerRegistry
{
    void Register<TRequest, TResponse, THandler>()
        where TRequest : IRequest<TResponse>
        where THandler : IRequestHandler<TRequest, TResponse>;
        
    void Unregister<TRequest>();
    
    bool IsRegistered<TRequest>();
}
```

#### Handler Versioning
```csharp
[EnumOption(Name = "CreateUserV2", Version = 2)]
public record CreateUserHandlerV2() : RequestHandlerEnum("CreateUserV2")
{
    // New implementation with version support
}
```

#### Streaming Support
```csharp
public interface IStreamRequest<T> : IRequest<IAsyncEnumerable<T>> { }

[EnhancedEnum("StreamHandlers")]
public abstract record StreamHandlerEnum(string Name) : IEnhancedEnum<StreamHandlerEnum>
{
    public abstract IAsyncEnumerable<object> HandleStreamAsync(
        IStreamRequest request,
        CancellationToken cancellationToken);
}
```

## Benefits Over Traditional MediatR

1. **Zero Reflection at Runtime** - All handlers discovered at compile time
2. **No Manual Registration** - Enhanced Enums handle discovery
3. **Better IntelliSense** - Generated extension methods
4. **Pipeline Customization** - Smart Delegates for behaviors
5. **Handler Metadata** - Query capabilities, versioning, etc.
6. **Compile-Time Validation** - Analyzer ensures handlers exist
7. **Performance** - Static caching of handler lookups
8. **Extensibility** - New handler types via Enhanced Enums

## Usage Example

```csharp
// Startup
services.AddEnhancedMediator(options =>
{
    options.EnablePipelineBehaviors = true;
    options.DefaultTimeout = TimeSpan.FromSeconds(30);
    options.EnableDiagnostics = true;
});

// Usage
public class UserController : ControllerBase
{
    private readonly IEnhancedMediator _mediator;
    
    public UserController(IEnhancedMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        // Type-safe, discovered handler
        var result = await _mediator.CreateUserAsync(dto.Name, dto.Email);
        
        return result.Match(
            success: user => Ok(user),
            failure: error => BadRequest(error)
        );
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUsers(int page = 1)
    {
        // Strongly-typed query with automatic handler discovery
        var result = await _mediator.GetUsersAsync(page, 20);
        
        return result.Match(
            success: users => Ok(users),
            failure: error => StatusCode(500, error)
        );
    }
}
```

## Integration with FractalDataWorks SDK

This would integrate perfectly with the existing architecture:

1. **Services use mediator** instead of direct dependencies
2. **Handlers are discovered** like other Enhanced Enums
3. **Pipeline behaviors** handle cross-cutting concerns
4. **Same Result<T> pattern** throughout
5. **Configuration-driven** handler selection

This creates a powerful, extensible mediator implementation that maintains all the benefits of MediatR while adding compile-time safety and automatic discovery.