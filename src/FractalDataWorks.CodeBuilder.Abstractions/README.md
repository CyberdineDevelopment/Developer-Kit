# FractalDataWorks.CodeBuilder.Abstractions

The **FractalDataWorks.CodeBuilder.Abstractions** project defines the core interfaces and contracts for the FractalDataWorks code generation framework. This project provides the foundational abstractions that enable flexible, extensible code generation capabilities across multiple programming languages.

## Overview and Purpose

This package contains the interface definitions that form the contract layer for:
- **Code Building**: Fluent builder interfaces for generating source code constructs
- **Code Parsing**: Language-agnostic parsing capabilities for analyzing existing source code
- **Code Generation**: Template-based code generation with pluggable language support
- **Syntax Tree Representation**: Abstract syntax tree interfaces for code analysis

The abstractions are designed to be language-agnostic, allowing implementations for C#, Java, TypeScript, Python, and other programming languages while maintaining a consistent API surface.

## Key Interfaces and Their Responsibilities

### Core Builder Interfaces

#### `ICodeBuilder`
The base interface for all code builders, providing fundamental generation capabilities:
- **`Build()`**: Generates and returns the final code as a string
- **`IndentLevel`**: Tracks current indentation for proper code formatting
- **`IndentString`**: Configurable indentation style (spaces, tabs, etc.)

#### `IClassBuilder`
Fluent builder for generating class definitions with comprehensive support for:
- Namespace and using directives
- Access modifiers, inheritance, and generic type parameters
- Class modifiers (static, abstract, sealed, partial)
- Members (fields, properties, methods, constructors, nested classes)
- Attributes and XML documentation

#### `IInterfaceBuilder` 
Specialized builder for interface definitions supporting:
- Interface inheritance and generic constraints
- Method and property signatures
- Event declarations
- Access modifiers and attributes

#### `IMethodBuilder`
Comprehensive method generation with support for:
- Method signatures with parameters and return types
- Method modifiers (static, virtual, override, abstract, async)
- Generic type parameters and constraints
- Method body or expression body syntax
- XML documentation for parameters and return values

#### `IPropertyBuilder`
Property generation supporting modern C# features:
- Auto-properties and custom getter/setter implementations
- Access modifiers for individual accessors
- Property modifiers (static, virtual, override, abstract)
- Init-only setters and property initializers
- Expression body properties

#### `IConstructorBuilder`
Constructor generation with:
- Parameter definitions with default values
- Base and this constructor chaining
- Static constructor support
- XML documentation

#### `IFieldBuilder`
Field definition builder supporting:
- Field modifiers (static, readonly, const, volatile)
- Initializer expressions
- Access modifiers and attributes

#### `IEnumBuilder`
Enumeration builder with:
- Underlying type specification
- Enum member definitions with explicit values
- Member-specific attributes and documentation

### Code Analysis Interfaces

#### `ICodeParser`
Language-specific parser interface for converting source code into analyzable structures:
- **`Language`**: Identifies the target programming language
- **`ParseAsync()`**: Asynchronously parses source code into a syntax tree
- **`ValidateAsync()`**: Validates source code without full parsing

#### `ISyntaxTree`
Represents a parsed syntax tree with navigation and analysis capabilities:
- **`Root`**: Access to the root syntax node
- **`HasErrors`**: Quick error detection
- **`FindNodes()`**: Query nodes by type
- **`GetNodeAtPosition()`**: Position-based node lookup
- **`GetErrors()`**: Enumeration of error nodes

#### `ISyntaxNode`
Individual nodes within the syntax tree providing:
- **Node Metadata**: Type, text content, position information
- **Tree Navigation**: Parent, children, and descendant traversal
- **Error Detection**: Identification of syntax errors
- **Search Capabilities**: Finding child nodes by type

### Language Registry Interface

#### `ILanguageRegistry`
Central registry for managing language-specific parsers:
- **Language Discovery**: Supported languages and file extensions
- **Parser Management**: Registration and retrieval of language parsers
- **Extension Mapping**: File extension to language association

### Code Generation Interface

#### `ICodeGenerator`
High-level code generation interface supporting:
- **Multi-Builder Generation**: Creating complete compilation units
- **Language Targeting**: Specific output language generation
- **Builder Integration**: Direct integration with all builder types

## Architecture and Design Patterns

### Builder Pattern
The framework extensively uses the Builder pattern to provide:
- **Fluent Interface**: Method chaining for intuitive code construction
- **Progressive Disclosure**: Complex configurations built incrementally
- **Immutable Results**: Builders generate immutable code strings

### Strategy Pattern
Language-specific implementations use the Strategy pattern:
- **Pluggable Parsers**: Different parsing strategies per language
- **Configurable Generation**: Language-specific code generation rules
- **Extension Points**: Easy addition of new language support

