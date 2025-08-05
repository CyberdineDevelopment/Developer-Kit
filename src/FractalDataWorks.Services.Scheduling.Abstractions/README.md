# FractalDataWorks.Services.Scheduling.Abstractions

## Overview

The FractalDataWorks Scheduling Abstractions library provides the foundational interfaces for task scheduling and execution management within the FractalDataWorks Framework. This library enables developers to create schedulers that can execute tasks on various schedules, manage task lifecycles, and provide robust execution coordination with dependency management, resource allocation, and monitoring capabilities.

Key features include:
- **Task Scheduling**: Support for cron expressions, intervals, one-time execution, and dependency-based scheduling
- **Execution Management**: Task lifecycle management with pause, resume, cancel, and immediate execution
- **Dependency Coordination**: Task dependency resolution and workflow orchestration
- **Resource Management**: Concurrent execution limits and resource allocation
- **Monitoring & Metrics**: Comprehensive task and scheduler performance monitoring
- **Execution Context**: Rich execution environment with progress reporting and checkpointing

## Architecture

The scheduling system is built around several core interfaces:

- `IScheduler`: Main interface for scheduler providers that manage task execution
- `IScheduledTask`: Interface for tasks that can be scheduled and executed
- `ITaskExecutionContext`: Runtime context provided to tasks during execution

## Quick Start

### Basic Scheduler Usage

```csharp
using FractalDataWorks.Services.Scheduling.Abstractions;
using FractalDataWorks.Framework.Abstractions;

// Get scheduler from service provider
var scheduler = serviceProvider.GetRequiredService<IScheduler>();

// Create a simple task
var task = new MyDataProcessingTask();
var schedule = new CronSchedule("0 */15 * * * *"); // Every 15 minutes

// Schedule the task
var result = await scheduler.ScheduleTask(task, schedule);
if (result.IsSuccess)
{
    Console.WriteLine($"Task scheduled with ID: {result.Value}");
}
```

### Creating a Scheduled Task

```csharp
public class DataCleanupTask : IScheduledTask
{
    public string TaskId => "data-cleanup-001";
    public string TaskName => "Daily Data Cleanup";
    public string TaskCategory => "Maintenance";
    public int Priority => 100;
    
    public TimeSpan? ExpectedExecutionTime => TimeSpan.FromMinutes(30);
    public TimeSpan? MaxExecutionTime => TimeSpan.FromHours(2);
    
    public IReadOnlyList<string> Dependencies => Array.Empty<string>();
    public bool AllowsConcurrentExecution => false;
    
    public IReadOnlyDictionary<string, object> Configuration { get; } = 
        new Dictionary<string, object>
        {
            ["RetentionDays"] = 90,
            ["BatchSize"] = 1000
        };
    
    public IReadOnlyDictionary<string, object> Metadata { get; } = 
        new Dictionary<string, object>
        {
            ["Owner"] = "DataTeam",
            ["Version"] = "1.2.0"
        };

    public async Task<IFdwResult<object?>> ExecuteAsync(ITaskExecutionContext context)
    {
        try
        {
            context.ReportProgress(0, "Starting data cleanup");
            
            var dataProvider = context.ServiceProvider.GetRequiredService<IDataProvider>();
            var retentionDays = (int)Configuration["RetentionDays"];
            var batchSize = (int)Configuration["BatchSize"];
            
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var deletedCount = 0;
            
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var deleteCommand = new DataCommand("DELETE FROM old_data WHERE created_date < @cutoff LIMIT @batch")
                {
                    Parameters = new Dictionary<string, object>
                    {
                        ["cutoff"] = cutoffDate,
                        ["batch"] = batchSize
                    }
                };
                
                var deleteResult = await dataProvider.ExecuteCommand<int>(deleteCommand);
                if (!deleteResult.IsSuccess || deleteResult.Value == 0)
                    break;
                
                deletedCount += deleteResult.Value;
                context.ReportProgress(Math.Min(95, deletedCount / 100), 
                    $"Deleted {deletedCount} records");
                
                // Set checkpoint for large operations
                await context.SetCheckpointAsync(new { DeletedCount = deletedCount, LastCutoff = cutoffDate });
            }
            
            context.ReportProgress(100, "Data cleanup completed");
            return FdwResult.Success<object?>(new { DeletedRecords = deletedCount });
        }
        catch (Exception ex)
        {
            return FdwResult.Failure<object?>(
                FdwError.Create("TASK_EXECUTION_FAILED", $"Data cleanup failed: {ex.Message}", ex));
        }
    }
    
    public IFdwResult ValidateTask()
    {
        if (!Configuration.ContainsKey("RetentionDays") || (int)Configuration["RetentionDays"] <= 0)
            return FdwResult.Failure(FdwError.Create("INVALID_CONFIG", "RetentionDays must be positive"));
        
        if (!Configuration.ContainsKey("BatchSize") || (int)Configuration["BatchSize"] <= 0)
            return FdwResult.Failure(FdwError.Create("INVALID_CONFIG", "BatchSize must be positive"));
        
        return FdwResult.Success();
    }
    
    public async Task OnCleanupAsync(ITaskExecutionContext context, string reason)
    {
        // Perform cleanup operations if needed
        Console.WriteLine($"Task cleanup triggered: {reason}");
        await Task.CompletedTask;
    }
}
```

