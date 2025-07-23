# FractalDataWorks.Tools

🚧 **IN PROGRESS** - Enhanced Enum Type Factories implementation in progress

Common utilities, helpers, and extension methods for the FractalDataWorks framework.

## Overview

FractalDataWorks.Tools provides:
- Extension methods for common operations
- Utility classes and helpers
- Performance utilities
- String manipulation helpers
- Collection extensions
- Async utilities

## Planned Components

### String Extensions

```csharp
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to PascalCase.
    /// </summary>
    public static string ToPascalCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;
            
        return CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(value.ToLower())
            .Replace(" ", "")
            .Replace("_", "");
    }
    
    /// <summary>
    /// Converts a string to camelCase.
    /// </summary>
    public static string ToCamelCase(this string value)
    {
        var pascalCase = value.ToPascalCase();
        if (string.IsNullOrEmpty(pascalCase))
            return pascalCase;
            
        return char.ToLowerInvariant(pascalCase[0]) + pascalCase.Substring(1);
    }
    
    /// <summary>
    /// Truncates a string to a specified length.
    /// </summary>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;
            
        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }
    
    /// <summary>
    /// Checks if a string contains any of the specified values.
    /// </summary>
    public static bool ContainsAny(this string value, params string[] values)
    {
        return values.Any(v => value.Contains(v, StringComparison.OrdinalIgnoreCase));
    }
}
```

### Collection Extensions

```csharp
public static class CollectionExtensions
{
    /// <summary>
    /// Performs an action on each element of the collection.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
    
    /// <summary>
    /// Converts a collection to a HashSet.
    /// </summary>
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
    {
        return new HashSet<T>(source, comparer);
    }
    
    /// <summary>
    /// Splits a collection into batches of specified size.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return YieldBatchElements(enumerator, batchSize - 1);
        }
        
        IEnumerable<T> YieldBatchElements(IEnumerator<T> source, int batchSize)
        {
            yield return source.Current;
            for (int i = 0; i < batchSize && source.MoveNext(); i++)
            {
                yield return source.Current;
            }
        }
    }
    
    /// <summary>
    /// Returns distinct elements based on a key selector.
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        foreach (var element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }
}
```

### Async Utilities

```csharp
public static class AsyncExtensions
{
    /// <summary>
    /// Executes async operations with a timeout.
    /// </summary>
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource();
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
        
        if (completedTask == task)
        {
            cts.Cancel();
            return await task;
        }
        
        throw new TimeoutException($"Operation timed out after {timeout}");
    }
    
    /// <summary>
    /// Retries an async operation with exponential backoff.
    /// </summary>
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        TimeSpan? initialDelay = null)
    {
        var delay = initialDelay ?? TimeSpan.FromSeconds(1);
        var exceptions = new List<Exception>();
        
        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (i < maxRetries)
            {
                exceptions.Add(ex);
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
        
        throw new AggregateException($"Operation failed after {maxRetries} retries", exceptions);
    }
    
    /// <summary>
    /// Executes multiple async operations in parallel with a degree of parallelism.
    /// </summary>
    public static async Task<IEnumerable<T>> WhenAllThrottled<T>(
        IEnumerable<Task<T>> tasks,
        int maxDegreeOfParallelism)
    {
        using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
        var results = new List<T>();
        
        var wrappedTasks = tasks.Select(async task =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await task;
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        return await Task.WhenAll(wrappedTasks);
    }
}
```

### Date/Time Utilities

```csharp
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to Unix timestamp.
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }
    
    /// <summary>
    /// Creates DateTime from Unix timestamp.
    /// </summary>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
    }
    
    /// <summary>
    /// Gets a human-readable time ago string.
    /// </summary>
    public static string ToTimeAgo(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;
        
        return timeSpan.TotalSeconds switch
        {
            < 60 => "just now",
            < 120 => "1 minute ago",
            < 3600 => $"{(int)timeSpan.TotalMinutes} minutes ago",
            < 7200 => "1 hour ago",
            < 86400 => $"{(int)timeSpan.TotalHours} hours ago",
            < 172800 => "yesterday",
            _ => $"{(int)timeSpan.TotalDays} days ago"
        };
    }
    
    /// <summary>
    /// Checks if a date is a weekend.
    /// </summary>
    public static bool IsWeekend(this DateTime dateTime)
    {
        return dateTime.DayOfWeek == DayOfWeek.Saturday || 
               dateTime.DayOfWeek == DayOfWeek.Sunday;
    }
}
```

### Guard Clauses

