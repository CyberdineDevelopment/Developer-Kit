# SmartDelegates Development Plan

## Project: FractalDataWorks.SmartGenerators.SmartDelegates

### Overview
SmartDelegates is a source generator-based library that creates queryable, composable, multi-stage processing pipelines using Enhanced Enums. It implements the Chain of Responsibility pattern with built-in observability, allowing inspection and modification of processing stages at runtime.

### Core Objectives
1. Create multi-stage processing pipelines that are queryable and modifiable
2. Support synchronous and asynchronous execution models
3. Enable middleware-style composition with before/after hooks
4. Provide rich telemetry and debugging capabilities
5. Allow runtime modification of pipeline stages
6. Maintain type safety throughout the pipeline

## Architecture Components

### 1. Core Abstractions

#### Base Classes
```csharp
// FractalDataWorks.SmartDelegates/Abstractions/SmartDelegateBase.cs
namespace FractalDataWorks.SmartDelegates;

public abstract record SmartDelegateBase<TContext>(string Name, int Order) : IEnhancedEnum<SmartDelegateBase<TContext>>
    where TContext : IPipelineContext
{
    public abstract Task<Result<TContext>> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
    
    public virtual bool ShouldExecute(TContext context) => true;
    
    public virtual int Priority => 100;
    
    // Middleware hooks
    public virtual Task<Result<TContext>> BeforeExecuteAsync(TContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<TContext>.Ok(context));
        
    public virtual Task<Result<TContext>> AfterExecuteAsync(TContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<TContext>.Ok(context));
}

// For transformational pipelines
public abstract record TransformDelegate<TInput, TOutput>(string Name, int Order) 
    : IEnhancedEnum<TransformDelegate<TInput, TOutput>>
{
    public abstract Task<Result<TOutput>> TransformAsync(TInput input, CancellationToken cancellationToken = default);
    
    // Composition support
    public TransformDelegate<TInput, TNewOutput> Then<TNewOutput>(TransformDelegate<TOutput, TNewOutput> next) =>
        new ComposedTransform<TInput, TOutput, TNewOutput>(this, next);
}
```

#### Context Interfaces
```csharp
// FractalDataWorks.SmartDelegates/Interfaces/IPipelineContext.cs
public interface IPipelineContext
{
    Guid CorrelationId { get; }
    Dictionary<string, object> Items { get; }
    bool ContinueProcessing { get; set; }
    PipelineMetadata Metadata { get; }
}

// For tracking pipeline execution
public interface IPipelineMetadata
{
    DateTime StartTime { get; }
    TimeSpan Elapsed { get; }
    List<StageExecution> ExecutedStages { get; }
    Dictionary<string, object> Metrics { get; }
}

// For queryable pipelines
public interface IQueryablePipeline<TDelegate>
    where TDelegate : IEnhancedEnum
{
    IEnumerable<TDelegate> Stages { get; }
    IQueryablePipeline<TDelegate> Where(Func<TDelegate, bool> predicate);
    IQueryablePipeline<TDelegate> Skip(int count);
    IQueryablePipeline<TDelegate> Take(int count);
    IQueryablePipeline<TDelegate> OrderBy<TKey>(Func<TDelegate, TKey> keySelector);
}
```

### 2. Attribute System

```csharp
// FractalDataWorks.SmartDelegates/Attributes/SmartDelegateAttribute.cs
[AttributeUsage(AttributeTargets.Class)]
public class SmartDelegateAttribute : EnhancedEnumAttribute
{
    public SmartDelegateAttribute(string pipelineName) : base(pipelineName) { }
    
    public Type ContextType { get; set; }
    public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.Sequential;
    public bool EnableTelemetry { get; set; } = true;
    public string ProviderClassName { get; set; }
}

// FractalDataWorks.SmartDelegates/Attributes/StageAttribute.cs
[AttributeUsage(AttributeTargets.Class)]
public class StageAttribute : EnumOptionAttribute
{
    public string Category { get; set; }
    public string[] Dependencies { get; set; } = Array.Empty<string>();
    public bool CanBeSkipped { get; set; } = false;
    public bool CanRunInParallel { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 30;
}

// FractalDataWorks.SmartDelegates/Attributes/ConditionalAttribute.cs
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ConditionalAttribute : Attribute
{
    public string Condition { get; set; } // Expression that evaluates to bool
    public ConditionalBehavior Behavior { get; set; } = ConditionalBehavior.Skip;
}

public enum ExecutionMode
{
    Sequential,      // Execute stages one after another
    Parallel,        // Execute independent stages in parallel
    Pipeline,        // Stream processing with backpressure
    Transactional    // All or nothing execution
}
```

### 3. Source Generator Structure

#### Generator Components