## Implementation Examples

### 1. Quartz.NET Scheduler Provider

```csharp
using Quartz;
using Quartz.Impl;

public class QuartzSchedulerProvider : IScheduler
{
    private readonly IScheduler _quartzScheduler;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, IScheduledTask> _tasks = new();

    public QuartzSchedulerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var factory = new StdSchedulerFactory();
        _quartzScheduler = factory.GetScheduler().Result;
        _quartzScheduler.Start();
    }

    public string ServiceName => "Quartz.NET Scheduler";
    public string ServiceVersion => "3.7.0";
    public string ServiceDescription => "Enterprise job scheduling using Quartz.NET";

    public IReadOnlyList<string> SupportedSchedulingStrategies { get; } = new[]
    {
        "Cron", "Interval", "Once", "Daily", "Weekly", "Monthly"
    };

    public IReadOnlyList<string> SupportedExecutionModes { get; } = new[]
    {
        "Sequential", "Parallel", "Batch"
    };

    public int? MaxConcurrentTasks => 50;
    public int ActiveTaskCount => _quartzScheduler.GetCurrentlyExecutingJobs().Result.Count;
    public int QueuedTaskCount => _quartzScheduler.GetTriggersOfJob(new JobKey("*")).Result.Count;

    public async Task<IFdwResult<string>> ScheduleTask(IScheduledTask task, ITaskSchedule schedule)
    {
        try
        {
            var validation = task.ValidateTask();
            if (!validation.IsSuccess)
                return FdwResult.Failure<string>(validation.Error);

            _tasks[task.TaskId] = task;

            var job = JobBuilder.Create<QuartzTaskExecutor>()
                .WithIdentity(task.TaskId)
                .UsingJobData("TaskId", task.TaskId)
                .Build();

            var trigger = CreateTrigger(task.TaskId, schedule);
            
            await _quartzScheduler.ScheduleJob(job, trigger);
            
            return FdwResult.Success(task.TaskId);
        }
        catch (Exception ex)
        {
            return FdwResult.Failure<string>(
                FdwError.Create("SCHEDULE_FAILED", $"Failed to schedule task: {ex.Message}", ex));
        }
    }

    public async Task<IFdwResult> CancelTask(string taskId)
    {
        try
        {
            var deleted = await _quartzScheduler.DeleteJob(new JobKey(taskId));
            _tasks.TryRemove(taskId, out _);
            
            return deleted 
                ? FdwResult.Success() 
                : FdwResult.Failure(FdwError.Create("TASK_NOT_FOUND", $"Task {taskId} not found"));
        }
        catch (Exception ex)
        {
            return FdwResult.Failure(
                FdwError.Create("CANCEL_FAILED", $"Failed to cancel task: {ex.Message}", ex));
        }
    }

    public async Task<IFdwResult> PauseTask(string taskId)
    {
        try
        {
            await _quartzScheduler.PauseJob(new JobKey(taskId));
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure(
                FdwError.Create("PAUSE_FAILED", $"Failed to pause task: {ex.Message}", ex));
        }
    }

    public async Task<IFdwResult> ResumeTask(string taskId)
    {
        try
        {
            await _quartzScheduler.ResumeJob(new JobKey(taskId));
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure(
                FdwError.Create("RESUME_FAILED", $"Failed to resume task: {ex.Message}", ex));
        }
    }

    private ITrigger CreateTrigger(string taskId, ITaskSchedule schedule)
    {
        return schedule.ScheduleType switch
        {
            "Cron" => TriggerBuilder.Create()
                .WithIdentity($"{taskId}-trigger")
                .WithCronSchedule(schedule.CronExpression)
                .Build(),
            "Interval" => TriggerBuilder.Create()
                .WithIdentity($"{taskId}-trigger")
                .WithSimpleSchedule(x => x.WithInterval(schedule.Interval.Value).RepeatForever())
                .Build(),
            _ => throw new NotSupportedException($"Schedule type {schedule.ScheduleType} not supported")
        };
    }
}

[DisallowConcurrentExecution]
public class QuartzTaskExecutor : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var taskId = context.JobDetail.JobDataMap.GetString("TaskId");
        var schedulerProvider = (QuartzSchedulerProvider)context.Scheduler.Context["SchedulerProvider"];
        
        if (schedulerProvider._tasks.TryGetValue(taskId, out var task))
        {
            var executionContext = new QuartzTaskExecutionContext(context, schedulerProvider._serviceProvider);
            await task.ExecuteAsync(executionContext);
        }
    }
}
```

