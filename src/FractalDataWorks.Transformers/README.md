# FractalDataWorks.Transformers

Data transformation pipeline framework built on Smart Delegates. Create composable, queryable, and observable data pipelines that can be modified at runtime and work with any data source.

## 📦 Package Information

- **Package ID**: `FractalDataWorks.Transformers`
- **Target Framework**: .NET Standard 2.1
- **Dependencies**: 
  - `FractalDataWorks` (core)
  - `FractalDataWorks.Services`
- **License**: Apache 2.0

## 🎯 Purpose

This package provides a powerful data transformation framework that enables:

- **Pipeline Processing**: Chain multiple transformation steps together
- **Smart Delegates**: Queryable and composable transformation stages  
- **Runtime Modification**: Add, remove, or reorder pipeline stages dynamically
- **Parallel Execution**: Run independent transformations in parallel
- **Conditional Logic**: Skip or include stages based on data content
- **Observable Pipelines**: Monitor execution, performance, and errors
- **Data Source Agnostic**: Work with any input/output data source

## 🚀 Usage

### Install Package

```bash
dotnet add package FractalDataWorks.Transformers
```

### Basic Pipeline

```csharp
using FractalDataWorks.Transformers;

// Define transformation context
public class CustomerTransformContext : IPipelineContext
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Dictionary<string, object> Items { get; set; } = new();
    public bool ContinueProcessing { get; set; } = true;
    public IPipelineMetadata Metadata { get; set; } = new PipelineMetadata();
    
    // Pipeline-specific data
    public List<RawCustomerData> InputData { get; set; } = new();
    public List<Customer> OutputData { get; set; } = new();
    public TransformationSettings Settings { get; set; } = new();
}

// Define transformation stages
[SmartDelegate("CustomerTransformPipeline", ContextType = typeof(CustomerTransformContext))]
public abstract class CustomerTransformStage(string name, int order) 
    : SmartDelegateBase<CustomerTransformContext>(name, order);

[Stage(Order = 10, Category = "Validation")]
public class ValidateInputStage() : CustomerTransformStage("ValidateInput", 10)
{
    public override async Task<Result<CustomerTransformContext>> Execute(
        CustomerTransformContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.InputData.Any())
        {
            return Result<CustomerTransformContext>.Fail("No input data provided");
        }
        
        var invalidRecords = context.InputData.Where(r => string.IsNullOrWhiteSpace(r.Email)).ToList();
        if (invalidRecords.Any())
        {
            context.Items["InvalidRecords"] = invalidRecords;
            // Remove invalid records or fail based on settings
            if (context.Settings.FailOnInvalidData)
            {
                return Result<CustomerTransformContext>.Fail($"{invalidRecords.Count} invalid records found");
            }
            
            context.InputData.RemoveAll(r => string.IsNullOrWhiteSpace(r.Email));
        }
        
        return Result<CustomerTransformContext>.Ok(context);
    }
}

[Stage(Order = 20, Category = "Transform", CanRunInParallel = true)]
public class NormalizeDataStage() : CustomerTransformStage("NormalizeData", 20)
{
    public override async Task<Result<CustomerTransformContext>> Execute(
        CustomerTransformContext context,
        CancellationToken cancellationToken = default)
    {
        foreach (var raw in context.InputData)
        {
            var customer = new Customer
            {
                Name = NormalizeName(raw.Name),
                Email = NormalizeEmail(raw.Email),
                Region = NormalizeRegion(raw.Region),
                IsActive = true
            };
            
            customer.GenerateId();
            context.OutputData.Add(customer);
        }
        
        return Result<CustomerTransformContext>.Ok(context);
    }
    
    private string NormalizeName(string name) => 
        string.IsNullOrWhiteSpace(name) ? "Unknown" : name.Trim().ToTitleCase();
        
    private string NormalizeEmail(string email) => 
        email?.Trim().ToLowerInvariant() ?? string.Empty;
        
    private string NormalizeRegion(string region) => 
        region?.Trim().ToUpperInvariant() ?? "UNKNOWN";
}

[Stage(Order = 30, Category = "Enrichment")]
public class EnrichDataStage() : CustomerTransformStage("EnrichData", 30)
{
    private readonly IDataConnection _dataConnection;
    
    public EnrichDataStage(IDataConnection dataConnection) : this()
    {
        _dataConnection = dataConnection;
    }
    
    public override async Task<Result<CustomerTransformContext>> Execute(
        CustomerTransformContext context,
        CancellationToken cancellationToken = default)
    {
        // Enrich with existing customer data
        foreach (var customer in context.OutputData)
        {
            var existingResult = await _dataConnection.Single<Customer>(
                c => c.Email == customer.Email, 
                cancellationToken);
                
            if (existingResult.IsSuccess)
            {
                // Merge data from existing customer
                customer.Id = existingResult.Value.Id;
                customer.TotalValue = existingResult.Value.TotalValue;
                customer.CreatedAt = existingResult.Value.CreatedAt;
            }
        }
        
        return Result<CustomerTransformContext>.Ok(context);
    }
}
```

