# FractalDataWorks.CodeBuilder

The **FractalDataWorks.CodeBuilder** project provides concrete implementations of the code generation framework defined in `FractalDataWorks.CodeBuilder.Abstractions`. This package delivers production-ready builders, parsers, and generators for C# code generation with Roslyn-based syntax analysis.

## Overview and Purpose

This implementation package provides:
- **Complete C# Code Builders**: Fully-featured builders for all C# language constructs
- **Roslyn-Based Parsing**: High-performance C# code parsing using Microsoft.CodeAnalysis
- **Code Generation Engine**: Template-driven code generation with extensible language support
- **Language Registry**: Built-in registry with C# support and extensibility for additional languages

The package is designed for production use in code generators, scaffolding tools, source generators, and automated refactoring applications.

## Key Implementations

### Base Infrastructure

#### `CodeBuilderBase`
Abstract base class providing common functionality for all concrete builders:
- **StringBuilder Management**: Efficient string building with proper memory management
- **Indentation Handling**: Automatic indentation with configurable indentation strings
- **Line Management**: Clean line appending with proper formatting
- **Builder State**: Centralized state management and cleanup

**Key Features:**
- Configurable indentation (spaces, tabs, custom strings)
- Efficient memory usage through StringBuilder reuse
- Consistent formatting across all builder types
- Thread-safe implementation for concurrent generation

### C# Language Builders

#### `ClassBuilder`
Complete implementation of `IClassBuilder` supporting all modern C# class features:

**Supported Features:**
- **Namespace and Usings**: File-scoped namespaces and organized using directives
- **Class Modifiers**: public, internal, abstract, sealed, static, partial
- **Inheritance**: Base classes and multiple interface implementation
- **Generic Types**: Type parameters with complex constraint support
- **Members**: Fields, properties, methods, constructors, nested classes
- **Attributes**: Class-level and member-specific attributes
- **Documentation**: XML documentation with multi-line support

**Example Usage:**
```csharp
var classBuilder = new ClassBuilder()
    .WithNamespace("MyProject.Domain")
    .WithUsings("System", "System.ComponentModel.DataAnnotations")
    .WithName("Customer")
    .WithAccessModifier("public")
    .AsSealed()
    .WithBaseClass("EntityBase")
    .WithInterfaces("IValidatableObject", "INotifyPropertyChanged")
    .WithGenericParameters("TKey")
    .WithGenericConstraint("TKey", "struct", "IComparable<TKey>")
    .WithAttribute("Table(\"Customers\")")
    .WithXmlDoc("Represents a customer entity with validation support");

var idProperty = new PropertyBuilder()
    .WithName("Id")
    .WithType("TKey")
    .AsReadOnly()
    .WithAttribute("Key")
    .WithXmlDoc("Gets the unique identifier for this customer");

classBuilder.WithProperty(idProperty);
```

#### `MethodBuilder`
Comprehensive method generation supporting all C# method patterns:

**Supported Features:**
- **Method Types**: Instance, static, abstract, virtual, override methods
- **Async Support**: Full async/await pattern support with proper Task return types
- **Generic Methods**: Generic type parameters with inheritance and constraint support
- **Parameters**: Required, optional, params, ref, out, in parameter modifiers
- **Body Types**: Block body, expression body, abstract (no body)
- **Documentation**: Complete XML documentation with parameter and return descriptions

**Example Usage:**
```csharp
var methodBuilder = new MethodBuilder()
    .WithName("ProcessAsync")
    .WithReturnType("Task<ProcessResult<T>>")
    .WithGenericParameters("T")
    .WithGenericConstraint("T", "class", "IProcessable")
    .WithParameter("T", "item")
    .WithParameter("CancellationToken", "cancellationToken", "default")
    .AsAsync()
    .WithAttribute("RequireAuthentication")
    .WithXmlDoc("Processes an item asynchronously with cancellation support")
    .WithParamDoc("item", "The item to process")
    .WithParamDoc("cancellationToken", "Token to cancel the operation")
    .WithReturnDoc("The result of the processing operation")
    .AddBodyLine("ArgumentNullException.ThrowIfNull(item);")
    .AddBodyLine("using var scope = _logger.BeginScope(\"Processing {ItemType}\", typeof(T).Name);")
    .AddBodyLine("return await _processor.ProcessAsync(item, cancellationToken);");
```