### 2. Hangfire Scheduler Provider

```csharp
using Hangfire;

public class HangfireSchedulerProvider : IScheduler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, IScheduledTask> _tasks = new();

    public HangfireSchedulerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public string ServiceName => "Hangfire Scheduler";
    public string ServiceVersion => "1.8.0";
    public string ServiceDescription => "Background job processing using Hangfire";

    public IReadOnlyList<string> SupportedSchedulingStrategies { get; } = new[]
    {
        "Cron", "Interval", "Once", "Delayed", "Recurring"
    };

    public IReadOnlyList<string> SupportedExecutionModes { get; } = new[]
    {
        "Sequential", "Parallel", "Queue-based"
    };

    public int? MaxConcurrentTasks => null; // Hangfire manages this
    public int ActiveTaskCount => JobStorage.Current.GetMonitoringApi().ProcessingCount();
    public int QueuedTaskCount => JobStorage.Current.GetMonitoringApi().EnqueuedCount("default");

    public async Task<IFdwResult<string>> ScheduleTask(IScheduledTask task, ITaskSchedule schedule)
    {
        try
        {
            var validation = task.ValidateTask();
            if (!validation.IsSuccess)
                return FdwResult.Failure<string>(validation.Error);

            _tasks[task.TaskId] = task;

            var jobId = schedule.ScheduleType switch
            {
                "Once" => BackgroundJob.Enqueue<HangfireTaskExecutor>(x => x.ExecuteTask(task.TaskId)),
                "Delayed" => BackgroundJob.Schedule<HangfireTaskExecutor>(
                    x => x.ExecuteTask(task.TaskId), schedule.StartAt.Value),
                "Recurring" => RecurringJob.AddOrUpdate<HangfireTaskExecutor>(
                    task.TaskId, x => x.ExecuteTask(task.TaskId), schedule.CronExpression),
                _ => throw new NotSupportedException($"Schedule type {schedule.ScheduleType} not supported")
            };

            return FdwResult.Success(jobId ?? task.TaskId);
        }
        catch (Exception ex)
        {
            return FdwResult.Failure<string>(
                FdwError.Create("SCHEDULE_FAILED", $"Failed to schedule task: {ex.Message}", ex));
        }
    }

    public async Task<IFdwResult> CancelTask(string taskId)
    {
        try
        {
            BackgroundJob.Delete(taskId);
            RecurringJob.RemoveIfExists(taskId);
            _tasks.TryRemove(taskId, out _);
            
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            return FdwResult.Failure(
                FdwError.Create("CANCEL_FAILED", $"Failed to cancel task: {ex.Message}", ex));
        }
    }
}

public class HangfireTaskExecutor
{
    private readonly IServiceProvider _serviceProvider;
    
    public HangfireTaskExecutor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteTask(string taskId)
    {
        var schedulerProvider = _serviceProvider.GetRequiredService<HangfireSchedulerProvider>();
        
        if (schedulerProvider._tasks.TryGetValue(taskId, out var task))
        {
            var executionContext = new HangfireTaskExecutionContext(_serviceProvider);
            await task.ExecuteAsync(executionContext);
        }
    }
}
```

