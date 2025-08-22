# FractalDataWorks.EnhancedEnums.Analyzers

[![NuGet](https://img.shields.io/nuget/v/FractalDataWorks.EnhancedEnums.Analyzers.svg)](https://www.nuget.org/packages/FractalDataWorks.EnhancedEnums.Analyzers/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Roslyn analyzers for Enhanced Enums that provide compile-time validation and code quality checks for enhanced enum types and collections.

## Table of Contents

- [Project Overview](#project-overview)
- [Analyzer Rules](#analyzer-rules)
  - [Shipped Rules](#shipped-rules)
  - [Unshipped Rules](#unshipped-rules)
- [Usage Instructions](#usage-instructions)
- [Integration](#integration)
- [Rule Details](#rule-details)
- [Development](#development)
- [Testing](#testing)

## Project Overview

The FractalDataWorks.EnhancedEnums.Analyzers project provides compile-time code analysis for Enhanced Enums to ensure:

- **Proper Configuration**: Validates that Enhanced Enum attributes are correctly configured
- **Inheritance Requirements**: Ensures classes follow the required inheritance patterns
- **Lookup Consistency**: Detects duplicate lookup values and ensures proper AllowMultiple configuration
- **Type Safety**: Validates generic constraints and interface requirements
- **Best Practices**: Enforces recommended patterns for Enhanced Enum implementations

### Why Analyzers Matter

Enhanced Enums rely on specific patterns and attributes to generate code correctly. These analyzers catch common configuration errors at compile time, preventing runtime issues and ensuring the generated code works as expected.

## Analyzer Rules

### Shipped Rules

| Rule ID | Category | Severity | Description |
|---------|----------|----------|-------------|
| [ENH1001](#enh1001-enhanced-enum-base-should-implement-ienuroption) | Usage | Warning | EnhancedEnumBaseAnalyzer - Enhanced enum base class should implement IEnumOption |
| [ENH1002](#enh1002-enhanced-enum-constructor-issues) | Design | Warning | EnhancedEnumConstructorAnalyzer - Enhanced enum constructor validation |

### Unshipped Rules

| Rule ID | Category | Severity | Description |
|---------|----------|----------|-------------|
| [ENH004](#enh004-duplicate-enhanced-enum-option-id) | Usage | Error | DuplicateEnumOptionAnalyzer - Duplicate enhanced enum option ID detected |
| [ENH005](#enh005-enhanced-enum-option-constructor-issues) | Usage | Error | EnumOptionConstructorAnalyzer - Enhanced enum option constructor issues |
| [ENH006](#enh006-abstract-property-in-enhanced-enum) | Design | Warning | AbstractMemberAnalyzer - Abstract property in enhanced enum |
| [ENH007](#enh007-abstract-field-in-enhanced-enum) | Design | Error | AbstractMemberAnalyzer - Abstract field in enhanced enum |
| [ENH008](#enh008-enumcollection-attribute-must-specify-collectionname) | Usage | Error | EnumCollectionAttributeAnalyzer - EnumCollection attribute must specify CollectionName |
| [ENH009](#enh009-enumcollection-classes-must-inherit-from-enumoptionbase) | Usage | Error | EnumCollectionAttributeAnalyzer - EnumCollection classes must inherit from EnumOptionBase&lt;T&gt; |
| [ENH010](#enh010-generic-enumcollection-must-specify-interface-constraint) | Usage | Error | EnumCollectionAttributeAnalyzer - Generic EnumCollection must specify a non-generic interface constraint for T |
| [ENHENUM001](#enhenum001-duplicate-lookup-values-detected) | EnhancedEnums | Warning | DuplicateLookupValueAnalyzer - Duplicate lookup values detected without AllowMultiple |

## Usage Instructions

### Installation

Install the analyzer package via NuGet:

```bash
# .NET CLI
dotnet add package FractalDataWorks.EnhancedEnums.Analyzers

# Package Manager Console
Install-Package FractalDataWorks.EnhancedEnums.Analyzers
```

### Package Reference

```xml
<PackageReference Include="FractalDataWorks.EnhancedEnums.Analyzers" Version="*.*.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

### Configuration

The analyzers run automatically when the package is installed. You can configure specific rules in your `.editorconfig` file:

```ini
# Disable specific rules
dotnet_diagnostic.ENH1001.severity = none
dotnet_diagnostic.ENHENUM001.severity = error

# Configure category severity
dotnet_analyzer_diagnostic.category-enhancedenums.severity = warning
```

## Integration

### With Enhanced Enums Packages

The analyzers are designed to work seamlessly with the Enhanced Enums ecosystem:

```xml
<!-- Core Enhanced Enums functionality -->
<PackageReference Include="FractalDataWorks.EnhancedEnums" Version="*.*.*" />

<!-- Source generators for automatic collection generation -->
<PackageReference Include="FractalDataWorks.EnhancedEnums.SourceGenerators" Version="*.*.*" />

<!-- Analyzers for compile-time validation -->
<PackageReference Include="FractalDataWorks.EnhancedEnums.Analyzers" Version="*.*.*" />

<!-- Code fixes for automatic issue resolution -->
<PackageReference Include="FractalDataWorks.EnhancedEnums.CodeFixes" Version="*.*.*" />
```

### Build Integration

The analyzers integrate with MSBuild and provide diagnostics during compilation:

```bash
# Build with analyzer output
dotnet build

# Treat analyzer warnings as errors
dotnet build --warnaserror
```

## Rule Details

### ENH1001: Enhanced enum base should implement IEnumOption

**Category:** Usage | **Severity:** Warning

Enhanced enum base classes should implement `IEnumOption` to enable features like `GetById` generation and proper interface-based return types.

**Problem:**
```csharp
[EnumCollection(CollectionName = "Statuses")]
public abstract class StatusBase : EnumOptionBase<StatusBase>
{
    // Missing IEnumOption implementation
}
```

**Solution:**
```csharp
[EnumCollection(CollectionName = "Statuses")]
public abstract class StatusBase : EnumOptionBase<StatusBase>, IEnumOption
{
    // Now implements IEnumOption
}
```

### ENH1002: Enhanced enum constructor issues

**Category:** Design | **Severity:** Warning

Validates that enhanced enum constructors follow required patterns.

### ENH004: Duplicate enhanced enum option ID

**Category:** Usage | **Severity:** Error

Detects when multiple enum options use the same ID value.

**Problem:**
```csharp
[EnumOption]
public class Active : StatusBase
{
    public Active() : base(1, "Active") { } // ID = 1
}

[EnumOption]  
public class Pending : StatusBase
{
    public Pending() : base(1, "Pending") { } // ID = 1 (duplicate!)
}
```

**Solution:**
```csharp
[EnumOption]
public class Active : StatusBase
{
    public Active() : base(1, "Active") { }
}

[EnumOption]
public class Pending : StatusBase
{
    public Pending() : base(2, "Pending") { } // Unique ID
}
```

### ENH005: Enhanced enum option constructor issues

**Category:** Usage | **Severity:** Error

Validates that enum option constructors are properly configured.

### ENH006: Abstract property in enhanced enum

**Category:** Design | **Severity:** Warning

Warns about abstract properties in enhanced enum classes.

### ENH007: Abstract field in enhanced enum

**Category:** Design | **Severity:** Error

Flags abstract fields in enhanced enum classes as errors.

### ENH008: EnumCollection attribute must specify CollectionName

**Category:** Usage | **Severity:** Error

Ensures that the `[EnumCollection]` attribute explicitly specifies the `CollectionName` parameter.

**Problem:**
```csharp
[EnumCollection] // Missing CollectionName
public abstract class StatusBase : EnumOptionBase<StatusBase>
{
}
```

**Solution:**
```csharp
[EnumCollection(CollectionName = "Statuses")]
public abstract class StatusBase : EnumOptionBase<StatusBase>
{
}
```

### ENH009: EnumCollection classes must inherit from EnumOptionBase

**Category:** Usage | **Severity:** Error

Validates that classes marked with `[EnumCollection]` inherit from `EnumOptionBase<T>` or `EnumCollectionBase<T>`.

**Problem:**
```csharp
[EnumCollection(CollectionName = "Statuses")]
public abstract class StatusBase // Missing inheritance
{
}
```

**Solution:**
```csharp
[EnumCollection(CollectionName = "Statuses")]
public abstract class StatusBase : EnumOptionBase<StatusBase>
{
}
```

### ENH010: Generic EnumCollection must specify interface constraint

**Category:** Usage | **Severity:** Error

Generic EnumCollection classes require a non-generic interface constraint for the type parameter.

**Problem:**
```csharp
[EnumCollection(CollectionName = "Animals", Generic = true)]
public abstract class AnimalBase<T> : EnumOptionBase<T> where T : AnimalBase<T>
{
    // Missing interface constraint
}
```

**Solution:**
```csharp
public interface IAnimal
{
    string Species { get; }
}

[EnumCollection(CollectionName = "Animals", Generic = true)]
public abstract class AnimalBase<T> : EnumOptionBase<T>, IAnimal 
    where T : IAnimal, AnimalBase<T>
{
    public abstract string Species { get; }
}
```

### ENHENUM001: Duplicate lookup values detected

**Category:** EnhancedEnums | **Severity:** Warning

Detects when multiple enum options have the same value for a lookup property without `AllowMultiple = true`.

**Problem:**
```csharp
public abstract class AnimalBase : EnumOptionBase<AnimalBase>
{
    [EnumLookup("GetByClass")]  // Missing AllowMultiple
    public string AnimalClass { get; }
    
    protected AnimalBase(int id, string name, string animalClass) : base(id, name)
    {
        AnimalClass = animalClass;
    }
}

[EnumOption]
public class Cat : AnimalBase
{
    public Cat() : base(1, "Cat", "Mammal") { }  // AnimalClass = "Mammal"
}

[EnumOption]
public class Dog : AnimalBase  
{
    public Dog() : base(2, "Dog", "Mammal") { }  // AnimalClass = "Mammal" (duplicate!)
}
```

**Solution (Automatic Code Fix Available):**
```csharp
[EnumLookup("GetByClass", AllowMultiple = true)]  // Added AllowMultiple = true
public string AnimalClass { get; }
```

**Generated Code Changes:**

Without `AllowMultiple`:
```csharp
public static AnimalBase? GetByClass(string animalClass) // Returns single or null
```

With `AllowMultiple = true`:
```csharp
public static ImmutableArray<AnimalBase> GetByClass(string animalClass) // Returns all matches
```

## Development

### Project Structure

```
FractalDataWorks.EnhancedEnums.Analyzers/
├── Analyzers/
│   ├── AbstractMemberAnalyzer.cs           # ENH006, ENH007
│   ├── DuplicateEnumOptionAnalyzer.cs      # ENH004
│   ├── DuplicateLookupValueAnalyzer.cs     # ENHENUM001
│   ├── EnhancedEnumBaseAnalyzer.cs         # ENH1001
│   ├── EnhancedEnumConstructorAnalyzer.cs  # ENH1002
│   ├── EnumCollectionAttributeAnalyzer.cs  # ENH008, ENH009, ENH010
│   └── EnumOptionConstructorAnalyzer.cs    # ENH005
├── AnalyzerReleases.Shipped.md
├── AnalyzerReleases.Unshipped.md
└── FractalDataWorks.EnhancedEnums.Analyzers.csproj
```

### Adding New Analyzers

1. **Create Analyzer Class:**
```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MyCustomAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ENH011";
    
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Rule title",
        "Message format",
        "Category",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        // Analysis logic here
    }
}
```

2. **Update AnalyzerReleases.Unshipped.md:**
```markdown
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ENH011 | Usage | Warning | MyCustomAnalyzer - Description
```

3. **Add Tests:**
```csharp
[Fact]
public void ShouldDetectViolation()
{
    var source = @"
        // Test source code
    ";
    
    var expected = new DiagnosticResult(MyCustomAnalyzer.DiagnosticId)
        .WithLocation(line, column)
        .WithArguments("argument");
        
    await VerifyAnalyzerAsync(source, expected);
}
```

### Building and Packaging

The analyzer project uses the standard analyzer packaging configuration:

```xml
<!-- Analyzer package settings -->
<IncludeBuildOutput>false</IncludeBuildOutput>
<DevelopmentDependency>true</DevelopmentDependency>

<!-- Package analyzers in the correct location -->
<ItemGroup>
  <None Include="$(OutputPath)\$(AssemblyName).dll" 
        Pack="true" 
        PackagePath="analyzers/dotnet/cs" 
        Visible="false" />
</ItemGroup>
```

### Release Process

1. **Move rules from Unshipped to Shipped:**
   - When releasing, move rules from `AnalyzerReleases.Unshipped.md` to `AnalyzerReleases.Shipped.md`
   - Add release version header in shipped file
   - Clear unshipped file (keeping header structure)

2. **Version and publish:**
   - Update version in project file
   - Build and pack: `dotnet pack`
   - Publish to NuGet

## Testing

### Unit Testing

Analyzers should be thoroughly tested with various scenarios:

```csharp
[Fact]
public async Task ShouldDetectMissingCollectionName()
{
    var source = @"
using FractalDataWorks.EnhancedEnums;

[EnumCollection] // Missing CollectionName
public abstract class TestBase : EnumOptionBase<TestBase>
{
}";

    var expected = DiagnosticResult
        .CompilerError(EnumCollectionAttributeAnalyzer.MissingCollectionNameDiagnosticId)
        .WithSpan(4, 23, 4, 31)
        .WithArguments("TestBase");

    await VerifyAnalyzerAsync(source, expected);
}
```

### Integration Testing

Test analyzers with real Enhanced Enums scenarios:

```csharp
[Fact]
public async Task ShouldWorkWithCompleteEnumDefinition()
{
    var source = @"
using FractalDataWorks.EnhancedEnums;

[EnumCollection(CollectionName = ""Colors"")]
public abstract class ColorBase : EnumOptionBase<ColorBase>
{
    protected ColorBase(int id, string name) : base(id, name) { }
}

[EnumOption]
public class Red : ColorBase
{
    public Red() : base(1, ""Red"") { }
}";

    // Should produce no diagnostics
    await VerifyAnalyzerAsync(source);
}
```

### Performance Testing

Ensure analyzers perform well on large codebases:

```csharp
[Fact]
public void ShouldPerformWellOnLargeCodebase()
{
    // Test with multiple files and complex inheritance hierarchies
    var sources = GenerateLargeCodebase(1000); // 1000 enum classes
    
    var stopwatch = Stopwatch.StartNew();
    await VerifyAnalyzerAsync(sources);
    stopwatch.Stop();
    
    Assert.True(stopwatch.ElapsedMilliseconds < 5000); // Under 5 seconds
}
```

---

For more information about Enhanced Enums, visit the [main project documentation](../README.md) or the [GitHub repository](https://github.com/FractalDataWorks/Enhanced-Enums).