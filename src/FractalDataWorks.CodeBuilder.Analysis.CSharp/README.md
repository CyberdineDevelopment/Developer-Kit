# FractalDataWorks.CodeBuilder.Analysis.CSharp

C# implementation of the CodeBuilder analysis framework using Roslyn. Provides compilation verification and syntax expectations for testing generated C# code.

## Overview

This package implements the language-agnostic analysis interfaces defined in `FractalDataWorks.CodeBuilder.Analysis` using Microsoft's Roslyn compiler platform. It enables comprehensive testing of generated C# code including compilation verification, syntax structure validation, and runtime method execution.

## Installation

```bash
dotnet add package FractalDataWorks.CodeBuilder.Analysis.CSharp
```

## Core Components

### CSharpCompilationVerifier

Implements `ICompilationVerifier` to verify that generated C# code compiles correctly:

```csharp
using FractalDataWorks.CodeBuilder.Testing.CSharp;

var verifier = new CSharpCompilationVerifier();

// Basic compilation verification
var result = verifier.CompileAndVerify(@"
    public class TestClass
    {
        public string Name { get; set; }
        public int Calculate() => 42;
    }");

Assert.True(result.Success);
Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
```

### CSharpSyntaxExpectations

Implements `ISyntaxExpectations` to provide fluent syntax verification:

```csharp
using FractalDataWorks.CodeBuilder.Testing.CSharp;

var syntaxExpectations = new CSharpSyntaxExpectations();

syntaxExpectations
    .ExpectCode(generatedCode)
    .HasClass("TestClass", c => c
        .IsPublic()
        .HasProperty("Name", p => p
            .HasType("string")
            .HasGetter()
            .HasSetter())
        .HasMethod("Calculate", m => m
            .IsPublic()
            .HasReturnType("int")))
    .Compiles();
```

## Features

### Compilation Verification

- **Full Compilation**: Compiles source code to assemblies using Roslyn
- **Custom Options**: Support for different output kinds, optimization levels, and references
- **Diagnostic Reporting**: Detailed error and warning information with file locations
- **Method Execution**: Invoke methods from compiled assemblies for integration testing

### Syntax Expectations

- **Structure Validation**: Verify namespaces, classes, interfaces, enums, records
- **Member Validation**: Assert methods, properties, fields, constructors
- **Modifier Validation**: Check access modifiers, static, abstract, sealed, etc.
- **Type Validation**: Verify parameter types, return types, property types
- **Fluent API**: Chainable expectations for readable test code

## Usage Examples

### Basic Compilation Testing

```csharp
[Test]
public void GeneratedCode_ShouldCompileSuccessfully()
{
    var generatedCode = @"
        namespace MyApp
        {
            public class Person
            {
                public string Name { get; set; }
                public int Age { get; set; }
                
                public string GetDisplayName()
                {
                    return $""{Name} ({Age})"";
                }
            }
        }";

    var verifier = new CSharpCompilationVerifier();
    var result = verifier.CompileAndVerify(generatedCode);
    
    Assert.True(result.Success);
    Assert.NotNull(result.AssemblyBytes);
}
```

### Advanced Compilation with Options

```csharp
[Test]
public void GeneratedCode_WithCustomOptions_ShouldCompile()
{
    var options = new AnalysisOptions
    {
        OutputKind = OutputKind.DynamicallyLinkedLibrary,
        OptimizationLevel = OptimizationLevel.Release,
        AllowUnsafe = false,
        References = new[]
        {
            typeof(System.Collections.Generic.IEnumerable<>).Assembly.Location,
            typeof(System.ComponentModel.INotifyPropertyChanged).Assembly.Location
        }
    };

    var result = verifier.CompileWithOptions(new[] { generatedCode }, options);
    Assert.True(result.Success);
}
```

### Method Execution Testing

```csharp
[Test]
public void CompiledCode_ShouldExecuteCorrectly()
{
    var sourceCode = @"
        public class Calculator
        {
            public static int Add(int a, int b)
            {
                return a + b;
            }
            
            public int Multiply(int a, int b)
            {
                return a * b;
            }
        }";

    var verifier = new CSharpCompilationVerifier();
    var result = verifier.CompileAndVerify(sourceCode);
    
    // Test static method
    var sum = result.InvokeMethod("Calculator", "Add", 5, 3);
    Assert.Equal(8, sum);
    
    // Test instance method
    var product = result.InvokeMethod("Calculator", "Multiply", 4, 6);
    Assert.Equal(24, product);
}
```

