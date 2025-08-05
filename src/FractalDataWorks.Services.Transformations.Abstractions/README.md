# FractalDataWorks.Services.Transformations.Abstractions

The **FractalDataWorks.Services.Transformations.Abstractions** package provides comprehensive data transformation capabilities for the FractalDataWorks Framework. This package defines interfaces and base classes for integrating with various transformation providers including TPL-based transformations, Apache Spark, Azure Data Factory, and custom transformation engines.

## Overview

This abstraction layer provides:

- **Multi-Provider Support** - TPL transformations, Spark, Databricks, custom engines
- **Data Format Flexibility** - JSON, XML, CSV, binary, streaming, and custom formats
- **Transformation Categories** - Mapping, filtering, aggregation, validation, formatting
- **Pipeline Composition** - Chain multiple transformations into complex data pipelines
- **Performance Optimization** - Parallel processing, streaming, and batch operations
- **Engine Abstraction** - Stateful transformation engines for complex multi-step processes
- **Context Awareness** - Security context, pipeline tracking, and correlation support

## Quick Start

### Using an Existing Transformation Provider

```csharp
using FractalDataWorks.Services.Transformations.Abstractions;
using FractalDataWorks.Framework.Abstractions;

// Define a simple transformation request
public sealed class JsonToCsvTransformationRequest : ITransformationRequest
{
    public string RequestId { get; } = Guid.NewGuid().ToString();
    public object? InputData { get; }
    public string InputType => "JSON";
    public string OutputType => "CSV";
    public string? TransformationCategory => "Formatting";
    public Type ExpectedResultType => typeof(string);
    public TimeSpan? Timeout => TimeSpan.FromMinutes(5);
    
    public IReadOnlyDictionary<string, object> Configuration { get; }
    public IReadOnlyDictionary<string, object> Options { get; }
    public ITransformationContext? Context { get; }
    
    public JsonToCsvTransformationRequest(object? jsonData, Dictionary<string, string>? fieldMappings = null)
    {
        InputData = jsonData;
        
        var config = new Dictionary<string, object>();
        if (fieldMappings != null)
            config["FieldMappings"] = fieldMappings;
        config["IncludeHeaders"] = true;
        config["Delimiter"] = ",";
        
        Configuration = config;
        
        Options = new Dictionary<string, object>
        {
            ["EnableParallel"] = true,
            ["ChunkSize"] = 1000
        };
        
        Context = new TransformationContext
        {
            Identity = "system",
            CorrelationId = Guid.NewGuid().ToString(),
            Properties = new Dictionary<string, object>
            {
                ["Source"] = "DataProcessor",
                ["CreatedAt"] = DateTime.UtcNow
            }
        };
    }
    
    public ITransformationRequest WithInputData(object? newInputData, string? newInputType = null)
    {
        return new JsonToCsvTransformationRequest(newInputData);
    }
    
    public ITransformationRequest WithOutputType(string newOutputType, Type? newExpectedResultType = null)
    {
        // Implementation would create a new request with different output type
        return this;
    }
    
    public ITransformationRequest WithConfiguration(IReadOnlyDictionary<string, object> newConfiguration)
    {
        // Implementation would create a new request with different configuration
        return this;
    }
    
    public ITransformationRequest WithOptions(IReadOnlyDictionary<string, object> newOptions)
    {
        // Implementation would create a new request with different options
        return this;
    }
}

// Using the transformation service
public async Task<IFdwResult<string>> ConvertJsonToCsvAsync(object jsonData)
{
    // Find a transformation provider that supports JSON to CSV conversion
    var provider = TransformationProviders.All
        .Where(p => p.SupportedInputTypes.Contains("JSON"))
        .Where(p => p.SupportedOutputTypes.Contains("CSV"))
        .FirstOrDefault();
        
    if (provider == null)
        return FdwResult<string>.Failure("No JSON to CSV transformation provider found");
    
    // Validate the transformation
    var validationResult = provider.ValidateTransformation("JSON", "CSV", "Formatting");
    if (validationResult.IsFailure)
        return FdwResult<string>.Failure($"Transformation validation failed: {validationResult.ErrorMessage}");
    
    // Execute the transformation
    var request = new JsonToCsvTransformationRequest(jsonData);
    var result = await provider.Transform<string>(request);
    
    if (result.IsSuccess)
    {
        Console.WriteLine("Transformation completed successfully");
        return result;
    }
    
    return FdwResult<string>.Failure("Transformation failed", result.Exception);
}
```

### Creating a Custom Transformation Provider