### 3. In-Memory Scheduler Provider

```csharp
public class InMemorySchedulerProvider : IScheduler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, ScheduledTaskInfo> _tasks = new();
    private readonly Timer _schedulerTimer;
    private readonly SemaphoreSlim _executionSemaphore;

    public InMemorySchedulerProvider(IServiceProvider serviceProvider, int maxConcurrentTasks = 10)
    {
        _serviceProvider = serviceProvider;
        MaxConcurrentTasks = maxConcurrentTasks;
        _executionSemaphore = new SemaphoreSlim(maxConcurrentTasks);
        
        _schedulerTimer = new Timer(CheckScheduledTasks, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    public string ServiceName => "In-Memory Scheduler";
    public string ServiceVersion => "1.0.0";
    public string ServiceDescription => "Simple in-memory task scheduler for testing and light workloads";

    public IReadOnlyList<string> SupportedSchedulingStrategies { get; } = new[]
    {
        "Cron", "Interval", "Once", "Dependent"
    };

    public IReadOnlyList<string> SupportedExecutionModes { get; } = new[]
    {
        "Sequential", "Parallel"
    };

    public int? MaxConcurrentTasks { get; }
    public int ActiveTaskCount => MaxConcurrentTasks.Value - _executionSemaphore.CurrentCount;
    public int QueuedTaskCount => _tasks.Count(t => t.Value.Status == TaskStatus.Scheduled);

    public async Task<IFdwResult<string>> ScheduleTask(IScheduledTask task, ITaskSchedule schedule)
    {
        try
        {
            var validation = task.ValidateTask();
            if (!validation.IsSuccess)
                return FdwResult.Failure<string>(validation.Error);

            var taskInfo = new ScheduledTaskInfo
            {
                Task = task,
                Schedule = schedule,
                Status = TaskStatus.Scheduled,
                NextExecution = CalculateNextExecution(schedule)
            };

            _tasks[task.TaskId] = taskInfo;
            
            return FdwResult.Success(task.TaskId);
        }
        catch (Exception ex)
        {
            return FdwResult.Failure<string>(
                FdwError.Create("SCHEDULE_FAILED", $"Failed to schedule task: {ex.Message}", ex));
        }
    }

    private async void CheckScheduledTasks(object state)
    {
        var now = DateTimeOffset.UtcNow;
        
        var readyTasks = _tasks.Values
            .Where(t => t.Status == TaskStatus.Scheduled && t.NextExecution <= now)
            .ToList();

        foreach (var taskInfo in readyTasks)
        {
            if (await _executionSemaphore.WaitAsync(0)) // Don't block
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ExecuteTaskInternal(taskInfo);
                    }
                    finally
                    {
                        _executionSemaphore.Release();
                    }
                });
            }
        }
    }

    private async Task ExecuteTaskInternal(ScheduledTaskInfo taskInfo)
    {
        taskInfo.Status = TaskStatus.Running;
        taskInfo.LastExecution = DateTimeOffset.UtcNow;
        
        try
        {
            var executionContext = new InMemoryTaskExecutionContext(_serviceProvider);
            var result = await taskInfo.Task.ExecuteAsync(executionContext);
            
            taskInfo.Status = TaskStatus.Completed;
            taskInfo.LastResult = result;
            
            // Schedule next execution if recurring
            if (taskInfo.Schedule.IsRecurring)
            {
                taskInfo.NextExecution = CalculateNextExecution(taskInfo.Schedule);
                taskInfo.Status = TaskStatus.Scheduled;
            }
        }
        catch (Exception ex)
        {
            taskInfo.Status = TaskStatus.Failed;
            taskInfo.LastError = ex;
        }
    }

    private DateTimeOffset CalculateNextExecution(ITaskSchedule schedule)
    {
        return schedule.ScheduleType switch
        {
            "Once" => schedule.StartAt ?? DateTimeOffset.UtcNow,
            "Interval" => DateTimeOffset.UtcNow.Add(schedule.Interval.Value),
            "Cron" => CronExpression.Parse(schedule.CronExpression).GetNextOccurrence(DateTimeOffset.UtcNow) 
                     ?? DateTimeOffset.MaxValue,
            _ => DateTimeOffset.MaxValue
        };
    }
}

public class ScheduledTaskInfo
{
    public IScheduledTask Task { get; set; }
    public ITaskSchedule Schedule { get; set; }
    public TaskStatus Status { get; set; }
    public DateTimeOffset? NextExecution { get; set; }
    public DateTimeOffset? LastExecution { get; set; }
    public IFdwResult<object?> LastResult { get; set; }
    public Exception LastError { get; set; }
}

public enum TaskStatus
{
    Scheduled,
    Running,
    Completed,
    Failed,
    Paused,
    Cancelled
}
```