### Comprehensive Syntax Validation

```csharp
[Test]
public void GeneratedClass_ShouldHaveExpectedStructure()
{
    var generatedCode = @"
        namespace MyApp.Models
        {
            public abstract class BaseEntity
            {
                public int Id { get; set; }
                public DateTime CreatedAt { get; init; }
                
                protected BaseEntity(int id)
                {
                    Id = id;
                    CreatedAt = DateTime.UtcNow;
                }
                
                public abstract void Validate();
            }
            
            public class User : BaseEntity
            {
                public string Name { get; set; }
                public string Email { get; set; }
                
                public User(int id, string name, string email) : base(id)
                {
                    Name = name;
                    Email = email;
                }
                
                public override void Validate()
                {
                    if (string.IsNullOrEmpty(Name))
                        throw new ArgumentException(""Name is required"");
                    if (string.IsNullOrEmpty(Email))
                        throw new ArgumentException(""Email is required"");
                }
                
                public static User Create(string name, string email)
                {
                    return new User(0, name, email);
                }
            }
        }";

    var syntaxExpectations = new CSharpSyntaxExpectations();
    syntaxExpectations
        .ExpectCode(generatedCode)
        .HasNamespace("MyApp.Models", ns => ns
            .HasClass("BaseEntity", c => c
                .IsPublic()
                .IsAbstract()
                .HasProperty("Id", p => p
                    .HasType("int")
                    .HasGetter()
                    .HasSetter())
                .HasProperty("CreatedAt", p => p
                    .HasType("DateTime")
                    .HasGetter()
                    .HasInitSetter())
                .HasConstructor(ctor => ctor
                    .HasParameter("id", "int"))
                .HasMethod("Validate", m => m
                    .IsPublic()
                    .IsAbstract()))
            .HasClass("User", c => c
                .IsPublic()
                .InheritsFrom("BaseEntity")
                .HasProperty("Name", p => p.HasType("string"))
                .HasProperty("Email", p => p.HasType("string"))
                .HasConstructor(ctor => ctor
                    .HasParameter("id", "int")
                    .HasParameter("name", "string")
                    .HasParameter("email", "string"))
                .HasMethod("Validate", m => m
                    .IsPublic()
                    .IsOverride())
                .HasMethod("Create", m => m
                    .IsPublic()
                    .IsStatic()
                    .HasReturnType("User")
                    .HasParameter("name", "string")
                    .HasParameter("email", "string"))))
        .Compiles();
}
```

### Interface Validation

```csharp
[Test]
public void GeneratedInterface_ShouldHaveCorrectMembers()
{
    var interfaceCode = @"
        public interface IRepository<T> where T : class
        {
            Task<T?> GetByIdAsync(int id);
            Task<IEnumerable<T>> GetAllAsync();
            Task<T> AddAsync(T entity);
            Task UpdateAsync(T entity);
            Task DeleteAsync(int id);
            
            IQueryable<T> Query { get; }
            int Count { get; }
        }";

    var syntaxExpectations = new CSharpSyntaxExpectations();
    syntaxExpectations
        .ExpectCode(interfaceCode)
        .HasInterface("IRepository", i => i
            .HasMethod("GetByIdAsync", m => m
                .HasReturnType("Task<T?>")
                .HasParameter("id", "int"))
            .HasMethod("GetAllAsync", m => m
                .HasReturnType("Task<IEnumerable<T>>"))
            .HasMethod("AddAsync", m => m
                .HasReturnType("Task<T>")
                .HasParameter("entity", "T"))
            .HasProperty("Query", p => p
                .HasType("IQueryable<T>"))
            .HasProperty("Count", p => p
                .HasType("int")))
        .Compiles();
}
```

### Record and Enum Validation

```csharp
[Test]
public void GeneratedRecordAndEnum_ShouldHaveCorrectStructure()
{
    var code = @"
        public enum Status
        {
            Pending = 1,
            Active = 2,
            Inactive = 3
        }
        
        public record PersonDto(int Id, string Name, Status Status)
        {
            public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
            
            public string GetStatusDisplay() => Status switch
            {
                Status.Pending => ""Pending Approval"",
                Status.Active => ""Active"",
                Status.Inactive => ""Inactive"",
                _ => ""Unknown""
            };
        }";

    var syntaxExpectations = new CSharpSyntaxExpectations();
    syntaxExpectations
        .ExpectCode(code)
        .HasEnum("Status", e => e
            .HasValue("Pending", 1)
            .HasValue("Active", 2)
            .HasValue("Inactive", 3))
        .HasRecord("PersonDto", r => r
            .HasParameter("Id", "int")
            .HasParameter("Name", "string")
            .HasParameter("Status", "Status")
            .HasProperty("CreatedAt", p => p
                .HasType("DateTime")
                .HasInitSetter())
            .HasProperty("GetStatusDisplay", p => p
                .HasType("string")))
        .Compiles();
}
```