```csharp
// Define configuration for your transformation provider
public sealed class TplTransformationConfiguration : FdwConfigurationBase
{
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
    public bool EnableBatching { get; set; } = true;
    public int BatchSize { get; set; } = 1000;
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(10);
    
    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        
        if (MaxDegreeOfParallelism <= 0)
            errors.Add("Max degree of parallelism must be positive");
            
        if (BatchSize <= 0)
            errors.Add("Batch size must be positive");
            
        if (DefaultTimeout <= TimeSpan.Zero)
            errors.Add("Default timeout must be positive");
            
        return errors;
    }
}

// Implement the transformation provider
public sealed class TplTransformationProvider : ITransformationProvider
{
    private readonly TplTransformationConfiguration _configuration;
    private readonly ILogger<TplTransformationProvider> _logger;
    
    public string ServiceId { get; } = "tpl-transformation-provider";
    public string ServiceName => "TPL Transformation Provider";
    public bool IsAvailable => true;
    
    public IReadOnlyList<string> SupportedInputTypes { get; } = new[]
    {
        "JSON", "XML", "CSV", "Object", "IEnumerable", "Stream", "DataTable"
    };
    
    public IReadOnlyList<string> SupportedOutputTypes { get; } = new[]
    {
        "JSON", "XML", "CSV", "Object", "IEnumerable", "Stream", "DataTable"
    };
    
    public IReadOnlyList<string> TransformationCategories { get; } = new[]
    {
        "Mapping", "Filtering", "Aggregation", "Validation", "Formatting", "Sorting", "Grouping"
    };
    
    public int Priority => 100;
    
    public TplTransformationProvider(
        TplTransformationConfiguration configuration,
        ILogger<TplTransformationProvider> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public IFdwResult ValidateTransformation(string inputType, string outputType, string? transformationCategory = null)
    {
        ArgumentNullException.ThrowIfNull(inputType);
        ArgumentNullException.ThrowIfNull(outputType);
        
        if (!SupportedInputTypes.Contains(inputType))
            return FdwResult.Failure($"Input type '{inputType}' is not supported");
            
        if (!SupportedOutputTypes.Contains(outputType))
            return FdwResult.Failure($"Output type '{outputType}' is not supported");
            
        if (!string.IsNullOrWhiteSpace(transformationCategory) &&
            !TransformationCategories.Contains(transformationCategory))
            return FdwResult.Failure($"Transformation category '{transformationCategory}' is not supported");
            
        return FdwResult.Success();
    }
    
    public async Task<IFdwResult<TOutput>> Transform<TOutput>(ITransformationRequest transformationRequest)
    {
        ArgumentNullException.ThrowIfNull(transformationRequest);
        
        try
        {
            var validationResult = ValidateTransformation(transformationRequest.InputType, 
                transformationRequest.OutputType, transformationRequest.TransformationCategory);
            if (validationResult.IsFailure)
                return FdwResult<TOutput>.Failure(validationResult.ErrorMessage);
            
            _logger.LogInformation("Starting transformation: {InputType} -> {OutputType} ({Category})",
                transformationRequest.InputType, transformationRequest.OutputType, 
                transformationRequest.TransformationCategory ?? "General");
            
            var cancellationToken = new CancellationTokenSource(
                transformationRequest.Timeout ?? _configuration.DefaultTimeout).Token;
            
            var result = transformationRequest.TransformationCategory switch
            {
                "Mapping" => await ExecuteMapping<TOutput>(transformationRequest, cancellationToken),
                "Filtering" => await ExecuteFiltering<TOutput>(transformationRequest, cancellationToken),
                "Aggregation" => await ExecuteAggregation<TOutput>(transformationRequest, cancellationToken),
                "Validation" => await ExecuteValidation<TOutput>(transformationRequest, cancellationToken),
                "Formatting" => await ExecuteFormatting<TOutput>(transformationRequest, cancellationToken),
                "Sorting" => await ExecuteSorting<TOutput>(transformationRequest, cancellationToken),
                "Grouping" => await ExecuteGrouping<TOutput>(transformationRequest, cancellationToken),
                _ => await ExecuteGenericTransformation<TOutput>(transformationRequest, cancellationToken)
            };
            
            _logger.LogInformation("Transformation completed successfully");
            return result;
        }
        catch (OperationCanceledException)
        {
            return FdwResult<TOutput>.Failure("Transformation timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transformation failed: {InputType} -> {OutputType}",
                transformationRequest.InputType, transformationRequest.OutputType);
            return FdwResult<TOutput>.Failure($"Transformation failed: {ex.Message}", ex);
        }
    }
    
    public async Task<IFdwResult<object?>> Transform(ITransformationRequest transformationRequest)
    {
        // For non-generic transformation, we'll determine the output type at runtime
        var outputType = GetOutputTypeFromRequest(transformationRequest);
        
        // Use reflection to call the generic method
        var method = GetType().GetMethod(nameof(Transform), new[] { typeof(ITransformationRequest) });
        var genericMethod = method!.MakeGenericMethod(outputType);
        
        var task = (Task)genericMethod.Invoke(this, new object[] { transformationRequest })!;
        await task;
        
        var result = task.GetType().GetProperty("Result")?.GetValue(task);
        if (result is IFdwResult fdwResult)
        {
            return fdwResult.IsSuccess 
                ? FdwResult<object?>.Success(fdwResult.GetType().GetProperty("Value")?.GetValue(fdwResult))
                : FdwResult<object?>.Failure(fdwResult.ErrorMessage!, fdwResult.Exception);
        }
        
        return FdwResult<object?>.Failure("Unable to process transformation result");
    }
    
    private async Task<IFdwResult<TOutput>> ExecuteMapping<TOutput>(ITransformationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Handle different input/output type combinations for mapping
            return (request.InputType, request.OutputType) switch
            {
                ("JSON", "Object") => await MapJsonToObject<TOutput>(request, cancellationToken),
                ("Object", "JSON") => await MapObjectToJson<TOutput>(request, cancellationToken),
                ("CSV", "Object") => await MapCsvToObject<TOutput>(request, cancellationToken),
                ("Object", "CSV") => await MapObjectToCsv<TOutput>(request, cancellationToken),
                ("IEnumerable", "IEnumerable") => await MapEnumerableToEnumerable<TOutput>(request, cancellationToken),
                _ => FdwResult<TOutput>.Failure($"Mapping from {request.InputType} to {request.OutputType} is not implemented")
            };
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"Mapping transformation failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<TOutput>> MapJsonToObject<TOutput>(ITransformationRequest request, CancellationToken cancellationToken)
    {
        if (request.InputData is not string jsonString)
            return FdwResult<TOutput>.Failure("Input data must be a JSON string");
        
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<TOutput>(jsonString, options);
            return FdwResult<TOutput>.Success(result);
        }
        catch (JsonException ex)
        {
            return FdwResult<TOutput>.Failure($"JSON deserialization failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<TOutput>> MapObjectToJson<TOutput>(ITransformationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(request.InputData, options);
            
            if (typeof(TOutput) == typeof(string))
                return FdwResult<TOutput>.Success((TOutput)(object)json);
                
            return FdwResult<TOutput>.Failure($"Cannot convert JSON string to {typeof(TOutput).Name}");
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"JSON serialization failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<TOutput>> MapEnumerableToEnumerable<TOutput>(ITransformationRequest request, CancellationToken cancellationToken)
    {
        if (request.InputData is not IEnumerable inputEnumerable)
            return FdwResult<TOutput>.Failure("Input data must be enumerable");
        
        try
        {
            // Get field mappings from configuration
            var fieldMappings = request.Configuration.TryGetValue("FieldMappings", out var mappings) 
                ? mappings as Dictionary<string, string> ?? new Dictionary<string, string>()
                : new Dictionary<string, string>();
            
            var parallelOptions = new ParallelQuery<object>(inputEnumerable.Cast<object>());
            
            if (_configuration.EnableBatching)
            {
                parallelOptions = parallelOptions.WithDegreeOfParallelism(_configuration.MaxDegreeOfParallelism);
            }
            
            var transformedItems = parallelOptions
                .Select(item => TransformItem(item, fieldMappings))
                .Where(item => item != null)
                .ToList();
            
            if (typeof(TOutput).IsAssignableFrom(transformedItems.GetType()))
                return FdwResult<TOutput>.Success((TOutput)(object)transformedItems);
                
            // Try to convert to the expected output type
            var convertedResult = ConvertToOutputType<TOutput>(transformedItems);
            return FdwResult<TOutput>.Success(convertedResult);
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"Enumerable mapping failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<TOutput>> ExecuteFiltering<TOutput>(ITransformationRequest request, CancellationToken cancellationToken)
    {
        if (request.InputData is not IEnumerable inputEnumerable)
            return FdwResult<TOutput>.Failure("Filtering requires enumerable input data");
        
        try
        {
            // Get filter criteria from configuration
            var filterExpression = request.Configuration.TryGetValue("FilterExpression", out var expr) 
                ? expr?.ToString() : null;
                
            if (string.IsNullOrWhiteSpace(filterExpression))
                return FdwResult<TOutput>.Failure("Filter expression is required for filtering transformation");
            
            var items = inputEnumerable.Cast<object>().ToList();
            var filteredItems = new List<object>();
            
            if (_configuration.EnableBatching)
            {
                var batches = items.Chunk(_configuration.BatchSize);
                var tasks = batches.Select(batch => Task.Run(() =>
                    batch.Where(item => EvaluateFilterExpression(item, filterExpression)).ToList(), cancellationToken));
                
                var batchResults = await Task.WhenAll(tasks);
                filteredItems = batchResults.SelectMany(batch => batch).ToList();
            }
            else
            {
                filteredItems = items.Where(item => EvaluateFilterExpression(item, filterExpression)).ToList();
            }
            
            var convertedResult = ConvertToOutputType<TOutput>(filteredItems);
            return FdwResult<TOutput>.Success(convertedResult);
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"Filtering transformation failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<TOutput>> ExecuteAggregation<TOutput>(ITransformationRequest request, CancellationToken cancellationToken)
    {
        if (request.InputData is not IEnumerable inputEnumerable)
            return FdwResult<TOutput>.Failure("Aggregation requires enumerable input data");
        
        try
        {
            var aggregationType = request.Configuration.TryGetValue("AggregationType", out var aggType) 
                ? aggType?.ToString() : "Count";
                
            var groupByField = request.Configuration.TryGetValue("GroupBy", out var groupBy) 
                ? groupBy?.ToString() : null;
            
            var items = inputEnumerable.Cast<object>().ToList();
            object result;
            
            if (!string.IsNullOrWhiteSpace(groupByField))
            {
                // Group by aggregation
                var groups = items.GroupBy(item => GetFieldValue(item, groupByField))
                    .ToDictionary(g => g.Key, g => PerformAggregation(g, aggregationType));
                result = groups;
            }
            else
            {
                // Simple aggregation
                result = PerformAggregation(items, aggregationType);
            }
            
            var convertedResult = ConvertToOutputType<TOutput>(result);
            return FdwResult<TOutput>.Success(convertedResult);
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"Aggregation transformation failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<TOutput>> ExecuteFormatting<TOutput>(ITransformationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (request.InputType, request.OutputType) switch
            {
                ("JSON", "CSV") => await FormatJsonToCsv<TOutput>(request, cancellationToken),
                ("CSV", "JSON") => await FormatCsvToJson<TOutput>(request, cancellationToken),
                ("Object", "XML") => await FormatObjectToXml<TOutput>(request, cancellationToken),
                ("XML", "Object") => await FormatXmlToObject<TOutput>(request, cancellationToken),
                _ => FdwResult<TOutput>.Failure($"Formatting from {request.InputType} to {request.OutputType} is not implemented")
            };
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"Formatting transformation failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<TOutput>> FormatJsonToCsv<TOutput>(ITransformationRequest request, CancellationToken cancellationToken)
    {
        if (request.InputData is not string jsonString)
            return FdwResult<TOutput>.Failure("Input data must be a JSON string");
        
        try
        {
            var jsonDocument = JsonDocument.Parse(jsonString);
            var csvBuilder = new StringBuilder();
            
            // Get configuration options
            var includeHeaders = request.Configuration.TryGetValue("IncludeHeaders", out var headers) && 
                                 Convert.ToBoolean(headers);
            var delimiter = request.Configuration.TryGetValue("Delimiter", out var delim) 
                ? delim?.ToString() ?? "," : ",";
            var fieldMappings = request.Configuration.TryGetValue("FieldMappings", out var mappings) 
                ? mappings as Dictionary<string, string> : null;
            
            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
            {
                var items = jsonDocument.RootElement.EnumerateArray().ToList();
                if (items.Count > 0)
                {
                    var firstItem = items[0];
                    var fieldNames = GetJsonFieldNames(firstItem, fieldMappings);
                    
                    // Write headers
                    if (includeHeaders)
                    {
                        csvBuilder.AppendLine(string.Join(delimiter, fieldNames));
                    }
                    
                    // Write data rows
                    foreach (var item in items)
                    {
                        var values = fieldNames.Select(field => GetJsonFieldValue(item, field)).ToArray();
                        csvBuilder.AppendLine(string.Join(delimiter, values.Select(EscapeCsvValue)));
                    }
                }
            }
            else
            {
                // Single object
                var fieldNames = GetJsonFieldNames(jsonDocument.RootElement, fieldMappings);
                
                if (includeHeaders)
                {
                    csvBuilder.AppendLine(string.Join(delimiter, fieldNames));
                }
                
                var values = fieldNames.Select(field => GetJsonFieldValue(jsonDocument.RootElement, field)).ToArray();
                csvBuilder.AppendLine(string.Join(delimiter, values.Select(EscapeCsvValue)));
            }
            
            var csvResult = csvBuilder.ToString();
            
            if (typeof(TOutput) == typeof(string))
                return FdwResult<TOutput>.Success((TOutput)(object)csvResult);
                
            return FdwResult<TOutput>.Failure($"Cannot convert CSV string to {typeof(TOutput).Name}");
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"JSON to CSV formatting failed: {ex.Message}", ex);
        }
    }
    
    public async Task<IFdwResult<ITransformationEngine>> CreateEngineAsync(ITransformationEngineConfiguration configuration)
    {
        try
        {
            var engine = new TplTransformationEngine(_configuration, _logger, configuration);
            var initResult = await engine.InitializeAsync();
            
            return initResult.IsSuccess 
                ? FdwResult<ITransformationEngine>.Success(engine)
                : FdwResult<ITransformationEngine>.Failure(initResult.ErrorMessage, initResult.Exception);
        }
        catch (Exception ex)
        {
            return FdwResult<ITransformationEngine>.Failure("Failed to create transformation engine", ex);
        }
    }
    
    public async Task<IFdwResult<ITransformationMetrics>> GetTransformationMetricsAsync()
    {
        var metrics = new TplTransformationMetrics
        {
            TotalTransformations = 1000, // Would track actual metrics
            SuccessfulTransformations = 980,
            FailedTransformations = 20,
            AverageExecutionTime = TimeSpan.FromMilliseconds(250),
            TotalDataProcessed = 1024 * 1024 * 100, // 100MB
            MaxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism,
            BatchProcessingEnabled = _configuration.EnableBatching,
            SupportedInputTypes = SupportedInputTypes.Count,
            SupportedOutputTypes = SupportedOutputTypes.Count
        };
        
        return FdwResult<ITransformationMetrics>.Success(metrics);
    }
    
    public async Task<IFdwResult> HealthCheckAsync()
    {
        try
        {
            // Perform a simple transformation test
            var testRequest = new SimpleTransformationRequest("test", "Object", "JSON");
            var testResult = await Transform<string>(testRequest);
            
            return testResult.IsSuccess 
                ? FdwResult.Success()
                : FdwResult.Failure("Health check transformation failed");
        }
        catch (Exception ex)
        {
            return FdwResult.Failure("Health check failed", ex);
        }
    }
    
    // Helper methods
    private object? TransformItem(object item, Dictionary<string, string> fieldMappings)
    {
        if (fieldMappings.Count == 0)
            return item;
            
        // Apply field mappings to transform the item
        var transformedItem = new Dictionary<string, object?>();
        
        foreach (var (sourceField, targetField) in fieldMappings)
        {
            var value = GetFieldValue(item, sourceField);
            transformedItem[targetField] = value;
        }
        
        return transformedItem;
    }
    
    private bool EvaluateFilterExpression(object item, string filterExpression)
    {
        // Simplified filter expression evaluation
        // In a real implementation, you'd use a proper expression evaluator
        try
        {
            if (filterExpression.Contains("!="))
            {
                var parts = filterExpression.Split("!=", 2);
                var fieldValue = GetFieldValue(item, parts[0].Trim())?.ToString();
                var expectedValue = parts[1].Trim().Trim('"');
                return fieldValue != expectedValue;
            }
            else if (filterExpression.Contains("=="))
            {
                var parts = filterExpression.Split("==", 2);
                var fieldValue = GetFieldValue(item, parts[0].Trim())?.ToString();
                var expectedValue = parts[1].Trim().Trim('"');
                return fieldValue == expectedValue;
            }
            else if (filterExpression.Contains(">"))
            {
                var parts = filterExpression.Split(">", 2);
                var fieldValue = GetFieldValue(item, parts[0].Trim());
                var expectedValue = Convert.ToDouble(parts[1].Trim());
                return fieldValue != null && Convert.ToDouble(fieldValue) > expectedValue;
            }
            
            return true; // Default to include if expression is not recognized
        }
        catch
        {
            return false; // Exclude if evaluation fails
        }
    }
    
    private object? GetFieldValue(object item, string fieldName)
    {
        if (item == null) return null;
        
        var type = item.GetType();
        var property = type.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        
        if (property != null)
            return property.GetValue(item);
            
        // Try as dictionary
        if (item is IDictionary<string, object> dict)
            return dict.TryGetValue(fieldName, out var value) ? value : null;
            
        return null;
    }
    
    private object PerformAggregation(IEnumerable<object> items, string aggregationType)
    {
        return aggregationType.ToLower() switch
        {
            "count" => items.Count(),
            "sum" => items.Select(i => Convert.ToDouble(i)).Sum(),
            "avg" or "average" => items.Select(i => Convert.ToDouble(i)).Average(),
            "min" => items.Select(i => Convert.ToDouble(i)).Min(),
            "max" => items.Select(i => Convert.ToDouble(i)).Max(),
            _ => items.Count()
        };
    }
    
    private TOutput ConvertToOutputType<TOutput>(object result)
    {
        if (result is TOutput directResult)
            return directResult;
            
        try
        {
            return (TOutput)Convert.ChangeType(result, typeof(TOutput));
        }
        catch
        {
            // If direct conversion fails, try JSON serialization/deserialization
            var json = JsonSerializer.Serialize(result);
            return JsonSerializer.Deserialize<TOutput>(json)!;
        }
    }
    
    private Type GetOutputTypeFromRequest(ITransformationRequest request)
    {
        if (request.ExpectedResultType != null)
            return request.ExpectedResultType;
            
        return request.OutputType switch
        {
            "JSON" => typeof(string),
            "XML" => typeof(string),
            "CSV" => typeof(string),
            "Object" => typeof(object),
            "IEnumerable" => typeof(IEnumerable<object>),
            _ => typeof(object)
        };
    }
    
    private List<string> GetJsonFieldNames(JsonElement element, Dictionary<string, string>? fieldMappings)
    {
        var fieldNames = new List<string>();
        
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var fieldName = fieldMappings?.TryGetValue(property.Name, out var mappedName) == true 
                    ? mappedName : property.Name;
                fieldNames.Add(fieldName);
            }
        }
        
        return fieldNames;
    }
    
    private string GetJsonFieldValue(JsonElement element, string fieldName)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(fieldName, out var property))
        {
            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString() ?? "",
                JsonValueKind.Number => property.GetDecimal().ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => "",
                _ => property.ToString()
            };
        }
        
        return "";
    }
    
    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
            
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        
        return value;
    }
    
    // Additional helper method implementations...
}

// Supporting classes
public class TransformationContext : ITransformationContext
{
    public string? Identity { get; set; }
    public string? CorrelationId { get; set; }
    public string? PipelineStage { get; set; }
    public IReadOnlyDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

public class SimpleTransformationRequest : ITransformationRequest
{
    public string RequestId { get; } = Guid.NewGuid().ToString();
    public object? InputData { get; }
    public string InputType { get; }
    public string OutputType { get; }
    public string? TransformationCategory { get; }
    public Type ExpectedResultType { get; }
    public TimeSpan? Timeout { get; }
    public IReadOnlyDictionary<string, object> Configuration { get; }
    public IReadOnlyDictionary<string, object> Options { get; }
    public ITransformationContext? Context { get; }
    
    public SimpleTransformationRequest(object? inputData, string inputType, string outputType, 
        string? category = null, Type? expectedResultType = null)
    {
        InputData = inputData;
        InputType = inputType;
        OutputType = outputType;
        TransformationCategory = category;
        ExpectedResultType = expectedResultType ?? typeof(object);
        Timeout = TimeSpan.FromMinutes(1);
        Configuration = new Dictionary<string, object>();
        Options = new Dictionary<string, object>();
        Context = null;
    }
    
    public ITransformationRequest WithInputData(object? newInputData, string? newInputType = null)
    {
        return new SimpleTransformationRequest(newInputData, newInputType ?? InputType, OutputType, TransformationCategory, ExpectedResultType);
    }
    
    public ITransformationRequest WithOutputType(string newOutputType, Type? newExpectedResultType = null)
    {
        return new SimpleTransformationRequest(InputData, InputType, newOutputType, TransformationCategory, newExpectedResultType ?? ExpectedResultType);
    }
    
    public ITransformationRequest WithConfiguration(IReadOnlyDictionary<string, object> newConfiguration)
    {
        var request = new SimpleTransformationRequest(InputData, InputType, OutputType, TransformationCategory, ExpectedResultType);
        typeof(SimpleTransformationRequest).GetProperty(nameof(Configuration))?.SetValue(request, newConfiguration);
        return request;
    }
    
    public ITransformationRequest WithOptions(IReadOnlyDictionary<string, object> newOptions)
    {
        var request = new SimpleTransformationRequest(InputData, InputType, OutputType, TransformationCategory, ExpectedResultType);
        typeof(SimpleTransformationRequest).GetProperty(nameof(Options))?.SetValue(request, newOptions);
        return request;
    }
}
```