### Template Method Pattern
Code builders implement consistent generation workflows:
- **Standardized Structure**: Common generation phases across all builders
- **Customizable Details**: Language-specific formatting and syntax rules
- **Consistent Output**: Predictable code structure and formatting

### Composite Pattern
Syntax tree representation uses the Composite pattern:
- **Uniform Interface**: Consistent node handling regardless of complexity
- **Tree Traversal**: Recursive operations on nested structures
- **Flexible Queries**: Deep search and filtering capabilities

## Usage Examples

### Basic Class Generation
```csharp
var classBuilder = new ClassBuilder()
    .WithNamespace("MyProject.Models")
    .WithUsings("System", "System.Collections.Generic")
    .WithName("Customer")
    .WithAccessModifier("public")
    .WithXmlDoc("Represents a customer entity")
    .WithProperty(new PropertyBuilder()
        .WithName("Id")
        .WithType("int")
        .AsReadOnly()
        .WithXmlDoc("Gets the customer identifier"));

string generatedCode = classBuilder.Build();
```

### Method with Generic Constraints
```csharp
var methodBuilder = new MethodBuilder()
    .WithName("ProcessItems")
    .WithReturnType("Task<IEnumerable<TResult>>")
    .WithGenericParameters("TItem", "TResult")
    .WithGenericConstraint("TItem", "class", "IComparable<TItem>")
    .WithGenericConstraint("TResult", "new()")
    .WithParameter("IEnumerable<TItem>", "items")
    .AsAsync()
    .WithXmlDoc("Processes a collection of items asynchronously")
    .WithParamDoc("items", "The items to process")
    .WithReturnDoc("The processed results");
```

### Syntax Tree Analysis
```csharp
var parser = await languageRegistry.GetParserAsync("csharp");
var parseResult = await parser.ParseAsync(sourceCode, "MyFile.cs");

if (parseResult.IsSuccess)
{
    var syntaxTree = parseResult.Value;
    var classNodes = syntaxTree.FindNodes("ClassDeclaration");
    
    foreach (var classNode in classNodes)
    {
        Console.WriteLine($"Found class: {classNode.Text}");
        var methods = classNode.FindChildren("MethodDeclaration");
        Console.WriteLine($"  Methods: {methods.Count()}");
    }
}
```

## Dependencies

### Core Dependencies
- **FractalDataWorks.net**: Provides `IFdwResult` for consistent error handling
- **.NET 8.0**: Modern C# language features and performance improvements

### Framework Integration
This abstractions package integrates seamlessly with:
- **Dependency Injection**: All interfaces designed for IoC container registration
- **Async/Await**: Full async support for parsing and generation operations
- **Cancellation**: CancellationToken support for long-running operations

## Integration Points

### With FractalDataWorks Framework
- **Service Integration**: Builders can be registered as framework services
- **Configuration Support**: Integration with framework configuration patterns
- **Result Patterns**: Consistent with framework result and error handling

### With Code Analysis Tools
- **Roslyn Integration**: Direct support for Microsoft.CodeAnalysis
- **Tree-Sitter Support**: Extensible for multiple parser backends
- **Language Server Protocol**: Foundation for LSP implementations

### With Build Systems
- **Source Generators**: Can be used in Roslyn source generators
- **Build-Time Generation**: Integration with MSBuild and build pipelines
- **Template Processing**: T4 template and Razor template integration

## Extension Points

### Custom Language Support
Implement `ICodeParser` for new programming languages:
```csharp
public class PythonParser : ICodeParser
{
    public string Language => "python";
    
    public async Task<IFdwResult<ISyntaxTree>> ParseAsync(
        string sourceCode, 
        string? filePath = null, 
        CancellationToken cancellationToken = default)
    {
        // Python-specific parsing implementation
    }
}
```

### Custom Builders
Create specialized builders for domain-specific code generation:
```csharp
public interface IEntityBuilder : ICodeBuilder
{
    IEntityBuilder WithTableName(string tableName);
    IEntityBuilder WithPrimaryKey(string keyField);
    IEntityBuilder WithIndex(params string[] fields);
}
```

### Code Analysis Extensions
Extend syntax analysis with custom node visitors:
```csharp
public class SecurityAnalyzer
{
    public async Task<SecurityReport> AnalyzeAsync(ISyntaxTree syntaxTree)
    {
        var methodCalls = syntaxTree.FindNodes("InvocationExpression");
        // Analyze for security vulnerabilities
    }
}
```

## Design Principles

### Language Agnostic Design
All interfaces are designed to work across programming languages while maintaining language-specific flexibility in implementations.

### Composability
Builders can be composed and nested to create complex code structures while maintaining clean separation of concerns.

### Extensibility
The interface design allows for easy extension without breaking existing implementations, supporting future language features and generation patterns.

### Performance Awareness
Async patterns and lazy evaluation support efficient handling of large codebases and complex generation tasks.

### Type Safety
Strong typing throughout the interface hierarchy prevents common code generation errors at compile time.