```
FractalDataWorks.SmartGenerators.SmartDelegates/
├── SmartDelegateGenerator.cs            # Main generator entry point
├── Discovery/
│   ├── DelegateDiscoveryService.cs     # Finds SmartDelegate implementations
│   ├── StageAnalyzer.cs                # Analyzes pipeline stages
│   ├── DependencyResolver.cs           # Resolves stage dependencies
│   └── ConditionalAnalyzer.cs          # Analyzes conditional execution
├── Generation/
│   ├── PipelineProviderGenerator.cs    # Generates pipeline executors
│   ├── QueryableWrapperGenerator.cs    # Generates queryable wrappers
│   ├── TelemetryGenerator.cs           # Generates telemetry code
│   ├── CompositionGenerator.cs         # Generates composition helpers
│   └── ParallelExecutorGenerator.cs    # Generates parallel execution
├── Models/
│   ├── PipelineDefinition.cs           # Model for discovered pipelines
│   ├── StageDefinition.cs              # Model for pipeline stages
│   ├── DependencyGraph.cs              # Model for stage dependencies
│   └── ExecutionPlan.cs                # Model for optimized execution
└── Templates/
    ├── PipelineProviderTemplate.cs     # Template for providers
    ├── QueryableTemplate.cs            # Template for queryable wrappers
    └── TelemetryTemplate.cs            # Template for telemetry
```

#### Generated Output Structure

For each SmartDelegate pipeline, generate:

1. **Pipeline Provider**
```csharp
// Generated: {PipelineName}Provider.g.cs
public sealed partial class {PipelineName}Provider : IPipelineProvider<{TContext}>
{
    private readonly List<SmartDelegateBase<{TContext}>> _stages;
    private readonly DependencyGraph _dependencies;
    private readonly ITelemetryProvider _telemetry;
    
    public {PipelineName}Provider(ITelemetryProvider telemetry = null)
    {
        _telemetry = telemetry ?? NullTelemetryProvider.Instance;
        _stages = InitializeStages();
        _dependencies = BuildDependencyGraph();
    }
    
    public async Task<Result<{TContext}>> ExecuteAsync(
        {TContext} context, 
        CancellationToken cancellationToken = default)
    {
        var execution = new PipelineExecution(context, _telemetry);
        
        try
        {
            foreach (var stage in GetExecutionOrder(context))
            {
                if (!context.ContinueProcessing) break;
                
                var result = await ExecuteStageAsync(stage, context, execution, cancellationToken);
                if (result.IsFailure) return result;
            }
            
            return Result<{TContext}>.Ok(context);
        }
        finally
        {
            await _telemetry.RecordExecutionAsync(execution);
        }
    }
    
    // Queryable support
    public IQueryablePipeline<SmartDelegateBase<{TContext}>> Query() =>
        new QueryablePipeline<SmartDelegateBase<{TContext}>>(_stages);
}
```

2. **Queryable Wrapper**
```csharp
// Generated: {PipelineName}QueryExtensions.g.cs
public static class {PipelineName}QueryExtensions
{
    public static IQueryablePipeline<T> WhereCategory<T>(
        this IQueryablePipeline<T> pipeline, 
        string category) where T : SmartDelegateBase<{TContext}>
    {
        return pipeline.Where(stage => stage.GetCategory() == category);
    }
    
    public static IQueryablePipeline<T> ExcludeSkippable<T>(
        this IQueryablePipeline<T> pipeline) where T : SmartDelegateBase<{TContext}>
    {
        return pipeline.Where(stage => !stage.CanBeSkipped);
    }
}
```

3. **Composition Helpers**
```csharp
// Generated: {PipelineName}Composition.g.cs
public static class {PipelineName}Composition
{
    public static IPipelineBuilder<{TContext}> CreateBuilder() =>
        new PipelineBuilder<{TContext}>();
        
    public static IPipelineBuilder<{TContext}> WithStage(
        this IPipelineBuilder<{TContext}> builder,
        SmartDelegateBase<{TContext}> stage) =>
        builder.AddStage(stage);
        
    public static IPipelineBuilder<{TContext}> WithMiddleware(
        this IPipelineBuilder<{TContext}> builder,
        Func<{TContext}, Task<Result<{TContext}>>>> middleware) =>
        builder.AddMiddleware(middleware);
}
```

### 4. Implementation Patterns