## Implementation Examples

### Apache Spark Transformation Provider

```csharp
public sealed class SparkTransformationProvider : ITransformationProvider
{
    public static readonly SparkTransformationProvider Instance = new();
    
    private SparkTransformationProvider()
    {
        SupportedInputTypes = new[] { "DataFrame", "RDD", "Dataset", "Parquet", "JSON", "CSV" };
        SupportedOutputTypes = new[] { "DataFrame", "RDD", "Dataset", "Parquet", "JSON", "CSV" };
        TransformationCategories = new[] { "Mapping", "Filtering", "Aggregation", "Join", "Window", "ML" };
        Priority = 200;
    }
    
    public string ServiceId => "spark-transformation-provider";
    public string ServiceName => "Apache Spark Transformation Provider";
    public bool IsAvailable => CheckSparkAvailability();
    
    public IReadOnlyList<string> SupportedInputTypes { get; }
    public IReadOnlyList<string> SupportedOutputTypes { get; }
    public IReadOnlyList<string> TransformationCategories { get; }
    public int Priority { get; }
    
    public async Task<IFdwResult<TOutput>> Transform<TOutput>(ITransformationRequest transformationRequest)
    {
        try
        {
            // Initialize Spark session
            var spark = SparkSession.Builder()
                .AppName("FractalDataWorks-Transformation")
                .GetOrCreate();
            
            // Convert input data to Spark DataFrame
            var inputDf = await ConvertToDataFrame(spark, transformationRequest.InputData, transformationRequest.InputType);
            
            // Apply transformations based on category
            DataFrame resultDf = transformationRequest.TransformationCategory switch
            {
                "Mapping" => ApplyMapping(inputDf, transformationRequest.Configuration),
                "Filtering" => ApplyFiltering(inputDf, transformationRequest.Configuration),
                "Aggregation" => ApplyAggregation(inputDf, transformationRequest.Configuration),
                "Join" => await ApplyJoin(spark, inputDf, transformationRequest.Configuration),
                _ => ApplyGenericTransformation(inputDf, transformationRequest.Configuration)
            };
            
            // Convert result to desired output format
            var result = await ConvertFromDataFrame<TOutput>(resultDf, transformationRequest.OutputType);
            
            return FdwResult<TOutput>.Success(result);
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"Spark transformation failed: {ex.Message}", ex);
        }
    }
    
    private DataFrame ApplyMapping(DataFrame df, IReadOnlyDictionary<string, object> configuration)
    {
        if (configuration.TryGetValue("SelectColumns", out var columns) && columns is string[] selectCols)
        {
            df = df.Select(selectCols);
        }
        
        if (configuration.TryGetValue("ColumnMappings", out var mappings) && mappings is Dictionary<string, string> colMappings)
        {
            foreach (var (oldName, newName) in colMappings)
            {
                df = df.WithColumnRenamed(oldName, newName);
            }
        }
        
        return df;
    }
    
    private DataFrame ApplyFiltering(DataFrame df, IReadOnlyDictionary<string, object> configuration)
    {
        if (configuration.TryGetValue("FilterCondition", out var condition) && condition is string filterCondition)
        {
            df = df.Filter(filterCondition);
        }
        
        return df;
    }
    
    private DataFrame ApplyAggregation(DataFrame df, IReadOnlyDictionary<string, object> configuration)
    {
        if (configuration.TryGetValue("GroupByColumns", out var groupCols) && groupCols is string[] groupByColumns)
        {
            var groupedDf = df.GroupBy(groupByColumns);
            
            if (configuration.TryGetValue("AggregationFunctions", out var aggFuncs) && aggFuncs is Dictionary<string, string> aggregations)
            {
                var aggExpressions = aggregations.Select(kvp => 
                    kvp.Value.ToLower() switch
                    {
                        "sum" => Functions.Sum(kvp.Key),
                        "avg" => Functions.Avg(kvp.Key),
                        "count" => Functions.Count(kvp.Key),
                        "max" => Functions.Max(kvp.Key),
                        "min" => Functions.Min(kvp.Key),
                        _ => Functions.Count(kvp.Key)
                    }).ToArray();
                
                df = groupedDf.Agg(aggExpressions[0], aggExpressions.Skip(1).ToArray());
            }
            else
            {
                df = groupedDf.Count();
            }
        }
        
        return df;
    }
    
    // Additional Spark transformation methods...
}
```