### Pipeline Execution

```csharp
public class CustomerTransformService
{
    private readonly CustomerTransformPipelineProvider _pipeline;
    
    public CustomerTransformService(CustomerTransformPipelineProvider pipeline)
    {
        _pipeline = pipeline;
    }
    
    public async Task<Result<List<Customer>>> TransformCustomers(
        List<RawCustomerData> rawData,
        TransformationSettings settings)
    {
        var context = new CustomerTransformContext
        {
            InputData = rawData,
            Settings = settings
        };
        
        var result = await _pipeline.Execute(context);
        
        return result.Match(
            success: (ctx, msg) => Result<List<Customer>>.Ok(ctx.OutputData),
            failure: error => Result<List<Customer>>.Fail(error)
        );
    }
}
```

## 🏗️ Advanced Features

### Conditional Stages

```csharp
[Stage(Order = 25, Category = "Transform")]
[Conditional(Condition = "context.Settings.EnableGeolocation")]
public class GeolocationStage() : CustomerTransformStage("Geolocation", 25)
{
    public override bool ShouldExecute(CustomerTransformContext context)
    {
        return context.Settings.EnableGeolocation && 
               context.OutputData.Any(c => !string.IsNullOrEmpty(c.Region));
    }
    
    public override async Task<Result<CustomerTransformContext>> Execute(
        CustomerTransformContext context,
        CancellationToken cancellationToken = default)
    {
        // Add geolocation data based on region
        foreach (var customer in context.OutputData)
        {
            var location = await _geolocationService.GetLocationAsync(customer.Region);
            if (location != null)
            {
                customer.Metadata["Latitude"] = location.Latitude;
                customer.Metadata["Longitude"] = location.Longitude;
            }
        }
        
        return Result<CustomerTransformContext>.Ok(context);
    }
}
```

### Parallel Processing

```csharp
[Stage(Order = 40, Category = "Output", CanRunInParallel = true)]
public class SaveToDatabase() : CustomerTransformStage("SaveToDatabase", 40)
{
    public override async Task<Result<CustomerTransformContext>> Execute(
        CustomerTransformContext context,
        CancellationToken cancellationToken = default)
    {
        // Process customers in batches for parallel saving
        var batches = context.OutputData.Chunk(100);
        
        var tasks = batches.Select(async batch =>
        {
            foreach (var customer in batch)
            {
                await _dataConnection.Insert(customer, cancellationToken);
            }
        });
        
        await Task.WhenAll(tasks);
        return Result<CustomerTransformContext>.Ok(context);
    }
}

[Stage(Order = 41, Category = "Output", CanRunInParallel = true)]
public class ExportToFile() : CustomerTransformStage("ExportToFile", 41)
{
    public override async Task<Result<CustomerTransformContext>> Execute(
        CustomerTransformContext context,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(context.OutputData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        var fileName = $"customers_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(fileName, json, cancellationToken);
        
        context.Items["ExportedFile"] = fileName;
        return Result<CustomerTransformContext>.Ok(context);
    }
}
```

