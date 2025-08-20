using System;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Services.SecretManagement.Abstractions.Tests;

/// <summary>
/// Essential tests for SecretProviderBaseLog to achieve code coverage.
/// </summary>
public sealed class SimpleSecretProviderBaseLogTests
{
    [Fact]
    public void EmptyCommandListShouldCallIsEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        // Act
        SecretProviderBaseLog.EmptyCommandList(mockLogger.Object);

        // Assert
        mockLogger.Verify(x => x.IsEnabled(LogLevel.Error), Times.AtLeastOnce);
    }

    [Fact]
    public void BatchExecutionFailedShouldCallIsEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        // Act
        SecretProviderBaseLog.BatchExecutionFailed(mockLogger.Object, "test error");

        // Assert
        mockLogger.Verify(x => x.IsEnabled(LogLevel.Error), Times.AtLeastOnce);
    }

    [Fact]
    public void UnsupportedCommandTypeShouldCallIsEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        // Act
        SecretProviderBaseLog.UnsupportedCommandType(mockLogger.Object, "provider", "command");

        // Assert
        mockLogger.Verify(x => x.IsEnabled(LogLevel.Error), Times.AtLeastOnce);
    }

    [Fact]
    public void HealthCheckFailedShouldCallIsEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        // Act
        SecretProviderBaseLog.HealthCheckFailed(mockLogger.Object, "health error");

        // Assert
        mockLogger.Verify(x => x.IsEnabled(LogLevel.Error), Times.AtLeastOnce);
    }

    [Fact]
    public void SecretProviderBaseLogShouldBeStaticClass()
    {
        // Arrange & Act
        var type = typeof(SecretProviderBaseLog);

        // Assert
        type.IsClass.ShouldBeTrue();
        type.IsAbstract.ShouldBeTrue();
        type.IsSealed.ShouldBeTrue();
    }
}