### Azure Data Factory Transformation Provider

```csharp
public sealed class AzureDataFactoryProvider : ITransformationProvider
{
    public static readonly AzureDataFactoryProvider Instance = new();
    
    private AzureDataFactoryProvider()
    {
        SupportedInputTypes = new[] { "AzureBlob", "AzureSQL", "CosmosDB", "JSON", "CSV", "Parquet" };
        SupportedOutputTypes = new[] { "AzureBlob", "AzureSQL", "CosmosDB", "JSON", "CSV", "Parquet" };
        TransformationCategories = new[] { "ETL", "DataFlow", "Pipeline", "Copy", "Mapping" };
        Priority = 150;
    }
    
    public string ServiceId => "adf-transformation-provider";
    public string ServiceName => "Azure Data Factory Transformation Provider";
    public bool IsAvailable => CheckAzureConnectivity();
    
    public IReadOnlyList<string> SupportedInputTypes { get; }
    public IReadOnlyList<string> SupportedOutputTypes { get; }
    public IReadOnlyList<string> TransformationCategories { get; }
    public int Priority { get; }
    
    public async Task<IFdwResult<TOutput>> Transform<TOutput>(ITransformationRequest transformationRequest)
    {
        try
        {
            // Initialize Azure Data Factory client
            var adfClient = new DataFactoryManagementClient(GetAzureCredentials())
            {
                SubscriptionId = GetSubscriptionId()
            };
            
            // Create pipeline for the transformation
            var pipelineResult = await CreateTransformationPipeline(adfClient, transformationRequest);
            if (pipelineResult.IsFailure)
                return FdwResult<TOutput>.Failure(pipelineResult.ErrorMessage);
            
            // Execute the pipeline
            var executionResult = await ExecutePipeline(adfClient, pipelineResult.Value);
            if (executionResult.IsFailure)
                return FdwResult<TOutput>.Failure(executionResult.ErrorMessage);
            
            // Get the result
            var result = await GetPipelineResult<TOutput>(adfClient, executionResult.Value);
            return result;
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"Azure Data Factory transformation failed: {ex.Message}", ex);
        }
    }
    
    // Additional ADF implementation methods...
}
```