## Configuration Examples

### JSON Configuration

```json
{
  "FractalDataWorks": {
    "Scheduling": {
      "Provider": "Quartz",
      "QuartzSettings": {
        "MaxConcurrentJobs": 50,
        "ThreadPoolSize": 10,
        "MisfireThreshold": "00:01:00",
        "JobStore": {
          "Type": "AdoJobStore",
          "ConnectionString": "Server=localhost;Database=QuartzJobs;Trusted_Connection=true;",
          "TablePrefix": "QRTZ_"
        }
      },
      "HangfireSettings": {
        "ConnectionString": "Server=localhost;Database=HangfireJobs;Trusted_Connection=true;",
        "QueueNames": ["default", "critical", "background"],
        "WorkerCount": 20
      },
      "InMemorySettings": {
        "MaxConcurrentTasks": 10,
        "CheckInterval": "00:00:30"
      },
      "DefaultTaskSettings": {
        "MaxExecutionTime": "01:00:00",
        "RetryAttempts": 3,
        "RetryDelay": "00:05:00"
      }
    }
  }
}
```

### Service Registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register scheduler provider based on configuration
    var schedulingConfig = Configuration.GetSection("FractalDataWorks:Scheduling");
    var provider = schedulingConfig["Provider"];
    
    switch (provider?.ToLower())
    {
        case "quartz":
            services.AddSingleton<IScheduler, QuartzSchedulerProvider>();
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
            });
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            break;
            
        case "hangfire":
            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(schedulingConfig["HangfireSettings:ConnectionString"]);
            });
            services.AddHangfireServer();
            services.AddSingleton<IScheduler, HangfireSchedulerProvider>();
            break;
            
        case "inmemory":
        default:
            services.AddSingleton<IScheduler, InMemorySchedulerProvider>();
            break;
    }

    // Register task factory and common services
    services.AddSingleton<ITaskFactory, TaskFactory>();
    services.AddScoped<ITaskExecutionMetrics, TaskExecutionMetrics>();
}
```

## Advanced Usage

### Task Dependencies and Workflows

```csharp
public class WorkflowOrchestrator
{
    private readonly IScheduler _scheduler;
    
