# FractalDataWorks.Validation.SourceGenerator - Implementation Notes

## Project Structure

This source generator automatically creates `IFdwResult Validate()` extension methods for configuration classes that have corresponding `AbstractValidator<T>` implementations.

### Files Created

1. **FractalDataWorks.Validation.SourceGenerator.csproj**
   - .NET Standard 2.0 target for maximum compatibility
   - References Microsoft.CodeAnalysis.CSharp for source generation
   - Configured as development dependency and analyzer

2. **ValidationExtensionsGenerator.cs**
   - Main source generator implementing ISourceGenerator
   - Scans compilation for AbstractValidator<T> classes
   - Generates extension methods using custom code builder

3. **CodeBuilder/ClassBuilder.cs**
   - Custom implementation following FractalDataWorks.CodeBuilder patterns
   - Fluent API for building C# classes
   - Handles using statements, namespaces, methods

4. **CodeBuilder/MethodBuilder.cs**
   - Fluent API for building C# methods
   - Supports access modifiers, parameters, async/static modifiers
   - Generates method signatures and bodies

5. **GlobalUsings.cs**
   - Global using statements for common namespaces
   - Microsoft.CodeAnalysis, System.Collections.Immutable

6. **GlobalSuppressions.cs**
   - Suppresses analyzer warnings that are acceptable for this project
   - Allows ISourceGenerator usage despite analyzer preferences

7. **README.md**
   - Comprehensive documentation of features and usage
   - Examples of input validators and generated output

## Code Generation Logic

### Discovery Process
1. Scans all syntax trees in compilation
2. Finds classes inheriting from `AbstractValidator<T>`
3. Extracts generic type parameter `T` (configuration type)
4. Groups validators by namespace

### Generated Extension Methods

For each `AbstractValidator<TConfig>` found:

```csharp
// Synchronous validation
public static IFdwResult Validate(this TConfig config)
{
    var validator = new TConfigValidator();
    var result = validator.Validate(config);
    
    if (result.IsValid)
        return FdwResult.Success();
    
    var errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
    return FdwResult.Failure($"Validation failed: {errors}");
}

// Asynchronous validation
public static async Task<IFdwResult> ValidateAsync(this TConfig config, CancellationToken cancellationToken = default)
{
    var validator = new TConfigValidator();
    var result = await validator.ValidateAsync(config, cancellationToken);
    
    if (result.IsValid)
        return FdwResult.Success();
    
    var errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
    return FdwResult.Failure($"Validation failed: {errors}");
}
```

### Generated File Structure
- One file per namespace containing validators
- Naming pattern: `ValidationExtensions_{namespace}.g.cs`
- Partial static classes allowing custom extensions

## Integration Requirements

### Dependencies
- FluentValidation (for AbstractValidator<T>)
- FractalDataWorks.Results (for IFdwResult and FdwResult)

### Usage in Projects
Add analyzer reference to consumer projects:
```xml
<ItemGroup>
  <Analyzer Include="path\to\FractalDataWorks.Validation.SourceGenerator.dll" />
</ItemGroup>
```

## Technical Decisions

### ISourceGenerator vs IIncrementalSourceGenerator
- Used ISourceGenerator for compatibility with .NET Standard 2.0
- IIncrementalSourceGenerator requires newer Roslyn versions
- Suppressed analyzer warnings about obsolete interface

### Custom Code Builder Implementation
- Created simplified version of FractalDataWorks.CodeBuilder patterns
- Maintains consistent API style with main framework
- Avoids dependency on external code builder packages

### Error Handling
- Aggregates multiple validation errors into single message
- Uses consistent FractalDataWorks.Results pattern
- Provides both sync and async validation methods

## Build Configuration

### Project Properties
- `TargetFramework`: netstandard2.0
- `IncludeBuildOutput`: false (analyzer project)
- `GeneratePackageOnBuild`: false (disabled for build simplicity)
- `DevelopmentDependency`: true

### Package References
- Microsoft.CodeAnalysis.CSharp (centrally managed version)
- System.Collections.Immutable (for ImmutableArray support)

### Suppressions
- RS1042: ISourceGenerator obsolete warning
- RS1036: EnforceExtendedAnalyzerRules warning
- MA0002: StringComparer warnings where acceptable
- MA0051: Method length warnings for generated code

## Testing Strategy

The source generator can be tested by:

1. Adding analyzer reference to projects with AbstractValidator<T> classes
2. Building the project to trigger source generation
3. Verifying generated extension methods compile correctly
4. Testing runtime behavior with actual configuration objects

Example test projects:
- FractalDataWorks.Services.ExternalConnections.MsSql (has MsSqlConfigurationValidator)
- Any project with FluentValidation validators

## Future Enhancements

1. **Incremental Source Generator**: Upgrade to IIncrementalSourceGenerator when framework allows
2. **Custom Attributes**: Support for controlling generation with attributes
3. **Validation Context**: Pass additional context to validators
4. **Custom Error Formatting**: Allow customization of error message format
5. **Package Distribution**: Create proper NuGet package for distribution

## Compatibility

- .NET Standard 2.0+ projects
- Compatible with FractalDataWorks ecosystem
- Requires FluentValidation 11.0+
- Works with both .NET Framework and .NET Core/.NET 5+ projects