### Stream Processing Transformation Provider

```csharp
public sealed class StreamTransformationProvider : ITransformationProvider
{
    private readonly ILogger<StreamTransformationProvider> _logger;
    
    public StreamTransformationProvider(ILogger<StreamTransformationProvider> logger)
    {
        _logger = logger;
        SupportedInputTypes = new[] { "Stream", "IEnumerable", "IAsyncEnumerable", "Channel" };
        SupportedOutputTypes = new[] { "Stream", "IEnumerable", "IAsyncEnumerable", "Channel" };
        TransformationCategories = new[] { "Streaming", "RealTime", "Windowing", "Buffering" };
        Priority = 120;
    }
    
    public string ServiceId => "stream-transformation-provider";
    public string ServiceName => "Stream Processing Transformation Provider";
    public bool IsAvailable => true;
    
    public IReadOnlyList<string> SupportedInputTypes { get; }
    public IReadOnlyList<string> SupportedOutputTypes { get; }
    public IReadOnlyList<string> TransformationCategories { get; }
    public int Priority { get; }
    
    public async Task<IFdwResult<TOutput>> Transform<TOutput>(ITransformationRequest transformationRequest)
    {
        try
        {
            return transformationRequest.TransformationCategory switch
            {
                "Streaming" => await ProcessStream<TOutput>(transformationRequest),
                "RealTime" => await ProcessRealTime<TOutput>(transformationRequest),
                "Windowing" => await ProcessWindowed<TOutput>(transformationRequest),
                "Buffering" => await ProcessBuffered<TOutput>(transformationRequest),
                _ => await ProcessGenericStream<TOutput>(transformationRequest)
            };
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"Stream transformation failed: {ex.Message}", ex);
        }
    }
    
    private async Task<IFdwResult<TOutput>> ProcessStream<TOutput>(ITransformationRequest request)
    {
        if (request.InputData is not IAsyncEnumerable<object> asyncEnumerable)
        {
            // Try to convert to async enumerable
            if (request.InputData is IEnumerable<object> enumerable)
            {
                asyncEnumerable = enumerable.ToAsyncEnumerable();
            }
            else
            {
                return FdwResult<TOutput>.Failure("Stream processing requires IAsyncEnumerable input");
            }
        }
        
        // Get transformation function from configuration
        var transformFunc = GetTransformationFunction(request.Configuration);
        var filterFunc = GetFilterFunction(request.Configuration);
        
        var results = new List<object>();
        
        await foreach (var item in asyncEnumerable)
        {
            if (filterFunc?.Invoke(item) ?? true)
            {
                var transformed = transformFunc?.Invoke(item) ?? item;
                results.Add(transformed);
            }
        }
        
        var convertedResult = ConvertToOutputType<TOutput>(results);
        return FdwResult<TOutput>.Success(convertedResult);
    }
    
    private async Task<IFdwResult<TOutput>> ProcessWindowed<TOutput>(ITransformationRequest request)
    {
        if (request.InputData is not IAsyncEnumerable<object> asyncEnumerable)
            return FdwResult<TOutput>.Failure("Windowed processing requires IAsyncEnumerable input");
        
        // Get window configuration
        var windowSize = request.Configuration.TryGetValue("WindowSize", out var size) 
            ? Convert.ToInt32(size) : 100;
        var windowDuration = request.Configuration.TryGetValue("WindowDuration", out var duration) 
            ? TimeSpan.Parse(duration.ToString()!) : TimeSpan.FromSeconds(5);
        
        var windowResults = new List<List<object>>();
        var currentWindow = new List<object>();
        var windowStartTime = DateTime.UtcNow;
        
        await foreach (var item in asyncEnumerable)
        {
            currentWindow.Add(item);
            
            // Check if window is full or time window has elapsed
            if (currentWindow.Count >= windowSize || DateTime.UtcNow - windowStartTime >= windowDuration)
            {
                windowResults.Add(new List<object>(currentWindow));
                currentWindow.Clear();
                windowStartTime = DateTime.UtcNow;
            }
        }
        
        // Add remaining items
        if (currentWindow.Count > 0)
        {
            windowResults.Add(currentWindow);
        }
        
        var convertedResult = ConvertToOutputType<TOutput>(windowResults);
        return FdwResult<TOutput>.Success(convertedResult);
    }
    
    private Func<object, object>? GetTransformationFunction(IReadOnlyDictionary<string, object> configuration)
    {
        if (configuration.TryGetValue("TransformExpression", out var expr) && expr is string expression)
        {
            // Simplified transformation expression evaluation
            return item => EvaluateTransformExpression(item, expression);
        }
        
        return null;
    }
    
    private Func<object, bool>? GetFilterFunction(IReadOnlyDictionary<string, object> configuration)
    {
        if (configuration.TryGetValue("FilterExpression", out var expr) && expr is string expression)
        {
            return item => EvaluateFilterExpression(item, expression);
        }
        
        return null;
    }
    
    // Additional stream processing methods...
}
```