    public WorkflowOrchestrator(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }
    
    public async Task<IFdwResult> CreateDataProcessingWorkflow()
    {
        // Step 1: Data Extraction
        var extractTask = new DataExtractionTask();
        var extractSchedule = new OnceSchedule { StartAt = DateTimeOffset.UtcNow };
        
        var extractResult = await _scheduler.ScheduleTask(extractTask, extractSchedule);
        if (!extractResult.IsSuccess) return extractResult;
        
        // Step 2: Data Transformation (depends on extraction)
        var transformTask = new DataTransformationTask 
        { 
            Dependencies = new[] { extractTask.TaskId } 
        };
        var transformSchedule = new DependentSchedule { DependsOn = new[] { extractTask.TaskId } };
        
        var transformResult = await _scheduler.ScheduleTask(transformTask, transformSchedule);
        if (!transformResult.IsSuccess) return transformResult;
        
        // Step 3: Data Loading (depends on transformation)
        var loadTask = new DataLoadingTask 
        { 
            Dependencies = new[] { transformTask.TaskId } 
        };
        var loadSchedule = new DependentSchedule { DependsOn = new[] { transformTask.TaskId } };
        
        return await _scheduler.ScheduleTask(loadTask, loadSchedule);
    }
}
```

### Custom Task with Progress Reporting

```csharp
public class LargeDataProcessingTask : IScheduledTask
{
    public string TaskId => "large-data-processing";
    public string TaskName => "Large Dataset Processing";
    public string TaskCategory => "DataProcessing";
    public int Priority => 50;
    
    public async Task<IFdwResult<object?>> ExecuteAsync(ITaskExecutionContext context)
    {
        var dataProvider = context.ServiceProvider.GetRequiredService<IDataProvider>();
        var totalRecords = await GetTotalRecordCount(dataProvider);
        var processedRecords = 0;
        var batchSize = 1000;
        
        while (processedRecords < totalRecords && !context.CancellationToken.IsCancellationRequested)
        {
            // Process batch
            var batch = await GetNextBatch(dataProvider, processedRecords, batchSize);
            await ProcessBatch(batch, context.CancellationToken);
            
            processedRecords += batch.Count;
            
            // Report progress
            var percentage = (int)((double)processedRecords / totalRecords * 100);
            context.ReportProgress(percentage, $"Processed {processedRecords:N0} of {totalRecords:N0} records");
            
            // Set checkpoint every 10 batches
            if (processedRecords % (batchSize * 10) == 0)
            {
                await context.SetCheckpointAsync(new { ProcessedRecords = processedRecords });
            }
        }
        
        return FdwResult.Success<object?>(new { ProcessedRecords = processedRecords });
    }
}
```

### Task Monitoring and Metrics

```csharp
public class SchedulerMonitoringService
{
    private readonly IScheduler _scheduler;
    
    public SchedulerMonitoringService(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }
    
    public async Task<SchedulerHealthReport> GetHealthReport()
    {
        var metricsResult = await _scheduler.GetSchedulerMetricsAsync();
        var allTasksResult = await _scheduler.GetAllTasks();
        
        if (!metricsResult.IsSuccess || !allTasksResult.IsSuccess)
        {
            return new SchedulerHealthReport { IsHealthy = false };
        }
        
        var metrics = metricsResult.Value;
        var tasks = allTasksResult.Value;
        
        return new SchedulerHealthReport
        {
            IsHealthy = true,
            ActiveTasks = _scheduler.ActiveTaskCount,
            QueuedTasks = _scheduler.QueuedTaskCount,
            TotalTasks = tasks.Count,
            FailedTasks = tasks.Count(t => t.Status == "Failed"),
            AverageExecutionTime = metrics.AverageTaskExecutionTime,
            TaskExecutionRate = metrics.TaskExecutionRate,
            ResourceUtilization = CalculateResourceUtilization()
        };
    }
    
