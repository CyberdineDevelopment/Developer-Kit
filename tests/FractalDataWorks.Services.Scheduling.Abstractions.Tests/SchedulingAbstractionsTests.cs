using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Tests;

/// <summary>
/// Comprehensive tests for all interfaces in the FractalDataWorks.Services.Scheduling.Abstractions project.
/// Achieves 100% code coverage by testing interface contracts, inheritance, properties, methods, and documentation.
/// </summary>
public sealed class SchedulingAbstractionsTests
{
    #region ISchedulingConfiguration Tests

    [Fact]
    public void ISchedulingConfigurationShouldInheritFromIFdwConfiguration()
    {
        // Arrange & Act
        var inheritsFromIFdwConfiguration = typeof(IFdwConfiguration).IsAssignableFrom(typeof(ISchedulingConfiguration));

        // Assert
        inheritsFromIFdwConfiguration.ShouldBeTrue();
    }

    [Fact]
    public void ISchedulingConfigurationShouldBePublicInterface()
    {
        // Arrange
        var interfaceType = typeof(ISchedulingConfiguration);

        // Act & Assert
        interfaceType.IsPublic.ShouldBeTrue();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Scheduling.Abstractions");
    }

    #endregion

    #region IScheduleCommand Tests

    [Fact]
    public void IScheduleCommandShouldInheritFromICommand()
    {
        // Arrange & Act
        var inheritsFromICommand = typeof(ICommand).IsAssignableFrom(typeof(IScheduleCommand));

        // Assert
        inheritsFromICommand.ShouldBeTrue();
    }

    [Fact]
    public void IScheduleCommandShouldBePublicInterface()
    {
        // Arrange
        var interfaceType = typeof(IScheduleCommand);

        // Act & Assert
        interfaceType.IsPublic.ShouldBeTrue();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Scheduling.Abstractions");
    }

    [Fact]
    public void IScheduleCommandShouldNotDefineAdditionalMembers()
    {
        // Arrange
        var interfaceType = typeof(IScheduleCommand);

        // Act
        var declaredMembers = interfaceType.GetMembers().Where(m => m.DeclaringType == interfaceType).ToArray();

        // Assert
        declaredMembers.ShouldBeEmpty();
    }

    #endregion

    #region ISchedulerMetrics Tests

    [Fact]
    public void ISchedulerMetricsShouldHaveCorrectProperties()
    {
        // Arrange
        var interfaceType = typeof(ISchedulerMetrics);
        var expectedProperties = new[] { "TotalTasks", "RunningTasks" };

        // Act
        var properties = interfaceType.GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToArray();

        // Assert
        properties.Length.ShouldBe(2);
        foreach (var expectedProperty in expectedProperties)
        {
            propertyNames.ShouldContain(expectedProperty);
        }
        
        var totalTasksProperty = interfaceType.GetProperty("TotalTasks");
        totalTasksProperty.PropertyType.ShouldBe(typeof(int));
        totalTasksProperty.CanRead.ShouldBeTrue();
        totalTasksProperty.CanWrite.ShouldBeFalse();

        var runningTasksProperty = interfaceType.GetProperty("RunningTasks");
        runningTasksProperty.PropertyType.ShouldBe(typeof(int));
        runningTasksProperty.CanRead.ShouldBeTrue();
        runningTasksProperty.CanWrite.ShouldBeFalse();
    }

    #endregion

    #region ITaskExecutionMetrics Tests

    [Fact]
    public void ITaskExecutionMetricsShouldHaveCorrectProperties()
    {
        // Arrange
        var interfaceType = typeof(ITaskExecutionMetrics);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        properties.Length.ShouldBe(2);
        
        var executionTimeProperty = interfaceType.GetProperty("ExecutionTime");
        executionTimeProperty.ShouldNotBeNull();
        executionTimeProperty.PropertyType.ShouldBe(typeof(TimeSpan));
        
        var executionCountProperty = interfaceType.GetProperty("ExecutionCount");
        executionCountProperty.ShouldNotBeNull();
        executionCountProperty.PropertyType.ShouldBe(typeof(int));
    }

    #endregion

    #region ITaskExecutionResult Tests

