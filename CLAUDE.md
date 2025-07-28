# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

### Building the Solution
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/FractalDataWorks.Configuration/FractalDataWorks.Configuration.csproj

# Build in Release mode
dotnet build -c Release
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/FractalDataWorks.Configuration.Tests/
```

### AgentManager Commands
```bash
# Run the MCP server
dotnet run --project arch/AgentManager/src/FractalDataWorks.AgentManager.McpServer

# Publishing (use PowerShell scripts)
.\arch\AgentManager\scripts\publish.ps1          # Interactive menu
.\arch\AgentManager\scripts\publish-local.ps1    # Standard publish
.\arch\AgentManager\scripts\publish-single-file.ps1  # Single executable
```

## High-Level Architecture

This is a **convention-based SDK framework** for building configurable services in .NET 9.0.

### Core Architecture Principles

1. **Layered Design with Specific Rules**:
   - `FractalDataWorks.Net` - Core interfaces only, NO dependencies
   - `FractalDataWorks.Services` - Base implementations and cross-cutting concerns
   - Feature projects - Specific business logic implementations
   - Data access projects - Connection providers and adapters

2. **Service Pattern**: All services inherit from `ServiceBase<TConfiguration>` with:
   - Strongly-typed configuration that self-validates
   - Automatic configuration binding from settings
   - Built-in logging and error handling

3. **Result Pattern**: Async operations return `Result<T>` for explicit error handling:
   ```csharp
   var result = await service.GetDataAsync();
   if (result.IsSuccess)
   {
       var data = result.Value;
   }
   ```

4. **Configuration-First Design**: Every service has a corresponding configuration class:
   - Configurations implement `IValidatableObject`
   - Self-documenting with XML comments
   - Auto-discovered by convention (`{ServiceName}Configuration`)

5. **Fail-Safe by Default**:
   - Methods return `bool` or `Result<T>` instead of throwing
   - Invalid objects use Null Object Pattern (`Invalid{Type}`)
   - Validation happens at object creation boundaries

### Key Patterns and Conventions

- **CQRS with MediatR**: Commands and queries in separate namespaces
- **Factory Pattern**: Use static factory methods for safe object creation
- **Enhanced Enums**: Type-safe enums with additional metadata
- **Smart Delegates**: Encapsulated delegate patterns for common scenarios
- **Async-First**: New methods are async with Result<T>, legacy sync methods wrap async

### Project Structure

- `/src` - Source projects following capability-based naming (not layer names)
- `/tests` - xUnit v3 tests with Shouldly assertions and NSubstitute mocks
- `/arch` - Architecture documentation and the AgentManager sub-project
- Solution uses Central Package Management via `Directory.Packages.props`

### AgentManager Sub-Project

Located in `/arch/AgentManager/`, this is a full-featured task management system with:
- MCP (Model Context Protocol) server for AI agent integration
- CQRS implementation for all operations
- SQLite database with migration scripts
- PowerShell publishing scripts for distribution

### Important Development Notes

- **Branch-Specific Warnings**: The `Directory.Build.props` sets warnings as errors on master branch only
- **Versioning**: Uses Nerdbank.GitVersioning for automatic semantic versioning
- **No Namespace Layer Names**: Use capability names like `Configuration`, not `BusinessLogic`
- **Universal Data Access**: Write data access code once, run against any provider via adapters