using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Smoke tests to verify basic ServiceBase logging functionality.
/// </summary>
public class ServiceBaseSmokeTests
{
    [Fact]
    public void ServiceBaseLogMethodsExist()
    {
        // Arrange
        var logger = NullLogger.Instance;

        // Act & Assert - These calls should compile and not throw
        ServiceBaseLog.ServiceStarted(logger, "TestService");
        ServiceBaseLog.InvalidConfiguration(logger, "Test error");
        ServiceBaseLog.InvalidConfigurationWarning(logger, "Test warning");
        ServiceBaseLog.ExecutingCommand(logger, "TestCommand", "TestService");
        ServiceBaseLog.CommandExecuted(logger, "TestCommand", 100.0);
        ServiceBaseLog.CommandFailed(logger, "TestCommand", "Test error");
        ServiceBaseLog.OperationFailed(logger, "TestOperation", "Test error", null);
        
        // If we get here, all methods exist and can be called
        true.ShouldBeTrue();
    }

    [Fact]
    public void PerformanceMetricsRecordWorks()
    {
        // Arrange & Act
        var metrics = new PerformanceMetrics(150.5, 42, "TestOperation", null);

        // Assert
        metrics.Duration.ShouldBe(150.5);
        metrics.ItemsProcessed.ShouldBe(42);
        metrics.OperationType.ShouldBe("TestOperation");
        metrics.SensitiveData.ShouldBeNull();
        metrics.ToString().ShouldBe("TestOperation: 42 items in 150.5ms");
    }
}