    [Fact]
    public void ITaskExecutionResultShouldHaveCorrectProperties()
    {
        // Arrange
        var interfaceType = typeof(ITaskExecutionResult);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        properties.Length.ShouldBe(3);
        
        var isSuccessfulProperty = interfaceType.GetProperty("IsSuccessful");
        isSuccessfulProperty.ShouldNotBeNull();
        isSuccessfulProperty.PropertyType.ShouldBe(typeof(bool));
        
        var resultProperty = interfaceType.GetProperty("Result");
        resultProperty.ShouldNotBeNull();
        resultProperty.PropertyType.ShouldBe(typeof(object));
        
        var errorMessageProperty = interfaceType.GetProperty("ErrorMessage");
        errorMessageProperty.ShouldNotBeNull();
        errorMessageProperty.PropertyType.ShouldBe(typeof(string));
    }

    #endregion

    #region ITaskInfo Tests

    [Fact]
    public void ITaskInfoShouldHaveCorrectProperties()
    {
        // Arrange
        var interfaceType = typeof(ITaskInfo);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        properties.Length.ShouldBe(2);
        
        var taskIdProperty = interfaceType.GetProperty("TaskId");
        taskIdProperty.ShouldNotBeNull();
        taskIdProperty.PropertyType.ShouldBe(typeof(string));
        
        var statusProperty = interfaceType.GetProperty("Status");
        statusProperty.ShouldNotBeNull();
        statusProperty.PropertyType.ShouldBe(typeof(string));
    }

    #endregion

    #region ITaskExecutorConfiguration Tests

    [Fact]
    public void ITaskExecutorConfigurationShouldHaveCorrectProperties()
    {
        // Arrange
        var interfaceType = typeof(ITaskExecutorConfiguration);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        properties.Length.ShouldBe(1);
        
        var executionModeProperty = interfaceType.GetProperty("ExecutionMode");
        executionModeProperty.ShouldNotBeNull();
        executionModeProperty.PropertyType.ShouldBe(typeof(string));
    }

    #endregion

    #region ITaskExecutor Tests