### Pipeline Querying and Modification

```csharp
public class DynamicPipelineService
{
    private readonly CustomerTransformPipelineProvider _pipeline;
    
    // Query pipeline stages
    public IEnumerable<CustomerTransformStage> GetValidationStages()
    {
        return _pipeline.Query()
            .Where(stage => stage.Category == "Validation")
            .OrderBy(stage => stage.Order);
    }
    
    public IEnumerable<CustomerTransformStage> GetParallelStages()
    {
        return _pipeline.Query()
            .Where(stage => stage.CanRunInParallel);
    }
    
    // Create modified pipeline
    public async Task<Result<List<Customer>>> TransformWithCustomPipeline(
        List<RawCustomerData> rawData,
        PipelineModifications modifications)
    {
        var customPipeline = _pipeline.CreateDynamic();
        
        // Apply modifications
        foreach (var skip in modifications.SkipStages)
        {
            customPipeline.Remove(skip);
        }
        
        foreach (var (stageName, newStage) in modifications.ReplaceStages)
        {
            customPipeline.Replace(stageName, newStage);
        }
        
        foreach (var (afterStage, newStage) in modifications.AddStages)
        {
            customPipeline.InsertAfter(afterStage, newStage);
        }
        
        // Execute modified pipeline
        var builtPipeline = customPipeline.Build();
        var context = new CustomerTransformContext { InputData = rawData };
        
        var result = await builtPipeline.Execute(context);
        return result.Map(ctx => ctx.OutputData);
    }
}
```

### Error Handling and Retry

```csharp
[Stage(Order = 15, Category = "Validation")]
public class DataQualityStage() : CustomerTransformStage("DataQuality", 15)
{
    public override async Task<Result<CustomerTransformContext>> Execute(
        CustomerTransformContext context,
        CancellationToken cancellationToken = default)
    {
        var qualityIssues = new List<string>();
        
        foreach (var customer in context.InputData)
        {
            if (!IsValidEmail(customer.Email))
                qualityIssues.Add($"Invalid email: {customer.Email}");
                
            if (string.IsNullOrWhiteSpace(customer.Name))
                qualityIssues.Add($"Missing name for email: {customer.Email}");
        }
        
        if (qualityIssues.Any())
        {
            context.Items["QualityIssues"] = qualityIssues;
            
            if (context.Settings.FailOnQualityIssues)
            {
                return Result<CustomerTransformContext>.Fail(
                    $"Data quality issues found: {string.Join(", ", qualityIssues)}");
            }
        }
        
        return Result<CustomerTransformContext>.Ok(context);
    }
    
    // Retry logic for transient failures
    public override async Task<Result<CustomerTransformContext>> BeforeExecute(
        CustomerTransformContext context,
        CancellationToken cancellationToken = default)
    {
        context.Items["RetryCount"] = 0;
        return Result<CustomerTransformContext>.Ok(context);
    }
    
    public override async Task<Result<CustomerTransformContext>> AfterExecute(
        CustomerTransformContext context,
        CancellationToken cancellationToken = default)
    {
        // Log execution metrics
        var retryCount = (int)context.Items.GetValueOrDefault("RetryCount", 0);
        if (retryCount > 0)
        {
            _logger.LogWarning("Stage {StageName} required {RetryCount} retries", Name, retryCount);
        }
        
        return Result<CustomerTransformContext>.Ok(context);
    }
}
```

### Monitoring and Telemetry

