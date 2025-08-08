# FractalDataWorks.Services.Scheduling.Abstractions

**Minimal task scheduling service abstractions for the FractalDataWorks Framework.**

## Purpose

This package provides the foundational interfaces for task scheduling services in the FractalDataWorks ecosystem. It defines the domain boundary for scheduling operations through minimal, clean abstractions.

## Architecture

The scheduling abstractions follow the framework's **enhanced service pattern**:

- **Core Interface**: `IScheduler` extends base service functionality
- **Task Contract**: `IScheduledTask` defines executable task structure
- **Configuration Base**: `ISchedulingConfiguration` provides configuration contract
- **Base Classes**: Add type constraints without implementation logic

## Key Interfaces

### Core Scheduler Interface
```csharp
public interface IScheduler
{
    // Service identification
    string ServiceName { get; }
    string ServiceVersion { get; }
    string ServiceDescription { get; }
    
    // Capability information
    IReadOnlyList<string> SupportedSchedulingStrategies { get; }
    IReadOnlyList<string> SupportedExecutionModes { get; }
    
    // Runtime information
    int? MaxConcurrentTasks { get; }
    int ActiveTaskCount { get; }
    int QueuedTaskCount { get; }
    
    // Task management
    Task<IFdwResult<string>> ScheduleTask(IScheduledTask task, ITaskSchedule schedule);
    Task<IFdwResult> CancelTask(string taskId);
    Task<IFdwResult> PauseTask(string taskId);
    Task<IFdwResult> ResumeTask(string taskId);
    Task<IFdwResult> ExecuteTaskImmediately(string taskId);
}
```

### Scheduled Task Interface
```csharp
public interface IScheduledTask
{
    // Task identification
    string TaskId { get; }
    string TaskName { get; }
    string TaskCategory { get; }
    int Priority { get; }
    
    // Execution parameters
    TimeSpan? ExpectedExecutionTime { get; }
    TimeSpan? MaxExecutionTime { get; }
    IReadOnlyList<string> Dependencies { get; }
    bool AllowsConcurrentExecution { get; }
    
    // Configuration and metadata
    IReadOnlyDictionary<string, object> Configuration { get; }
    IReadOnlyDictionary<string, object> Metadata { get; }
    
    // Task execution
    Task<IFdwResult<object?>> ExecuteAsync(ITaskExecutionContext context);
    IFdwResult ValidateTask();
    Task OnCleanupAsync(ITaskExecutionContext context, string reason);
}
```

### Task Schedule Interface
```csharp
public interface ITaskSchedule
{
    // Schedule identification and type
    string ScheduleType { get; } // "Cron", "Interval", "Once", "Dependent"
    string? CronExpression { get; }
    TimeSpan? Interval { get; }
    DateTimeOffset? StartAt { get; }
    DateTimeOffset? EndAt { get; }
    bool IsRecurring { get; }
    
    // Dependencies and conditions
    string[]? DependsOn { get; }
    string? Condition { get; }
    
    // Execution settings
    int MaxRetries { get; }
    TimeSpan RetryDelay { get; }
    bool EnableRecovery { get; }
}
```

### Execution Context Interface
```csharp
public interface ITaskExecutionContext
{
    // Context information
    string ExecutionId { get; }
    string TaskId { get; }
    DateTimeOffset StartTime { get; }
    IServiceProvider ServiceProvider { get; }
    CancellationToken CancellationToken { get; }
    
    // Progress and state management
    void ReportProgress(int percentage, string? message = null);
    Task SetCheckpointAsync(object checkpointData);
    Task<T?> GetCheckpointAsync<T>();
    
    // Logging and metrics
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
}
```

## Base Classes

The package includes base classes that **only add generic type constraints**:

- Base classes provide **no implementation logic**
- They exist solely for type safety and service enumeration

**Important**: Unlike other abstractions, scheduling uses a **rich interface pattern** because scheduling requires complex orchestration capabilities.

## Enhanced Service Pattern

Scheduling uses an **enhanced service pattern** because:

- **Complex Orchestration**: Tasks need lifecycle management, dependencies, and state tracking
- **Runtime Querying**: Schedulers need to report current state and capabilities
- **Rich Metadata**: Tasks have execution parameters, constraints, and configuration
- **Context Awareness**: Execution contexts provide services and state management

This is more complex than simple command processing because scheduling is an **orchestration service**.

## Usage

Concrete implementations should:

1. **Implement IScheduler** with actual scheduling logic (Quartz, Hangfire, etc.)
2. **Define task types** that implement `IScheduledTask`
3. **Create schedule definitions** that implement `ITaskSchedule`
4. **Provide execution contexts** that implement `ITaskExecutionContext`
5. **Handle task lifecycle** including errors, retries, and cleanup

## Generic Constraints

The type hierarchy is **intentionally flat** to avoid over-engineering:

```
IScheduler (rich interface for orchestration)
    ↓
ConcreteScheduler (Quartz, Hangfire, In-Memory, etc.)

IScheduledTask (rich interface for tasks)
    ↓
ConcreteTask (specific business logic)
```

## Framework Integration

This abstraction integrates with other FractalDataWorks services:

- **DataProviders**: Tasks can perform data operations
- **ExternalConnections**: Tasks can connect to external systems
- **Authentication**: Tasks run with proper security context
- **SecretManagement**: Tasks access secrets securely

## Design Philosophy

These abstractions balance **minimal design** with **orchestration needs**:

- ✅ Define domain boundaries through interfaces
- ✅ Provide rich task and scheduler capabilities
- ✅ Enable complex orchestration scenarios
- ✅ Support multiple scheduling backends
- ❌ No implementation logic in abstractions
- ❌ No specific scheduler coupling
- ❌ No complex inheritance hierarchies

## Evolution

This package will grow organically as the framework evolves:

- New scheduler capabilities added when needed
- Enhanced task contexts for more complex scenarios
- Additional schedule types for specific patterns
- Backward compatibility maintained for existing implementations

The rich interface design provides the capabilities needed for task scheduling while maintaining clean abstractions and implementation flexibility.