    public async Task<IFdwResult> CheckTaskHealth(string taskId)
    {
        var taskInfoResult = await _scheduler.GetTaskInfo(taskId);
        if (!taskInfoResult.IsSuccess)
            return taskInfoResult;
            
        var taskInfo = taskInfoResult.Value;
        
        // Check if task is stuck
        if (taskInfo.Status == "Running" && 
            DateTimeOffset.UtcNow - taskInfo.StartTime > taskInfo.MaxExecutionTime)
        {
            return FdwResult.Failure(
                FdwError.Create("TASK_TIMEOUT", $"Task {taskId} has exceeded maximum execution time"));
        }
        
        // Check failure rate
        if (taskInfo.FailureRate > 0.8)
        {
            return FdwResult.Failure(
                FdwError.Create("HIGH_FAILURE_RATE", $"Task {taskId} has high failure rate: {taskInfo.FailureRate:P}"));
        }
        
        return FdwResult.Success();
    }
}
```

## Integration Examples

### Integrating with Data Providers

```csharp
public class DatabaseMaintenanceTask : IScheduledTask
{
    public async Task<IFdwResult<object?>> ExecuteAsync(ITaskExecutionContext context)
    {
        var dataProvider = context.ServiceProvider.GetRequiredService<IDataProvider>();
        
        // Rebuild indexes
        var rebuildIndexesCommand = new DataCommand("EXEC sp_MSforeachtable 'ALTER INDEX ALL ON ? REBUILD'");
        var rebuildResult = await dataProvider.ExecuteCommand(rebuildIndexesCommand);
        
        if (!rebuildResult.IsSuccess)
        {
            return FdwResult.Failure<object?>(rebuildResult.Error);
        }
        
        // Update statistics
        var updateStatsCommand = new DataCommand("EXEC sp_updatestats");
        var statsResult = await dataProvider.ExecuteCommand(updateStatsCommand);
        
        if (!statsResult.IsSuccess)
        {
            return FdwResult.Failure<object?>(statsResult.Error);
        }
        
        context.ReportProgress(100, "Database maintenance completed");
        
        return FdwResult.Success<object?>(new 
        { 
            IndexesRebuilt = rebuildResult.Value,
            StatisticsUpdated = statsResult.Value
        });
    }
}
```

### Integrating with External Connections

```csharp
public class DataSynchronizationTask : IScheduledTask
{
    public async Task<IFdwResult<object?>> ExecuteAsync(ITaskExecutionContext context)
    {
        var connectionFactory = context.ServiceProvider.GetRequiredService<IExternalConnectionFactory>();
        
        // Get source connection
        var sourceResult = await connectionFactory.CreateConnection("source-api");
        if (!sourceResult.IsSuccess) return FdwResult.Failure<object?>(sourceResult.Error);
        
        // Get destination connection  
        var destResult = await connectionFactory.CreateConnection("dest-database");
        if (!destResult.IsSuccess) return FdwResult.Failure<object?>(destResult.Error);
        
        using var sourceConnection = sourceResult.Value;
        using var destConnection = destResult.Value;
        
        // Perform synchronization
        var syncResult = await SynchronizeData(sourceConnection, destConnection, context);
        
        return syncResult;
    }
    
    private async Task<IFdwResult<object?>> SynchronizeData(
        IExternalConnection source, 
        IExternalConnection destination,
        ITaskExecutionContext context)
    {
        var syncedRecords = 0;
        var batch = await source.GetDataBatch(1000);
        
        while (batch.Any() && !context.CancellationToken.IsCancellationRequested)
        {
            await destination.WriteBatch(batch);
            syncedRecords += batch.Count();
            
            context.ReportProgress(0, $"Synchronized {syncedRecords} records");
            batch = await source.GetDataBatch(1000);
        }
        
        return FdwResult.Success<object?>(new { SynchronizedRecords = syncedRecords });
    }
}
```

## License

Licensed under the Apache License, Version 2.0.