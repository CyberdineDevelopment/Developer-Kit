# FractalDataWorks.CodeBuilder

A pure code generation library implementing the **TRUE Builder Pattern** for creating C# source code. Designed specifically for source generators and other code generation scenarios where immutable, composable builders are essential.

## Key Features

### 🏗️ True Builder Pattern Implementation
- **Immutable Product Classes**: `ClassDefinition`, `MethodDefinition`, `PropertyDefinition`, etc.
- **Separate Builder Classes**: `ClassBuilder`, `MethodBuilder`, `PropertyBuilder`, etc.
- **Each builder method returns a new builder instance** - no mutation of existing builders
- **Internal constructors** on products - only builders can create them

### 🔧 Pure Code Generation
- **No dependency on Roslyn or Tree-sitter** for generation (only for parsing if needed)
- **String-based code generation** with proper formatting and indentation
- **Visitor pattern support** for extensible code generation
- **Designed for source generators** - lightweight and fast

### 🎯 Better than SmartGenerators.CodeBuilders
- **Proper separation** between builders and products
- **Immutable builders** prevent accidental state mutation
- **Comprehensive validation** before building
- **Fluent API** with method chaining
- **Generic type parameter support**
- **Attribute and documentation generation**

## Architecture

```
FractalDataWorks.CodeBuilder/
├── Abstractions/           # Core interfaces (from separate project)
│   ├── IAstNode.cs        # Base interface for AST nodes
│   ├── IAstBuilder.cs     # Base interface for builders
│   ├── ICodeGenerator.cs  # Code generation interface
│   └── IMemberDefinition.cs # Interface for class members
├── Definitions/           # Immutable product classes
│   ├── ClassDefinition.cs
│   ├── MethodDefinition.cs
│   ├── PropertyDefinition.cs
│   ├── ParameterDefinition.cs
│   ├── ConstructorDefinition.cs
│   ├── AttributeDefinition.cs
│   └── GenericParameterDefinition.cs
├── Builders/              # Builder implementations
│   ├── ClassBuilder.cs
│   ├── MethodBuilder.cs
│   ├── PropertyBuilder.cs
│   ├── ParameterBuilder.cs
│   └── ConstructorBuilder.cs
├── Generators/            # Code generation
│   ├── CSharpCodeGenerator.cs
│   └── CodeFormatter.cs
├── Types/                 # Supporting types
│   └── Modifiers.cs       # Flags enum for modifiers
└── Examples/              # Usage examples
    └── OrderServiceExample.cs
```

## Usage Example

### Building a Service Class with the TRUE Builder Pattern

```csharp
// Each method returns a NEW builder instance - builders are immutable!
var serviceClass = new ClassBuilder()
    .WithName("OrderService")
    .WithAccess(AccessModifier.Public)
    .AsSealed()
    .AddBaseType("IOrderService")
    .AddAttribute("Service", "ServiceLifetime.Scoped")
    .WithDocumentation("Service for managing orders")
    
    // Add constructor with dependency injection
    .AddConstructor(ctor => ctor
        .WithAccess(AccessModifier.Public)
        .AddParameter("ILogger<OrderService>", "logger")
        .AddParameter("IOrderRepository", "repository")
        .WithBody("""
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            """))
    
    // Add async method
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
        .WithBody("return await Repository.GetByIdAsync(orderId, cancellationToken);")
        .AddAttribute("HttpGet", "\"{id}\""))
    
    // Add properties
    .AddProperty(prop => prop
        .WithName("Logger")
        .WithType("ILogger<OrderService>")
        .WithAccess(AccessModifier.Private)
        .MakeReadOnly())
    
    .Build(); // Returns immutable ClassDefinition

// Generate C# code
var generator = new CSharpCodeGenerator();
var code = generator.Generate(serviceClass);
```

### Generated Code Output

