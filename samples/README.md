# FractalDataWorks Samples

This directory contains sample applications demonstrating the FractalDataWorks framework capabilities.

## Building and Running Samples

The samples are **not** built as part of the main solution build. To build and run samples:

```bash
cd samples
dotnet build FractalDataWorks.Samples.sln
```

## Available Samples

### 1. BasicService Sample
**Location:** `FractalDataWorks.Samples.BasicService`

Demonstrates:
- Creating a service that inherits from `ServiceBase`
- Configuration with validation using FluentValidation
- JSON configuration source for persistence
- Command pattern implementation
- Service lifecycle management
- Logging integration

To run:
```bash
cd FractalDataWorks.Samples.BasicService
dotnet run
```

### 2. ConfigurationDemo Sample (Coming Soon)
**Location:** `FractalDataWorks.Samples.ConfigurationDemo`

Will demonstrate:
- Multiple configuration sources
- Configuration registry usage
- Configuration validation patterns
- Runtime configuration updates

### 3. EnhancedEnums Sample (Coming Soon)
**Location:** `FractalDataWorks.Samples.EnhancedEnums`

Will demonstrate:
- Enhanced Enum pattern for service types
- Service type factories
- Service type providers
- Dynamic service selection

## Key Concepts Demonstrated

### Service Pattern
All services inherit from `ServiceBase` which provides:
- Automatic logging
- Configuration management
- Command execution pipeline
- Service lifecycle hooks

### Configuration Pattern
Configurations inherit from `ConfigurationBase` which provides:
- Self-validation using FluentValidation
- IsEnabled/IsValid properties
- Automatic validation caching

### Command Pattern
Commands implement `ICommand` and are processed by services:
- Type-safe command execution
- Result pattern for error handling
- Support for generic return types

### JSON Configuration Source
The `JsonConfigurationSource` provides:
- File-based configuration persistence
- Type-safe serialization/deserialization
- CRUD operations for configurations

## Sample Architecture

```
samples/
├── Directory.Build.props     # Excludes samples from main build
├── FractalDataWorks.Samples.sln
├── README.md
└── FractalDataWorks.Samples.BasicService/
    ├── Program.cs           # Main entry point
    ├── WeatherService.cs    # Sample service implementation
    ├── WeatherServiceConfiguration.cs
    └── WeatherServiceConfigurationValidator.cs
```

## Notes

- Samples use `net10.0` target framework
- All samples reference local project builds (not NuGet packages)
- Samples are excluded from code coverage analysis
- Each sample is self-contained and can be run independently