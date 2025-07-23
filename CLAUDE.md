# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the solution
dotnet build                     # Debug configuration (default)
dotnet build -c Release          # Production-ready build

# Run tests
dotnet test                      # All tests
dotnet test --filter "Category!=Integration"  # Unit tests only
dotnet test --collect:"XPlat Code Coverage"   # With code coverage

# Package NuGet packages
dotnet pack -c Release

# Run specific test project
dotnet test src/FractalDataWorks.Services.Tests/
```

## Architecture Overview

This is a layered .NET library architecture with progressive abstraction layers:

**Layer 0.5 - Core Foundation** (`FractalDataWorks.net`):
- Targets netstandard2.0 for maximum compatibility
- Contains ALL core abstractions (interfaces moved from Layer 1):
  - `IFdwService`, `IFdwConfiguration`, `IFdwResult`
  - `IServiceFactory`, `IConfigurationRegistry`
  - `IExternalConnection`, `IFdwConnection`
  - `IDataConnection`, `IFdwDataCommand`
- Base types that all other layers depend on

**Layer 1 - Domain-Specific Implementations**:
- `FractalDataWorks.Services`: Service base implementations
  - `ServiceBase<TConfiguration, TCommand>`
  - `DataConnection<TDataCommand, TConnection>` - Universal data service
- `FractalDataWorks.Connections`: External connection implementations
  - `ExternalConnectionBase<TCommandBuilder, TCommand, TConnection, TFactory, TConfig>`
  - Provider-specific connections (MsSql, Postgres, etc.)
  - Command builders for transforming universal commands
- `FractalDataWorks.Configuration`: Configuration providers and patterns
- `FractalDataWorks.DependencyInjection`: DI container abstractions
- `FractalDataWorks.Tools`: Common utilities and helpers
- `FractalDataWorks.Hosts`: Web and worker host abstractions
- `FractalDataWorks.Data`: Entity base classes and patterns

Each source project has a corresponding `.Tests` project for unit testing.

## Development Configuration

The project uses six build configurations with progressive quality gates:
- **Debug**: Fast development, minimal checks
- **Experimental**: Early development phase
- **Alpha**: Basic quality checks
- **Beta**: Recommended for development, warnings as errors
- **Preview**: Strict checks for pre-release
- **Release**: Production-ready with all checks

## Code Quality Tools

The project enforces code quality through multiple analyzers:
- StyleCop.Analyzers for code style
- AsyncFixer for async/await patterns
- Meziantou.Analyzer for performance
- Microsoft.VisualStudio.Threading.Analyzers for threading safety
- Roslynator.Analyzers for refactoring suggestions

Critical rules (security, memory leaks, async issues) are configured as errors in `.editorconfig`.

## Testing Approach

- **Framework**: xUnit v3
- **Parallel execution**: Enabled for faster test runs
- **Categories**: Tests are categorized as "Unit" or "Integration"
- **Coverage**: Uses Coverlet with Cobertura format
- Test projects follow naming pattern: `FractalDataWorks.[Package].Tests`

## Git Workflow

The project follows git-flow:
- `master`: Production releases only
- `develop`: Main development branch (current)
- `feature/*`: Feature development
- `beta/*`, `release/*`: Release preparation
- `experimental/*`: Experimental features

## Key Patterns

1. **Interface-based design**: All major components start with interfaces (e.g., `IFdwService`)
2. **Generic service pattern**: Services can be typed with configuration (`IFdwService<TConfig>`)
3. **Result pattern**: Operations return `IFdwResult` for consistent error handling
4. **Testability**: Every component designed for unit testing
5. **Package independence**: Each Layer 1 package can be used independently
6. **Universal Data Service**: Single data service with command pattern and provider-specific implementations
7. **External Connections as Boundaries**: External connections isolated from core architecture
8. **Command Transformation**: Universal commands transformed to provider-specific via builders

## Documentation Requirements

All public and protected members in source code require XML documentation comments. This includes:
- Classes and interfaces
- Methods and properties
- Public fields and events
- Type parameters and method parameters

Documentation requirements:
- **Source projects**: XML documentation required for all visible members (CS1591 warnings enabled)
- **Test projects**: No documentation required
- **Development phase**: Warnings shown for missing documentation to encourage adding it
- **Pre-publish phase**: Documentation will be verified via automated tooling (planned MCP server with Roslyn API integration)

The project generates XML documentation files for all builds.

## Code Organization Within Projects

Projects follow a consistent internal structure:
- **Folder by concept**: Types organized by functional area (e.g., `/Configuration/`, `/Services/`, `/Results/`)
- **One type per file**: Each interface or class gets its own file
- **File naming**: Matches type name exactly (e.g., `IFdwService.cs`)
- **Namespace hierarchy**: 
  - `FractalDataWorks.net` project uses `FractalDataWorks` as root namespace
  - Subfolders add to namespace (e.g., `FractalDataWorks.Configuration`)
  - Other projects use project name as namespace directly
- **No separate interface folders**: Interfaces and implementations grouped by function, not by type

## Important Notes

- Do NOT add promotional text or emojis to commits or code
- Keep commit messages professional and descriptive

## Naming Conventions

- All framework types use the `Fdw` prefix (e.g., `IFdwService`, `FdwResult`)
- External connections use descriptive names (e.g., `MsSqlConnection`, `PostgresConnection`)
- Command builders follow pattern: `{Provider}CommandBuilder`

## Data Service Architecture

The data layer is implemented as a universal service:

1. **Universal Commands**: `IFdwDataCommand` contains LINQ-like queries
2. **Data Service**: `DataConnection<TDataCommand, TConnection>` inherits from `ServiceBase`
3. **Provider Selection**: `ExternalConnectionProvider` selects appropriate connection
4. **Command Transformation**: Provider-specific command builders transform universal commands
5. **Execution**: Provider executes transformed commands and returns results

Flow: `IFdwDataCommand → DataConnection → ExternalConnectionProvider → Provider Connection → Command Builder → Execute → Result`