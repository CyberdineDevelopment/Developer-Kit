using System;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.CodeBuilder.Builders;
using FractalDataWorks.CodeBuilder.Generators;
using FractalDataWorks.CodeBuilder.Types;

namespace FractalDataWorks.CodeBuilder.Examples;

/// <summary>
/// Example demonstrating the TRUE builder pattern for code generation.
/// This example shows how to build a complete service class with the immutable builder pattern.
/// </summary>
public static class OrderServiceExample
{
    /// <summary>
    /// Demonstrates building a complete OrderService class using the TRUE builder pattern.
    /// Each builder method returns a new immutable builder instance.
    /// </summary>
    /// <returns>Generated C# code for the OrderService class.</returns>
    public static string GenerateOrderService()
    {
        // Build the OrderService class using the TRUE builder pattern
        var orderServiceClass = new ClassBuilder()
            .WithName("OrderService")
            .WithAccess(AccessModifier.Public)
            .AsSealed()
            .AddBaseType("IOrderService")
            .AddAttribute("Service", "ServiceLifetime.Scoped")
            .WithDocumentation("""
                <summary>
                Service for managing orders in the e-commerce system.
                Provides methods for creating, updating, and retrieving orders.
                </summary>
                """)
            
            // Add private readonly fields
            .AddProperty(prop => prop
                .WithName("Logger")
                .WithType("ILogger<OrderService>")
                .WithAccess(AccessModifier.Private)
                .MakeReadOnly())
            
            .AddProperty(prop => prop
                .WithName("Repository")
                .WithType("IOrderRepository")
                .WithAccess(AccessModifier.Private)
                .MakeReadOnly())
            
            // Add constructor
            .AddConstructor(ctor => ctor
                .WithAccess(AccessModifier.Public)
                .AddParameter("ILogger<OrderService>", "logger")
                .AddParameter("IOrderRepository", "repository")
                .WithBody("""
                    Logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    Repository = repository ?? throw new ArgumentNullException(nameof(repository));
                    """)
                .WithDocumentation("""
                    <summary>
                    Initializes a new instance of the OrderService class.
                    </summary>
                    <param name="logger">The logger instance.</param>
                    <param name="repository">The order repository instance.</param>
                    <exception cref="ArgumentNullException">Thrown when logger or repository is null.</exception>
                    """))
            
            // Add async method with generic parameters
            .AddMethod(method => method
                .WithName("GetOrderAsync")
                .WithReturnType("Task<Order?>")
                .WithAccess(AccessModifier.Public)
                .AsAsync()
                .AddParameter("int", "orderId")
                .AddParameter(param => param
                    .WithName("cancellationToken")
                    .WithType("CancellationToken")
                    .WithDefaultValue("default"))
                .WithBody("""
                    Logger.LogInformation("Retrieving order with ID: {OrderId}", orderId);
                    
                    try
                    {
                        var order = await Repository.GetByIdAsync(orderId, cancellationToken);
                        
                        if (order == null)
                        {
                            Logger.LogWarning("Order not found: {OrderId}", orderId);
                        }
                        
                        return order;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error retrieving order: {OrderId}", orderId);
                        throw;
                    }
                    """)
                .AddAttribute("HttpGet", "\"{id}\"")
                .WithDocumentation("""
                    <summary>
                    Retrieves an order by its unique identifier.
                    </summary>
                    <param name="orderId">The unique identifier of the order.</param>
                    <param name="cancellationToken">Cancellation token.</param>
                    <returns>The order if found; otherwise, null.</returns>
                    <exception cref="ArgumentException">Thrown when orderId is invalid.</exception>
                    """))
            
            // Add method with validation
            .AddMethod(method => method
                .WithName("CreateOrderAsync")
                .WithReturnType("Task<Order>")
                .WithAccess(AccessModifier.Public)
                .AsAsync()
                .AddParameter("CreateOrderRequest", "request")
                .AddParameter("CancellationToken", "cancellationToken")
                .WithBody("""
                    if (request == null)
                        throw new ArgumentNullException(nameof(request));
                    
                    Logger.LogInformation("Creating new order for customer: {CustomerId}", request.CustomerId);
                    
                    var order = new Order
                    {
                        CustomerId = request.CustomerId,
                        Items = request.Items.ToList(),
                        CreatedAt = DateTime.UtcNow,
                        Status = OrderStatus.Pending
                    };
                    
                    var createdOrder = await Repository.CreateAsync(order, cancellationToken);
                    
                    Logger.LogInformation("Order created successfully: {OrderId}", createdOrder.Id);
                    
                    return createdOrder;
                    """)
                .AddAttribute("HttpPost")
                .WithDocumentation("""
                    <summary>
                    Creates a new order from the provided request.
                    </summary>
                    <param name="request">The order creation request.</param>
                    <param name="cancellationToken">Cancellation token.</param>
                    <returns>The created order.</returns>
                    <exception cref="ArgumentNullException">Thrown when request is null.</exception>
                    """))
            
            // Add static utility method
            .AddMethod(method => method
                .WithName("ValidateOrderId")
                .WithReturnType("bool")
                .WithAccess(AccessModifier.Private)
                .AsStatic()
                .AddParameter("int", "orderId")
                .WithBody("""
                    return orderId > 0;
                    """))
            
            .Build(); // Build the immutable ClassDefinition

        // Generate C# code from the built class definition
        var generator = new CSharpCodeGenerator();
        var options = new CodeGenerationOptions
        {
            GenerateDocumentation = true,
            GenerateAttributes = true,
            UseFileScopedNamespaces = true,
            Indentation = "    ", // 4 spaces
            MaxLineLength = 120
        };

        return generator.Generate(orderServiceClass, options);
    }