```csharp
public static class Guard
{
    public static T NotNull<T>(T value, string parameterName) where T : class
    {
        if (value == null)
            throw new ArgumentNullException(parameterName);
        return value;
    }
    
    public static string NotNullOrEmpty(string value, string parameterName)
    {
        NotNull(value, parameterName);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty or whitespace.", parameterName);
        return value;
    }
    
    public static int NotNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
        return value;
    }
    
    public static T NotDefault<T>(T value, string parameterName) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
            throw new ArgumentException("Value cannot be default.", parameterName);
        return value;
    }
    
    public static void InRange(int value, int min, int max, string parameterName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(parameterName, 
                $"Value must be between {min} and {max}.");
    }
}
```

### Performance Utilities

```csharp
public static class PerformanceMonitor
{
    /// <summary>
    /// Measures the execution time of an operation.
    /// </summary>
    public static TimeSpan MeasureTime(Action operation)
    {
        var stopwatch = Stopwatch.StartNew();
        operation();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
    
    /// <summary>
    /// Measures the execution time of an async operation.
    /// </summary>
    public static async Task<(T Result, TimeSpan Duration)> MeasureTimeAsync<T>(Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed);
    }
}

public class DisposableStopwatch : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly Action<TimeSpan> _onDispose;
    
    public DisposableStopwatch(Action<TimeSpan> onDispose)
    {
        _stopwatch = Stopwatch.StartNew();
        _onDispose = onDispose;
    }
    
    public void Dispose()
    {
        _stopwatch.Stop();
        _onDispose(_stopwatch.Elapsed);
    }
}
```

### Object Extensions

```csharp
public static class ObjectExtensions
{
    /// <summary>
    /// Performs a deep clone of an object using JSON serialization.
    /// </summary>
    public static T DeepClone<T>(this T source) where T : class
    {
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<T>(json)!;
    }
    
    /// <summary>
    /// Converts an object to a dictionary of its properties.
    /// </summary>
    public static Dictionary<string, object?> ToDictionary(this object source)
    {
        return source.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(
                prop => prop.Name,
                prop => prop.GetValue(source));
    }
}
```

## Usage Examples

### String Manipulation
```csharp
var input = "hello_world_example";
var pascalCase = input.ToPascalCase(); // "HelloWorldExample"
var camelCase = input.ToCamelCase();   // "helloWorldExample"

var longText = "This is a very long text that needs to be truncated";
var truncated = longText.Truncate(20); // "This is a very lo..."
```

### Collection Operations
```csharp
var numbers = Enumerable.Range(1, 100);
var batches = numbers.Batch(10); // 10 batches of 10 numbers each

var people = GetPeople();
var uniqueByName = people.DistinctBy(p => p.Name);

people.ForEach(p => Console.WriteLine(p.Name));
```

### Async Operations
```csharp
// With timeout
var result = await GetDataAsync().WithTimeout(TimeSpan.FromSeconds(30));

// With retry
var data = await AsyncExtensions.RetryAsync(
    async () => await apiClient.GetDataAsync(),
    maxRetries: 3,
    initialDelay: TimeSpan.FromSeconds(1));

// Parallel with throttling
var tasks = urls.Select(url => DownloadAsync(url));
var results = await AsyncExtensions.WhenAllThrottled(tasks, maxDegreeOfParallelism: 5);
```

### Guard Clauses
```csharp
public void ProcessOrder(Order order, int quantity)
{
    Guard.NotNull(order, nameof(order));
    Guard.NotNegative(quantity, nameof(quantity));
    Guard.InRange(quantity, 1, 100, nameof(quantity));
    
    // Process the order...
}
```

### Performance Monitoring
```csharp
// Measure synchronous operation
var duration = PerformanceMonitor.MeasureTime(() =>
{
    ProcessLargeDataSet();
});
Console.WriteLine($"Operation took {duration.TotalMilliseconds}ms");

// Measure async operation
var (result, elapsed) = await PerformanceMonitor.MeasureTimeAsync(async () =>
{
    return await CalculateAsync();
});
Console.WriteLine($"Calculation took {elapsed.TotalMilliseconds}ms");

// Using disposable stopwatch
using (new DisposableStopwatch(time => _logger.LogDebug($"Query took {time.TotalMilliseconds}ms")))
{
    await ExecuteQueryAsync();
}
```

## Installation

```xml
<PackageReference Include="FractalDataWorks.Tools" Version="*" />
```

## Dependencies

- FractalDataWorks.net (core abstractions)
- System.Text.Json (for serialization utilities)

## Status