#### `PropertyBuilder`
Modern C# property generation with full language feature support:

**Supported Features:**
- **Property Types**: Auto-properties, computed properties, backing field properties
- **Accessors**: Get, set, init-only setters with individual access modifiers
- **Property Bodies**: Expression body properties and custom accessor implementations
- **Modifiers**: Static, virtual, override, abstract properties
- **Initializers**: Property initializers and backing field initialization

**Example Usage:**
```csharp
var propertyBuilder = new PropertyBuilder()
    .WithName("FullName")
    .WithType("string")
    .WithAccessModifier("public")
    .AsVirtual()
    .WithExpressionBody("$\"{FirstName} {LastName}\"")
    .WithXmlDoc("Gets the full name by combining first and last name");

// Auto-property with init setter
var autoPropertyBuilder = new PropertyBuilder()
    .WithName("CreatedAt")
    .WithType("DateTime")
    .WithAccessModifier("public")
    .AsReadOnly()
    .WithInitSetter()
    .WithInitializer("DateTime.UtcNow")
    .WithXmlDoc("Gets the creation timestamp");
```

#### `ConstructorBuilder`
Complete constructor generation including static constructors:

**Supported Features:**
- **Constructor Types**: Instance constructors, static constructors
- **Parameters**: Full parameter support including default values
- **Constructor Chaining**: Base constructor calls and this() chaining
- **Body Generation**: Multi-line constructor body support

**Example Usage:**
```csharp
var constructorBuilder = new ConstructorBuilder()
    .WithClassName("Customer")
    .WithAccessModifier("public")
    .WithParameter("string", "firstName")
    .WithParameter("string", "lastName")
    .WithParameter("DateTime?", "dateOfBirth", "null")
    .WithBaseCall("Guid.NewGuid()")
    .WithXmlDoc("Initializes a new customer instance")
    .WithParamDoc("firstName", "The customer's first name")
    .WithParamDoc("lastName", "The customer's last name")
    .WithParamDoc("dateOfBirth", "Optional date of birth")
    .AddBodyLine("ArgumentException.ThrowIfNullOrWhiteSpace(firstName);")
    .AddBodyLine("ArgumentException.ThrowIfNullOrWhiteSpace(lastName);")
    .AddBodyLine("FirstName = firstName;")
    .AddBodyLine("LastName = lastName;")
    .AddBodyLine("DateOfBirth = dateOfBirth;");
```

#### `FieldBuilder`
Field generation supporting all C# field modifiers:

**Supported Features:**
- **Field Types**: Instance fields, static fields, constants, readonly fields
- **Modifiers**: volatile, readonly, const with proper validation
- **Initializers**: Field initialization with compile-time and runtime values

#### `PropertyBuilder` (Advanced Features)
Additional advanced property patterns:

```csharp
// Property with custom getter/setter
var advancedProperty = new PropertyBuilder()
    .WithName("Status")
    .WithType("CustomerStatus")
    .WithAccessModifier("public")
    .WithGetter("return _status;")
    .WithSetter("_status = value; OnPropertyChanged();")
    .WithSetterAccessModifier("private")
    .WithAttribute("JsonPropertyName(\"status\")");

// Expression body property
var computedProperty = new PropertyBuilder()
    .WithName("IsActive")
    .WithType("bool")
    .WithExpressionBody("Status == CustomerStatus.Active && LastLoginDate > DateTime.Now.AddDays(-30)");
```

### Roslyn-Based Parsing

#### `RoslynCSharpParser`
Production-ready C# parser using Microsoft.CodeAnalysis:

**Features:**
- **Full C# Support**: Latest C# language version support
- **Error Handling**: Comprehensive error detection and reporting
- **Performance**: Optimized for large codebases with async support
- **Documentation**: XML documentation parsing support
- **Cancellation**: Proper cancellation token support for long operations

**Example Usage:**
```csharp
var parser = new RoslynCSharpParser();
var result = await parser.ParseAsync(sourceCode, "Customer.cs");

if (result.IsSuccess)
{
    var syntaxTree = result.Value;
    Console.WriteLine($"Parsed file: {syntaxTree.FilePath}");
    Console.WriteLine($"Has errors: {syntaxTree.HasErrors}");
    
    // Find all class declarations
    var classes = syntaxTree.FindNodes("ClassDeclaration");
    foreach (var classNode in classes)
    {
        Console.WriteLine($"Class: {classNode.Text.Split()[2]}"); // Get class name
    }
}
```