```csharp
public class PipelineMonitoringService
{
    private readonly ITelemetryProvider _telemetry;
    
    public async Task<PipelineExecutionReport> GenerateReport(IPipelineMetadata metadata)
    {
        return new PipelineExecutionReport
        {
            TotalExecutionTime = metadata.Elapsed,
            StageExecutions = metadata.ExecutedStages.Select(stage => new StageReport
            {
                StageName = stage.StageName,
                Duration = stage.Duration,
                Success = stage.Success,
                Error = stage.Error,
                Metrics = stage.Metrics
            }).ToList(),
            OverallMetrics = metadata.Metrics
        };
    }
    
    public async Task MonitorPipelineExecution(CustomerTransformContext context)
    {
        var report = await GenerateReport(context.Metadata);
        
        // Alert on long-running stages
        var slowStages = report.StageExecutions
            .Where(s => s.Duration > TimeSpan.FromMinutes(5))
            .ToList();
            
        if (slowStages.Any())
        {
            await _alertService.SendSlowStageAlert(slowStages);
        }
        
        // Alert on failures
        var failedStages = report.StageExecutions.Where(s => !s.Success).ToList();
        if (failedStages.Any())
        {
            await _alertService.SendFailureAlert(failedStages);
        }
    }
}
```

## 🧪 Testing

### Unit Testing Stages

```csharp
[Test]
public async Task ValidateInputStage_WithValidData_ShouldSucceed()
{
    var stage = new ValidateInputStage();
    var context = new CustomerTransformContext
    {
        InputData = new List<RawCustomerData>
        {
            new() { Name = "John Doe", Email = "john@example.com" },
            new() { Name = "Jane Smith", Email = "jane@example.com" }
        },
        Settings = new TransformationSettings { FailOnInvalidData = true }
    };
    
    var result = await stage.Execute(context);
    
    result.IsSuccess.ShouldBeTrue();
    context.InputData.Count.ShouldBe(2);
}

[Test]
public async Task ValidateInputStage_WithInvalidData_ShouldFail()
{
    var stage = new ValidateInputStage();
    var context = new CustomerTransformContext
    {
        InputData = new List<RawCustomerData>
        {
            new() { Name = "John Doe", Email = "" }, // Invalid
            new() { Name = "Jane Smith", Email = "jane@example.com" }
        },
        Settings = new TransformationSettings { FailOnInvalidData = true }
    };
    
    var result = await stage.Execute(context);
    
    result.IsFailure.ShouldBeTrue();
    result.Error.Message.ShouldContain("invalid records found");
}
```

### Integration Testing Pipelines

```csharp
[Test]
public async Task CustomerTransformPipeline_EndToEnd_ShouldWork()
{
    var rawData = new List<RawCustomerData>
    {
        new() { Name = "john doe", Email = "JOHN@EXAMPLE.COM", Region = "north" },
        new() { Name = "jane smith", Email = "jane@example.com", Region = "south" }
    };
    
    var service = new CustomerTransformService(_pipeline);
    var result = await service.TransformCustomers(rawData, new TransformationSettings());
    
    result.IsSuccess.ShouldBeTrue();
    result.Value.Count.ShouldBe(2);
    
    var john = result.Value.First(c => c.Email == "john@example.com");
    john.Name.ShouldBe("John Doe"); // Normalized
    john.Region.ShouldBe("NORTH"); // Normalized
}
```

## 📋 Configuration

### appsettings.json

```json
{
  "Transformers": {
    "CustomerTransformPipeline": {
      "Enabled": true,
      "MaxParallelStages": 4,
      "DefaultTimeout": 300,
      "EnableTelemetry": true,
      "DisabledStages": ["GeolocationStage"],
      "StageTimeouts": {
        "ValidateInput": 30,
        "EnrichData": 120
      }
    }
  }
}
```

### Service Registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register transformer pipelines
    services.AddFractalDataWorksTransformers(Configuration);
    
    // Register specific pipeline
    services.AddTransformPipeline<CustomerTransformPipelineProvider>();
    
    // Register monitoring
    services.AddSingleton<PipelineMonitoringService>();
    services.AddSingleton<ITelemetryProvider, ApplicationInsightsTelemetryProvider>();
}
```

## 🔄 Version History

- **0.1.0-preview**: Initial release with Smart Delegate pipelines
- **Future**: Visual pipeline designer, workflow integration, real-time streaming

## 📄 License

Licensed under the Apache License 2.0. See [LICENSE](../../LICENSE) for details.