    /// <summary>
    /// Demonstrates building a simple data model class.
    /// </summary>
    /// <returns>Generated C# code for a simple Order class.</returns>
    public static string GenerateOrderModel()
    {
        var orderClass = new ClassBuilder()
            .WithName("Order")
            .WithAccess(AccessModifier.Public)
            .AsSealed()
            .WithDocumentation("""
                <summary>
                Represents an order in the e-commerce system.
                </summary>
                """)
            
            // Add auto-implemented properties
            .AddProperty(prop => prop
                .WithName("Id")
                .WithType("int")
                .WithAccess(AccessModifier.Public)
                .MakeReadOnly())
            
            .AddProperty(prop => prop
                .WithName("CustomerId")
                .WithType("int")
                .WithAccess(AccessModifier.Public)
                .MakeReadWrite())
            
            .AddProperty(prop => prop
                .WithName("Items")
                .WithType("List<OrderItem>")
                .WithAccess(AccessModifier.Public)
                .MakeReadWrite())
            
            .AddProperty(prop => prop
                .WithName("Status")
                .WithType("OrderStatus")
                .WithAccess(AccessModifier.Public)
                .MakeReadWrite())
            
            .AddProperty(prop => prop
                .WithName("CreatedAt")
                .WithType("DateTime")
                .WithAccess(AccessModifier.Public)
                .WithInit()) // C# 9+ init-only property
            
            .AddProperty(prop => prop
                .WithName("UpdatedAt")
                .WithType("DateTime?")
                .WithAccess(AccessModifier.Public)
                .MakeReadWrite())
            
            .Build();

        var generator = new CSharpCodeGenerator();
        return generator.Generate(orderClass);
    }

    /// <summary>
    /// Demonstrates creating a generic repository interface.
    /// </summary>
    /// <returns>Generated C# code for a generic repository interface.</returns>
    public static string GenerateGenericRepository()
    {
        // Note: InterfaceBuilder would need to be implemented similar to ClassBuilder
        // This is a conceptual example of what the interface generation would look like
        
        var repositoryInterface = new ClassBuilder() // Would be InterfaceBuilder in complete implementation
            .WithName("IRepository")
            .WithAccess(AccessModifier.Public)
            .AddGenericParameter("TEntity", "class")
            .AddGenericParameter("TKey", "struct")
            .WithDocumentation("""
                <summary>
                Generic repository interface for data access operations.
                </summary>
                <typeparam name="TEntity">The entity type.</typeparam>
                <typeparam name="TKey">The key type.</typeparam>
                """)
            
            .AddMethod(method => method
                .WithName("GetByIdAsync")
                .WithReturnType("Task<TEntity?>")
                .WithAccess(AccessModifier.Public)
                .AddParameter("TKey", "id")
                .AddParameter("CancellationToken", "cancellationToken")
                .WithDocumentation("""
                    <summary>
                    Retrieves an entity by its unique identifier.
                    </summary>
                    <param name="id">The entity identifier.</param>
                    <param name="cancellationToken">Cancellation token.</param>
                    <returns>The entity if found; otherwise, null.</returns>
                    """))
            
            .AddMethod(method => method
                .WithName("CreateAsync")
                .WithReturnType("Task<TEntity>")
                .WithAccess(AccessModifier.Public)
                .AddParameter("TEntity", "entity")
                .AddParameter("CancellationToken", "cancellationToken"))
            
            .Build();

        var generator = new CSharpCodeGenerator();
        return generator.Generate(repositoryInterface);
    }
}