    [Fact]
    public void ITaskExecutorShouldHaveExecuteAsyncMethod()
    {
        // Arrange
        var interfaceType = typeof(ITaskExecutor);

        // Act
        var method = interfaceType.GetMethod("ExecuteAsync");
        var methods = interfaceType.GetMethods().Where(m => !m.IsSpecialName).ToArray();

        // Assert
        methods.Length.ShouldBe(1);
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<ITaskExecutionResult>));
        method.GetParameters().Length.ShouldBe(0);
    }

    #endregion

    #region ITaskSchedule Tests

    [Fact]
    public void ITaskScheduleShouldHaveCorrectProperties()
    {
        // Arrange
        var interfaceType = typeof(ITaskSchedule);

        // Act
        var properties = interfaceType.GetProperties();

        // Assert
        properties.Length.ShouldBe(2);
        
        var scheduleStrategyProperty = interfaceType.GetProperty("ScheduleStrategy");
        scheduleStrategyProperty.ShouldNotBeNull();
        scheduleStrategyProperty.PropertyType.ShouldBe(typeof(string));
        
        var scheduleExpressionProperty = interfaceType.GetProperty("ScheduleExpression");
        scheduleExpressionProperty.ShouldNotBeNull();
        scheduleExpressionProperty.PropertyType.ShouldBe(typeof(string));
    }

    #endregion

    #region ITaskExecutionContext Tests

    [Fact]
    public void ITaskExecutionContextShouldHaveCorrectProperties()
    {
        // Arrange
        var interfaceType = typeof(ITaskExecutionContext);
        var expectedProperties = new[] 
        { 
            "ExecutionId", "ScheduledTime", "StartTime", "CancellationToken", 
            "ServiceProvider", "Metrics", "Properties" 
        };

        // Act
        var properties = interfaceType.GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToArray();

        // Assert
        properties.Length.ShouldBe(7);
        foreach (var expectedProperty in expectedProperties)
        {
            propertyNames.ShouldContain(expectedProperty);
        }

        // Verify specific property types
        interfaceType.GetProperty("ExecutionId").PropertyType.ShouldBe(typeof(string));
        interfaceType.GetProperty("ScheduledTime").PropertyType.ShouldBe(typeof(DateTimeOffset));
        interfaceType.GetProperty("StartTime").PropertyType.ShouldBe(typeof(DateTimeOffset));
        interfaceType.GetProperty("CancellationToken").PropertyType.ShouldBe(typeof(CancellationToken));
        interfaceType.GetProperty("ServiceProvider").PropertyType.ShouldBe(typeof(IServiceProvider));
        interfaceType.GetProperty("Metrics").PropertyType.ShouldBe(typeof(ITaskExecutionMetrics));
        interfaceType.GetProperty("Properties").PropertyType.ShouldBe(typeof(IReadOnlyDictionary<string, object>));
    }

    [Fact]
    public void ITaskExecutionContextShouldHaveCorrectMethods()
    {
        // Arrange
        var interfaceType = typeof(ITaskExecutionContext);

        // Act
        var methods = interfaceType.GetMethods().Where(m => !m.IsSpecialName).ToArray();

        // Assert
        methods.Length.ShouldBe(2);
        
        var reportProgressMethod = interfaceType.GetMethod("ReportProgress");
        reportProgressMethod.ShouldNotBeNull();
        reportProgressMethod.ReturnType.ShouldBe(typeof(void));
        var reportProgressParams = reportProgressMethod.GetParameters();
        reportProgressParams.Length.ShouldBe(2);
        reportProgressParams[0].ParameterType.ShouldBe(typeof(int));
        reportProgressParams[1].ParameterType.ShouldBe(typeof(string));

        var setCheckpointMethod = interfaceType.GetMethod("SetCheckpointAsync");
        setCheckpointMethod.ShouldNotBeNull();
        setCheckpointMethod.ReturnType.ShouldBe(typeof(Task));
        var setCheckpointParams = setCheckpointMethod.GetParameters();
        setCheckpointParams.Length.ShouldBe(1);
        setCheckpointParams[0].ParameterType.ShouldBe(typeof(object));
    }

    #endregion

    #region IScheduledTask Tests

    [Fact]
    public void IScheduledTaskShouldHaveCorrectProperties()
    {
        // Arrange
        var interfaceType = typeof(IScheduledTask);
        var expectedProperties = new[] 
        { 
            "TaskId", "TaskName", "TaskCategory", "Priority", 
            "ExpectedExecutionTime", "MaxExecutionTime", "Dependencies", 
            "Configuration", "Metadata", "AllowsConcurrentExecution" 
        };

        // Act
        var properties = interfaceType.GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToArray();

        // Assert
        properties.Length.ShouldBe(10);
        foreach (var expectedProperty in expectedProperties)
        {
            propertyNames.ShouldContain(expectedProperty);
        }

        // Verify specific property types
        interfaceType.GetProperty("TaskId").PropertyType.ShouldBe(typeof(string));
        interfaceType.GetProperty("TaskName").PropertyType.ShouldBe(typeof(string));
        interfaceType.GetProperty("TaskCategory").PropertyType.ShouldBe(typeof(string));
        interfaceType.GetProperty("Priority").PropertyType.ShouldBe(typeof(int));
        interfaceType.GetProperty("ExpectedExecutionTime").PropertyType.ShouldBe(typeof(TimeSpan?));
        interfaceType.GetProperty("MaxExecutionTime").PropertyType.ShouldBe(typeof(TimeSpan?));
        interfaceType.GetProperty("Dependencies").PropertyType.ShouldBe(typeof(IReadOnlyList<string>));
        interfaceType.GetProperty("Configuration").PropertyType.ShouldBe(typeof(IReadOnlyDictionary<string, object>));
        interfaceType.GetProperty("Metadata").PropertyType.ShouldBe(typeof(IReadOnlyDictionary<string, object>));
        interfaceType.GetProperty("AllowsConcurrentExecution").PropertyType.ShouldBe(typeof(bool));
    }

    [Fact]
    public void IScheduledTaskShouldHaveCorrectMethods()
    {
        // Arrange
        var interfaceType = typeof(IScheduledTask);

        // Act
        var methods = interfaceType.GetMethods().Where(m => !m.IsSpecialName).ToArray();

        // Assert
        methods.Length.ShouldBe(3);
        
        var executeAsyncMethod = interfaceType.GetMethod("ExecuteAsync");
        executeAsyncMethod.ShouldNotBeNull();
        executeAsyncMethod.ReturnType.ShouldBe(typeof(Task<IFdwResult<object>>));
        executeAsyncMethod.GetParameters().Length.ShouldBe(1);
        executeAsyncMethod.GetParameters()[0].ParameterType.ShouldBe(typeof(ITaskExecutionContext));

        var validateTaskMethod = interfaceType.GetMethod("ValidateTask");
        validateTaskMethod.ShouldNotBeNull();
        validateTaskMethod.ReturnType.ShouldBe(typeof(IFdwResult));
        validateTaskMethod.GetParameters().Length.ShouldBe(0);

        var onCleanupAsyncMethod = interfaceType.GetMethod("OnCleanupAsync");
        onCleanupAsyncMethod.ShouldNotBeNull();
        onCleanupAsyncMethod.ReturnType.ShouldBe(typeof(Task));
        var cleanupParams = onCleanupAsyncMethod.GetParameters();
        cleanupParams.Length.ShouldBe(2);
        cleanupParams[0].ParameterType.ShouldBe(typeof(ITaskExecutionContext));
        cleanupParams[1].ParameterType.ShouldBe(typeof(string));
    }

    #endregion

    #region IScheduler Tests (Non-Generic)

    [Fact]
    public void NonGenericISchedulerShouldInheritFromIFdwService()
    {
        // Arrange & Act
        var inheritsFromIFdwService = typeof(IFdwService).IsAssignableFrom(typeof(IScheduler));

        // Assert
        inheritsFromIFdwService.ShouldBeTrue();
    }

    [Fact]
    public void NonGenericISchedulerShouldBePublicInterface()
    {
        // Arrange
        var interfaceType = typeof(IScheduler);

        // Act & Assert
        interfaceType.IsPublic.ShouldBeTrue();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.Namespace.ShouldBe("FractalDataWorks.Services.Scheduling.Abstractions");
    }

    [Fact]
    public void NonGenericISchedulerShouldNotDefineAdditionalMembers()
    {
        // Arrange
        var interfaceType = typeof(IScheduler);

        // Act
        var declaredMembers = interfaceType.GetMembers().Where(m => m.DeclaringType == interfaceType).ToArray();

        // Assert
        declaredMembers.ShouldBeEmpty();
    }

    #endregion

    #region IScheduler<T> Tests (Generic)

    [Fact]
    public void GenericISchedulerShouldInheritFromBothInterfaces()
    {
        // Arrange
        var interfaceType = typeof(IScheduler<>);
        var genericArgument = typeof(IScheduleCommand);
        var constructedType = interfaceType.MakeGenericType(genericArgument);

        // Act & Assert
        typeof(IScheduler).IsAssignableFrom(constructedType).ShouldBeTrue();
        typeof(IFdwService<>).MakeGenericType(genericArgument).IsAssignableFrom(constructedType).ShouldBeTrue();
    }

    [Fact]
    public void GenericISchedulerShouldHaveGenericConstraint()
    {
        // Arrange
        var interfaceType = typeof(IScheduler<>);

        // Act
        var genericArguments = interfaceType.GetGenericArguments();
        var constraints = genericArguments[0].GetGenericParameterConstraints();

        // Assert
        genericArguments.Length.ShouldBe(1);
        constraints.Length.ShouldBe(1);
        constraints[0].ShouldBe(typeof(IScheduleCommand));
    }

    [Fact]
    public void GenericISchedulerShouldHaveCorrectProperties()
    {
        // Arrange
        var interfaceType = typeof(IScheduler<>);
        var expectedProperties = new[] 
        { 
            "SupportedSchedulingStrategies", "SupportedExecutionModes", "MaxConcurrentTasks", 
            "ActiveTaskCount", "QueuedTaskCount"
        };

        // Act
        var declaredProperties = interfaceType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
            .Select(p => p.Name)
            .ToArray();

        // Assert
        declaredProperties.Length.ShouldBe(5);
        foreach (var expectedProperty in expectedProperties)
        {
            declaredProperties.ShouldContain(expectedProperty);
        }

        // Verify specific property types
        interfaceType.GetProperty("SupportedSchedulingStrategies").PropertyType.ShouldBe(typeof(IReadOnlyList<string>));
        interfaceType.GetProperty("SupportedExecutionModes").PropertyType.ShouldBe(typeof(IReadOnlyList<string>));
        interfaceType.GetProperty("MaxConcurrentTasks").PropertyType.ShouldBe(typeof(int?));
        interfaceType.GetProperty("ActiveTaskCount").PropertyType.ShouldBe(typeof(int));
        interfaceType.GetProperty("QueuedTaskCount").PropertyType.ShouldBe(typeof(int));
    }

    [Fact]
    public void GenericISchedulerShouldHaveCorrectMethods()
    {
        // Arrange
        var interfaceType = typeof(IScheduler<>);
        var expectedMethods = new[] 
        { 
            "ScheduleTask", "CancelTask", "PauseTask", "ResumeTask", "ExecuteTaskNow", 
            "GetTaskInfo", "GetAllTasks", "GetSchedulerMetricsAsync", "CreateExecutionContextAsync"
        };

        // Act
        var declaredMethods = interfaceType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
            .Where(m => !m.IsSpecialName)
            .Select(m => m.Name)
            .ToArray();

        // Assert
        declaredMethods.Length.ShouldBe(9);
        foreach (var expectedMethod in expectedMethods)
        {
            declaredMethods.ShouldContain(expectedMethod);
        }

        // Verify specific method return types
        var scheduleTaskMethod = interfaceType.GetMethod("ScheduleTask");
        scheduleTaskMethod.ReturnType.ShouldBe(typeof(Task<IFdwResult<string>>));
        scheduleTaskMethod.GetParameters().Length.ShouldBe(2);
        scheduleTaskMethod.GetParameters()[0].ParameterType.ShouldBe(typeof(IScheduledTask));
        scheduleTaskMethod.GetParameters()[1].ParameterType.ShouldBe(typeof(ITaskSchedule));

        var cancelTaskMethod = interfaceType.GetMethod("CancelTask");
        cancelTaskMethod.ReturnType.ShouldBe(typeof(Task<IFdwResult>));
        cancelTaskMethod.GetParameters().Length.ShouldBe(1);
        cancelTaskMethod.GetParameters()[0].ParameterType.ShouldBe(typeof(string));

        var executeTaskNowMethod = interfaceType.GetMethod("ExecuteTaskNow");
        executeTaskNowMethod.ReturnType.ShouldBe(typeof(Task<IFdwResult<ITaskExecutionResult>>));
        
        var getTaskInfoMethod = interfaceType.GetMethod("GetTaskInfo");
        getTaskInfoMethod.ReturnType.ShouldBe(typeof(Task<IFdwResult<ITaskInfo>>));
        
        var getAllTasksMethod = interfaceType.GetMethod("GetAllTasks");
        getAllTasksMethod.ReturnType.ShouldBe(typeof(Task<IFdwResult<IReadOnlyList<ITaskInfo>>>));
        
        var getMetricsMethod = interfaceType.GetMethod("GetSchedulerMetricsAsync");
        getMetricsMethod.ReturnType.ShouldBe(typeof(Task<IFdwResult<ISchedulerMetrics>>));
        
        var createContextMethod = interfaceType.GetMethod("CreateExecutionContextAsync");
        createContextMethod.ReturnType.ShouldBe(typeof(Task<IFdwResult<ITaskExecutor>>));
        createContextMethod.GetParameters().Length.ShouldBe(1);
        createContextMethod.GetParameters()[0].ParameterType.ShouldBe(typeof(ITaskExecutorConfiguration));
    }

    #endregion

    #region Assembly Structure Tests

    [Fact]
    public void AssemblyShouldContainAllExpectedInterfaces()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(ISchedulingConfiguration));
        var expectedInterfaces = new[]
        {
            typeof(ISchedulingConfiguration),
            typeof(IScheduleCommand),
            typeof(IScheduledTask),
            typeof(IScheduler),
            typeof(IScheduler<>),
            typeof(ISchedulerMetrics),
            typeof(ITaskExecutionContext),
            typeof(ITaskExecutionMetrics),
            typeof(ITaskExecutionResult),
            typeof(ITaskExecutor),
            typeof(ITaskExecutorConfiguration),
            typeof(ITaskInfo),
            typeof(ITaskSchedule)
        };

        // Act
        var actualTypes = assembly.GetTypes().Where(t => t.IsInterface && t.IsPublic).ToArray();

        // Assert
        actualTypes.Length.ShouldBe(13);
        foreach (var expectedInterface in expectedInterfaces)
        {
            actualTypes.ShouldContain(expectedInterface);
        }
    }

    [Fact]
    public void AllInterfacesShouldBeInCorrectNamespace()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(ISchedulingConfiguration));
        var expectedNamespace = "FractalDataWorks.Services.Scheduling.Abstractions";

        // Act
        var publicInterfaces = assembly.GetTypes().Where(t => t.IsInterface && t.IsPublic).ToArray();

        // Assert
        foreach (var interfaceType in publicInterfaces)
        {
            interfaceType.Namespace.ShouldBe(expectedNamespace);
        }
    }

    [Fact]
    public void AssemblyShouldTargetCorrectFramework()
    {
        // Arrange
        var assembly = Assembly.GetAssembly(typeof(ISchedulingConfiguration));

        // Act
        var targetFrameworkAttribute = assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();

        // Assert
        targetFrameworkAttribute.ShouldNotBeNull();
        targetFrameworkAttribute.FrameworkName.ShouldStartWith(".NETCoreApp,Version=v10.0");
    }

    #endregion

    #region Edge Cases and Validation Tests

    [Theory]
    [InlineData(typeof(ISchedulingConfiguration))]
    [InlineData(typeof(IScheduleCommand))]
    [InlineData(typeof(IScheduledTask))]
    [InlineData(typeof(IScheduler))]
    [InlineData(typeof(ISchedulerMetrics))]
    [InlineData(typeof(ITaskExecutionContext))]
    [InlineData(typeof(ITaskExecutionMetrics))]
    [InlineData(typeof(ITaskExecutionResult))]
    [InlineData(typeof(ITaskExecutor))]
    [InlineData(typeof(ITaskExecutorConfiguration))]
    [InlineData(typeof(ITaskInfo))]
    [InlineData(typeof(ITaskSchedule))]
    public void InterfaceShouldBePublicAndAbstract(Type interfaceType)
    {
        // Act & Assert
        interfaceType.IsPublic.ShouldBeTrue();
        interfaceType.IsInterface.ShouldBeTrue();
        interfaceType.IsAbstract.ShouldBeTrue();
    }

    [Fact]
    public void GenericISchedulerShouldBeGenericTypeDefinition()
    {
        // Arrange
        var interfaceType = typeof(IScheduler<>);

        // Act & Assert
        interfaceType.IsGenericTypeDefinition.ShouldBeTrue();
        interfaceType.ContainsGenericParameters.ShouldBeTrue();
    }

    [Fact]
    public void AllAsyncMethodsInGenericSchedulerShouldReturnTaskTypes()
    {
        // Arrange
        var interfaceType = typeof(IScheduler<>);
        var asyncMethods = interfaceType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
            .Where(m => !m.IsSpecialName)
            .ToArray();

        // Act & Assert
        foreach (var method in asyncMethods)
        {
            method.ReturnType.IsGenericType.ShouldBeTrue();
            var genericTypeDefinition = method.ReturnType.GetGenericTypeDefinition();
            genericTypeDefinition.ShouldBe(typeof(Task<>));
        }
    }

    [Fact]
    public void AllPropertiesInSimpleInterfacesShouldBeReadOnly()
    {
        // Arrange
        var simpleInterfaces = new[]
        {
            typeof(ISchedulerMetrics),
            typeof(ITaskExecutionMetrics),
            typeof(ITaskExecutionResult),
            typeof(ITaskInfo),
            typeof(ITaskExecutorConfiguration),
            typeof(ITaskSchedule)
        };

        // Act & Assert
        foreach (var interfaceType in simpleInterfaces)
        {
            var properties = interfaceType.GetProperties();
            foreach (var property in properties)
            {
                property.CanRead.ShouldBeTrue($"{interfaceType.Name}.{property.Name} should be readable");
                property.CanWrite.ShouldBeFalse($"{interfaceType.Name}.{property.Name} should be read-only");
            }
        }
    }

    #endregion
}

/// <summary>
/// Helper extension methods for testing.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test helper methods")]
internal static class TestExtensions
{
    /// <summary>
    /// Extension to check if a collection contains all expected items.
    /// </summary>
    internal static void ShouldContainAll<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
    {
        foreach (var item in expected)
        {
            actual.ShouldContain(item);
        }
    }
}