#### Validation Pipeline
```csharp
[SmartDelegate("ValidationPipeline", ContextType = typeof(ValidationContext))]
public abstract record ValidationStage(string Name, int Order) : SmartDelegateBase<ValidationContext>(Name, Order);

[Stage(Order = 10, Category = "Schema")]
public record SchemaValidation() : ValidationStage("Schema", 10)
{
    public override async Task<Result<ValidationContext>> ExecuteAsync(
        ValidationContext context, 
        CancellationToken cancellationToken)
    {
        var errors = ValidateSchema(context.Input);
        if (errors.Any())
        {
            context.Errors.AddRange(errors);
            context.ContinueProcessing = false;
        }
        return Result<ValidationContext>.Ok(context);
    }
}

[Stage(Order = 20, Category = "Business", Dependencies = new[] { "Schema" })]
[Conditional(Condition = "context.Input.Type == 'Order'")]
public record OrderValidation() : ValidationStage("OrderValidation", 20)
{
    public override async Task<Result<ValidationContext>> ExecuteAsync(
        ValidationContext context,
        CancellationToken cancellationToken)
    {
        // Validate order-specific rules
        return Result<ValidationContext>.Ok(context);
    }
}
```

#### Transform Pipeline
```csharp
[SmartDelegate("DataTransformPipeline")]
public abstract record DataTransform(string Name, int Order) : TransformDelegate<RawData, ProcessedData>(Name, Order);

[Stage(Order = 10, CanRunInParallel = true)]
public record NormalizeTransform() : DataTransform("Normalize", 10)
{
    public override async Task<Result<ProcessedData>> TransformAsync(
        RawData input,
        CancellationToken cancellationToken)
    {
        var normalized = await NormalizeDataAsync(input);
        return Result<ProcessedData>.Ok(normalized);
    }
}
```

### 5. Advanced Features

#### Parallel Execution
```csharp
// Generated parallel executor for independent stages
public async Task<Result<TContext>> ExecuteParallelStagesAsync(
    IEnumerable<SmartDelegateBase<TContext>> stages,
    TContext context,
    CancellationToken cancellationToken)
{
    var tasks = stages.Select(stage => ExecuteStageAsync(stage, context.Clone(), cancellationToken));
    var results = await Task.WhenAll(tasks);
    
    // Merge results
    return MergeResults(results, context);
}
```

#### Pipeline Modification
```csharp
public class DynamicPipeline<TContext> where TContext : IPipelineContext
{
    private readonly List<SmartDelegateBase<TContext>> _stages;
    
    public DynamicPipeline<TContext> InsertBefore(string stageName, SmartDelegateBase<TContext> newStage)
    {
        var index = _stages.FindIndex(s => s.Name == stageName);
        if (index >= 0) _stages.Insert(index, newStage);
        return this;
    }
    
    public DynamicPipeline<TContext> Replace(string stageName, SmartDelegateBase<TContext> newStage)
    {
        var index = _stages.FindIndex(s => s.Name == stageName);
        if (index >= 0) _stages[index] = newStage;
        return this;
    }
    
    public DynamicPipeline<TContext> Remove(string stageName)
    {
        _stages.RemoveAll(s => s.Name == stageName);
        return this;
    }
}
```

#### Telemetry Integration
```csharp
public interface ITelemetryProvider
{
    Task RecordStageExecutionAsync(StageExecution execution);
    Task RecordPipelineExecutionAsync(PipelineExecution execution);
    IDisposable BeginScope(string scopeName, Dictionary<string, object> properties);
}

public class StageExecution
{
    public string StageName { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
    public Dictionary<string, object> Metrics { get; set; }
}
```

### 6. Integration Standards

#### Dependency Injection
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmartDelegates(
        this IServiceCollection services,
        Action<SmartDelegateOptions> configure = null)
    {
        var options = new SmartDelegateOptions();
        configure?.Invoke(options);
        
        // Register all discovered pipelines
        foreach (var pipelineType in SmartDelegateDiscovery.FindAll())
        {
            var providerType = pipelineType.GetGeneratedProviderType();
            services.AddScoped(providerType);
        }
        
        // Register telemetry
        if (options.EnableTelemetry)
        {
            services.AddSingleton<ITelemetryProvider, DefaultTelemetryProvider>();
        }
        
        return services;
    }
}
```

#### Usage Patterns
```csharp
// Basic usage
var pipeline = new ValidationPipelineProvider();
var result = await pipeline.ExecuteAsync(context);

// Querying stages
var criticalStages = pipeline.Query()
    .Where(s => s.Priority < 50)
    .OrderBy(s => s.Order)
    .ToList();

// Dynamic modification
var customPipeline = pipeline.CreateDynamic()
    .InsertAfter("SchemaValidation", new CustomValidation())
    .Remove("OptionalValidation")
    .Build();

// With dependency injection
public class OrderService
{
    private readonly ValidationPipelineProvider _validationPipeline;
    private readonly DataTransformPipelineProvider _transformPipeline;
    