## Configuration Examples

### JSON Configuration for Multiple Providers

```json
{
  "Transformations": {
    "Providers": {
      "TPL": {
        "MaxDegreeOfParallelism": 8,
        "EnableBatching": true,
        "BatchSize": 1000,
        "DefaultTimeout": "00:10:00",
        "Enabled": true
      },
      "Spark": {
        "MasterUrl": "local[*]",
        "ApplicationName": "FractalDataWorks-Transformations",
        "ExecutorMemory": "2g",
        "DriverMemory": "1g",
        "MaxResultSize": "1g",
        "Enabled": true
      },
      "AzureDataFactory": {
        "SubscriptionId": "your-subscription-id",
        "ResourceGroupName": "your-resource-group",
        "DataFactoryName": "your-data-factory",
        "TenantId": "your-tenant-id",
        "ClientId": "your-client-id",
        "ClientSecret": "your-client-secret",
        "Enabled": true
      },
      "Stream": {
        "DefaultWindowSize": 100,
        "DefaultWindowDuration": "00:00:05",
        "BufferSize": 1000,
        "EnableBackpressure": true,
        "Enabled": true
      }
    },
    "DefaultProvider": "TPL",
    "EnableProviderFallback": true,
    "DefaultTimeout": "00:05:00",
    "EnableMetrics": true
  }
}
```

