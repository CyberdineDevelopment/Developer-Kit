# FractalDataWorks.Services.Transformations.Abstractions

**Minimal data transformation service abstractions for the FractalDataWorks Framework.**

## Purpose

This package provides the foundational interfaces for data transformation services in the FractalDataWorks ecosystem. It defines the domain boundary for transformation operations through minimal, clean abstractions.

## Architecture

The transformation abstractions follow the framework's **enhanced service pattern**:

- **Core Interface**: `ITransformationProvider` extends `IFdwService`
- **Request Contract**: `ITransformationRequest` defines transformation input/output structure
- **Configuration Base**: `ITransformationConfiguration` provides configuration contract
- **Base Classes**: Add type constraints without implementation logic

## Key Interfaces

### Core Transformation Provider Interface
```csharp
public interface ITransformationProvider : IFdwService
{
    // Capability advertisement
    IReadOnlyList<string> SupportedInputTypes { get; }
    IReadOnlyList<string> SupportedOutputTypes { get; }
    IReadOnlyList<string> TransformationCategories { get; }
    int Priority { get; }
    
    // Validation and execution
    IFdwResult ValidateTransformation(string inputType, string outputType, string? transformationCategory = null);
    Task<IFdwResult<TOutput>> Transform<TOutput>(ITransformationRequest transformationRequest);
    Task<IFdwResult<object?>> Transform(ITransformationRequest transformationRequest);
    
    // Advanced capabilities
    Task<IFdwResult<ITransformationEngine>> CreateEngineAsync(ITransformationEngineConfiguration configuration);
    Task<IFdwResult<ITransformationMetrics>> GetTransformationMetricsAsync();
}
```

### Transformation Request Interface
```csharp
public interface ITransformationRequest
{
    // Request identification
    string RequestId { get; }
    
    // Input/output definition
    object? InputData { get; }
    string InputType { get; }
    string OutputType { get; }
    string? TransformationCategory { get; }
    Type ExpectedResultType { get; }
    TimeSpan? Timeout { get; }
    
    // Configuration and context
    IReadOnlyDictionary<string, object> Configuration { get; }
    IReadOnlyDictionary<string, object> Options { get; }
    ITransformationContext? Context { get; }
    
    // Fluent configuration
    ITransformationRequest WithInputData(object? newInputData, string? newInputType = null);
    ITransformationRequest WithOutputType(string newOutputType, Type? newExpectedResultType = null);
    ITransformationRequest WithConfiguration(IReadOnlyDictionary<string, object> newConfiguration);
    ITransformationRequest WithOptions(IReadOnlyDictionary<string, object> newOptions);
}
```

### Transformation Context Interface
```csharp
public interface ITransformationContext
{
    // Security and correlation
    string? Identity { get; }
    string? CorrelationId { get; }
    string? PipelineStage { get; }
    IReadOnlyDictionary<string, object> Properties { get; }
}
```

### Transformation Engine Interface
```csharp
public interface ITransformationEngine
{
    // Engine identification and state
    string EngineId { get; }
    string EngineType { get; }
    bool IsRunning { get; }
    
    // Engine operations
    Task<IFdwResult<ITransformationResult>> ExecuteTransformationAsync(
        ITransformationRequest request, 
        CancellationToken cancellationToken = default);
    Task<IFdwResult> StartAsync(CancellationToken cancellationToken = default);
    Task<IFdwResult> StopAsync(CancellationToken cancellationToken = default);
}
```

## Core Supporting Interfaces

### Engine Configuration
```csharp
public interface ITransformationEngineConfiguration : IFdwConfiguration
{
    string EngineType { get; }
    int MaxConcurrency { get; }
    int TimeoutSeconds { get; }
    bool EnableCaching { get; }
    bool EnableMetrics { get; }
}
```

### Performance Metrics
```csharp
public interface ITransformationMetrics
{
    long TotalTransformations { get; }
    long SuccessfulTransformations { get; }
    long FailedTransformations { get; }
    double AverageTransformationDurationMs { get; }
    int ActiveTransformations { get; }
    DateTime MetricsStartTime { get; }
    DateTime? LastTransformationTime { get; }
}
```

### Transformation Results
```csharp
public interface ITransformationResult
{
    // Result identification
    string RequestId { get; }
    string TransformationType { get; }
    
    // Execution information
    DateTime StartTime { get; }
    DateTime EndTime { get; }
    TimeSpan Duration { get; }
    
    // Result data
    bool IsSuccess { get; }
    object? ResultData { get; }
    string? ErrorMessage { get; }
    Exception? Exception { get; }
}
```

## Base Classes

The package includes base classes that **only add generic type constraints**:

- Base classes provide **no implementation logic**
- They exist solely for type safety and service enumeration

**Important**: Transformations use a **rich interface pattern** because they require:
- Format negotiation between providers
- Complex pipeline composition capabilities
- Engine management for stateful transformations
- Performance monitoring and optimization

## Enhanced Service Pattern

Transformations use an **enhanced service pattern** because:

- **Multi-Format Support**: Providers must advertise input/output capabilities
- **Pipeline Composition**: Multiple transformations chained together
- **Engine Abstraction**: Complex transformations need stateful engines
- **Performance Critical**: Metrics and optimization are essential
- **Context Awareness**: Security and correlation tracking required

This is more complex than simple command processing because transformations are **data processing services** with rich capabilities.

## Usage

Concrete implementations should:

1. **Implement ITransformationProvider** with format support advertisement
2. **Define transformation requests** for specific use cases
3. **Create transformation engines** for complex stateful operations
4. **Provide context information** for security and tracking
5. **Handle performance metrics** for optimization

## Generic Constraints

The type hierarchy balances **flexibility** with **type safety**:

```
IFdwService
    ↓
ITransformationProvider (rich interface for transformation)
    ↓
ConcreteTransformationProvider (TPL, Spark, Azure Data Factory, etc.)

ITransformationRequest (rich interface for requests)
    ↓
ConcreteTransformationRequest (specific transformation logic)
```

## Framework Integration

This abstraction integrates with other FractalDataWorks services:

- **DataProviders**: Source and destination for transformation data
- **ExternalConnections**: Connect to transformation engines and data sources
- **Authentication**: Secure access to transformation services
- **SecretManagement**: Store API keys for external transformation services
- **Scheduling**: Schedule batch transformations and data pipelines

## Design Philosophy

These abstractions balance **minimal design** with **transformation needs**:

- ✅ Define domain boundaries through interfaces
- ✅ Provide rich transformation capabilities
- ✅ Enable complex data pipeline scenarios
- ✅ Support multiple transformation backends
- ✅ Facilitate performance optimization
- ❌ No implementation logic in abstractions
- ❌ No specific transformation engine coupling
- ❌ No complex inheritance hierarchies

## Evolution

This package will grow organically as the framework evolves:

- New transformation types added when needed
- Enhanced pipeline composition capabilities
- Additional engine types for specific scenarios
- Improved performance monitoring and optimization
- Backward compatibility maintained for existing implementations

The rich interface design provides the capabilities needed for data transformation while maintaining clean abstractions and maximum implementation flexibility.