```csharp
/// <summary>
/// Service for managing orders
/// </summary>
[Service(ServiceLifetime.Scoped)]
public sealed class OrderService : IOrderService
{
    private ILogger<OrderService> Logger { get; }

    /// <summary>
    /// Initializes a new instance of the OrderService class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="repository">The order repository instance.</param>
    public OrderService(ILogger<OrderService> logger, IOrderRepository repository)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order if found; otherwise, null.</returns>
    [HttpGet("{id}")]
    public async Task<Order?> GetOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await Repository.GetByIdAsync(orderId, cancellationToken);
    }
}
```

## Integration with TreeSitter (from EnhancedEnums)

The CodeBuilder is designed to work seamlessly with TreeSitter parsing from your EnhancedEnums package:

### Parse → Transform → Generate Workflow

```csharp
// 1. Parse existing code with TreeSitter (from EnhancedEnums)
var parser = new TreeSitterParser();
var syntaxTree = parser.ParseCSharp(existingCode);

// 2. Convert TreeSitter AST to CodeBuilder definitions
var converter = new TreeSitterToCodeBuilderConverter();
var classDefinition = converter.ConvertClass(syntaxTree.RootNode);

// 3. Transform using builders (add methods, properties, etc.)
var enhancedClass = new ClassBuilder()
    .FromExisting(classDefinition)  // Start with parsed class
    .AddMethod(method => method
        .WithName("ToString")
        .WithReturnType("string")
        .AsOverride()
        .WithBody("return Name;"))
    .AddProperty(prop => prop
        .WithName("IsValid")
        .WithType("bool")
        .MakeReadOnly()
        .WithGetter("return !string.IsNullOrEmpty(Name);"))
    .Build();

// 4. Generate modified code
var generator = new CSharpCodeGenerator();
var modifiedCode = generator.Generate(enhancedClass);
```

### Source Generator Integration

```csharp
[Generator]
public class MySourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Parse existing code with TreeSitter
        // Transform with CodeBuilder
        // Generate new code
        
        var serviceClass = new ClassBuilder()
            .WithName($"{className}Service")
            .WithAccess(AccessModifier.Public)
            // ... build the class
            .Build();
        
        var generator = new CSharpCodeGenerator();
        var code = generator.Generate(serviceClass);
        
        context.AddSource($"{className}Service.g.cs", code);
    }
}
```

## Core Principles

### 1. TRUE Builder Pattern
- **Products are immutable** - once built, they cannot be changed
- **Builders are separate from products** - no inheritance or mixing
- **Each builder method returns a new builder instance** - no mutation
- **Internal constructors** on products ensure only builders can create them

### 2. Composability
- Builders can be composed and combined
- Methods accept `Action<TBuilder>` for nested configuration
- Support for conditional building with validation

### 3. Validation
- Builders validate state before building
- Clear error messages for invalid configurations
- Fail-fast approach prevents runtime errors

### 4. Performance
- Designed for source generator scenarios
- Minimal allocations during building
- Efficient string generation with proper formatting

## Benefits Over String Building

❌ **Traditional String Building Approach:**
```csharp
var code = new StringBuilder();
code.AppendLine("public class MyClass");
code.AppendLine("{");
code.AppendLine("    public string Name { get; set; }");
code.AppendLine("}");
// Error-prone, no validation, hard to maintain
```

✅ **CodeBuilder TRUE Builder Pattern:**
```csharp
var myClass = new ClassBuilder()
    .WithName("MyClass")
    .WithAccess(AccessModifier.Public)
    .AddProperty(prop => prop
        .WithName("Name")
        .WithType("string")
        .MakeReadWrite())
    .Build();
// Type-safe, validated, composable, maintainable
```

## Next Steps

1. **Complete Interface Builder** - Similar to ClassBuilder for interfaces
2. **Enum Builder** - For generating enum types
3. **Namespace Builder** - For complete file generation
4. **TreeSitter Integration Package** - Bridge between parsing and building
5. **Visual Studio Extension** - Design-time code building tools

This library provides the foundation for sophisticated code generation scenarios while maintaining the purity and immutability that makes builders reliable and predictable.