### Dependency Injection Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure transformation provider settings
    services.Configure<TplTransformationConfiguration>(
        Configuration.GetSection("Transformations:Providers:TPL"));
    services.Configure<SparkTransformationConfiguration>(
        Configuration.GetSection("Transformations:Providers:Spark"));
    services.Configure<AzureDataFactoryConfiguration>(
        Configuration.GetSection("Transformations:Providers:AzureDataFactory"));
    
    // Register transformation providers
    services.AddSingleton<TplTransformationProvider>();
    services.AddSingleton<SparkTransformationProvider>();
    services.AddSingleton<AzureDataFactoryProvider>();
    services.AddSingleton<StreamTransformationProvider>();
    
    // Register transformation services
    services.AddScoped<ITransformationManager, TransformationManager>();
    services.AddSingleton<ITransformationProviderRouter, TransformationProviderRouter>();
    services.AddSingleton<ITransformationPipelineBuilder, TransformationPipelineBuilder>();
    
    // Register background services
    services.AddHostedService<TransformationMetricsCollector>();
}
```

## Advanced Usage

### Transformation Pipeline

```csharp
public sealed class TransformationPipelineBuilder
{
    public IFdwResult<ITransformationPipeline> BuildPipeline(ITransformationPipelineDefinition definition)
    {
        try
        {
            var pipeline = new TransformationPipeline(definition.Name);
            
            foreach (var step in definition.Steps)
            {
                var provider = GetProviderForStep(step);
                if (provider == null)
                    return FdwResult<ITransformationPipeline>.Failure($"No provider found for step: {step.Name}");
                
                pipeline.AddStep(step.Name, provider, step.Request);
            }
            
            return FdwResult<ITransformationPipeline>.Success(pipeline);
        }
        catch (Exception ex)
        {
            return FdwResult<ITransformationPipeline>.Failure("Failed to build transformation pipeline", ex);
        }
    }
}

public sealed class TransformationPipeline : ITransformationPipeline
{
    private readonly List<PipelineStep> _steps = new();
    
    public string Name { get; }
    
    public TransformationPipeline(string name)
    {
        Name = name;
    }
    
    public void AddStep(string stepName, ITransformationProvider provider, ITransformationRequest request)
    {
        _steps.Add(new PipelineStep(stepName, provider, request));
    }
    