#### `RoslynSyntaxTree`
Comprehensive syntax tree implementation:

**Features:**
- **Full Tree Navigation**: Parent, child, and descendant traversal
- **Position Lookup**: Line/column and character position node lookup
- **Error Analysis**: Detailed error node enumeration with diagnostics
- **Query Support**: Node type filtering and deep tree searching

#### `RoslynSyntaxNode`
Rich syntax node implementation:

**Features:**
- **Node Metadata**: Complete type, position, and content information
- **Tree Navigation**: Efficient parent/child relationships
- **Error Detection**: Syntax error identification and reporting
- **Search Capabilities**: Flexible node finding and filtering

### Code Generation

#### `CSharpCodeGenerator`
Extensible code generation engine:

**Features:**
- **Multi-Builder Support**: Generates complete compilation units
- **Builder Integration**: Direct integration with all builder types
- **Template Support**: Foundation for template-based generation
- **Extensible Design**: Easy addition of new generation patterns

**Example Usage:**
```csharp
var generator = new CSharpCodeGenerator();

var builders = new ICodeBuilder[]
{
    new ClassBuilder().WithName("Customer"),
    new InterfaceBuilder().WithName("ICustomerService"),
    new EnumBuilder().WithName("CustomerStatus")
};

string compilationUnit = generator.GenerateCompilationUnit(builders);
```

### Language Registry

#### `LanguageRegistry`
Extensible registry for language-specific parsers:

**Features:**
- **Built-in C# Support**: Automatic C# parser registration
- **Extension Mapping**: File extension to language resolution
- **Dynamic Registration**: Runtime parser registration capability
- **Thread Safety**: Concurrent access support

**Example Usage:**
```csharp
var registry = new LanguageRegistry();

// Built-in support
var parser = await registry.GetParserAsync("csharp");
var language = registry.GetLanguageByExtension(".cs"); // Returns "csharp"

// Register custom parser
registry.RegisterParser("typescript", new TypeScriptParser(), ".ts", ".tsx");
```

## Architecture and Design Patterns

### Builder Pattern Implementation
The concrete builders implement a sophisticated builder pattern:
- **Fluent Interface**: Natural method chaining for code construction
- **State Validation**: Automatic validation of incompatible settings
- **Lazy Generation**: Code generation only occurs on Build() call
- **Immutable Output**: Generated code is immutable once created

### Template Method Pattern
All builders follow a consistent generation workflow:
1. **Initialization**: Clear previous state and prepare for generation
2. **Documentation**: Generate XML documentation comments
3. **Attributes**: Apply attributes and annotations
4. **Signature**: Build the main construct signature
5. **Body**: Generate the construct body or members
6. **Finalization**: Complete the construct and return formatted code

### Factory Pattern
The language registry implements a factory pattern for parser creation:
- **Lazy Loading**: Parsers created on demand
- **Caching**: Parser instances cached for performance
- **Extension Points**: Easy registration of custom parsers

### Composite Pattern
Syntax tree implementation uses composite pattern:
- **Uniform Interface**: Consistent handling of simple and complex nodes
- **Recursive Operations**: Tree traversal and manipulation
- **Flexible Queries**: Deep searching with custom predicates

## Usage Examples