### Error Handling and Diagnostics

```csharp
[Test]
public void InvalidCode_ShouldProvideDetailedDiagnostics()
{
    var invalidCode = @"
        public class InvalidClass
        {
            public void InvalidMethod()
            {
                // This will cause a compilation error
                UnknownType variable = new UnknownType();
            }
        }";

    var verifier = new CSharpCompilationVerifier();
    var result = verifier.CompileAndVerify(invalidCode);
    
    Assert.False(result.Success);
    
    var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
    Assert.NotEmpty(errors);
    
    var error = errors.First();
    Assert.Contains("UnknownType", error.Message);
    Assert.NotNull(error.Line);
    Assert.NotNull(error.Column);
}
```

## Testing Source Generators

This package is particularly useful for testing Roslyn source generators:

```csharp
[Test]
public void SourceGenerator_ShouldGenerateExpectedCode()
{
    // Arrange
    var inputCode = @"
        [AutoProperty]
        public partial class Person
        {
            private string _name;
            private int _age;
        }";

    // Act
    var generatedCode = RunSourceGenerator<AutoPropertyGenerator>(inputCode);
    
    // Assert
    var verifier = new CSharpCompilationVerifier();
    var syntaxExpectations = new CSharpSyntaxExpectations();
    
    // Verify it compiles
    var result = verifier.CompileAndVerify(inputCode, generatedCode);
    Assert.True(result.Success);
    
    // Verify structure
    syntaxExpectations
        .ExpectCode(generatedCode)
        .HasClass("Person", c => c
            .IsPublic()
            .IsPartial()
            .HasProperty("Name", p => p
                .HasType("string")
                .HasGetter()
                .HasSetter())
            .HasProperty("Age", p => p
                .HasType("int")
                .HasGetter()
                .HasSetter()));
}
```

## Best Practices

1. **Always Verify Compilation**: Use `CompileAndVerify` to ensure generated code is syntactically correct
2. **Test Structure and Behavior**: Use syntax expectations for structure and method invocation for behavior
3. **Handle Diagnostics**: Check compilation diagnostics for warnings and errors
4. **Use Appropriate References**: Include necessary assembly references for complex scenarios
5. **Test Edge Cases**: Verify error handling and boundary conditions
6. **Isolated Tests**: Keep tests independent and focused on specific scenarios

## Integration with Testing Frameworks

This package works well with popular .NET testing frameworks:

### xUnit
```csharp
public class CodeGenerationTests
{
    private readonly CSharpCompilationVerifier _verifier = new();
    private readonly CSharpSyntaxExpectations _syntaxExpectations = new();
    
    [Fact]
    public void ShouldGenerateValidCode() { /* test code */ }
}
```

### NUnit
```csharp
[TestFixture]
public class CodeGenerationTests
{
    private CSharpCompilationVerifier _verifier;
    private CSharpSyntaxExpectations _syntaxExpectations;
    
    [SetUp]
    public void Setup()
    {
        _verifier = new CSharpCompilationVerifier();
        _syntaxExpectations = new CSharpSyntaxExpectations();
    }
    
    [Test]
    public void ShouldGenerateValidCode() { /* test code */ }
}
```

### MSTest
```csharp
[TestClass]
public class CodeGenerationTests
{
    private CSharpCompilationVerifier _verifier;
    private CSharpSyntaxExpectations _syntaxExpectations;
    
    [TestInitialize]
    public void Initialize()
    {
        _verifier = new CSharpCompilationVerifier();
        _syntaxExpectations = new CSharpSyntaxExpectations();
    }
    
    [TestMethod]
    public void ShouldGenerateValidCode() { /* test code */ }
}
```

## Dependencies

- **Microsoft.CodeAnalysis.CSharp**: For Roslyn compiler services
- **FractalDataWorks.CodeBuilder.Analysis**: For language-agnostic interfaces
- **.NET Standard 2.0**: Compatible with .NET Framework 4.6.1+ and .NET Core 2.0+

## License

Apache License 2.0 - see the [LICENSE](../../LICENSE) file for details.