    public async Task<IFdwResult<TOutput>> ExecuteAsync<TOutput>(object? initialInput)
    {
        object? currentInput = initialInput;
        
        foreach (var step in _steps)
        {
            var request = step.Request.WithInputData(currentInput);
            var result = await step.Provider.Transform(request);
            
            if (result.IsFailure)
                return FdwResult<TOutput>.Failure($"Pipeline step '{step.Name}' failed: {result.ErrorMessage}", result.Exception);
            
            currentInput = result.Value;
        }
        
        if (currentInput is TOutput finalResult)
            return FdwResult<TOutput>.Success(finalResult);
            
        try
        {
            var converted = (TOutput)Convert.ChangeType(currentInput, typeof(TOutput));
            return FdwResult<TOutput>.Success(converted);
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure("Failed to convert pipeline result to expected type", ex);
        }
    }
}

// Usage example
public async Task<IFdwResult<string>> ProcessDataPipelineAsync(object rawData)
{
    var pipelineDefinition = new TransformationPipelineDefinition
    {
        Name = "Data Processing Pipeline",
        Steps = new[]
        {
            new TransformationStep
            {
                Name = "ParseJson",
                Request = new SimpleTransformationRequest(rawData, "JSON", "Object", "Mapping")
            },
            new TransformationStep
            {
                Name = "FilterData",
                Request = new SimpleTransformationRequest(null, "Object", "Object", "Filtering")
                    .WithConfiguration(new Dictionary<string, object> { ["FilterExpression"] = "status == 'active'" })
            },
            new TransformationStep
            {
                Name = "FormatOutput",
                Request = new SimpleTransformationRequest(null, "Object", "CSV", "Formatting")
                    .WithConfiguration(new Dictionary<string, object> { ["IncludeHeaders"] = true })
            }
        }
    };
    
    var pipelineBuilder = new TransformationPipelineBuilder();
    var pipelineResult = pipelineBuilder.BuildPipeline(pipelineDefinition);
    
    if (pipelineResult.IsFailure)
        return FdwResult<string>.Failure(pipelineResult.ErrorMessage);
    
    var pipeline = pipelineResult.Value;
    var result = await pipeline.ExecuteAsync<string>(rawData);
    
    return result;
}
```

### Real-time Data Stream Processing

```csharp
public sealed class RealTimeDataProcessor
{
    private readonly ITransformationProvider _streamProvider;
    private readonly ILogger<RealTimeDataProcessor> _logger;
    
    public RealTimeDataProcessor(ITransformationProvider streamProvider, ILogger<RealTimeDataProcessor> logger)
    {
        _streamProvider = streamProvider;
        _logger = logger;
    }
    
    public async Task ProcessIncomingDataStreamAsync(IAsyncEnumerable<RawDataEvent> dataStream, 
        Func<ProcessedDataEvent, Task> outputHandler)
    {
        var transformRequest = new StreamTransformationRequest(dataStream)
        {
            TransformationCategory = "RealTime",
            Configuration = new Dictionary<string, object>
            {
                ["TransformExpression"] = "enrichWithTimestamp",
                ["FilterExpression"] = "priority > 5",
                ["WindowSize"] = 50,
                ["WindowDuration"] = TimeSpan.FromSeconds(2)
            },
            Options = new Dictionary<string, object>
            {
                ["EnableBackpressure"] = true,
                ["MaxConcurrency"] = 4
            }
        };
        
        try
        {
            var resultStream = await _streamProvider.Transform<IAsyncEnumerable<ProcessedDataEvent>>(transformRequest);
            
            if (resultStream.IsSuccess)
            {
                await foreach (var processedEvent in resultStream.Value)
                {
                    await outputHandler(processedEvent);
                }
            }
            else
            {
                _logger.LogError("Stream transformation failed: {Error}", resultStream.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Real-time data processing failed");
        }
    }
}
```

### ML Model Integration

```csharp
public sealed class MLTransformationProvider : ITransformationProvider
{
    public async Task<IFdwResult<TOutput>> Transform<TOutput>(ITransformationRequest transformationRequest)
    {
        if (transformationRequest.TransformationCategory != "MachineLearning")
            return FdwResult<TOutput>.Failure("This provider only supports ML transformations");
        
        try
        {
            var modelPath = transformationRequest.Configuration.GetValueOrDefault("ModelPath")?.ToString();
            if (string.IsNullOrWhiteSpace(modelPath))
                return FdwResult<TOutput>.Failure("ML model path is required");
            
            // Load ML model (example using ML.NET)
            var mlContext = new MLContext();
            var model = mlContext.Model.Load(modelPath, out var modelInputSchema);
            
            // Create prediction engine
            var predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            
            // Transform input data to model input format
            var modelInput = ConvertToModelInput(transformationRequest.InputData);
            
            // Make prediction
            var prediction = predictionEngine.Predict(modelInput);
            
            // Convert prediction to output format
            var result = ConvertPredictionToOutput<TOutput>(prediction);
            
            return FdwResult<TOutput>.Success(result);
        }
        catch (Exception ex)
        {
            return FdwResult<TOutput>.Failure($"ML transformation failed: {ex.Message}", ex);
        }
    }
}
```

## Best Practices

1. **Choose appropriate providers** based on data size and complexity
2. **Use streaming transformations** for real-time data processing
3. **Implement proper error handling** and fallback mechanisms
4. **Monitor transformation performance** and optimize bottlenecks
5. **Cache intermediate results** when processing large datasets
6. **Use parallel processing** when data allows for it
7. **Validate input and output data** to ensure data quality
8. **Implement proper resource management** for external engines
9. **Use pipeline composition** for complex multi-step transformations
10. **Test transformations thoroughly** with edge cases and large datasets

## Integration with Other Framework Components

This abstraction layer works seamlessly with other FractalDataWorks packages:

- **DataProviders**: Source and sink data through various data providers
- **ExternalConnections**: Connect to external transformation engines and data sources
- **Authentication**: Secure access to transformation services and data sources
- **SecretManagement**: Manage API keys and credentials for external services
- **Scheduling**: Schedule batch transformations and data pipeline executions

## License

This package is part of the FractalDataWorks Framework and is licensed under the Apache 2.0 License.