### Complete Class Generation
```csharp
var customerClass = new ClassBuilder()
    .WithNamespace("MyProject.Domain.Entities")
    .WithUsings("System", "System.ComponentModel.DataAnnotations", "System.Text.Json.Serialization")
    .WithName("Customer")
    .WithAccessModifier("public")
    .AsSealed()
    .WithBaseClass("EntityBase<Guid>")
    .WithInterfaces("IValidatableObject", "IAuditable")
    .WithXmlDoc("Represents a customer entity with full audit and validation support")
    
    // Primary key property
    .WithProperty(new PropertyBuilder()
        .WithName("Id")
        .WithType("Guid")
        .WithAccessModifier("public")
        .AsOverride()
        .AsReadOnly()
        .WithInitSetter()
        .WithXmlDoc("Gets the unique identifier for this customer"))
    
    // Business properties
    .WithProperty(new PropertyBuilder()
        .WithName("FirstName")
        .WithType("string")
        .WithAccessModifier("public")
        .WithAttribute("Required", "StringLength(50)")
        .WithAttribute("JsonPropertyName(\"firstName\")")
        .AsReadOnly()
        .WithInitSetter()
        .WithXmlDoc("Gets the customer's first name"))
    
    .WithProperty(new PropertyBuilder()
        .WithName("LastName")
        .WithType("string")
        .WithAccessModifier("public")
        .WithAttribute("Required", "StringLength(50)")
        .WithAttribute("JsonPropertyName(\"lastName\")")
        .AsReadOnly()
        .WithInitSetter()
        .WithXmlDoc("Gets the customer's last name"))
    
    // Computed property
    .WithProperty(new PropertyBuilder()
        .WithName("FullName")
        .WithType("string")
        .WithAccessModifier("public")
        .WithAttribute("JsonPropertyName(\"fullName\")")
        .WithExpressionBody("$\"{FirstName} {LastName}\"")
        .WithXmlDoc("Gets the customer's full name"))
    
    // Constructor
    .WithConstructor(new ConstructorBuilder()
        .WithClassName("Customer")
        .WithAccessModifier("public")
        .WithParameter("string", "firstName")
        .WithParameter("string", "lastName")
        .WithParameter("string?", "email", "null")
        .WithXmlDoc("Initializes a new customer instance")
        .WithParamDoc("firstName", "The customer's first name")
        .WithParamDoc("lastName", "The customer's last name")
        .WithParamDoc("email", "Optional email address")
        .AddBodyLine("ArgumentException.ThrowIfNullOrWhiteSpace(firstName);")
        .AddBodyLine("ArgumentException.ThrowIfNullOrWhiteSpace(lastName);")
        .AddBodyLine("FirstName = firstName;")
        .AddBodyLine("LastName = lastName;")
        .AddBodyLine("Email = email;"))
    
    // Validation method
    .WithMethod(new MethodBuilder()
        .WithName("Validate")
        .WithReturnType("IEnumerable<ValidationResult>")
        .WithParameter("ValidationContext", "validationContext")
        .WithAccessModifier("public")
        .WithXmlDoc("Validates the customer entity")
        .WithParamDoc("validationContext", "The validation context")
        .WithReturnDoc("Collection of validation results")
        .AddBodyLine("var results = new List<ValidationResult>();")
        .AddBodyLine("if (string.IsNullOrWhiteSpace(FirstName))")
        .AddBodyLine("    results.Add(new ValidationResult(\"First name is required\", new[] { nameof(FirstName) }));")
        .AddBodyLine("if (string.IsNullOrWhiteSpace(LastName))")
        .AddBodyLine("    results.Add(new ValidationResult(\"Last name is required\", new[] { nameof(LastName) }));")
        .AddBodyLine("return results;"));

string generatedCode = customerClass.Build();
```

### Code Analysis Workflow
```csharp
var registry = new LanguageRegistry();
var parser = await registry.GetParserAsync("csharp");

// Parse existing code
var parseResult = await parser.ParseAsync(existingCode, "Customer.cs");
if (parseResult.IsSuccess)
{
    var tree = parseResult.Value;
    
    // Find all properties
    var properties = tree.FindNodes("PropertyDeclaration");
    
    // Generate corresponding interface
    var interfaceBuilder = new InterfaceBuilder()
        .WithNamespace("MyProject.Contracts")
        .WithName("ICustomer")
        .WithAccessModifier("public")
        .WithXmlDoc("Interface for customer operations");
    
    foreach (var property in properties)
    {
        // Extract property information and add to interface
        var propertyName = ExtractPropertyName(property);
        var propertyType = ExtractPropertyType(property);
        
        interfaceBuilder.WithProperty(new PropertyBuilder()
            .WithName(propertyName)
            .WithType(propertyType)
            .AsReadOnly());
    }
    
    string interfaceCode = interfaceBuilder.Build();
}
```