    public OrderService(
        ValidationPipelineProvider validationPipeline,
        DataTransformPipelineProvider transformPipeline)
    {
        _validationPipeline = validationPipeline;
        _transformPipeline = transformPipeline;
    }
}
```

### 7. Testing Infrastructure

#### Test Helpers
```csharp
public abstract class PipelineTestBase<TPipeline, TContext>
    where TPipeline : SmartDelegateBase<TContext>
    where TContext : IPipelineContext
{
    protected Mock<ITelemetryProvider> TelemetryMock { get; private set; }
    
    [SetUp]
    public void Setup()
    {
        TelemetryMock = new Mock<ITelemetryProvider>();
    }
    
    protected async Task AssertStageExecuted(string stageName)
    {
        TelemetryMock.Verify(t => t.RecordStageExecutionAsync(
            It.Is<StageExecution>(e => e.StageName == stageName)), 
            Times.Once);
    }
    
    protected TContext CreateContext() => // Factory method
}
```

#### Pipeline Testing
```csharp
[Test]
public async Task Pipeline_ShouldExecuteInOrder()
{
    var pipeline = new TestPipelineProvider(TelemetryMock.Object);
    var context = CreateContext();
    
    var result = await pipeline.ExecuteAsync(context);
    
    result.IsSuccess.ShouldBeTrue();
    AssertStagesExecutedInOrder("Stage1", "Stage2", "Stage3");
}

[Test]
public async Task ConditionalStage_ShouldSkipWhenConditionFalse()
{
    var pipeline = new TestPipelineProvider();
    var context = CreateContext();
    context.Items["SkipOptional"] = true;
    
    var result = await pipeline.ExecuteAsync(context);
    
    await AssertStageNotExecuted("OptionalStage");
}
```

### 8. Package Structure

```
FractalDataWorks.SmartDelegates/
├── FractalDataWorks.SmartDelegates/              # Runtime library
│   ├── Abstractions/
│   ├── Attributes/
│   ├── Interfaces/
│   ├── Telemetry/
│   └── Extensions/
├── FractalDataWorks.SmartGenerators.SmartDelegates/  # Source generator
│   ├── Discovery/
│   ├── Generation/
│   ├── Models/
│   ├── Analysis/
│   └── Templates/
└── FractalDataWorks.SmartDelegates.Tests/       # Test project
    ├── Unit/
    ├── Integration/
    ├── Performance/
    └── TestHelpers/
```

### 9. Configuration

#### Pipeline Configuration
```json
{
  "SmartDelegates": {
    "EnableTelemetry": true,
    "DefaultTimeout": 30,
    "MaxParallelStages": 4,
    "Pipelines": {
      "ValidationPipeline": {
        "Enabled": true,
        "DisabledStages": ["ExpensiveValidation"],
        "StageTimeouts": {
          "SchemaValidation": 5,
          "BusinessValidation": 10
        }
      }
    }
  }
}
```

#### MSBuild Integration
```xml
<PropertyGroup>
  <!-- Control generated code -->
  <SmartDelegatesGenerateTelemetry>true</SmartDelegatesGenerateTelemetry>
  <SmartDelegatesGenerateQueryable>true</SmartDelegatesGenerateQueryable>
  <SmartDelegatesEnableParallel>true</SmartDelegatesEnableParallel>
</PropertyGroup>
```

### 10. Performance Considerations

#### Optimization Strategies
1. **Stage Reordering**: Analyze dependencies to optimize execution order
2. **Parallel Execution**: Identify independent stages for parallel processing
3. **Short-Circuit Evaluation**: Skip remaining stages when appropriate
4. **Caching**: Cache stage results when idempotent
5. **Pooling**: Pool context objects to reduce allocations

#### Generated Optimizations
```csharp
// Generated code includes optimizations based on analysis
private static readonly StageExecutionPlan _executionPlan = new()
{
    ParallelGroups = new[]
    {
        new[] { "Stage1", "Stage2" }, // Can run in parallel
        new[] { "Stage3" }            // Depends on previous group
    },
    CacheableStages = new[] { "ExpensiveCalculation" },
    SkippableStages = new[] { "OptionalValidation" }
};
```

## Dependencies

- **FractalDataWorks.SmartGenerators.EnhancedEnums**: Base Enhanced Enum functionality
- **Microsoft.CodeAnalysis.CSharp**: For source generation
- **System.Threading.Tasks.Dataflow**: For pipeline execution (optional)
- **Microsoft.Extensions.DependencyInjection.Abstractions**: For DI integration

## Success Criteria

1. Support for complex, multi-stage pipelines with dependencies
2. Runtime queryability and modification of pipeline stages
3. Built-in telemetry and observability
4. Performance optimization through parallel execution
5. Type-safe context flowing through pipeline
6. Easy testing with comprehensive test helpers
7. Integration with existing Enhanced Enum infrastructure
8. Support for both sync and async execution models