This package is currently in planning phase. The utilities described above represent common patterns that will be implemented based on actual usage needs.

## Enhanced Enum Type Factories

🚧 **IN PROGRESS** - New pattern for tool type registration using Enhanced Enums

### Overview

The Enhanced Enum Type Factories pattern uses source generators to create strongly-typed tool registrations:

```csharp
[EnumOption(1, "CodeGenerator", "Source code generation tool")]
public class CodeGeneratorToolType : ToolTypeBase<ICodeGeneratorTool, CodeGeneratorConfiguration>
{
    public CodeGeneratorToolType() : base(1, "CodeGenerator", "Source code generation tool")
    {
    }

    public override object Create(CodeGeneratorConfiguration configuration)
    {
        return new CodeGeneratorTool(configuration);
    }

    public override Task<ICodeGeneratorTool> GetTool(string configurationName)
    {
        // Implementation to retrieve tool by configuration name
    }

    public override Task<ICodeGeneratorTool> GetTool(int configurationId)
    {
        // Implementation to retrieve tool by configuration ID
    }
}
```

### ToolTypeBase Pattern

The new pattern introduces two base classes:
- **ToolTypeFactoryBase<TTool, TConfiguration>**: Non-generic base with factory methods (no Enhanced Enum attributes)
- **ToolTypeBase<TTool, TConfiguration>**: Enhanced Enum base with `[EnhancedEnumBase]` attribute

### Benefits

1. **Compile-time Safety**: Tool types are generated at compile time
2. **IntelliSense Support**: Full IDE support for ToolTypes.* collections
3. **Automatic DI Registration**: Tools are automatically registered with dependency injection
4. **Factory Pattern**: Each tool type acts as a factory for creating tool instances

### Usage with Dependency Injection

```csharp
// Register all tool types in an assembly
services.AddToolTypes(Assembly.GetExecutingAssembly());

// Tool types are registered as both themselves and their factory interfaces
var generatorFactory = provider.GetService<IToolFactory<ICodeGeneratorTool, CodeGeneratorConfiguration>>();
var tool = generatorFactory.Create(generatorConfig);
```

### Generated Collections

Enhanced Enums generates static collections for easy access:

```csharp
// Access all tool types
var allTools = ToolTypes.All;

// Get by ID
var codeGenerator = ToolTypes.GetById(1);

// Get by name
var migrationTool = ToolTypes.GetByName("MigrationTool");

// Iterate through all
foreach (var toolType in ToolTypes.All)
{
    Console.WriteLine($"{toolType.Id}: {toolType.Name}");
}
```

### Example Implementation

```csharp
[EnumOption(2, "DataMigration", "Data migration and transformation tool")]
public class DataMigrationToolType : ToolTypeBase<IDataMigrationTool, DataMigrationConfiguration>
{
    private readonly ILogger<DataMigrationToolType> _logger;
    private readonly IDataConnectionProvider _connectionProvider;
    
    public DataMigrationToolType(
        ILogger<DataMigrationToolType> logger,
        IDataConnectionProvider connectionProvider) 
        : base(2, "DataMigration", "Data migration and transformation tool")
    {
        _logger = logger;
        _connectionProvider = connectionProvider;
    }

    public override object Create(DataMigrationConfiguration configuration)
    {
        return new DataMigrationTool(_logger, _connectionProvider, configuration);
    }

    public override async Task<IDataMigrationTool> GetTool(string configurationName)
    {
        var config = await _configurationRegistry.GetByName(configurationName);
        return new DataMigrationTool(_logger, _connectionProvider, config);
    }

    public override async Task<IDataMigrationTool> GetTool(int configurationId)
    {
        var config = await _configurationRegistry.GetById(configurationId);
        return new DataMigrationTool(_logger, _connectionProvider, config);
    }
}
```

### Tool Interface Example

```csharp
public interface ICodeGeneratorTool : IFdwTool
{
    Task<GenerationResult> GenerateAsync(GenerationRequest request);
    Task<ValidationResult> ValidateTemplateAsync(string template);
    IEnumerable<string> GetSupportedLanguages();
}

public interface IDataMigrationTool : IFdwTool
{
    Task<MigrationResult> MigrateAsync(MigrationPlan plan);
    Task<ValidationResult> ValidatePlanAsync(MigrationPlan plan);
    Task<MigrationStatus> GetStatusAsync(string migrationId);
}
```

## Contributing

This package is accepting contributions for:
- Additional extension methods
- Performance utilities
- Common helper functions
- Unit tests for all utilities
- Documentation and examples