### Batch Code Generation
```csharp
var entities = new[] { "Customer", "Order", "Product", "Category" };
var generator = new CSharpCodeGenerator();
var builders = new List<ICodeBuilder>();

foreach (var entity in entities)
{
    // Generate entity class
    builders.Add(new ClassBuilder()
        .WithNamespace($"MyProject.Domain.{entity}s")
        .WithName(entity)
        .WithAccessModifier("public")
        .AsSealed()
        .WithBaseClass("EntityBase"));
    
    // Generate repository interface
    builders.Add(new InterfaceBuilder()
        .WithNamespace($"MyProject.Domain.{entity}s")
        .WithName($"I{entity}Repository")
        .WithAccessModifier("public")
        .WithBaseInterfaces("IRepository<{entity}, Guid>"));
    
    // Generate service interface
    builders.Add(new InterfaceBuilder()
        .WithNamespace($"MyProject.Application.{entity}s")
        .WithName($"I{entity}Service")
        .WithAccessModifier("public"));
}

string compilationUnit = generator.GenerateCompilationUnit(builders);
```

## Dependencies

### Core Dependencies
- **FractalDataWorks.CodeBuilder.Abstractions**: Interface definitions
- **FractalDataWorks.net**: Framework result patterns and base types
- **Microsoft.CodeAnalysis.CSharp**: Roslyn C# parser and syntax analysis
- **.NET 8.0**: Modern C# features and performance improvements

### Optional Dependencies
For advanced scenarios, consider integrating with:
- **Microsoft.CodeAnalysis.Workspaces**: For solution-level analysis
- **Microsoft.Extensions.DependencyInjection**: For IoC container integration
- **Microsoft.Extensions.Logging**: For detailed generation logging

## Integration Points

### With Build Systems
```xml
<!-- MSBuild integration for code generation -->
<ItemGroup>
  <PackageReference Include="FractalDataWorks.CodeBuilder" Version="1.0.0" />
</ItemGroup>

<Target Name="GenerateCode" BeforeTargets="BeforeBuild">
  <Exec Command="dotnet run --project $(MSBuildProjectDirectory)\CodeGen -- $(OutputPath)" />
</Target>
```

### With Source Generators
```csharp
[Generator]
public class EntityGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var classBuilder = new ClassBuilder()
            .WithNamespace("Generated")
            .WithName("GeneratedEntity");
            
        context.AddSource("GeneratedEntity.g.cs", classBuilder.Build());
    }
}
```

### With Dependency Injection
```csharp
services.AddSingleton<ILanguageRegistry, LanguageRegistry>();
services.AddTransient<ICodeGenerator, CSharpCodeGenerator>();
services.AddTransient<IClassBuilder, ClassBuilder>();
services.AddTransient<IMethodBuilder, MethodBuilder>();
```

## Performance Considerations

### Memory Management
- StringBuilder reuse across generation operations
- Lazy evaluation of syntax tree analysis
- Proper disposal of Roslyn syntax trees

### Concurrent Generation
- Thread-safe builder implementations
- Parallel code generation for multiple files
- Async parsing for large codebases

### Caching Strategies
- Parser instance caching in language registry
- Template compilation caching for repeated generation
- Syntax tree caching for analysis operations

## Extension Points

### Custom Builders
```csharp
public class EntityBuilder : ClassBuilder
{
    public EntityBuilder WithTableName(string tableName)
    {
        return WithAttribute($"Table(\"{tableName}\")");
    }
    
    public EntityBuilder WithPrimaryKey(string propertyName, Type keyType)
    {
        return WithProperty(new PropertyBuilder()
            .WithName(propertyName)
            .WithType(keyType.Name)
            .WithAttribute("Key")
            .AsReadOnly()
            .WithInitSetter());
    }
}
```

### Language Extensions
```csharp
public class TypeScriptParser : ICodeParser
{
    public string Language => "typescript";
    
    public async Task<IFdwResult<ISyntaxTree>> ParseAsync(
        string sourceCode, 
        string? filePath = null, 
        CancellationToken cancellationToken = default)
    {
        // TypeScript parsing implementation
        // Could use Tree-sitter or other parsing technology
        return await ParseTypeScriptAsync(sourceCode, filePath, cancellationToken);
    }
}
```

### Code Analysis Extensions
```csharp
public static class SyntaxTreeExtensions
{
    public static IEnumerable<ISyntaxNode> FindMethodsWithAttribute(
        this ISyntaxTree tree, 
        string attributeName)
    {
        return tree.FindNodes("MethodDeclaration")
            .Where(method => method.FindChildren("AttributeList")
                .Any(attr => attr.Text.Contains(attributeName)));
    }
}
```

This implementation provides a complete, production-ready code generation framework with extensibility points for future enhancements and additional language support.